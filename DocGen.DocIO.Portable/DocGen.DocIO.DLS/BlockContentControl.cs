using System;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class BlockContentControl : TextBodyItem, IWidgetContainer, IWidget, IBlockContentControl, ICompositeEntity, IEntity
{
	private ContentControlProperties m_controlProperties;

	private WCharacterFormat m_breakCharacterFormat;

	private WTextBody m_textBody;

	private WParagraph m_mappedParagraph;

	private string m_comparisonText;

	internal WParagraph MappedParagraph
	{
		get
		{
			return m_mappedParagraph;
		}
		set
		{
			m_mappedParagraph = value;
		}
	}

	public WTextBody TextBody => m_textBody;

	public ContentControlProperties ContentControlProperties => m_controlProperties;

	public EntityCollection ChildEntities => m_textBody.ChildEntities;

	internal Entity LastChildEntity
	{
		get
		{
			if (m_textBody.ChildEntities.Count > 0)
			{
				return m_textBody.ChildEntities[m_textBody.ChildEntities.Count - 1];
			}
			return null;
		}
	}

	public WCharacterFormat BreakCharacterFormat => m_breakCharacterFormat;

	public override EntityType EntityType => EntityType.BlockContentControl;

	internal string ComparisonText
	{
		get
		{
			if (m_comparisonText == null)
			{
				string s = GetAsString().ToString();
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				m_comparisonText = base.Document.Comparison.ConvertBytesAsHash(bytes);
			}
			return m_comparisonText;
		}
	}

	int IWidgetContainer.Count => WidgetCollection.Count;

	IWidget IWidgetContainer.this[int index] => WidgetCollection[index] as IWidget;

	protected IEntityCollectionBase WidgetCollection => TextBody.Items;

	EntityCollection IWidgetContainer.WidgetInnerCollection => WidgetCollection as EntityCollection;

	public BlockContentControl(WordDocument doc)
		: base(doc)
	{
		m_controlProperties = new ContentControlProperties(doc, this);
		m_textBody = new WTextBody(doc, this);
		m_breakCharacterFormat = new WCharacterFormat(doc);
	}

	public BlockContentControl(WordDocument doc, ContentControlType controlType)
		: base(doc)
	{
		m_controlProperties = new ContentControlProperties(doc, this);
		m_controlProperties.Type = controlType;
		m_textBody = new WTextBody(doc, this);
		m_breakCharacterFormat = new WCharacterFormat(doc);
	}

	internal override void AddSelf()
	{
		m_textBody.AddSelf();
	}

	internal new BlockContentControl Clone()
	{
		return (BlockContentControl)CloneImpl();
	}

	protected override object CloneImpl()
	{
		BlockContentControl blockContentControl = (BlockContentControl)base.CloneImpl();
		blockContentControl.m_controlProperties = m_controlProperties.Clone();
		blockContentControl.m_controlProperties.SetOwnerContentControl(blockContentControl);
		blockContentControl.m_textBody = (WTextBody)m_textBody.Clone();
		blockContentControl.m_textBody.SetOwner(blockContentControl);
		blockContentControl.m_breakCharacterFormat = new WCharacterFormat(base.Document);
		blockContentControl.m_breakCharacterFormat.ImportContainer(BreakCharacterFormat);
		blockContentControl.m_breakCharacterFormat.CopyProperties(BreakCharacterFormat);
		return blockContentControl;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		m_textBody.CloneRelationsTo(doc, nextOwner);
		ContentControlProperties.CloneRelationsTo(doc, nextOwner);
		ContentControlProperties.SetOwner(this);
		if (m_breakCharacterFormat != null)
		{
			m_breakCharacterFormat.CloneRelationsTo(doc, nextOwner);
		}
	}

	internal override TextBodyItem GetNextTextBodyItemValue()
	{
		if (base.NextSibling != null)
		{
			return base.NextSibling as TextBodyItem;
		}
		if (base.Owner is WTableCell)
		{
			return (base.Owner as WTableCell).GetNextTextBodyItem();
		}
		if (base.Owner is WTextBody)
		{
			if (base.OwnerTextBody.Owner is WTextBox)
			{
				return (base.OwnerTextBody.Owner as WTextBox).GetNextTextBodyItem();
			}
			if (base.OwnerTextBody.Owner is WSection)
			{
				return GetNextInSection(base.OwnerTextBody.Owner as WSection);
			}
		}
		return null;
	}

	internal bool IsHiddenParagraphMarkIsInLastItemOfSDTContent()
	{
		BodyItemCollection items = TextBody.Items;
		if (items != null && items.Count > 0 && items[items.Count - 1] is WParagraph && (items[items.Count - 1] as WParagraph).BreakCharacterFormat != null && (items[items.Count - 1] as WParagraph).BreakCharacterFormat.Hidden)
		{
			return true;
		}
		return false;
	}

	internal bool IsDeletionParagraphMarkIsInLastItemOfSDTContent()
	{
		BodyItemCollection items = TextBody.Items;
		if (items != null && items.Count > 0 && items[items.Count - 1] is WParagraph && (items[items.Count - 1] as WParagraph).BreakCharacterFormat != null && (items[items.Count - 1] as WParagraph).BreakCharacterFormat.IsDeleteRevision)
		{
			return true;
		}
		return false;
	}

	internal bool ContainsParagraph()
	{
		BodyItemCollection items = TextBody.Items;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is WParagraph)
			{
				return true;
			}
			if (items[i] is BlockContentControl)
			{
				return (items[i] as BlockContentControl).ContainsParagraph();
			}
		}
		return false;
	}

	internal WParagraph GetFirstParagraphOfSDTContent()
	{
		BodyItemCollection items = TextBody.Items;
		if (items.Count > 0 && items[0] is WParagraph)
		{
			return (WParagraph)items[0];
		}
		return null;
	}

	internal WParagraph GetLastParagraphOfSDTContent()
	{
		BodyItemCollection items = TextBody.Items;
		if (items.Count > 0 && items[items.Count - 1] is WParagraph)
		{
			return items[items.Count - 1] as WParagraph;
		}
		return null;
	}

	internal override bool CheckDeleteRev()
	{
		if (m_breakCharacterFormat != null)
		{
			return m_breakCharacterFormat.IsDeleteRevision;
		}
		return false;
	}

	internal override void SetChangedPFormat(bool check)
	{
	}

	internal override void SetChangedCFormat(bool check)
	{
	}

	internal override void SetDeleteRev(bool check)
	{
		if (m_breakCharacterFormat != null)
		{
			m_breakCharacterFormat.IsDeleteRevision = check;
		}
	}

	internal override void SetInsertRev(bool check)
	{
		if (m_breakCharacterFormat != null)
		{
			m_breakCharacterFormat.IsInsertRevision = check;
		}
	}

	internal override bool HasTrackedChanges()
	{
		return false;
	}

	public override int Replace(Regex pattern, string replace)
	{
		int num = 0;
		foreach (TextBodyItem item in TextBody.Items)
		{
			num += item.Replace(pattern, replace);
			if (base.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		return num;
	}

	internal int Replace(Regex pattern, TextBodyPart textPart, bool saveFormatting)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		TextSelectionList textSelectionList = FindAll(pattern, isDocumentComparison: false);
		if (textSelectionList != null)
		{
			foreach (TextSelection item in textSelectionList)
			{
				WCharacterFormat srcFormat = null;
				if (saveFormatting)
				{
					srcFormat = item.StartTextRange.CharacterFormat;
				}
				int pItemIndex = item.SplitAndErase();
				WParagraph ownerParagraph = item.OwnerParagraph;
				textPart.PasteAt(ownerParagraph.OwnerTextBody, ownerParagraph.GetIndexInOwnerCollection(), pItemIndex, srcFormat, saveFormatting);
				if (base.Document.ReplaceFirst)
				{
					break;
				}
			}
			return textSelectionList.Count;
		}
		return 0;
	}

	public override int Replace(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, replace);
	}

	public override int Replace(Regex pattern, TextSelection textSelection)
	{
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public override int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		int num = 0;
		foreach (TextBodyItem item in TextBody.Items)
		{
			num += item.Replace(pattern, textSelection, saveFormatting);
			if (base.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		return num;
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, textSelection, saveFormatting);
	}

	internal int ReplaceFirst(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return ReplaceFirst(pattern, replace);
	}

	internal int ReplaceFirst(Regex pattern, string replace)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		TextReplacer instance = TextReplacer.Instance;
		bool replaceFirst = base.Document.ReplaceFirst;
		base.Document.ReplaceFirst = true;
		int num = 0;
		for (int i = 0; i < TextBody.Items.Count; i++)
		{
			if (TextBody.Items[i] is WParagraph)
			{
				num += instance.Replace((WParagraph)TextBody.Items[i], pattern, replace);
			}
			else if (TextBody.Items[i] is BlockContentControl)
			{
				num += ReplaceFirst(pattern, replace);
			}
		}
		base.Document.ReplaceFirst = replaceFirst;
		return num;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Vertical);
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBody != null)
		{
			m_textBody.InitLayoutInfo(entity, ref isLastTOCEntry);
			_ = isLastTOCEntry;
		}
	}

	internal override void RemoveCFormatChanges()
	{
	}

	internal override void RemovePFormatChanges()
	{
	}

	internal override void AcceptCChanges()
	{
	}

	internal override void AcceptPChanges()
	{
	}

	internal override bool CheckChangedCFormat()
	{
		return false;
	}

	internal override bool CheckInsertRev()
	{
		return false;
	}

	public override TextSelection Find(Regex pattern)
	{
		foreach (TextBodyItem item in TextBody.Items)
		{
			TextSelection textSelection = item.Find(pattern);
			if (textSelection != null && textSelection.Count > 0)
			{
				return textSelection;
			}
		}
		return null;
	}

	public TextSelection Find(string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Find(pattern);
	}

	internal override void MakeChanges(bool acceptChanges)
	{
	}

	internal override TextSelectionList FindAll(Regex pattern, bool isDocumentComparison)
	{
		TextSelectionList textSelectionList = null;
		if (isDocumentComparison)
		{
			return textSelectionList;
		}
		foreach (TextBodyItem item in TextBody.Items)
		{
			TextSelectionList textSelectionList2 = item.FindAll(pattern, isDocumentComparison: false);
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

	private WCharacterFormat GetSrcCharacterFormat(TextSelection sel)
	{
		WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
		sel.StartTextRange.CharacterFormat.UpdateSourceFormatting(wCharacterFormat);
		return wCharacterFormat;
	}

	internal int Replace(Regex pattern, IWordDocument replaceDoc, bool saveFormatting)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		WCharacterFormat srcFormat = null;
		TextSelectionList textSelectionList = FindAll(pattern, isDocumentComparison: false);
		if (textSelectionList != null)
		{
			foreach (TextSelection item in textSelectionList)
			{
				if (saveFormatting)
				{
					srcFormat = GetSrcCharacterFormat(item);
				}
				int pItemIndex = item.SplitAndErase();
				WParagraph ownerParagraph = item.OwnerParagraph;
				for (int num = replaceDoc.Sections.Count - 1; num >= 0; num--)
				{
					IWSection iWSection = replaceDoc.Sections[num];
					TextBodyPart textBodyPart = new TextBodyPart(base.Document);
					textBodyPart.Copy(iWSection.Body, clone: false);
					textBodyPart.PasteAt(ownerParagraph.OwnerTextBody, ownerParagraph.GetIndexInOwnerCollection(), pItemIndex, srcFormat, saveFormatting);
				}
				if (base.Document.ReplaceFirst)
				{
					break;
				}
			}
			return textSelectionList.Count;
		}
		return 0;
	}

	internal override void Close()
	{
		if (m_textBody != null)
		{
			m_textBody.Close();
			m_textBody = null;
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Close();
			m_controlProperties = null;
		}
		if (m_breakCharacterFormat != null)
		{
			m_breakCharacterFormat.Close();
			m_breakCharacterFormat = null;
		}
		if (m_mappedParagraph != null)
		{
			m_mappedParagraph.Close();
			m_mappedParagraph = null;
		}
		base.Close();
	}

	internal override bool CheckChangedPFormat()
	{
		return false;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0011');
		stringBuilder.Append(ContentControlProperties.GetAsString());
		stringBuilder.Append(TextBody.GetAsString());
		stringBuilder.Append('\u0011');
		return stringBuilder;
	}

	internal override void Compare(WordDocument orgDoc)
	{
		CompareBlockContentControl(orgDoc);
	}

	internal void Compare(WTextBody orgTextBody)
	{
		CompareBlockContentControl(null, orgTextBody);
	}

	private void CompareBlockContentControl(WordDocument orgDoc, WTextBody orgTextBody = null)
	{
		Comparison comparison = ((orgTextBody != null) ? orgTextBody.Document.Comparison : orgDoc.Comparison);
		_ = Index;
		int num = 0;
		while (num < comparison.BlockContentControls.Count)
		{
			BlockContentControl blockContentControl = comparison.BlockContentControls[num];
			if (blockContentControl != null && blockContentControl.ContentControlProperties.Compare(ContentControlProperties))
			{
				if (blockContentControl.ContentControlProperties.Type == ContentControlType.RichText || blockContentControl.ContentControlProperties.Type == ContentControlType.BuildingBlockGallery)
				{
					TextBody.Compare(blockContentControl.TextBody);
				}
				else if (ComparisonText != blockContentControl.ComparisonText)
				{
					num++;
					continue;
				}
				Document.Comparison.InsertAndDeleteUnmatchedItems(orgDoc, blockContentControl, this, orgTextBody);
				Document.Comparison.MoveCurrPosition(orgDoc, blockContentControl, this, orgTextBody);
				comparison.RemoveFromDocCollection(blockContentControl);
				break;
			}
			num++;
		}
	}

	internal override void AddDelMark()
	{
		WTextBody textBody = TextBody;
		base.Document.Comparison.ApplyDelRev(textBody, 0, textBody.Items.Count);
		base.Document.Comparison.RemoveFromDocCollection(this);
	}

	internal override void AddInsMark()
	{
		WTextBody textBody = TextBody;
		base.Document.Comparison.ApplyInsRev(textBody, 0, textBody.Items.Count);
	}
}
