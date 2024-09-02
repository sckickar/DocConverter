using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WSection : WidgetContainer, IWSection, ICompositeEntity, IEntity, IWidgetContainer, IWidget
{
	internal class SectionChildEntities : EntityCollection
	{
		protected override Type[] TypesOfElement
		{
			get
			{
				throw new Exception("Cannot insert an object to SectionChildEntities collection.");
			}
		}

		internal SectionChildEntities()
			: base(null)
		{
		}
	}

	private const float DEF_DISTANCE_BETWEEN_COLUMNS = 36f;

	private WTextBody m_body;

	internal WSectionFormat m_sectionFormat;

	internal WHeadersFooters m_headersFooters;

	private EntityCollection m_childEntities;

	protected internal byte[] m_internalData;

	private short m_previousHeaderCount;

	private short m_previousFooterCount;

	private byte m_bFlags = 1;

	internal short PreviousHeaderCount
	{
		get
		{
			return m_previousHeaderCount;
		}
		set
		{
			m_previousHeaderCount = value;
		}
	}

	internal short PreviousFooterCount
	{
		get
		{
			return m_previousFooterCount;
		}
		set
		{
			m_previousFooterCount = value;
		}
	}

	public WTextBody Body => m_body;

	public WHeadersFooters HeadersFooters => m_headersFooters;

	public WPageSetup PageSetup
	{
		get
		{
			return SectionFormat.PageSetup;
		}
		internal set
		{
			SectionFormat.PageSetup = value;
		}
	}

	internal WSectionFormat SectionFormat => m_sectionFormat;

	public ColumnCollection Columns => SectionFormat.Columns;

	public SectionBreakCode BreakCode
	{
		get
		{
			return SectionFormat.BreakCode;
		}
		set
		{
			SectionFormat.BreakCode = value;
		}
	}

	internal byte[] DataArray
	{
		get
		{
			return m_internalData;
		}
		set
		{
			m_internalData = value;
		}
	}

	public override EntityType EntityType => EntityType.Section;

	public EntityCollection ChildEntities
	{
		get
		{
			if (m_childEntities == null)
			{
				m_childEntities = new SectionChildEntities();
				m_childEntities.AddToInnerList(m_body);
				m_body.SetOwner(this);
				for (int i = 0; i < 6; i++)
				{
					m_childEntities.AddToInnerList(m_headersFooters[i]);
					m_headersFooters[i].SetOwner(this);
				}
			}
			return m_childEntities;
		}
	}

	public IWParagraphCollection Paragraphs => Body.Paragraphs;

	public IWTableCollection Tables => Body.Tables;

	internal DocTextDirection TextDirection
	{
		get
		{
			return SectionFormat.TextDirection;
		}
		set
		{
			SectionFormat.TextDirection = value;
		}
	}

	public bool ProtectForm
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsSectionFitInSamePage
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	protected override IEntityCollectionBase WidgetCollection
	{
		get
		{
			IEntity entity = null;
			AddEmptyParagraph();
			if (m_body.Items.Count > 0 && m_body.Items.LastItem is WParagraph && (m_body.Items.LastItem as WParagraph).SectionEndMark && (entity = (m_body.Items.LastItem as WParagraph).PreviousSibling) is WParagraph && (entity as WParagraph).ChildEntities.Count > 0 && (entity as WParagraph).LastItem is Break && ((entity as WParagraph).LastItem as Break).BreakType == BreakType.PageBreak && !base.Document.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark])
			{
				BodyItemCollection bodyItemCollection = new BodyItemCollection(m_body);
				for (int i = 0; i < m_body.Items.Count; i++)
				{
					bodyItemCollection.AddToInnerList(m_body.Items[i]);
				}
				bodyItemCollection.RemoveFromInnerList(bodyItemCollection.IndexOf(bodyItemCollection.LastItem));
				return bodyItemCollection;
			}
			return m_body.Items;
		}
	}

	public WSection(IWordDocument doc)
		: base((WordDocument)doc, null)
	{
		m_sectionFormat = new WSectionFormat(this);
		m_body = new WTextBody(this);
		SectionFormat.m_columns = new ColumnCollection(this);
		m_headersFooters = new WHeadersFooters(this);
		m_headersFooters.SetOwner(this);
		PageSetup = new WPageSetup(this);
		PageSetup.SetOwner(this);
	}

	public Column AddColumn(float width, float spacing)
	{
		return AddColumn(width, spacing, isOpening: false);
	}

	internal Column AddColumn(float width, float spacing, bool isOpening)
	{
		Column column = new Column(base.Document);
		column.Width = width;
		column.Space = spacing;
		Columns.Add(column, isOpening);
		return column;
	}

	public void MakeColumnsEqual()
	{
		if (Columns.Count <= 0)
		{
			return;
		}
		float width = (PageSetup.PageSize.Width - ((PageSetup.Margins.Left != -0.05f) ? PageSetup.Margins.Left : 0f) - ((PageSetup.Margins.Right != -0.05f) ? PageSetup.Margins.Right : 0f) - (float)(Columns.Count - 1) * 36f) / (float)Columns.Count;
		foreach (Column column in Columns)
		{
			column.Width = width;
			column.Space = 36f;
		}
	}

	public new WSection Clone()
	{
		return (WSection)base.Clone();
	}

	public IWParagraph AddParagraph()
	{
		return Body.AddParagraph();
	}

	public IWTable AddTable()
	{
		return Body.AddTable();
	}

	internal IBlockContentControl AddStructureDocumentTag()
	{
		return Body.AddStructureDocumentTag();
	}

	internal AlternateChunk AddAlternateChunk()
	{
		return Body.AddAlternateChunk();
	}

	internal override void AddSelf()
	{
		Body.AddSelf();
		for (int i = 0; i < 6; i++)
		{
			HeadersFooters[i].AddSelf();
		}
	}

	internal string GetText(WParagraph lastParagraph)
	{
		string text = string.Empty;
		for (int i = 0; i < Body.ChildEntities.Count; i++)
		{
			Entity entity = Body.ChildEntities[i];
			if (entity is WParagraph)
			{
				text += (entity as WParagraph).GetParagraphText(lastParagraph == entity);
			}
			else if (entity is WTable)
			{
				text += (entity as WTable).GetTableText();
			}
			if (base.Document.m_prevClonedEntity != null)
			{
				i = base.Document.m_prevClonedEntity.GetIndexInOwnerCollection();
				base.Document.m_prevClonedEntity = null;
			}
		}
		return text;
	}

	internal void AddEmptyParagraph()
	{
		bool flag = false;
		if (base.NextSibling == null)
		{
			flag = true;
		}
		if (Body.ChildEntities.Count == 0 && !flag)
		{
			AddParagraph();
		}
		if (Body.ChildEntities.LastItem is WTable)
		{
			AddParagraph();
		}
	}

	internal bool LineNumbersEnabled()
	{
		if (PageSetup.LineNumberingMode != LineNumberingMode.None && PageSetup.Margins.Left > 0f)
		{
			return PageSetup.LineNumberingStep > 0;
		}
		return false;
	}

	internal WSection CloneWithoutBodyItems()
	{
		bool isCloning = base.Document.IsCloning;
		base.Document.IsCloning = true;
		WSection wSection = new WSection(base.Document);
		if (m_sectionFormat != null)
		{
			wSection.m_sectionFormat = m_sectionFormat.Clone();
			wSection.m_sectionFormat.SetOwner(wSection);
			wSection.PageSetup = PageSetup.Clone();
			wSection.PageSetup.SetOwner(wSection);
			wSection.SectionFormat.m_columns = new ColumnCollection(wSection);
			m_sectionFormat.m_columns.CloneTo(wSection.SectionFormat.m_columns);
		}
		if (!base.Document.IsComparing)
		{
			wSection.m_headersFooters = m_headersFooters.Clone();
			wSection.m_headersFooters.SetOwner(wSection);
			for (int i = 0; i < 6; i++)
			{
				wSection.m_headersFooters[i].SetOwner(wSection);
			}
		}
		base.Document.IsCloning = isCloning;
		return wSection;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		Body.CloneRelationsTo(doc, nextOwner);
		ImportOptions importOptions = doc.ImportOptions;
		bool importStyles = doc.ImportStyles;
		if ((doc.ImportOptions & ImportOptions.UseDestinationStyles) == 0 && (doc.ImportOptions & ImportOptions.KeepSourceFormatting) == 0)
		{
			doc.ImportOptions = ImportOptions.UseDestinationStyles;
			doc.ImportStyles = false;
		}
		for (int i = 0; i <= 5; i++)
		{
			m_headersFooters[i].CloneRelationsTo(doc, nextOwner);
		}
		if (doc.ImportOptions != importOptions)
		{
			doc.ImportOptions = importOptions;
			doc.ImportStyles = importStyles;
		}
	}

	protected override object CloneImpl()
	{
		bool isCloning = base.Document.IsCloning;
		base.Document.IsCloning = true;
		WSection wSection = (WSection)base.CloneImpl();
		wSection.m_childEntities = null;
		if (m_sectionFormat != null)
		{
			wSection.m_sectionFormat = m_sectionFormat.Clone();
			wSection.m_sectionFormat.SetOwner(wSection);
			wSection.PageSetup = PageSetup.Clone();
			wSection.PageSetup.SetOwner(wSection);
			wSection.SectionFormat.m_columns = new ColumnCollection(wSection);
			m_sectionFormat.m_columns.CloneTo(wSection.SectionFormat.m_columns);
		}
		wSection.m_body = (WTextBody)m_body.Clone();
		wSection.m_body.SetOwner(wSection);
		wSection.m_headersFooters = m_headersFooters.Clone();
		wSection.m_headersFooters.SetOwner(wSection);
		for (int i = 0; i < 6; i++)
		{
			wSection.m_headersFooters[i].SetOwner(wSection);
			if (m_headersFooters[i].Watermark != null && m_headersFooters[i].Watermark.Type != 0)
			{
				wSection.m_headersFooters[i].Watermark = (Watermark)m_headersFooters[i].Watermark.Clone();
				wSection.m_headersFooters[i].Watermark.SetOwner(wSection.m_headersFooters[i]);
			}
		}
		base.Document.IsCloning = isCloning;
		return wSection;
	}

	internal void MakeChanges(bool acceptChanges)
	{
		m_body.MakeChanges(acceptChanges);
		if (m_internalData != null && m_internalData.Length < 300 && m_internalData.Length != 0)
		{
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray(m_internalData, 0);
			SinglePropertyModifierRecord singlePropertyModifierRecord = singlePropertyModifierArray.TryGetSprm(12857);
			if (singlePropertyModifierRecord != null)
			{
				int num = singlePropertyModifierArray.Modifiers.IndexOf(singlePropertyModifierRecord) + 1;
				List<SinglePropertyModifierRecord> list = null;
				if (num < singlePropertyModifierArray.Count)
				{
					list = new List<SinglePropertyModifierRecord>();
					int i = num;
					for (int count = singlePropertyModifierArray.Count; i < count; i++)
					{
						list.Add(singlePropertyModifierArray.GetSprmByIndex(i));
					}
					foreach (SinglePropertyModifierRecord item in list)
					{
						singlePropertyModifierArray.RemoveValue(item.Options);
						singlePropertyModifierArray.Add(item);
					}
				}
				singlePropertyModifierArray.RemoveValue(12857);
			}
			m_internalData = new byte[m_internalData.Length];
			singlePropertyModifierArray.Save(m_internalData, 0);
		}
		for (int j = 0; j < 6; j++)
		{
			HeadersFooters[j].MakeChanges(acceptChanges);
		}
	}

	internal bool HasTrackedChanges()
	{
		if (m_body.HasTrackedChanges())
		{
			return true;
		}
		for (int i = 0; i < 6; i++)
		{
			if (HeadersFooters[i].HasTrackedChanges())
			{
				return true;
			}
		}
		return false;
	}

	internal void CompareHeaderFooter(WSection originalSection)
	{
		HeadersFooters.OddHeader.Compare(originalSection.HeadersFooters.OddHeader);
		HeadersFooters.EvenHeader.Compare(originalSection.HeadersFooters.EvenHeader);
		HeadersFooters.FirstPageHeader.Compare(originalSection.HeadersFooters.FirstPageHeader);
		HeadersFooters.OddFooter.Compare(originalSection.HeadersFooters.OddFooter);
		HeadersFooters.EvenFooter.Compare(originalSection.HeadersFooters.EvenFooter);
		HeadersFooters.FirstPageFooter.Compare(originalSection.HeadersFooters.FirstPageFooter);
	}

	internal bool CompareSectionFormat(WSection section)
	{
		if (section.BreakCode != BreakCode)
		{
			return false;
		}
		if (section.PageSetup.PageSize != PageSetup.PageSize)
		{
			return false;
		}
		if (section.Columns.Count != Columns.Count)
		{
			return false;
		}
		if (section.PageSetup.DifferentFirstPage != PageSetup.DifferentFirstPage)
		{
			return false;
		}
		if (section.PageSetup.DifferentOddAndEvenPages != PageSetup.DifferentOddAndEvenPages)
		{
			return false;
		}
		if (PageSetup.FooterDistance != section.PageSetup.FooterDistance)
		{
			return false;
		}
		if (PageSetup.HeaderDistance != section.PageSetup.HeaderDistance)
		{
			return false;
		}
		if (PageSetup.Margins.Left != section.PageSetup.Margins.Left)
		{
			return false;
		}
		if (PageSetup.Margins.Top != section.PageSetup.Margins.Top)
		{
			return false;
		}
		if (PageSetup.Margins.Bottom != section.PageSetup.Margins.Bottom)
		{
			return false;
		}
		if (PageSetup.Margins.Right != section.PageSetup.Margins.Right)
		{
			return false;
		}
		if (PageSetup.Bidi != section.PageSetup.Bidi)
		{
			return false;
		}
		if (PageSetup.RestartPageNumbering != section.PageSetup.RestartPageNumbering)
		{
			return false;
		}
		if (!PageSetup.Borders.Left.Compare(section.PageSetup.Borders.Left))
		{
			return false;
		}
		if (!PageSetup.Borders.Right.Compare(section.PageSetup.Borders.Right))
		{
			return false;
		}
		if (!PageSetup.Borders.Top.Compare(section.PageSetup.Borders.Top))
		{
			return false;
		}
		if (!PageSetup.Borders.Bottom.Compare(section.PageSetup.Borders.Bottom))
		{
			return false;
		}
		if (!PageSetup.Borders.DiagonalDown.Compare(section.PageSetup.Borders.DiagonalDown))
		{
			return false;
		}
		if (!PageSetup.Borders.DiagonalUp.Compare(section.PageSetup.Borders.DiagonalUp))
		{
			return false;
		}
		if (!PageSetup.Borders.Horizontal.Compare(section.PageSetup.Borders.Horizontal))
		{
			return false;
		}
		if (!PageSetup.Borders.Vertical.Compare(section.PageSetup.Borders.Vertical))
		{
			return false;
		}
		if (PageSetup.LineNumberingDistanceFromText != section.PageSetup.LineNumberingDistanceFromText)
		{
			return false;
		}
		if (PageSetup.LineNumberingMode != section.PageSetup.LineNumberingMode)
		{
			return false;
		}
		if (PageSetup.LineNumberingStartValue != section.PageSetup.LineNumberingStartValue)
		{
			return false;
		}
		if (PageSetup.LineNumberingStep != section.PageSetup.LineNumberingStep)
		{
			return false;
		}
		if (PageSetup.LinePitch != section.PageSetup.LinePitch)
		{
			return false;
		}
		if (PageSetup.DrawLinesBetweenCols != section.PageSetup.DrawLinesBetweenCols)
		{
			return false;
		}
		if (PageSetup.Orientation != section.PageSetup.Orientation)
		{
			return false;
		}
		if (PageSetup.EndnoteNumberFormat != section.PageSetup.EndnoteNumberFormat)
		{
			return false;
		}
		if (PageSetup.FootnoteNumberFormat != section.PageSetup.FootnoteNumberFormat)
		{
			return false;
		}
		if (PageSetup.VerticalAlignment != section.PageSetup.VerticalAlignment)
		{
			return false;
		}
		if (PageSetup.EndnotePosition != section.PageSetup.EndnotePosition)
		{
			return false;
		}
		if (PageSetup.FootnotePosition != section.PageSetup.FootnotePosition)
		{
			return false;
		}
		if (PageSetup.InitialEndnoteNumber != section.PageSetup.InitialEndnoteNumber)
		{
			return false;
		}
		if (PageSetup.InitialFootnoteNumber != section.PageSetup.InitialFootnoteNumber)
		{
			return false;
		}
		if (PageSetup.PageBorderOffsetFrom != section.PageSetup.PageBorderOffsetFrom)
		{
			return false;
		}
		if (PageSetup.PageNumberStyle != section.PageSetup.PageNumberStyle)
		{
			return false;
		}
		if (PageSetup.PageBordersApplyType != section.PageSetup.PageBordersApplyType)
		{
			return false;
		}
		if (PageSetup.PageStartingNumber != section.PageSetup.PageStartingNumber)
		{
			return false;
		}
		if (PageSetup.PitchType != section.PageSetup.PitchType)
		{
			return false;
		}
		if (PageSetup.RestartIndexForEndnote != section.PageSetup.RestartIndexForEndnote)
		{
			return false;
		}
		if (PageSetup.RestartIndexForFootnotes != section.PageSetup.RestartIndexForFootnotes)
		{
			return false;
		}
		if (PageSetup.FirstPageTray != section.PageSetup.FirstPageTray)
		{
			return false;
		}
		if (PageSetup.OtherPagesTray != section.PageSetup.OtherPagesTray)
		{
			return false;
		}
		if (PageSetup.PageNumbers != null && PageSetup.PageNumbers.ChapterPageSeparator != section.PageSetup.PageNumbers.ChapterPageSeparator)
		{
			return false;
		}
		if (PageSetup.PageNumbers != null && PageSetup.PageNumbers.HeadingLevelForChapter != section.PageSetup.PageNumbers.HeadingLevelForChapter)
		{
			return false;
		}
		if (ProtectForm != section.ProtectForm)
		{
			return false;
		}
		return true;
	}

	internal override void Close()
	{
		if (m_body != null)
		{
			m_body.Close();
			m_body = null;
		}
		if (m_headersFooters != null)
		{
			m_headersFooters.Close();
			m_headersFooters = null;
		}
		if (PageSetup != null)
		{
			PageSetup.Close();
			PageSetup = null;
		}
		if (Columns != null)
		{
			Columns.Close();
			SectionFormat.m_columns = null;
		}
		if (m_sectionFormat != null)
		{
			m_sectionFormat.Close();
			m_sectionFormat = null;
		}
		if (m_internalData != null)
		{
			m_internalData = null;
		}
		base.Close();
	}

	internal WParagraph GetFirstParagraph()
	{
		IEntity entity = ((Body.ChildEntities.Count > 0) ? Body.ChildEntities[0] : null);
		if (entity == null)
		{
			return null;
		}
		if (entity is WParagraph)
		{
			return Body.ChildEntities[0] as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			if ((entity as BlockContentControl).ChildEntities.Count <= 0)
			{
				return null;
			}
			return (entity as BlockContentControl).ChildEntities[0] as WParagraph;
		}
		return entity as WParagraph;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("body", m_body);
		base.XDLSHolder.AddElement("page-setup", PageSetup);
		base.XDLSHolder.AddElement("columns", Columns);
		base.XDLSHolder.AddElement("headers-footers", m_headersFooters);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("BreakCode", BreakCode);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("BreakCode"))
		{
			BreakCode = (SectionBreakCode)(object)reader.ReadEnum("BreakCode", typeof(SectionBreakCode));
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (DataArray != null)
		{
			writer.WriteChildBinaryElement("internal-data", DataArray);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "internal-data")
		{
			DataArray = reader.ReadChildBinaryElement();
			result = true;
		}
		return result;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Vertical);
		AddEmptyParagraph();
		if (Body.Items.Count == 0)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		if (m_headersFooters != null)
		{
			m_headersFooters.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				return;
			}
		}
		if (m_body != null)
		{
			m_layoutInfo = null;
			m_body.InitLayoutInfo(entity, ref isLastTOCEntry);
			_ = isLastTOCEntry;
		}
	}
}
