using System;
using System.IO;
using DocGen.Office.Markdown;

namespace DocGen.DocIO.DLS;

internal class WordToMdConversion
{
	private MarkdownDocument markdownDocument;

	private MdCodeBlock codeBlock;

	private bool isFieldEnd;

	private bool isFieldSeparator;

	private bool isHyperlink;

	private WFieldMark seperator;

	private int imageCount;

	internal string filePath = string.Empty;

	internal MarkdownDocument GetMarkdownText(WordDocument document)
	{
		markdownDocument = new MarkdownDocument();
		foreach (WSection section in document.Sections)
		{
			IterateBody(section.Body);
		}
		return markdownDocument;
	}

	private void IterateBody(ITextBody body)
	{
		TextBodyItem textBodyItem = null;
		MdCodeBlock mdCodeBlock = null;
		for (int i = 0; i < body.ChildEntities.Count; i++)
		{
			textBodyItem = body.ChildEntities[i] as TextBodyItem;
			switch (textBodyItem.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				mdCodeBlock = ((mdCodeBlock == null) ? IterateParagraph(wParagraph) : (((!mdCodeBlock.IsFencedCode || !(wParagraph.StyleName == "FencedCode")) && (mdCodeBlock.IsFencedCode || !(wParagraph.StyleName == "IndentedCode"))) ? IterateParagraph(wParagraph) : ConvertAsCodeBlock(wParagraph, mdCodeBlock)));
				break;
			}
			case EntityType.Table:
			{
				if (isFieldSeparator)
				{
					break;
				}
				MdTable mdTable = markdownDocument.AddMdTable();
				WTable wTable = (WTable)textBodyItem;
				for (int j = 0; j < wTable.Rows.Count; j++)
				{
					WTableRow row = wTable.Rows[j];
					if (j == 0)
					{
						SetTableAlignment(row, mdTable);
					}
					MdTableRow mdRow = mdTable.AddMdTableRow();
					IterateTableRow(mdRow, row);
				}
				break;
			}
			case EntityType.BlockContentControl:
				IterateBody((textBodyItem as IBlockContentControl).TextBody);
				break;
			}
		}
	}

	private MdCodeBlock IterateParagraph(WParagraph paragraph)
	{
		if (paragraph.StyleName == "FencedCode" || paragraph.StyleName == "IndentedCode")
		{
			return ConvertAsCodeBlock(paragraph, null);
		}
		MdParagraph mdParagraph = markdownDocument.AddMdParagraph();
		if (!paragraph.IsInCell)
		{
			mdParagraph.ApplyParagraphStyle(paragraph.StyleName, mdParagraph);
			ApplyListFormat(mdParagraph, paragraph);
		}
		IterateParagraphItems(paragraph.ChildEntities as ParagraphItemCollection, mdParagraph);
		return null;
	}

	private void IterateParagraphItems(ParagraphItemCollection paragraphItems, MdParagraph mdParagraph)
	{
		for (int i = 0; i < paragraphItems.Count; i++)
		{
			IParagraphItem paragraphItem = paragraphItems[i];
			if (isFieldEnd && paragraphItem is WFieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				isFieldEnd = false;
			}
			if (isFieldEnd)
			{
				continue;
			}
			switch (paragraphItem.EntityType)
			{
			case EntityType.TextRange:
			{
				if (isFieldSeparator || isHyperlink)
				{
					break;
				}
				if (paragraphItem.Owner is InlineContentControl && (paragraphItem.Owner as InlineContentControl).ContentControlProperties.Type == ContentControlType.CheckBox)
				{
					if (mdParagraph.Inlines.Count == 0)
					{
						mdParagraph.TaskItemProperties = new MdTaskProperties();
						mdParagraph.TaskItemProperties.IsChecked = (paragraphItem.Owner as InlineContentControl).ContentControlProperties.IsChecked;
					}
					break;
				}
				WTextRange wTextRange = paragraphItem as WTextRange;
				if (mdParagraph.Inlines.Count > 0 && mdParagraph.Inlines[mdParagraph.Inlines.Count - 1] is MdTextRange && (mdParagraph.Inlines[mdParagraph.Inlines.Count - 1] as MdTextRange).Text != null)
				{
					MdTextRange mdTextRange = new MdTextRange();
					ApplyTextFormat(wTextRange.CharacterFormat, wTextRange.Text, mdTextRange);
					if (CheckFormat(mdTextRange, mdParagraph.Inlines[mdParagraph.Inlines.Count - 1] as MdTextRange))
					{
						(mdParagraph.Inlines[mdParagraph.Inlines.Count - 1] as MdTextRange).Text += wTextRange.Text;
						mdTextRange = null;
					}
					else
					{
						mdParagraph.Inlines.Add(mdTextRange);
					}
				}
				else
				{
					MdTextRange mdTextRange2 = mdParagraph.AddMdTextRange();
					ApplyTextFormat((paragraphItem as WTextRange).CharacterFormat, (paragraphItem as WTextRange).Text, mdTextRange2);
				}
				break;
			}
			case EntityType.AutoShape:
			{
				Shape shape = paragraphItem as Shape;
				if (shape.IsHorizontalRule || (shape.AutoShapeType == AutoShapeType.Line && shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline))
				{
					markdownDocument.AddMdThematicBreak();
				}
				break;
			}
			case EntityType.Field:
			{
				WField wField = paragraphItem as WField;
				switch (wField.FieldType)
				{
				case FieldType.FieldHyperlink:
				{
					Hyperlink hyperlink = new Hyperlink(wField);
					if (hyperlink != null)
					{
						MdHyperlink mdHyperlink = mdParagraph.AddMdHyperlink();
						isHyperlink = IterateFieldHyperlink(hyperlink, mdHyperlink);
					}
					if (seperator == null)
					{
						seperator = wField.FieldSeparator;
					}
					break;
				}
				case FieldType.FieldIf:
					if (seperator == null)
					{
						seperator = wField.FieldSeparator;
					}
					if (wField.FieldEnd != null)
					{
						isFieldSeparator = true;
					}
					break;
				}
				break;
			}
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
				if (paragraphItem is WDropDownFormField)
				{
					MdTextRange mdTextRange3 = mdParagraph.AddMdTextRange();
					ApplyTextFormat((paragraphItem as WTextRange).CharacterFormat, (paragraphItem as WTextRange).Text, mdTextRange3);
					mdTextRange3.Text = (paragraphItem as WDropDownFormField).DropDownValue;
				}
				if (seperator == null)
				{
					seperator = (paragraphItem as WFormField).FieldSeparator;
				}
				if ((paragraphItem as WFormField).FieldEnd != null)
				{
					isFieldSeparator = true;
				}
				break;
			case EntityType.FieldMark:
				if ((paragraphItem as WFieldMark).Type == FieldMarkType.FieldEnd)
				{
					isHyperlink = false;
				}
				if (paragraphItem == seperator)
				{
					seperator = null;
					isFieldSeparator = false;
				}
				else if (seperator == null)
				{
					isFieldSeparator = false;
				}
				break;
			case EntityType.Break:
				if ((paragraphItem as Break).BreakType == BreakType.TextWrappingBreak || (paragraphItem as Break).BreakType == BreakType.LineBreak)
				{
					mdParagraph.AddMdTextRange().IsLineBreak = true;
				}
				break;
			case EntityType.InlineContentControl:
				IterateParagraphItems((paragraphItem as InlineContentControl).ParagraphItems, mdParagraph);
				break;
			case EntityType.Picture:
			{
				WPicture wPicture = paragraphItem as WPicture;
				string imageFormat = GetImageFormat(wPicture);
				if (!wPicture.IsMetaFile)
				{
					switch (imageFormat)
					{
					case "jpeg":
					case "png":
					case "gif":
					{
						MdPicture mdPicture = new MdPicture();
						ConvertAsMdPicture(mdPicture, wPicture, mdParagraph, imageFormat);
						ExecuteEvent(mdPicture, wPicture);
						mdParagraph.Inlines.Add(mdPicture);
						break;
					}
					}
				}
				break;
			}
			}
		}
	}

	private MdCodeBlock ConvertAsCodeBlock(WParagraph paragraph, MdCodeBlock codeBlock)
	{
		if (codeBlock == null)
		{
			codeBlock = markdownDocument.AddMdCodeBlock();
		}
		string text = string.Empty;
		foreach (Entity childEntity in paragraph.ChildEntities)
		{
			if (childEntity.EntityType == EntityType.TextRange)
			{
				WTextRange wTextRange = childEntity as WTextRange;
				text += wTextRange.Text;
			}
		}
		codeBlock.Lines.Add(text);
		codeBlock.IsFencedCode = paragraph.StyleName == "FencedCode";
		return codeBlock;
	}

	private void IterateTableRow(MdTableRow mdRow, WTableRow row)
	{
		foreach (WTableCell cell in row.Cells)
		{
			MdTableCell mdCell = mdRow.AddMdTableCell();
			ConvertCellIntoText(cell, mdRow, mdCell);
		}
	}

	private void ConvertCellIntoText(WTableCell cell, MdTableRow mdRow, MdTableCell mdCell)
	{
		if (cell.CellFormat.VerticalMerge == CellMerge.Continue || cell.CellFormat.HorizontalMerge == CellMerge.Continue)
		{
			return;
		}
		foreach (Entity childEntity in cell.ChildEntities)
		{
			GetCellContent(childEntity, mdRow, mdCell);
		}
		AddCellsforSpannedCells(cell, mdCell, mdRow);
	}

	private void GetCellContent(Entity entity, MdTableRow mdRow, MdTableCell mdCell)
	{
		switch (entity.EntityType)
		{
		case EntityType.Paragraph:
			if (mdCell.Items.Count > 0)
			{
				InsertSpace(mdCell);
			}
			GetParaContentInCell(entity as IWParagraph, mdCell);
			break;
		case EntityType.Table:
			if (mdCell.Items.Count > 0)
			{
				InsertSpace(mdCell);
			}
			IterateNestedTable(entity as WTable, mdRow, mdCell);
			break;
		case EntityType.BlockContentControl:
		{
			foreach (Entity childEntity in (entity as IBlockContentControl).TextBody.ChildEntities)
			{
				GetCellContent(childEntity, mdRow, mdCell);
			}
			break;
		}
		}
	}

	private void IterateNestedTable(WTable table, MdTableRow mdRow, MdTableCell mdCell)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				ConvertCellIntoText(cell, mdRow, mdCell);
			}
		}
	}

	internal bool IterateFieldHyperlink(Hyperlink link, MdHyperlink mdHyperlink)
	{
		mdHyperlink.DisplayText = link.TextToDisplay;
		if (link.Type == HyperlinkType.FileLink)
		{
			mdHyperlink.Url = link.FilePath;
		}
		else
		{
			mdHyperlink.Url = link.Uri;
		}
		return true;
	}

	private void AddCellsforSpannedCells(WTableCell cell, MdTableCell mdCell, MdTableRow mdRow)
	{
		if (cell.CellFormat.CellGridSpan > 1)
		{
			for (int i = 1; i < cell.CellFormat.CellGridSpan; i++)
			{
				mdCell = mdRow.AddMdTableCell();
			}
		}
	}

	private void InsertSpace(MdTableCell mdCell)
	{
		MdTextRange mdTextRange = new MdTextRange();
		mdTextRange.Text = " ";
		mdCell.Items.Add(mdTextRange);
	}

	private void GetParaContentInCell(IWParagraph paragraph, MdTableCell cell)
	{
		MdParagraph mdParagraph = new MdParagraph();
		IterateParagraphItems(paragraph.ChildEntities as ParagraphItemCollection, mdParagraph);
		foreach (IMdInline inline in mdParagraph.Inlines)
		{
			cell.Items.Add(inline);
		}
	}

	private void SetTableAlignment(WTableRow row, MdTable table)
	{
		foreach (WTableCell cell in row.Cells)
		{
			if (cell.Paragraphs.Count > 0)
			{
				switch (cell.Paragraphs[0].ParagraphFormat.HorizontalAlignment)
				{
				case HorizontalAlignment.Right:
					table.ColumnAlignments.Add(ColumnAlignment.Right);
					break;
				case HorizontalAlignment.Center:
					table.ColumnAlignments.Add(ColumnAlignment.Center);
					break;
				default:
					table.ColumnAlignments.Add(ColumnAlignment.Left);
					break;
				}
				if (cell.CellFormat.CellGridSpan > 1)
				{
					for (int i = 1; i < cell.CellFormat.CellGridSpan; i++)
					{
						table.ColumnAlignments.Add(ColumnAlignment.Left);
					}
				}
			}
			else
			{
				table.ColumnAlignments.Add(ColumnAlignment.Left);
			}
		}
	}

	private bool CheckFormat(MdTextRange currentText, MdTextRange previousText)
	{
		if (currentText.TextFormat.IsHidden == previousText.TextFormat.IsHidden && currentText.TextFormat.Bold == previousText.TextFormat.Bold && currentText.TextFormat.CodeSpan == previousText.TextFormat.CodeSpan && currentText.TextFormat.SubSuperScriptType == previousText.TextFormat.SubSuperScriptType && currentText.TextFormat.Italic == previousText.TextFormat.Italic && currentText.TextFormat.StrikeThrough == previousText.TextFormat.StrikeThrough)
		{
			return true;
		}
		return false;
	}

	private void ExecuteEvent(MdPicture mdPicture, WPicture picture)
	{
		if (picture.Document.SaveOptions.IsEventSubscribed)
		{
			ImageNodeVisitedEventArgs imageNodeVisitedEventArgs = null;
			imageNodeVisitedEventArgs = picture.Document.SaveOptions.ExecuteSaveImageEvent(new MemoryStream(mdPicture.ImageBytes), null);
			if (imageNodeVisitedEventArgs != null && !string.IsNullOrEmpty(imageNodeVisitedEventArgs.Uri))
			{
				mdPicture.Url = imageNodeVisitedEventArgs.Uri;
				return;
			}
		}
		if (!string.IsNullOrEmpty(picture.Document.SaveOptions.MarkdownExportImagesFolder))
		{
			SaveImageInFolder(picture, mdPicture, picture.Document.SaveOptions.MarkdownExportImagesFolder);
		}
	}

	private void SaveImageInFolder(WPicture picture, MdPicture mdPicture, string exportImagesFolder)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		try
		{
			empty = exportImagesFolder;
			if (!empty.EndsWith("\\"))
			{
				empty += "\\";
			}
			empty = Path.GetFullPath(empty);
			empty2 = picture.Document.SaveOptions.GetImagePath(ref imageCount, m_cacheFilesInternally: false, empty3) + "." + mdPicture.ImageFormat;
			picture.Document.SaveOptions.EnsureImagesFolder(m_bImagesFolderCreated: false, m_cacheFilesInternally: false, empty, empty3);
		}
		catch (Exception)
		{
			throw new Exception("Given physical folder not accessible to export the  images. Enable permission if access is denied.");
		}
		picture.Document.SaveOptions.ProcessImageUsingFileStream(empty, empty2, mdPicture.ImageBytes);
		mdPicture.Url = empty + empty2;
	}

	internal void ApplyListFormat(MdParagraph mdparagraph, IWParagraph paragraph)
	{
		MdListFormat mdListFormat = null;
		if (paragraph.ListFormat.ListType != ListType.NoList)
		{
			mdListFormat = new MdListFormat();
			mdListFormat.IsNumbered = paragraph.ListFormat.ListType == ListType.Numbered;
			string text = (mdListFormat.IsNumbered ? mdListFormat.NumberedListMarker : mdListFormat.BulletedListMarker);
			int count = text.Length * paragraph.ListFormat.ListLevelNumber;
			mdListFormat.ListValue = new string(' ', count);
			mdListFormat.ListValue += text;
		}
		mdparagraph.ListFormat = mdListFormat;
	}

	internal void ApplyTextFormat(WCharacterFormat characterFormat, string text, MdTextRange mdTextRange)
	{
		switch (characterFormat.SubSuperScript)
		{
		case SubSuperScript.SuperScript:
			mdTextRange.TextFormat.SubSuperScriptType = MdSubSuperScript.SuperScript;
			break;
		case SubSuperScript.SubScript:
			mdTextRange.TextFormat.SubSuperScriptType = MdSubSuperScript.SubScript;
			break;
		default:
			mdTextRange.TextFormat.SubSuperScriptType = MdSubSuperScript.None;
			break;
		}
		mdTextRange.TextFormat.CodeSpan = characterFormat.CharStyleName == "InlineCode Char";
		mdTextRange.TextFormat.Bold = characterFormat.Bold || characterFormat.BoldBidi;
		mdTextRange.TextFormat.Italic = characterFormat.Italic || characterFormat.ItalicBidi;
		mdTextRange.TextFormat.StrikeThrough = characterFormat.Strikeout;
		mdTextRange.TextFormat.IsHidden = characterFormat.Hidden;
		mdTextRange.Text = text;
	}

	internal void ConvertAsMdPicture(MdPicture mdPicture, WPicture picture, MdParagraph mdParagraph, string imageFormat)
	{
		mdPicture.ImageBytes = picture.ImageBytes;
		mdPicture.AltText = picture.AlternativeText;
		mdPicture.ImageFormat = imageFormat;
		if (mdPicture.AltText != null && mdPicture.AltText.Contains("\n"))
		{
			mdPicture.AltText = mdPicture.AltText.Replace("\n", " ");
		}
	}

	private string GetImageFormat(WPicture picture)
	{
		_ = string.Empty;
		_ = picture.Image;
		return picture.Image.Format.ToString().ToLowerInvariant();
	}

	internal void ConvertAndWrite(string filepath, WordDocument document, StreamWriter writer)
	{
		MarkdownDocument markdownText = GetMarkdownText(document);
		string value = new MarkdownSerializer().SerializeMarkdown(markdownText);
		writer.Write(value);
		markdownText.Dispose();
	}
}
