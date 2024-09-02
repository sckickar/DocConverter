using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Office.Markdown;

internal class MarkdownParser
{
	private int currLineIndex = -1;

	private string[] textlines;

	internal MarkdownDocument markdownDocument;

	private string prevLine = string.Empty;

	private string currentLine = string.Empty;

	private int totalLines;

	private bool isSkip;

	private Dictionary<int, int> listLevelSpace = new Dictionary<int, int>();

	private int listLevel;

	private string startListNumber = string.Empty;

	private string prevListMarker = string.Empty;

	internal MdImportSettings markdownImportSettings;

	private Stack<MdTextFormat> m_styleStack;

	internal bool HasNextLine
	{
		get
		{
			if (textlines != null)
			{
				return currLineIndex <= totalLines - 1;
			}
			return false;
		}
	}

	internal string PreviousLine
	{
		get
		{
			return prevLine;
		}
		set
		{
			prevLine = value;
		}
	}

	internal IMdBlock PreviousBlockItem
	{
		get
		{
			if (markdownDocument == null || markdownDocument.Blocks == null || markdownDocument.Blocks.Count <= 0)
			{
				return null;
			}
			return markdownDocument.Blocks[markdownDocument.Blocks.Count - 1];
		}
	}

	protected MdTextFormat CurrentFormat
	{
		get
		{
			if (m_styleStack.Count > 0)
			{
				return m_styleStack.Peek();
			}
			return new MdTextFormat();
		}
	}

	internal void ParseMd(StreamReader reader, MdImportSettings mdImportSettings)
	{
		markdownImportSettings = mdImportSettings;
		markdownDocument = new MarkdownDocument();
		string text = reader.ReadToEnd();
		text = text.Replace("\r\n", "\n");
		text = text.Replace("\r", "\n");
		textlines = text.Split("\n".ToCharArray());
		totalLines = textlines.Length;
		while (HasNextLine)
		{
			if (!isSkip)
			{
				ReadLine();
			}
			if (!string.IsNullOrEmpty(currentLine))
			{
				isSkip = false;
				string line = currentLine.Trim("\r".ToCharArray());
				MdTable mdTable;
				if (!CheckCurrLineIsIndentedForList() && IsIndentedCodeBlock())
				{
					ParseIndentedCodeBlock();
				}
				else if (IsThematicBreak())
				{
					markdownDocument.AddMdThematicBreak();
				}
				else if (IsFencedCodeBlock())
				{
					ParseFencedCodeBlock();
				}
				else if (IsTable(line, out mdTable))
				{
					ParseTable(line, mdTable);
				}
				else
				{
					ParseParagraph();
				}
			}
		}
		Close();
	}

	private bool IsHyperlinkOrImage(string line, ref string displayText, ref string urlText, ref string screenTip, ref int endIndex)
	{
		string textWithinBracket = string.Empty;
		bool flag = false;
		endIndex = 0;
		int startIndex = 0;
		char[] array = line.ToCharArray();
		if (GetContentWithinBracket(line, startIndex, ref endIndex, '[', ']', ref displayText))
		{
			if (endIndex < line.Length - 1 && array[endIndex + 1] == '(')
			{
				flag = true;
			}
			if (flag && GetContentWithinBracket(line, endIndex + 1, ref endIndex, '(', ')', ref textWithinBracket))
			{
				if (!string.IsNullOrEmpty(textWithinBracket) && textWithinBracket.Contains(','.ToString()))
				{
					string[] array2 = SplitStringByCharacter(textWithinBracket, ',');
					if (array2.Length != 2)
					{
						return false;
					}
					array2[0] = array2[0].TrimStart();
					if (array2[0].Contains(' '.ToString()))
					{
						return false;
					}
					urlText = array2[0];
					if (array2[1].StartsWith(" "))
					{
						array2[1] = array2[1].Trim();
						if (!array2[1].StartsWith('"'.ToString()) || !array2[1].EndsWith('"'.ToString()))
						{
							return false;
						}
						screenTip = array2[1].Substring(1, array2[1].Length - 2);
					}
					else
					{
						urlText = textWithinBracket;
					}
				}
				else
				{
					urlText = textWithinBracket;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private bool GetContentWithinBracket(string text, int startIndex, ref int endIndex, char openingChar, char closingChar, ref string textWithinBracket)
	{
		Stack<char> stack = new Stack<char>();
		char[] array = text.ToCharArray();
		bool flag = true;
		for (int i = startIndex; i < array.Length; i++)
		{
			if (array[i] == openingChar)
			{
				stack.Push(openingChar);
				if (flag)
				{
					startIndex = i;
					flag = false;
				}
			}
			else if (array[i] == closingChar && array[i - 1] != '\\')
			{
				if (stack.Count == 0)
				{
					return false;
				}
				stack.Pop();
				if (stack.Count == 0)
				{
					endIndex = i;
					break;
				}
			}
		}
		if (stack.Count > 0)
		{
			return false;
		}
		textWithinBracket = text.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
		return true;
	}

	private bool IsFencedCodeBlock()
	{
		if (currentLine.TrimStart(' ').StartsWith("```"))
		{
			string text = currentLine.TrimStart(' ');
			string continuousChar = GetContinuousChar(text, '`');
			if (text.IndexOf('`', continuousChar.Length) == -1)
			{
				return true;
			}
		}
		else if (currentLine.TrimStart(' ').StartsWith("~~~"))
		{
			return true;
		}
		return false;
	}

	internal string ReadLine()
	{
		PreviousLine = currentLine;
		currLineIndex++;
		currentLine = (HasNextLine ? textlines[currLineIndex] : null);
		return currentLine;
	}

	internal void ResetPostionToPrevLine()
	{
		currLineIndex--;
		currentLine = (HasNextLine ? textlines[currLineIndex] : null);
		PreviousLine = ((currLineIndex > 0) ? textlines[currLineIndex - 1] : string.Empty);
	}

	internal void ParseIndentedCodeBlock()
	{
		MdCodeBlock mdCodeBlock = markdownDocument.AddMdCodeBlock();
		mdCodeBlock.Lines.Add(currentLine.Remove(0, 4));
		mdCodeBlock.IsFencedCode = false;
		ReadLine();
		while (currentLine != null && (currentLine.StartsWith("    ") || string.IsNullOrWhiteSpace(currentLine)))
		{
			if (string.IsNullOrWhiteSpace(currentLine))
			{
				mdCodeBlock.Lines.Add(currentLine);
			}
			else
			{
				mdCodeBlock.Lines.Add(currentLine.Remove(0, 4));
			}
			ReadLine();
		}
		isSkip = true;
		int num = mdCodeBlock.Lines.Count - 1;
		while (num >= 0 && string.IsNullOrWhiteSpace(mdCodeBlock.Lines[num]))
		{
			mdCodeBlock.Lines.RemoveAt(num);
			num--;
		}
	}

	internal void ParseFencedCodeBlock()
	{
		char c = (currentLine.TrimStart(' ').StartsWith("```") ? '`' : '~');
		string line = currentLine.TrimStart(' ');
		string continuousChar = GetContinuousChar(line, c);
		MdCodeBlock mdCodeBlock = markdownDocument.AddMdCodeBlock();
		ReadLine();
		while (currentLine != null && ((!(currentLine.TrimStart(' ') == continuousChar) && (!currentLine.TrimStart(' ').StartsWith(continuousChar) || !IsFencedCodeEnd(currentLine, c))) || currentLine.StartsWith("    ")))
		{
			mdCodeBlock.Lines.Add(currentLine);
			ReadLine();
		}
	}

	internal void ParseTable(string line, MdTable mdTable)
	{
		string text = line.Trim();
		int num = -1;
		do
		{
			string[] array = SplitStringByCharacter(text, '|');
			num++;
			MdTableRow mdTableRow = mdTable.AddMdTableRow();
			for (int i = 0; i < array.Length && i < mdTable.ColumnAlignments.Count; i++)
			{
				string text2 = array[i];
				MdTableCell mdTableCell = mdTableRow.AddMdTableCell();
				MdParagraph mdParagraph = new MdParagraph();
				ProcessLine(text2, mdParagraph);
				MoveItemstoCells(mdParagraph, mdTableCell, num == 0);
			}
			text = ReadLine();
			text = ((!string.IsNullOrEmpty(text)) ? text.Trim() : null);
		}
		while (!string.IsNullOrEmpty(text) && HasTableSyntax(text));
		if (string.IsNullOrEmpty(text))
		{
			markdownDocument.AddMdParagraph().AddMdTextRange().Text = string.Empty;
		}
		else if (!HasTableSyntax(text))
		{
			currLineIndex--;
		}
	}

	private void MoveItemstoCells(MdParagraph mdParagraph, MdTableCell mdTableCell, bool isHeaderRow)
	{
		for (int num = mdParagraph.Inlines.Count - 1; num >= 0; num--)
		{
			IMdInline mdInline = mdParagraph.Inlines[num];
			if (isHeaderRow)
			{
				if (mdInline is MdTextRange)
				{
					(mdInline as MdTextRange).TextFormat.Bold = true;
				}
				else if (mdInline is MdHyperlink)
				{
					(mdInline as MdHyperlink).TextFormat.Bold = true;
				}
			}
			mdTableCell.Items.Insert(0, mdInline);
			mdParagraph.Inlines.RemoveAt(num);
		}
	}

	private bool IsTable(string line, out MdTable mdTable)
	{
		string line2 = line.Trim();
		mdTable = null;
		if (HasTableSyntax(line2) && HasNextLine)
		{
			string text = ReadLine();
			text = ((!string.IsNullOrEmpty(text)) ? text.Trim() : null);
			if (!string.IsNullOrEmpty(text) && HasTableSyntax(text))
			{
				string[] array = SplitStringByCharacter(line2, '|');
				string[] array2 = SplitStringByCharacter(text, '|');
				if (array.Length == array2.Length)
				{
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = array2[i].Trim();
						string text2 = array2[i].Trim(':');
						if (text2.Contains('-'.ToString()))
						{
							text2 = text2.Replace('-'.ToString(), string.Empty);
							if (!string.IsNullOrEmpty(text2))
							{
								ResetPostionToPrevLine();
								return false;
							}
							continue;
						}
						ResetPostionToPrevLine();
						return false;
					}
					mdTable = markdownDocument.AddMdTable();
					string[] array3 = array2;
					foreach (string text3 in array3)
					{
						if (!string.IsNullOrEmpty(text3))
						{
							if (text3.StartsWith(":") && !text3.EndsWith(":"))
							{
								mdTable.ColumnAlignments.Add(ColumnAlignment.Left);
							}
							else if (!text3.StartsWith(":") && text3.EndsWith(":"))
							{
								mdTable.ColumnAlignments.Add(ColumnAlignment.Right);
							}
							else
							{
								mdTable.ColumnAlignments.Add(ColumnAlignment.Center);
							}
						}
					}
					return true;
				}
				ResetPostionToPrevLine();
				return false;
			}
			ResetPostionToPrevLine();
			return false;
		}
		return false;
	}

	private bool HasTableSyntax(string line)
	{
		if (!string.IsNullOrEmpty(line))
		{
			line = line.Trim();
			if (line.StartsWith('|'.ToString()))
			{
				return line.EndsWith('|'.ToString());
			}
			return false;
		}
		return false;
	}

	internal void ParseParagraph()
	{
		if (string.IsNullOrWhiteSpace(currentLine))
		{
			return;
		}
		if (IsComment())
		{
			MdTextRange mdTextRange = markdownDocument.AddMdParagraph().AddMdTextRange();
			mdTextRange.Text = currentLine;
			mdTextRange.TextFormat.IsHidden = true;
			return;
		}
		string currListValue = string.Empty;
		bool isNumberedList = false;
		string text = currentLine.TrimStart(' ');
		string continuousChar = GetContinuousChar(text, '#');
		if (!string.IsNullOrEmpty(continuousChar) && string.IsNullOrWhiteSpace(text.Trim('#')))
		{
			return;
		}
		MdParagraph mdParagraph = new MdParagraph();
		if (!string.IsNullOrEmpty(continuousChar) && continuousChar.Length <= 6 && continuousChar.Length < text.Trim().Length && string.IsNullOrWhiteSpace(text.Substring(continuousChar.Length, 1)) && !currentLine.StartsWith("    "))
		{
			mdParagraph.ApplyParagraphStyle("Heading " + continuousChar.Length, mdParagraph);
			text = text.Trim();
			string text2 = text.Split(' ')[^1];
			text = ((!(GetContinuousChar(text2, '#') == text2)) ? text.TrimStart('#').Trim() : text.Trim('#').Trim());
		}
		else if (IsListParagraph(text, ref currListValue, ref isNumberedList, checkConditionOnly: false))
		{
			mdParagraph.ListFormat = new MdListFormat();
			mdParagraph.ListFormat.ListLevel = listLevel;
			if (isNumberedList)
			{
				mdParagraph.ListFormat.NumberedListMarker = currListValue + ".";
			}
			mdParagraph.ListFormat.IsNumbered = isNumberedList;
			text = text.Substring(text.IndexOf(' ') + 1);
		}
		if (mdParagraph.StyleName == MdParagraphStyle.None && mdParagraph.ListFormat == null)
		{
			if (PreviousBlockItem is MdParagraph && (PreviousBlockItem as MdParagraph).StyleName == MdParagraphStyle.None && !string.IsNullOrWhiteSpace(PreviousLine))
			{
				MdParagraph mdParagraph2 = PreviousBlockItem as MdParagraph;
				MdTextRange mdTextRange2 = ((mdParagraph2.Inlines.Count > 0) ? (mdParagraph2.Inlines[mdParagraph2.Inlines.Count - 1] as MdTextRange) : null);
				if (mdTextRange2 != null && mdTextRange2.Text.Length > 0 && (mdTextRange2.Text.EndsWith("  ") || mdTextRange2.Text[mdTextRange2.Text.Length - 1] == '\\'))
				{
					if (mdTextRange2.Text[mdTextRange2.Text.Length - 1] == '\\')
					{
						mdTextRange2.Text = mdTextRange2.Text.Substring(0, mdTextRange2.Text.Length - 1);
					}
					markdownDocument.Blocks.Add(mdParagraph);
				}
				else
				{
					mdParagraph = mdParagraph2;
					if (mdTextRange2 != null)
					{
						mdTextRange2.Text = (mdTextRange2.Text.EndsWith(' '.ToString()) ? mdTextRange2.Text : (mdTextRange2.Text + ' '));
					}
					else
					{
						mdParagraph.AddMdTextRange().Text = ' '.ToString();
					}
				}
			}
			else
			{
				markdownDocument.Blocks.Add(mdParagraph);
			}
		}
		else
		{
			markdownDocument.Blocks.Add(mdParagraph);
		}
		ProcessLine(text, mdParagraph);
	}

	private void ProcessLine(string currentLine, MdParagraph mdParagraph)
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		StringBuilder currentText = new StringBuilder();
		int i = 0;
		char c = '\0';
		for (; i < currentLine.Length; i++)
		{
			char c2 = c;
			c = currentLine[i];
			char c3 = ((i < currentLine.Length - 1) ? currentLine[i + 1] : '\0');
			if (c2 != 0 && c2 == '\\')
			{
				currentText.Append(c);
				continue;
			}
			switch (c)
			{
			case '*':
			case '`':
			case '~':
			{
				string text = c.ToString();
				if (c3 != 0 && c.ToString() + c3 == "**")
				{
					if (list.Count > 1)
					{
						int num = list.IndexOf(new KeyValuePair<string, string>("**", "Opener"));
						int num2 = ((num > -1) ? list.IndexOf(new KeyValuePair<string, string>('*'.ToString(), "Opener")) : (-1));
						if (num == -1 || num2 == -1 || num2 != num + 1)
						{
							text = "**";
							i++;
						}
					}
					else
					{
						text = "**";
						i++;
					}
				}
				else if (c3 != 0 && c.ToString() + c3 == "~~")
				{
					text = "~~";
					i++;
				}
				int num3 = list.IndexOf(new KeyValuePair<string, string>(text, "Opener"));
				if (num3 > -1)
				{
					AddAsLiteralText(list, ref currentText);
					CloseDelimiter(list, num3, text, text);
				}
				else
				{
					AddAsLiteralText(list, ref currentText);
					list.Add(new KeyValuePair<string, string>(text, "Opener"));
				}
				continue;
			}
			case '[':
				ProcessLinkOrImageSyntax(ref i, currentLine, list, ref currentText, c, isImage: false);
				continue;
			case '!':
				if (c3 == '[')
				{
					ProcessLinkOrImageSyntax(ref i, currentLine, list, ref currentText, c, isImage: true);
					continue;
				}
				break;
			}
			if (c == '<')
			{
				int endIndex = 0;
				string textWithinBracket = string.Empty;
				if (GetContentWithinBracket(currentLine.Substring(i), 0, ref endIndex, '<', '>', ref textWithinBracket))
				{
					switch (textWithinBracket)
					{
					case "sub":
					case "sup":
						AddAsLiteralText(list, ref currentText);
						list.Add(new KeyValuePair<string, string>("<" + textWithinBracket + ">", "Opener"));
						i += endIndex;
						break;
					case "/sub":
						CloseScript(list, ref currentText, ref i, c, endIndex, "<sub>", "</sub>");
						break;
					case "/sup":
						CloseScript(list, ref currentText, ref i, c, endIndex, "<sup>", "</sup>");
						break;
					default:
						currentText.Append(c);
						break;
					}
				}
				else
				{
					currentText.Append(c);
				}
			}
			else
			{
				currentText.Append(c);
			}
		}
		AddAsLiteralText(list, ref currentText);
		ConvertTextToInlineItems(list, mdParagraph);
	}

	private void CloseScript(List<KeyValuePair<string, string>> splittedText, ref StringBuilder currentText, ref int currentPos, char currCharacter, int endIndex, string openDelimiter, string closeDelimiter)
	{
		int num = splittedText.IndexOf(new KeyValuePair<string, string>(openDelimiter, "Opener"));
		if (num > -1)
		{
			AddAsLiteralText(splittedText, ref currentText);
			CloseDelimiter(splittedText, num, openDelimiter, closeDelimiter);
			currentPos += endIndex;
		}
		else
		{
			currentText.Append(currCharacter);
		}
	}

	private void ProcessLinkOrImageSyntax(ref int currentPos, string currentLine, List<KeyValuePair<string, string>> splittedText, ref StringBuilder currentText, char currCharacter, bool isImage)
	{
		string text = currentLine.Substring(currentPos);
		int currentPos2 = 0;
		if (HasBasicHyperlinkOrImageSyntax(text, currentPos2))
		{
			string displayText = string.Empty;
			string urlText = string.Empty;
			string screenTip = string.Empty;
			int endIndex = 0;
			if (IsHyperlinkOrImage(text, ref displayText, ref urlText, ref screenTip, ref endIndex))
			{
				currentPos += endIndex;
				AddAsLiteralText(splittedText, ref currentText);
				if (isImage)
				{
					AddImageSyntax(splittedText, displayText, urlText);
				}
				else
				{
					AddHyperlinkSyntax(splittedText, displayText, urlText, screenTip);
				}
			}
		}
		else
		{
			currentText.Append(currCharacter);
		}
	}

	private void AddHyperlinkSyntax(List<KeyValuePair<string, string>> splittedText, string displayText, string urlLink, string screenTip)
	{
		splittedText.Add(new KeyValuePair<string, string>("Hyperlink", "Start"));
		splittedText.Add(new KeyValuePair<string, string>("DisplayText", displayText));
		splittedText.Add(new KeyValuePair<string, string>("Url", urlLink));
		splittedText.Add(new KeyValuePair<string, string>("ScreenTip", screenTip));
		splittedText.Add(new KeyValuePair<string, string>("Hyperlink", "End"));
	}

	private void AddImageSyntax(List<KeyValuePair<string, string>> splittedText, string altText, string src)
	{
		splittedText.Add(new KeyValuePair<string, string>("Img", "Start"));
		splittedText.Add(new KeyValuePair<string, string>("AltText", altText));
		if (src.StartsWith('"'.ToString()) && src.EndsWith('"'.ToString()))
		{
			src = src.Substring(1, src.Length - 2);
		}
		splittedText.Add(new KeyValuePair<string, string>("Src", src));
		splittedText.Add(new KeyValuePair<string, string>("Img", "End"));
	}

	private bool HasBasicHyperlinkOrImageSyntax(string text, int currentPos)
	{
		int num = ((currentPos < text.Length) ? text.IndexOf(']', currentPos) : (-1));
		int num2 = ((num > currentPos) ? text.IndexOf('(', num) : (-1));
		return ((num2 > num) ? text.IndexOf(')', num2) : (-1)) > -1;
	}

	private void AddAsLiteralText(List<KeyValuePair<string, string>> splittedText, ref StringBuilder currentText)
	{
		if (currentText.Length != 0)
		{
			splittedText.Add(new KeyValuePair<string, string>("Text", currentText.ToString()));
			currentText = new StringBuilder();
		}
	}

	private void CloseDelimiter(List<KeyValuePair<string, string>> splittedText, int delimiterOpenIndex, string openDelimiter, string closeDelimiter)
	{
		for (int i = delimiterOpenIndex + 1; i < splittedText.Count; i++)
		{
			KeyValuePair<string, string> item = splittedText[i];
			if (IsDelimterOpener(item))
			{
				string key = item.Key;
				RemoveAndInsertKeyPair(splittedText, i, "Text", key);
			}
		}
		RemoveAndInsertKeyPair(splittedText, delimiterOpenIndex, openDelimiter, "Start");
		splittedText.Add(new KeyValuePair<string, string>(closeDelimiter, "End"));
	}

	private void RemoveAndInsertKeyPair(List<KeyValuePair<string, string>> splittedText, int indexToRemoveAndInsert, string newKey, string newValue)
	{
		splittedText.RemoveAt(indexToRemoveAndInsert);
		splittedText.Insert(indexToRemoveAndInsert, new KeyValuePair<string, string>(newKey, newValue));
	}

	private bool IsDelimterOpener(KeyValuePair<string, string> item)
	{
		if (item.Key == "**".ToString() || item.Key == '*'.ToString() || item.Key == "~~".ToString() || item.Key == "<sub>".ToString() || item.Key == "<sup>".ToString())
		{
			return item.Value == "Opener";
		}
		return false;
	}

	private byte[] GetImage(string src)
	{
		byte[] result = null;
		try
		{
			if (markdownImportSettings != null && markdownImportSettings.IsEventSubscribed)
			{
				MdImageNodeVisitedEventArgs mdImageNodeVisitedEventArgs = null;
				mdImageNodeVisitedEventArgs = markdownImportSettings.ExecuteImageNodeVisitedEvent(null, src);
				if (mdImageNodeVisitedEventArgs.ImageStream != null)
				{
					Stream imageStream = mdImageNodeVisitedEventArgs.ImageStream;
					MemoryStream memoryStream = new MemoryStream();
					imageStream.CopyTo(memoryStream);
					result = memoryStream.ToArray();
				}
			}
			else if (src.StartsWith("data:image/"))
			{
				int num = src.IndexOf(",");
				src = src.Substring(num + 1);
				result = new MemoryStream(Convert.FromBase64String(src)).ToArray();
			}
		}
		catch
		{
			result = null;
		}
		return result;
	}

	private bool IsComment()
	{
		if (currentLine.Trim().StartsWith("<!--"))
		{
			return currentLine.Trim().EndsWith("-->");
		}
		return false;
	}

	private string GetListValue(MdListFormat mdList)
	{
		return (mdList.IsNumbered ? (prevListMarker + ".") : prevListMarker) + " ";
	}

	internal bool IsListParagraph(string line, ref string currListValue, ref bool isNumberedList, bool checkConditionOnly)
	{
		bool flag = (line.StartsWith('-' + " ") && !line.StartsWith("- [x] ", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("- [ ] ", StringComparison.OrdinalIgnoreCase)) || line.StartsWith('+' + " ") || line.StartsWith('*' + " ");
		string continuousNumber = GetContinuousNumber(line);
		bool flag2 = !string.IsNullOrEmpty(continuousNumber) && line.StartsWith(continuousNumber + ". ");
		MdListFormat mdListFormat = ((PreviousBlockItem is MdParagraph && (PreviousBlockItem as MdParagraph).ListFormat != null) ? (PreviousBlockItem as MdParagraph).ListFormat : null);
		if (mdListFormat != null && (flag2 || flag))
		{
			string listValue = GetListValue(mdListFormat);
			string continuousChar = GetContinuousChar(currentLine, ' ');
			if (listValue.Length == continuousChar.Length)
			{
				if (checkConditionOnly)
				{
					return true;
				}
				if (listLevelSpace.Count < 9)
				{
					listLevel++;
					if (!listLevelSpace.ContainsKey(continuousChar.Length))
					{
						listLevelSpace.Add(continuousChar.Length, listLevel);
					}
				}
				currListValue = (flag2 ? "1" : '-'.ToString());
				isNumberedList = flag2;
				prevListMarker = continuousChar + currListValue;
				return true;
			}
			if (string.IsNullOrEmpty(continuousChar) || continuousChar.Length < listValue.Length)
			{
				if (checkConditionOnly)
				{
					return true;
				}
				if (listLevelSpace.ContainsKey(continuousChar.Length))
				{
					listLevel = listLevelSpace[continuousChar.Length];
				}
				else
				{
					listLevel = 0;
				}
				if (string.IsNullOrEmpty(startListNumber))
				{
					startListNumber = continuousNumber;
				}
				currListValue = ((!flag2) ? '-'.ToString() : ((listLevel > 0) ? "1" : startListNumber));
				if (listLevel == 0)
				{
					listLevelSpace.Clear();
					listLevelSpace.Add(0, 0);
				}
				isNumberedList = flag2;
				prevListMarker = continuousChar + (flag2 ? continuousNumber : '-'.ToString());
				return true;
			}
		}
		else if (!currentLine.StartsWith("    ") && (flag2 || flag))
		{
			if (checkConditionOnly)
			{
				return true;
			}
			listLevelSpace.Clear();
			listLevel = 0;
			currListValue = (flag2 ? continuousNumber : '-'.ToString());
			listLevelSpace.Add(0, 0);
			if (flag2)
			{
				startListNumber = currListValue;
			}
			isNumberedList = flag2;
			prevListMarker = currListValue;
			return true;
		}
		return false;
	}

	internal bool CheckCurrLineIsIndentedForList()
	{
		if (!string.IsNullOrWhiteSpace(currentLine) && currentLine.StartsWith("    ") && string.IsNullOrWhiteSpace(PreviousLine) && PreviousBlockItem is MdParagraph && (PreviousBlockItem as MdParagraph).ListFormat != null)
		{
			string currListValue = "";
			bool isNumberedList = false;
			if (IsListParagraph(currentLine.TrimStart(' '), ref currListValue, ref isNumberedList, checkConditionOnly: true))
			{
				return true;
			}
		}
		return false;
	}

	private string GetContinuousNumber(string line)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in line)
		{
			if (!char.IsDigit(c))
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	internal bool IsIndentedCodeBlock()
	{
		if (!currentLine.StartsWith("    ") || string.IsNullOrWhiteSpace(currentLine) || currLineIndex != 0)
		{
			if (currentLine.StartsWith("    ") && !string.IsNullOrWhiteSpace(currentLine))
			{
				if (!string.IsNullOrWhiteSpace(PreviousLine))
				{
					if (!(PreviousBlockItem is MdCodeBlock) || !(PreviousBlockItem as MdCodeBlock).IsFencedCode)
					{
						if (PreviousBlockItem is MdParagraph)
						{
							return (PreviousBlockItem as MdParagraph).StyleName != MdParagraphStyle.None;
						}
						return false;
					}
					return true;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal bool IsFencedCodeEnd(string currLine, char fencedCodeChar)
	{
		if (string.IsNullOrWhiteSpace(currLine.Trim(fencedCodeChar)))
		{
			return true;
		}
		return false;
	}

	internal string GetContinuousChar(string line, char startCharacter)
	{
		string text = "";
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c != startCharacter)
			{
				break;
			}
			text += c;
		}
		return text;
	}

	private string[] SplitStringByCharacter(string line, char searchCharacter)
	{
		if (line.Contains("\\" + searchCharacter))
		{
			char[] array = line.ToCharArray();
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0 && array[i] == searchCharacter && array[i - 1] != '\\')
				{
					list.Add(i);
				}
				else if (i == 0 && array[i] == searchCharacter)
				{
					list.Add(i);
				}
			}
			int num = 1;
			string[] array2 = new string[list.Count - 1];
			int num2 = 0;
			{
				foreach (int item in list)
				{
					if (item > 0)
					{
						int length = item - num;
						array2[num2] = line.Substring(num, length);
						num = item + 1;
						num2++;
					}
				}
				return array2;
			}
		}
		if (line[0] == searchCharacter && line[line.Length - 1] == searchCharacter)
		{
			line = line.Substring(1, line.Length - 2);
		}
		return line.Split(searchCharacter);
	}

	private void ConvertTextToInlineItems(List<KeyValuePair<string, string>> splittedText, MdParagraph mdParagraph)
	{
		m_styleStack = new Stack<MdTextFormat>();
		for (int i = 0; i < splittedText.Count; i++)
		{
			string text = splittedText[i].Key.ToLower();
			string text2 = splittedText[i].Value;
			switch (text)
			{
			case "text":
				if (mdParagraph.Inlines.Count == 0)
				{
					if (text2.TrimStart().StartsWith("- [x] ", StringComparison.OrdinalIgnoreCase))
					{
						text2 = text2.TrimStart();
						int num = text2.IndexOf("- [x] ", StringComparison.OrdinalIgnoreCase);
						text2 = text2.Substring(num + "- [x] ".Length);
						mdParagraph.TaskItemProperties = new MdTaskProperties();
						mdParagraph.TaskItemProperties.IsChecked = true;
					}
					else if (text2.TrimStart().StartsWith("- [ ] ", StringComparison.OrdinalIgnoreCase))
					{
						text2 = text2.TrimStart();
						int num2 = text2.IndexOf("- [ ] ", StringComparison.OrdinalIgnoreCase);
						text2 = text2.Substring(num2 + "- [ ] ".Length);
						mdParagraph.TaskItemProperties = new MdTaskProperties();
					}
				}
				AddMdTextRange(mdParagraph, text2);
				break;
			case "hyperlink":
			{
				bool flag = false;
				MdHyperlink mdHyperlink = mdParagraph.AddMdHyperlink();
				while (i < splittedText.Count && !flag)
				{
					text = splittedText[i].Key.ToLower();
					text2 = splittedText[i].Value;
					switch (text)
					{
					case "displaytext":
						mdHyperlink.DisplayText = text2;
						break;
					case "url":
						mdHyperlink.Url = text2;
						break;
					case "screentip":
						mdHyperlink.ScreenTip = text2;
						break;
					case "hyperlink":
						if (text2.ToLower() == "end")
						{
							flag = true;
						}
						break;
					}
					if (!flag)
					{
						i++;
					}
				}
				ApplyCurrentFormat(mdHyperlink.TextFormat);
				break;
			}
			case "img":
			{
				bool flag2 = false;
				MdPicture mdPicture = mdParagraph.AddPicture();
				while (i < splittedText.Count && !flag2)
				{
					text = splittedText[i].Key.ToLower();
					text2 = splittedText[i].Value;
					switch (text)
					{
					case "alttext":
						mdPicture.AltText = text2;
						break;
					case "src":
					{
						byte[] image = GetImage(text2);
						mdPicture.ImageBytes = image;
						break;
					}
					case "img":
						if (text2.ToLower() == "end")
						{
							flag2 = true;
						}
						break;
					}
					if (!flag2)
					{
						i++;
					}
				}
				break;
			}
			default:
				if (text2.ToLower() == "start")
				{
					ApplyNewStyleInStack(text);
				}
				else if (text2.ToLower() == "end")
				{
					m_styleStack.Pop();
				}
				else if (text2.ToLower() == "opener")
				{
					AddMdTextRange(mdParagraph, text);
				}
				break;
			}
		}
	}

	private void AddMdTextRange(MdParagraph mdParagraph, string text)
	{
		MdTextRange mdTextRange = mdParagraph.AddMdTextRange();
		ApplyCurrentFormat(mdTextRange.TextFormat);
		mdTextRange.Text = ReplaceSymbols(text);
	}

	private void ApplyCurrentFormat(MdTextFormat textFormat)
	{
		textFormat.Bold = CurrentFormat.Bold;
		textFormat.Italic = CurrentFormat.Italic;
		textFormat.CodeSpan = CurrentFormat.CodeSpan;
		textFormat.StrikeThrough = CurrentFormat.StrikeThrough;
		textFormat.SubSuperScriptType = CurrentFormat.SubSuperScriptType;
	}

	private void ApplyNewStyleInStack(string key)
	{
		MdTextFormat mdTextFormat = ((m_styleStack.Count > 0) ? m_styleStack.Peek().Clone() : new MdTextFormat());
		switch (key)
		{
		case "**":
			mdTextFormat.Bold = true;
			break;
		case "*":
			mdTextFormat.Italic = true;
			break;
		case "~~":
			mdTextFormat.StrikeThrough = true;
			break;
		case "<sup>":
			mdTextFormat.SubSuperScriptType = MdSubSuperScript.SuperScript;
			break;
		case "<sub>":
			mdTextFormat.SubSuperScriptType = MdSubSuperScript.SubScript;
			break;
		case "`":
			mdTextFormat.CodeSpan = true;
			break;
		case "<!--":
			mdTextFormat.IsHidden = true;
			break;
		}
		m_styleStack.Push(mdTextFormat);
	}

	private string ReplaceSymbols(string text)
	{
		char[] array = new char[12]
		{
			'#', '-', '*', '`', '~', '=', '+', '>', '<', '&',
			'[', '\\'
		};
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (text.Contains("\\" + c))
			{
				text = text.Replace("\\" + c, c.ToString());
			}
		}
		return text;
	}

	internal bool IsThematicBreak()
	{
		if (string.IsNullOrWhiteSpace(prevLine) || (PreviousBlockItem is MdParagraph && (PreviousBlockItem as MdParagraph).ListFormat != null) || currLineIndex == 0)
		{
			string text = currentLine.Replace(' '.ToString(), string.Empty);
			if (!string.IsNullOrEmpty(text) && text.StartsWith("---"))
			{
				return string.IsNullOrEmpty(text.Replace('-'.ToString(), string.Empty));
			}
		}
		return false;
	}

	internal void Close()
	{
		if (textlines != null)
		{
			textlines = null;
		}
		if (listLevelSpace != null)
		{
			listLevelSpace.Clear();
			listLevelSpace = null;
		}
		if (m_styleStack != null)
		{
			m_styleStack.Clear();
			m_styleStack = null;
		}
	}
}
