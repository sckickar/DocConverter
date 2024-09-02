using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class Break : ParagraphItem, ILeafWidget, IWidget
{
	internal BreakType m_breakType;

	private WTextRange m_lineBreakText;

	internal HtmlToDocLayoutInfo m_htmlToDocLayoutInfo;

	internal HtmlToDocLayoutInfo HtmlToDocLayoutInfo
	{
		get
		{
			if (m_htmlToDocLayoutInfo == null)
			{
				m_htmlToDocLayoutInfo = new HtmlToDocLayoutInfo();
			}
			return m_htmlToDocLayoutInfo;
		}
	}

	public override EntityType EntityType => EntityType.Break;

	public BreakType BreakType => m_breakType;

	internal WTextRange TextRange
	{
		get
		{
			if (m_lineBreakText == null)
			{
				m_lineBreakText = new WTextRange(base.Document);
				m_lineBreakText.SetOwner(this);
			}
			return m_lineBreakText;
		}
		set
		{
			m_lineBreakText = value;
		}
	}

	internal WCharacterFormat CharacterFormat => TextRange.CharacterFormat;

	internal override int EndPos => base.EndPos + ((m_lineBreakText != null) ? m_lineBreakText.Text.Length : 0);

	public Break(IWordDocument doc)
		: this(doc, BreakType.LineBreak)
	{
	}

	public Break(IWordDocument doc, BreakType breakType)
		: base((WordDocument)doc)
	{
		m_breakType = breakType;
		if (breakType == BreakType.TextWrappingBreak)
		{
			CharacterFormat.BreakClear = BreakClearType.All;
		}
	}

	internal override void Close()
	{
		if (m_lineBreakText != null)
		{
			m_lineBreakText.Close();
			m_lineBreakText = null;
		}
		base.Close();
	}

	protected override object CloneImpl()
	{
		Break @break = (Break)base.CloneImpl();
		if (m_lineBreakText != null)
		{
			@break.m_lineBreakText = m_lineBreakText.Clone() as WTextRange;
			@break.m_lineBreakText.SetOwner(@break);
		}
		return @break;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (m_lineBreakText != null)
		{
			m_lineBreakText.CloneRelationsTo(doc, nextOwner);
		}
	}

	internal override void AttachToParagraph(WParagraph paragraph, int itemPos)
	{
		base.AttachToParagraph(paragraph, itemPos);
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue != null && (m_breakType == BreakType.LineBreak || m_breakType == BreakType.TextWrappingBreak) && m_lineBreakText != null)
		{
			ownerParagraphValue.UpdateText(this, 0, m_lineBreakText.Text, isRemove: true);
		}
	}

	internal override void Detach()
	{
		base.Detach();
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue != null && (m_breakType == BreakType.LineBreak || m_breakType == BreakType.TextWrappingBreak) && m_lineBreakText != null)
		{
			ownerParagraphValue.UpdateText(this, m_lineBreakText.Text.Length, string.Empty, isRemove: true);
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.Break);
		writer.WriteValue("BreakType", BreakType);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_breakType = (BreakType)(object)reader.ReadEnum("BreakType", typeof(BreakType));
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("text-range", TextRange);
	}

	protected override void CreateLayoutInfo()
	{
		switch (m_breakType)
		{
		case BreakType.PageBreak:
			m_layoutInfo = new ParagraphLayoutInfo(ChildrenLayoutDirection.Vertical, isPageBreak: false);
			m_layoutInfo.IsPageBreakItem = true;
			break;
		case BreakType.LineBreak:
		case BreakType.TextWrappingBreak:
			m_layoutInfo = new ParagraphLayoutInfo(ChildrenLayoutDirection.Vertical, isPageBreak: false);
			m_layoutInfo.IsLineBreak = true;
			break;
		case BreakType.ColumnBreak:
			m_layoutInfo = new ParagraphLayoutInfo(ChildrenLayoutDirection.Vertical, isPageBreak: false);
			m_layoutInfo.IsPageBreakItem = true;
			break;
		}
		if (CharacterFormat.Hidden)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (CharacterFormat.IsDeleteRevision && !base.Document.RevisionOptions.ShowDeletedText)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (m_layoutInfo != null && !m_layoutInfo.IsLineBreak && IsPageBreakNeedToBeSkipped())
		{
			if (((base.Owner is WParagraph) ? base.OwnerParagraph.GetOwnerEntity() : null) is WTableCell)
			{
				m_layoutInfo.IsSkip = true;
			}
			else
			{
				m_layoutInfo.IsLineBreak = true;
			}
			m_layoutInfo.IsPageBreakItem = false;
		}
		m_layoutInfo.Font = new SyncFont(CharacterFormat.GetFontToRender(FontScriptType.English));
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	private bool IsPageBreakNeedToBeSkipped()
	{
		Entity entity = this;
		do
		{
			if (entity.Owner == null)
			{
				return false;
			}
			entity = entity.Owner;
		}
		while (!(entity is WTextBox) && !(entity is WFootnote) && !(entity is HeaderFooter) && !(entity is Shape) && !(entity is WTableCell));
		return true;
	}

	internal bool IsCarriageReturn()
	{
		if (TextRange != null)
		{
			return TextRange.Text == ControlChar.CarriegeReturn;
		}
		return false;
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		SizeF result = default(SizeF);
		WParagraph wParagraph = base.OwnerParagraph;
		if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		ParagraphItemCollection paragraphItemCollection = wParagraph.Items;
		if (wParagraph.HasSDTInlineItem)
		{
			paragraphItemCollection = wParagraph.GetParagraphItems();
		}
		if (BreakType == BreakType.LineBreak || BreakType == BreakType.TextWrappingBreak)
		{
			result.Height = dc.MeasureString(" ", ((IWidget)this).LayoutInfo.Font.GetFont(base.Document, FontScriptType.English), null, FontScriptType.English).Height;
		}
		bool flag = false;
		if (BreakType == BreakType.PageBreak)
		{
			flag = BreakType == BreakType.PageBreak && paragraphItemCollection.Count == 1 && wParagraph.PreviousSibling != null && wParagraph.NextSibling != null && !m_doc.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark];
		}
		if (flag && ((wParagraph.NextSibling is WParagraph && !(wParagraph.NextSibling as WParagraph).SectionEndMark) || wParagraph.NextSibling is BlockContentControl || wParagraph.NextSibling is WTable))
		{
			result.Height = base.OwnerParagraph.m_layoutInfo.Size.Height;
		}
		if ((BreakType != BreakType.LineBreak && BreakType != BreakType.TextWrappingBreak && !flag) || BreakType == BreakType.ColumnBreak)
		{
			return SizeF.Empty;
		}
		return result;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0018');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0018');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (BreakType == BreakType.LineBreak || BreakType == BreakType.TextWrappingBreak)
		{
			stringBuilder.Append(2.ToString());
		}
		else
		{
			stringBuilder.Append(((int)BreakType).ToString());
		}
		return stringBuilder;
	}

	internal bool Compare(Break break1)
	{
		if (BreakType == BreakType.LineBreak || BreakType == BreakType.TextWrappingBreak)
		{
			if (break1.BreakType == BreakType.LineBreak || break1.BreakType == BreakType.TextWrappingBreak)
			{
				return true;
			}
			return false;
		}
		if (break1.BreakType == BreakType)
		{
			return true;
		}
		return false;
	}
}
