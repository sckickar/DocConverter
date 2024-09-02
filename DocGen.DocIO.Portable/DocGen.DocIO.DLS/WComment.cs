using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WComment : ParagraphItem, ICompositeEntity, IEntity
{
	protected WTextBody m_textBody;

	protected WCommentFormat m_format;

	private ParagraphItemCollection m_commItems;

	private TextBodyPart m_bodyPart;

	private bool m_appendItems;

	private WCommentMark m_commentRangeStart;

	private WCommentMark m_commentRangeEnd;

	private string m_parentParaId = string.Empty;

	private byte m_bFlags;

	public WCommentMark CommentRangeStart
	{
		get
		{
			return m_commentRangeStart;
		}
		internal set
		{
			m_commentRangeStart = value;
		}
	}

	public WCommentMark CommentRangeEnd
	{
		get
		{
			return m_commentRangeEnd;
		}
		internal set
		{
			m_commentRangeEnd = value;
		}
	}

	public EntityCollection ChildEntities => m_textBody.ChildEntities;

	public override EntityType EntityType => EntityType.Comment;

	public WTextBody TextBody => m_textBody;

	public WCommentFormat Format => m_format;

	public ParagraphItemCollection CommentedItems
	{
		get
		{
			if (m_commItems == null)
			{
				m_commItems = new ParagraphItemCollection(m_doc);
			}
			return m_commItems;
		}
	}

	internal bool AppendItems => m_appendItems;

	internal TextBodyPart CommentedBodyPart => m_bodyPart;

	internal string ParentParaId
	{
		get
		{
			return m_parentParaId;
		}
		set
		{
			m_parentParaId = value;
		}
	}

	public WComment Ancestor => GetAncestorComment();

	internal bool IsDetached
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

	public bool Done
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		internal set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public WComment(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_format = new WCommentFormat();
		m_format.SetOwner(this);
		m_textBody = new WTextBody(base.Document, this);
	}

	protected override object CloneImpl()
	{
		WComment wComment = (WComment)base.CloneImpl();
		wComment.m_format = Format.Clone(base.Document);
		wComment.m_format.SetOwner(wComment);
		wComment.m_textBody = (WTextBody)TextBody.Clone();
		wComment.m_textBody.SetOwner(wComment);
		wComment.m_commItems = null;
		m_bodyPart = null;
		wComment.IsDetached = true;
		return wComment;
	}

	public void RemoveCommentedItems()
	{
		if (m_commItems != null && m_commItems.Count != 0)
		{
			if (m_appendItems)
			{
				m_commItems.Clear();
				m_bodyPart = null;
				return;
			}
			ParagraphItem firstItem = m_commItems.FirstItem as ParagraphItem;
			ParagraphItem lastItem = m_commItems.LastItem as ParagraphItem;
			RemoveItemsBetween(firstItem, lastItem);
			Format.BookmarkStartOffset = 0;
			Format.BookmarkEndOffset = 1;
			m_commItems.Clear();
			m_appendItems = false;
		}
	}

	internal void RemoveItemsBetween(ParagraphItem firstItem, ParagraphItem lastItem)
	{
		if (firstItem.PreviousSibling != null && firstItem.PreviousSibling is WCommentMark)
		{
			firstItem.OwnerParagraph.Items.Remove(firstItem.PreviousSibling);
		}
		if (lastItem.NextSibling != null && lastItem.NextSibling is WCommentMark)
		{
			lastItem.OwnerParagraph.Items.Remove(lastItem.NextSibling);
		}
		if (firstItem != lastItem)
		{
			if (firstItem.OwnerParagraph != lastItem.OwnerParagraph)
			{
				while (firstItem.OwnerParagraph.NextTextBodyItem != lastItem.OwnerParagraph && firstItem.OwnerParagraph.NextTextBodyItem != null && CheckTextBody(firstItem.OwnerParagraph.NextTextBodyItem))
				{
					firstItem.OwnerParagraph.NextTextBodyItem.RemoveSelf();
				}
			}
			while (firstItem.NextSibling != null && firstItem.NextSibling != lastItem && !(firstItem.NextSibling is WComment))
			{
				firstItem.OwnerParagraph.Items.Remove(firstItem.NextSibling);
			}
			while (lastItem.PreviousSibling != null && lastItem.PreviousSibling != firstItem && !(firstItem.NextSibling is WComment))
			{
				lastItem.OwnerParagraph.Items.Remove(lastItem.PreviousSibling);
			}
			RemoveFirstItem(firstItem, lastItem);
		}
		lastItem.RemoveSelf();
	}

	public void ReplaceCommentedItems(string text)
	{
		string text2 = ModifyText(text);
		WTextRange wTextRange = new WTextRange(m_doc)
		{
			Text = text
		};
		if (Format.TagBkmk == "")
		{
			Format.UpdateTagBkmk();
		}
		if (text2.IndexOf(ControlChar.CarriegeReturn) != -1)
		{
			RemoveCommentedItems();
			m_appendItems = false;
			string tagBkmk = Format.TagBkmk;
			int indexInOwnerCollection = GetIndexInOwnerCollection();
			WCommentMark entity = new WCommentMark(base.Document, tagBkmk, CommentMarkType.CommentStart);
			WCommentMark entity2 = new WCommentMark(base.Document, tagBkmk, CommentMarkType.CommentEnd);
			base.OwnerParagraph.Items.Insert(indexInOwnerCollection, entity2);
			base.OwnerParagraph.Items.Insert(indexInOwnerCollection, wTextRange);
			base.OwnerParagraph.Items.Insert(indexInOwnerCollection, entity);
		}
		else
		{
			RemoveCommentedItems();
			m_appendItems = true;
			CommentedItems.InnerList.Add(wTextRange);
		}
	}

	public void ReplaceCommentedItems(TextBodyPart textBodyPart)
	{
		RemoveCommentedItems();
		m_appendItems = true;
		m_bodyPart = textBodyPart;
		FillCommItems();
	}

	internal override void AddSelf()
	{
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		base.Document.Comments.Add(this);
		if (m_textBody != null)
		{
			m_textBody.AttachToDocument();
		}
	}

	internal override void AttachToDocument()
	{
		if (IsDetached)
		{
			base.Document.Comments.Add(this);
			if (m_textBody != null)
			{
				m_textBody.AttachToDocument();
			}
		}
	}

	internal override void Close()
	{
		if (m_textBody != null)
		{
			m_textBody.Close();
			m_textBody = null;
		}
		m_format = null;
		m_bodyPart = null;
		if (m_commItems != null)
		{
			m_commItems.Clear();
			m_commItems = null;
		}
		base.Close();
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (m_textBody != null)
		{
			m_textBody.CloneRelationsTo(doc, nextOwner);
		}
	}

	public void AddCommentedItem(IParagraphItem paraItem)
	{
		if (!(base.Owner is WParagraph) || (m_commItems != null && m_commItems.Contains(paraItem)))
		{
			return;
		}
		WParagraph ownerParagraph = base.OwnerParagraph;
		int indexInOwnerCollection = GetIndexInOwnerCollection();
		if (m_format.TagBkmk == "")
		{
			string text = Convert.ToString(TagIdRandomizer.Instance.Next());
			m_format.TagBkmk = text;
			WCommentMark wCommentMark = new WCommentMark(m_doc, text);
			wCommentMark.Type = CommentMarkType.CommentStart;
			WCommentMark wCommentMark2 = new WCommentMark(m_doc, text);
			wCommentMark2.Type = CommentMarkType.CommentEnd;
			ownerParagraph.Items.Insert(indexInOwnerCollection, wCommentMark2);
			ownerParagraph.Items.Insert(indexInOwnerCollection, wCommentMark);
		}
		indexInOwnerCollection = GetIndexInOwnerCollection();
		if (!(ownerParagraph.Items[indexInOwnerCollection - 1] is WCommentMark))
		{
			return;
		}
		string tagBkmk = m_format.TagBkmk;
		if (!(paraItem.Owner is WParagraph))
		{
			InsertCommItem(ownerParagraph, indexInOwnerCollection - 1, paraItem);
			return;
		}
		if (ownerParagraph.Items.Count > indexInOwnerCollection + 1 && paraItem == ownerParagraph.Items[indexInOwnerCollection + 1])
		{
			ownerParagraph.Items.RemoveAt(indexInOwnerCollection + 1);
			InsertCommItem(ownerParagraph, indexInOwnerCollection - 1, paraItem);
			return;
		}
		WCommentMark wCommentMark3 = FindCommentStart(indexInOwnerCollection, tagBkmk, ownerParagraph.Items);
		if (wCommentMark3 != null && paraItem == ownerParagraph.Items[wCommentMark3.GetIndexInOwnerCollection() - 1])
		{
			int indexInOwnerCollection2 = wCommentMark3.GetIndexInOwnerCollection();
			ownerParagraph.Items.RemoveAt(indexInOwnerCollection2 - 1);
			InsertCommItem(ownerParagraph, indexInOwnerCollection2, paraItem);
		}
		else
		{
			ParagraphItem item = paraItem.Clone() as ParagraphItem;
			InsertCommItem(ownerParagraph, indexInOwnerCollection - 1, item);
		}
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("body", m_textBody);
		base.XDLSHolder.AddElement("comment-format", m_format);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.Comment);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBody != null)
		{
			m_textBody.InitLayoutInfo(entity, ref isLastTOCEntry);
		}
		if (m_commItems != null && m_commItems.Count > 0)
		{
			foreach (Entity commItem in m_commItems)
			{
				commItem.InitLayoutInfo(entity, ref isLastTOCEntry);
				if (isLastTOCEntry)
				{
					return;
				}
			}
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	private void InsertCommItem(WParagraph para, int index, IParagraphItem item)
	{
		para.Items.Insert(index, item);
		(item as ParagraphItem).SetOwner(para);
		CommentedItems.InnerList.Add(item);
	}

	private WCommentMark FindCommentStart(int index, string startId, ParagraphItemCollection itemCollection)
	{
		ParagraphItem paragraphItem = null;
		WCommentMark result = null;
		for (int num = index; num > 0; num--)
		{
			paragraphItem = itemCollection[num];
			if (paragraphItem is WCommentMark)
			{
				WCommentMark wCommentMark = paragraphItem as WCommentMark;
				if (wCommentMark.Type == CommentMarkType.CommentStart && wCommentMark.CommentId == startId)
				{
					result = wCommentMark;
					break;
				}
			}
		}
		return result;
	}

	private bool CheckTextBody(TextBodyItem item)
	{
		if (item is WParagraph)
		{
			return CheckPara(item as WParagraph);
		}
		return CheckTable(item as WTable);
	}

	private bool CheckPara(WParagraph para)
	{
		foreach (ParagraphItem item in para.Items)
		{
			if (item is WComment)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckTable(WTable table)
	{
		bool flag = true;
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (IEntity childEntity in cell.ChildEntities)
				{
					if (!((!(childEntity is WParagraph)) ? CheckTable(childEntity as WTable) : CheckPara(childEntity as WParagraph)))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void RemoveFirstItem(ParagraphItem firstItem, ParagraphItem lastItem)
	{
		WParagraph ownerParagraph = firstItem.OwnerParagraph;
		if (ownerParagraph.GetIndexInOwnerCollection() > 0)
		{
			firstItem.RemoveSelf();
			return;
		}
		WTable wTable = null;
		if (ownerParagraph.IsInCell)
		{
			wTable = (ownerParagraph.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable;
		}
		if (wTable == null)
		{
			if (ownerParagraph.Items.Count > 1)
			{
				firstItem.RemoveSelf();
			}
			else
			{
				ownerParagraph.RemoveSelf();
			}
			return;
		}
		WParagraph ownerParagraph2 = lastItem.OwnerParagraph;
		WTable wTable2 = null;
		if (ownerParagraph2.IsInCell)
		{
			wTable2 = (ownerParagraph2.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable;
		}
		if (wTable != wTable2 && ownerParagraph.Owner == wTable.FirstRow.Cells[0])
		{
			wTable.RemoveSelf();
		}
		else
		{
			firstItem.RemoveSelf();
		}
	}

	private void FillCommItems()
	{
		foreach (TextBodyItem bodyItem in m_bodyPart.BodyItems)
		{
			if (bodyItem is WParagraph)
			{
				FillCommItems(bodyItem as WParagraph);
			}
			else
			{
				FillCommItems(bodyItem as WTable);
			}
		}
	}

	private void FillCommItems(WParagraph para)
	{
		foreach (ParagraphItem item in para.Items)
		{
			CommentedItems.InnerList.Add(item);
		}
	}

	private void FillCommItems(WTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (IEntity childEntity in cell.ChildEntities)
				{
					if (childEntity is WParagraph)
					{
						FillCommItems(childEntity as WParagraph);
					}
					else
					{
						FillCommItems(childEntity as WTable);
					}
				}
			}
		}
	}

	private string ModifyText(string text)
	{
		text = text.Replace(ControlChar.CrLf, ControlChar.CarriegeReturn);
		text = text.Replace(ControlChar.LineFeedChar, '\r');
		return text;
	}

	internal string SetParentParaIdAndIsResolved(List<string> paraIdOfComments)
	{
		foreach (WCommentExtended item in m_doc.CommentsEx)
		{
			if (ChildEntities.LastItem is WParagraph && item.ParaId == (ChildEntities.LastItem as WParagraph).ParaId)
			{
				ParentParaId = item.ParentParaId;
				Done = item.IsResolved;
				break;
			}
		}
		if (string.IsNullOrEmpty(ParentParaId) || !paraIdOfComments.Contains(ParentParaId))
		{
			return string.Empty;
		}
		for (int num = m_doc.Comments.InnerList.IndexOf(this) - 1; num >= 0; num--)
		{
			WComment wComment = m_doc.Comments[num];
			if (string.IsNullOrEmpty(wComment.ParentParaId))
			{
				string paraId = (wComment.ChildEntities.LastItem as WParagraph).ParaId;
				if (m_doc.Comments[num + 1] != null && paraId == m_doc.Comments[num + 1].ParentParaId)
				{
					return paraId;
				}
				return string.Empty;
			}
			if (!paraIdOfComments.Contains(wComment.ParentParaId))
			{
				return string.Empty;
			}
		}
		return string.Empty;
	}

	private WComment GetAncestorComment()
	{
		if (!string.IsNullOrEmpty(ParentParaId))
		{
			for (int num = m_doc.Comments.InnerList.IndexOf(this) - 1; num >= 0; num--)
			{
				WComment wComment = m_doc.Comments[num];
				if (wComment.ChildEntities.LastItem is WParagraph && (wComment.ChildEntities.LastItem as WParagraph).ParaId == ParentParaId)
				{
					return wComment;
				}
			}
		}
		return null;
	}
}
