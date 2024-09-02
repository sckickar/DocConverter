using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Office.Markdown;

namespace DocGen.DocIO.DLS;

internal class MdToWordConversion
{
	private WordDocument wordDoc;

	private MarkdownDocument markdownDoc;

	private WListFormat currListFormat;

	internal void ConvertToWordDOM(MarkdownDocument markdownDocument, WordDocument document)
	{
		markdownDoc = markdownDocument;
		wordDoc = document;
		ConvertToWordDocument();
		Close();
	}

	private void ConvertToWordDocument()
	{
		wordDoc.AddSection();
		WSection lastSection = wordDoc.LastSection;
		lastSection.PageSetup.InitializeDocxPageSetup();
		int currLvlNumInWord = 0;
		bool isFirstListOccured = false;
		for (int i = 0; i < markdownDoc.Blocks.Count; i++)
		{
			IMdBlock mdBlock = markdownDoc.Blocks[i];
			if (mdBlock is MdCodeBlock)
			{
				MdCodeBlock mdCodeBlock = mdBlock as MdCodeBlock;
				if (i > 0 && markdownDoc.Blocks[i - 1] is MdCodeBlock)
				{
					WParagraph wParagraph = lastSection.AddParagraph() as WParagraph;
				}
				if (mdCodeBlock.Lines.Count == 0)
				{
					WParagraph wParagraph = lastSection.AddParagraph() as WParagraph;
					CreateFencedCodeStyle();
					wParagraph.ApplyStyle("FencedCode");
					continue;
				}
				foreach (string line in mdCodeBlock.Lines)
				{
					WParagraph wParagraph = lastSection.AddParagraph() as WParagraph;
					wParagraph.AppendText(line);
					if (mdCodeBlock.IsFencedCode)
					{
						ApplyCodeBlockStyle(wParagraph, "FencedCode");
					}
					else
					{
						ApplyCodeBlockStyle(wParagraph, "IndentedCode");
					}
				}
				continue;
			}
			if (mdBlock is MdTable)
			{
				WTable wTable = lastSection.AddTable() as WTable;
				ConvertMdTable(mdBlock as MdTable, wTable);
				continue;
			}
			if (mdBlock is MdThematicBreak)
			{
				Shape shape = (lastSection.AddParagraph() as WParagraph).AppendShape(AutoShapeType.Line, lastSection.PageSetup.ClientWidth, 0f);
				shape.IsHorizontalRule = true;
				shape.LineFormat.Color = Color.FromArgb(160, 160, 160);
				shape.LineFormat.Weight = 0.5f;
				shape.Height = 0.5f;
				shape.WrapFormat.TextWrappingStyle = TextWrappingStyle.Inline;
				continue;
			}
			MdParagraph mdParagraph = mdBlock as MdParagraph;
			WParagraph wParagraph2 = lastSection.AddParagraph() as WParagraph;
			if (mdParagraph != null && mdParagraph.Inlines.Count > 0)
			{
				ApplyListStyle(mdParagraph.ListFormat, wParagraph2, ref currLvlNumInWord, i, ref isFirstListOccured);
				ApplyParagraphStyle(mdParagraph.StyleName, wParagraph2);
				if (mdParagraph.TaskItemProperties != null)
				{
					wParagraph2.AppendInlineContentControl(ContentControlType.CheckBox).ContentControlProperties.IsChecked = mdParagraph.TaskItemProperties.IsChecked;
				}
				ConvertMdInlineItems(wParagraph2, mdParagraph.Inlines);
			}
		}
	}

	private void ApplyCodeBlockStyle(WParagraph paragraph, string paraStyleName)
	{
		if (wordDoc.Styles.FindByName(paraStyleName, StyleType.ParagraphStyle) == null)
		{
			wordDoc.AddParagraphStyle(paraStyleName).ParagraphFormat.BackColor = Color.FromArgb(226, 226, 226);
		}
		paragraph.ApplyStyle(paraStyleName);
	}

	private void ApplyListStyle(MdListFormat mdListFormat, WParagraph paragraph, ref int currLvlNumInWord, int i, ref bool isFirstListOccured)
	{
		if (mdListFormat != null)
		{
			int listLevel = mdListFormat.ListLevel;
			if (i == 0 || (i > 0 && !(markdownDoc.Blocks[i - 1] is MdParagraph)) || currListFormat == null || (i > 0 && markdownDoc.Blocks[i - 1] is MdParagraph && (markdownDoc.Blocks[i - 1] as MdParagraph).ListFormat == null))
			{
				if (mdListFormat.IsNumbered)
				{
					paragraph.ListFormat.ApplyDefNumberedStyle();
				}
				else
				{
					paragraph.ListFormat.ApplyDefBulletStyle();
				}
				currListFormat = paragraph.ListFormat;
				if (isFirstListOccured)
				{
					currListFormat.RestartNumbering = true;
				}
				isFirstListOccured = true;
			}
			if (currListFormat != null)
			{
				paragraph.ListFormat.ApplyStyle(currListFormat.CurrentListStyle.Name);
			}
			paragraph.ParagraphFormat.LeftIndent = listLevel * 36;
			paragraph.ParagraphFormat.FirstLineIndent = 5f;
			if (listLevel > currLvlNumInWord)
			{
				paragraph.ListFormat.IncreaseIndentLevel();
				currLvlNumInWord++;
			}
			else
			{
				while (listLevel < currLvlNumInWord)
				{
					paragraph.ListFormat.DecreaseIndentLevel();
					currLvlNumInWord--;
				}
			}
			paragraph.ListFormat.ListLevelNumber = currLvlNumInWord;
			if (mdListFormat.IsNumbered)
			{
				paragraph.ListFormat.CurrentListLevel.PatternType = GetListPatternType(currLvlNumInWord);
				paragraph.ListFormat.CurrentListLevel.StartAt = Convert.ToInt32(mdListFormat.NumberedListMarker.Trim().TrimEnd('.'));
				return;
			}
			paragraph.ListFormat.CurrentListLevel.PatternType = ListPatternType.Bullet;
			if (currLvlNumInWord > 1)
			{
				paragraph.ListFormat.CurrentListLevel.BulletCharacter = "\uf0a7";
				paragraph.ListFormat.CurrentListLevel.CharacterFormat.FontName = "Wingdings";
			}
		}
		else
		{
			currLvlNumInWord = 0;
			currListFormat = null;
		}
	}

	private void ConvertMdInlineItems(WParagraph wParagraph, List<IMdInline> mdInineItems)
	{
		foreach (IMdInline mdInineItem in mdInineItems)
		{
			if (mdInineItem is MdHyperlink)
			{
				ConvertMdHyperlink(mdInineItem as MdHyperlink, wParagraph);
			}
			else if (mdInineItem is MdPicture)
			{
				ConvertMdPicture(mdInineItem as MdPicture, wParagraph);
			}
			else if (mdInineItem is MdTextRange)
			{
				wParagraph.Items.Add(ConvertMdTextRangeToWTextRange(mdInineItem as MdTextRange));
			}
		}
	}

	private void ConvertMdHyperlink(MdHyperlink mdHyperlink, WParagraph wParagraph)
	{
		IWField iWField = wParagraph.AppendHyperlink(mdHyperlink.Url, mdHyperlink.DisplayText, HyperlinkType.WebLink);
		if (!string.IsNullOrEmpty(mdHyperlink.ScreenTip))
		{
			(iWField as WField).ScreenTip = mdHyperlink.ScreenTip;
		}
	}

	private void ConvertMdPicture(MdPicture mdPicture, WParagraph wParagraph)
	{
		byte[] array = null;
		if (mdPicture.ImageBytes == null)
		{
			Stream manifestResourceStream = WPicture.GetManifestResourceStream("ImageNotFound.jpg");
			MemoryStream memoryStream = new MemoryStream();
			manifestResourceStream.CopyTo(memoryStream);
			array = memoryStream.ToArray();
		}
		else
		{
			array = mdPicture.ImageBytes;
		}
		WPicture obj = wParagraph.AppendPicture(array) as WPicture;
		obj.AlternativeText = mdPicture.AltText;
		obj.Height = (float)((double)obj.Image.Height * 0.75);
		obj.Width = (float)((double)obj.Image.Width * 0.75);
	}

	private void ConvertMdTable(MdTable mdTable, WTable wTable)
	{
		foreach (MdTableRow row in mdTable.Rows)
		{
			WTableRow wTableRow = new WTableRow(wordDoc);
			wTable.Rows.Add(wTableRow);
			for (int i = 0; i < row.Cells.Count; i++)
			{
				MdTableCell mdTableCell = row.Cells[i];
				WTableCell wTableCell = wTableRow.AddCell();
				wTableCell.CellFormat.Borders.BorderType = BorderStyle.Single;
				WParagraph wParagraph = wTableCell.AddParagraph() as WParagraph;
				wParagraph.ParagraphFormat.HorizontalAlignment = GetColumnAlignment(mdTable.ColumnAlignments[i]);
				ConvertMdInlineItems(wParagraph, mdTableCell.Items);
			}
			if (wTableRow.Cells.Count < mdTable.ColumnAlignments.Count)
			{
				int num = mdTable.ColumnAlignments.Count - wTableRow.Cells.Count;
				for (int j = 0; j < num; j++)
				{
					wTableRow.AddCell();
				}
			}
		}
	}

	private WTextRange ConvertMdTextRangeToWTextRange(MdTextRange mdTextRange)
	{
		WTextRange wTextRange = new WTextRange(wordDoc);
		wTextRange.Text = GetProcessedText(mdTextRange.Text, '|'.ToString());
		wTextRange.CharacterFormat.Bold = mdTextRange.TextFormat.Bold;
		wTextRange.CharacterFormat.Italic = mdTextRange.TextFormat.Italic;
		wTextRange.CharacterFormat.Strikeout = mdTextRange.TextFormat.StrikeThrough;
		wTextRange.CharacterFormat.SubSuperScript = (SubSuperScript)mdTextRange.TextFormat.SubSuperScriptType;
		wTextRange.CharacterFormat.Hidden = mdTextRange.TextFormat.IsHidden;
		if (mdTextRange.TextFormat.CodeSpan)
		{
			if (wordDoc.Styles.FindByName("InlineCode", StyleType.CharacterStyle) == null)
			{
				(wordDoc.AddCharacterStyle("InlineCode") as WCharacterStyle).CharacterFormat.TextBackgroundColor = Color.FromArgb(226, 226, 226);
			}
			wTextRange.CharacterFormat.CharStyleName = "InlineCode";
		}
		return wTextRange;
	}

	private string GetProcessedText(string text, string replaceCharacter)
	{
		if (text.Contains("\\" + replaceCharacter))
		{
			text = ((!text.Contains("\\\\" + replaceCharacter)) ? text.Replace("\\" + replaceCharacter, replaceCharacter) : text.Replace("\\\\" + replaceCharacter, replaceCharacter));
		}
		return text;
	}

	private HorizontalAlignment GetColumnAlignment(ColumnAlignment columnAlignment)
	{
		return columnAlignment switch
		{
			ColumnAlignment.Left => HorizontalAlignment.Left, 
			ColumnAlignment.Right => HorizontalAlignment.Right, 
			_ => HorizontalAlignment.Center, 
		};
	}

	private void CreateFencedCodeStyle()
	{
		if (wordDoc.Styles.FindByName("FencedCode", StyleType.ParagraphStyle) == null)
		{
			wordDoc.AddParagraphStyle("FencedCode").ParagraphFormat.BackColor = Color.FromArgb(226, 226, 226);
		}
	}

	internal void ApplyParagraphStyle(MdParagraphStyle mdStyleName, WParagraph paragraph)
	{
		switch (mdStyleName)
		{
		case MdParagraphStyle.Heading1:
			paragraph.ApplyStyle(BuiltinStyle.Heading1);
			break;
		case MdParagraphStyle.Heading2:
			paragraph.ApplyStyle(BuiltinStyle.Heading2);
			break;
		case MdParagraphStyle.Heading3:
			paragraph.ApplyStyle(BuiltinStyle.Heading3);
			break;
		case MdParagraphStyle.Heading4:
			paragraph.ApplyStyle(BuiltinStyle.Heading4);
			break;
		case MdParagraphStyle.Heading5:
			paragraph.ApplyStyle(BuiltinStyle.Heading5);
			break;
		case MdParagraphStyle.Heading6:
			paragraph.ApplyStyle(BuiltinStyle.Heading6);
			break;
		default:
			paragraph.ApplyStyle(BuiltinStyle.Normal);
			break;
		}
	}

	internal ListPatternType GetListPatternType(int currentListLevel)
	{
		return currentListLevel switch
		{
			0 => ListPatternType.Arabic, 
			1 => ListPatternType.LowRoman, 
			_ => ListPatternType.LowLetter, 
		};
	}

	internal void Close()
	{
		if (currListFormat != null)
		{
			currListFormat = null;
		}
	}
}
