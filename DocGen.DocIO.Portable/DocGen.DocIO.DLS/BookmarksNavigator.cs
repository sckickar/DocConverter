using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class BookmarksNavigator
{
	private const string c_DocumentPropertyNotInitialized = "You can not use DocumentNavigator without initializing Document property";

	private const string c_NotFoundSpecifiedBookmark = "Specified bookmark not found";

	private const string c_NotEqualDocumentProperty = " Document property must be equal this Document property";

	private const string c_CurrBookmarkNull = "Current Bookmark didn't select";

	private const string c_NotSupportGettingContent = "Not supported getting content between bookmarks in different paragraphs";

	private const string c_NotSupportDeletingContent = "Not supported deleting content between bookmarks in different paragraphs";

	private WordDocument m_document;

	private int m_currParagraphItemIndex;

	private IWParagraph m_currParagraph;

	private Bookmark m_currBookmark;

	private IParagraphItem m_currBookmarkItem;

	private byte m_flag = 4;

	public IWordDocument Document
	{
		get
		{
			return m_document;
		}
		set
		{
			m_document = (WordDocument)value;
		}
	}

	public Bookmark CurrentBookmark => m_currBookmark;

	private bool IsStart
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	private bool IsAfter
	{
		get
		{
			return (m_flag & 2) >> 1 != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool RemoveEmptyParagraph
	{
		get
		{
			return (m_flag & 4) >> 1 != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public IParagraphItem CurrentBookmarkItem
	{
		get
		{
			IParagraphItem currBookmarkItem;
			if (!IsStart && m_currBookmark.BookmarkEnd != null)
			{
				IParagraphItem bookmarkEnd = m_currBookmark.BookmarkEnd;
				currBookmarkItem = bookmarkEnd;
			}
			else
			{
				IParagraphItem bookmarkEnd = m_currBookmark.BookmarkStart;
				currBookmarkItem = bookmarkEnd;
			}
			m_currBookmarkItem = currBookmarkItem;
			return m_currBookmarkItem;
		}
	}

	private int CurrentParagraphItemIndex
	{
		get
		{
			if (m_currBookmark != null && CurrentBookmarkItem.Owner is WParagraph)
			{
				if (IsAfter)
				{
					m_currParagraphItemIndex = m_currBookmarkItem.OwnerParagraph.ChildEntities.IndexOf(m_currBookmarkItem) + 1;
				}
				else
				{
					m_currParagraphItemIndex = m_currBookmarkItem.OwnerParagraph.ChildEntities.IndexOf(m_currBookmarkItem);
				}
				return m_currParagraphItemIndex;
			}
			throw new ArgumentException("Specified bookmark not found");
		}
	}

	public BookmarksNavigator(IWordDocument doc)
	{
		m_document = (WordDocument)doc;
	}

	public void MoveToBookmark(string bookmarkName)
	{
		MoveToBookmark(bookmarkName, isStart: false, isAfter: false);
	}

	public void MoveToBookmark(string bookmarkName, bool isStart, bool isAfter)
	{
		IsStart = isStart;
		IsAfter = isAfter;
		string name = bookmarkName.Replace('-', '_');
		if (m_document == null)
		{
			throw new InvalidOperationException("You can not use DocumentNavigator without initializing Document property");
		}
		m_currBookmark = m_document.Bookmarks.FindByName(name);
		if (m_currBookmark != null)
		{
			IParagraphItem paragraphItem;
			if (!IsStart && m_currBookmark.BookmarkEnd != null)
			{
				IParagraphItem bookmarkEnd = m_currBookmark.BookmarkEnd;
				paragraphItem = bookmarkEnd;
			}
			else
			{
				IParagraphItem bookmarkEnd = m_currBookmark.BookmarkStart;
				paragraphItem = bookmarkEnd;
			}
			IParagraphItem paragraphItem2 = paragraphItem;
			m_currParagraph = paragraphItem2.OwnerParagraph;
			return;
		}
		throw new ArgumentException("Specified bookmark not found");
	}

	public IWTextRange InsertText(string text)
	{
		return InsertText(text, saveFormatting: true);
	}

	public IWTextRange InsertText(string text, bool saveFormatting)
	{
		return InsertText(text, saveFormatting, isReplaceContent: false);
	}

	public void InsertTable(IWTable table)
	{
		InsertBodyItem(table as TextBodyItem);
	}

	public IParagraphItem InsertParagraphItem(ParagraphItemType itemType)
	{
		IParagraphItem paragraphItem = m_document.CreateParagraphItem(itemType);
		m_currParagraph.Items.Insert(CurrentParagraphItemIndex, paragraphItem);
		return paragraphItem;
	}

	public void InsertParagraph(IWParagraph paragraph)
	{
		InsertBodyItem(paragraph as TextBodyItem);
	}

	public void InsertTextBodyPart(TextBodyPart bodyPart)
	{
		if (CurrentBookmarkItem == null)
		{
			return;
		}
		TextBodyItem textBodyItem = m_currBookmarkItem.Owner as TextBodyItem;
		int indexInOwnerCollection = textBodyItem.GetIndexInOwnerCollection();
		int num = (m_currBookmarkItem as ParagraphItem).GetIndexInOwnerCollection();
		BookmarkEnd bookmarkEnd = ((m_currBookmarkItem is BookmarkEnd) ? (m_currBookmarkItem as BookmarkEnd) : null);
		WParagraph wParagraph = textBodyItem as WParagraph;
		if (bookmarkEnd != null && bookmarkEnd.Name != "_GoBack" && !bookmarkEnd.HasRenderableItemBefore())
		{
			WParagraph ownerParagraph = Document.Bookmarks.FindByName(bookmarkEnd.Name).BookmarkStart.OwnerParagraph;
			WParagraph wParagraph2 = wParagraph.PreviousSibling as WParagraph;
			if (ownerParagraph != wParagraph && wParagraph2 != null)
			{
				indexInOwnerCollection = wParagraph2.GetIndexInOwnerCollection();
				num = wParagraph2.ChildEntities.Count;
				textBodyItem = wParagraph2;
			}
		}
		if (IsStart)
		{
			while (num < (textBodyItem as WParagraph).Items.Count)
			{
				ParagraphItem paragraphItem = (textBodyItem as WParagraph).Items[num - 1];
				if (!(paragraphItem is BookmarkEnd) || !((paragraphItem as BookmarkEnd).Name != CurrentBookmark.Name))
				{
					break;
				}
				num++;
				if (num > (textBodyItem as WParagraph).Items.Count - 1)
				{
					break;
				}
			}
		}
		else
		{
			while (num > 0)
			{
				ParagraphItem paragraphItem2 = (textBodyItem as WParagraph).Items[num - 1];
				if (!(paragraphItem2 is BookmarkStart) || !((paragraphItem2 as BookmarkStart).Name != CurrentBookmark.Name))
				{
					break;
				}
				num--;
				if (num < 1)
				{
					break;
				}
			}
		}
		bodyPart.PasteAt(textBodyItem.OwnerTextBody, indexInOwnerCollection, num, isBkmkReplace: true);
	}

	public TextBodyPart GetBookmarkContent()
	{
		CheckCurrentState();
		TextBodyPart textBodyPart = new TextBodyPart();
		textBodyPart.GetBookmarkContentPart(CurrentBookmark.BookmarkStart, CurrentBookmark.BookmarkEnd);
		return textBodyPart;
	}

	public WordDocumentPart GetContent()
	{
		CheckCurrentState();
		WordDocumentPart wordDocumentPart = new WordDocumentPart();
		wordDocumentPart.GetWordDocumentPart(CurrentBookmark.BookmarkStart, CurrentBookmark.BookmarkEnd);
		return wordDocumentPart;
	}

	public void DeleteBookmarkContent(bool saveFormatting)
	{
		DeleteBookmarkContent(saveFormatting, removeEmptyParagraph: false);
	}

	[Obsolete("This method will be removed in future version. As a work around to remove bookmarked paragraph, utilize current bookmark property of bookmark navigator to access the current bookmarked paragraph and then remove its index from its owner (Text Body) collection.", false)]
	public void DeleteBookmarkContent(bool saveFormatting, bool removeEmptyParagraph)
	{
		DeleteBookmarkContent(saveFormatting, removeEmptyParagrph: false, isReplaceBkmkContent: false);
	}

	public void ReplaceBookmarkContent(TextBodyPart bodyPart)
	{
		CurrentBookmark.BookmarkStart.Document.ImportStyles = false;
		if (bodyPart.BodyItems != null && bodyPart.BodyItems.Document != null)
		{
			bodyPart.BodyItems.Document.IsSkipFieldDetach = true;
		}
		DeleteBookmarkContent(saveFormatting: true, removeEmptyParagrph: false, isReplaceBkmkContent: true);
		string name = CurrentBookmark.Name;
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		if (!CurrentBookmarkItem.OwnerParagraph.IsInCell)
		{
			InsertTextBodyPart(bodyPart);
		}
		else
		{
			ReplaceTableBookmarkContent(null, null, bodyPart);
		}
		ReplaceBookmarkIndex(name, bkmkIndex);
		CurrentBookmark.BookmarkStart.Document.ImportStyles = true;
		if (bodyPart.BodyItems != null && bodyPart.BodyItems.Document != null)
		{
			bodyPart.BodyItems.Document.IsSkipFieldDetach = false;
		}
	}

	public void ReplaceContent(WordDocumentPart documentPart)
	{
		CurrentBookmark.BookmarkStart.Document.ImportStyles = false;
		DeleteBookmarkContent(saveFormatting: true, removeEmptyParagrph: false, isReplaceBkmkContent: true);
		string name = CurrentBookmark.Name;
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		if (!CurrentBookmarkItem.OwnerParagraph.IsInCell)
		{
			ReplaceParagraphBookmarkContent(documentPart.Sections);
		}
		else
		{
			ReplaceTableBookmarkContent(null, documentPart, null);
		}
		ReplaceBookmarkIndex(name, bkmkIndex);
		CurrentBookmark.BookmarkStart.Document.ImportStyles = true;
	}

	internal void ReplaceContent(WordDocument document)
	{
		CurrentBookmark.BookmarkStart.Document.ImportStyles = false;
		DeleteBookmarkContent(saveFormatting: true, removeEmptyParagrph: false, isReplaceBkmkContent: true);
		string name = CurrentBookmark.Name;
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		if (!CurrentBookmarkItem.OwnerParagraph.IsInCell)
		{
			ReplaceParagraphBookmarkContent(document.Sections);
		}
		else
		{
			ReplaceTableBookmarkContent(document, null, null);
		}
		ReplaceBookmarkIndex(name, bkmkIndex);
		CurrentBookmark.BookmarkStart.Document.ImportStyles = true;
	}

	public void ReplaceBookmarkContent(string text, bool saveFormatting)
	{
		CurrentBookmark.BookmarkStart.Document.ImportStyles = false;
		DeleteBookmarkContent(saveFormatting, removeEmptyParagrph: true, isReplaceBkmkContent: true);
		InsertText(text, saveFormatting, isReplaceContent: true);
		CurrentBookmark.BookmarkStart.Document.ImportStyles = true;
	}

	private void ReplaceBookmarkIndex(string bookmarkName, int bkmkIndex)
	{
		m_currBookmark = Document.Bookmarks.FindByName(bookmarkName);
		WParagraph ownerParagraph = m_currBookmark.BookmarkStart.OwnerParagraph;
		WParagraph ownerParagraph2 = m_currBookmark.BookmarkEnd.OwnerParagraph;
		int indexInOwnerCollection = m_currBookmark.BookmarkStart.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = m_currBookmark.BookmarkEnd.GetIndexInOwnerCollection();
		Document.Bookmarks.Remove(m_currBookmark);
		BookmarkStart bookmarkStart = new BookmarkStart(Document, bookmarkName);
		ownerParagraph.Items.Insert(indexInOwnerCollection, bookmarkStart);
		BookmarkEnd bookmarkEnd = new BookmarkEnd(Document, bookmarkName);
		ownerParagraph2.Items.Insert(indexInOwnerCollection2, bookmarkEnd);
		CurrentBookmark.SetStart(bookmarkStart);
		CurrentBookmark.SetEnd(bookmarkEnd);
		Document.Bookmarks.InnerList.Insert(bkmkIndex, CurrentBookmark);
		Document.Bookmarks.InnerList.RemoveAt(Document.Bookmarks.Count - 1);
		m_currBookmark = Document.Bookmarks.FindByName(bookmarkName);
	}

	private void ReplaceParagraphBookmarkContent(WSectionCollection sections)
	{
		if (sections.Count == 1)
		{
			TextBodyPart textBodyPart = new TextBodyPart();
			textBodyPart.m_textPart = sections[0].Body;
			InsertTextBodyPart(textBodyPart);
		}
		else
		{
			if (sections.Count <= 1)
			{
				return;
			}
			int num = GetSection(CurrentBookmark.BookmarkStart.Owner).GetIndexInOwnerCollection();
			int num2 = 0;
			while (num2 < sections.Count)
			{
				if (num2 == 0)
				{
					WSection wSection = sections[0].Clone();
					WParagraph ownerParagraph = CurrentBookmark.BookmarkStart.OwnerParagraph;
					WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
					int num3 = ownerParagraph.GetIndexInOwnerCollection();
					int num4 = CurrentBookmark.BookmarkStart.GetIndexInOwnerCollection();
					bool updateRevisionOnComparing = false;
					if (ownerTextBody.Document.IsComparing)
					{
						updateRevisionOnComparing = ownerTextBody.Document.UpdateRevisionOnComparing;
						ownerTextBody.Document.UpdateRevisionOnComparing = false;
					}
					if (wSection.Body.Items[0] is WParagraph)
					{
						WParagraph wParagraph = wSection.Body.Items[0] as WParagraph;
						while (num4 >= 0)
						{
							wParagraph.Items.Insert(0, ownerParagraph.Items[num4].Clone());
							ownerParagraph.Items[num4].RemoveSelf();
							num4--;
						}
						num3--;
					}
					else if (CurrentBookmark.BookmarkStart.OwnerParagraph == CurrentBookmark.BookmarkEnd.OwnerParagraph)
					{
						WParagraph wParagraph2 = ownerParagraph.CloneWithoutItems();
						while (num4 >= 0)
						{
							wParagraph2.Items.Insert(0, ownerParagraph.Items[num4].Clone());
							ownerParagraph.Items[num4].RemoveSelf();
							num4--;
						}
						wSection.Body.Items.Insert(0, wParagraph2);
						num3--;
					}
					while (num3 > -1)
					{
						wSection.Body.Items.Insert(0, ownerTextBody.Items[num3].Clone());
						ownerTextBody.Items[num3].RemoveSelf();
						num3--;
					}
					if (ownerTextBody.Document.IsComparing)
					{
						ownerTextBody.Document.UpdateRevisionOnComparing = updateRevisionOnComparing;
					}
					Document.Sections.Insert(num, wSection);
				}
				else if (num2 == sections.Count - 1)
				{
					sections[sections.Count - 1].BreakCode = SectionBreakCode.NewPage;
					WSection wSection2 = sections[sections.Count - 1].Clone();
					WParagraph ownerParagraph2 = CurrentBookmark.BookmarkEnd.OwnerParagraph;
					WTextBody ownerTextBody2 = ownerParagraph2.OwnerTextBody;
					CurrentBookmark.BookmarkEnd.GetIndexInOwnerCollection();
					int num5 = sections[sections.Count - 1].Body.Items.Count - 1;
					for (int num6 = num5; num6 >= 0; num6--)
					{
						if (num6 == num5 && wSection2.Body.Items[num5] is WParagraph)
						{
							WParagraph wParagraph3 = wSection2.Body.Items[num5] as WParagraph;
							bool updateRevisionOnComparing2 = false;
							if (ownerParagraph2.Document.IsComparing)
							{
								updateRevisionOnComparing2 = ownerParagraph2.Document.UpdateRevisionOnComparing;
								ownerParagraph2.Document.UpdateRevisionOnComparing = false;
							}
							while (wParagraph3.Items.Count != 0)
							{
								ownerParagraph2.Items.Insert(0, wParagraph3.Items[wParagraph3.Items.Count - 1]);
							}
							if (ownerParagraph2.Document.IsComparing)
							{
								ownerParagraph2.Document.UpdateRevisionOnComparing = updateRevisionOnComparing2;
								ownerParagraph2.UpdateParagraphRevision(ownerParagraph2, isIncludeParaItems: true);
							}
						}
						else
						{
							ownerTextBody2.Items.Insert(0, wSection2.Body.Items[num6]);
						}
					}
					m_currBookmark = Document.Bookmarks.FindByName(CurrentBookmark.Name);
					for (int i = 0; i < ownerParagraph2.ChildEntities.Count; i++)
					{
						if (ownerParagraph2.ChildEntities[i] is BookmarkEnd && (ownerParagraph2.ChildEntities[i] as BookmarkEnd).Name == CurrentBookmark.Name)
						{
							m_currBookmark.SetEnd(ownerParagraph2.ChildEntities[i] as BookmarkEnd);
							break;
						}
					}
				}
				else
				{
					Document.Sections.Insert(num, sections[num2].Clone());
				}
				num2++;
				num++;
			}
		}
	}

	private void DeleteBookmarkContent(bool saveFormatting, bool removeEmptyParagrph, bool isReplaceBkmkContent)
	{
		m_document.IsDeletingBookmarkContent = true;
		BookmarkStart bookmarkStart = CurrentBookmark.BookmarkStart;
		BookmarkEnd bookmarkEnd = CurrentBookmark.BookmarkEnd;
		IsStart = false;
		IsAfter = false;
		if (CurrentBookmark == null)
		{
			throw new InvalidOperationException();
		}
		if (bookmarkEnd != null)
		{
			WParagraph ownerParagraph = bookmarkStart.OwnerParagraph;
			WParagraph ownerParagraph2 = bookmarkEnd.OwnerParagraph;
			if (ownerParagraph.Owner != ownerParagraph2.Owner)
			{
				if (!ownerParagraph.IsInCell && !ownerParagraph2.IsInCell)
				{
					DeleteBookmarkTextBody(saveFormatting, isReplaceBkmkContent);
				}
				else if (ownerParagraph.IsInCell && ownerParagraph2.IsInCell)
				{
					DeleteBkmkContentInDiffCell(ownerParagraph, ownerParagraph2, bookmarkStart, bookmarkEnd, null, isReplaceBkmkContent);
				}
				else if (ownerParagraph.IsInCell && !ownerParagraph2.IsInCell)
				{
					DeleteBkmkContentInTableAfterParagraph(ownerParagraph, ownerParagraph2, bookmarkStart, bookmarkEnd, isReplaceBkmkContent);
				}
				else if (!ownerParagraph.IsInCell && ownerParagraph2.IsInCell)
				{
					DeleteBkmkContentInParagraphAftertable(ownerParagraph, ownerParagraph2, bookmarkStart, bookmarkEnd, isReplaceBkmkContent);
				}
			}
			else if (bookmarkEnd.IsAfterCellMark || bookmarkEnd.IsAfterRowMark || bookmarkEnd.IsAfterTableMark)
			{
				DeleteBkmkContentInDiffCell(ownerParagraph, ownerParagraph2, bookmarkStart, bookmarkEnd, null, isReplaceBkmkContent);
			}
			else
			{
				DeleteBookmarkTextBody(saveFormatting, isReplaceBkmkContent);
			}
		}
		if (m_document.Bookmarks.InnerList.Contains(CurrentBookmark))
		{
			MoveToBookmark(CurrentBookmark.Name, IsStart, IsAfter);
		}
		m_document.IsDeletingBookmarkContent = false;
	}

	private void DeleteInBetweenSections(int startSectiontionIndex, int endSectiontionIndex)
	{
		for (int i = startSectiontionIndex; i < endSectiontionIndex; i++)
		{
			Document.Sections.RemoveAt(startSectiontionIndex);
		}
	}

	private void DeleteBkmkContentInDiffCell(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, WTableCell bkmkEndCell, bool isReplaceContent)
	{
		WTableCell wTableCell = paragraphStart.GetOwnerEntity() as WTableCell;
		if (paragraphEnd.IsInCell)
		{
			bkmkEndCell = paragraphEnd.GetOwnerEntity() as WTableCell;
		}
		WTableCell tempBkmkEndCell = bkmkEndCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTable bkmkEndTable = (WTable)bkmkEndCell.Owner.Owner;
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int endTableRowIndex = bkmkEndCell.Owner.GetIndexInOwnerCollection();
		int bkmkEndCellIndex = bkmkEndCell.GetIndexInOwnerCollection();
		int bkmkStartCellIndex = wTableCell.GetIndexInOwnerCollection();
		if (m_document.IsMailMerge && wTable == bkmkEndTable && indexInOwnerCollection == endTableRowIndex)
		{
			DeleteMailMergeBkmkCntInDiffCell(paragraphStart, paragraphEnd, bkmkStart, bkmkEnd, wTableCell, bkmkEndCell, wTable, indexInOwnerCollection);
			return;
		}
		bkmkStart.GetBookmarkStartAndEndCell(wTableCell, bkmkEndCell, tempBkmkEndCell, wTable, bkmkEndTable, bkmkStart, bkmkEnd, indexInOwnerCollection, ref endTableRowIndex, ref bkmkStartCellIndex, ref bkmkEndCellIndex);
		if (wTable != bkmkEndTable)
		{
			if (isReplaceContent && bkmkEndTable.Rows.Count - 1 != endTableRowIndex)
			{
				throw new InvalidOperationException("You cannot replace bookmark content when the bookmark starts in one table and ends in another table partially");
			}
			if (bkmkEndTable.IsInCell && bkmkStart.IsBookmarkEndAtSameCell(wTableCell, wTable, ref bkmkEndTable, ref endTableRowIndex))
			{
				DeleteNestedTableBookmarkTextBodyInSameCell(bkmkStart, wTableCell, bkmkEndTable, endTableRowIndex);
			}
			else
			{
				DeleteTableBookmarkTextBody(wTable, bkmkEndTable, indexInOwnerCollection, endTableRowIndex, isReplaceContent, paragraphEnd, bkmkEnd);
			}
		}
		else
		{
			bool num = IsBookmarkEnclosedTable(wTable, bkmkEndTable, bkmkStart, bkmkEnd);
			DeleteTableBookmarkTextBody(wTable, indexInOwnerCollection, endTableRowIndex, bkmkStartCellIndex, bkmkEndCellIndex, bkmkStart, bkmkEnd);
			if (num)
			{
				int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
				WTextBody ownerTextBody = wTable.OwnerTextBody;
				int indexInOwnerCollection2 = wTable.GetIndexInOwnerCollection();
				if (wTable.NextSibling is WParagraph)
				{
					WParagraph paragraph = wTable.NextSibling as WParagraph;
					ReplaceCurrentBookmark(paragraph, 0, 1, bkmkIndex);
				}
				else if (wTable.PreviousSibling is WParagraph)
				{
					WParagraph paragraph = wTable.PreviousSibling as WParagraph;
					ReplaceCurrentBookmark(paragraph, paragraph.Items.Count, paragraph.Items.Count + 1, bkmkIndex);
				}
				else
				{
					WParagraph paragraph = new WParagraph(Document);
					ownerTextBody.ChildEntities.Insert(indexInOwnerCollection2, paragraph);
					ReplaceCurrentBookmark(paragraph, 0, 1, bkmkIndex);
				}
				wTable.RemoveSelf();
			}
		}
		m_currBookmark = Document.Bookmarks.FindByName(bkmkEnd.Name);
		if (m_currBookmark.BookmarkEnd == null)
		{
			m_currBookmark.SetEnd(bkmkEnd);
		}
	}

	private void DeleteMailMergeBkmkCntInDiffCell(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, WTableCell bkmkStartCell, WTableCell bkmkEndCell, WTable bkmkTable, int bkmkStartRowIndex)
	{
		int indexInOwnerCollection = bkmkStartCell.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = bkmkEndCell.GetIndexInOwnerCollection();
		WTableRow wTableRow = bkmkTable.Rows[bkmkStartRowIndex];
		int indexInOwnerCollection3 = bkmkStart.GetIndexInOwnerCollection();
		int indexInOwnerCollection4 = paragraphStart.GetIndexInOwnerCollection();
		int indexInOwnerCollection5 = paragraphEnd.GetIndexInOwnerCollection();
		int num = bkmkEnd.GetIndexInOwnerCollection();
		for (int i = indexInOwnerCollection; i <= indexInOwnerCollection2; i++)
		{
			if (i == indexInOwnerCollection)
			{
				for (int j = indexInOwnerCollection4; j < bkmkStartCell.ChildEntities.Count; j++)
				{
					if (j == indexInOwnerCollection4)
					{
						int num2;
						for (num2 = indexInOwnerCollection3; num2 < paragraphStart.ChildEntities.Count; num2++)
						{
							paragraphStart.ChildEntities.RemoveAt(num2);
							num2--;
						}
					}
					else
					{
						bkmkStartCell.ChildEntities.RemoveAt(j);
						j--;
					}
				}
			}
			else if (i == indexInOwnerCollection2)
			{
				for (int k = 0; k <= indexInOwnerCollection5; k++)
				{
					if (k == indexInOwnerCollection5)
					{
						int num3;
						for (num3 = 0; num3 < num; num3++)
						{
							paragraphEnd.ChildEntities.RemoveAt(k);
							num3--;
							num--;
						}
					}
					else
					{
						bkmkEndCell.ChildEntities.RemoveAt(k);
						k--;
					}
				}
			}
			else
			{
				wTableRow.Cells[i].RemoveSelf();
			}
		}
		for (int l = indexInOwnerCollection; l < wTableRow.Cells.Count; l++)
		{
			if (l == wTableRow.Cells.Count - 1)
			{
				wTableRow.Cells[l].RemoveSelf();
				continue;
			}
			WTableCell wTableCell = wTableRow.Cells[l];
			WTableCell wTableCell2 = wTableRow.Cells[l + 1];
			int num4;
			for (num4 = 0; num4 < wTableCell2.ChildEntities.Count; num4++)
			{
				wTableCell.ChildEntities.Add(wTableCell2.ChildEntities[num4]);
				num4--;
			}
		}
	}

	private void CreateBookmark(IWParagraph paragraphStart, IWParagraph paragraphEnd, int bkmkIndex, BookmarkStart bkmkStart, int columnFirst, int columnLast)
	{
		if (Document.Bookmarks.FindByName(CurrentBookmark.Name) != null)
		{
			Document.Bookmarks.Remove(CurrentBookmark);
		}
		BookmarkStart bookmarkStart = new BookmarkStart(Document, CurrentBookmark.Name);
		paragraphStart.ChildEntities.Add(bookmarkStart);
		BookmarkEnd bookmarkEnd = new BookmarkEnd(Document, CurrentBookmark.Name);
		paragraphEnd.ChildEntities.Add(bookmarkEnd);
		CurrentBookmark.SetStart(bookmarkStart);
		CurrentBookmark.SetEnd(bookmarkEnd);
		if (bkmkIndex < Document.Bookmarks.Count)
		{
			Document.Bookmarks.InnerList.Insert(bkmkIndex, CurrentBookmark);
			Document.Bookmarks.InnerList.RemoveAt(Document.Bookmarks.Count - 1);
		}
		m_currBookmark = Document.Bookmarks.FindByName(bkmkStart.Name);
		m_currBookmark.BookmarkStart.ColumnFirst = (short)columnFirst;
		m_currBookmark.BookmarkStart.ColumnLast = (short)columnLast;
	}

	private bool IsBookmarkEnclosedTable(WTable bkmkStartTable, WTable bkmkEndTable, BookmarkStart bookmarkStart, BookmarkEnd bookmarkEnd)
	{
		int indexInOwnerCollection = bookmarkStart.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = bookmarkEnd.GetIndexInOwnerCollection();
		bool num = indexInOwnerCollection == 0 && bookmarkStart.OwnerParagraph == bkmkStartTable.FirstRow.Cells[0].Paragraphs[0];
		WParagraph wParagraph = bkmkEndTable.NextSibling as WParagraph;
		bool flag = (bookmarkEnd.OwnerParagraph == bkmkEndTable.LastCell.LastParagraph && indexInOwnerCollection2 == bkmkEndTable.LastCell.LastParagraph.ChildEntities.Count - 1) || (wParagraph != null && bookmarkEnd.OwnerParagraph == wParagraph && !bookmarkEnd.HasRenderableItemBefore());
		return num && flag;
	}

	private void DeleteNestedTableBookmarkTextBodyInSameCell(BookmarkStart bkmrkStart, WTableCell bkmkStartCell, WTable endTable, int bkmkEndRowIndex)
	{
		int num = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		WParagraph ownerParagraph = bkmrkStart.OwnerParagraph;
		int index = ownerParagraph.Index;
		int index2 = endTable.Index;
		for (int num2 = ownerParagraph.Items.Count - 1; num2 >= bkmrkStart.Index + 1; num2--)
		{
			if (!(ownerParagraph.Items[num2] is BookmarkStart) && !(ownerParagraph.Items[num2] is BookmarkEnd))
			{
				ownerParagraph.Items.RemoveAt(num2);
			}
		}
		for (int i = index; i <= index2 && i < bkmkStartCell.Items.Count; i++)
		{
			if (bkmkStartCell.Items[ownerParagraph.Index + 1] is WParagraph)
			{
				WParagraph wParagraph = bkmkStartCell.Items[index + 1] as WParagraph;
				DeleteParagraphItemsInCell(wParagraph);
				wParagraph.RemoveSelf();
			}
			else if (bkmkStartCell.Items[index + 1] is WTable)
			{
				WTable wTable = bkmkStartCell.Items[index + 1] as WTable;
				if (wTable == endTable)
				{
					DeleteTableRows(0, bkmkEndRowIndex, wTable);
					break;
				}
				wTable.RemoveSelf();
			}
		}
		if (Document.Bookmarks.FindByName(CurrentBookmark.Name) != null)
		{
			Document.Bookmarks.Remove(CurrentBookmark);
		}
		BookmarkStart bookmarkStart = new BookmarkStart(Document, CurrentBookmark.Name);
		ownerParagraph.ChildEntities.Add(bookmarkStart);
		BookmarkEnd bookmarkEnd = new BookmarkEnd(Document, CurrentBookmark.Name);
		ownerParagraph.ChildEntities.Add(bookmarkEnd);
		CurrentBookmark.SetStart(bookmarkStart);
		CurrentBookmark.SetEnd(bookmarkEnd);
		if (num < Document.Bookmarks.Count)
		{
			Document.Bookmarks.InnerList.Insert(num, CurrentBookmark);
			Document.Bookmarks.InnerList.RemoveAt(Document.Bookmarks.Count - 1);
		}
		m_currBookmark = Document.Bookmarks.FindByName(bookmarkStart.Name);
	}

	private void DeleteTableBookmarkTextBody(WTable bkmkStartTable, WTable bkmkEndTable, int bkmkStartRowIndex, int bkmkEndRowIndex, bool isReplaceContent, WParagraph paragraphEnd, BookmarkEnd bkmkEnd)
	{
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		WTextBody wTextBody = (WTextBody)bkmkStartTable.Owner;
		WTextBody wTextBody2 = (WTextBody)bkmkEndTable.Owner;
		int num = GetSection(bkmkStartTable).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection = GetSection(bkmkEndTable).GetIndexInOwnerCollection();
		int num2 = bkmkStartTable.GetIndexInOwnerCollection() + 1;
		int endParagraphIndex = bkmkEndTable.GetIndexInOwnerCollection();
		bool flag = false;
		if (num - 1 == indexInOwnerCollection)
		{
			flag = true;
		}
		WTable wTable = null;
		if (bkmkStartTable.IsInCell)
		{
			wTable = wTextBody.Owner.Owner as WTable;
			while (wTable != null && wTable.IsInCell)
			{
				wTable = wTable.Owner.Owner.Owner as WTable;
			}
		}
		if (wTable != null && wTable == bkmkEndTable)
		{
			DeleteTableRows(bkmkStartRowIndex, bkmkStartTable.Rows.Count - 1, bkmkStartTable);
			WTable wTable2 = wTextBody.Owner.Owner as WTable;
			int index = wTextBody.Owner.Index;
			while (wTable2 != null && wTable2 != bkmkEndTable)
			{
				DeleteTableRows(index, wTable2.Rows.Count - 1, wTable2);
				wTable2 = wTable2.Owner.Owner.Owner as WTable;
				index = wTable2.Owner.Owner.Index;
			}
		}
		else
		{
			DeleteTableRows(bkmkStartRowIndex, bkmkStartTable.Rows.Count - 1, bkmkStartTable);
			DeleteFirstSectionItemsFromDocument(num2, ref endParagraphIndex, wTextBody, -1, flag);
		}
		if (!flag)
		{
			DeleteInBetweenSections(indexInOwnerCollection, indexInOwnerCollection);
		}
		DeleteLastSectionItemsFromDocument(endParagraphIndex, wTextBody2, IsFirstBkmkEnd: true, flag);
		if (IsBkmkEndFirstItemInTable(bkmkEndRowIndex, bkmkEndTable, paragraphEnd, bkmkEnd))
		{
			bkmkEnd.RemoveSelf();
		}
		else
		{
			DeleteTableRows(0, bkmkEndRowIndex, bkmkEndTable, paragraphEnd, bkmkEnd);
		}
		if (bkmkEndTable.Rows.Count == 0)
		{
			bkmkEndTable.RemoveSelf();
		}
		if (bkmkStartTable.Rows.Count == 0)
		{
			bkmkStartTable.RemoveSelf();
			num2--;
		}
		if (!isReplaceContent && CheckTwoTableProperties(bkmkStartTable, bkmkEndTable))
		{
			if (RemoveEmptyParagraph)
			{
				SetCurrentBookmarkPosition(bkmkStartTable, bkmkEndTable, wTextBody, wTextBody2, bkmkIndex);
			}
			if (bkmkStartTable.Rows.Count != 0 && bkmkEndTable.Rows.Count != 0)
			{
				while (bkmkEndTable.Rows.Count != 0)
				{
					bkmkStartTable.Rows.Add(bkmkEndTable.Rows[0]);
				}
				bkmkStartTable.UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
			}
		}
		else
		{
			WParagraph wParagraph = new WParagraph(Document);
			ReplaceCurrentBookmark(wParagraph, 0, 1, bkmkIndex);
			wTextBody.Items.Insert(num2, wParagraph);
		}
		if (!flag)
		{
			MergeMultiSectionBodyItems(wTextBody, wTextBody2);
		}
	}

	private bool CheckTwoTableProperties(WTable bkmkStartTable, WTable bkmkEndTable)
	{
		bool result = false;
		if (bkmkStartTable.TableFormat.WrapTextAround && bkmkEndTable.TableFormat.WrapTextAround)
		{
			if (bkmkStartTable.TableFormat.HorizontalAlignment != bkmkEndTable.TableFormat.HorizontalAlignment || bkmkStartTable.TableFormat.Positioning.AllowOverlap != bkmkEndTable.TableFormat.Positioning.AllowOverlap || bkmkStartTable.TableFormat.Positioning.DistanceFromBottom != bkmkEndTable.TableFormat.Positioning.DistanceFromBottom || bkmkStartTable.TableFormat.Positioning.DistanceFromLeft != bkmkEndTable.TableFormat.Positioning.DistanceFromLeft || bkmkStartTable.TableFormat.Positioning.DistanceFromRight != bkmkEndTable.TableFormat.Positioning.DistanceFromRight || bkmkStartTable.TableFormat.Positioning.DistanceFromTop != bkmkEndTable.TableFormat.Positioning.DistanceFromTop || bkmkStartTable.TableFormat.Positioning.VertRelationTo != bkmkEndTable.TableFormat.Positioning.VertRelationTo || bkmkStartTable.TableFormat.Positioning.VertPosition != bkmkEndTable.TableFormat.Positioning.VertPosition || bkmkStartTable.TableFormat.Positioning.VertPositionAbs != bkmkEndTable.TableFormat.Positioning.VertPositionAbs || bkmkStartTable.TableFormat.Positioning.HorizPosition != bkmkEndTable.TableFormat.Positioning.HorizPosition || bkmkStartTable.TableFormat.Positioning.HorizPositionAbs != bkmkEndTable.TableFormat.Positioning.HorizPositionAbs || bkmkStartTable.TableFormat.Positioning.HorizRelationTo != bkmkEndTable.TableFormat.Positioning.HorizRelationTo)
			{
				return false;
			}
			result = true;
		}
		else if (!bkmkStartTable.TableFormat.WrapTextAround && !bkmkEndTable.TableFormat.WrapTextAround)
		{
			result = true;
		}
		if (bkmkStartTable.StyleName != null || bkmkEndTable.StyleName != null)
		{
			result = ((bkmkStartTable.StyleName == bkmkEndTable.StyleName) ? true : false);
		}
		return result;
	}

	private void DeleteBkmkContentInTableAfterParagraph(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, bool isReplaceContent)
	{
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		WTableCell wTableCell = paragraphStart.GetOwnerEntity() as WTableCell;
		WTableCell wTableCell2 = null;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int num = -1;
		if (bkmkStart.ColumnFirst != -1 && bkmkStart.ColumnLast != -1)
		{
			num = wTable.Rows.Count - 1;
			if (bkmkStart.ColumnFirst < wTable.Rows[indexInOwnerCollection].Cells.Count && bkmkStart.ColumnLast < wTable.Rows[num].Cells.Count)
			{
				wTableCell = wTable.Rows[indexInOwnerCollection].Cells[bkmkStart.ColumnFirst];
				wTableCell2 = wTable.Rows[num].Cells[bkmkStart.ColumnLast];
			}
			int indexInOwnerCollection2 = wTableCell.GetIndexInOwnerCollection();
			int indexInOwnerCollection3 = wTableCell2.GetIndexInOwnerCollection();
			DeleteTableBookmarkTextBody(wTable, indexInOwnerCollection, num, indexInOwnerCollection2, indexInOwnerCollection3, bkmkStart, bkmkEnd);
			return;
		}
		int num2 = wTable.GetIndexInOwnerCollection();
		int endParagraphIndex = paragraphEnd.GetIndexInOwnerCollection();
		int bkmkEndPreviosItemIndex = bkmkEnd.GetIndexInOwnerCollection() - 1;
		WTextBody wTextBody = (WTextBody)wTable.Owner;
		WTextBody wTextBody2 = (WTextBody)paragraphEnd.Owner;
		bool flag = false;
		int num3 = GetSection(wTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection4 = GetSection(wTextBody2).GetIndexInOwnerCollection();
		if (num3 - 1 == indexInOwnerCollection4)
		{
			flag = true;
		}
		bool flag2 = false;
		flag2 = IsBkmkEndInFirstItem(paragraphEnd, bkmkEnd, bkmkEndPreviosItemIndex);
		if (!(flag && wTextBody.ChildEntities[num2 + 1] == paragraphEnd && flag2))
		{
			DeleteTableRows(indexInOwnerCollection, wTable.Rows.Count - 1, wTable);
			if (wTable.Rows.Count == 0)
			{
				wTable.RemoveSelf();
				if (flag)
				{
					endParagraphIndex--;
				}
				num2--;
			}
			DeleteFirstSectionItemsFromDocument(num2 + 1, ref endParagraphIndex, wTextBody, -1, flag);
			if (flag2)
			{
				endParagraphIndex--;
			}
			if (!flag || !flag2)
			{
				DeleteInBetweenSections(num3, indexInOwnerCollection4);
				if (endParagraphIndex >= 0)
				{
					DeleteLastSectionItemsFromDocument(endParagraphIndex, wTextBody2, flag2, flag);
				}
			}
			WParagraph paragraphToInsertBookmark = GetParagraphToInsertBookmark(wTextBody, wTextBody2, paragraphEnd, endParagraphIndex, bkmkStart, bkmkEnd, flag, isParaAfterTable: false);
			if (paragraphToInsertBookmark != null)
			{
				ReplaceCurrentBookmark(paragraphToInsertBookmark, 0, 1, bkmkIndex);
				if (paragraphToInsertBookmark != paragraphEnd)
				{
					MoveNestedBookmark(paragraphEnd, paragraphToInsertBookmark);
					paragraphEnd.RemoveSelf();
				}
			}
			else
			{
				ReplaceCurrentBookmark(paragraphEnd, 0, 1, bkmkIndex);
			}
			if (!flag)
			{
				MergeMultiSectionBodyItems(wTextBody, wTextBody2);
			}
		}
		else
		{
			DeleteBkmkContentInDiffCell(paragraphStart, paragraphEnd, bkmkStart, bkmkEnd, wTable.LastCell, isReplaceContent);
		}
	}

	private void DeleteTableRows(int startRowIndex, int endRowIndex, WTable bkmkTable)
	{
		if (bkmkTable.PreviousSibling is Entity entity)
		{
			Stack<Entity> stack = new Stack<Entity>();
			UpdateBookmark(bkmkTable, stack);
			WParagraph wParagraph = entity as WParagraph;
			if (entity is WTable)
			{
				wParagraph = (entity as WTable).LastCell.LastParagraph as WParagraph;
			}
			int count = wParagraph.Items.Count;
			while (stack.Count > 0)
			{
				Entity entity2 = stack.Pop();
				if (startRowIndex == 0)
				{
					wParagraph.Items.Insert(count, entity2);
				}
			}
		}
		for (int i = startRowIndex; i <= endRowIndex; i++)
		{
			bkmkTable.Rows[startRowIndex].RemoveSelf();
		}
	}

	private void UpdateBookmark(TextBodyItem textBodyItem, Stack<Entity> bookmarks)
	{
		if (textBodyItem is WTable)
		{
			foreach (WTableRow row in (textBodyItem as WTable).Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					foreach (TextBodyItem item in cell.Items)
					{
						UpdateBookmark(item, bookmarks);
					}
				}
			}
		}
		if (!(textBodyItem is WParagraph))
		{
			return;
		}
		for (int i = 0; i < (textBodyItem as WParagraph).Items.Count; i++)
		{
			ParagraphItem paragraphItem = (textBodyItem as WParagraph).Items[i];
			if ((!(paragraphItem is BookmarkStart) || !((paragraphItem as BookmarkStart).Name != CurrentBookmark.Name)) && (!(paragraphItem is BookmarkEnd) || !((paragraphItem as BookmarkEnd).Name != CurrentBookmark.Name)))
			{
				continue;
			}
			BookmarkStart bookmarkStart = ((paragraphItem is BookmarkStart) ? (paragraphItem as BookmarkStart) : ((Document.Bookmarks.FindByName((paragraphItem as BookmarkEnd).Name) != null) ? Document.Bookmarks.FindByName((paragraphItem as BookmarkEnd).Name).BookmarkStart : null));
			BookmarkEnd bookmarkEnd = ((paragraphItem is BookmarkEnd) ? (paragraphItem as BookmarkEnd) : ((Document.Bookmarks.FindByName((paragraphItem as BookmarkStart).Name) != null) ? Document.Bookmarks.FindByName((paragraphItem as BookmarkStart).Name).BookmarkEnd : null));
			if (bookmarkStart != null && bookmarkEnd != null)
			{
				Entity ownerEntity = GetOwnerEntity(bookmarkEnd);
				Entity ownerEntity2 = GetOwnerEntity(bookmarkStart);
				if (ownerEntity != ownerEntity2)
				{
					bookmarks.Push(paragraphItem);
				}
			}
		}
	}

	private Entity GetOwnerEntity(Entity entity)
	{
		Entity owner = entity.Owner;
		while (!(owner is WTable) && (!(owner is WParagraph) || !((owner as WParagraph).OwnerTextBody.Owner is WSection)) && owner.Owner != null)
		{
			owner = owner.Owner;
		}
		return owner;
	}

	private void DeleteTableRows(int startRowIndex, int endRowIndex, WTable bkmkTable, WParagraph paragraphEnd, BookmarkEnd bkmkEnd)
	{
		int indexInOwnerCollection = (paragraphEnd.GetOwnerEntity() as WTableCell).GetIndexInOwnerCollection();
		for (int i = startRowIndex; i <= endRowIndex; i++)
		{
			if (i == endRowIndex)
			{
				WTableRow wTableRow = bkmkTable.Rows[i];
				for (int j = 0; j <= indexInOwnerCollection; j++)
				{
					if (j == indexInOwnerCollection)
					{
						DeleteBookmarkEndRow(paragraphEnd, bkmkEnd, j, wTableRow);
					}
					else
					{
						wTableRow.Cells[j].ChildEntities.Clear();
					}
				}
			}
			else
			{
				bkmkTable.Rows[startRowIndex].RemoveSelf();
			}
		}
	}

	private void DeleteBookmarkEndRow(WParagraph paragraphEnd, BookmarkEnd bkmkEnd, int j, WTableRow endRow)
	{
		int indexInOwnerCollection = paragraphEnd.GetIndexInOwnerCollection();
		for (int i = 0; i <= indexInOwnerCollection; i++)
		{
			if (i == indexInOwnerCollection)
			{
				int num = 0;
				while (num < bkmkEnd.GetIndexInOwnerCollection())
				{
					ParagraphItem paragraphItem = paragraphEnd.ChildEntities[num] as ParagraphItem;
					if (paragraphItem is BookmarkEnd)
					{
						Bookmark bookmark = Document.Bookmarks.FindByName((paragraphItem as BookmarkEnd).Name);
						if (bookmark != null && bookmark.BookmarkStart != null && bookmark.BookmarkStart.IsDetached)
						{
							paragraphItem.RemoveSelf();
						}
						else
						{
							num++;
						}
					}
					else if (paragraphItem is BookmarkStart)
					{
						Bookmark bookmark2 = Document.Bookmarks.FindByName((paragraphItem as BookmarkStart).Name);
						WParagraph ownerParagraph = bookmark2.BookmarkEnd.OwnerParagraph;
						if (CheckBookmarkEnd(paragraphItem.OwnerParagraph, ownerParagraph, bookmark2, bkmkEnd.GetIndexInOwnerCollection()))
						{
							paragraphItem.RemoveSelf();
						}
						else
						{
							num++;
						}
					}
					else
					{
						paragraphItem.RemoveSelf();
					}
				}
				bkmkEnd.RemoveSelf();
			}
			else
			{
				endRow.Cells[j].ChildEntities.RemoveAt(i);
			}
		}
	}

	private bool CheckBookmarkEnd(WParagraph bkmkStartPara, WParagraph bkmkEndPara, Bookmark bkmk, int endIndex)
	{
		if (bkmkStartPara == bkmkEndPara)
		{
			if (bkmk.BookmarkEnd.GetIndexInOwnerCollection() < endIndex)
			{
				return true;
			}
			return false;
		}
		if (bkmkStartPara.OwnerTextBody == bkmkEndPara.OwnerTextBody)
		{
			if (bkmkStartPara.Index < bkmkEndPara.Index)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void DeleteBkmkContentInParagraphAftertable(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, bool isReplaceContent)
	{
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		WTextBody ownerTextBody = paragraphStart.OwnerTextBody;
		WTableCell wTableCell = paragraphEnd.GetOwnerEntity() as WTableCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTextBody ownerTextBody2 = wTable.OwnerTextBody;
		int endParagraphIndex = wTable.GetIndexInOwnerCollection();
		int num = GetSection(ownerTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection = GetSection(ownerTextBody2).GetIndexInOwnerCollection();
		bool flag = false;
		if (num - 1 == indexInOwnerCollection)
		{
			flag = true;
		}
		int num2 = wTableCell.Owner.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = wTableCell.GetIndexInOwnerCollection();
		if (indexInOwnerCollection2 == 0 && num2 != 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph && IsBkmkEndInFirstItem(wTableCell.Paragraphs[0], bkmkEnd, bkmkEnd.GetIndexInOwnerCollection() - 1))
		{
			num2--;
		}
		int indexInOwnerCollection3 = paragraphStart.GetIndexInOwnerCollection();
		int num3 = bkmkStart.GetIndexInOwnerCollection() + 1;
		if (endParagraphIndex >= 0 && num2 == 0 && indexInOwnerCollection2 == 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph)
		{
			endParagraphIndex--;
			if (wTableCell.ChildEntities[0] is WParagraph && endParagraphIndex > 0)
			{
				(ownerTextBody2.Items[endParagraphIndex] as WParagraph).Items.Add(bkmkEnd);
				DeleteBookmarkTextBody(saveFormatting: false, isReplaceContent);
				return;
			}
		}
		if (isReplaceContent && wTable.Rows.Count - 1 != num2)
		{
			throw new InvalidOperationException("You cannot replace bookmark content when the bookmark starts before the table and ends in table partially");
		}
		DeleteFirstSectionItemsFromDocument(indexInOwnerCollection3, ref endParagraphIndex, ownerTextBody, num3, flag);
		DeleteInBetweenSections(num, indexInOwnerCollection);
		DeleteLastSectionItemsFromDocument(endParagraphIndex - 1, ownerTextBody2, IsFirstBkmkEnd: true, flag);
		if (endParagraphIndex < 0)
		{
			return;
		}
		DeleteTableRows(0, num2, wTable);
		if (wTable.Rows.Count == 0)
		{
			bool num4 = IsBkmkEndInFirstItem(paragraphStart, bkmkStart, paragraphStart.Items.Count - 1);
			WParagraph wParagraph = paragraphStart;
			if (num4)
			{
				wParagraph = GetParagraphToInsertBookmark(ownerTextBody, ownerTextBody2, null, endParagraphIndex, bkmkStart, bkmkEnd, flag, isParaAfterTable: false);
			}
			if (wParagraph == paragraphStart)
			{
				ReplaceCurrentBookmark(wParagraph, num3 - 1, num3, bkmkIndex);
			}
			else
			{
				ReplaceCurrentBookmark(wParagraph, 0, 1, bkmkIndex);
				MoveNestedBookmark(paragraphStart, wParagraph);
				paragraphStart.RemoveSelf();
			}
			wTable.RemoveSelf();
		}
		else
		{
			WParagraph paragraphToInsertBookmark = GetParagraphToInsertBookmark(ownerTextBody, ownerTextBody2, paragraphStart, indexInOwnerCollection3, bkmkStart, bkmkEnd, flag, isParaAfterTable: true);
			if (paragraphToInsertBookmark == paragraphStart)
			{
				ReplaceCurrentBookmark(paragraphToInsertBookmark, num3 - 1, num3, bkmkIndex);
			}
			else
			{
				ReplaceCurrentBookmark(paragraphToInsertBookmark, 0, 1, bkmkIndex);
				paragraphStart.RemoveSelf();
			}
		}
		if (wTable.Rows.Count != 0 && wTable.Rows[0].Cells.Count > 0 && wTable.Rows[0].Cells[0].Paragraphs.Count > 0)
		{
			WParagraph wParagraph2 = wTable.Rows[0].Cells[0].Paragraphs[0];
			ReplaceCurrentBookmark(wParagraph2, 0, 1, bkmkIndex);
			MoveNestedBookmark(paragraphStart, wParagraph2);
			paragraphStart.RemoveSelf();
		}
		if (!flag)
		{
			MergeMultiSectionBodyItems(ownerTextBody, ownerTextBody2);
		}
	}

	private WParagraph GetParagraphToInsertBookmark(WTextBody startTextBody, WTextBody endTextBody, WParagraph paragraph, int bkmkItemIndex, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, bool isInSingleSection, bool isParaAfterTable)
	{
		WTextBody wTextBody = (isInSingleSection ? startTextBody : (isParaAfterTable ? startTextBody : endTextBody));
		if (paragraph != null && !IsBkmkEndInFirstItem(paragraph, bkmkStart, paragraph.ChildEntities.Count - 1))
		{
			return paragraph;
		}
		if (wTextBody.ChildEntities.Count > bkmkItemIndex + 1)
		{
			if (wTextBody.ChildEntities[bkmkItemIndex + 1] is WParagraph)
			{
				return wTextBody.ChildEntities[bkmkItemIndex + 1] as WParagraph;
			}
			if (wTextBody.ChildEntities[bkmkItemIndex + 1] is WTable)
			{
				return (wTextBody.ChildEntities[bkmkItemIndex + 1] as WTable).FirstRow.Cells[0].Paragraphs[0];
			}
		}
		return null;
	}

	private void DeleteTableBookmarkTextBody(WTable bkmkTable, int startRowIndex, int endRowIndex, int startCellIndex, int endCellIndex, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		_ = bkmkStart.Owner;
		_ = bkmkEnd.Owner;
		int columnFirst = bkmkStart.ColumnFirst;
		int columnLast = bkmkStart.ColumnLast;
		bool flag = bkmkStart.ColumnFirst == -1 && bkmkStart.ColumnLast == -1;
		for (int i = startRowIndex; i <= endRowIndex; i++)
		{
			WTableRow wTableRow = bkmkTable.Rows[i];
			if (flag)
			{
				endCellIndex = wTableRow.Cells.Count - 1;
			}
			for (int j = startCellIndex; j <= endCellIndex && j <= wTableRow.Cells.Count - 1; j++)
			{
				WTableCell wTableCell = bkmkTable.Rows[i].Cells[j];
				if (wTableCell.ChildEntities.Count == 0)
				{
					continue;
				}
				WParagraph wParagraph = wTableCell.LastParagraph as WParagraph;
				wParagraph.GetIndexInOwnerCollection();
				while (wTableCell.Paragraphs.Count > 1)
				{
					WParagraph wParagraph2 = wTableCell.Paragraphs[0];
					DeleteParagraphItemsInCell(wParagraph2);
					while (wParagraph2.Items.Count != 0)
					{
						wParagraph.Items.Add(wParagraph2.Items[0]);
					}
					wParagraph2.RemoveSelf();
				}
				_ = wTableCell.Tables.Count;
				while (wTableCell.Tables.Count != 0)
				{
					(wTableCell.Tables[0] as WTable).RemoveSelf();
				}
				DeleteParagraphItemsInCell(wParagraph);
			}
		}
		WTableCell wTableCell2 = bkmkTable.Rows[startRowIndex].Cells[startCellIndex];
		WTableCell wTableCell3 = bkmkTable.Rows[endRowIndex].Cells[endCellIndex];
		CreateBookmark(wTableCell2.LastParagraph, wTableCell3.LastParagraph, bkmkIndex, bkmkStart, columnFirst, columnLast);
		m_currBookmark.BookmarkEnd.IsAfterRowMark = true;
	}

	private void DeleteParagraphItemsInCell(WParagraph paragraph)
	{
		for (int num = paragraph.Items.Count - 1; num >= 0; num--)
		{
			if (!(paragraph.Items[num] is BookmarkStart) && !(paragraph.Items[num] is BookmarkEnd))
			{
				paragraph.Items.RemoveAt(num);
			}
		}
	}

	private void DeleteBookmarkTextBody(bool saveFormatting, bool isReplaceBkmkContent)
	{
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		BookmarkStart bookmarkStart = CurrentBookmark.BookmarkStart;
		BookmarkEnd bookmarkEnd = CurrentBookmark.BookmarkEnd;
		WParagraph ownerParagraph = bookmarkStart.OwnerParagraph;
		WParagraph ownerParagraph2 = bookmarkEnd.OwnerParagraph;
		WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
		WTextBody ownerTextBody2 = ownerParagraph2.OwnerTextBody;
		int indexInOwnerCollection = ownerParagraph.GetIndexInOwnerCollection();
		int bkmkStartNextItemIndex = bookmarkStart.GetIndexInOwnerCollection() + 1;
		int bkmkEndPreviosItemIndex = bookmarkEnd.GetIndexInOwnerCollection() - 1;
		bool flag = false;
		int num = -1;
		int num2 = -1;
		if (ownerParagraph.IsInCell && ownerParagraph2.IsInCell && ownerTextBody == ownerTextBody2)
		{
			flag = true;
			num = (num2 = ownerTextBody.GetIndexInOwnerCollection());
		}
		else
		{
			num = GetSection(ownerTextBody).GetIndexInOwnerCollection() + 1;
			num2 = GetSection(ownerTextBody2).GetIndexInOwnerCollection();
			if (num - 1 == num2)
			{
				flag = true;
			}
		}
		bool flag2 = false;
		bool flag3 = false;
		flag3 = IsBkmkEndInFirstItem(ownerParagraph, bookmarkStart, bkmkStartNextItemIndex);
		flag2 = IsBkmkEndInFirstItem(ownerParagraph2, bookmarkEnd, bkmkEndPreviosItemIndex);
		int endParagraphIndex = ownerParagraph2.GetIndexInOwnerCollection();
		if (saveFormatting && ownerParagraph.Items.Count > bkmkStartNextItemIndex)
		{
			WTextRange wTextRange = null;
			int i;
			for (i = bkmkStartNextItemIndex; i < ownerParagraph.Items.Count - 1 && !(ownerParagraph.Items[i] is WTextRange); i++)
			{
			}
			if (ownerParagraph.Items[i] is WTextRange wTextRange2)
			{
				WTextRange wTextRange3 = new WTextRange(Document);
				wTextRange3.CharacterFormat.ImportContainer(wTextRange2.CharacterFormat);
				if (wTextRange3.CharacterFormat.PropertiesHash.ContainsKey(106))
				{
					wTextRange3.CharacterFormat.PropertiesHash.Remove(106);
				}
				ownerParagraph.ChildEntities.Insert(bkmkStartNextItemIndex, wTextRange3);
				bkmkStartNextItemIndex++;
			}
		}
		if (flag && indexInOwnerCollection == endParagraphIndex)
		{
			while (bkmkStartNextItemIndex < ownerParagraph.ChildEntities.Count && (!(ownerParagraph.ChildEntities[bkmkStartNextItemIndex] is BookmarkEnd) || !((ownerParagraph.ChildEntities[bkmkStartNextItemIndex] as BookmarkEnd).Name == bookmarkEnd.Name)))
			{
				ParagraphItem paragraphItem = ownerParagraph2.Items[bkmkStartNextItemIndex];
				if ((paragraphItem is WField || paragraphItem is WFieldMark) && !(CurrentBookmark.Name.ToLower() == "_fieldbookmark"))
				{
					CheckFieldWithinBookmark(ownerParagraph2, ref bkmkStartNextItemIndex);
				}
				else if (!(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd))
				{
					ownerParagraph2.Items.RemoveAt(bkmkStartNextItemIndex);
				}
				else if (paragraphItem is BookmarkStart && (paragraphItem as BookmarkStart).Name == "_GoBack")
				{
					ownerParagraph2.Items.RemoveAt(bkmkStartNextItemIndex);
				}
				else
				{
					bkmkStartNextItemIndex++;
				}
			}
			if (bookmarkEnd.IsAfterParagraphMark && (flag2 || flag3))
			{
				WParagraph paragraphToInsertBookmark = GetParagraphToInsertBookmark(ownerTextBody, ownerTextBody2, ownerParagraph, indexInOwnerCollection, bookmarkStart, bookmarkEnd, isInSingleSection: true, isParaAfterTable: true);
				if (paragraphToInsertBookmark != null && paragraphToInsertBookmark != ownerParagraph)
				{
					ReplaceCurrentBookmark(paragraphToInsertBookmark, 0, 1, bkmkIndex);
					MoveNestedBookmark(ownerParagraph, paragraphToInsertBookmark);
					ownerParagraph.RemoveSelf();
				}
			}
			List<Bookmark> list = new List<Bookmark>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			Entity entity = CurrentBookmark.BookmarkStart.PreviousSibling as Entity;
			Entity entity2 = CurrentBookmark.BookmarkEnd.NextSibling as Entity;
			if (entity != null)
			{
				while (entity.EntityType == EntityType.BookmarkStart)
				{
					if ((entity as BookmarkStart).Name.IndexOf("_") != 0)
					{
						list2.Add((entity as BookmarkStart).Name);
					}
					entity = entity.PreviousSibling as Entity;
					if (entity == null)
					{
						break;
					}
				}
			}
			if (entity2 != null)
			{
				while (entity2.EntityType == EntityType.BookmarkEnd)
				{
					if ((entity2 as BookmarkEnd).Name.IndexOf("_") != 0)
					{
						list3.Add((entity2 as BookmarkEnd).Name);
					}
					entity2 = entity2.NextSibling as Entity;
					if (entity2 == null)
					{
						break;
					}
				}
			}
			foreach (ParagraphItem item in ownerParagraph2.Items)
			{
				if (!(item is BookmarkStart))
				{
					continue;
				}
				Bookmark bookmark = Document.Bookmarks.FindByName((item as BookmarkStart).Name);
				if (bookmark.BookmarkEnd != null && CurrentBookmark.BookmarkStart != null && CurrentBookmark.BookmarkEnd != null)
				{
					if (bookmark.Name.IndexOf('_') != 0 && item.Index > CurrentBookmark.BookmarkStart.Index && bookmark.BookmarkEnd.OwnerParagraph == CurrentBookmark.BookmarkEnd.OwnerParagraph && bookmark.BookmarkEnd.Index < CurrentBookmark.BookmarkEnd.Index)
					{
						list.Add(bookmark);
					}
					else if (list2.Contains(bookmark.Name) && bookmark.BookmarkEnd.OwnerParagraph == CurrentBookmark.BookmarkEnd.OwnerParagraph && bookmark.BookmarkEnd.Index < CurrentBookmark.BookmarkEnd.Index)
					{
						list.Add(bookmark);
					}
					else if (list3.Contains(bookmark.Name) && bookmark.BookmarkStart.Index > CurrentBookmark.BookmarkStart.Index)
					{
						list.Add(bookmark);
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			{
				foreach (Bookmark item2 in list)
				{
					Document.Bookmarks.Remove(item2);
				}
				return;
			}
		}
		DeleteFirstSectionItemsFromDocument(indexInOwnerCollection, ref endParagraphIndex, ownerTextBody, bkmkStartNextItemIndex, flag);
		if (!flag2 || !flag)
		{
			DeleteLastSectionItemsFromDocument(endParagraphIndex, ownerTextBody2, flag2, flag);
		}
		if (flag2)
		{
			endParagraphIndex--;
		}
		flag2 = ((endParagraphIndex < 0 && flag2 && !flag3) ? true : false);
		if (!RemoveEmptyParagraph)
		{
			return;
		}
		SetCurrentBookmarkPosition(ownerParagraph, ownerParagraph2, ownerTextBody, ownerTextBody2, bookmarkStart, bookmarkEnd, endParagraphIndex, flag2, flag, isReplaceBkmkContent, bkmkIndex);
		if (!flag)
		{
			DeleteInBetweenSections(num, num2);
			if (!flag2)
			{
				MergeMultiSectionBodyItems(ownerTextBody, ownerTextBody2);
			}
		}
	}

	private void CheckFieldWithinBookmark(WParagraph paragraphEnd, ref int bkmkStartNextItemIndex)
	{
		int num = -1;
		int bkmkIndex = Document.Bookmarks.InnerList.IndexOf(CurrentBookmark);
		ParagraphItem paragraphItem = paragraphEnd.Items[bkmkStartNextItemIndex];
		bool flag = CurrentBookmark.BookmarkStart.GetIndexInOwnerCollection() < bkmkStartNextItemIndex;
		int num2 = -1;
		int num3 = -1;
		if (paragraphItem is WField)
		{
			num2 = bkmkStartNextItemIndex;
			num3 = (paragraphItem as WField).FieldEnd.GetIndexInOwnerCollection();
		}
		else if (paragraphItem is WFieldMark)
		{
			num2 = (paragraphItem as WFieldMark).ParentField.GetIndexInOwnerCollection();
			num3 = (((paragraphItem as WFieldMark).Type == FieldMarkType.FieldEnd) ? paragraphItem.GetIndexInOwnerCollection() : (paragraphItem as WFieldMark).ParentField.FieldEnd.GetIndexInOwnerCollection());
		}
		while (num2 < paragraphEnd.Items.Count && num2 <= num3)
		{
			if ((!(paragraphEnd.Items[num2] is BookmarkEnd) || !((paragraphEnd.Items[num2] as BookmarkEnd).Name == CurrentBookmark.Name)) && (!(paragraphEnd.Items[num2] is BookmarkStart) || !((paragraphEnd.Items[num2] as BookmarkStart).Name == CurrentBookmark.Name)))
			{
				int count = paragraphEnd.Items.Count;
				paragraphEnd.Items.RemoveAt(num2);
				num3 -= count - paragraphEnd.Items.Count;
				continue;
			}
			if (paragraphEnd.Items[num2] is BookmarkEnd)
			{
				num = num2;
			}
			num2++;
		}
		if (CurrentBookmark.BookmarkEnd == null)
		{
			int indexInOwnerCollection = CurrentBookmark.BookmarkStart.GetIndexInOwnerCollection();
			CurrentBookmark.BookmarkStart.RemoveSelf();
			ReplaceCurrentBookmark(paragraphEnd, indexInOwnerCollection, flag ? bkmkStartNextItemIndex : (indexInOwnerCollection + 1), bkmkIndex);
		}
		if (CurrentBookmark.BookmarkStart == null || CurrentBookmark.BookmarkStart.IsDetached)
		{
			int indexInOwnerCollection2 = CurrentBookmark.BookmarkEnd.GetIndexInOwnerCollection();
			CurrentBookmark.BookmarkEnd.RemoveSelf();
			ReplaceCurrentBookmark(paragraphEnd, indexInOwnerCollection2, indexInOwnerCollection2 + 1, bkmkIndex);
			bkmkStartNextItemIndex = indexInOwnerCollection2 + 1;
		}
		if (num != -1)
		{
			bkmkStartNextItemIndex = num;
		}
	}

	private void MergeMultiSectionBodyItems(WTextBody startTextBody, WTextBody endTextBody)
	{
		for (int num = startTextBody.ChildEntities.Count; num > 0; num--)
		{
			endTextBody.ChildEntities.Insert(0, startTextBody.ChildEntities[num - 1]);
		}
		Document.Sections.RemoveAt((GetSection(startTextBody) as WSection).GetIndexInOwnerCollection());
	}

	private void DeleteFirstSectionItemsFromDocument(int startParagraphIndex, ref int endParagraphIndex, WTextBody startTextBody, int bkmkStartNextItemIndex, bool isInSingleSection)
	{
		int num = (isInSingleSection ? endParagraphIndex : startTextBody.ChildEntities.Count);
		int num2 = startParagraphIndex;
		for (int i = startParagraphIndex; i < num; i++)
		{
			if (startParagraphIndex == i)
			{
				if (bkmkStartNextItemIndex > 0)
				{
					WParagraph wParagraph = startTextBody.ChildEntities[i] as WParagraph;
					while (wParagraph.ChildEntities.Count > bkmkStartNextItemIndex)
					{
						if (wParagraph.Items[bkmkStartNextItemIndex] is BookmarkEnd)
						{
							BookmarkEnd bookmarkEnd = wParagraph.Items[bkmkStartNextItemIndex] as BookmarkEnd;
							if (bookmarkEnd.Name != "_GoBack" && bookmarkEnd.Name != CurrentBookmark.Name)
							{
								Bookmark bookmark = Document.Bookmarks.FindByName(bookmarkEnd.Name);
								if (IsDeleteBkmk(bookmark, startParagraphIndex))
								{
									wParagraph.ChildEntities.RemoveAt(bookmark.BookmarkStart.Index);
									bkmkStartNextItemIndex--;
								}
								else if (bookmark != null && bookmark.BookmarkStart != null && (!bookmark.BookmarkStart.IsDetached || num2 > GetOwnerEntity(bookmark.BookmarkStart).Index || endParagraphIndex < GetOwnerEntity(bookmark.BookmarkEnd).Index))
								{
									bkmkStartNextItemIndex++;
									continue;
								}
							}
						}
						else if (wParagraph.Items[bkmkStartNextItemIndex] is BookmarkStart)
						{
							BookmarkStart bookmarkStart = wParagraph.Items[bkmkStartNextItemIndex] as BookmarkStart;
							if (bookmarkStart.Name != "_GoBack" && bookmarkStart.Name != CurrentBookmark.Name)
							{
								Bookmark bookmark2 = Document.Bookmarks.FindByName(bookmarkStart.Name);
								if (bookmark2 != null && bookmark2.BookmarkStart != null && (num2 > GetOwnerEntity(bookmark2.BookmarkStart).Index || endParagraphIndex < GetOwnerEntity(bookmark2.BookmarkEnd).Index))
								{
									bkmkStartNextItemIndex++;
									continue;
								}
							}
						}
						else if (!(CurrentBookmark.Name.ToLower() == "_fieldbookmark") && wParagraph.Items[bkmkStartNextItemIndex] is WField && (wParagraph.Items[bkmkStartNextItemIndex] as WField).FieldEnd.OwnerParagraph != null && wParagraph != (wParagraph.Items[bkmkStartNextItemIndex] as WField).FieldEnd.OwnerParagraph)
						{
							throw new InvalidOperationException("Bookmark content not replaced properly while replacing with another bookmark content");
						}
						wParagraph.ChildEntities.RemoveAt(bkmkStartNextItemIndex);
					}
					startParagraphIndex++;
					bkmkStartNextItemIndex = -1;
					continue;
				}
				if (startTextBody.ChildEntities[startParagraphIndex] is WParagraph)
				{
					DeleteBkmkFromParagraph(startTextBody.ChildEntities[startParagraphIndex] as WParagraph, num2, num);
					if ((startTextBody.ChildEntities[startParagraphIndex] as WParagraph).ChildEntities.Count == 0)
					{
						startTextBody.ChildEntities.RemoveAt(startParagraphIndex);
					}
					else
					{
						startParagraphIndex++;
					}
				}
				else
				{
					Stack<Entity> stack = new Stack<Entity>();
					UpdateBookmark(startTextBody.ChildEntities[startParagraphIndex] as TextBodyItem, stack);
					if (stack.Count != 0)
					{
						WParagraph wParagraph2 = new WParagraph(Document);
						int num3 = 0;
						while (stack.Count > num3)
						{
							Entity entity = stack.Pop();
							if (!IsBkMkInsideCurrBkMkRegion(entity, num2, endParagraphIndex))
							{
								wParagraph2.Items.Add(entity);
							}
							else
							{
								num3++;
							}
						}
						if (wParagraph2.ChildEntities.Count > 0)
						{
							startTextBody.ChildEntities.Insert(startParagraphIndex, wParagraph2);
							startParagraphIndex++;
						}
					}
					startTextBody.ChildEntities.RemoveAt(startParagraphIndex);
				}
				if (isInSingleSection)
				{
					endParagraphIndex--;
				}
				continue;
			}
			if (startTextBody.ChildEntities.Count > startParagraphIndex)
			{
				if (startTextBody.ChildEntities[startParagraphIndex] is WParagraph)
				{
					DeleteBkmkFromParagraph(startTextBody.ChildEntities[startParagraphIndex] as WParagraph, num2, num);
					if ((startTextBody.ChildEntities[startParagraphIndex] as WParagraph).ChildEntities.Count == 0)
					{
						startTextBody.ChildEntities.RemoveAt(startParagraphIndex);
					}
					else
					{
						startParagraphIndex++;
					}
				}
				else
				{
					Stack<Entity> stack2 = new Stack<Entity>();
					UpdateBookmark(startTextBody.ChildEntities[startParagraphIndex] as TextBodyItem, stack2);
					if (stack2.Count != 0)
					{
						WParagraph wParagraph3 = new WParagraph(Document);
						int num4 = 0;
						while (stack2.Count > num4)
						{
							Entity entity2 = stack2.Pop();
							if (!IsBkMkInsideCurrBkMkRegion(entity2, num2, endParagraphIndex))
							{
								wParagraph3.Items.Add(entity2);
							}
							else
							{
								num4++;
							}
						}
						if (wParagraph3.ChildEntities.Count > 0)
						{
							startTextBody.ChildEntities.Insert(startParagraphIndex, wParagraph3);
							startParagraphIndex++;
						}
					}
					startTextBody.ChildEntities.RemoveAt(startParagraphIndex);
				}
			}
			if (isInSingleSection)
			{
				endParagraphIndex--;
			}
		}
	}

	private bool IsDeleteBkmk(Bookmark bkmkStart, int startParagraphIndex)
	{
		bool result = false;
		if (bkmkStart != null && bkmkStart.BookmarkStart != null && bkmkStart.BookmarkStart.OwnerParagraph.Index == startParagraphIndex)
		{
			for (int i = bkmkStart.BookmarkStart.Index + 1; i <= CurrentBookmark.BookmarkStart.Index; i++)
			{
				if (bkmkStart.BookmarkStart.OwnerParagraph.Items[i] is BookmarkStart || bkmkStart.BookmarkStart.OwnerParagraph.Items[i] is BookmarkEnd)
				{
					result = true;
					continue;
				}
				result = false;
				break;
			}
		}
		return result;
	}

	private bool IsBkMkInsideCurrBkMkRegion(Entity item, int startParaIndex, int endParaIndex)
	{
		bool result = true;
		if (item is BookmarkEnd)
		{
			BookmarkEnd bookmarkEnd = item as BookmarkEnd;
			if (bookmarkEnd.Name != "_GoBack" && bookmarkEnd.Name != CurrentBookmark.Name)
			{
				Bookmark bookmark = Document.Bookmarks.FindByName(bookmarkEnd.Name);
				if (bookmark != null && bookmark.BookmarkStart != null)
				{
					result = ((startParaIndex <= GetOwnerEntity(bookmark.BookmarkStart).Index && endParaIndex >= GetOwnerEntity(bookmark.BookmarkEnd).Index) ? true : false);
				}
			}
		}
		else if (item is BookmarkStart)
		{
			BookmarkStart bookmarkStart = item as BookmarkStart;
			if (bookmarkStart.Name != "_GoBack" && bookmarkStart.Name != CurrentBookmark.Name)
			{
				Bookmark bookmark2 = Document.Bookmarks.FindByName(bookmarkStart.Name);
				if (bookmark2 != null && bookmark2.BookmarkStart != null)
				{
					result = ((startParaIndex <= GetOwnerEntity(bookmark2.BookmarkStart).Index && endParaIndex >= GetOwnerEntity(bookmark2.BookmarkEnd).Index) ? true : false);
				}
			}
		}
		return result;
	}

	private WParagraph DeleteBkmkFromParagraph(WParagraph paragraph, int startBkmkIndex, int endBkmkIndex)
	{
		int num = 0;
		while (paragraph.ChildEntities.Count > num)
		{
			if (IsBkMkInsideCurrBkMkRegion(paragraph.ChildEntities[num], startBkmkIndex, endBkmkIndex))
			{
				paragraph.ChildEntities.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		return paragraph;
	}

	private void DeleteLastSectionItemsFromDocument(int endParagraphIndex, WTextBody endTextBody, bool IsFirstBkmkEnd, bool isInSingleSection)
	{
		if (!isInSingleSection)
		{
			DeleteFirstSectionItemsFromDocument(0, ref endParagraphIndex, endTextBody, 0, isInSingleSection: true);
		}
		if (!IsFirstBkmkEnd)
		{
			WParagraph paragraphEnd = endTextBody.ChildEntities[endParagraphIndex] as WParagraph;
			DeletePreviousItemsInOwnerParagraphgrah(paragraphEnd, CurrentBookmark.BookmarkEnd);
		}
	}

	private Entity GetSection(Entity entity)
	{
		while (!(entity is WSection))
		{
			entity = entity.Owner;
		}
		return entity;
	}

	internal bool IsBkmkEndInFirstItem(WParagraph paragraph, ParagraphItem bkmkEnd, int bkmkEndPreviosItemIndex)
	{
		for (int i = 0; i <= bkmkEndPreviosItemIndex; i++)
		{
			if (bkmkEndPreviosItemIndex >= paragraph.ChildEntities.Count)
			{
				break;
			}
			if ((!(paragraph.Items[i] is BookmarkStart) || (bkmkEnd is BookmarkEnd && !((paragraph.Items[i] as BookmarkStart).Name != (bkmkEnd as BookmarkEnd).Name))) && !(paragraph.Items[i] is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsBkmkEndFirstItemInTable(int bkmkEndRowIndex, WTable bkmkEndTable, WParagraph paragraphEnd, BookmarkEnd bkmkEnd)
	{
		if (bkmkEndRowIndex > 0)
		{
			return false;
		}
		if (bkmkEnd.GetIndexInOwnerCollection() > 0)
		{
			return false;
		}
		if ((paragraphEnd.GetOwnerEntity() as WTableCell).GetIndexInOwnerCollection() > 0)
		{
			return false;
		}
		return true;
	}

	private void SetCurrentBookmarkPosition(WParagraph paragraphStart, WParagraph paragraphEnd, WTextBody textBodyStart, WTextBody textBodyEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, int startParagraphNextIndex, bool isFirstItemBkmkEnd, bool isInSingleSection, bool isReplaceOperation, int bkmkIndex)
	{
		bool flag = IsBkmkEndInFirstItem(paragraphStart, bkmkStart, paragraphStart.ChildEntities.Count - 1);
		bool flag2 = IsBkmkEndInFirstItem(paragraphEnd, bkmkEnd, paragraphEnd.ChildEntities.Count - 1);
		if (isFirstItemBkmkEnd && !flag)
		{
			return;
		}
		if (!flag && !flag2)
		{
			if (paragraphEnd.PreviousSibling != paragraphStart || bkmkEnd.HasRenderableItemBefore())
			{
				return;
			}
			WParagraph paragraphToInsertBookmark = GetParagraphToInsertBookmark(textBodyStart, textBodyEnd, paragraphEnd, startParagraphNextIndex, bkmkStart, bkmkEnd, isInSingleSection, isParaAfterTable: false);
			if (paragraphToInsertBookmark != null)
			{
				int num = bkmkStart.Index + 1;
				WTextRange wTextRange = ((paragraphStart.ChildEntities.Count > num) ? (paragraphStart.ChildEntities[num] as WTextRange) : null);
				int num2 = ((!(bkmkEnd.PreviousSibling is BookmarkStart) && !(bkmkEnd.PreviousSibling is BookmarkEnd)) ? 1 : (bkmkEnd.Index + 1));
				ReplaceCurrentBookmark(paragraphToInsertBookmark, 0, num2, bkmkIndex);
				if (wTextRange != null)
				{
					paragraphToInsertBookmark.Items.Insert(num2, wTextRange);
				}
				MoveNestedBookmark(paragraphStart, paragraphToInsertBookmark);
				paragraphStart.RemoveSelf();
				if (paragraphToInsertBookmark != paragraphEnd)
				{
					paragraphEnd.RemoveSelf();
				}
			}
		}
		else if (flag && !flag2)
		{
			ReplaceCurrentBookmark(paragraphEnd, 0, 1, bkmkIndex);
			MoveNestedBookmark(paragraphStart, paragraphEnd);
			paragraphStart.RemoveSelf();
		}
		else if (!flag && flag2)
		{
			while (paragraphEnd.Items.Count != 0)
			{
				paragraphStart.UpdateBookmarkEnd(paragraphEnd.Items[0], paragraphStart, isAddItem: true);
			}
			paragraphEnd.RemoveSelf();
		}
		else
		{
			if (!(flag && flag2))
			{
				return;
			}
			if (!isReplaceOperation)
			{
				WParagraph paragraphToInsertBookmark2 = GetParagraphToInsertBookmark(textBodyStart, textBodyEnd, paragraphEnd, startParagraphNextIndex, bkmkStart, bkmkEnd, isInSingleSection, isParaAfterTable: false);
				if (paragraphToInsertBookmark2 != null)
				{
					ReplaceCurrentBookmark(paragraphToInsertBookmark2, 0, 1, bkmkIndex);
					MoveNestedBookmark(paragraphStart, paragraphToInsertBookmark2);
					paragraphStart.RemoveSelf();
					if (paragraphToInsertBookmark2 != paragraphEnd)
					{
						paragraphEnd.RemoveSelf();
					}
				}
			}
			else
			{
				while (paragraphEnd.Items.Count != 0)
				{
					paragraphStart.UpdateBookmarkEnd(paragraphEnd.Items[0], paragraphStart, isAddItem: true);
				}
				paragraphEnd.RemoveSelf();
			}
		}
	}

	private void SetCurrentBookmarkPosition(WTable bkmkStartTable, WTable bkmkEndTable, WTextBody startTableTextBody, WTextBody endTableTextBody, int bkmkIndex)
	{
		if (bkmkEndTable.Rows.Count != 0 && bkmkEndTable.Rows[0].Cells.Count > 0)
		{
			WTableCell wTableCell = bkmkEndTable.Rows[0].Cells[0];
			if (wTableCell.ChildEntities.Count != 0)
			{
				WParagraph paragraph = wTableCell.Paragraphs[0];
				ReplaceCurrentBookmark(paragraph, 0, 1, bkmkIndex);
			}
			else
			{
				WParagraph paragraph2 = wTableCell.AddParagraph() as WParagraph;
				ReplaceCurrentBookmark(paragraph2, 0, 1, bkmkIndex);
			}
		}
		else
		{
			WParagraph wParagraph = null;
			wParagraph = ((startTableTextBody != endTableTextBody) ? GetParagraphToInsertBookmark(startTableTextBody, endTableTextBody, null, bkmkEndTable.GetIndexInOwnerCollection(), null, null, isInSingleSection: false, isParaAfterTable: false) : GetParagraphToInsertBookmark(startTableTextBody, endTableTextBody, null, bkmkEndTable.GetIndexInOwnerCollection(), null, null, isInSingleSection: true, isParaAfterTable: false));
			if (wParagraph != null)
			{
				ReplaceCurrentBookmark(wParagraph, 0, 1, bkmkIndex);
			}
		}
	}

	private void DeletePreviousItemsInOwnerParagraphgrah(WParagraph paragraphEnd, BookmarkEnd bkmkEnd)
	{
		int num = 0;
		while (paragraphEnd.Items.Count > 0 && num < paragraphEnd.Items.Count && paragraphEnd.Items[num] != bkmkEnd)
		{
			if (!(paragraphEnd.Items[num] is BookmarkStart) && !(paragraphEnd.Items[num] is BookmarkEnd))
			{
				paragraphEnd.Items.RemoveAt(num);
			}
			else if (paragraphEnd.Items[num] is BookmarkStart && (paragraphEnd.Items[num] as BookmarkStart).Name == "_GoBack")
			{
				paragraphEnd.Items.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private void MoveNestedBookmark(WParagraph sourceParagraph, WParagraph destinationParagrah)
	{
		if (sourceParagraph.ChildEntities.Count != 0)
		{
			while (sourceParagraph.Items.Count != 0)
			{
				destinationParagrah.UpdateBookmarkEnd(sourceParagraph.LastItem, destinationParagrah, isAddItem: false);
			}
		}
	}

	private void ReplaceCurrentBookmark(WParagraph paragraph, int bkmkStartIndex, int bkmkEndIndex, int bkmkIndex)
	{
		string name = CurrentBookmark.Name;
		if (Document.Bookmarks.FindByName(name) != null)
		{
			Document.Bookmarks.Remove(CurrentBookmark);
		}
		BookmarkStart bookmarkStart = new BookmarkStart(Document, name);
		paragraph.Items.Insert(bkmkStartIndex, bookmarkStart);
		BookmarkEnd bookmarkEnd = new BookmarkEnd(Document, name);
		paragraph.Items.Insert(bkmkEndIndex, bookmarkEnd);
		CurrentBookmark.SetStart(bookmarkStart);
		CurrentBookmark.SetEnd(bookmarkEnd);
		if (bkmkIndex < Document.Bookmarks.Count)
		{
			Document.Bookmarks.InnerList.Insert(bkmkIndex, CurrentBookmark);
			Document.Bookmarks.InnerList.RemoveAt(Document.Bookmarks.Count - 1);
		}
		m_currBookmark = Document.Bookmarks.FindByName(name);
	}

	private void ReplaceTableBookmarkContent(WordDocument document, WordDocumentPart documentPart, TextBodyPart textPart)
	{
		if ((document != null && document.Sections.Count > 1) || (documentPart != null && documentPart.Sections.Count > 1))
		{
			throw new InvalidOperationException("You cannot replace bookmark content with multiple sections when bookmark starts and ends within the same table");
		}
		if ((document == null || document.Sections.Count != 1) && (documentPart == null || documentPart.Sections.Count != 1) && textPart == null)
		{
			return;
		}
		WTableCell obj = CurrentBookmark.BookmarkStart.OwnerParagraph.GetOwnerEntity() as WTableCell;
		WTableCell wTableCell = CurrentBookmark.BookmarkEnd.OwnerParagraph.GetOwnerEntity() as WTableCell;
		WTable ownerTable = obj.OwnerRow.OwnerTable;
		int indexInOwnerCollection = obj.GetIndexInOwnerCollection();
		int num = wTableCell.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = obj.OwnerRow.GetIndexInOwnerCollection();
		int indexInOwnerCollection3 = wTableCell.OwnerRow.GetIndexInOwnerCollection();
		if (indexInOwnerCollection2 == indexInOwnerCollection3 && indexInOwnerCollection == num)
		{
			if (document != null)
			{
				ReplaceParagraphBookmarkContent(document.Sections);
			}
			else if (documentPart != null)
			{
				ReplaceParagraphBookmarkContent(documentPart.Sections);
			}
			else
			{
				InsertTextBodyPart(textPart);
			}
			return;
		}
		WTextBody wTextBody = null;
		wTextBody = ((document != null) ? document.Sections[0].Body : ((documentPart == null) ? textPart.m_textPart : documentPart.Sections[0].Body));
		bool flag = CurrentBookmark.BookmarkStart.ColumnFirst == -1 && CurrentBookmark.BookmarkStart.ColumnLast == -1;
		for (int i = indexInOwnerCollection2; i <= indexInOwnerCollection3; i++)
		{
			if (flag)
			{
				num = ownerTable.Rows[i].Cells.Count - 1;
			}
			for (int j = indexInOwnerCollection; j <= num; j++)
			{
				ownerTable.Rows[i].Cells[j].Items.Clear();
				wTextBody.Items.CloneTo(ownerTable.Rows[i].Cells[j].Items);
				if (i == indexInOwnerCollection2 && j == indexInOwnerCollection)
				{
					if (ownerTable.Rows[i].Cells[j].Paragraphs.Count != 0)
					{
						ownerTable.Rows[i].Cells[j].Paragraphs[0].Items.Insert(0, CurrentBookmark.BookmarkStart);
					}
					else
					{
						ownerTable.Rows[i].Cells[j].AddParagraph().Items.Add(CurrentBookmark.BookmarkStart);
					}
				}
				if (i == indexInOwnerCollection3 && j == num)
				{
					if (ownerTable.Rows[i].Cells[j].Paragraphs.Count != 0)
					{
						ownerTable.Rows[i].Cells[j].LastParagraph.Items.Add(CurrentBookmark.BookmarkEnd);
					}
					else
					{
						ownerTable.Rows[i].Cells[j].AddParagraph().Items.Add(CurrentBookmark.BookmarkEnd);
					}
				}
			}
		}
	}

	private void CheckCurrentState()
	{
		if (m_document == null)
		{
			throw new InvalidOperationException("You can not use DocumentNavigator without initializing Document property");
		}
		if (m_currBookmark == null || m_currParagraph == null || m_currParagraphItemIndex < 0)
		{
			throw new InvalidOperationException("Current Bookmark didn't select");
		}
	}

	private IWTextRange InsertText(string text, bool saveFormatting, bool isReplaceContent)
	{
		CheckCurrentState();
		IWTextRange iWTextRange = null;
		if (saveFormatting)
		{
			iWTextRange = ((!IsStart) ? (m_currBookmark.BookmarkEnd.PreviousSibling as IWTextRange) : (m_currBookmark.BookmarkStart.PreviousSibling as IWTextRange));
		}
		IWTextRange iWTextRange2 = InsertParagraphItem(ParagraphItemType.TextRange) as IWTextRange;
		iWTextRange2.Text = text;
		if (saveFormatting)
		{
			WCharacterFormat wCharacterFormat = null;
			if (iWTextRange != null)
			{
				wCharacterFormat = iWTextRange.CharacterFormat;
				iWTextRange2.CharacterFormat.ImportContainer(wCharacterFormat);
				if (isReplaceContent)
				{
					iWTextRange2.OwnerParagraph.ChildEntities.Remove(iWTextRange);
				}
			}
			else
			{
				ApplyParagraphFormatting(iWTextRange2);
			}
		}
		return iWTextRange2;
	}

	private void ApplyParagraphFormatting(IWTextRange textRange)
	{
		if (m_currBookmark.BookmarkStart.Owner is WParagraph wParagraph)
		{
			textRange.CharacterFormat.ImportContainer(wParagraph.BreakCharacterFormat);
		}
	}

	private void InsertBodyItem(TextBodyItem item)
	{
		if (CurrentBookmarkItem == null)
		{
			return;
		}
		WParagraph ownerParagraph = m_currBookmarkItem.OwnerParagraph;
		int num = ownerParagraph.GetIndexInOwnerCollection();
		WParagraph wParagraph = new WParagraph(ownerParagraph.Document);
		if (CurrentParagraphItemIndex != 0)
		{
			if (m_currParagraphItemIndex < ownerParagraph.Items.Count)
			{
				TextBodyPart.SplitParagraph(ownerParagraph, m_currParagraphItemIndex, new WParagraph(ownerParagraph.Document));
			}
			else if ((item is WTable && ownerParagraph.NextSibling is WParagraph) || item is WParagraph)
			{
				ownerParagraph.OwnerTextBody.Items.Insert(num + 1, wParagraph);
				wParagraph.BreakCharacterFormat.ImportContainer(ownerParagraph.BreakCharacterFormat);
				wParagraph.ParagraphFormat.ImportContainer(ownerParagraph.ParagraphFormat);
			}
			num++;
			if (item is WParagraph)
			{
				WParagraph wParagraph2 = item as WParagraph;
				while (wParagraph2.Items.Count > 0)
				{
					ownerParagraph.Items.Add(wParagraph2.Items[0]);
				}
				ownerParagraph.ParagraphFormat.ImportContainer(wParagraph2.ParagraphFormat);
				ownerParagraph.BreakCharacterFormat.ImportContainer(wParagraph2.BreakCharacterFormat);
				ownerParagraph.ListFormat.ImportContainer(wParagraph2.ListFormat);
			}
		}
		if (item is WTable || m_currParagraphItemIndex <= 0)
		{
			ownerParagraph.OwnerTextBody.Items.Insert(num, item);
		}
	}
}
