using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

internal class TextReplacer
{
	[ThreadStatic]
	public static TextReplacer m_instance;

	public static TextReplacer Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new TextReplacer();
			}
			return m_instance;
		}
	}

	public int Replace(WParagraph para, Regex pattern, string replacement)
	{
		MatchCollection matchCollection = null;
		string text = para.Text;
		if (!string.IsNullOrEmpty(text))
		{
			matchCollection = pattern.Matches(text);
		}
		int num = 0;
		if (matchCollection != null && matchCollection.Count > 0)
		{
			int num2 = 0;
			int num3 = 0;
			int length = replacement.Length;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			foreach (Match item in matchCollection)
			{
				num4 = item.Index + num2;
				num5 = item.Length;
				num3 = length - item.Length;
				num6 = num4 + item.Length;
				int startIndex = 0;
				if (!EnsureStartAndEndOwner(para, num4, num6, out var tr, out startIndex))
				{
					continue;
				}
				para.ReplaceWithoutCorrection(num4, num5, replacement);
				num++;
				int num7 = tr.StartPos + tr.TextLength;
				if (tr.Owner is Break && ((tr.Owner as Break).BreakType == BreakType.LineBreak || (tr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
				{
					num7 = (tr.Owner as Break).EndPos;
					tr.StartPos = (tr.Owner as Break).StartPos;
					int indexInOwnerCollection = tr.Owner.GetIndexInOwnerCollection();
					para.Items.UnsafeRemoveAt(indexInOwnerCollection);
					para.Items.InsertToInnerList(indexInOwnerCollection, tr);
					tr.SetOwner(para);
					tr.TextLength = num7 - tr.StartPos;
				}
				tr.SafeText = false;
				ParagraphItem pItem = tr;
				if (num7 >= num4 + num5)
				{
					tr.TextLength += num3;
					if (tr.Owner is InlineContentControl && (tr.Owner as InlineContentControl).ContentControlProperties.XmlMapping.IsMapped)
					{
						tr.Text = tr.Text;
					}
				}
				else
				{
					List<ParagraphItem> list = new List<ParagraphItem>();
					WTextRange nextTr;
					if (tr.Owner is InlineContentControl)
					{
						RemoveInternalItems((tr.Owner as InlineContentControl).ParagraphItems, num4 + num5, startIndex + 1, out nextTr, list);
					}
					else
					{
						RemoveInternalItems(para.Items, num4 + num5, startIndex + 1, out nextTr, list);
					}
					num6 = num4 + num5;
					if (nextTr != null)
					{
						nextTr.TextLength -= num6 - nextTr.StartPos;
						nextTr.StartPos = num6 + num3;
						startIndex++;
						pItem = nextTr;
					}
					tr.TextLength = num6 + num3 - tr.StartPos;
					if (tr.Owner is InlineContentControl && (tr.Owner as InlineContentControl).ContentControlProperties.XmlMapping.IsMapped)
					{
						tr.Text = tr.Text;
					}
					if (list.Count != 0)
					{
						foreach (ParagraphItem item2 in list)
						{
							if (item2 is BookmarkStart)
							{
								para.Items.InsertToInnerList(tr.Index + 1, item2);
							}
							else if (item2 is BookmarkEnd)
							{
								if (num4 != tr.StartPos)
								{
									num4 -= tr.StartPos;
									WTextRange wTextRange = tr.Clone() as WTextRange;
									wTextRange.Text = wTextRange.Text.Substring(0, num4);
									tr.Text = tr.Text.Substring(num4);
									para.Items.InsertToInnerList(tr.Index, wTextRange);
									num4 = tr.StartPos;
								}
								para.Items.InsertToInnerList(tr.Index, item2);
							}
						}
					}
				}
				CorrectNextItems(pItem, startIndex, num3);
				num2 += num3;
				if (para.Document.ReplaceFirst)
				{
					break;
				}
			}
		}
		if (!para.Document.ReplaceFirst || num <= 0)
		{
			num += ReplaceInItems(para.Items, pattern, replacement);
		}
		para.IsTextReplaced = false;
		return num;
	}

	private bool EnsureStartAndEndOwner(WParagraph para, int startCharPos, int endCharPos, out WTextRange tr, out int startIndex)
	{
		startIndex = FindUtils.GetStartRangeIndex(para, startCharPos + 1, out tr);
		WTextRange wTextRange = tr;
		tr = null;
		FindUtils.GetStartRangeIndex(para, endCharPos, out tr);
		WTextRange endTextRange = tr;
		tr = null;
		tr = wTextRange;
		return FindUtils.EnsureSameOwner(wTextRange, endTextRange);
	}

	private static int ReplaceInItems(ParagraphItemCollection items, Regex pattern, string replacement)
	{
		int num = 0;
		foreach (ParagraphItem item in items)
		{
			WTextBody wTextBody = null;
			switch (item.EntityType)
			{
			case EntityType.Comment:
				wTextBody = ((WComment)item).TextBody;
				break;
			case EntityType.Footnote:
				wTextBody = ((WFootnote)item).TextBody;
				break;
			case EntityType.TextBox:
				wTextBody = ((WTextBox)item).TextBoxBody;
				break;
			case EntityType.AutoShape:
				wTextBody = ((Shape)item).TextBody;
				break;
			}
			if (wTextBody != null)
			{
				num += wTextBody.Replace(pattern, replacement);
			}
			if (item is InlineContentControl)
			{
				num += ReplaceInItems((item as InlineContentControl).ParagraphItems, pattern, replacement);
			}
			else if (item is GroupShape)
			{
				GroupShape groupShape = (GroupShape)item;
				num += ReplaceInItems(groupShape.ChildShapes, pattern, replacement);
			}
			if (item.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		return num;
	}

	private static int ReplaceInItems(ChildShapeCollection childShapes, Regex pattern, string replacement)
	{
		int num = 0;
		foreach (ChildShape childShape in childShapes)
		{
			if (childShape is ChildGroupShape)
			{
				ChildGroupShape childGroupShape = (ChildGroupShape)childShape;
				num += ReplaceInItems(childGroupShape.ChildShapes, pattern, replacement);
			}
			else if (childShape.HasTextBody)
			{
				num += childShape.TextBody.Replace(pattern, replacement);
			}
			if (childShape.Document.ReplaceFirst && num > 0)
			{
				return num;
			}
		}
		return num;
	}

	internal void ReplaceSingleLine(TextSelection[] findText, string replacement)
	{
		if (findText == null || findText.Length == 0)
		{
			return;
		}
		TextSelection textSelection = null;
		int num = findText.Length - 1;
		textSelection = findText[0];
		if (textSelection.SelectedText == textSelection.OwnerParagraph.Text)
		{
			WTextRange wTextRange = textSelection.GetAsOneRange().Clone() as WTextRange;
			WParagraph ownerParagraph = findText[num].OwnerParagraph;
			for (int i = 0; i <= num; i++)
			{
				textSelection = findText[i];
				textSelection.SplitAndErase();
				if (i == num)
				{
					ownerParagraph.ChildEntities.Insert(0, wTextRange);
					wTextRange.Text = replacement;
				}
				else
				{
					RemoveOwnerPara(textSelection);
				}
			}
		}
		else
		{
			for (int num2 = num; num2 > 0; num2--)
			{
				textSelection = findText[num2];
				textSelection.SplitAndErase();
				RemoveOwnerPara(textSelection);
			}
			textSelection = findText[0];
			textSelection.GetAsOneRange().Text = replacement;
		}
	}

	internal void ReplaceSingleLine(TextSelection[] findText, TextSelection replacement)
	{
		if (findText == null || findText.Length == 0)
		{
			return;
		}
		TextSelection textSelection = null;
		int num = findText.Length - 1;
		textSelection = findText[0];
		if (textSelection.SelectedText == textSelection.OwnerParagraph.Text)
		{
			for (int i = 0; i <= num; i++)
			{
				textSelection = findText[i];
				int startIndex = textSelection.SplitAndErase();
				if (i == num)
				{
					WParagraph ownerParagraph = textSelection.OwnerParagraph;
					replacement.CopyTo(ownerParagraph, startIndex, saveFormatting: false, null);
				}
				else
				{
					RemoveOwnerPara(textSelection);
				}
			}
			return;
		}
		for (int num2 = num; num2 >= 0; num2--)
		{
			textSelection = findText[num2];
			int startIndex2 = textSelection.SplitAndErase();
			if (num2 == 0)
			{
				WParagraph ownerParagraph2 = textSelection.OwnerParagraph;
				replacement.CopyTo(ownerParagraph2, startIndex2, saveFormatting: false, null);
			}
			else
			{
				RemoveOwnerPara(textSelection);
			}
		}
	}

	internal void ReplaceSingleLine(TextSelection[] findText, TextBodyPart replacement)
	{
		if (findText == null || findText.Length == 0)
		{
			return;
		}
		TextSelection textSelection = null;
		for (int num = findText.Length - 1; num >= 0; num--)
		{
			textSelection = findText[num];
			int pItemIndex = textSelection.SplitAndErase();
			if (num == 0)
			{
				WParagraph ownerParagraph = textSelection.OwnerParagraph;
				replacement.PasteAt(ownerParagraph.OwnerTextBody, ownerParagraph.GetIndexInOwnerCollection(), pItemIndex);
			}
			else
			{
				RemoveOwnerPara(textSelection);
			}
		}
	}

	private void RemoveOwnerPara(TextSelection selection)
	{
		WParagraph ownerParagraph = selection.OwnerParagraph;
		if (ownerParagraph.Items.Count == 0)
		{
			ownerParagraph.RemoveSelf();
		}
	}

	private void RemoveInternalItems(ParagraphItemCollection paraItems, int end, int startIndex, out WTextRange nextTr, List<ParagraphItem> bookmarks)
	{
		int num = 0;
		bool flag = false;
		nextTr = null;
		int num2;
		for (num2 = startIndex; num2 < paraItems.Count; num2++)
		{
			nextTr = paraItems[num2] as WTextRange;
			if (nextTr != null)
			{
				num = nextTr.StartPos + nextTr.TextLength;
				if (num > end)
				{
					break;
				}
				if (num == end)
				{
					flag = true;
				}
			}
			if (paraItems[num2] is BookmarkEnd)
			{
				BookmarkEnd bookmarkEnd = paraItems[num2] as BookmarkEnd;
				Bookmark bookmark = bookmarkEnd.Document.Bookmarks.FindByName(bookmarkEnd.Name);
				if (bookmarks.Contains(bookmark.BookmarkStart))
				{
					bookmarks.Remove(bookmark.BookmarkStart);
				}
				else
				{
					bookmarks.Add(paraItems[num2]);
				}
			}
			else if (paraItems[num2] is BookmarkStart)
			{
				bookmarks.Add(paraItems[num2]);
			}
			paraItems.UnsafeRemoveAt(num2);
			if (flag)
			{
				nextTr = ((num2 < paraItems.Count) ? (paraItems[num2] as WTextRange) : null);
				break;
			}
			num2--;
		}
	}

	private void CorrectNextItems(ParagraphItem pItem, int startIndex, int offset)
	{
		if (pItem.Owner is InlineContentControl)
		{
			pItem.Document.UpdateStartPosOfInlineContentControlItems(pItem.Owner as InlineContentControl, startIndex + 1, offset);
			Entity entity = pItem;
			while (!(entity is WParagraph) && entity != null)
			{
				entity = entity.Owner;
				if (entity.Owner is WParagraph)
				{
					pItem.Document.UpdateStartPosOfParaItems(entity as ParagraphItem, offset);
					break;
				}
				if (entity.Owner is InlineContentControl)
				{
					pItem.Document.UpdateStartPosOfInlineContentControlItems(entity.Owner as InlineContentControl, entity.Index + 1, offset);
				}
			}
		}
		else
		{
			pItem.Document.UpdateStartPosOfParaItems(pItem, offset);
		}
	}
}
