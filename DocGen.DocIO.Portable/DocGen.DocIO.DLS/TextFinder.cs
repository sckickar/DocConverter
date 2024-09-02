using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

internal class TextFinder
{
	private List<WParagraph> m_linePCol;

	[ThreadStatic]
	public static TextFinder m_instance;

	public static TextFinder Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new TextFinder();
			}
			return m_instance;
		}
	}

	internal List<WParagraph> SingleLinePCol
	{
		get
		{
			if (m_linePCol == null)
			{
				m_linePCol = new List<WParagraph>();
			}
			return m_linePCol;
		}
	}

	public TextSelectionList Find(WParagraph para, Regex pattern, bool onlyFirstMatch)
	{
		return FindItem(para, pattern, onlyFirstMatch);
	}

	internal TextSelectionList FindItem(WParagraph para, Regex pattern, bool onlyFirstMatch, bool isDocumentComparison = false)
	{
		if (para.Document.IsComparing && para.InternalText != para.Text)
		{
			string internalText = para.InternalText;
			if (pattern.Matches(internalText).Count == 0)
			{
				return null;
			}
		}
		string text = para.Text;
		MatchCollection matchCollection = pattern.Matches(text);
		TextSelectionList textSelectionList = new TextSelectionList();
		if (matchCollection.Count > 0)
		{
			foreach (Match item in matchCollection)
			{
				int index = item.Index;
				int endCharPos = item.Index + item.Length;
				TextSelection textSelection = new TextSelection(para, index, endCharPos);
				if (textSelection.Count != 0)
				{
					textSelection.SelectionChain = textSelectionList;
					textSelectionList.Add(textSelection);
				}
				if (onlyFirstMatch)
				{
					break;
				}
			}
		}
		if (!isDocumentComparison)
		{
			FindInItems(para.Items, pattern, onlyFirstMatch, textSelectionList);
		}
		return textSelectionList;
	}

	private static void FindInItems(ParagraphItemCollection items, Regex pattern, bool onlyFirstMatch, TextSelectionList selections)
	{
		if (selections.Count > 0 && onlyFirstMatch)
		{
			return;
		}
		foreach (ParagraphItem item in items)
		{
			WTextBody textBody = GetTextBody(item);
			if (textBody == null)
			{
				if (item is InlineContentControl)
				{
					FindInItems((item as InlineContentControl).ParagraphItems, pattern, onlyFirstMatch, selections);
					if (selections.Count > 0 && onlyFirstMatch)
					{
						break;
					}
				}
				else if (item is GroupShape && Find(((GroupShape)item).ChildShapes, pattern, onlyFirstMatch, selections))
				{
					break;
				}
			}
			else if (Find(textBody, pattern, onlyFirstMatch, selections))
			{
				break;
			}
		}
	}

	private static bool Find(ChildShapeCollection childShapes, Regex pattern, bool onlyFirstMatch, TextSelectionList selections)
	{
		foreach (ChildShape childShape in childShapes)
		{
			if (childShape is ChildGroupShape)
			{
				if (Find(((ChildGroupShape)childShape).ChildShapes, pattern, onlyFirstMatch, selections))
				{
					return true;
				}
			}
			else if (childShape.HasTextBody && Find(childShape.TextBody, pattern, onlyFirstMatch, selections))
			{
				return true;
			}
		}
		return false;
	}

	private static bool Find(WTextBody textBody, Regex pattern, bool onlyFirstMatch, TextSelectionList selections)
	{
		if (onlyFirstMatch)
		{
			TextSelection textSelection = textBody.Find(pattern);
			if (textSelection != null)
			{
				selections.Add(textSelection);
				return true;
			}
		}
		else
		{
			TextSelectionList textSelectionList = textBody.FindAll(pattern, isDocumentComparison: false, isFromTextbody: false);
			if (textSelectionList != null && textSelectionList.Count > 0)
			{
				selections.AddRange(textSelectionList);
			}
		}
		return false;
	}

	private static WTextBody GetTextBody(ParagraphItem item)
	{
		WTextBody result = null;
		switch (item.EntityType)
		{
		case EntityType.Comment:
			result = ((WComment)item).TextBody;
			break;
		case EntityType.Footnote:
			result = ((WFootnote)item).TextBody;
			break;
		case EntityType.TextBox:
			result = ((WTextBox)item).TextBoxBody;
			break;
		case EntityType.AutoShape:
			result = ((Shape)item).TextBody;
			break;
		}
		return result;
	}

	public TextSelection[] FindSingleLine(WTextBody textBody, Regex pattern)
	{
		if (textBody.Items.Count == 0)
		{
			return null;
		}
		return FindSingleLine(textBody, pattern, 0, textBody.Items.Count - 1);
	}

	public TextSelection[] FindSingleLine(WTextBody textBody, Regex pattern, int startIndex, int endIndex)
	{
		TextSelection[] array = null;
		WParagraph wParagraph = null;
		WTable wTable = null;
		for (int i = startIndex; i <= endIndex; i++)
		{
			if (textBody.Items[i] is WParagraph)
			{
				wParagraph = textBody.Items[i] as WParagraph;
				array = FindInItems(wParagraph, pattern, 0, wParagraph.Items.Count - 1);
				if (array != null)
				{
					return array;
				}
				array = FindSingleLine(pattern);
			}
			else if (textBody.Items[i] is WTable)
			{
				wTable = textBody.Items[i] as WTable;
				array = FindSingleLine(wTable, pattern);
			}
			if (array != null)
			{
				return array;
			}
		}
		return FindSingleLine(pattern);
	}

	internal TextSelection[] FindInItems(WParagraph para, Regex pattern, int startIndex, int endIndex)
	{
		if (!SingleLinePCol.Contains(para))
		{
			SingleLinePCol.Add(para);
		}
		TextSelection[] array = null;
		for (int i = startIndex; i <= endIndex; i++)
		{
			ParagraphItem paragraphItem = para[i];
			WTextBody textBody = GetTextBody(paragraphItem);
			if (textBody == null)
			{
				if (paragraphItem is GroupShape)
				{
					GroupShape groupShape = (GroupShape)paragraphItem;
					array = FindInItems(groupShape.ChildShapes, pattern);
				}
			}
			else
			{
				array = FindSingleLine(textBody, pattern);
			}
			if (array != null)
			{
				return array;
			}
		}
		return array;
	}

	private TextSelection[] FindInItems(ChildShapeCollection childShapes, Regex pattern)
	{
		TextSelection[] array = null;
		foreach (ChildShape childShape in childShapes)
		{
			if (childShape is ChildGroupShape)
			{
				ChildGroupShape childGroupShape = (ChildGroupShape)childShape;
				array = FindInItems(childGroupShape.ChildShapes, pattern);
			}
			else if (childShape.HasTextBody)
			{
				array = FindSingleLine(childShape.TextBody, pattern);
			}
			if (array != null)
			{
				return array;
			}
		}
		return array;
	}

	internal TextSelection[] FindSingleLine(Regex pattern)
	{
		if (m_linePCol == null || m_linePCol.Count == 0)
		{
			return null;
		}
		string text = string.Empty;
		WParagraph wParagraph = null;
		Match match = null;
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int count = m_linePCol.Count; i < count; i++)
		{
			wParagraph = m_linePCol[i];
			stringBuilder.Append(wParagraph.Text);
			if (i == count - 1)
			{
				text = stringBuilder.ToString();
				match = pattern.Match(text);
			}
		}
		if (match != null && match.Success)
		{
			int index = match.Index;
			int num = index + match.Length;
			TextSelectionList textSelectionList;
			if (index == 0 && num == text.Length)
			{
				textSelectionList = new TextSelectionList();
				foreach (WParagraph item2 in m_linePCol)
				{
					if (match.Length == item2.Text.Length)
					{
						TextSelection item = new TextSelection(item2, 0, item2.Text.Length);
						textSelectionList.Add(item);
					}
				}
			}
			else
			{
				textSelectionList = FindSingleLine(m_linePCol, match);
			}
			if (textSelectionList != null && textSelectionList.Count > 0)
			{
				m_linePCol.Clear();
				return textSelectionList.ToArray();
			}
		}
		return null;
	}

	internal TextSelection[] FindSingleLine(WTable table, Regex pattern)
	{
		TextSelection[] array = null;
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				array = FindSingleLine(cell, pattern);
				if (array != null)
				{
					return array;
				}
			}
		}
		return array;
	}

	private TextSelectionList FindSingleLine(List<WParagraph> paragraphs, Match match)
	{
		int index = match.Index;
		int num = index + match.Length;
		string text = string.Empty;
		int num2 = 0;
		int num3 = 0;
		TextSelectionList textSelectionList = null;
		foreach (WParagraph paragraph in paragraphs)
		{
			int num4 = -1;
			int num5 = -1;
			num3 = text.Length;
			text += paragraph.Text;
			num2 = text.Length;
			if (num3 <= index && index <= num2)
			{
				textSelectionList = new TextSelectionList();
				num4 = index - num3;
			}
			if (num3 <= num && num <= num2)
			{
				num5 = num - num3;
			}
			if (num4 != -1 || num5 != -1)
			{
				if (num4 != -1 && num5 != -1)
				{
					textSelectionList.Add(new TextSelection(paragraph, num4, num5));
					break;
				}
				if (num4 != -1 && num4 < paragraph.Text.Length)
				{
					textSelectionList.Add(new TextSelection(paragraph, num4, paragraph.Text.Length));
				}
				else if (num5 != -1 && num5 <= paragraph.Text.Length)
				{
					textSelectionList.Add(new TextSelection(paragraph, 0, num5));
					break;
				}
			}
			else if (num3 > index && num2 < num && paragraph.Text != string.Empty)
			{
				textSelectionList.Add(new TextSelection(paragraph, 0, paragraph.Text.Length));
			}
		}
		return textSelectionList;
	}

	internal static void Close()
	{
		if (m_instance != null)
		{
			m_instance = null;
		}
	}
}
