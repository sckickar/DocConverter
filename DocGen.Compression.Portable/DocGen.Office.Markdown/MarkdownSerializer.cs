using System;
using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MarkdownSerializer
{
	private MdCodeBlock codeBlock;

	private const char codeSpanChar = '`';

	private const char cellMarkerChar = '|';

	private const char backSlashChar = '\\';

	private string punctuation = "`~!@#$%^&*()_-+=|\\}{][:<>?';./,\"";

	private char[] symbols = new char[12]
	{
		'#', '-', '*', '`', '~', '=', '+', '>', '<', '&',
		'[', '\\'
	};

	private readonly string CrLf = "\r\n";

	private readonly char SpaceChar = ' ';

	internal string SerializeMarkdown(MarkdownDocument markdownDoc)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (IMdBlock block in markdownDoc.Blocks)
		{
			if (block is MdParagraph)
			{
				MdParagraph mdParagraph = block as MdParagraph;
				if (mdParagraph.Inlines.Count == 0)
				{
					text2 += CrLf;
				}
				else if (!string.IsNullOrEmpty(text2))
				{
					text2 = text2 + CrLf + CrLf;
				}
				if (mdParagraph.ListFormat != null)
				{
					text2 += mdParagraph.ListFormat.ListValue;
				}
				text = mdParagraph.GetCharForParaStyle(mdParagraph, text);
				text2 += text;
				text = string.Empty;
				if (mdParagraph.TaskItemProperties != null)
				{
					text = ((!mdParagraph.TaskItemProperties.IsChecked) ? mdParagraph.TaskItemProperties.Uncheckedmarker : mdParagraph.TaskItemProperties.CheckedMarker);
					text2 += text;
					text = string.Empty;
				}
				text = IterateInlineItems(mdParagraph.Inlines, text);
				text2 += text;
				text = string.Empty;
				if (((markdownDoc.Blocks.IndexOf(mdParagraph) + 1 < markdownDoc.Blocks.Count) ? markdownDoc.Blocks[markdownDoc.Blocks.IndexOf(mdParagraph) + 1] : null) is MdTable && mdParagraph.ListFormat != null)
				{
					text2 += CrLf;
				}
			}
			if (block is MdTable)
			{
				MdTable table = block as MdTable;
				text2 = ((text2 == string.Empty) ? (text2 + "|") : (text2 + CrLf + "|"));
				text = WriteTable(table, text, text2);
				text2 = text2 + text + CrLf;
				text = string.Empty;
			}
			if (block is MdThematicBreak)
			{
				MdThematicBreak mdThematicBreak = new MdThematicBreak();
				if (!text2.EndsWith(CrLf + CrLf))
				{
					text2 = ((!text2.EndsWith(CrLf)) ? (text2 + CrLf + CrLf) : (text2 + CrLf));
				}
				text2 += mdThematicBreak.HorizontalRuleChar;
			}
			if (!(block is MdCodeBlock))
			{
				continue;
			}
			codeBlock = block as MdCodeBlock;
			if (codeBlock.Lines.Count <= 0)
			{
				continue;
			}
			if (text2 != string.Empty)
			{
				text2 += CrLf;
			}
			if (codeBlock.IsFencedCode)
			{
				foreach (string line in codeBlock.Lines)
				{
					if (line != string.Empty)
					{
						text = text + line + CrLf;
					}
				}
				text = "```" + CrLf + text + "```";
				text2 += text;
				text = string.Empty;
				continue;
			}
			text += CrLf;
			foreach (string line2 in codeBlock.Lines)
			{
				if (line2 != string.Empty)
				{
					text = text + "    " + line2 + CrLf;
				}
			}
			text2 += text;
			text = string.Empty;
		}
		return text2;
	}

	private string WriteTable(MdTable table, string localText, string m_text)
	{
		string text = string.Empty;
		for (int i = 0; i < table.Rows.Count; i++)
		{
			MdTableRow mdTableRow = table.Rows[i];
			if (i == 0)
			{
				foreach (MdTableCell cell in mdTableRow.Cells)
				{
					localText += IterateInlineItems(cell.Items, localText);
					text = text + localText + "|";
					localText = string.Empty;
				}
				text += CrLf;
				localText = string.Empty;
				for (int j = 0; j < table.ColumnAlignments.Count; j++)
				{
					if (table.ColumnAlignments[j] == ColumnAlignment.Left)
					{
						localText += "|:---";
					}
					if (table.ColumnAlignments[j] == ColumnAlignment.Center)
					{
						localText += "|:---:";
					}
					if (table.ColumnAlignments[j] == ColumnAlignment.Right)
					{
						localText += "|---:";
					}
				}
				text = text + localText + "|";
				localText = string.Empty;
				continue;
			}
			text = text + CrLf + "|";
			localText = string.Empty;
			foreach (MdTableCell cell2 in mdTableRow.Cells)
			{
				localText += IterateInlineItems(cell2.Items, localText);
				text = text + localText + "|";
				localText = string.Empty;
			}
		}
		return text;
	}

	private string IterateInlineItems(List<IMdInline> inlines, string localText)
	{
		string m_text = localText;
		localText = string.Empty;
		for (int i = 0; i < inlines.Count; i++)
		{
			IMdInline mdInline = inlines[i];
			if (mdInline is MdTextRange)
			{
				MdTextRange mdTextRange = mdInline as MdTextRange;
				localText = mdTextRange.Text;
				string emptySpace = string.Empty;
				if (localText != string.Empty && localText != "\t")
				{
					MdTextRange nextSibiling = ((i + 1 < inlines.Count && inlines[i + 1] is MdTextRange) ? (inlines[i + 1] as MdTextRange) : null);
					if (!string.IsNullOrEmpty(mdTextRange.Text))
					{
						ValidateDelimiters(ref m_text, ref emptySpace, ref localText, nextSibiling);
					}
					if (mdTextRange.TextFormat.CodeSpan)
					{
						localText = "`" + localText + "`";
					}
					if (mdTextRange.TextFormat.Bold)
					{
						localText = "**" + localText + "**";
					}
					if (mdTextRange.TextFormat.Italic)
					{
						localText = "*" + localText + "*";
					}
					if (mdTextRange.TextFormat.StrikeThrough)
					{
						localText = "~~" + localText + "~~";
					}
					if (mdTextRange.TextFormat.IsHidden)
					{
						localText = "<!--" + localText + "-->";
					}
					if (mdTextRange.IsLineBreak)
					{
						localText = SpaceChar + "\\" + CrLf;
					}
					switch (mdTextRange.TextFormat.SubSuperScriptType)
					{
					case MdSubSuperScript.SuperScript:
						localText = "<sup>" + localText + "</sup>";
						break;
					case MdSubSuperScript.SubScript:
						localText = "<sub>" + localText + "</sub>";
						break;
					}
				}
				m_text = m_text + localText + emptySpace;
				localText = string.Empty;
			}
			else if (mdInline is MdHyperlink)
			{
				MdHyperlink mdHyperlink = mdInline as MdHyperlink;
				localText = "[" + mdHyperlink.DisplayText + "](" + mdHyperlink.Url + ")";
				m_text += localText;
				localText = string.Empty;
			}
			else if (mdInline is MdPicture)
			{
				MdPicture mdPicture = mdInline as MdPicture;
				localText = "![" + ((mdPicture.AltText != null) ? mdPicture.AltText : "Picture") + "]";
				if (!string.IsNullOrEmpty(mdPicture.Url))
				{
					localText = localText + "(" + mdPicture.Url + ")";
				}
				else
				{
					string text = Convert.ToBase64String(mdPicture.ImageBytes);
					localText = localText + "(data:image/" + ((!string.IsNullOrEmpty(mdPicture.ImageFormat)) ? mdPicture.ImageFormat : "png") + ";base64," + text + ")";
				}
				m_text += localText;
				localText = string.Empty;
			}
		}
		while (m_text != string.Empty && m_text[0] == '\t')
		{
			m_text = m_text.Remove(0, 1);
		}
		return m_text;
	}

	private void ValidateDelimiters(ref string m_text, ref string emptySpace, ref string text, MdTextRange nextSibiling)
	{
		char c = text[0];
		char c2 = text[text.Length - 1];
		char c3 = ((m_text != string.Empty) ? m_text[m_text.Length - 1] : '\0');
		if (punctuation.Contains(c.ToString()) && m_text != string.Empty && c3 != ' ' && c3 != '\t' && !punctuation.Contains(c3.ToString()))
		{
			m_text += " ";
		}
		while (text != string.Empty && (text[0] == ' ' || text[0] == '\t'))
		{
			m_text += text[0];
			text = text.Remove(0, 1);
		}
		if (punctuation.Contains(c2.ToString()) && nextSibiling != null && nextSibiling.Text != string.Empty && !nextSibiling.IsLineBreak && nextSibiling.Text[0] != ' ' && nextSibiling.Text[0] != '\t' && !punctuation.Contains(nextSibiling.Text[0].ToString()))
		{
			emptySpace += " ";
		}
		while (text != string.Empty && (text[text.Length - 1] == ' ' || text[text.Length - 1] == '\t'))
		{
			emptySpace += text[text.Length - 1];
			text = text.Remove(text.Length - 1, 1);
		}
		string text2 = string.Empty;
		while (text.IndexOfAny(symbols) != -1)
		{
			int num = text.IndexOfAny(symbols);
			text2 = text2 + text.Substring(0, num) + "\\" + text[num];
			text = text.Remove(0, num + 1);
		}
		text = text2 + text;
	}
}
