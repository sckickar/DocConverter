using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WTextBody : WidgetContainer, ITextBody, ICompositeEntity, IEntity, IWidgetContainer, IWidget
{
	protected BodyItemCollection m_bodyItems;

	private WParagraphCollection m_paragraphs;

	private WTableCollection m_tables;

	private FormFieldCollection m_formFields;

	private List<AlternateChunk> m_alternateChunkCollection;

	private byte m_bflag;

	internal int m_bodyItemIndex;

	internal int m_paraItemIndex;

	internal int m_textStartIndex;

	internal int m_startRangeIndex;

	internal int m_endRangeIndex;

	internal int m_textEndIndex = -1;

	internal int m_matchBodyItemIndex = -1;

	internal int m_matchParaItemIndex = -1;

	internal int currSectionIndex;

	internal int currBodyItemIndex;

	internal int currParaItemIndex;

	public override EntityType EntityType => EntityType.TextBody;

	public IWParagraphCollection Paragraphs => m_paragraphs;

	public IWTableCollection Tables => m_tables;

	public FormFieldCollection FormFields
	{
		get
		{
			if (m_formFields == null)
			{
				m_formFields = new FormFieldCollection(this);
			}
			return m_formFields;
		}
	}

	internal List<AlternateChunk> AlternateChunkCollection
	{
		get
		{
			if (m_alternateChunkCollection == null)
			{
				m_alternateChunkCollection = new List<AlternateChunk>();
			}
			return m_alternateChunkCollection;
		}
	}

	internal bool IsPerformingFindAndReplace
	{
		get
		{
			return (m_bflag & 1) != 0;
		}
		set
		{
			m_bflag = (byte)((m_bflag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public IWParagraph LastParagraph
	{
		get
		{
			if (Paragraphs.Count <= 0)
			{
				return null;
			}
			return Paragraphs[Paragraphs.Count - 1];
		}
	}

	internal bool IsFormFieldsCreated => m_formFields != null;

	internal BodyItemCollection Items => m_bodyItems;

	public EntityCollection ChildEntities => m_bodyItems;

	protected override IEntityCollectionBase WidgetCollection => m_bodyItems;

	internal WTextBody(WordDocument doc, Entity owner)
		: base(doc, owner)
	{
		m_bodyItems = new BodyItemCollection(this);
		m_paragraphs = new WParagraphCollection(m_bodyItems);
		m_tables = new WTableCollection(m_bodyItems);
	}

	internal WTextBody(WSection sec)
		: this(sec.Document, sec)
	{
	}

	public IWParagraph AddParagraph()
	{
		WParagraph entity = new WParagraph(base.Document);
		int index = m_bodyItems.Add(entity);
		return m_bodyItems[index] as IWParagraph;
	}

	public IWTable AddTable()
	{
		IWTable iWTable = new WTable(base.Document);
		m_bodyItems.Add(iWTable);
		return iWTable;
	}

	public IBlockContentControl AddBlockContentControl(ContentControlType controlType)
	{
		switch (controlType)
		{
		case ContentControlType.BuildingBlockGallery:
		case ContentControlType.Group:
		case ContentControlType.RepeatingSection:
			throw new NotImplementedException("Creating a content control for the " + controlType.ToString() + "type is not implemented");
		default:
		{
			BlockContentControl blockContentControl = new BlockContentControl(base.Document);
			blockContentControl.ContentControlProperties.Type = controlType;
			int index = m_bodyItems.Add(blockContentControl);
			return m_bodyItems[index] as IBlockContentControl;
		}
		}
	}

	internal IBlockContentControl AddStructureDocumentTag()
	{
		IBlockContentControl blockContentControl = new BlockContentControl(m_doc);
		m_bodyItems.Add(blockContentControl);
		return blockContentControl;
	}

	internal AlternateChunk AddAlternateChunk()
	{
		AlternateChunk alternateChunk = new AlternateChunk(m_doc);
		m_bodyItems.Add(alternateChunk);
		return alternateChunk;
	}

	internal AlternateChunk AddAltChunk(AlternateChunk altChunk)
	{
		m_bodyItems.Add(altChunk);
		return altChunk;
	}

	public void InsertXHTML(string html)
	{
		InsertXHTML(html, Paragraphs.Count);
	}

	public void InsertXHTML(string html, int paragraphIndex)
	{
		Paragraphs.Insert(paragraphIndex, new WParagraph(base.Document));
		InsertXHTML(html, paragraphIndex, 0);
	}

	public void InsertXHTML(string html, int paragraphIndex, int paragraphItemIndex)
	{
		IHtmlConverter instance = HtmlConverterFactory.GetInstance();
		(instance as HTMLConverterImpl).HtmlImportSettings = base.Document.HTMLImportSettings;
		instance.AppendToTextBody(this, html, paragraphIndex, paragraphItemIndex);
	}

	public bool IsValidXHTML(string html, XHTMLValidationType type)
	{
		IHtmlConverter instance = HtmlConverterFactory.GetInstance();
		(instance as HTMLConverterImpl).HtmlImportSettings = base.Document.HTMLImportSettings;
		return instance.IsValid(html, type);
	}

	public bool IsValidXHTML(string html, XHTMLValidationType type, out string exceptionMessage)
	{
		IHtmlConverter instance = HtmlConverterFactory.GetInstance();
		(instance as HTMLConverterImpl).HtmlImportSettings = base.Document.HTMLImportSettings;
		return instance.IsValid(html, type, out exceptionMessage);
	}

	public void EnsureMinimum()
	{
		if (Paragraphs.Count == 0)
		{
			AddParagraph();
		}
	}

	internal TextSelection Find(Regex pattern)
	{
		foreach (TextBodyItem item in Items)
		{
			TextSelection textSelection = item.Find(pattern);
			if (textSelection != null && textSelection.Count > 0)
			{
				return textSelection;
			}
		}
		return null;
	}

	internal TextSelectionList FindAll(Regex pattern, bool isDocumentComparison, bool isFromTextbody)
	{
		TextSelectionList textSelectionList = null;
		foreach (TextBodyItem item in Items)
		{
			if ((item is WTable || item is BlockContentControl) && isDocumentComparison)
			{
				continue;
			}
			TextSelectionList textSelectionList2 = item.FindAll(pattern, isDocumentComparison && !isFromTextbody);
			if (textSelectionList2 != null && textSelectionList2.Count > 0)
			{
				if (textSelectionList == null)
				{
					textSelectionList = new TextSelectionList();
				}
				textSelectionList.AddRange(textSelectionList2);
			}
		}
		return textSelectionList;
	}

	internal int Replace(Regex pattern, string replace)
	{
		int num = 0;
		IsPerformingFindAndReplace = true;
		foreach (TextBodyItem item in Items)
		{
			num += item.Replace(pattern, replace);
			if (base.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		IsPerformingFindAndReplace = false;
		return num;
	}

	internal int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		int num = 0;
		IsPerformingFindAndReplace = true;
		foreach (TextBodyItem item in Items)
		{
			num += item.Replace(pattern, textSelection, saveFormatting);
			if (base.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		IsPerformingFindAndReplace = false;
		return num;
	}

	internal int Replace(Regex pattern, TextBodyPart textPart, bool saveFormatting)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		IsPerformingFindAndReplace = true;
		TextSelectionList textSelectionList = FindAll(pattern, isDocumentComparison: false, isFromTextbody: false);
		int num = 0;
		if (textSelectionList != null)
		{
			foreach (TextSelection item in textSelectionList)
			{
				WCharacterFormat wCharacterFormat = null;
				if (saveFormatting)
				{
					wCharacterFormat = item.StartTextRange.CharacterFormat;
				}
				InlineContentControl inlineContentControl = item.StartTextRange.Owner as InlineContentControl;
				int num2 = item.SplitAndErase();
				if (inlineContentControl != null)
				{
					textPart.PasteAt(inlineContentControl, num2, wCharacterFormat, saveFormatting);
				}
				else
				{
					WParagraph ownerParagraph = item.OwnerParagraph;
					textPart.PasteAt(ownerParagraph.OwnerTextBody, ownerParagraph.GetIndexInOwnerCollection(), num2, wCharacterFormat, saveFormatting);
				}
				num++;
				if (base.Document.ReplaceFirst)
				{
					break;
				}
			}
			IsPerformingFindAndReplace = false;
			return num;
		}
		return 0;
	}

	internal int Replace(Regex pattern, IWordDocument replaceDoc, bool saveFormatting)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		IsPerformingFindAndReplace = true;
		WCharacterFormat wCharacterFormat = null;
		TextSelectionList textSelectionList = FindAll(pattern, isDocumentComparison: false, isFromTextbody: false);
		int num = 0;
		if (textSelectionList != null)
		{
			foreach (TextSelection item in textSelectionList)
			{
				if (saveFormatting)
				{
					wCharacterFormat = GetSrcCharacterFormat(item);
				}
				InlineContentControl inlineContentControl = item.StartTextRange.Owner as InlineContentControl;
				int num2 = item.SplitAndErase();
				WParagraph ownerParagraph = item.OwnerParagraph;
				int indexInOwnerCollection = ownerParagraph.GetIndexInOwnerCollection();
				WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
				for (int num3 = replaceDoc.Sections.Count - 1; num3 >= 0; num3--)
				{
					IWSection iWSection = replaceDoc.Sections[num3];
					TextBodyPart textBodyPart = new TextBodyPart(base.Document);
					textBodyPart.Copy(iWSection.Body, clone: false);
					if (inlineContentControl != null)
					{
						textBodyPart.PasteAt(inlineContentControl, num2, wCharacterFormat, saveFormatting);
					}
					else
					{
						textBodyPart.PasteAt(ownerTextBody, indexInOwnerCollection, num2, wCharacterFormat, saveFormatting);
					}
				}
				num++;
				if (base.Document.ReplaceFirst)
				{
					break;
				}
			}
			IsPerformingFindAndReplace = false;
			return num;
		}
		return 0;
	}

	private WCharacterFormat GetSrcCharacterFormat(TextSelection sel)
	{
		WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
		sel.StartTextRange.CharacterFormat.UpdateSourceFormatting(wCharacterFormat);
		return wCharacterFormat;
	}

	internal WParagraph GetTextBodyFirstPara(bool isAddNewParagraph)
	{
		if (ChildEntities.Count > 0)
		{
			Entity entity = ChildEntities[0];
			if (entity is WParagraph)
			{
				return entity as WParagraph;
			}
			if (entity is WTable)
			{
				WTable wTable = entity as WTable;
				if (wTable.Rows.Count > 0 && wTable.Rows[0].Cells.Count > 0)
				{
					return wTable.Rows[0].Cells[0].GetTextBodyFirstPara(isAddNewParagraph);
				}
			}
			else if (entity is BlockContentControl)
			{
				return (entity as BlockContentControl).TextBody.GetTextBodyFirstPara(isAddNewParagraph);
			}
		}
		if (isAddNewParagraph)
		{
			WParagraph wParagraph = new WParagraph(m_doc);
			ChildEntities.Insert(0, wParagraph);
			return wParagraph;
		}
		return null;
	}

	internal override void AddSelf()
	{
		foreach (TextBodyItem childEntity in ChildEntities)
		{
			childEntity.AddSelf();
		}
	}

	protected override object CloneImpl()
	{
		WTextBody wTextBody = (WTextBody)base.CloneImpl();
		wTextBody.m_bodyItems = new BodyItemCollection(wTextBody);
		ChildEntities.CloneTo(wTextBody.m_bodyItems);
		wTextBody.m_paragraphs = new WParagraphCollection(wTextBody.m_bodyItems);
		wTextBody.m_tables = new WTableCollection(wTextBody.m_bodyItems);
		return wTextBody;
	}

	private Entity GetTextBodyBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection));
		return entity2;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		int i = 0;
		for (int count = ChildEntities.Count; i < count; i++)
		{
			Entity entity = ChildEntities[i];
			Entity textBodyBaseEntity = GetTextBodyBaseEntity(entity);
			if (entity is AlternateChunk && textBodyBaseEntity is WSection)
			{
				(textBodyBaseEntity as WSection).Body.AlternateChunkCollection.Add(entity as AlternateChunk);
			}
			entity.CloneRelationsTo(doc, nextOwner);
		}
	}

	internal override void Close()
	{
		if (m_bodyItems != null && m_bodyItems.InnerList != null && m_bodyItems.Count > 0)
		{
			for (int i = 0; i < m_bodyItems.Count; i++)
			{
				m_bodyItems[i].Close();
			}
			m_bodyItems.Close();
			m_bodyItems = null;
		}
		if (m_paragraphs != null)
		{
			m_paragraphs.Close();
			m_paragraphs = null;
		}
		if (m_tables != null)
		{
			m_tables.Close();
			m_tables = null;
		}
		if (m_formFields != null)
		{
			m_formFields.Close();
			m_formFields = null;
		}
		if (m_alternateChunkCollection != null)
		{
			m_alternateChunkCollection.Clear();
			m_alternateChunkCollection = null;
		}
		base.Close();
	}

	internal bool IsContainBlockContentControl()
	{
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if (ChildEntities[i] is BlockContentControl)
			{
				return true;
			}
		}
		return false;
	}

	internal void MakeChanges(bool acceptChanges)
	{
		TextBodyItem textBodyItem = null;
		for (int i = 0; i < m_bodyItems.Count; i++)
		{
			textBodyItem = m_bodyItems[i];
			if (RemoveChangedItem(textBodyItem, acceptChanges, ref i))
			{
				continue;
			}
			bool num = CheckMoveToNext(textBodyItem, acceptChanges);
			if (textBodyItem is WTable)
			{
				WTable wTable = textBodyItem as WTable;
				if (acceptChanges)
				{
					wTable.m_trackTableGrid = null;
					if (wTable.DocxTableFormat.Format.OldPropertiesHash.Count > 0)
					{
						wTable.DocxTableFormat.Format.OldPropertiesHash.Clear();
					}
				}
				if (!acceptChanges && wTable.m_trackTableGrid != null)
				{
					wTable.ChangeTrackTableGrid();
				}
			}
			if (!acceptChanges)
			{
				RemoveChangedFormat(textBodyItem);
			}
			if (textBodyItem.IsInsertRevision || textBodyItem.IsDeleteRevision || textBodyItem.IsChangedCFormat)
			{
				textBodyItem.AcceptCChanges();
			}
			if (textBodyItem.IsChangedPFormat)
			{
				textBodyItem.AcceptPChanges();
			}
			textBodyItem.MakeChanges(acceptChanges);
			if (num && MoveToNextPara(textBodyItem))
			{
				ChildEntities.RemoveAt(i);
				i--;
			}
		}
	}

	private bool RemoveChangedItem(TextBodyItem item, bool acceptChanges, ref int itemIndex)
	{
		if ((item.IsInsertRevision && !acceptChanges) || (item.IsDeleteRevision && acceptChanges))
		{
			bool flag = true;
			if (item is WTable)
			{
				flag = (item as WTable).RemoveChangedTable(acceptChanges);
			}
			else if (item is WParagraph)
			{
				flag = (item as WParagraph).CheckOnRemove();
			}
			if (flag)
			{
				ChildEntities.RemoveAt(itemIndex);
				itemIndex--;
				return true;
			}
		}
		return false;
	}

	internal bool HasTrackedChanges()
	{
		foreach (TextBodyItem bodyItem in m_bodyItems)
		{
			if (bodyItem.HasTrackedChanges())
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveChangedFormat(TextBodyItem item)
	{
		if (item.IsChangedCFormat)
		{
			item.RemoveCFormatChanges();
		}
		if (item.IsChangedPFormat)
		{
			item.RemovePFormatChanges();
		}
	}

	private bool CheckMoveToNext(TextBodyItem item, bool acceptChanges)
	{
		bool result = false;
		if (item is WParagraph && item.NextSibling is WParagraph && ((item.IsInsertRevision && !acceptChanges) || (item.IsDeleteRevision && acceptChanges)))
		{
			result = true;
		}
		return result;
	}

	private bool MoveToNextPara(TextBodyItem item)
	{
		if (item is WParagraph wParagraph)
		{
			int num = wParagraph.Items.Count - 1;
			if (!(item.NextSibling is WParagraph wParagraph2))
			{
				return false;
			}
			for (int num2 = num; num2 >= 0; num2--)
			{
				wParagraph2.Items.Insert(0, wParagraph.Items[num2]);
			}
		}
		return true;
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("paragraphs", m_bodyItems);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Vertical);
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_bodyItems == null || m_bodyItems.InnerList == null || m_bodyItems.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_bodyItems.Count; i++)
		{
			m_bodyItems[i].InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				break;
			}
		}
	}

	internal bool IsAutoFit(ref bool isNeedToUpdateWidth, IWidget entity)
	{
		if (entity is WTextBox)
		{
			WTextBox wTextBox = entity as WTextBox;
			if (wTextBox.TextBoxFormat.WrappingMode == WrapMode.None || (wTextBox.IsShape && wTextBox.Shape.TextFrame.NoWrap))
			{
				isNeedToUpdateWidth = true;
			}
			if (wTextBox.TextBoxFormat.AutoFit || (wTextBox.IsShape && wTextBox.Shape.TextFrame.ShapeAutoFit))
			{
				return true;
			}
		}
		else if (entity is Shape)
		{
			Shape shape = entity as Shape;
			if (shape.TextFrame.NoWrap && shape.TextBody.ChildEntities.Count != 0)
			{
				isNeedToUpdateWidth = true;
			}
			if (shape.TextFrame.ShapeAutoFit)
			{
				return true;
			}
		}
		else if (entity is ChildShape)
		{
			ChildShape childShape = entity as ChildShape;
			if (childShape.TextFrame.NoWrap && childShape.TextBody.ChildEntities.Count != 0)
			{
				isNeedToUpdateWidth = true;
			}
			if (childShape.TextFrame.ShapeAutoFit)
			{
				return true;
			}
		}
		return false;
	}

	internal void UpdateMatchIndex()
	{
		m_matchBodyItemIndex = m_bodyItemIndex;
		m_matchParaItemIndex = m_paraItemIndex;
	}

	internal override void Compare(WordDocument originalDocument)
	{
		int num = base.Document.currSectionIndex;
		int num2 = base.Document.currBodyItemIndex;
		while (num2 < Items.Count && num == base.Document.currSectionIndex)
		{
			base.Document.currBodyItemIndex = num2;
			TextBodyItem textBodyItem = Items[base.Document.currBodyItemIndex];
			if (originalDocument.m_sectionIndex >= originalDocument.Sections.Count || base.Document.m_sectionIndex >= base.Document.Sections.Count)
			{
				return;
			}
			textBodyItem.Compare(originalDocument);
			num2 = ((base.Document.currBodyItemIndex != num2) ? base.Document.currBodyItemIndex : num2);
			num2++;
		}
		if (num == base.Document.currSectionIndex && base.Document.currBodyItemIndex >= Items.Count - 1)
		{
			base.Document.currBodyItemIndex = 0;
		}
	}

	internal void UpdateIndex(int bodyItemIndex, int paraItemIndex, int startRangeIndex, int endRangeIndex, int textStartIndex, int textEndIndex = -1)
	{
		m_bodyItemIndex = bodyItemIndex;
		m_paraItemIndex = paraItemIndex;
		m_startRangeIndex = startRangeIndex;
		m_endRangeIndex = endRangeIndex;
		m_textStartIndex = textStartIndex;
		m_textEndIndex = textEndIndex;
	}

	internal void Compare(WTextBody originalTextBody)
	{
		if (originalTextBody.Items.Count == 0 && Items.Count == 0)
		{
			return;
		}
		List<BlockContentControl> blockContentControls = new List<BlockContentControl>();
		List<InlineContentControl> inlineContentControls = new List<InlineContentControl>();
		List<Shape> shapes = new List<Shape>();
		List<GroupShape> groupShapes = new List<GroupShape>();
		List<WPicture> pictures = new List<WPicture>();
		List<WTextBox> textBoxes = new List<WTextBox>();
		List<WChart> charts = new List<WChart>();
		List<WField> fields = new List<WField>();
		List<WTable> tables = new List<WTable>();
		List<WMath> maths = new List<WMath>();
		List<WOleObject> oles = new List<WOleObject>();
		List<TableOfContent> tocs = new List<TableOfContent>();
		Comparison comparison = originalTextBody.Document.Comparison;
		BackUpCollections(originalTextBody, blockContentControls, inlineContentControls, shapes, groupShapes, pictures, textBoxes, charts, fields, tables, maths, oles, tocs);
		int num;
		for (num = currBodyItemIndex; num < Items.Count; num++)
		{
			currBodyItemIndex = num;
			TextBodyItem textBodyItem = Items[currBodyItemIndex];
			if (textBodyItem is WParagraph)
			{
				(textBodyItem as WParagraph).Compare(originalTextBody);
			}
			else if (textBodyItem is WTable)
			{
				(textBodyItem as WTable).Compare(originalTextBody);
			}
			else if (textBodyItem is BlockContentControl)
			{
				(textBodyItem as BlockContentControl).Compare(originalTextBody);
			}
			num = ((currBodyItemIndex != num) ? currBodyItemIndex : num);
		}
		EndOfTheTextBody(originalTextBody);
		base.Document.CompareEmptyParagraphs(originalTextBody, this);
		ResetCollections(comparison, blockContentControls, inlineContentControls, shapes, groupShapes, pictures, textBoxes, charts, fields, tables, maths, oles, tocs);
	}

	private void BackUpCollections(WTextBody originalTextBody, List<BlockContentControl> blockContentControls, List<InlineContentControl> inlineContentControls, List<Shape> shapes, List<GroupShape> groupShapes, List<WPicture> pictures, List<WTextBox> textBoxes, List<WChart> charts, List<WField> fields, List<WTable> tables, List<WMath> maths, List<WOleObject> oles, List<TableOfContent> tocs)
	{
		Comparison comparison = originalTextBody.Document.Comparison;
		blockContentControls.AddRange(comparison.BlockContentControls);
		inlineContentControls.AddRange(comparison.InlineContentControls);
		shapes.AddRange(comparison.Shapes);
		groupShapes.AddRange(comparison.GroupShapes);
		pictures.AddRange(comparison.Pictures);
		textBoxes.AddRange(comparison.TextBoxes);
		charts.AddRange(comparison.Charts);
		fields.AddRange(comparison.Fields);
		tables.AddRange(comparison.Tables);
		maths.AddRange(comparison.Maths);
		oles.AddRange(comparison.OLEs);
		oles.AddRange(comparison.OLEs);
		tocs.AddRange(comparison.TOCs);
		ClearCollections(comparison);
		comparison.AddComparisonCollection(originalTextBody);
	}

	private void ClearCollections(Comparison comparison)
	{
		comparison.BlockContentControls.Clear();
		comparison.InlineContentControls.Clear();
		comparison.Shapes.Clear();
		comparison.GroupShapes.Clear();
		comparison.Pictures.Clear();
		comparison.TextBoxes.Clear();
		comparison.Charts.Clear();
		comparison.Fields.Clear();
		comparison.Tables.Clear();
		comparison.Maths.Clear();
		comparison.OLEs.Clear();
		comparison.TOCs.Clear();
	}

	private void ResetCollections(Comparison comparison, List<BlockContentControl> blockContentControls, List<InlineContentControl> inlineContentControls, List<Shape> shapes, List<GroupShape> groupShapes, List<WPicture> pictures, List<WTextBox> textBoxes, List<WChart> charts, List<WField> fields, List<WTable> tables, List<WMath> maths, List<WOleObject> oles, List<TableOfContent> tocs)
	{
		ClearCollections(comparison);
		comparison.BlockContentControls.AddRange(blockContentControls);
		comparison.InlineContentControls.AddRange(inlineContentControls);
		comparison.Shapes.AddRange(shapes);
		comparison.GroupShapes.AddRange(groupShapes);
		comparison.Pictures.AddRange(pictures);
		comparison.TextBoxes.AddRange(textBoxes);
		comparison.Charts.AddRange(charts);
		comparison.Fields.AddRange(fields);
		comparison.Tables.AddRange(tables);
		comparison.Maths.AddRange(maths);
		comparison.OLEs.AddRange(oles);
		comparison.TOCs.AddRange(tocs);
	}

	private void EndOfTheTextBody(WTextBody originalTextBody)
	{
		int num = originalTextBody.Items.Count - 1;
		WParagraph wParagraph = ((num > -1) ? (originalTextBody.Items[num] as WParagraph) : null);
		int num2 = ((wParagraph != null && wParagraph.Items.Count > 0) ? wParagraph.Items.Count : 0);
		int num3 = Items.Count - 1;
		WParagraph wParagraph2 = ((num3 > -1) ? (Items[num3] as WParagraph) : null);
		int num4 = ((wParagraph2 != null && wParagraph2.Items.Count > 0) ? wParagraph2.Items.Count : 0);
		bool flag = false;
		if (originalTextBody.m_bodyItemIndex > num || (originalTextBody.m_bodyItemIndex == num && originalTextBody.m_paraItemIndex >= num2 && originalTextBody.m_paraItemIndex != 0))
		{
			flag = true;
		}
		bool flag2 = false;
		if (m_bodyItemIndex > num3 || (m_bodyItemIndex == num3 && m_paraItemIndex >= num4 && m_paraItemIndex != 0))
		{
			flag2 = true;
		}
		if ((originalTextBody.m_paraItemIndex == 0 && originalTextBody.m_bodyItemIndex > 0 && originalTextBody.m_bodyItemIndex == originalTextBody.Items.Count - 1 && originalTextBody.Items[originalTextBody.m_bodyItemIndex - 1] is WParagraph) || (m_paraItemIndex == 0 && m_bodyItemIndex > 0 && m_bodyItemIndex == Items.Count - 1 && Items[m_bodyItemIndex - 1] is WParagraph))
		{
			originalTextBody.m_bodyItemIndex--;
			originalTextBody.m_paraItemIndex = (originalTextBody.Items[originalTextBody.m_bodyItemIndex] as WParagraph).Items.Count;
			m_bodyItemIndex--;
			m_paraItemIndex = (Items[m_bodyItemIndex] as WParagraph).Items.Count;
		}
		if (!(flag && flag2))
		{
			if (!flag && flag2)
			{
				DeleteItemsToTextBody(originalTextBody);
				return;
			}
			if (flag && !flag2)
			{
				InsertItemsToTextBody(originalTextBody);
				return;
			}
			DeleteItemsToTextBody(originalTextBody);
			InsertItemsToTextBody(originalTextBody);
		}
	}

	private void DeleteItemsToTextBody(WTextBody originalTextBody)
	{
		Comparison comparison = originalTextBody.Document.Comparison;
		int num = originalTextBody.Items.Count - 1;
		WParagraph wParagraph = ((num > -1) ? (originalTextBody.Items[num] as WParagraph) : null);
		int num2 = ((wParagraph != null && wParagraph.Items.Count > 0) ? (wParagraph.Items.Count - 1) : 0);
		if (num != originalTextBody.m_matchBodyItemIndex)
		{
			bool isNeedToInsert = false;
			if (wParagraph != null)
			{
				comparison.ApplyDelRevision(null, null, 0, num, num2, ref isNeedToInsert, isDocumentEnd: false, originalTextBody);
				wParagraph.ApplyDelRevision(wParagraph, 0, num2 + 1);
			}
			else
			{
				comparison.ApplyDelRevision(null, null, 0, num + 1, num2, ref isNeedToInsert, isDocumentEnd: false, originalTextBody);
			}
		}
		else if (num2 != originalTextBody.m_matchParaItemIndex)
		{
			wParagraph.ApplyDelRevision(wParagraph, originalTextBody.m_paraItemIndex, num2 + 1);
		}
		originalTextBody.m_bodyItemIndex = num;
		originalTextBody.m_paraItemIndex = wParagraph?.Items.Count ?? 0;
	}

	private void InsertItemsToTextBody(WTextBody originalTextBody)
	{
		int num = ChildEntities.Count - 1;
		WParagraph wParagraph = ((num > -1) ? (Items[num] as WParagraph) : null);
		int currRevParaItemIndex = ((wParagraph != null && wParagraph.Items.Count > 0) ? wParagraph.Items.Count : 0);
		if (wParagraph == null)
		{
			num++;
		}
		base.Document.Comparison.Insertion(null, currRevParaItemIndex, num, 0, originalTextBody.m_paraItemIndex, originalTextBody.m_bodyItemIndex, 0, originalTextBody, this);
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			IEntity entity = ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				stringBuilder.Append(wParagraph.GetAsString(wParagraph.Items));
				break;
			}
			case EntityType.Table:
				stringBuilder.Append((entity as WTable).GetAsString());
				break;
			case EntityType.BlockContentControl:
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				stringBuilder.Append(blockContentControl.GetAsString());
				break;
			}
			}
		}
		return stringBuilder;
	}
}
