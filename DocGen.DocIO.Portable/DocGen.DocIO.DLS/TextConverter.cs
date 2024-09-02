using System.IO;

namespace DocGen.DocIO.DLS;

public class TextConverter
{
	private StreamWriter m_writer;

	private string m_text = "";

	private int m_curSectionIndex;

	private bool m_bGetString;

	private WordDocument m_document;

	private WParagraph m_lastPara;

	private bool isFieldEnd;

	private bool isFieldSeparator;

	private WFieldMark seperator;

	private bool isCommentBody;

	public string GetText(WordDocument document)
	{
		m_document = document;
		m_bGetString = true;
		Write();
		m_bGetString = false;
		return m_text;
	}

	public void Write(StreamWriter writer, IWordDocument document)
	{
		m_writer = writer;
		m_document = document as WordDocument;
		Write();
	}

	public void Read(StreamReader reader, IWordDocument document)
	{
		string text = reader.ReadToEnd();
		Read(text, document);
	}

	internal void Read(string text, IWordDocument document)
	{
		text = text.Replace(ControlChar.CrLf, ControlChar.LineFeed);
		text = text.Replace(ControlChar.CarriegeReturn, ControlChar.LineFeed);
		string[] array = text.Split(ControlChar.LineFeed.ToCharArray());
		if (document.LastParagraph == null)
		{
			if (document.LastSection == null)
			{
				document.EnsureMinimal();
			}
			else
			{
				document.LastSection.Body.AddParagraph();
			}
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			string text2 = array[i];
			text2 = text2.Trim(ControlChar.CarriegeReturn.ToCharArray());
			if (i > 0 && (i + 1 < num || !string.IsNullOrEmpty(text2)))
			{
				document.LastSection.Body.AddParagraph();
			}
			if (!string.IsNullOrEmpty(text2))
			{
				((IWParagraph)document.LastParagraph).AppendText(text2);
			}
		}
		InitBuiltinDocumentProperties(text, array, document);
	}

	private void InitBuiltinDocumentProperties(string text, string[] textLines, IWordDocument doc)
	{
		int num = textLines.Length;
		int num2 = 0;
		foreach (string text2 in textLines)
		{
			if (text2 == ControlChar.CarriegeReturn || text2 == string.Empty)
			{
				num--;
				continue;
			}
			string[] array = text2.Split(" ".ToCharArray());
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != string.Empty)
				{
					num2++;
				}
			}
		}
		text = text.Replace(" ", string.Empty);
		text = text.Replace(ControlChar.LineFeed, string.Empty);
		text = text.Replace(ControlChar.CarriegeReturn, string.Empty);
	}

	protected void WriteHFBody(WordDocument document)
	{
	}

	protected void WriteBody(ITextBody body)
	{
		int num = body.ChildEntities.Count - 1;
		TextBodyItem textBodyItem = null;
		for (int i = 0; i <= num; i++)
		{
			textBodyItem = body.ChildEntities[i] as TextBodyItem;
			switch (textBodyItem.EntityType)
			{
			case EntityType.Paragraph:
			{
				bool lastPara = textBodyItem as WParagraph == m_lastPara;
				WriteParagraph((textBodyItem as IWParagraph).ChildEntities as ParagraphItemCollection, lastPara);
				break;
			}
			case EntityType.Table:
				WriteTable(textBodyItem as IWTable);
				break;
			case EntityType.BlockContentControl:
				WriteBody((textBodyItem as IBlockContentControl).TextBody);
				break;
			}
		}
	}

	protected void WriteParagraph(ParagraphItemCollection paragraphItems, bool lastPara)
	{
		IWParagraph iWParagraph = null;
		iWParagraph = ((!(paragraphItems.Owner is InlineContentControl)) ? (paragraphItems.Owner as IWParagraph) : (paragraphItems.Owner as InlineContentControl).GetOwnerParagraphValue());
		if (!iWParagraph.ListFormat.IsEmptyList && iWParagraph.ChildEntities.Count != 0 && !(iWParagraph as WParagraph).SectionEndMark)
		{
			WriteList(iWParagraph);
		}
		if (isCommentBody)
		{
			return;
		}
		int i = 0;
		for (int count = paragraphItems.Count; i < count; i++)
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
			case EntityType.Break:
				if ((paragraphItem as Break).BreakType == BreakType.LineBreak || (paragraphItem as Break).BreakType == BreakType.TextWrappingBreak)
				{
					WriteNewLine();
				}
				break;
			case EntityType.Field:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
			{
				WField wField = paragraphItem as WField;
				switch (wField.FieldType)
				{
				case FieldType.FieldDocVariable:
				{
					string text = wField.Document.Variables[wField.FieldValue];
					WriteText(text);
					if (wField.FieldEnd != null && wField.FieldEnd.Owner is WParagraph)
					{
						isFieldEnd = true;
					}
					break;
				}
				case FieldType.FieldUnknown:
					if (wField.FieldEnd != null && wField.FieldEnd.Owner is WParagraph)
					{
						isFieldEnd = true;
					}
					break;
				}
				if (wField.FieldType == FieldType.FieldIf && seperator == null)
				{
					seperator = wField.FieldSeparator;
				}
				if (wField.FieldEnd != null)
				{
					isFieldSeparator = true;
				}
				break;
			}
			case EntityType.TextRange:
				if (!isFieldSeparator)
				{
					WriteText((paragraphItem as IWTextRange).Text);
				}
				break;
			case EntityType.TextBox:
				WriteBody((paragraphItem as WTextBox).TextBoxBody);
				break;
			case EntityType.AutoShape:
				WriteBody((paragraphItem as Shape).TextBody);
				break;
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
				if (seperator == null)
				{
					seperator = (paragraphItem as WFormField).FieldSeparator;
				}
				if ((paragraphItem as WFormField).FieldEnd != null)
				{
					isFieldSeparator = true;
				}
				break;
			case EntityType.OleObject:
				if ((paragraphItem as WOleObject).Field.FieldEnd != null)
				{
					isFieldSeparator = true;
				}
				break;
			case EntityType.TOC:
				if ((paragraphItem as TableOfContent).TOCField.FieldEnd != null)
				{
					isFieldSeparator = true;
				}
				break;
			case EntityType.FieldMark:
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
			case EntityType.XmlParaItem:
				if (!(paragraphItem is XmlParagraphItem { MathParaItemsCollection: not null } xmlParagraphItem) || xmlParagraphItem.MathParaItemsCollection.Count <= 0)
				{
					break;
				}
				foreach (ParagraphItem item in xmlParagraphItem.MathParaItemsCollection)
				{
					if (item is WTextRange)
					{
						WriteText((item as IWTextRange).Text);
					}
				}
				break;
			case EntityType.InlineContentControl:
				WriteParagraph((paragraphItem as InlineContentControl).ParagraphItems, lastPara: true);
				break;
			}
		}
		if (!lastPara)
		{
			WriteNewLine();
		}
	}

	protected void WriteTable(IWTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				WriteBody(cell);
			}
		}
	}

	protected void WriteSectionEnd(IWSection section, bool lastSection)
	{
		if (m_bGetString)
		{
			m_text += ControlChar.CrLf;
		}
		else if (!lastSection)
		{
			m_writer.WriteLine("");
		}
		m_curSectionIndex++;
	}

	protected void WriteText(string text)
	{
		if (m_bGetString)
		{
			m_text += text;
		}
		else
		{
			m_writer.Write(text);
		}
	}

	protected void WriteList(IWParagraph paragraph)
	{
		bool isPicBullet = false;
		string listText = (paragraph as WParagraph).GetListText(isFromTextConverter: true, ref isPicBullet);
		if (!isPicBullet)
		{
			(paragraph as WParagraph).m_liststring = listText;
		}
		if (!isCommentBody)
		{
			if (m_bGetString)
			{
				m_text += listText;
			}
			else
			{
				m_writer.Write(listText);
			}
		}
	}

	protected void WriteNewLine()
	{
		if (m_bGetString)
		{
			m_text += ControlChar.CrLf;
		}
		else
		{
			m_writer.WriteLine("");
		}
	}

	private void UpdateLastPara()
	{
		ITextBody footer = GetFooter(m_document.LastSection, m_document.LastSection.Index);
		if (footer.ChildEntities.Count > 0)
		{
			m_lastPara = footer.LastParagraph as WParagraph;
		}
		else
		{
			m_lastPara = m_document.LastParagraph;
		}
	}

	private void Write()
	{
		WSection wSection = null;
		int num = m_document.Sections.Count - 1;
		bool flag = false;
		UpdateLastPara();
		AddTrailVersion(isBottom: false);
		for (int i = 0; i <= num; i++)
		{
			wSection = m_document.Sections[i];
			flag = i == num;
			WriteBody(GetHeader(wSection, m_curSectionIndex));
			WriteBody(wSection.Body);
			WriteSectionEnd(wSection, flag);
			WriteBody(GetFooter(wSection, m_curSectionIndex - 1));
		}
		AddTrailVersion(isBottom: true);
		m_document.ClearLists();
		for (int j = 0; j < m_document.Comments.Count; j++)
		{
			WComment wComment = m_document.Comments[j];
			isCommentBody = true;
			WriteBody(wComment.TextBody);
			isCommentBody = false;
		}
	}

	internal void AddTrailVersion(bool isBottom)
	{
		
	}

	private ITextBody GetFooter(WSection section, int sectionIndex)
	{
		HeaderFooterType headerFooterType = (section.PageSetup.DifferentFirstPage ? HeaderFooterType.FirstPageFooter : HeaderFooterType.OddFooter);
		if (section.HeadersFooters[headerFooterType].LinkToPrevious && sectionIndex > 0)
		{
			int num = sectionIndex - 1;
			while (num >= 0)
			{
				WSection wSection = m_document.Sections[num];
				HeaderFooter headerFooter = wSection.HeadersFooters[headerFooterType];
				if (headerFooterType == headerFooter.Type && headerFooter.LinkToPrevious)
				{
					num--;
				}
				else if (headerFooterType == headerFooter.Type && !headerFooter.LinkToPrevious)
				{
					section.HeadersFooters[headerFooterType] = wSection.HeadersFooters[headerFooterType];
					break;
				}
			}
		}
		if (section.PageSetup.DifferentFirstPage)
		{
			return section.HeadersFooters.FirstPageFooter;
		}
		return section.HeadersFooters.Footer;
	}

	private ITextBody GetHeader(WSection section, int sectionIndex)
	{
		HeaderFooterType headerFooterType = ((!section.PageSetup.DifferentFirstPage) ? HeaderFooterType.OddHeader : HeaderFooterType.FirstPageHeader);
		if (section.HeadersFooters[headerFooterType].LinkToPrevious && sectionIndex > 0)
		{
			int num = sectionIndex - 1;
			while (num >= 0)
			{
				WSection wSection = m_document.Sections[num];
				HeaderFooter headerFooter = wSection.HeadersFooters[headerFooterType];
				if (headerFooterType == headerFooter.Type && headerFooter.LinkToPrevious)
				{
					num--;
				}
				else if (headerFooterType == headerFooter.Type && !headerFooter.LinkToPrevious)
				{
					section.HeadersFooters[headerFooterType] = wSection.HeadersFooters[headerFooterType];
					break;
				}
			}
		}
		if (section.PageSetup.DifferentFirstPage)
		{
			return section.HeadersFooters.FirstPageHeader;
		}
		return section.HeadersFooters.Header;
	}
}
