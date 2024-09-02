using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class TextBodyPart
{
	internal WTextBody m_textPart;

	private WTextBody m_body;

	private int m_itemIndex;

	private int m_pItemIndex;

	private WCharacterFormat m_srcFormat;

	private bool m_saveFormatting;

	public BodyItemCollection BodyItems
	{
		get
		{
			if (m_textPart == null)
			{
				return null;
			}
			return m_textPart.Items;
		}
	}

	public TextBodyPart()
	{
	}

	public TextBodyPart(TextBodySelection textBodySelection)
	{
		Copy(textBodySelection);
	}

	public TextBodyPart(TextSelection textSelection)
	{
		Copy(textSelection);
	}

	public TextBodyPart(WordDocument doc)
	{
		EnsureTextBody(doc);
	}

	public void Clear()
	{
		m_textPart.Items.Clear();
	}

	public void Copy(TextSelection textSel)
	{
		WParagraph wParagraph = ((textSel.OwnerParagraph == null && (textSel.StartTextRange.Owner is InlineContentControl || textSel.StartTextRange.Owner is Break)) ? textSel.StartTextRange.GetOwnerParagraphValue() : textSel.OwnerParagraph);
		EnsureTextBody(wParagraph.Document);
		WTextRange[] ranges = textSel.GetRanges();
		WParagraph wParagraph2 = new WParagraph(m_textPart.Document);
		m_textPart.Items.Add(wParagraph2);
		int i = 0;
		for (int num = ranges.Length; i < num; i++)
		{
			wParagraph2.Items.Add(ranges[i].Clone());
		}
	}

	private void Copy(TextBodySelection textSel, bool isFindField)
	{
		EnsureTextBody(textSel.TextBody.Document);
		int itemStartIndex = textSel.ItemStartIndex;
		int itemEndIndex = textSel.ItemEndIndex;
		for (int i = itemStartIndex; i <= itemEndIndex; i++)
		{
			TextBodyItem textBodyItem = (TextBodyItem)textSel.TextBody.Items[i].Clone();
			if ((i == itemStartIndex || i == itemEndIndex) && textBodyItem.EntityType == EntityType.Paragraph)
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				if (i == itemEndIndex)
				{
					int num = textSel.ParagraphItemEndIndex + 1;
					if (isFindField)
					{
						for (int j = ((itemStartIndex == itemEndIndex) ? textSel.ParagraphItemStartIndex : 0); j < num; j++)
						{
							ParagraphItem paragraphItem = wParagraph.Items[j];
							if (paragraphItem is WField)
							{
								WField wField = paragraphItem as WField;
								if (wField.FieldEnd.Index > num && wField.FieldEnd.OwnerParagraph.Equals(wParagraph))
								{
									wParagraph.Items.RemoveFromInnerList(num);
									num = wField.FieldEnd.Index + 1;
									break;
								}
							}
						}
					}
					while (num < wParagraph.Items.Count)
					{
						if (wParagraph.Document.IsComparing)
						{
							wParagraph.Items.RemoveAt(wParagraph.Items.Count - 1);
						}
						else
						{
							wParagraph.Items.RemoveFromInnerList(wParagraph.Items.Count - 1);
						}
					}
				}
				if (i == itemStartIndex)
				{
					int num2 = textSel.ParagraphItemStartIndex;
					if (isFindField)
					{
						int num3 = ((itemStartIndex == itemEndIndex) ? (textSel.ParagraphItemEndIndex + 1) : wParagraph.Items.Count);
						for (int k = num2; k < num3; k++)
						{
							ParagraphItem paragraphItem2 = wParagraph.Items[k];
							WField wField2 = ((paragraphItem2 is WField) ? (paragraphItem2 as WField) : null);
							if (paragraphItem2 is WFieldMark && (paragraphItem2 as WFieldMark).Type == FieldMarkType.FieldEnd && (paragraphItem2 as WFieldMark).ParentField != null && (paragraphItem2 as WFieldMark).ParentField != wField2 && (paragraphItem2 as WFieldMark).ParentField.Index != -1 && (paragraphItem2 as WFieldMark).ParentField.Index < num2 - 1 && (paragraphItem2 as WFieldMark).ParentField.OwnerParagraph.Equals(wParagraph))
							{
								wParagraph.Items.RemoveFromInnerList(num2 - 1);
								num2 = (paragraphItem2 as WFieldMark).ParentField.Index;
								break;
							}
						}
					}
					while (num2 > 0 && wParagraph.Items.Count > 0)
					{
						if (wParagraph.Document.IsComparing)
						{
							if (wParagraph.Items[0] is WField && (wParagraph.Items[0] as WField).FieldEnd.OwnerParagraph == wParagraph)
							{
								ParagraphItem paragraphItem3 = wParagraph.Items[num2];
								wParagraph.Items.RemoveAt(0);
								num2 = paragraphItem3.Index + 1;
							}
							else
							{
								wParagraph.Items.RemoveAt(0);
							}
						}
						else
						{
							wParagraph.Items.RemoveFromInnerList(0);
						}
						num2--;
					}
				}
			}
			m_textPart.Items.Add(textBodyItem);
		}
	}

	public void Copy(TextBodySelection textSel)
	{
		Copy(textSel, isFindField: false);
	}

	public void Copy(TextBodyItem bodyItem, bool clone)
	{
		if (clone)
		{
			bodyItem = (TextBodyItem)bodyItem.Clone();
		}
		EnsureTextBody(bodyItem.Document);
		m_textPart.Items.Add(bodyItem);
	}

	public void Copy(ParagraphItem pItem, bool clone)
	{
		if (clone)
		{
			pItem = (ParagraphItem)pItem.Clone();
		}
		EnsureTextBody(pItem.Document);
		m_textPart.AddParagraph().Items.Add(pItem);
	}

	public WordDocument GetAsWordDocument()
	{
		WordDocument wordDocument = new WordDocument();
		IWSection iWSection = wordDocument.AddSection();
		foreach (TextBodyItem item in m_textPart.Items)
		{
			iWSection.Body.Items.Add(item.Clone());
		}
		return wordDocument;
	}

	public void Close()
	{
		if (m_textPart.Items != null && m_textPart.Items.Count > 0)
		{
			for (int i = 0; i < m_textPart.Items.Count; i++)
			{
				m_textPart.Items[i].Close();
			}
			m_textPart.Items.Clear();
			m_textPart = null;
		}
	}

	internal void Copy(WTextBody textBody, bool clone)
	{
		EnsureTextBody(textBody.Document);
		if (clone)
		{
			m_textPart = (WTextBody)textBody.Clone();
		}
		else
		{
			m_textPart = textBody;
		}
	}

	public void PasteAfter(TextBodyItem bodyItem)
	{
		int indexInOwnerCollection = bodyItem.GetIndexInOwnerCollection();
		PasteAt(bodyItem.OwnerTextBody, indexInOwnerCollection + 1);
	}

	public void PasteAfter(ParagraphItem paragraphItem)
	{
		TextBodyItem textBodyItem = paragraphItem.Owner as TextBodyItem;
		int indexInOwnerCollection = textBodyItem.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = paragraphItem.GetIndexInOwnerCollection();
		PasteAt(textBodyItem.OwnerTextBody, indexInOwnerCollection, indexInOwnerCollection2 + 1);
	}

	public void PasteAt(ITextBody textBody, int itemIndex)
	{
		PasteAt(textBody, itemIndex, 0);
	}

	internal void PasteAt(ITextBody textBody, int itemIndex, int pItemIndex, WCharacterFormat srcFormat, bool saveFormatting)
	{
		bool importStyles = false;
		if (saveFormatting && textBody != null && textBody.Document != null)
		{
			importStyles = textBody.Document.ImportStyles;
			textBody.Document.ImportStyles = false;
		}
		m_srcFormat = srcFormat;
		m_saveFormatting = saveFormatting;
		PasteAt(textBody, itemIndex, pItemIndex);
		if (saveFormatting && textBody != null && textBody.Document != null)
		{
			textBody.Document.ImportStyles = importStyles;
		}
	}

	public void PasteAt(ITextBody textBody, int itemIndex, int pItemIndex)
	{
		PasteAt(textBody, itemIndex, pItemIndex, isBkmkReplace: false);
	}

	public void PasteAtEnd(ITextBody textBody)
	{
		PasteAt(textBody, ((WTextBody)textBody).Items.Count);
	}

	internal void PasteAt(ITextBody textBody, int itemIndex, int pItemIndex, bool isBkmkReplace)
	{
		if (m_textPart.Items.Count == 0)
		{
			return;
		}
		m_body = textBody as WTextBody;
		m_itemIndex = itemIndex;
		m_pItemIndex = pItemIndex;
		ValidateArgs();
		WParagraph wParagraph = m_textPart.Items[0] as WParagraph;
		WParagraph wParagraph2 = m_textPart.Items[m_textPart.Count - 1] as WParagraph;
		WParagraph wParagraph3 = ((itemIndex < m_body.Items.Count) ? (m_body.Items[itemIndex] as WParagraph) : null);
		string text = null;
		if (isBkmkReplace && wParagraph3 != null && wParagraph3.ChildEntities.Count > pItemIndex && wParagraph3.ChildEntities[pItemIndex] is BookmarkEnd)
		{
			text = (wParagraph3.ChildEntities[pItemIndex] as BookmarkEnd).Name;
		}
		WParagraph wParagraph4 = null;
		int num = 0;
		int num2 = 0;
		int num3 = m_textPart.Items.Count - 1;
		bool num4 = num3 > 0 || wParagraph == null;
		int count = m_body.Document.ClonedFields.Count;
		if (num4 && wParagraph3 != null && m_pItemIndex >= 0)
		{
			wParagraph4 = SplitParagraph(wParagraph3, wParagraph2);
			if (wParagraph3.Items.Count > 0 || wParagraph3.IsAppendingHTML)
			{
				num = 1;
			}
			if (m_body.IsPerformingFindAndReplace && wParagraph3.Items.Count == 0 && wParagraph == null)
			{
				wParagraph3.RemoveSelf();
			}
			if (wParagraph2 != null)
			{
				num3--;
			}
		}
		if (wParagraph != null && wParagraph3 != null && m_saveFormatting)
		{
			int num5 = wParagraph3.Items.Count - m_pItemIndex;
			int index = wParagraph3.Items.Count - num5;
			int i = 0;
			for (int count2 = wParagraph.Items.Count; i < count2; i++)
			{
				Entity entity = wParagraph.Items[i].Clone();
				wParagraph3.Items.Insert(index, entity);
				wParagraph3.ListFormat.ImportListFormat(wParagraph.ListFormat);
				if (entity is WTextRange && m_saveFormatting && m_srcFormat != null)
				{
					m_srcFormat.UpdateSourceFormatting((entity as WTextRange).CharacterFormat);
				}
				index = wParagraph3.Items.Count - num5;
			}
			num2 = 1;
			num = 1;
		}
		else if (wParagraph != null && wParagraph3 != null && !m_saveFormatting)
		{
			wParagraph3.ImportStyle(wParagraph.ParaStyle);
			wParagraph.ParagraphFormat.UpdateSourceFormatting(wParagraph3.ParagraphFormat);
			if (!wParagraph3.Document.IsComparing || !wParagraph3.ListFormat.Compare(wParagraph.ListFormat))
			{
				wParagraph3.ListFormat.ImportListFormat(wParagraph.ListFormat);
			}
			wParagraph.BreakCharacterFormat.UpdateSourceFormatting(wParagraph3.BreakCharacterFormat);
			CopyParagraphItems(wParagraph, wParagraph3);
			if (wParagraph3.Document.IsComparing && wParagraph.BreakCharacterFormat.m_revisions != null && wParagraph3.Document.Bookmarks.FindByName(text).BookmarkEnd.OwnerParagraph != wParagraph3)
			{
				if (wParagraph.ParagraphFormat.Tabs != null && wParagraph.ParagraphFormat.Tabs.Count > 0 && wParagraph3.Document.m_matchBodyItemIndex != wParagraph3.Index)
				{
					wParagraph.ParagraphFormat.Tabs.UpdateTabs(wParagraph3.ParagraphFormat.Tabs);
				}
				wParagraph3.BreakCharacterFormat.m_clonedRevisions = new List<Revision>();
				foreach (Revision revision in wParagraph.BreakCharacterFormat.m_revisions)
				{
					wParagraph3.BreakCharacterFormat.m_clonedRevisions.Add(revision.Clone());
				}
				wParagraph3.BreakCharacterFormat.IsInsertRevision = true;
				wParagraph3.BreakCharacterFormat.AuthorName = m_body.Document.m_authorName;
				wParagraph3.BreakCharacterFormat.RevDateTime = m_body.Document.m_dateTime;
				wParagraph3.UpdateParagraphRevision(wParagraph3, isIncludeParaItems: false);
			}
			num2 = 1;
			num = 1;
		}
		itemIndex += num - num2;
		int j = num2;
		for (int num6 = num3; j <= num6; j++)
		{
			Entity entity2 = m_textPart.Items[j].Clone();
			m_body.Items.Insert(itemIndex + j, entity2);
			if (entity2 is WParagraph)
			{
				(entity2 as WParagraph).ApplyStyle((entity2 as WParagraph).StyleName, isDomChanges: false);
				if (m_saveFormatting)
				{
					ApplySrcFormat(entity2 as WParagraph);
				}
				else
				{
					UpdateFormatting(entity2 as WParagraph, m_textPart.Items[j] as WParagraph);
				}
			}
			else if (entity2 is WTable)
			{
				ApplySrcFormat(entity2 as WTable, m_textPart.Items[j] as WTable);
			}
		}
		if (m_body.Document.ClonedFields.Count > count && wParagraph4 != null)
		{
			for (int k = 0; k < wParagraph4.ChildEntities.Count; k++)
			{
				if (m_body.Document.ClonedFields.Count <= 0)
				{
					break;
				}
				if (wParagraph4.ChildEntities[k] is WFieldMark && (wParagraph4.ChildEntities[k] as WFieldMark).Type == FieldMarkType.FieldEnd)
				{
					m_body.Document.ClonedFields.Pop().FieldEnd = wParagraph4.ChildEntities[k] as WFieldMark;
				}
			}
		}
		if (itemIndex > 0 && m_body.Items[itemIndex - 1] is WParagraph && (m_body.Items[itemIndex - 1] as WParagraph).Items.Count == 1 && (m_body.Items[itemIndex - 1] as WParagraph).Items[0] is BookmarkStart && m_textPart.Items[0] is WTable)
		{
			WParagraph obj = m_body.Items[itemIndex - 1] as WParagraph;
			WTable wTable = m_body.Items[itemIndex] as WTable;
			string name = (obj.Items[0].Clone() as BookmarkStart).Name;
			WordDocument document = m_body.Document;
			document.Bookmarks.Remove(document.Bookmarks[name]);
			obj.RemoveSelf();
			if (wTable.FirstRow != null && wTable.FirstRow.Cells.Count > 0)
			{
				if (wTable.FirstRow.Cells[0].Items.Count == 0)
				{
					wTable.FirstRow.Cells[0].Items.Add(new WParagraph(document));
				}
				if (wTable.FirstRow.Cells[0].Items[0] is WParagraph)
				{
					(wTable.FirstRow.Cells[0].Items[0] as WParagraph).Items.Insert(0, new BookmarkStart(document, name));
				}
			}
			WParagraph wParagraph5;
			if (m_textPart.Items.Count == 1)
			{
				wParagraph5 = m_body.Items[itemIndex] as WParagraph;
				wParagraph5.Items.Insert(0, new BookmarkEnd(document, name));
				return;
			}
			wParagraph5 = m_body.Items[itemIndex + m_textPart.Items.Count - 2] as WParagraph;
			if (wParagraph5 == null)
			{
				wParagraph5 = new WParagraph(document);
				m_body.Items.Add(wParagraph5);
			}
			wParagraph5.Items.Add(new BookmarkEnd(document, name));
		}
		else
		{
			if (!isBkmkReplace || text == null)
			{
				return;
			}
			Bookmark bookmark = m_body.Document.Bookmarks.FindByName(text);
			if (bookmark.BookmarkEnd != null)
			{
				return;
			}
			WParagraph wParagraph6 = null;
			if (wParagraph3 != null && wParagraph4 == null)
			{
				wParagraph6 = wParagraph3;
			}
			else if (wParagraph4 != null)
			{
				wParagraph6 = wParagraph4;
			}
			if (wParagraph6 == null)
			{
				return;
			}
			for (int l = 0; l < wParagraph6.ChildEntities.Count; l++)
			{
				if (wParagraph6.ChildEntities[l] is BookmarkEnd && (wParagraph6.ChildEntities[l] as BookmarkEnd).Name == text)
				{
					bookmark.SetEnd(wParagraph6.ChildEntities[l] as BookmarkEnd);
					break;
				}
			}
		}
	}

	internal void PasteAt(InlineContentControl inlineContentControl, int index, WCharacterFormat sourceFormat, bool saveFormatting)
	{
		m_body = inlineContentControl.OwnerParagraph.OwnerTextBody;
		m_pItemIndex = index;
		m_srcFormat = sourceFormat;
		m_saveFormatting = saveFormatting;
		PasteAt(inlineContentControl, m_textPart);
	}

	private void PasteAt(InlineContentControl inlineContentControl, WTextBody textBody)
	{
		if (textBody.Items.Count == 0)
		{
			return;
		}
		for (int i = 0; i < textBody.Items.Count; i++)
		{
			if (textBody.Items[i] is WParagraph)
			{
				PasteParagraphAtInlineContentControl(inlineContentControl, textBody.Items[i] as WParagraph);
				if (inlineContentControl.ContentControlProperties.Type == ContentControlType.Text && inlineContentControl.ContentControlProperties.Multiline && i < textBody.Items.Count - 1)
				{
					Break entity = new Break(m_body.Document, BreakType.LineBreak);
					inlineContentControl.ParagraphItems.Insert(m_pItemIndex, entity);
					m_pItemIndex++;
				}
			}
			else if (textBody.Items[i] is WTable)
			{
				PasteTableAtInlineContentControl(inlineContentControl, textBody.Items[i] as WTable);
			}
			else if (textBody.Items[i] is BlockContentControl)
			{
				BlockContentControl blockContentControl = textBody.Items[i] as BlockContentControl;
				if (inlineContentControl.ContentControlProperties.Type == ContentControlType.RichText)
				{
					InlineContentControl inlineContentControl2 = new InlineContentControl(m_body.Document, ContentControlType.RichText);
					inlineContentControl.ParagraphItems.Insert(m_pItemIndex, inlineContentControl2);
					m_pItemIndex = 0;
					PasteAt(inlineContentControl2, blockContentControl.TextBody);
					m_pItemIndex = inlineContentControl2.Index;
				}
				else if (inlineContentControl.ContentControlProperties.Type == ContentControlType.Text)
				{
					PasteAt(inlineContentControl, blockContentControl.TextBody);
				}
			}
		}
	}

	private void PasteParagraphAtInlineContentControl(InlineContentControl inlineContentControl, WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = paragraph.Items[i];
			if (paragraphItem is WTextRange)
			{
				PasteTextRangeAtInlineContentControl(inlineContentControl, paragraphItem as WTextRange);
			}
			else if (paragraphItem is InlineContentControl && inlineContentControl.ContentControlProperties.Type == ContentControlType.Text)
			{
				PasteContentControlAsPlainText(inlineContentControl, paragraphItem as InlineContentControl);
			}
			else if (paragraphItem is Break && ((paragraphItem as Break).BreakType == BreakType.LineBreak || (paragraphItem as Break).BreakType == BreakType.TextWrappingBreak) && (inlineContentControl.ContentControlProperties.Type == ContentControlType.RichText || (inlineContentControl.ContentControlProperties.Type == ContentControlType.Text && inlineContentControl.ContentControlProperties.Multiline)))
			{
				ParagraphItem entity = paragraphItem.Clone() as ParagraphItem;
				inlineContentControl.ParagraphItems.Insert(m_pItemIndex, entity);
				m_pItemIndex++;
			}
			else if (inlineContentControl.ContentControlProperties.Type == ContentControlType.RichText)
			{
				ParagraphItem entity2 = paragraphItem.Clone() as ParagraphItem;
				inlineContentControl.ParagraphItems.Insert(m_pItemIndex, entity2);
				m_pItemIndex++;
			}
		}
		if (paragraph.Items.Count == 0 && inlineContentControl.ContentControlProperties.Type == ContentControlType.RichText)
		{
			WTextRange wTextRange = new WTextRange(m_body.Document);
			wTextRange.Text = " ";
			PasteTextRangeAtInlineContentControl(inlineContentControl, wTextRange);
		}
	}

	private void PasteContentControlAsPlainText(InlineContentControl sourceContentControl, InlineContentControl nestedContentControl)
	{
		foreach (ParagraphItem paragraphItem in nestedContentControl.ParagraphItems)
		{
			if (paragraphItem is WTextRange)
			{
				PasteTextRangeAtInlineContentControl(sourceContentControl, paragraphItem as WTextRange);
			}
			else if (paragraphItem is InlineContentControl)
			{
				PasteContentControlAsPlainText(sourceContentControl, paragraphItem as InlineContentControl);
			}
		}
	}

	private void PasteTableAtInlineContentControl(InlineContentControl inlineContentControl, WTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				for (int i = 0; i < cell.Items.Count; i++)
				{
					if (cell.Items[i] is WParagraph)
					{
						PasteParagraphAtInlineContentControl(inlineContentControl, cell.Items[i] as WParagraph);
					}
					else if (cell.Items[i] is WTable)
					{
						PasteTableAtInlineContentControl(inlineContentControl, cell.Items[i] as WTable);
					}
				}
				if (inlineContentControl.ContentControlProperties.Type == ContentControlType.Text && cell.Index < row.Cells.Count - 1)
				{
					WTextRange wTextRange = new WTextRange(m_body.Document);
					wTextRange.Text = '\t'.ToString();
					PasteTextRangeAtInlineContentControl(inlineContentControl, wTextRange);
				}
			}
			if (inlineContentControl.ContentControlProperties.Type == ContentControlType.Text && inlineContentControl.ContentControlProperties.Multiline && row.Index < table.Rows.Count - 1)
			{
				Break entity = new Break(m_body.Document, BreakType.LineBreak);
				inlineContentControl.ParagraphItems.Insert(m_pItemIndex, entity);
				m_pItemIndex++;
			}
		}
	}

	private void PasteTextRangeAtInlineContentControl(InlineContentControl inlineContentControl, WTextRange textRange)
	{
		ParagraphItem paragraphItem = textRange.Clone() as ParagraphItem;
		inlineContentControl.ParagraphItems.Insert(m_pItemIndex, paragraphItem);
		m_pItemIndex++;
		if (m_saveFormatting && m_srcFormat != null)
		{
			m_srcFormat.UpdateSourceFormatting((paragraphItem as WTextRange).CharacterFormat);
		}
		else if (!m_saveFormatting)
		{
			textRange.ParaItemCharFormat.UpdateSourceFormatting((paragraphItem as WTextRange).CharacterFormat);
		}
	}

	private void CopyParagraphItems(WParagraph srcParagraph, WParagraph destParagraph)
	{
		int num = 0;
		for (int i = 0; i < srcParagraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = srcParagraph.Items[i];
			ParagraphItem paragraphItem2 = paragraphItem.Clone() as ParagraphItem;
			int count = destParagraph.Items.Count;
			destParagraph.Items.Insert(m_pItemIndex + i - num, paragraphItem2);
			if (paragraphItem is WTextRange)
			{
				paragraphItem.ParaItemCharFormat.UpdateSourceFormatting((paragraphItem2 as WTextRange).CharacterFormat);
			}
			if (count == destParagraph.Items.Count)
			{
				num++;
			}
		}
	}

	internal static void SplitParagraph(WParagraph paragraph, int nextpItemIndex, WParagraph paragraphToInsert)
	{
		int indexInOwnerCollection = paragraph.GetIndexInOwnerCollection();
		paragraph.OwnerTextBody.Items.Insert(indexInOwnerCollection + 1, paragraphToInsert);
		paragraphToInsert.ParagraphFormat.ImportContainer(paragraph.ParagraphFormat);
		paragraphToInsert.BreakCharacterFormat.ImportContainer(paragraph.BreakCharacterFormat);
		while (paragraph.Items.Count > nextpItemIndex)
		{
			paragraphToInsert.Items.Add(paragraph.Items[nextpItemIndex]);
		}
	}

	private void ValidateArgs()
	{
		if (m_body == null)
		{
			throw new ArgumentNullException("textBody");
		}
		if (m_itemIndex < 0 || m_itemIndex > m_body.Items.Count)
		{
			throw new ArgumentOutOfRangeException("itemIndex", "itemIndex is less than 0 or greater than " + m_body.Items.Count);
		}
		if (((m_body.Items.Count > m_itemIndex) ? m_body.Items[m_itemIndex] : null) is WParagraph wParagraph && (m_pItemIndex < 0 || m_pItemIndex > wParagraph.Items.Count))
		{
			throw new ArgumentOutOfRangeException("pItemIndex", "pItemIndex is less than 0 or greater than  " + wParagraph.Items.Count);
		}
	}

	private WParagraph SplitParagraph(WParagraph trgFirstParagraph, WParagraph srcLastParagraph)
	{
		WParagraph wParagraph = ((srcLastParagraph == null) ? new WParagraph(m_body.Document) : ((WParagraph)srcLastParagraph.Clone()));
		if (trgFirstParagraph.Items.Count > m_pItemIndex || srcLastParagraph != null)
		{
			if (wParagraph.Document.IsComparing)
			{
				wParagraph.BreakCharacterFormat.IsInsertRevision = false;
				if (wParagraph.BreakCharacterFormat.m_clonedRevisions != null)
				{
					wParagraph.BreakCharacterFormat.m_clonedRevisions.Clear();
				}
			}
			m_body.Items.Insert(m_itemIndex + 1, wParagraph);
			if (trgFirstParagraph.HasRenderableItemFromIndex(m_pItemIndex))
			{
				wParagraph.ApplyStyle(trgFirstParagraph.StyleName, isDomChanges: false);
			}
			ApplySrcFormat(wParagraph);
		}
		while (trgFirstParagraph.Items.Count > m_pItemIndex)
		{
			wParagraph.UpdateBookmarkEnd(trgFirstParagraph.Items[m_pItemIndex], wParagraph, isAddItem: true);
		}
		return wParagraph;
	}

	private void UpdateFormatting(WParagraph trgParagraph, WParagraph srcParagraph)
	{
		srcParagraph.ParagraphFormat.UpdateSourceFormatting(trgParagraph.ParagraphFormat);
		srcParagraph.BreakCharacterFormat.UpdateSourceFormatting(trgParagraph.BreakCharacterFormat);
		int i = 0;
		for (int count = trgParagraph.Items.Count; i < count; i++)
		{
			Entity entity = trgParagraph.Items[i];
			Entity entity2 = null;
			if (i < srcParagraph.Items.Count)
			{
				entity2 = srcParagraph.Items[i];
			}
			if (entity is WTextRange && entity2 is WTextRange)
			{
				(entity2 as WTextRange).CharacterFormat.UpdateSourceFormatting((entity as WTextRange).CharacterFormat);
			}
		}
	}

	private void EnsureTextBody(WordDocument doc)
	{
		if (m_textPart != null && m_textPart.Document == doc)
		{
			Clear();
		}
		else
		{
			m_textPart = new WTextBody(doc, null);
		}
	}

	private void ApplySrcFormat(WParagraph paragraph)
	{
		if (!m_saveFormatting || m_srcFormat == null || paragraph == null)
		{
			return;
		}
		m_srcFormat.UpdateSourceFormatting(paragraph.BreakCharacterFormat);
		foreach (ParagraphItem item in paragraph.Items)
		{
			if (item is WTextRange)
			{
				m_srcFormat.UpdateSourceFormatting(item.ParaItemCharFormat);
			}
		}
	}

	private void ApplySrcFormat(WTable table, WTable srcTable)
	{
		if ((m_saveFormatting && m_srcFormat == null) || table == null)
		{
			return;
		}
		for (int i = 0; i < table.Rows.Count; i++)
		{
			for (int j = 0; j < table.Rows[i].Cells.Count; j++)
			{
				for (int k = 0; k < table.Rows[i].Cells[j].ChildEntities.Count; k++)
				{
					if (table.Rows[i].Cells[j].ChildEntities[k] is WParagraph)
					{
						WParagraph wParagraph = table.Rows[i].Cells[j].ChildEntities[k] as WParagraph;
						wParagraph.ApplyStyle(wParagraph.StyleName, isDomChanges: false);
						if (m_saveFormatting)
						{
							ApplySrcFormat(wParagraph);
						}
						else
						{
							UpdateFormatting(wParagraph, srcTable.Rows[i].Cells[j].ChildEntities[k] as WParagraph);
						}
					}
					else if (table.Rows[i].Cells[j].ChildEntities[k] is WTable)
					{
						ApplySrcFormat(table.Rows[i].Cells[j].ChildEntities[k] as WTable, srcTable.Rows[i].Cells[j].ChildEntities[k] as WTable);
					}
				}
			}
		}
	}

	internal void GetBookmarkContentPart(BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WParagraph wParagraph = bkmkStart.Owner as WParagraph;
		WParagraph wParagraph2 = bkmkEnd.Owner as WParagraph;
		if ((!wParagraph.IsInCell && !wParagraph2.IsInCell) || wParagraph.OwnerTextBody == wParagraph2.OwnerTextBody)
		{
			GetParagraphBookmarkContent(wParagraph, wParagraph2, bkmkStart, bkmkEnd);
		}
		else if (wParagraph.IsInCell && wParagraph2.IsInCell)
		{
			GetTableBookmarkContent(wParagraph, wParagraph2, bkmkStart, bkmkEnd, null);
		}
		else if (!wParagraph.IsInCell && wParagraph2.IsInCell)
		{
			GetParagraphAfterTableBkmkContent(wParagraph, wParagraph2, bkmkStart, bkmkEnd);
		}
		else if (wParagraph.IsInCell && !wParagraph2.IsInCell)
		{
			GetTableAfteParagraphBkmkContent(wParagraph, wParagraph2, bkmkStart, bkmkEnd);
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

	private void GetTableAfteParagraphBkmkContent(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTableCell wTableCell = paragraphStart.GetOwnerEntity() as WTableCell;
		WTableCell wTableCell2 = null;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int num = -1;
		if (bkmkStart.ColumnFirst != -1 && bkmkStart.ColumnLast != -1)
		{
			num = wTable.LastRow.GetIndexInOwnerCollection();
			if (bkmkStart.ColumnFirst < wTable.Rows[indexInOwnerCollection].Cells.Count && bkmkStart.ColumnLast < wTable.Rows[num].Cells.Count)
			{
				wTableCell = wTable.Rows[indexInOwnerCollection].Cells[bkmkStart.ColumnFirst];
				wTableCell2 = wTable.Rows[num].Cells[bkmkStart.ColumnLast];
			}
			wTableCell.GetIndexInOwnerCollection();
			wTableCell2.GetIndexInOwnerCollection();
			GetTableBookmarkContent(paragraphStart, paragraphEnd, bkmkStart, bkmkEnd, wTableCell2);
			return;
		}
		m_textPart = new WTextBody(wTable.OwnerTextBody.Owner as WSection);
		int indexInOwnerCollection2 = wTable.GetIndexInOwnerCollection();
		int indexInOwnerCollection3 = paragraphEnd.GetIndexInOwnerCollection();
		int num2 = bkmkEnd.GetIndexInOwnerCollection() - 1;
		WTextBody wTextBody = (WTextBody)wTable.Owner;
		WTextBody wTextBody2 = (WTextBody)paragraphEnd.Owner;
		int num3 = GetSection(wTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection4 = GetSection(wTextBody2).GetIndexInOwnerCollection();
		bool flag = false;
		if (num3 - 1 == indexInOwnerCollection4)
		{
			flag = true;
		}
		bool flag2 = false;
		flag2 = IsBkmkEndInFirstItem(paragraphEnd, bkmkEnd, num2);
		if (!(wTextBody == wTextBody2 && wTextBody.ChildEntities[indexInOwnerCollection2 + 1] == paragraphEnd && flag2))
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, wTable.LastRow.GetIndexInOwnerCollection(), 0, wTable.LastCell.GetIndexInOwnerCollection(), m_textPart);
			AddTextBodyItems(indexInOwnerCollection2, indexInOwnerCollection3, wTable.OwnerTextBody);
			if (!flag2 || !flag)
			{
				CopyMultiSectionBodyItems(num3, indexInOwnerCollection4, wTextBody.Document);
				CopyBkmkEndTextBody(indexInOwnerCollection3, num2, wTextBody2, flag2, flag);
			}
		}
		else
		{
			wTableCell2 = wTable.LastCell;
			GetTableBookmarkContent(paragraphStart, paragraphEnd, bkmkStart, bkmkEnd, wTableCell2);
		}
	}

	private void GetParagraphAfterTableBkmkContent(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTextBody wTextBody = (WTextBody)paragraphStart.Owner;
		WTableCell wTableCell = paragraphEnd.GetOwnerEntity() as WTableCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTextBody wTextBody2 = (WTextBody)wTable.Owner;
		m_textPart = new WTextBody(wTextBody.Owner as WSection);
		bool flag = false;
		int indexInOwnerCollection = wTable.GetIndexInOwnerCollection();
		int num = GetSection(wTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = GetSection(wTextBody2).GetIndexInOwnerCollection();
		if (num - 1 == indexInOwnerCollection2)
		{
			flag = true;
		}
		int num2 = wTableCell.Owner.GetIndexInOwnerCollection();
		int indexInOwnerCollection3 = wTableCell.GetIndexInOwnerCollection();
		if (indexInOwnerCollection3 == 0 && num2 != 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph && IsBkmkEndInFirstItem(wTableCell.Paragraphs[0], bkmkEnd, bkmkEnd.GetIndexInOwnerCollection() - 1))
		{
			num2--;
		}
		if (indexInOwnerCollection >= 0 && num2 == 0 && indexInOwnerCollection3 == 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph)
		{
			if (wTableCell.ChildEntities[0] is WParagraph && indexInOwnerCollection == 0)
			{
				WParagraph wParagraph = wTableCell.ChildEntities[0] as WParagraph;
				if (wParagraph.Items[0] == bkmkEnd)
				{
					WParagraph wParagraph2 = new WParagraph(wTextBody2.Document);
					wParagraph2.Items.Add(wParagraph.Items[0]);
					wTextBody2.ChildEntities.Insert(0, wParagraph2);
					GetParagraphBookmarkContent(paragraphStart, paragraphEnd, bkmkStart, bkmkEnd);
					return;
				}
			}
			else if (indexInOwnerCollection > 0)
			{
				(wTextBody2.Items[indexInOwnerCollection - 1] as WParagraph).Items.Add(bkmkEnd);
				GetParagraphBookmarkContent(paragraphStart, wTextBody2.Items[indexInOwnerCollection - 1] as WParagraph, bkmkStart, bkmkEnd);
				return;
			}
		}
		int indexInOwnerCollection4 = paragraphStart.GetIndexInOwnerCollection();
		int bkmkStartNextItemIndex = bkmkStart.GetIndexInOwnerCollection() + 1;
		CopyBkmkStartTextBody(indexInOwnerCollection4, indexInOwnerCollection, wTextBody, bkmkStartNextItemIndex, indexInOwnerCollection - 1, flag);
		if (!flag)
		{
			CopyMultiSectionBodyItems(num, indexInOwnerCollection2, wTextBody.Document);
			AddTextBodyItems(0, indexInOwnerCollection - 1, wTable.OwnerTextBody);
		}
		bkmkStart.GetBkmkContentInDiffCell(wTable, 0, num2, 0, wTable.LastCell.GetIndexInOwnerCollection(), m_textPart);
	}

	private void GetTableBookmarkContent(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, WTableCell bkmkEndCell)
	{
		WTableCell wTableCell = paragraphStart.GetOwnerEntity() as WTableCell;
		if (paragraphEnd.IsInCell)
		{
			bkmkEndCell = paragraphEnd.GetOwnerEntity() as WTableCell;
		}
		WTableCell tempBkmkEndCell = bkmkEndCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTable wTable2 = (WTable)bkmkEndCell.Owner.Owner;
		m_textPart = new WTextBody(wTable.Owner.Owner as WSection);
		bool flag = false;
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int endTableRowIndex = bkmkEndCell.Owner.GetIndexInOwnerCollection();
		int bkmkEndCellIndex = bkmkEndCell.GetIndexInOwnerCollection();
		int bkmkStartCellIndex = wTableCell.GetIndexInOwnerCollection();
		int num = GetSection(wTable).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = GetSection(wTable2).GetIndexInOwnerCollection();
		if (num - 1 == indexInOwnerCollection2)
		{
			flag = true;
		}
		bkmkStart.GetBookmarkStartAndEndCell(wTableCell, bkmkEndCell, tempBkmkEndCell, wTable, wTable2, bkmkStart, bkmkEnd, indexInOwnerCollection, ref endTableRowIndex, ref bkmkStartCellIndex, ref bkmkEndCellIndex);
		if (wTable == wTable2)
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, endTableRowIndex, bkmkStartCellIndex, bkmkEndCellIndex, m_textPart);
			return;
		}
		WTable bkmkEndTable = wTable2;
		int bkmkEndRowIndex = endTableRowIndex;
		bool flag2 = flag && wTable2.IsInCell && bkmkStart.IsBookmarkEndAtSameCell(wTableCell, wTable, ref bkmkEndTable, ref bkmkEndRowIndex);
		if (!flag2)
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, wTable.Rows.Count - 1, 0, wTable.LastCell.GetIndexInOwnerCollection(), m_textPart);
		}
		int indexInOwnerCollection3 = wTable.GetIndexInOwnerCollection();
		int indexInOwnerCollection4 = wTable2.GetIndexInOwnerCollection();
		if (flag)
		{
			if (flag2)
			{
				AddTextBodyItemsOfNestedTable(bkmkStart, bkmkEndTable, bkmkEndRowIndex, wTableCell);
			}
			else
			{
				AddTextBodyItems(indexInOwnerCollection3, indexInOwnerCollection4, wTable.OwnerTextBody);
			}
		}
		else
		{
			AddTextBodyItems(indexInOwnerCollection3, wTable.OwnerTextBody.ChildEntities.Count, wTable.OwnerTextBody);
			CopyMultiSectionBodyItems(num, indexInOwnerCollection2, wTable2.Document);
			AddTextBodyItems(0, indexInOwnerCollection4, wTable2.OwnerTextBody);
		}
		if (flag2)
		{
			bkmkStart.GetBkmkContentInDiffCell(bkmkEndTable, 0, bkmkEndRowIndex, 0, bkmkEndTable.LastCell.GetIndexInOwnerCollection(), m_textPart);
		}
		else
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable2, 0, endTableRowIndex, 0, wTable2.LastCell.GetIndexInOwnerCollection(), m_textPart);
		}
	}

	private void AddTextBodyItemsOfNestedTable(BookmarkStart bkmrkStart, WTable bkrmkEndTable, int endRowIndex, WTableCell bkmrkStartCell)
	{
		int index = bkmrkStart.OwnerParagraph.Index;
		int index2 = bkrmkEndTable.Index;
		for (int i = index; i < index2; i++)
		{
			TextBodyItem textBodyItem = (TextBodyItem)bkmrkStartCell.Items[i].Clone();
			if (i == index && textBodyItem.EntityType == EntityType.Paragraph)
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				for (int num = bkmrkStart.Index; num > 0; num--)
				{
					wParagraph.Items.RemoveFromInnerList(0);
				}
			}
			m_textPart.Items.Add(textBodyItem);
		}
	}

	private void AddTextBodyItems(int startItemIndex, int endItemIndex, WTextBody textBody)
	{
		for (int i = startItemIndex + 1; i < endItemIndex; i++)
		{
			TextBodyItem entity = (TextBodyItem)textBody.ChildEntities[i].CloneInt();
			m_textPart.ChildEntities.Add(entity);
		}
	}

	private void GetParagraphBookmarkContent(WParagraph paragraphStart, WParagraph paragraphEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTextBody ownerTextBody = paragraphStart.OwnerTextBody;
		WTextBody ownerTextBody2 = paragraphEnd.OwnerTextBody;
		m_textPart = new WTextBody(GetSection(ownerTextBody) as WSection);
		bool flag = false;
		int indexInOwnerCollection = paragraphStart.GetIndexInOwnerCollection();
		int bkmkStartNextItemIndex = bkmkStart.GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = paragraphEnd.GetIndexInOwnerCollection();
		int num = bkmkEnd.GetIndexInOwnerCollection() - 1;
		if (ownerTextBody == ownerTextBody2)
		{
			flag = true;
		}
		if (flag)
		{
			TextBodySelection textBodySelection = new TextBodySelection(bkmkStart, bkmkEnd);
			textBodySelection.ParagraphItemStartIndex++;
			textBodySelection.ParagraphItemEndIndex--;
			Copy(textBodySelection, isFindField: true);
		}
		else
		{
			CopyBkmkStartTextBody(indexInOwnerCollection, indexInOwnerCollection2, ownerTextBody, bkmkStartNextItemIndex, num, isInSingleSection: false);
			int startSectionIndex = GetSection(ownerTextBody).GetIndexInOwnerCollection() + 1;
			int indexInOwnerCollection3 = GetSection(ownerTextBody2).GetIndexInOwnerCollection();
			CopyMultiSectionBodyItems(startSectionIndex, indexInOwnerCollection3, ownerTextBody.Document);
			bool isFirstBkmkEnd = IsBkmkEndInFirstItem(paragraphEnd, bkmkEnd, num);
			CopyBkmkEndTextBody(indexInOwnerCollection2, num, ownerTextBody2, isFirstBkmkEnd, isInSingleSection: false);
		}
	}

	private void CopyBkmkStartTextBody(int startParagraphIndex, int endParagraphIndex, WTextBody startTextBody, int bkmkStartNextItemIndex, int bkmkEndPrevItemIndex, bool isInSingleSection)
	{
		int num = (isInSingleSection ? endParagraphIndex : startTextBody.ChildEntities.Count);
		for (int i = startParagraphIndex; i < num; i++)
		{
			TextBodyItem textBodyItem = startTextBody.ChildEntities[i] as TextBodyItem;
			if (startParagraphIndex == i && bkmkStartNextItemIndex > 0)
			{
				WParagraph wParagraph = (textBodyItem as WParagraph).Clone() as WParagraph;
				while (bkmkStartNextItemIndex > 0)
				{
					wParagraph.Items.RemoveFromInnerList(0);
					bkmkStartNextItemIndex--;
				}
				if (wParagraph.ChildEntities.Count > 0)
				{
					m_textPart.ChildEntities.Add(wParagraph);
				}
			}
			else
			{
				m_textPart.ChildEntities.Add(textBodyItem.Clone());
			}
		}
	}

	private void CopyBkmkEndTextBody(int endParagraphIndex, int bkmkEndPreviosItemIndex, WTextBody endTextBody, bool IsFirstBkmkEnd, bool isInSingleSection)
	{
		if (!isInSingleSection)
		{
			CopyBkmkStartTextBody(0, endParagraphIndex, endTextBody, 0, -1, isInSingleSection: true);
		}
		if (!IsFirstBkmkEnd)
		{
			WParagraph wParagraph = (endTextBody.Items[endParagraphIndex] as WParagraph).Clone() as WParagraph;
			while (bkmkEndPreviosItemIndex + 1 < wParagraph.Items.Count)
			{
				wParagraph.Items.RemoveFromInnerList(bkmkEndPreviosItemIndex + 1);
			}
			m_textPart.ChildEntities.Add(wParagraph);
		}
	}

	private bool IsBkmkEndInFirstItem(WParagraph paragraph, BookmarkEnd bkmkEnd, int bkmkEndPreItemIndex)
	{
		for (int i = 0; i <= bkmkEndPreItemIndex; i++)
		{
			if (!(paragraph.Items[i] is BookmarkStart) && !(paragraph.Items[i] is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}

	private void CopyMultiSectionBodyItems(int startSectionIndex, int endSectionIndex, WordDocument Document)
	{
		for (int i = startSectionIndex; i < endSectionIndex; i++)
		{
			foreach (TextBodyItem childEntity in Document.Sections[i].Body.ChildEntities)
			{
				m_textPart.ChildEntities.Add(childEntity.Clone());
			}
		}
	}
}
