using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.DocIO.DLS.Entities;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class HTMLExport
{
	private const string DEF_HYPHEN = "-";

	private const char DEF_NONBREAK_HYPHEN = '\u001e';

	private const char DEF_SOFT_HYPHEN = '\u001f';

	private XmlWriter m_writer;

	private string m_fileNameWithoutExt;

	private bool m_bIsWriteListTab;

	private int m_imgCounter;

	private int m_currListLevel = -1;

	private Stack<int> listStack = new Stack<int>();

	private Stack<WField> m_fieldStack;

	private Stack<WField> m_nestedHyperlinkFieldStack;

	private bool isKeepValue;

	private Dictionary<int, WFootnote> m_footnotes;

	private Dictionary<int, WFootnote> m_endnotes;

	private Dictionary<string, Dictionary<int, int>> m_lists;

	private string m_ftntAttrStr;

	private string m_ftntString;

	private bool m_bUseAbsolutePath;

	private bool m_bSkipFieldItem;

	private bool m_bSkiPageRefFieldItem;

	private WParagraph m_currPara;

	private bool m_bIsFirstSection = true;

	private Dictionary<string, string> m_stylesColl;

	private WordDocument m_document;

	private string m_prefixedValue;

	private bool m_bIsPrefixedList;

	private bool m_bIsParaWithinDivision;

	private bool m_bIsPreserveListAsPara;

	private Dictionary<WPicture, int> m_behindWrapStyleFloatingItems;

	private bool m_cacheFilesInternally;

	private bool m_hasNavigationId;

	private bool m_hasOEBHeaderFooter;

	private int m_nameID;

	private string[] m_headingStyles;

	private MemoryStream m_styleSheet;

	private Dictionary<string, string> m_bookmarks;

	private string m_ftntRefAttrStr;

	private WCharacterFormat m_currListCharFormat;

	public bool UseAbsolutePath
	{
		get
		{
			return m_bUseAbsolutePath;
		}
		set
		{
			m_bUseAbsolutePath = Convert.ToBoolean(value);
		}
	}

	private Dictionary<string, Dictionary<int, int>> Lists
	{
		get
		{
			if (m_lists == null)
			{
				m_lists = new Dictionary<string, Dictionary<int, int>>();
			}
			return m_lists;
		}
	}

	private Dictionary<WPicture, int> BehindWrapStyleFloatingItems
	{
		get
		{
			if (m_behindWrapStyleFloatingItems == null)
			{
				m_behindWrapStyleFloatingItems = new Dictionary<WPicture, int>();
			}
			return m_behindWrapStyleFloatingItems;
		}
	}

	private WField CurrentField
	{
		get
		{
			if (m_fieldStack == null || m_fieldStack.Count <= 0)
			{
				return null;
			}
			return m_fieldStack.Peek();
		}
	}

	private WField PreviousField
	{
		get
		{
			if (FieldStack.Count > 1)
			{
				return FieldStack.ToArray()[1];
			}
			return null;
		}
	}

	private Dictionary<int, WFootnote> Footnotes
	{
		get
		{
			if (m_footnotes == null)
			{
				m_footnotes = new Dictionary<int, WFootnote>();
			}
			return m_footnotes;
		}
	}

	private Stack<WField> FieldStack
	{
		get
		{
			if (m_fieldStack == null)
			{
				m_fieldStack = new Stack<WField>();
			}
			return m_fieldStack;
		}
	}

	private Dictionary<int, WFootnote> Endnotes
	{
		get
		{
			if (m_endnotes == null)
			{
				m_endnotes = new Dictionary<int, WFootnote>();
			}
			return m_endnotes;
		}
	}

	internal bool CacheFilesInternally
	{
		get
		{
			return m_cacheFilesInternally;
		}
		set
		{
			m_cacheFilesInternally = value;
		}
	}

	internal bool HasNavigationId
	{
		get
		{
			return m_hasNavigationId;
		}
		set
		{
			m_hasNavigationId = value;
		}
	}

	internal bool HasOEBHeaderFooter
	{
		get
		{
			return m_hasOEBHeaderFooter;
		}
		set
		{
			m_hasOEBHeaderFooter = value;
		}
	}

	internal Stream EmbeddedStyleSheet => m_styleSheet;

	public void SaveAsXhtml(WordDocument doc, Stream stream)
	{
		SaveAsXhtml(doc, stream, Encoding.UTF8);
	}

	public void SaveAsXhtml(WordDocument doc, Stream stream, Encoding encoding)
	{
		m_document = doc;
		SortBehindWrapStyleItemByZindex();
		UnitsConvertor.Instance.InitDefProporsions();
		if (!string.IsNullOrEmpty(doc.SaveOptions.HtmlExportImagesFolder))
		{
			m_bUseAbsolutePath = true;
		}
		else
		{
			m_bUseAbsolutePath = false;
		}
		if (!doc.SaveOptions.HtmlExportImagesFolder.EndsWith("\\"))
		{
			doc.SaveOptions.HtmlExportImagesFolder += "\\";
		}
		CssStyleSheetType htmlExportCssStyleSheetType = doc.SaveOptions.HtmlExportCssStyleSheetType;
		if (doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			doc.SaveOptions.HtmlExportCssStyleSheetType = CssStyleSheetType.Inline;
		}
		m_stylesColl = new Dictionary<string, string>();
		m_writer = CreateWriter(stream, encoding);
		if (!m_cacheFilesInternally)
		{
			WriteXhtml(doc, string.Empty);
		}
		else
		{
			WriteXhtml(doc, doc.SaveOptions.HtmlExportCssStyleSheetFileName);
		}
		m_writer.Flush();
		if (doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			doc.SaveOptions.HtmlExportCssStyleSheetType = htmlExportCssStyleSheetType;
		}
	}

	private XmlWriter CreateWriter(Stream data, Encoding encoding)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = encoding;
		if (m_document.SaveOptions.HtmlExportOmitXmlDeclaration || m_document.SaveOptions.HtmlExportBodyContentAlone)
		{
			xmlWriterSettings.OmitXmlDeclaration = true;
		}
		return XmlWriter.Create(data, xmlWriterSettings);
	}

	private void WriteXhtml(WordDocument doc, string cssFileName)
	{
		WriteHead(doc, cssFileName);
		WriteBody(doc);
		if (!doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			m_writer.WriteEndElement();
		}
		Close();
	}

	private void Close()
	{
		if (m_nestedHyperlinkFieldStack != null)
		{
			m_nestedHyperlinkFieldStack.Clear();
			m_nestedHyperlinkFieldStack = null;
		}
		if (m_fieldStack != null)
		{
			m_fieldStack.Clear();
			m_fieldStack = null;
		}
		if (listStack != null)
		{
			listStack.Clear();
			listStack = null;
		}
		if (m_footnotes != null)
		{
			m_footnotes.Clear();
			m_footnotes = null;
		}
		if (m_endnotes != null)
		{
			m_endnotes.Clear();
			m_endnotes = null;
		}
		if (m_lists != null)
		{
			m_lists.Clear();
			m_lists = null;
		}
		if (m_stylesColl != null)
		{
			m_stylesColl.Clear();
			m_stylesColl = null;
		}
		if (m_behindWrapStyleFloatingItems != null)
		{
			m_behindWrapStyleFloatingItems.Clear();
			m_behindWrapStyleFloatingItems = null;
		}
		if (m_bookmarks != null)
		{
			m_bookmarks.Clear();
			m_bookmarks = null;
		}
	}

	private void WriteHead(WordDocument doc, string cssFileName)
	{
		if (doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			return;
		}
		if (!doc.SaveOptions.HtmlExportOmitXmlDeclaration)
		{
			m_writer.WriteStartDocument();
		}
		m_writer.WriteDocType("html", "-//W3C//DTD XHTML 1.1//EN", "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd", null);
		m_writer.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
		m_writer.WriteStartElement("head");
		m_writer.WriteRaw("<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=utf-8\" />");
		m_writer.WriteRaw($"<title>{m_fileNameWithoutExt}</title>");
		if ((doc.SaveOptions.HtmlExportCssStyleSheetType == CssStyleSheetType.Internal || string.IsNullOrEmpty(cssFileName)) && doc.SaveOptions.HtmlExportCssStyleSheetType != 0)
		{
			m_writer.WriteStartElement("style");
			m_writer.WriteAttributeString("type", "text/css");
			m_writer.WriteRaw(GetStyleSheet(doc));
			m_writer.WriteEndElement();
		}
		else
		{
			if (m_cacheFilesInternally)
			{
				m_styleSheet = new MemoryStream();
				StreamWriter streamWriter = new StreamWriter(m_styleSheet);
				streamWriter.Write(GetStyleSheet(doc));
				streamWriter.Flush();
			}
			m_writer.WriteRaw($"<link href=\"{cssFileName}\" type=\"text/css\" rel=\"stylesheet\"/>");
		}
		m_writer.WriteEndElement();
	}

	private void WriteBody(WordDocument doc)
	{
		if (!doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			m_writer.WriteStartElement("body");
		}
		if (doc.Background != null && doc.Background.Picture != null)
		{
			WriteBackgroundImage(doc.Background.Picture);
		}
		m_bIsFirstSection = true;
		foreach (WSection section in doc.Sections)
		{
			WriteSection(section);
		}
		WriteFootnotes(FootnoteType.Footnote);
		m_bIsFirstSection = true;
		WriteFootnotes(FootnoteType.Endnote);
		AddTrailVersion(doc);
		if (!doc.SaveOptions.HtmlExportBodyContentAlone)
		{
			m_writer.WriteEndElement();
		}
	}

	internal void AddTrailVersion(WordDocument doc)
	{
	}

	private void WriteBackgroundImage(byte[] pic)
	{
		DocGen.DocIO.DLS.Entities.Image image;
		using (MemoryStream memoryStream = new MemoryStream(pic))
		{
			image = DocGen.DocIO.DLS.Entities.Image.FromStream(memoryStream);
		}
		string text = "." + ((DocGen.DocIO.DLS.Entities.ImageFormat.Jpeg.Equals(image.RawFormat) || DocGen.DocIO.DLS.Entities.ImageFormat.Emf.Equals(image.RawFormat) || DocGen.DocIO.DLS.Entities.ImageFormat.Wmf.Equals(image.RawFormat)) ? DocGen.DocIO.DLS.Entities.ImageFormat.Png : ((!DocGen.DocIO.DLS.Entities.ImageFormat.Gif.Equals(image.RawFormat)) ? DocGen.DocIO.DLS.Entities.ImageFormat.Jpeg : DocGen.DocIO.DLS.Entities.ImageFormat.Gif)).ToString().ToLowerInvariant();
		string text2 = "data:image/" + text;
		string text3 = Convert.ToBase64String(pic);
		m_writer.WriteAttributeString("background", text2 + ";base64," + text3);
	}

	private string GetStyleSheet(WordDocument doc)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (doc.SaveOptions.EPubExportFont && doc.SaveOptions.FontFiles != null)
		{
			string[] fontFiles = doc.SaveOptions.FontFiles;
			foreach (string textline in fontFiles)
			{
				AppendLine(stringBuilder, textline);
			}
		}
		AppendLine(stringBuilder, "body{ font-family:'Times New Roman'; font-size:1em; }");
		AppendLine(stringBuilder, "ul, ol{ margin-top: 0; margin-bottom: 0; }");
		IEnumerator enumerator = doc.Styles.GetEnumerator();
		try
		{
			Style style;
			for (; enumerator.MoveNext(); stringBuilder.Append("."), stringBuilder.Append(EncodeName(style.Name)), AppendStyleSheet(style, stringBuilder))
			{
				style = (Style)enumerator.Current;
				string text = EncodeName(style.Name).ToLowerInvariant();
				if (text == null)
				{
					continue;
				}
				int i = text.Length;
				if (i != 8)
				{
					if (i != 9)
					{
						continue;
					}
					switch (text[8])
					{
					case '1':
						break;
					case '2':
						goto IL_01cd;
					case '3':
						goto IL_0205;
					case '4':
						goto IL_023d;
					case '5':
						goto IL_0275;
					case '6':
						goto IL_02ad;
					default:
						continue;
					}
					switch (text)
					{
					case "heading-1":
					case "heading 1":
					case "heading_1":
						break;
					default:
						continue;
					}
				}
				else
				{
					switch (text[7])
					{
					case '1':
						break;
					case '2':
						goto IL_0127;
					case '3':
						goto IL_013d;
					case '4':
						goto IL_0153;
					case '5':
						goto IL_0169;
					case '6':
						goto IL_017f;
					default:
						continue;
					}
					if (!(text == "heading1"))
					{
						continue;
					}
				}
				stringBuilder.Append("h1");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_0327:
				stringBuilder.Append("h4");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_013d:
				if (!(text == "heading3"))
				{
					continue;
				}
				goto IL_0310;
				IL_0127:
				if (!(text == "heading2"))
				{
					continue;
				}
				goto IL_02f9;
				IL_02ad:
				switch (text)
				{
				case "heading-6":
				case "heading 6":
				case "heading_6":
					break;
				default:
					continue;
				}
				goto IL_0355;
				IL_0275:
				switch (text)
				{
				case "heading-5":
				case "heading 5":
				case "heading_5":
					break;
				default:
					continue;
				}
				goto IL_033e;
				IL_023d:
				switch (text)
				{
				case "heading-4":
				case "heading 4":
				case "heading_4":
					break;
				default:
					continue;
				}
				goto IL_0327;
				IL_0205:
				switch (text)
				{
				case "heading-3":
				case "heading 3":
				case "heading_3":
					break;
				default:
					continue;
				}
				goto IL_0310;
				IL_01cd:
				switch (text)
				{
				case "heading-2":
				case "heading 2":
				case "heading_2":
					break;
				default:
					continue;
				}
				goto IL_02f9;
				IL_0310:
				stringBuilder.Append("h3");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_02f9:
				stringBuilder.Append("h2");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_017f:
				if (!(text == "heading6"))
				{
					continue;
				}
				goto IL_0355;
				IL_0355:
				stringBuilder.Append("h6");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_0169:
				if (!(text == "heading5"))
				{
					continue;
				}
				goto IL_033e;
				IL_033e:
				stringBuilder.Append("h5");
				AppendStyleSheet(style, stringBuilder);
				continue;
				IL_0153:
				if (!(text == "heading4"))
				{
					continue;
				}
				goto IL_0327;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return stringBuilder.ToString();
	}

	private void AppendStyleSheet(Style style, StringBuilder sb)
	{
		string value = string.Empty;
		sb.Append("{");
		switch (style.StyleType)
		{
		case StyleType.ParagraphStyle:
			if (style is WParagraphStyle wParagraphStyle)
			{
				value = GetStyle(wParagraphStyle.ParagraphFormat, isListLevel: false, m_bIsPreserveListAsPara, null);
				value += GetStyle(wParagraphStyle.CharacterFormat);
			}
			break;
		case StyleType.CharacterStyle:
			if (style is WCharacterStyle wCharacterStyle)
			{
				value = GetStyle(wCharacterStyle.CharacterFormat);
			}
			break;
		}
		sb.Append(value);
		if (!m_stylesColl.ContainsKey(style.Name))
		{
			m_stylesColl.Add(style.Name, value);
		}
		AppendLine(sb, "}");
	}

	private void AppendLine(StringBuilder sb, string textline)
	{
		sb.AppendLine(textline);
	}

	private void WritePageBreakBeforeSection()
	{
		m_writer.WriteStartElement("span");
		m_writer.WriteAttributeString("style", "font-size:12pt;font-family:Times New Roman");
		m_writer.WriteStartElement("br");
		m_writer.WriteAttributeString("clear", "all");
		m_writer.WriteAttributeString("style", "page-break-before:always");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void WriteSection(WSection sec)
	{
		bool htmlExportHeadersFooters = sec.Document.SaveOptions.HtmlExportHeadersFooters;
		if (sec.PreviousSibling != null && sec.BreakCode == SectionBreakCode.NewPage)
		{
			WritePageBreakBeforeSection();
		}
		m_writer.WriteStartElement("div");
		m_writer.WriteAttributeString("class", "Section" + sec.GetIndexInOwnerCollection());
		if (sec.Document.SaveFormatType != FormatType.Html && sec.PreviousSibling != null && sec.BreakCode == SectionBreakCode.NewPage)
		{
			m_writer.WriteAttributeString("style", "clear: both; page-break-before: always");
		}
		if (htmlExportHeadersFooters && m_bIsFirstSection)
		{
			if (m_hasOEBHeaderFooter)
			{
				m_writer.WriteStartElement("div");
				m_writer.WriteAttributeString("style", "display: oeb-page-head");
			}
			WriteTextBody(sec.PageSetup.DifferentFirstPage ? sec.HeadersFooters.FirstPageHeader : sec.HeadersFooters.OddHeader);
			if (m_hasOEBHeaderFooter)
			{
				m_writer.WriteEndElement();
			}
		}
		WriteTextBody(sec.Body);
		if (htmlExportHeadersFooters && sec.NextSibling == null)
		{
			sec = sec.Document.Sections[0];
			if (m_hasOEBHeaderFooter)
			{
				m_writer.WriteStartElement("div");
				m_writer.WriteAttributeString("style", "display: oeb-page-foot");
			}
			WriteTextBody(sec.PageSetup.DifferentFirstPage ? sec.HeadersFooters.FirstPageFooter : sec.HeadersFooters.Footer);
			if (m_hasOEBHeaderFooter)
			{
				m_writer.WriteEndElement();
			}
		}
		if (listStack.Count > 0)
		{
			WriteEndElement(listStack.Count);
		}
		m_writer.WriteEndElement();
		m_bIsFirstSection = false;
	}

	private void WriteFootnotes(FootnoteType ftnType)
	{
		Dictionary<int, WFootnote> dictionary = ((ftnType == FootnoteType.Footnote) ? m_footnotes : m_endnotes);
		if (dictionary == null || dictionary.Count <= 0)
		{
			return;
		}
		m_writer.WriteElementString("hr", "");
		foreach (int key in dictionary.Keys)
		{
			string text = (key + 1).ToString(CultureInfo.InvariantCulture);
			WFootnote wFootnote = dictionary[key];
			if (ftnType == FootnoteType.Footnote)
			{
				m_ftntAttrStr = "_ftn" + text;
				m_ftntRefAttrStr = "_ftnref" + text;
			}
			else
			{
				m_ftntAttrStr = "_edn" + text;
				m_ftntRefAttrStr = "_ednref" + text;
			}
			m_ftntString = "[" + text + "] ";
			WriteTextBody(wFootnote.TextBody);
		}
		if (ftnType == FootnoteType.Footnote)
		{
			m_footnotes.Clear();
		}
	}

	private void WriteTextBody(WTextBody body)
	{
		for (int i = 0; i < body.Items.Count; i++)
		{
			WriteBodyItem(body.Items[i]);
		}
	}

	private void WriteBodyItem(TextBodyItem bodyItem)
	{
		switch (bodyItem.EntityType)
		{
		case EntityType.Paragraph:
			m_currPara = bodyItem as WParagraph;
			if (m_currPara != null)
			{
				m_currPara.SplitTextRange();
			}
			WriteParagraph(bodyItem as WParagraph);
			break;
		case EntityType.Table:
			WriteTable(bodyItem as WTable, isTableCreatedFromTextBox: false);
			break;
		case EntityType.BlockContentControl:
			if (bodyItem is BlockContentControl blockContentControl)
			{
				WriteTextBody(blockContentControl.TextBody);
			}
			break;
		}
	}

	private void WriteParagraph(WParagraph para)
	{
		if (para.Items.Count > 0 && para.Items[0].EntityType == EntityType.Break)
		{
			WriteBreak(para.Items[0]);
		}
		WriteParagraphOrList(para);
		if (para.Items.Count == 0 || (para.Text == "" && para.ChildEntities.Count == 1 && (para.ChildEntities[0].EntityType == EntityType.TextRange || para.ChildEntities[0].EntityType == EntityType.BookmarkEnd || para.ChildEntities[0].EntityType == EntityType.FieldMark)))
		{
			WriteEmptyPara(para.BreakCharacterFormat);
		}
		WriteParagraphItems(para.Items);
		m_writer.WriteEndElement();
		if ((para.NextSibling is WParagraph && (para.NextSibling as WParagraph).StyleName != para.StyleName) || para.NextSibling == null || !(para.NextSibling is WParagraph))
		{
			WParagraphStyle wParagraphStyle = m_document.Styles.FindByName(para.StyleName) as WParagraphStyle;
			if (wParagraphStyle.ParagraphFormat.BackColor != Color.Empty && wParagraphStyle.ParagraphFormat.BackColor != Color.White)
			{
				m_writer.WriteEndElement();
			}
		}
		if (para.ParagraphFormat.Bidi || (para.IsInCell && (para.GetOwnerTable(para) as WTable).TableFormat.Bidi))
		{
			WListFormat listFormat = GetListFormat(para);
			if (!m_bIsPreserveListAsPara && !IsPreserveListAsParagraph(listFormat) && !para.SectionEndMark && listFormat.ListType != ListType.NoList && (listFormat.CurrentListLevel == null || listFormat.CurrentListLevel.PatternType != ListPatternType.None))
			{
				m_writer.WriteEndElement();
			}
		}
		m_writer.WriteRaw(ControlChar.CrLf);
	}

	private bool SkipItem(ParagraphItem item)
	{
		WField currentField = CurrentField;
		if (m_bSkipFieldItem && currentField != null)
		{
			if (item is WFieldMark)
			{
				if ((item as WFieldMark).Type == FieldMarkType.FieldSeparator)
				{
					if (currentField.FieldSeparator == item)
					{
						if (currentField.FieldType == FieldType.FieldHyperlink)
						{
							m_bSkipFieldItem = false;
						}
						else if (currentField.FieldType == FieldType.FieldTOC || currentField.FieldType == FieldType.FieldEmbed)
						{
							m_bSkipFieldItem = false;
							FieldStack.Pop();
						}
					}
				}
				else if ((currentField.IsFormField() || m_bSkiPageRefFieldItem) && currentField.FieldEnd == item)
				{
					m_bSkipFieldItem = false;
					FieldStack.Pop();
				}
				else if ((item as WFieldMark).Type == FieldMarkType.FieldEnd && (item as WFieldMark).ParentField != null && (item as WFieldMark).ParentField.EntityType == EntityType.TOC && m_bSkiPageRefFieldItem)
				{
					m_bSkiPageRefFieldItem = false;
				}
			}
			return true;
		}
		if (IsWritingHyperinkFieldResult())
		{
			if (item is WField)
			{
				WField wField = item as WField;
				if (!m_bSkiPageRefFieldItem || wField.FieldType != FieldType.FieldPageRef)
				{
					FieldStack.Push(wField);
					if (wField.FieldType == FieldType.FieldHyperlink)
					{
						m_bSkipFieldItem = true;
						if (m_nestedHyperlinkFieldStack == null)
						{
							m_nestedHyperlinkFieldStack = new Stack<WField>();
						}
						m_nestedHyperlinkFieldStack.Push(wField);
					}
					return true;
				}
				PushToFieldStack(wField);
				m_bSkipFieldItem = true;
			}
			else if (item is WFieldMark && ((currentField.FieldType != FieldType.FieldHyperlink) ? (item == ((currentField.FieldSeparator != null) ? currentField.FieldSeparator : currentField.FieldEnd)) : (item == currentField.FieldEnd)))
			{
				FieldStack.Pop();
				if (currentField.FieldType == FieldType.FieldHyperlink)
				{
					if (m_nestedHyperlinkFieldStack != null && m_nestedHyperlinkFieldStack.Count != 0 && m_nestedHyperlinkFieldStack.Peek() == currentField)
					{
						m_nestedHyperlinkFieldStack.Pop();
					}
					else
					{
						m_writer.WriteFullEndElement();
					}
				}
				return true;
			}
			if ((item.EntityType != EntityType.TextRange && item.EntityType != EntityType.Picture) || currentField.FieldType != FieldType.FieldHyperlink)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWritingHyperinkFieldResult()
	{
		if (FieldStack.Count == 0)
		{
			return false;
		}
		foreach (WField item in FieldStack)
		{
			if (item.FieldType == FieldType.FieldHyperlink)
			{
				return true;
			}
		}
		return false;
	}

	private void WriteParagraphItems(ParagraphItemCollection paraItems)
	{
		bool flag = false;
		string text = null;
		for (int i = 0; i < paraItems.Count; i++)
		{
			ParagraphItem paragraphItem = paraItems[i];
			if (SkipItem(paragraphItem))
			{
				continue;
			}
			switch (paragraphItem.EntityType)
			{
			case EntityType.TextRange:
			{
				string combinedText = CombineTextInSubsequentTextRanges(paraItems, ref i);
				WriteTextRange(paragraphItem as WTextRange, combinedText);
				break;
			}
			case EntityType.Picture:
				if (paragraphItem is WPicture { ImageRecord: not null } wPicture)
				{
					WriteImage(wPicture);
				}
				break;
			case EntityType.Field:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
				if (paragraphItem is WMergeField)
				{
					(paragraphItem as WMergeField).UpdateFieldMarks();
				}
				WriteField(paragraphItem as WField);
				break;
			case EntityType.FieldMark:
				WriteFieldMark(paragraphItem as WFieldMark);
				break;
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
				if (paragraphItem is WFormField { FieldEnd: not null } wFormField)
				{
					WriteFormField(wFormField);
					m_bSkipFieldItem = true;
					FieldStack.Push(wFormField);
				}
				break;
			case EntityType.BookmarkStart:
				if (flag)
				{
					m_writer.WriteEndElement();
					flag = false;
				}
				WriteBookmark(paragraphItem as BookmarkStart);
				if ((paragraphItem as BookmarkStart).Name != "_GoBack")
				{
					flag = true;
					text = (paragraphItem as BookmarkStart).Name;
				}
				break;
			case EntityType.BookmarkEnd:
				if (flag && (paragraphItem as BookmarkEnd).Name == text)
				{
					m_writer.WriteEndElement();
					flag = false;
					text = null;
				}
				break;
			case EntityType.Footnote:
				WriteFootnote(paragraphItem as WFootnote);
				break;
			case EntityType.TextBox:
				WriteTextBox(paragraphItem as WTextBox);
				break;
			case EntityType.Break:
			{
				ParagraphItemCollection paragraphItemCollection = paraItems;
				if (paragraphItem.Owner is InlineContentControl && paragraphItem.Owner.Owner is WParagraph wParagraph)
				{
					paragraphItemCollection = wParagraph.GetParagraphItems();
				}
				if (paragraphItemCollection.IndexOf(paragraphItem) > 0)
				{
					WriteBreak(paragraphItem);
				}
				break;
			}
			case EntityType.Symbol:
				if (paragraphItem is WSymbol wSymbol)
				{
					string style = GetStyle(wSymbol.CharacterFormat, style: true);
					string data = "&#" + wSymbol.CharacterCode + ";";
					string fontName = wSymbol.FontName;
					style = style + "font-family:\"" + fontName + "\"";
					m_writer.WriteStartElement("span");
					m_writer.WriteAttributeString("style", style);
					m_writer.WriteRaw(data);
					m_writer.WriteEndElement();
				}
				break;
			case EntityType.TOC:
			{
				WField tOCField = (paragraphItem as TableOfContent).TOCField;
				if (tOCField.FieldSeparator != null)
				{
					PushToFieldStack(tOCField);
					m_bSkipFieldItem = true;
					if (tOCField.m_formattingString.Contains("\\z"))
					{
						m_bSkiPageRefFieldItem = true;
					}
				}
				break;
			}
			case EntityType.OleObject:
				if ((paragraphItem as WOleObject).Field.FieldSeparator != null)
				{
					PushToFieldStack((paragraphItem as WOleObject).Field);
					m_bSkipFieldItem = true;
				}
				break;
			case EntityType.InlineContentControl:
				if (paragraphItem is InlineContentControl inlineContentControl)
				{
					WriteParagraphItems(inlineContentControl.ParagraphItems);
				}
				break;
			case EntityType.XmlParaItem:
				if (paragraphItem is XmlParagraphItem { MathParaItemsCollection: not null } xmlParagraphItem && xmlParagraphItem.MathParaItemsCollection.Count > 0)
				{
					WriteParagraphItems(xmlParagraphItem.MathParaItemsCollection);
				}
				break;
			}
		}
		if (flag)
		{
			m_writer.WriteEndElement();
			flag = false;
		}
	}

	private string CombineTextInSubsequentTextRanges(ParagraphItemCollection paraItemCollection, ref int index)
	{
		WTextRange wTextRange = paraItemCollection[index] as WTextRange;
		StringBuilder stringBuilder = new StringBuilder();
		if (wTextRange != null)
		{
			stringBuilder.Append(wTextRange.Text);
			while (wTextRange.NextSibling != null && wTextRange.NextSibling.EntityType == EntityType.TextRange && wTextRange.CharacterFormat.Compare(((WTextRange)wTextRange.NextSibling).CharacterFormat))
			{
				wTextRange = (WTextRange)wTextRange.NextSibling;
				stringBuilder.Append(wTextRange.Text);
				index++;
			}
		}
		return stringBuilder.ToString();
	}

	private void WriteBreak(ParagraphItem item)
	{
		if (!(item is Break @break))
		{
			return;
		}
		if (@break.BreakType == BreakType.LineBreak || @break.BreakType == BreakType.TextWrappingBreak)
		{
			if (m_currPara.ChildEntities.Count > 0 && (item == m_currPara.LastItem || IsLastItemLineBreak(@break)))
			{
				m_writer.WriteRaw("<br/>");
				m_writer.WriteRaw("<br/>");
			}
			else
			{
				m_writer.WriteRaw("<br/>");
			}
		}
		else if (@break.BreakType == BreakType.PageBreak)
		{
			m_writer.WriteRaw("<br style='clear:both;page-break-before:always'/>");
		}
	}

	private bool IsLastItemLineBreak(Break breakItem)
	{
		EntityCollection childEntities = breakItem.OwnerParagraph.ChildEntities;
		for (int num = childEntities.Count - 1; num >= 0; num--)
		{
			if (!(childEntities[num] is BookmarkStart) && !(childEntities[num] is BookmarkEnd) && !(childEntities[num] is WComment) && !(childEntities[num] is WCommentMark) && (!(childEntities[num] is WTextRange) || !string.IsNullOrEmpty((childEntities[num] as WTextRange).Text)))
			{
				return childEntities[num] == breakItem;
			}
		}
		return false;
	}

	private void WriteTextBox(WTextBox textBox)
	{
		WTable asTable = textBox.GetAsTable(0);
		asTable.SetOwner(textBox.Owner);
		asTable.Rows[0].Cells[0].CellFormat.Paddings.Left = textBox.TextBoxFormat.InternalMargin.Left;
		asTable.Rows[0].Cells[0].CellFormat.Paddings.Right = textBox.TextBoxFormat.InternalMargin.Right;
		asTable.Rows[0].Cells[0].CellFormat.Paddings.Bottom = textBox.TextBoxFormat.InternalMargin.Bottom;
		asTable.Rows[0].Cells[0].CellFormat.Paddings.Top = textBox.TextBoxFormat.InternalMargin.Top;
		if (!textBox.Visible)
		{
			asTable.TableFormat.Hidden = !textBox.Visible;
		}
		WriteTable(asTable, isTableCreatedFromTextBox: true);
	}

	private void WriteFootnote(WFootnote footnote)
	{
		if (footnote.FootnoteType == FootnoteType.Footnote)
		{
			Footnotes.Add(Footnotes.Count, footnote);
			m_writer.WriteStartElement("a");
			m_writer.WriteAttributeString("href", "#_ftn" + Footnotes.Count);
			m_writer.WriteAttributeString("name", "_ftnref" + Footnotes.Count);
			WriteFootnoteSpan(footnote.MarkerCharacterFormat);
			if (footnote.MarkerCharacterFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteStartElement("sub");
			}
			m_writer.WriteRaw("[" + m_footnotes.Count.ToString(CultureInfo.InvariantCulture) + "]");
			if (footnote.MarkerCharacterFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
		else
		{
			Endnotes.Add(Endnotes.Count, footnote);
			m_writer.WriteStartElement("a");
			m_writer.WriteAttributeString("href", "#_edn" + Endnotes.Count);
			m_writer.WriteAttributeString("name", "_ednref" + Endnotes.Count);
			WriteFootnoteSpan(footnote.MarkerCharacterFormat);
			if (footnote.MarkerCharacterFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteStartElement("sub");
			}
			m_writer.WriteRaw("[" + Endnotes.Count.ToString(CultureInfo.InvariantCulture) + "]");
			if (footnote.MarkerCharacterFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
	}

	private void WriteFootnoteSpan(WCharacterFormat charFormat)
	{
		m_writer.WriteStartElement("span");
		string text = GetStyle(charFormat, style: true);
		if (charFormat.CharStyleName != null && charFormat.CharStyleName.Length > 0)
		{
			Style style = charFormat.Document.Styles.FindByName(charFormat.CharStyleName) as Style;
			string classAttr = GetClassAttr(style);
			m_writer.WriteAttributeString("class", classAttr);
			if (!string.IsNullOrEmpty(charFormat.CharStyleName))
			{
				text = ValidateStyle(charFormat.CharStyleName, text);
			}
			if (!text.Contains("font-size") && (double)charFormat.FontSize > 0.0)
			{
				text = text + "font-size:" + charFormat.FontSize.ToString(CultureInfo.InvariantCulture) + "pt;";
			}
		}
		if (text.Length > 0)
		{
			m_writer.WriteAttributeString("style", text);
		}
	}

	private void WriteFtntAttributes(WCharacterFormat charFormat)
	{
		if (m_ftntString != null && m_ftntAttrStr != null)
		{
			m_writer.WriteStartElement("a");
			m_writer.WriteAttributeString("id", m_ftntAttrStr);
			m_writer.WriteAttributeString("href", "#" + m_ftntRefAttrStr);
			WriteFootnoteSpan(charFormat);
			if (charFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteStartElement("sub");
			}
			m_writer.WriteString(m_ftntString);
			if (charFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
			m_ftntAttrStr = null;
			m_ftntRefAttrStr = null;
			m_ftntString = null;
		}
	}

	private void WriteFormField(WFormField field)
	{
		switch (field.FieldType)
		{
		case FieldType.FieldFormCheckBox:
			if (field is WCheckBox wCheckBox)
			{
				m_writer.WriteStartElement("a");
				m_writer.WriteAttributeString("name", wCheckBox.Name);
				m_writer.WriteEndElement();
				m_writer.WriteStartElement("input");
				m_writer.WriteAttributeString("type", "checkbox");
				m_writer.WriteAttributeString("name", wCheckBox.Name);
				string style = GetStyle(wCheckBox.CharacterFormat);
				float checkBoxSize = 0f;
				SetCheckBoxSize(style, wCheckBox, ref checkBoxSize);
				if (wCheckBox.Checked)
				{
					m_writer.WriteAttributeString("checked", "checked");
				}
				m_writer.WriteEndElement();
			}
			break;
		case FieldType.FieldFormDropDown:
			if (!(field is WDropDownFormField wDropDownFormField))
			{
				break;
			}
			m_writer.WriteStartElement("a");
			m_writer.WriteAttributeString("name", wDropDownFormField.Name);
			m_writer.WriteEndElement();
			m_writer.WriteStartElement("select");
			m_writer.WriteAttributeString("name", wDropDownFormField.Name);
			foreach (WDropDownItem dropDownItem in wDropDownFormField.DropDownItems)
			{
				m_writer.WriteStartElement("option");
				if (wDropDownFormField.DropDownValue == dropDownItem.Text)
				{
					m_writer.WriteAttributeString("selected", "selected");
				}
				m_writer.WriteRaw(DocxSerializator.ReplaceInvalidSurrogateCharacters(dropDownItem.Text));
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			break;
		case FieldType.FieldFormTextInput:
			if (field is WTextFormField wTextFormField)
			{
				m_writer.WriteStartElement("a");
				m_writer.WriteAttributeString("name", wTextFormField.Name);
				m_writer.WriteEndElement();
				if (field.Document.SaveOptions.HtmlExportTextInputFormFieldAsText)
				{
					m_writer.WriteElementString("span", wTextFormField.Text);
					break;
				}
				m_writer.WriteStartElement("input");
				m_writer.WriteAttributeString("type", "text");
				m_writer.WriteAttributeString("name", wTextFormField.Name);
				m_writer.WriteAttributeString("value", wTextFormField.Text);
				m_writer.WriteEndElement();
			}
			break;
		}
	}

	private void WriteParagraphOrList(WParagraph para)
	{
		WListFormat listFormat = GetListFormat(para);
		if (!m_bIsPreserveListAsPara)
		{
			CloseList(GetLevelNumer(listFormat), para);
		}
		m_prefixedValue = string.Empty;
		m_bIsPrefixedList = false;
		m_bIsWriteListTab = false;
		m_bIsParaWithinDivision = false;
		string style = string.Empty;
		m_currListLevel = GetLevelNumer(listFormat);
		bool flag = false;
		bool flag2 = false;
		if (!para.IsInCell)
		{
			EnsureWithinDivision(para);
		}
		WParagraphStyle wParagraphStyle = m_document.Styles.FindByName(para.StyleName) as WParagraphStyle;
		if (wParagraphStyle.ParagraphFormat.BackColor != Color.Empty && wParagraphStyle.ParagraphFormat.BackColor != Color.White && ((para.PreviousSibling is WParagraph && (para.PreviousSibling as WParagraph).StyleName != wParagraphStyle.Name) || para.PreviousSibling == null || !(para.PreviousSibling is WParagraph)))
		{
			m_writer.WriteStartElement("div");
			m_writer.WriteAttributeString("style", "background-color:#" + wParagraphStyle.ParagraphFormat.BackColor.Name.Substring(2) + ";");
		}
		string text;
		if (para.SectionEndMark || listFormat.ListType == ListType.NoList || (listFormat.CurrentListLevel != null && listFormat.CurrentListLevel.PatternType == ListPatternType.None))
		{
			if (para.ParaStyle != null)
			{
				text = EncodeName(para.ParaStyle.Name).ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length != 8)
					{
						if (length == 9)
						{
							switch (text[8])
							{
							case '1':
								break;
							case '2':
								goto IL_02d3;
							case '3':
								goto IL_030b;
							case '4':
								goto IL_0343;
							case '5':
								goto IL_037b;
							case '6':
								goto IL_03b0;
							default:
								goto IL_0448;
							}
							switch (text)
							{
							case "heading-1":
							case "heading 1":
							case "heading_1":
								break;
							default:
								goto IL_0448;
							}
							goto IL_03dc;
						}
					}
					else
					{
						switch (text[7])
						{
						case '1':
							break;
						case '2':
							goto IL_022d;
						case '3':
							goto IL_0243;
						case '4':
							goto IL_0259;
						case '5':
							goto IL_026f;
						case '6':
							goto IL_0285;
						default:
							goto IL_0448;
						}
						if (text == "heading1")
						{
							goto IL_03dc;
						}
					}
				}
				goto IL_0448;
			}
			m_writer.WriteStartElement("p");
			goto IL_046a;
		}
		if (m_bIsPreserveListAsPara || IsPreserveListAsParagraph(listFormat))
		{
			m_currListLevel = -1;
			PreserveListAsPara(listFormat, para);
			flag2 = true;
		}
		else
		{
			style = WriteList(listFormat, para);
		}
		goto IL_0539;
		IL_0412:
		m_writer.WriteStartElement("h4");
		goto IL_046a;
		IL_037b:
		switch (text)
		{
		case "heading-5":
		case "heading 5":
		case "heading_5":
			break;
		default:
			goto IL_0448;
		}
		goto IL_0424;
		IL_0436:
		m_writer.WriteStartElement("h6");
		goto IL_046a;
		IL_0424:
		m_writer.WriteStartElement("h5");
		goto IL_046a;
		IL_022d:
		if (text == "heading2")
		{
			goto IL_03ee;
		}
		goto IL_0448;
		IL_0243:
		if (text == "heading3")
		{
			goto IL_0400;
		}
		goto IL_0448;
		IL_0285:
		if (text == "heading6")
		{
			goto IL_0436;
		}
		goto IL_0448;
		IL_026f:
		if (text == "heading5")
		{
			goto IL_0424;
		}
		goto IL_0448;
		IL_03b0:
		switch (text)
		{
		case "heading-6":
		case "heading 6":
		case "heading_6":
			break;
		default:
			goto IL_0448;
		}
		goto IL_0436;
		IL_0259:
		if (text == "heading4")
		{
			goto IL_0412;
		}
		goto IL_0448;
		IL_0539:
		if (m_bIsPreserveListAsPara || listFormat.ListType == ListType.NoList || flag2)
		{
			return;
		}
		if (!flag)
		{
			WriteParaStyle(para, style, listFormat);
		}
		if (m_bIsPrefixedList)
		{
			m_currListLevel = -1;
			m_writer.WriteStartElement("span");
			if (m_currListCharFormat != null)
			{
				string style2 = GetStyle(m_currListCharFormat);
				m_writer.WriteAttributeString("style", style2);
			}
			m_writer.WriteRaw(m_prefixedValue);
			m_writer.WriteEndElement();
			m_currListCharFormat = null;
		}
		return;
		IL_046a:
		if (para.ParagraphFormat.Bidi)
		{
			m_writer.WriteAttributeString("dir", "rtl");
		}
		else if (para.IsInCell && (para.GetOwnerTable(para) as WTable).TableFormat.Bidi)
		{
			m_writer.WriteAttributeString("dir", "ltr");
		}
		if (!isKeepValue)
		{
			style = GetStyle(para.ParagraphFormat, isListLevel: false, m_bIsPreserveListAsPara, null);
		}
		else
		{
			if (!m_bIsParaWithinDivision)
			{
				CreateNavigationPoint(para);
			}
			style = ValidateStyle(para.StyleName, style);
		}
		WriteParaStyle(para, style, listFormat);
		flag = true;
		goto IL_0539;
		IL_03dc:
		m_writer.WriteStartElement("h1");
		goto IL_046a;
		IL_03ee:
		m_writer.WriteStartElement("h2");
		goto IL_046a;
		IL_02d3:
		switch (text)
		{
		case "heading-2":
		case "heading 2":
		case "heading_2":
			break;
		default:
			goto IL_0448;
		}
		goto IL_03ee;
		IL_0400:
		m_writer.WriteStartElement("h3");
		goto IL_046a;
		IL_0343:
		switch (text)
		{
		case "heading-4":
		case "heading 4":
		case "heading_4":
			break;
		default:
			goto IL_0448;
		}
		goto IL_0412;
		IL_030b:
		switch (text)
		{
		case "heading-3":
		case "heading 3":
		case "heading_3":
			break;
		default:
			goto IL_0448;
		}
		goto IL_0400;
		IL_0448:
		m_writer.WriteStartElement("p");
		goto IL_046a;
	}

	private bool IsPreserveListAsParagraph(WListFormat listFormat)
	{
		if (m_document.SaveOptions.HTMLExportWithWordCompatibility && listFormat.CurrentListLevel != null)
		{
			return listFormat.CurrentListLevel.PatternType == ListPatternType.Bullet;
		}
		return false;
	}

	private void EnsureWithinDivision(WParagraph para)
	{
		if (para.ParagraphFormat.Keep && !isKeepValue)
		{
			Borders borders = para.ParagraphFormat.Borders;
			if (borders.Left.IsBorderDefined && borders.Right.IsBorderDefined && borders.Top.IsBorderDefined && borders.Bottom.IsBorderDefined)
			{
				m_writer.WriteStartElement("div");
				string style = GetStyle(para.ParagraphFormat, isListLevel: false, m_bIsPreserveListAsPara, null);
				if (para.Document.Styles.FindByName(para.StyleName) is Style style2 && !IsHeadingStyleNeedToPreserveAsElementSelector(style2.Name))
				{
					string value = EncodeName(style2.Name);
					m_writer.WriteAttributeString("class", value);
				}
				if (!string.IsNullOrEmpty(para.StyleName))
				{
					CreateNavigationPoint(para);
				}
				if (!string.IsNullOrEmpty(style))
				{
					style = ValidateStyle(para.StyleName, style);
					if (style != string.Empty)
					{
						m_writer.WriteAttributeString("style", style);
					}
				}
				isKeepValue = true;
				m_bIsParaWithinDivision = true;
			}
		}
		if (!para.ParagraphFormat.Keep && isKeepValue)
		{
			m_writer.WriteEndElement();
			isKeepValue = false;
		}
	}

	private void WriteParaStyle(WParagraph para, string style, WListFormat listFormat)
	{
		if (!string.IsNullOrEmpty(para.StyleName) && para.StyleName != "Normal" && !isKeepValue)
		{
			Style style2 = para.Document.Styles.FindByName(para.StyleName) as Style;
			if ((style2 != null && !IsHeadingStyleNeedToPreserveAsElementSelector(para.StyleName)) || (listFormat != null && listFormat.ListType != ListType.NoList))
			{
				string value = EncodeName(style2.Name);
				if (m_document.SaveOptions.HtmlExportCssStyleSheetType != 0)
				{
					m_writer.WriteAttributeString("class", value);
				}
			}
			CreateNavigationPoint(para);
			if (m_document.SaveOptions.HtmlExportCssStyleSheetType != 0)
			{
				style = ValidateStyle(para.StyleName, style);
				style = AddInlineDecorationStyle(para, style);
			}
		}
		if (style.Length > 0)
		{
			m_writer.WriteAttributeString("style", style);
		}
	}

	private string AddInlineDecorationStyle(WParagraph para, string style)
	{
		if (para.ChildEntities.Count == 1)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			if (para.ChildEntities.LastItem is WTextRange)
			{
				text += GetStyle((para.ChildEntities.LastItem as WTextRange).CharacterFormat);
			}
			if (text.ToLower().Contains("text-decoration"))
			{
				string[] array = text.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].ToLower().Contains("text-decoration"))
					{
						text2 = array[i];
					}
				}
			}
			if (style.ToLower().Contains("text-decoration"))
			{
				string[] array2 = style.Split(';');
				style = string.Empty;
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].ToLower().Contains("text-decoration"))
					{
						array2[j] = text2;
					}
					style = style + array2[j] + ";";
				}
			}
			else
			{
				style += text2;
			}
		}
		return style;
	}

	private bool IsHeadingStyleNeedToPreserveAsElementSelector(string styleName)
	{
		string text = EncodeName(styleName).ToLowerInvariant();
		if (text != null)
		{
			int length = text.Length;
			if (length != 8)
			{
				if (length == 9)
				{
					switch (text[8])
					{
					case '1':
						break;
					case '2':
						goto IL_0127;
					case '3':
						goto IL_0143;
					case '4':
						goto IL_015f;
					case '5':
						goto IL_017b;
					case '6':
						goto IL_0197;
					default:
						goto IL_01b3;
					}
					if (text == "heading-1" || text == "heading 1")
					{
						goto IL_01b1;
					}
				}
			}
			else
			{
				switch (text[7])
				{
				case '1':
					break;
				case '2':
					goto IL_0099;
				case '3':
					goto IL_00ae;
				case '4':
					goto IL_00c3;
				case '5':
					goto IL_00d8;
				case '6':
					goto IL_00ed;
				default:
					goto IL_01b3;
				}
				if (text == "heading1")
				{
					goto IL_01b1;
				}
			}
		}
		goto IL_01b3;
		IL_0127:
		if (text == "heading-2" || text == "heading 2")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_0143:
		if (text == "heading-3" || text == "heading 3")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_01b3:
		return false;
		IL_0099:
		if (text == "heading2")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_00ae:
		if (text == "heading3")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_00c3:
		if (text == "heading4")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_00d8:
		if (text == "heading5")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_00ed:
		if (text == "heading6")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_01b1:
		return true;
		IL_017b:
		if (text == "heading-5" || text == "heading 5")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_015f:
		if (text == "heading-4" || text == "heading 4")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
		IL_0197:
		if (text == "heading-6" || text == "heading 6")
		{
			goto IL_01b1;
		}
		goto IL_01b3;
	}

	private string WriteList(WListFormat listFormat, WParagraph para)
	{
		string text = null;
		int startAt = 0;
		if (listFormat.CurrentListLevel.PatternType != ListPatternType.Bullet)
		{
			startAt = GetStartValue(listFormat);
		}
		WriteListStartTag(listFormat, startAt);
		if (para.ParagraphFormat.Bidi)
		{
			m_writer.WriteStartElement("div");
			m_writer.WriteAttributeString("dir", "rtl");
		}
		else if (para.IsInCell && (para.GetOwnerTable(para) as WTable).TableFormat.Bidi)
		{
			m_writer.WriteStartElement("div");
			m_writer.WriteAttributeString("dir", "ltr");
		}
		m_writer.WriteStartElement(m_bIsPrefixedList ? "p" : "li");
		string text2 = GetStyle(para.ParagraphFormat, isListLevel: true, m_bIsPreserveListAsPara, listFormat);
		if (listFormat.CurrentListLevel != null)
		{
			WCharacterFormat characterFormatOfList = GetCharacterFormatOfList(para);
			if (characterFormatOfList != null)
			{
				text = GetStyle(characterFormatOfList);
			}
			else
			{
				if (para.BreakCharacterFormat != null)
				{
					text2 += GetStyle(para.BreakCharacterFormat);
				}
				text = GetStyle(listFormat.CurrentListLevel.CharacterFormat);
			}
			text = text.Replace("font-family:'Wingdings';", string.Empty);
		}
		if (text != null)
		{
			text2 = EnsureStyle(text, text2);
		}
		return text2;
	}

	private WCharacterFormat GetCharacterFormatOfList(WParagraph paragraph)
	{
		if (paragraph.ListFormat.IsEmptyList || paragraph.SectionEndMark)
		{
			return null;
		}
		WCharacterFormat wCharacterFormat = null;
		WListFormat wListFormat = null;
		WParagraphStyle wParagraphStyle = paragraph.ParaStyle as WParagraphStyle;
		if (paragraph.ListFormat.ListType != ListType.NoList)
		{
			wListFormat = paragraph.ListFormat;
		}
		else if (wParagraphStyle != null && wParagraphStyle.ListFormat.ListType != ListType.NoList)
		{
			wListFormat = wParagraphStyle.ListFormat;
		}
		if (wListFormat != null && wListFormat.CurrentListStyle != null)
		{
			ListStyle currentListStyle = wListFormat.CurrentListStyle;
			int levelNumber = 0;
			if (paragraph.ListFormat.HasKey(0))
			{
				levelNumber = paragraph.ListFormat.ListLevelNumber;
			}
			else if (wParagraphStyle != null && wParagraphStyle.ListFormat.HasKey(0))
			{
				levelNumber = wParagraphStyle.ListFormat.ListLevelNumber;
			}
			WListLevel wListLevel = currentListStyle.GetNearLevel(levelNumber);
			ListOverrideStyle listOverrideStyle = null;
			if (!string.IsNullOrEmpty(wListFormat.LFOStyleName))
			{
				listOverrideStyle = m_document.ListOverrides.FindByName(wListFormat.LFOStyleName);
			}
			if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(levelNumber) && listOverrideStyle.OverrideLevels[levelNumber].OverrideFormatting)
			{
				wListLevel = listOverrideStyle.OverrideLevels[levelNumber].OverrideListLevel;
			}
			wCharacterFormat = new WCharacterFormat(m_document);
			wCharacterFormat.ImportContainer(paragraph.BreakCharacterFormat);
			wCharacterFormat.CopyProperties(paragraph.BreakCharacterFormat);
			wCharacterFormat.ApplyBase(paragraph.BreakCharacterFormat.BaseFormat);
			if (wCharacterFormat.PropertiesHash.ContainsKey(7))
			{
				wCharacterFormat.UnderlineStyle = UnderlineStyle.None;
				wCharacterFormat.PropertiesHash.Remove(7);
			}
			CopyCharacterFormatting(wListLevel.CharacterFormat, wCharacterFormat);
		}
		return wCharacterFormat;
	}

	private void CopyCharacterFormatting(WCharacterFormat sourceFormat, WCharacterFormat destFormat)
	{
		if (sourceFormat.HasValue(3))
		{
			destFormat.SetPropertyValue(3, sourceFormat.FontSize);
		}
		if (sourceFormat.HasValue(1))
		{
			destFormat.TextColor = sourceFormat.TextColor;
		}
		if (sourceFormat.HasValue(2))
		{
			destFormat.FontName = sourceFormat.FontName;
		}
		if (sourceFormat.HasValue(4))
		{
			destFormat.Bold = sourceFormat.Bold;
		}
		if (sourceFormat.HasValue(5))
		{
			destFormat.Italic = sourceFormat.Italic;
		}
		if (sourceFormat.HasValue(7))
		{
			destFormat.UnderlineStyle = sourceFormat.UnderlineStyle;
		}
		if (sourceFormat.HasValue(63))
		{
			destFormat.HighlightColor = sourceFormat.HighlightColor;
		}
		if (sourceFormat.HasValue(50))
		{
			destFormat.Shadow = sourceFormat.Shadow;
		}
		if (sourceFormat.HasValue(18))
		{
			destFormat.SetPropertyValue(18, sourceFormat.CharacterSpacing);
		}
		if (sourceFormat.HasValue(14))
		{
			destFormat.DoubleStrike = sourceFormat.DoubleStrike;
		}
		if (sourceFormat.HasValue(51))
		{
			destFormat.Emboss = sourceFormat.Emboss;
		}
		if (sourceFormat.HasValue(52))
		{
			destFormat.Engrave = sourceFormat.Engrave;
		}
		if (sourceFormat.HasValue(10))
		{
			destFormat.SubSuperScript = sourceFormat.SubSuperScript;
		}
		destFormat.TextBackgroundColor = sourceFormat.TextBackgroundColor;
		if (sourceFormat.HasValue(54))
		{
			destFormat.AllCaps = sourceFormat.AllCaps;
		}
		if (sourceFormat.Bidi)
		{
			destFormat.Bidi = true;
			destFormat.FontNameBidi = sourceFormat.FontNameBidi;
			destFormat.SetPropertyValue(62, sourceFormat.FontSizeBidi);
		}
		if (sourceFormat.HasValue(59))
		{
			destFormat.BoldBidi = sourceFormat.BoldBidi;
		}
		if (sourceFormat.HasValue(109))
		{
			destFormat.FieldVanish = sourceFormat.FieldVanish;
		}
		if (sourceFormat.HasValue(53))
		{
			destFormat.Hidden = sourceFormat.Hidden;
		}
		if (sourceFormat.HasValue(24))
		{
			destFormat.SpecVanish = sourceFormat.SpecVanish;
		}
		if (sourceFormat.HasValue(55))
		{
			destFormat.SmallCaps = sourceFormat.SmallCaps;
		}
	}

	private string EnsureStyle(string currentStyle, string existingStyle)
	{
		string[] array = existingStyle.Split(new char[1] { ';' });
		string[] array2 = currentStyle.Split(new char[1] { ';' });
		foreach (string text in array2)
		{
			bool flag = false;
			if (text.Length <= 0)
			{
				continue;
			}
			int num = text.IndexOf(":");
			string text2 = text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			string[] array3 = array;
			foreach (string text4 in array3)
			{
				if (text4.Length > 0)
				{
					int num2 = text4.IndexOf(":");
					if (text4.Substring(0, num2).ToLower() == text2.ToLower())
					{
						flag = true;
						string oldValue = text4.Substring(num2 + 1);
						string newValue = text4.Replace(oldValue, text3);
						existingStyle = existingStyle.Replace(text4, newValue);
					}
				}
			}
			if (!flag)
			{
				existingStyle = existingStyle + text2 + ":" + text3 + ";";
			}
		}
		return existingStyle;
	}

	private string ValidateStyle(string p, string style)
	{
		if (!string.IsNullOrEmpty(p) && m_stylesColl.ContainsKey(p))
		{
			string[] array = m_stylesColl[p].Split(new char[1] { ';' });
			foreach (string text in array)
			{
				if (text.Length > 0 && style.Length > 0)
				{
					style = style.Replace(text + ";", string.Empty);
				}
			}
		}
		return style;
	}

	private void WriteListStartTag(WListFormat listFormat, int startAt)
	{
		bool flag = false;
		if (m_currListLevel < 0)
		{
			return;
		}
		bool flag2 = false;
		bool flag3 = false;
		if (!(flag2 = listStack.Contains(m_currListLevel)) && listFormat.CurrentListLevel.PatternType == ListPatternType.Bullet)
		{
			listStack.Push(m_currListLevel);
			flag = true;
		}
		if (listFormat.CurrentListLevel.PatternType == ListPatternType.Bullet)
		{
			if (flag)
			{
				m_writer.WriteStartElement("ul");
				WriteListType(listFormat.CurrentListLevel.PatternType, listFormat);
				m_writer.WriteAttributeString("style", "margin:0pt; padding-left:0pt");
			}
			m_writer.WriteRaw(ControlChar.CrLf);
		}
		else if ((flag3 = string.IsNullOrEmpty(listFormat.CurrentListLevel.NumberPrefix)) || !listFormat.CurrentListLevel.NumberPrefix.StartsWith("\0."))
		{
			if (flag3 && !string.IsNullOrEmpty(listFormat.CurrentListLevel.NumberSuffix) && listFormat.CurrentListLevel.NumberSuffix != ".")
			{
				flag3 = false;
			}
			if (flag3)
			{
				if (!flag2)
				{
					listStack.Push(m_currListLevel);
					m_writer.WriteStartElement("ol");
					WriteListType(listFormat.CurrentListLevel.PatternType, listFormat);
					if (startAt >= 0)
					{
						m_writer.WriteAttributeString("start", startAt.ToString(CultureInfo.InvariantCulture));
					}
					m_writer.WriteAttributeString("style", "margin:0pt; padding-left:0pt");
				}
				m_writer.WriteRaw(ControlChar.CrLf);
			}
			else
			{
				WListLevel listLevel = m_currPara.GetListLevel(listFormat);
				m_prefixedValue = m_document.UpdateListValue(m_currPara, listFormat, listLevel);
				m_bIsPrefixedList = true;
				m_bIsWriteListTab = true;
			}
		}
		else
		{
			m_bIsPrefixedList = true;
			m_prefixedValue = GetPrefixValue(listFormat, startAt);
			m_bIsWriteListTab = true;
		}
	}

	private int GetStartValue(WListFormat listFormat)
	{
		if (listFormat.RestartNumbering)
		{
			EnsureLvlRestart(listFormat, fullRestart: true);
		}
		else if (listFormat.ListLevelNumber == 0)
		{
			EnsureLvlRestart(listFormat, fullRestart: false);
		}
		return GetLstStartVal(listFormat);
	}

	private void PreserveListAsPara(WListFormat listFormat, WParagraph para)
	{
		int startValue = GetStartValue(listFormat);
		m_writer.WriteStartElement("p");
		if (para.ParagraphFormat.Bidi)
		{
			m_writer.WriteAttributeString("dir", "rtl");
		}
		string style = GetStyle(para.ParagraphFormat, isListLevel: true, m_bIsPreserveListAsPara || IsPreserveListAsParagraph(listFormat), listFormat);
		if (listFormat.CurrentListLevel.NumberPrefix != null && listFormat.CurrentListLevel.NumberPrefix.StartsWith("\0."))
		{
			m_bIsPrefixedList = true;
			m_prefixedValue = GetPrefixValue(listFormat, startValue);
		}
		WriteParaStyle(para, style, listFormat);
		PreserveBulletsAndNumberingAsText(listFormat, startValue);
	}

	private void PreserveBulletsAndNumberingAsText(WListFormat listFormat, int startAt)
	{
		WCharacterFormat wCharacterFormat = (IsPreserveListAsParagraph(listFormat) ? GetCharacterFormatOfList(listFormat.OwnerParagraph) : listFormat.CurrentListLevel.CharacterFormat);
		string style = GetStyle((wCharacterFormat != null) ? wCharacterFormat : listFormat.CurrentListLevel.CharacterFormat);
		m_writer.WriteStartElement("span");
		m_writer.WriteAttributeString("style", style);
		if (listFormat.OwnerParagraph.ParagraphFormat.Bidi)
		{
			m_writer.WriteStartElement("span");
			m_writer.WriteAttributeString("dir", "rtl");
		}
		if (listFormat.CurrentListLevel.PatternType == ListPatternType.Bullet)
		{
			if (listFormat.CurrentListLevel.CharacterFormat.FontName.ToLowerInvariant() == "symbol" || listFormat.CurrentListLevel.CharacterFormat.FontName.ToLowerInvariant() == "wingdings")
			{
				byte b = (byte)listFormat.CurrentListLevel.BulletCharacter[0];
				m_writer.WriteRaw("&#" + b.ToString(CultureInfo.InvariantCulture) + ";");
			}
			else
			{
				m_writer.WriteRaw(listFormat.CurrentListLevel.BulletCharacter);
			}
		}
		else if (m_bIsPrefixedList)
		{
			m_writer.WriteRaw(m_prefixedValue);
		}
		else
		{
			string numberingsAsText = GetNumberingsAsText(listFormat.CurrentListLevel.PatternType, startAt);
			m_writer.WriteRaw(DocxSerializator.ReplaceInvalidSurrogateCharacters(numberingsAsText));
		}
		if (!m_bIsPrefixedList && listFormat.CurrentListLevel.NumberSuffix != null)
		{
			m_writer.WriteRaw(DocxSerializator.ReplaceInvalidSurrogateCharacters(listFormat.CurrentListLevel.NumberSuffix));
		}
		m_writer.WriteEndElement();
		if (listFormat.OwnerParagraph.ParagraphFormat.Bidi)
		{
			m_writer.WriteFullEndElement();
		}
		WriteTabSpace(listFormat);
	}

	private void WriteTabSpace(WListFormat listFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		float textPosition = listFormat.CurrentListLevel.TextPosition;
		float num = listFormat.CurrentListLevel.TextPosition + listFormat.CurrentListLevel.NumberPosition;
		int num2 = ((!(listFormat.CurrentListLevel.TabSpaceAfter <= 0f)) ? ((int)Math.Round((double)(listFormat.CurrentListLevel.TabSpaceAfter - num) / 36.0)) : ((int)Math.Round((double)(textPosition - num) / 36.0)));
		m_writer.WriteStartElement("span");
		stringBuilder.Append("font-size:" + Math.Round(0.5833333134651184, 2).ToString(CultureInfo.InvariantCulture) + "em;");
		stringBuilder.Append("font-family:'Times New Roman';");
		m_writer.WriteAttributeString("style", stringBuilder.ToString());
		if (num2 > 0)
		{
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < 22; j++)
				{
					m_writer.WriteRaw("&#xa0;");
				}
			}
		}
		else if (IsPreserveListAsParagraph(listFormat))
		{
			m_writer.WriteRaw("&#xa0;&#xa0;&#xa0;&#xa0;&#xa0;");
		}
		else
		{
			m_writer.WriteRaw("&#xa0;&#xa0;&#xa0;&#xa0;&#xa0;&#xa0;&#xa0;");
		}
		m_writer.WriteEndElement();
	}

	private string GetNumberingsAsText(ListPatternType type, int startAt)
	{
		switch (type)
		{
		case ListPatternType.LowLetter:
		{
			int num = 97 + (startAt - 1);
			return string.Format(CultureInfo.InvariantCulture, ((char)num).ToString());
		}
		case ListPatternType.UpLetter:
		{
			int num = 65 + (startAt - 1);
			return string.Format(CultureInfo.InvariantCulture, ((char)num).ToString());
		}
		case ListPatternType.LowRoman:
			return ConvertArabicToRoman(startAt).ToLowerInvariant();
		case ListPatternType.UpRoman:
			return ConvertArabicToRoman(startAt).ToUpperInvariant();
		default:
			return startAt.ToString(CultureInfo.InvariantCulture);
		}
	}

	private string ConvertArabicToRoman(int arabic)
	{
		string text = "";
		for (int i = 0; i < arabic; i++)
		{
			while (arabic >= 1000)
			{
				text += "M";
				arabic -= 1000;
			}
			while (arabic >= 900)
			{
				text += "CM";
				arabic -= 900;
			}
			while (arabic >= 500)
			{
				text += "D";
				arabic -= 500;
			}
			while (arabic >= 400)
			{
				text += "CD";
				arabic -= 400;
			}
			while (arabic >= 100)
			{
				text += "C";
				arabic -= 100;
			}
			while (arabic >= 90)
			{
				text += "XC";
				arabic -= 90;
			}
			while (arabic >= 50)
			{
				text += "L";
				arabic -= 50;
			}
			while (arabic >= 40)
			{
				text += "XL";
				arabic -= 40;
			}
			while (arabic >= 10)
			{
				text += "X";
				arabic -= 10;
			}
			while (arabic >= 9)
			{
				text += "IX";
				arabic -= 9;
			}
			while (arabic >= 5)
			{
				text += "V";
				arabic -= 5;
			}
			while (arabic >= 4)
			{
				text += "IV";
				arabic -= 4;
			}
			while (arabic >= 1)
			{
				text += "I";
				arabic--;
			}
		}
		return text;
	}

	private string GetPrefixValue(WListFormat listFormat, int startAt)
	{
		string result = string.Empty;
		int levelNumber = listFormat.CurrentListLevel.LevelNumber;
		if (Lists.ContainsKey(listFormat.CustomStyleName))
		{
			string text = string.Empty;
			Dictionary<int, int> dictionary = Lists[listFormat.CustomStyleName];
			for (int i = 0; i < levelNumber; i++)
			{
				if (dictionary.ContainsKey(i))
				{
					text = text + (Convert.ToInt32(dictionary[i]) - 1).ToString(CultureInfo.InvariantCulture) + ".";
				}
			}
			text += startAt.ToString(CultureInfo.InvariantCulture);
			result = text + listFormat.CurrentListLevel.NumberSuffix;
		}
		return result;
	}

	private void WriteBookmark(BookmarkStart bookmark)
	{
		if (!(bookmark.Name == "_GoBack"))
		{
			m_writer.WriteStartElement("a");
			m_writer.WriteAttributeString("name", bookmark.Name);
			if (IsParaHasOnlyBookmarks(bookmark))
			{
				m_writer.WriteRaw("&#xa0;");
			}
			else
			{
				m_writer.WriteRaw(string.Empty);
			}
		}
	}

	private bool IsParaHasOnlyBookmarks(BookmarkStart bookmarkstart)
	{
		WParagraph ownerParagraph = bookmarkstart.OwnerParagraph;
		if (string.IsNullOrEmpty(ownerParagraph.Text))
		{
			EntityCollection entityCollection = ownerParagraph.ChildEntities;
			if (bookmarkstart.Owner is InlineContentControl)
			{
				entityCollection = (bookmarkstart.Owner as InlineContentControl).ParagraphItems;
			}
			for (int num = entityCollection.Count - 1; num >= 0; num--)
			{
				Entity entity = entityCollection[num];
				if (!(entity is BookmarkStart) && !(entity is BookmarkEnd))
				{
					break;
				}
				if (entity is BookmarkStart && (entity as BookmarkStart).Name != "_GoBack")
				{
					return entity == bookmarkstart;
				}
			}
		}
		return false;
	}

	private void WriteField(WField field)
	{
		if (field.FieldEnd == null)
		{
			return;
		}
		if (field.FieldType == FieldType.FieldHyperlink)
		{
			Hyperlink hyperlink = new Hyperlink(field);
			WriteHyperlink(hyperlink);
			m_bSkipFieldItem = true;
		}
		else
		{
			InsertFieldBegin(field);
			if (field.FieldType == FieldType.FieldEmbed)
			{
				m_bSkipFieldItem = true;
			}
		}
		PushToFieldStack(field);
	}

	private void PushToFieldStack(WField field)
	{
		FieldStack.Push(field);
	}

	private void InsertFieldBegin(WField field)
	{
		_ = string.Empty;
		if (field.CharacterFormat != null)
		{
			GetStyle(field.CharacterFormat);
		}
		if (FieldStack.Count == 0 || CurrentField.IsInFieldResult)
		{
			m_writer.WriteRaw("<!--[if supportFields]>");
		}
		m_writer.WriteStartElement("span");
		m_writer.WriteAttributeString("style", "mso-element:field-begin");
		m_writer.WriteRaw("");
		m_writer.WriteFullEndElement();
	}

	private void WriteFieldMark(WFieldMark fieldMark)
	{
		bool flag = PreviousField != null && PreviousField.IsInFieldResult;
		if (CurrentField == null)
		{
			return;
		}
		if (fieldMark.Type == FieldMarkType.FieldSeparator)
		{
			m_writer.WriteStartElement("span");
			m_writer.WriteAttributeString("style", "mso-element:field-separator");
			m_writer.WriteRaw("");
			m_writer.WriteEndElement();
			if (FieldStack.Count == 1 || flag)
			{
				m_writer.WriteRaw("<![endif]-->");
				CurrentField.IsInFieldResult = true;
			}
		}
		else
		{
			if (fieldMark.Type != FieldMarkType.FieldEnd)
			{
				return;
			}
			if (CurrentField.FieldType != FieldType.FieldTOC)
			{
				if (FieldStack.Count == 1 || flag)
				{
					m_writer.WriteRaw("<!--[if supportFields]>");
				}
				m_writer.WriteStartElement("span");
				m_writer.WriteAttributeString("style", "mso-element:field-end");
				m_writer.WriteRaw("");
				m_writer.WriteFullEndElement();
				if (FieldStack.Count == 1 || flag)
				{
					m_writer.WriteRaw("<![endif]-->");
					CurrentField.IsInFieldResult = false;
				}
			}
			FieldStack.Pop();
		}
	}

	private void WriteHyperlink(Hyperlink hyperlink)
	{
		m_writer.WriteStartElement("a");
		string charStyleName = hyperlink.Field.CharacterFormat.CharStyleName;
		if (charStyleName != null && charStyleName.Length > 0)
		{
			Style style = hyperlink.Field.Document.Styles.FindByName(charStyleName) as Style;
			string classAttr = GetClassAttr(style);
			m_writer.WriteAttributeString("class", classAttr);
		}
		WField field = hyperlink.Field;
		WTextRange wTextRange = new WTextRange(field.Document);
		if (field.FieldSeparator != null && field.FieldSeparator.NextSibling is WTextRange)
		{
			wTextRange = field.FieldSeparator.NextSibling as WTextRange;
		}
		string text = GetStyle(wTextRange.CharacterFormat, style: false);
		if (!string.IsNullOrEmpty(charStyleName))
		{
			text = ValidateStyle(charStyleName, text);
		}
		if (text.Length > 0)
		{
			m_writer.WriteAttributeString("style", text);
		}
		string text2 = string.Empty;
		if (field.IsLocal && field.LocalReference != null && field.LocalReference != string.Empty)
		{
			text2 = "#" + field.LocalReference;
		}
		switch (hyperlink.Type)
		{
		case HyperlinkType.FileLink:
			m_writer.WriteAttributeString("href", hyperlink.FilePath + text2);
			break;
		case HyperlinkType.WebLink:
			m_writer.WriteAttributeString("href", hyperlink.Uri + text2);
			break;
		case HyperlinkType.EMailLink:
			m_writer.WriteAttributeString("href", hyperlink.Uri);
			break;
		case HyperlinkType.Bookmark:
			m_writer.WriteAttributeString("href", "#" + hyperlink.BookmarkName);
			break;
		case HyperlinkType.None:
			break;
		}
	}

	private void WriteImage(WPicture pic)
	{
		_ = pic.Image;
		DocGen.DocIO.DLS.Entities.ImageFormat format = pic.Image.Format;
		_ = "." + format.ToString().ToLowerInvariant();
		int num = 0;
		int num2 = 0;
		if (Math.Round(pic.WidthScale) > 0.0 || Math.Round(pic.HeightScale) > 0.0)
		{
			num = (int)Math.Round(UnitsConvertor.Instance.ConvertToPixels(pic.Width, PrintUnits.Point));
			num2 = (int)Math.Round(UnitsConvertor.Instance.ConvertToPixels(pic.Height, PrintUnits.Point));
		}
		else
		{
			num = Convert.ToInt32(pic.Size.Width);
			num2 = Convert.ToInt32(pic.Size.Height);
		}
		if (num < 0 || num2 < 0)
		{
			return;
		}
		if (pic.TextWrappingStyle == TextWrappingStyle.InFrontOfText || pic.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			int num3 = pic.OrderIndex;
			if (BehindWrapStyleFloatingItems.ContainsKey(pic))
			{
				num3 = BehindWrapStyleFloatingItems[pic];
			}
			m_writer.WriteStartElement("span");
			if (pic.VerticalAlignment == ShapeVerticalAlignment.Center && pic.HorizontalAlignment == ShapeHorizontalAlignment.Center)
			{
				m_writer.WriteAttributeString("style", "position:" + ShapePosition.Absolute.ToString() + "; width:" + num + "px; height:" + num2 + "px; left:0px; margin-left:0px; margin-top:0px;z-index:" + num3);
			}
			else if (pic.HorizontalAlignment == ShapeHorizontalAlignment.Right)
			{
				int num4 = 1024 - num;
				m_writer.WriteAttributeString("style", "position:" + ShapePosition.Absolute.ToString() + "; width:" + num + "px; height:" + num2 + "px; left:0px; margin-left:" + num4 + "px; margin-top:" + Math.Round(UnitsConvertor.Instance.ConvertToPixels(pic.VerticalPosition, PrintUnits.Point)) + "px;z-index:" + num3);
			}
			else
			{
				m_writer.WriteAttributeString("style", "position:" + ShapePosition.Absolute.ToString() + "; width:" + num + "px; height:" + num2 + "px; left:0px; margin-left:" + Math.Round(UnitsConvertor.Instance.ConvertToPixels(pic.HorizontalPosition, PrintUnits.Point)) + "px; margin-top:" + Math.Round(UnitsConvertor.Instance.ConvertToPixels(pic.VerticalPosition, PrintUnits.Point)) + "px;z-index:" + num3);
			}
		}
		m_writer.WriteStartElement("img");
		ImageNodeVisitedEventArgs imageNodeVisitedEventArgs = null;
		imageNodeVisitedEventArgs = pic.Document.SaveOptions.ExecuteSaveImageEvent(new MemoryStream(pic.ImageBytes), null);
		if (imageNodeVisitedEventArgs != null && !string.IsNullOrEmpty(imageNodeVisitedEventArgs.Uri))
		{
			m_writer.WriteAttributeString("src", imageNodeVisitedEventArgs.Uri);
		}
		else
		{
			string text = "data:image/" + format.ToString().ToLowerInvariant();
			string text2 = Convert.ToBase64String(pic.ImageBytes);
			m_writer.WriteAttributeString("src", text + ";base64," + text2);
		}
		m_writer.WriteAttributeString("width", num.ToString(CultureInfo.InvariantCulture));
		m_writer.WriteAttributeString("height", num2.ToString(CultureInfo.InvariantCulture));
		if (pic.TextWrappingStyle == TextWrappingStyle.Square || pic.TextWrappingStyle == TextWrappingStyle.Tight || pic.TextWrappingStyle == TextWrappingStyle.Through)
		{
			if (pic.HorizontalAlignment == ShapeHorizontalAlignment.Right)
			{
				m_writer.WriteAttributeString("align", "right");
			}
			else
			{
				m_writer.WriteAttributeString("align", "left");
			}
		}
		if (!string.IsNullOrEmpty(pic.AlternativeText))
		{
			m_writer.WriteAttributeString("alt", pic.AlternativeText);
		}
		m_writer.WriteEndElement();
		if (pic.TextWrappingStyle == TextWrappingStyle.TopAndBottom)
		{
			m_writer.WriteStartElement("br");
			m_writer.WriteAttributeString("clear", "ALL");
			m_writer.WriteEndElement();
		}
		if (pic.TextWrappingStyle == TextWrappingStyle.InFrontOfText || pic.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			m_writer.WriteEndElement();
		}
	}

	private void WriteTextRange(WTextRange tr, string combinedText)
	{
		if (combinedText == ControlChar.Tab && tr.NextSibling is WField && (tr.NextSibling as WField).FieldType == FieldType.FieldPageRef && m_bSkiPageRefFieldItem)
		{
			return;
		}
		if (combinedText == string.Format(CultureInfo.InvariantCulture, '\u0002'.ToString()))
		{
			WriteFtntAttributes(tr.CharacterFormat);
		}
		else if (!string.IsNullOrEmpty(combinedText))
		{
			m_writer.WriteStartElement("span");
			if (Enum.IsDefined(typeof(LocaleIDs), (int)tr.CharacterFormat.LocaleIdASCII))
			{
				m_writer.WriteAttributeString("lang", string.Format(CultureInfo.InvariantCulture, ((LocaleIDs)tr.CharacterFormat.LocaleIdASCII).ToString().Replace('_', '-')));
			}
			if (tr.CharacterFormat.Bidi && !tr.OwnerParagraph.ParagraphFormat.Bidi)
			{
				m_writer.WriteAttributeString("dir", "rtl");
			}
			else if (tr.OwnerParagraph.ParagraphFormat.Bidi && !tr.CharacterFormat.Bidi)
			{
				m_writer.WriteAttributeString("dir", "ltr");
			}
			string style = GetStyle(tr.CharacterFormat, style: false);
			if (0 == 0)
			{
				WriteSpanText(combinedText, style, tr);
				m_writer.WriteEndElement();
			}
		}
	}

	private void WriteSpanText(string text, string style, WTextRange tr)
	{
		if (text.Contains(ControlChar.Space + ControlChar.Space))
		{
			style += "mso-spacerun:yes;";
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == ControlChar.SpaceChar && i != text.Length - 1 && text[i + 1] == ControlChar.SpaceChar)
				{
					stringBuilder.Append(ControlChar.NonBreakingSpaceChar);
				}
				else
				{
					stringBuilder.Append(text[i]);
				}
			}
			text = stringBuilder.ToString();
		}
		if (text.StartsWith(ControlChar.Space) && tr.Owner is WParagraph && tr.OwnerParagraph.Items.FirstItem == tr)
		{
			if (!style.Contains("mso-spacerun:yes;"))
			{
				style += "mso-spacerun:yes;";
			}
			text = ReplaceEmptySpace(text);
		}
		if (tr.CharacterFormat.CharStyleName != null && tr.CharacterFormat.CharStyleName.Length > 0)
		{
			Style style2 = tr.Document.Styles.FindByName(tr.CharacterFormat.CharStyleName) as Style;
			string classAttr = GetClassAttr(style2);
			m_writer.WriteAttributeString("class", classAttr);
			if (!string.IsNullOrEmpty(tr.CharacterFormat.CharStyleName))
			{
				style = ValidateStyle(tr.CharacterFormat.CharStyleName, style);
			}
		}
		if (style.Length > 0)
		{
			m_writer.WriteAttributeString("style", style);
		}
		bool flag = tr.CharacterFormat.CharStyle != null && tr.CharacterFormat.CharStyle.Name == "Footnote Reference";
		if (tr.CharacterFormat.SubSuperScript == SubSuperScript.SubScript)
		{
			m_writer.WriteStartElement("sub");
		}
		else if (tr.CharacterFormat.SubSuperScript == SubSuperScript.SuperScript && !flag)
		{
			m_writer.WriteStartElement("sup");
		}
		WriteText(text);
		if (tr.CharacterFormat.SubSuperScript == SubSuperScript.SubScript || (tr.CharacterFormat.SubSuperScript == SubSuperScript.SuperScript && !flag))
		{
			m_writer.WriteEndElement();
		}
	}

	private void WriteTable(WTable table, bool isTableCreatedFromTextBox)
	{
		ApplyTableGridStyle(table, isTableCreatedFromTextBox);
		table.ApplyBaseStyleFormats();
		table.UpdateGridSpan();
		if (table.Rows.Count == 0)
		{
			return;
		}
		if (listStack.Count > 0)
		{
			WriteEndElement(listStack.Count);
		}
		List<float> list = new List<float>();
		if (table.Document.IsDOCX() && table.IsTableGridVerified)
		{
			WTableColumnCollection tableGrid = table.TableGrid;
			for (int i = 0; i < tableGrid.Count; i++)
			{
				WTableColumn wTableColumn = tableGrid[i];
				float num = ((i > 0) ? tableGrid[i - 1].EndOffset : 0f);
				wTableColumn.PreferredWidth = (wTableColumn.EndOffset - num) / 20f;
				list.Add(wTableColumn.EndOffset / 20f);
			}
			foreach (WTableRow row in table.Rows)
			{
				for (int j = 0; j < row.Cells.Count; j++)
				{
					WTableCell wTableCell = row.Cells[j];
					float cellWidth = tableGrid.GetCellWidth(wTableCell.GridColumnStartIndex, wTableCell.GridSpan);
					wTableCell.CellFormat.CellWidth = cellWidth;
				}
			}
		}
		else
		{
			list = CalculateOffsets(table);
		}
		m_writer.WriteStartElement("div");
		if (table.TableFormat.Bidi)
		{
			m_writer.WriteAttributeString("dir", "rtl");
		}
		if (table.TableFormat.HasValue(121) && table.TableFormat.Hidden)
		{
			m_writer.WriteAttributeString("style", "display:none");
		}
		m_writer.WriteStartElement("table");
		WriteTableAttributes(table, isTableCreatedFromTextBox);
		int k = 0;
		for (int count = table.Rows.Count; k < count; k++)
		{
			WTableRow wTableRow2 = table.Rows[k];
			m_writer.WriteStartElement("tr");
			float num2 = 0f;
			WriteRowAttributes(wTableRow2);
			short gridBefore = wTableRow2.RowFormat.GridBefore;
			if (gridBefore > 0)
			{
				WriteGridCell(gridBefore, wTableRow2.RowFormat.GridBeforeWidth);
			}
			int l = 0;
			for (int count2 = wTableRow2.Cells.Count; l < count2; l++)
			{
				WTableCell wTableCell2 = wTableRow2.Cells[l];
				if (wTableCell2.CellFormat.VerticalMerge == CellMerge.Continue || wTableCell2.CellFormat.HorizontalMerge == CellMerge.Continue)
				{
					num2 += (float)Math.Round(wTableCell2.Width, 2);
					continue;
				}
				if (wTableRow2.IsHeader)
				{
					m_writer.WriteStartElement("th");
				}
				else
				{
					m_writer.WriteStartElement("td");
				}
				float num3 = WriteCellAttributes(wTableCell2);
				string style = GetStyle(wTableCell2.CellFormat);
				style = ((!wTableCell2.CellFormat.SamePaddingsAsTable) ? (style + GetPaddings(wTableCell2.CellFormat.Paddings)) : (style + GetPaddings(wTableCell2)));
				if (num3 > 0f)
				{
					style = style + "width:" + num3.ToString(CultureInfo.InvariantCulture) + "px;";
				}
				if (style.Length > 0)
				{
					m_writer.WriteAttributeString("style", style);
				}
				WriteSpanAttributes(list, num2, wTableCell2);
				if (wTableCell2.Items.Count == 0)
				{
					m_writer.WriteStartElement("p");
					WriteEmptyPara(wTableCell2.CharacterFormat);
					m_writer.WriteEndElement();
				}
				WriteTextBody(wTableCell2);
				if (listStack.Count > 0)
				{
					WriteEndElement(listStack.Count);
				}
				m_writer.WriteEndElement();
				num2 = (float)Math.Round(num2 + wTableCell2.Width, 2);
			}
			gridBefore = wTableRow2.RowFormat.GridAfter;
			if (gridBefore > 0)
			{
				WriteGridCell(gridBefore, wTableRow2.RowFormat.GridAfterWidth);
			}
			m_writer.WriteEndElement();
		}
		if (table.Document.IsDOCX() && CheckTableContainsMisalignedColumns(table, list))
		{
			WriteOffsetsRow(list);
		}
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private bool CheckTableContainsMisalignedColumns(WTable table, List<float> colOffsets)
	{
		for (int i = 0; i < table.Rows.Count; i++)
		{
			WTableRow wTableRow = table.Rows[i];
			if (colOffsets.Count != wTableRow.Cells.Count)
			{
				return true;
			}
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				if (j == 0)
				{
					if (wTableCell.Width != (float)Math.Round(colOffsets[j], 2))
					{
						return true;
					}
				}
				else if (wTableCell.Width != (float)Math.Round(colOffsets[j] - colOffsets[j - 1], 2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void WriteGridCell(int gridCount, PreferredWidthInfo gridWidth)
	{
		m_writer.WriteStartElement("td");
		m_writer.WriteAttributeString("style", "border:none;");
		if (gridWidth.WidthType == FtsWidth.Percentage)
		{
			m_writer.WriteAttributeString("width", gridWidth.Width.ToString(CultureInfo.InvariantCulture) + "%");
		}
		else if (gridWidth.WidthType == FtsWidth.Point)
		{
			m_writer.WriteAttributeString("width", gridWidth.Width.ToString(CultureInfo.InvariantCulture) + "pt");
		}
		if (gridCount > 1)
		{
			m_writer.WriteAttributeString("colspan", gridCount.ToString(CultureInfo.InvariantCulture));
		}
		m_writer.WriteRaw("&nbsp;");
		m_writer.WriteEndElement();
	}

	private void ApplyTableGridStyle(WTable table, bool isTableCreatedFromTextBox)
	{
		if (isTableCreatedFromTextBox || (table.StyleName != null && !(table.StyleName == string.Empty)))
		{
			return;
		}
		switch (table.Document.ActualFormatType)
		{
		case FormatType.Doc:
		case FormatType.Docx:
		case FormatType.Word2007:
		case FormatType.Word2010:
			if (!table.DocxTableFormat.HasFormat)
			{
				table.ApplyStyle(BuiltinTableStyle.TableGrid, isClearCellShading: false);
			}
			break;
		case FormatType.Dot:
		case FormatType.StrictDocx:
			break;
		}
	}

	private Border GetBottomBorderOfVerticallyMergedCell(WTableCell cell)
	{
		Border bottom = cell.CellFormat.Borders.Bottom;
		WTableRow ownerRow = cell.OwnerRow;
		WTable ownerTable = ownerRow.OwnerTable;
		int num = ownerTable.Rows.IndexOf((IEntity)ownerRow);
		int num2 = ownerRow.Cells.IndexOf((IEntity)cell);
		for (int i = num; i < ownerTable.Rows.Count; i++)
		{
			if (num2 < ownerTable.Rows[i].Cells.Count && ownerTable.Rows[i].Cells[num2].CellFormat.VerticalMerge == CellMerge.Continue)
			{
				bottom = ownerTable.Rows[i].Cells[num2].CellFormat.Borders.Bottom;
			}
		}
		return bottom;
	}

	private Border GetRightBorderOfHorizontallyMergedCell(WTableCell cell)
	{
		Border right = cell.CellFormat.Borders.Right;
		WTableRow ownerRow = cell.OwnerRow;
		for (int i = ownerRow.Cells.IndexOf((IEntity)cell); i < ownerRow.Cells.Count; i++)
		{
			if (ownerRow.Cells[i].CellFormat.HorizontalMerge == CellMerge.Continue)
			{
				right = ownerRow.Cells[i].CellFormat.Borders.Right;
			}
		}
		return right;
	}

	private void WriteOffsetsRow(List<float> offsets)
	{
		if (offsets.Count != 0)
		{
			m_writer.WriteStartElement("tr");
			m_writer.WriteAttributeString("style", string.Format("{0}:{1}px;", "height", "0"));
			int i = 0;
			for (int count = offsets.Count; i < count; i++)
			{
				m_writer.WriteStartElement("td");
				float value = ((i != 0) ? (offsets[i] - offsets[i - 1]) : offsets[i]);
				string value2 = string.Format("{0}:{1}px;", "width", UnitsConvertor.Instance.ConvertToPixels(value, PrintUnits.Point)) + "border:none;padding:0pt;";
				m_writer.WriteAttributeString("style", value2);
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
		}
	}

	private void WriteSpanAttributes(List<float> colOffsets, float rowOffset, WTableCell cell)
	{
		int colspan;
		if (cell.CellFormat.HorizontalMerge == CellMerge.Start)
		{
			colspan = GetColspan(cell, colOffsets, rowOffset + cell.Width);
			rowOffset = (float)Math.Round(rowOffset, 2);
			colspan += GetColspan(colOffsets, rowOffset, cell.Width) - 1;
		}
		else
		{
			rowOffset = (float)Math.Round(rowOffset, 2);
			colspan = GetColspan(colOffsets, rowOffset, cell.Width);
		}
		if (colspan > 1)
		{
			m_writer.WriteAttributeString("colspan", colspan.ToString(CultureInfo.InvariantCulture));
		}
		if (cell.CellFormat.VerticalMerge == CellMerge.Start)
		{
			int rowspan = GetRowspan(cell, rowOffset);
			m_writer.WriteAttributeString("rowspan", rowspan.ToString(CultureInfo.InvariantCulture));
		}
	}

	private int GetRowspan(WTableCell cell, float rowOffset)
	{
		int num = 1;
		WTableRow ownerRow = cell.OwnerRow;
		WTable ownerTable = ownerRow.OwnerTable;
		int rowIndex = ownerRow.GetRowIndex();
		if (ownerRow.RowFormat.GridBefore > 0)
		{
			if (ownerRow.RowFormat.GridBeforeWidth.WidthType == FtsWidth.Point)
			{
				rowOffset += ownerRow.RowFormat.GridBeforeWidth.Width;
			}
			else if (ownerRow.RowFormat.GridBeforeWidth.WidthType == FtsWidth.Percentage)
			{
				float num2 = (cell.OwnerRow.OwnerTable.OwnerTextBody.Owner as WSection).PageSetup.ClientWidth * (ownerRow.RowFormat.GridBeforeWidth.Width / 100f);
				rowOffset += num2;
			}
		}
		int i = rowIndex + 1;
		for (int count = ownerTable.Rows.Count; i < count; i++)
		{
			WTableCell cellByOffset = GetCellByOffset(ownerTable.Rows[i], rowOffset);
			if (cellByOffset == null || cell.Width != cellByOffset.Width || cellByOffset.CellFormat.VerticalMerge != CellMerge.Continue)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private WTableCell GetCellByOffset(WTableRow row, float rowOffset)
	{
		float num = 0f;
		if (row.RowFormat.GridBefore > 0)
		{
			if (row.RowFormat.GridBeforeWidth.WidthType == FtsWidth.Point)
			{
				num = row.RowFormat.GridBeforeWidth.Width;
			}
			else if (row.RowFormat.GridBeforeWidth.WidthType == FtsWidth.Percentage)
			{
				num = (row.OwnerTable.OwnerTextBody.Owner as WSection).PageSetup.ClientWidth * (row.RowFormat.GridBeforeWidth.Width / 100f);
			}
		}
		int i = 0;
		for (int count = row.Cells.Count; i < count; i++)
		{
			if ((float)Math.Round(num, 2) == rowOffset)
			{
				return row.Cells[i];
			}
			num += (float)Math.Round(row.Cells[i].Width, 2);
		}
		return null;
	}

	private List<float> CalculateOffsets(WTable table)
	{
		List<float> list = new List<float>();
		int i = 0;
		for (int count = table.Rows.Count; i < count; i++)
		{
			WTableRow wTableRow = table.Rows[i];
			float num = 0f;
			int j = 0;
			for (int count2 = wTableRow.Cells.Count; j < count2; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				num = (float)Math.Round(num + wTableCell.Width, 2);
				if (!list.Contains(num))
				{
					list.Add(num);
				}
			}
		}
		list.Sort();
		return list;
	}

	private int GetColspan(List<float> colOffsets, float startOffset, float colWidth)
	{
		int num = colOffsets.IndexOf(startOffset);
		if (num < 0 && startOffset > 0f)
		{
			return 1;
		}
		int i = 1;
		float num2 = startOffset + colWidth;
		if (colOffsets.Count > num + i)
		{
			for (; num2 - colOffsets[num + i] > 0.01f; i++)
			{
			}
		}
		return i;
	}

	private int GetColspan(WTableCell cell, List<float> colOffsets, float startOffset)
	{
		int num = 1;
		int indexInOwnerCollection = cell.GetIndexInOwnerCollection();
		WTableRow ownerRow = cell.OwnerRow;
		int i = indexInOwnerCollection + 1;
		for (int count = ownerRow.Cells.Count; i < count && ownerRow.Cells[i].CellFormat.HorizontalMerge == CellMerge.Continue; i++)
		{
			startOffset = (float)Math.Round(startOffset, 2);
			num += GetColspan(colOffsets, startOffset, ownerRow.Cells[i].Width);
			startOffset += ownerRow.Cells[i].Width;
		}
		return num;
	}

	private float WriteCellAttributes(WTableCell cell)
	{
		WTableRow ownerRow = cell.OwnerRow;
		int num = ownerRow.Cells.IndexOf(cell);
		float num2 = cell.Width;
		for (int i = num + 1; i < ownerRow.Cells.Count && ownerRow.Cells[i].CellFormat.HorizontalMerge == CellMerge.Continue; i++)
		{
			num2 += ownerRow.Cells[i].Width;
		}
		if (num2 > 0f)
		{
			num2 = UnitsConvertor.Instance.ConvertToPixels(num2, PrintUnits.Point);
		}
		return num2;
	}

	private void WriteTableAttributes(WTable table, bool isTableCreatedFromTextBox)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (table.TableFormat.CellSpacing >= 0f)
		{
			string bordersStyle = GetBordersStyle(table.TableFormat.Borders, stringBuilder);
			if (IsBorderAttributeNeedToPreserve(table, bordersStyle))
			{
				m_writer.WriteAttributeString("border", "1");
			}
		}
		if (!string.IsNullOrEmpty(table.Title))
		{
			m_writer.WriteAttributeString("title", table.Title);
		}
		if (!string.IsNullOrEmpty(table.Description))
		{
			m_writer.WriteAttributeString("summary", table.Description);
		}
		if (table.TableFormat.HasValue(108) && table.TableFormat.BackColor != Color.Empty)
		{
			stringBuilder.Append("background-color:" + GetColor(table.TableFormat.BackColor) + ";");
		}
		if (isTableCreatedFromTextBox && table.TableFormat.ForeColor != Color.Empty)
		{
			stringBuilder.Append("color:" + GetColor(table.TableFormat.ForeColor) + ";");
		}
		if (table.IndentFromLeft != 0f && table.TableFormat.HorizontalAlignment == RowAlignment.Left)
		{
			stringBuilder.Append("margin-left:" + table.IndentFromLeft.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		stringBuilder.Append(WriteTableWidth(table));
		WriteTableAlignment(table, isTableCreatedFromTextBox);
		stringBuilder.Append(WriteTableCellSpacing(table));
		if (table.TableFormat.CellSpacing >= 0f)
		{
			WriteTableBorder(table, stringBuilder);
		}
		if (stringBuilder.ToString() != string.Empty)
		{
			m_writer.WriteAttributeString("style", stringBuilder.ToString());
		}
	}

	private bool IsBorderAttributeNeedToPreserve(WTable table, string borderStyle)
	{
		if (borderStyle == string.Empty && !table.TableFormat.Borders.Bottom.HasNoneStyle && !table.TableFormat.Borders.Right.HasNoneStyle && !table.TableFormat.Borders.Left.HasNoneStyle && !table.TableFormat.Borders.Top.HasNoneStyle && table.TableFormat.Borders.Top.BorderType != BorderStyle.Cleared && table.TableFormat.Borders.Left.BorderType != BorderStyle.Cleared && table.TableFormat.Borders.Bottom.BorderType != BorderStyle.Cleared && table.TableFormat.Borders.Right.BorderType != BorderStyle.Cleared)
		{
			return true;
		}
		return false;
	}

	private void WriteTableBorder(WTable table, StringBuilder sb)
	{
		GetTableborder(table.TableFormat.Borders.Bottom, "bottom", sb);
		GetTableborder(table.TableFormat.Borders.Top, "bottom", sb);
		GetTableborder(table.TableFormat.Borders.Left, "bottom", sb);
		GetTableborder(table.TableFormat.Borders.Right, "bottom", sb);
	}

	private void GetTableborder(Border border, string suffix, StringBuilder sb)
	{
		if (border.BorderType != BorderStyle.Cleared)
		{
			if (border.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-style:" + ToBorderStyle(border.BorderType) + ";");
			}
			if (border.LineWidth > 0f)
			{
				sb.Append("border-" + suffix + "-width:" + border.LineWidth.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			if (border.Color != Color.Empty)
			{
				sb.Append("border-" + suffix + "-color" + GetColor(border.Color) + ";");
			}
			else if (border.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-color:#000000;");
			}
		}
	}

	private string WriteTableCellSpacing(WTable table)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (table.TableFormat.CellSpacing > 0f)
		{
			m_writer.WriteAttributeString("cellspacing", UnitsConvertor.Instance.ConvertToPixels(table.TableFormat.CellSpacing * 2f, PrintUnits.Point).ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			m_writer.WriteAttributeString("cellspacing", "0");
		}
		if (table.TableFormat.CellSpacing <= 0f)
		{
			stringBuilder.Append("border-collapse: collapse; ");
		}
		return stringBuilder.ToString();
	}

	private string WriteTableWidth(WTable table)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (table.PreferredTableWidth.WidthType)
		{
		case FtsWidth.Percentage:
			stringBuilder.Append("width: " + table.PreferredTableWidth.Width.ToString(CultureInfo.InvariantCulture) + "%; ");
			break;
		case FtsWidth.Point:
			stringBuilder.Append("width: " + table.PreferredTableWidth.Width.ToString(CultureInfo.InvariantCulture) + "pt; ");
			break;
		case FtsWidth.Auto:
			stringBuilder.Append("width: auto; ");
			break;
		}
		return stringBuilder.ToString();
	}

	private void WriteTableAlignment(WTable table, bool isTableCreatedFromTextBox)
	{
		switch (isTableCreatedFromTextBox ? table.TableFormat.HorizontalAlignment : GetTableAlignment(table.TableFormat))
		{
		case RowAlignment.Center:
			m_writer.WriteAttributeString("align", "center");
			break;
		case RowAlignment.Right:
			m_writer.WriteAttributeString("align", "right");
			break;
		}
	}

	private RowAlignment GetTableAlignment(RowFormat tableFormat)
	{
		if (tableFormat.PropertiesHash.ContainsKey(62))
		{
			if (tableFormat.Positioning.HorizPosition == -8f)
			{
				return RowAlignment.Right;
			}
			return RowAlignment.Left;
		}
		if (tableFormat.PropertiesHash.ContainsKey(105))
		{
			RowAlignment rowAlignment = (RowAlignment)tableFormat.PropertiesHash[105];
			if (tableFormat.Bidi && (tableFormat.Document.ActualFormatType == FormatType.Doc || tableFormat.Document.ActualFormatType == FormatType.Docx))
			{
				switch (rowAlignment)
				{
				case RowAlignment.Right:
					rowAlignment = RowAlignment.Left;
					break;
				case RowAlignment.Left:
					rowAlignment = RowAlignment.Right;
					break;
				}
			}
			return rowAlignment;
		}
		return RowAlignment.Left;
	}

	private void WriteRowAttributes(WTableRow row)
	{
		string text = "height: ";
		row.Height = Math.Abs(row.Height);
		text = ((!(row.Height > 0f)) ? (text + "2px") : (text + UnitsConvertor.Instance.ConvertToPixels(row.Height, PrintUnits.Point).ToString(CultureInfo.InvariantCulture) + "px"));
		if (row.RowFormat.HasValue(121) && row.RowFormat.Hidden && IsAllCellsHidden(row))
		{
			text += ";display:none";
		}
		m_writer.WriteAttributeString("style", text);
	}

	private bool IsAllCellsHidden(WTableRow row)
	{
		foreach (WTableCell cell in row.Cells)
		{
			foreach (Entity childEntity in cell.ChildEntities)
			{
				switch (childEntity.EntityType)
				{
				case EntityType.Paragraph:
				{
					WParagraph wParagraph = childEntity as WParagraph;
					if (wParagraph.BreakCharacterFormat.Hidden)
					{
						foreach (ParagraphItem childEntity2 in wParagraph.ChildEntities)
						{
							if (!childEntity2.ParaItemCharFormat.Hidden)
							{
								return false;
							}
						}
						break;
					}
					return false;
				}
				case EntityType.Table:
					foreach (WTableRow row2 in (childEntity as WTable).Rows)
					{
						if (!row2.RowFormat.HasValue(121) || !row2.RowFormat.Hidden || !IsAllCellsHidden(row2))
						{
							return false;
						}
					}
					break;
				}
			}
		}
		return true;
	}

	private string GetPaddings(Paddings paddings)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("padding-left:" + paddings.Left.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-right:" + paddings.Right.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-top:" + paddings.Top.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-bottom:" + paddings.Bottom.ToString(CultureInfo.InvariantCulture) + "pt;");
		return stringBuilder.ToString();
	}

	private string GetPaddings(WTableCell cell)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Paddings cellPaddingBasedOnTable = GetCellPaddingBasedOnTable(cell);
		stringBuilder.Append("padding-left:" + cellPaddingBasedOnTable.Left.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-right:" + cellPaddingBasedOnTable.Right.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-top:" + cellPaddingBasedOnTable.Top.ToString(CultureInfo.InvariantCulture) + "pt;");
		stringBuilder.Append("padding-bottom:" + cellPaddingBasedOnTable.Bottom.ToString(CultureInfo.InvariantCulture) + "pt;");
		return stringBuilder.ToString();
	}

	private Paddings GetCellPaddingBasedOnTable(WTableCell cell)
	{
		Paddings paddings = new Paddings();
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(1))
		{
			paddings.Left = cell.OwnerRow.RowFormat.Paddings.Left;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(1))
		{
			paddings.Left = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Left;
		}
		else if (cell.Document.ActualFormatType == FormatType.Doc)
		{
			paddings.Left = 0f;
		}
		else
		{
			paddings.Left = 5.4f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(4))
		{
			paddings.Right = cell.OwnerRow.RowFormat.Paddings.Right;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(4))
		{
			paddings.Right = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Right;
		}
		else if (cell.Document.ActualFormatType == FormatType.Doc)
		{
			paddings.Right = 0f;
		}
		else
		{
			paddings.Right = 5.4f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(2))
		{
			paddings.Top = cell.OwnerRow.RowFormat.Paddings.Top;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(2))
		{
			paddings.Top = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Top;
		}
		else
		{
			paddings.Top = 0f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(3))
		{
			paddings.Bottom = cell.OwnerRow.RowFormat.Paddings.Bottom;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(3))
		{
			paddings.Bottom = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Bottom;
		}
		else
		{
			paddings.Bottom = 0f;
		}
		return paddings;
	}

	private string GetStyle(CellFormat format)
	{
		WTableCell ownerCell = format.OwnerBase as WTableCell;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("vertical-align:" + format.VerticalAlignment.ToString().ToLowerInvariant() + ";");
		if (format.ForeColor != Color.Empty || format.TextureStyle != 0 || format.BackColor != Color.Empty)
		{
			stringBuilder.Append("background-color:" + GetCellBackground(format) + ";");
		}
		GetBordersStyle(format.Borders, format.OwnerRowFormat.Borders, stringBuilder, ownerCell);
		return stringBuilder.ToString();
	}

	private string GetCellBackground(CellFormat format)
	{
		float percent = Build_TextureStyle(format.TextureStyle);
		int colorValue = GetColorValue(format.ForeColor.R, format.BackColor.R, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		int colorValue2 = GetColorValue(format.ForeColor.G, format.BackColor.G, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		int colorValue3 = GetColorValue(format.ForeColor.B, format.BackColor.B, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		Color color = Color.FromArgb(colorValue, colorValue2, colorValue3);
		return GetColor(color);
	}

	private string GetParagraphBackground(WParagraphFormat format)
	{
		float percent = Build_TextureStyle(format.TextureStyle);
		int colorValue = GetColorValue(format.ForeColor.R, format.BackColor.R, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		int colorValue2 = GetColorValue(format.ForeColor.G, format.BackColor.G, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		int colorValue3 = GetColorValue(format.ForeColor.B, format.BackColor.B, percent, format.ForeColor.IsEmpty, format.BackColor.IsEmpty);
		Color color = Color.FromArgb(colorValue, colorValue2, colorValue3);
		return GetColor(color);
	}

	private int GetColorValue(int foreColorValue, int backColorValue, float percent, bool isForeColorEmpty, bool isBackColorEmpty)
	{
		if (percent == 100f)
		{
			return foreColorValue;
		}
		if (isForeColorEmpty)
		{
			if (isBackColorEmpty)
			{
				return (int)Math.Round(255f * (1f - percent / 100f));
			}
			return (int)Math.Round((float)backColorValue * (1f - percent / 100f));
		}
		if (isBackColorEmpty)
		{
			return (int)Math.Round((float)foreColorValue * (percent / 100f));
		}
		return backColorValue + (int)Math.Round((float)foreColorValue * (percent / 100f)) - (int)Math.Round((float)backColorValue * (percent / 100f));
	}

	private void GetBordersStyle(Borders cellBorders, Borders rowBorders, StringBuilder sb, WTableCell ownerCell)
	{
		Border rowBorder = GetRowBorder(rowBorders, ownerCell, "top");
		GetBorderStyle(cellBorders.Top, rowBorder, sb, "top", ownerCell);
		rowBorder = GetRowBorder(rowBorders, ownerCell, "left");
		GetBorderStyle(cellBorders.Left, rowBorder, sb, "left", ownerCell);
		rowBorder = GetRowBorder(rowBorders, ownerCell, "right");
		if (ownerCell.CellFormat.HorizontalMerge == CellMerge.Start)
		{
			GetBorderStyle(GetRightBorderOfHorizontallyMergedCell(ownerCell), rowBorder, sb, "right", ownerCell);
		}
		else
		{
			GetBorderStyle(cellBorders.Right, rowBorder, sb, "right", ownerCell);
		}
		rowBorder = GetRowBorder(rowBorders, ownerCell, "bottom");
		if (ownerCell.CellFormat.VerticalMerge == CellMerge.Start)
		{
			GetBorderStyle(GetBottomBorderOfVerticallyMergedCell(ownerCell), rowBorder, sb, "bottom", ownerCell);
		}
		else
		{
			GetBorderStyle(cellBorders.Bottom, rowBorder, sb, "bottom", ownerCell);
		}
	}

	private Border GetRowBorder(Borders borders, WTableCell cell, string side)
	{
		switch (side)
		{
		case "top":
		{
			WTableRow ownerRow = cell.OwnerRow;
			if (ownerRow != null && ownerRow.GetIndexInOwnerCollection() > 0)
			{
				return borders.Horizontal;
			}
			return borders.Top;
		}
		case "left":
			if (cell.GetIndexInOwnerCollection() > 0)
			{
				return borders.Vertical;
			}
			return borders.Left;
		case "right":
		{
			WTableRow ownerRow = cell.OwnerRow;
			if (ownerRow != null && cell.GetIndexInOwnerCollection() == ownerRow.Cells.Count - 1)
			{
				return borders.Right;
			}
			return borders.Vertical;
		}
		case "bottom":
		{
			WTableRow ownerRow = cell.OwnerRow;
			WTable ownerTable = ownerRow.OwnerTable;
			if (ownerTable != null && ownerRow.GetIndexInOwnerCollection() == ownerTable.Rows.Count - 1)
			{
				return borders.Bottom;
			}
			return borders.Horizontal;
		}
		default:
			return null;
		}
	}

	private void GetBorderStyle(Border cellBorder, Border rowBorder, StringBuilder sb, string suffix, WTableCell cell)
	{
		if (cellBorder.BorderType == BorderStyle.Cleared)
		{
			sb.Append("border-" + suffix + ":none;");
			return;
		}
		BorderStyle borderStyle = ((cellBorder.BorderType != 0 && cellBorder.BorderType != BorderStyle.Cleared) ? cellBorder.BorderType : rowBorder.BorderType);
		if (borderStyle != 0 && borderStyle != BorderStyle.Cleared)
		{
			sb.Append("border-" + suffix + "-style:" + ToBorderStyle(borderStyle) + ";");
		}
		else if (!cellBorder.HasNoneStyle && !rowBorder.HasNoneStyle)
		{
			GetCellborderStyleBasedOnTableBorder(cell, suffix, sb);
		}
		Color color = ((cellBorder.Color != Color.Empty) ? cellBorder.Color : rowBorder.Color);
		if (color != Color.Empty)
		{
			sb.Append("border-" + suffix + "-color:" + GetColor(color) + ";");
		}
		else if (!cellBorder.HasNoneStyle && !rowBorder.HasNoneStyle)
		{
			GetCellBorderColorBasedOnTableBorder(cell, suffix, sb);
		}
		float num = 0f;
		if (cellBorder.LineWidth > 0f)
		{
			num = GetLineWidthBasedOnBorderStyle(cellBorder);
		}
		else if (rowBorder.LineWidth > 0f)
		{
			num = GetLineWidthBasedOnBorderStyle(rowBorder);
		}
		if (num > 0f)
		{
			sb.Append("border-" + suffix + "-width:" + num.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		else if (!cellBorder.HasNoneStyle && !rowBorder.HasNoneStyle)
		{
			GetCellborderWidthBasedOnTableBorder(cell, suffix, sb);
		}
	}

	private void GetCellborderStyleBasedOnTableBorder(WTableCell cell, string suffix, StringBuilder sb)
	{
		WTableRow ownerRow = cell.OwnerRow;
		WTable ownerTable = ownerRow.OwnerTable;
		int indexInOwnerCollection = cell.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = ownerRow.GetIndexInOwnerCollection();
		switch (suffix)
		{
		case "top":
			if (indexInOwnerCollection2 == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Top.BorderType != 0)
				{
					sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Top.BorderType) + ";");
				}
			}
			else if (ownerTable.TableFormat.Borders.Horizontal.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Horizontal.BorderType) + ";");
			}
			break;
		case "bottom":
			if (indexInOwnerCollection2 == ownerTable.Rows.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Bottom.BorderType != 0)
				{
					sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Bottom.BorderType) + ";");
				}
			}
			else if (ownerTable.TableFormat.Borders.Horizontal.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Horizontal.BorderType) + ";");
			}
			break;
		case "left":
			if (indexInOwnerCollection == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Left.BorderType != 0)
				{
					sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Left.BorderType) + ";");
				}
			}
			else if (ownerTable.TableFormat.Borders.Vertical.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Vertical.BorderType) + ";");
			}
			break;
		case "right":
			if (indexInOwnerCollection == ownerRow.Cells.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Right.BorderType != 0)
				{
					sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Right.BorderType) + ";");
				}
			}
			else if (ownerTable.TableFormat.Borders.Vertical.BorderType != 0)
			{
				sb.Append("border-" + suffix + "-style:" + ToBorderStyle(ownerTable.TableFormat.Borders.Vertical.BorderType) + ";");
			}
			break;
		}
	}

	private void GetCellBorderColorBasedOnTableBorder(WTableCell cell, string suffix, StringBuilder sb)
	{
		WTableRow ownerRow = cell.OwnerRow;
		WTable ownerTable = ownerRow.OwnerTable;
		int indexInOwnerCollection = cell.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = ownerRow.GetIndexInOwnerCollection();
		switch (suffix)
		{
		case "top":
			if (indexInOwnerCollection2 == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Top, sb, suffix);
			}
			else
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Horizontal, sb, suffix);
			}
			break;
		case "bottom":
			if (indexInOwnerCollection2 == ownerTable.Rows.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Bottom, sb, suffix);
			}
			else
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Horizontal, sb, suffix);
			}
			break;
		case "left":
			if (indexInOwnerCollection == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Left, sb, suffix);
			}
			else
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Vertical, sb, suffix);
			}
			break;
		case "right":
			if (indexInOwnerCollection == ownerRow.Cells.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Right, sb, suffix);
			}
			else
			{
				GetBorderColor(ownerTable.TableFormat.Borders.Vertical, sb, suffix);
			}
			break;
		}
	}

	private void GetBorderColor(Border border, StringBuilder sb, string suffix)
	{
		if (border.Color != Color.Empty)
		{
			sb.Append("border-" + suffix + "-color:" + GetColor(border.Color) + ";");
		}
		else if (border.BorderType != 0)
		{
			sb.Append("border-" + suffix + "-color:#000000;");
		}
	}

	private void GetCellborderWidthBasedOnTableBorder(WTableCell cell, string suffix, StringBuilder sb)
	{
		WTableRow ownerRow = cell.OwnerRow;
		WTable ownerTable = ownerRow.OwnerTable;
		int indexInOwnerCollection = cell.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = ownerRow.GetIndexInOwnerCollection();
		switch (suffix)
		{
		case "top":
			if (indexInOwnerCollection2 == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Top.LineWidth > 0f)
				{
					sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Top).ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else if (ownerTable.TableFormat.Borders.Horizontal.LineWidth > 0f)
			{
				sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Horizontal).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			break;
		case "bottom":
			if (indexInOwnerCollection2 == ownerTable.Rows.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Bottom.LineWidth > 0f)
				{
					sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Bottom).ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else if (ownerTable.TableFormat.Borders.Horizontal.LineWidth > 0f)
			{
				sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Horizontal).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			break;
		case "left":
			if (indexInOwnerCollection == 0 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Left.LineWidth > 0f)
				{
					sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Left).ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else if (ownerTable.TableFormat.Borders.Vertical.LineWidth > 0f)
			{
				sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Vertical).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			break;
		case "right":
			if (indexInOwnerCollection == ownerRow.Cells.Count - 1 && ownerTable.TableFormat.CellSpacing <= 0f)
			{
				if (ownerTable.TableFormat.Borders.Right.LineWidth > 0f)
				{
					sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Right).ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else if (ownerTable.TableFormat.Borders.Vertical.LineWidth > 0f)
			{
				sb.Append("border-" + suffix + "-width:" + GetLineWidthBasedOnBorderStyle(ownerTable.TableFormat.Borders.Vertical).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			break;
		}
	}

	private float GetLineWidthBasedOnBorderStyle(Border border)
	{
		switch (border.BorderType)
		{
		case BorderStyle.Triple:
			return border.LineWidth * 5f;
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
			return (float)((double)border.LineWidth + 1.5);
		case BorderStyle.ThinThickThinSmallGap:
			return border.LineWidth + 3f;
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
			return border.LineWidth * 2f;
		case BorderStyle.ThickThickThinMediumGap:
			return border.LineWidth * 3f;
		case BorderStyle.ThinThickLargeGap:
			return (float)((double)border.LineWidth + 2.25);
		case BorderStyle.ThickThinLargeGap:
		case BorderStyle.ThinThickThinLargeGap:
			return border.LineWidth * 2f + 3f;
		case BorderStyle.Double:
		case BorderStyle.DoubleWave:
			if (border.LineWidth <= 0.5f)
			{
				return 1.5f;
			}
			return border.LineWidth * 3f;
		default:
			if (border.LineWidth < 1f)
			{
				return 1f;
			}
			return border.LineWidth;
		}
	}

	private string GetStyle(WParagraphFormat format, bool isListLevel, bool isListAsPara, WListFormat listFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = "left";
		switch ((byte)format.HorizontalAlignment)
		{
		case 1:
			text = "center";
			break;
		case 2:
			text = "right";
			break;
		case 6:
			if (format.Bidi)
			{
				text = "right";
			}
			break;
		case 3:
		case 4:
		case 5:
		case 7:
		case 8:
		case 9:
			text = "justify";
			break;
		}
		stringBuilder.Append("text-align:" + text + ";");
		if (format.Keep)
		{
			stringBuilder.Append("page-break-inside:avoid;");
		}
		else
		{
			stringBuilder.Append("page-break-inside:auto;");
		}
		if (format.KeepFollow)
		{
			stringBuilder.Append("page-break-after:avoid;");
		}
		else
		{
			stringBuilder.Append("page-break-after:auto;");
		}
		if (format.PageBreakBefore)
		{
			stringBuilder.Append("page-break-before:always;");
		}
		else
		{
			stringBuilder.Append("page-break-before:avoid;");
		}
		float lineSpacing = format.LineSpacing;
		LineSpacingRule lineSpacingRule = format.LineSpacingRule;
		if (Math.Abs(lineSpacing) > 0f && CheckParentFormat(format))
		{
			if (Math.Abs(lineSpacing) == 12f && lineSpacingRule == LineSpacingRule.Multiple)
			{
				stringBuilder.Append("line-height:normal;");
			}
			else if (lineSpacingRule == LineSpacingRule.Multiple)
			{
				stringBuilder.Append("line-height:" + (Math.Abs(lineSpacing) / 12f * 100f).ToString(CultureInfo.InvariantCulture) + "%;");
			}
			else if (lineSpacingRule != 0 || Math.Abs(lineSpacing) > 10f)
			{
				stringBuilder.Append("line-height:" + Math.Abs(lineSpacing).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
		}
		if (!format.ContextualSpacing || !ContextualSpacingChecking() || !m_document.SaveOptions.UseContextualSpacing)
		{
			if (format.SpaceBeforeAuto)
			{
				stringBuilder.Append("-sf-before-space-auto:yes;");
				if (!m_document.Settings.CompatibilityOptions[CompatibilityOption.DontUseHTMLParagraphAutoSpacing])
				{
					stringBuilder.Append("margin-top:" + XmlConvert.ToString(14) + "pt;");
				}
				else
				{
					stringBuilder.Append("margin-top:" + format.BeforeSpacing.ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else
			{
				stringBuilder.Append("margin-top:" + format.BeforeSpacing.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			if (format.SpaceAfterAuto)
			{
				stringBuilder.Append("-sf-after-space-auto:yes;");
				if (!m_document.Settings.CompatibilityOptions[CompatibilityOption.DontUseHTMLParagraphAutoSpacing])
				{
					stringBuilder.Append("margin-bottom:" + XmlConvert.ToString(14) + "pt;");
				}
				else
				{
					stringBuilder.Append("margin-bottom:" + format.AfterSpacing.ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else
			{
				stringBuilder.Append("margin-bottom:" + format.AfterSpacing.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
		}
		else
		{
			stringBuilder.Append("margin-top:0pt;");
			stringBuilder.Append("margin-bottom:0pt;");
		}
		if (!format.WordWrap)
		{
			stringBuilder.Append("word-break:break-all;");
		}
		WListLevel wListLevel = null;
		WParagraph wParagraph = format.OwnerBase as WParagraph;
		if (wParagraph != null && listFormat != null)
		{
			wListLevel = wParagraph.GetListLevel(listFormat);
		}
		if (isListLevel && !isListAsPara)
		{
			float num = 0f;
			float num2 = 0f;
			if (wParagraph != null)
			{
				WParagraphStyle paraStyle = wParagraph.ParaStyle as WParagraphStyle;
				float[] leftRightMargindAndFirstLineIndent = wParagraph.GetLeftRightMargindAndFirstLineIndent(listFormat, wListLevel, paraStyle);
				num = leftRightMargindAndFirstLineIndent[0];
				if (format.Bidi && (format.Document.ActualFormatType == FormatType.Doc || format.Document.ActualFormatType == FormatType.Docx))
				{
					num = leftRightMargindAndFirstLineIndent[1];
				}
				num2 = leftRightMargindAndFirstLineIndent[2];
			}
			m_currListCharFormat = new WCharacterFormat(wParagraph.Document);
			m_currListCharFormat.ImportContainer(wParagraph.BreakCharacterFormat);
			m_currListCharFormat.CopyProperties(wParagraph.BreakCharacterFormat);
			m_currListCharFormat.ApplyBase(wParagraph.BreakCharacterFormat.BaseFormat);
			if (m_currListCharFormat.PropertiesHash.ContainsKey(7))
			{
				m_currListCharFormat.UnderlineStyle = UnderlineStyle.None;
				m_currListCharFormat.PropertiesHash.Remove(7);
			}
			if (wListLevel != null)
			{
				CopyCharacterFormatting(wListLevel.CharacterFormat, m_currListCharFormat);
			}
			if (num2 >= 0f || m_bIsPrefixedList || listFormat.CurrentListLevel.FollowCharacter == FollowCharacterType.Space || format.HorizontalAlignment == HorizontalAlignment.Center || format.HorizontalAlignment == HorizontalAlignment.Right)
			{
				if (!m_bIsPrefixedList)
				{
					stringBuilder.Append("list-style-position:inside;");
				}
				stringBuilder.Append("margin-left:" + num.ToString(CultureInfo.InvariantCulture) + "pt;");
				stringBuilder.Append("text-indent:" + num2.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			else
			{
				stringBuilder.Append("margin-left:" + num.ToString(CultureInfo.InvariantCulture) + "pt;");
				stringBuilder.Append("padding-left:" + num2.ToString(CultureInfo.InvariantCulture) + "pt;");
				stringBuilder.Append("text-indent:0pt;");
			}
		}
		else if (isListLevel)
		{
			float num3 = format.LeftIndent;
			if (format.Bidi && (format.Document.ActualFormatType == FormatType.Doc || format.Document.ActualFormatType == FormatType.Docx))
			{
				num3 = format.RightIndent;
			}
			float num4 = num3 + format.FirstLineIndent;
			float num5 = 0f;
			if (wListLevel != null)
			{
				num4 = wListLevel.TextPosition;
				num5 = wListLevel.NumberPosition;
			}
			stringBuilder.Append("margin-left:" + num4.ToString(CultureInfo.InvariantCulture) + "pt;");
			stringBuilder.Append("text-indent:" + num5.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		else
		{
			if (wParagraph != null && m_document.Settings.IsOptimizedForBrowser)
			{
				float num6 = format.LeftIndent;
				float firstLineIndent = format.FirstLineIndent;
				if (format.Bidi && (format.Document.ActualFormatType == FormatType.Doc || format.Document.ActualFormatType == FormatType.Docx))
				{
					num6 = format.RightIndent;
				}
				if (num6 < 0f)
				{
					num6 = 0f;
				}
				if (firstLineIndent < 0f)
				{
					num6 += ((num6 + firstLineIndent < 0f) ? Math.Abs(num6 + firstLineIndent) : 0f);
				}
				if (num6 > 0f || format.HasValue(2))
				{
					stringBuilder.Append("margin-left:" + num6.ToString(CultureInfo.InvariantCulture) + "pt;");
				}
			}
			else if (format.HasValueWithParent(2))
			{
				float num7 = format.LeftIndent;
				if (format.Bidi && (format.Document.ActualFormatType == FormatType.Doc || format.Document.ActualFormatType == FormatType.Docx))
				{
					num7 = format.RightIndent;
				}
				stringBuilder.Append("margin-left:" + num7.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
			if (format.HasValueWithParent(5))
			{
				stringBuilder.Append("text-indent:" + format.FirstLineIndent.ToString(CultureInfo.InvariantCulture) + "pt;");
			}
		}
		if (format.HasValueWithParent(3))
		{
			float num8 = format.RightIndent;
			if (format.Bidi && (format.Document.ActualFormatType == FormatType.Doc || format.Document.ActualFormatType == FormatType.Docx))
			{
				num8 = format.LeftIndent;
			}
			stringBuilder.Append("margin-right:" + num8.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		if (format.TextureStyle != 0)
		{
			stringBuilder.Append("background-color:" + GetParagraphBackground(format) + ";");
		}
		else if (format.BackColor != Color.Empty)
		{
			stringBuilder.Append("background-color:" + GetColor(format.BackColor) + ";");
		}
		return GetBordersStyle(format.Borders, stringBuilder);
	}

	private bool ContextualSpacingChecking()
	{
		if (m_currPara != null && m_currPara.NextSibling != null)
		{
			if (m_currPara.NextSibling is WParagraph && m_currPara.StyleName == (m_currPara.NextSibling as WParagraph).StyleName)
			{
				return true;
			}
			if (m_currPara.NextSibling is WTable)
			{
				WParagraph paragraphFromTable = GetParagraphFromTable(m_currPara.NextSibling as WTable);
				if (m_currPara.StyleName == paragraphFromTable.StyleName)
				{
					return true;
				}
			}
		}
		return false;
	}

	private WParagraph GetParagraphFromTable(WTable table)
	{
		WParagraph result = null;
		if (table.Rows[0] != null)
		{
			WTableCell wTableCell = table.Rows[0].Cells[0];
			if (wTableCell.Items[0] is WParagraph)
			{
				result = wTableCell.Items[0] as WParagraph;
			}
			else if (wTableCell.Items[0] is WTable)
			{
				result = GetParagraphFromTable(wTableCell.Items[0] as WTable);
			}
		}
		return result;
	}

	private string GetStyle(WCharacterFormat format)
	{
		return GetStyle(format, style: true);
	}

	private string GetStyle(WCharacterFormat format, bool style)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (format.CharacterSpacing != 0f)
		{
			stringBuilder.Append("letter-spacing:" + format.CharacterSpacing.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		if (format.TextColor != Color.Empty)
		{
			stringBuilder.Append("color:" + GetColor(format.TextColor) + ";");
		}
		if (!(format.OwnerBase is WSymbol))
		{
			stringBuilder.Append("font-family:" + (format.FontName.Contains(" ") ? ("'" + format.FontName + "'") : format.FontName) + ";");
		}
		if ((double)format.FontSize > 0.0)
		{
			stringBuilder.Append("font-size:" + format.FontSize.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		if (!format.HighlightColor.IsEmpty)
		{
			stringBuilder.Append("background-color:" + GetHighlightColor(format.HighlightColor) + ";");
		}
		else if (!format.TextBackgroundColor.IsEmpty)
		{
			stringBuilder.Append("background-color:" + GetColor(format.TextBackgroundColor) + ";");
		}
		stringBuilder.Append(format.AllCaps ? "text-transform:uppercase;" : "text-transform:none;");
		if (format.Bold || format.Emboss || format.OutLine)
		{
			stringBuilder.Append("font-weight:bold;");
		}
		else
		{
			stringBuilder.Append("font-weight:normal;");
		}
		if (format.Hidden)
		{
			stringBuilder.Append("display:none;");
		}
		stringBuilder.Append(format.Italic ? "font-style:italic;" : "font-style:normal;");
		stringBuilder.Append(format.SmallCaps ? "font-variant:small-caps;" : "font-variant:normal;");
		if (format.SubSuperScript == SubSuperScript.SubScript && style)
		{
			stringBuilder.Append("vertical-align:sub;");
		}
		else if (format.SubSuperScript == SubSuperScript.SuperScript && style)
		{
			stringBuilder.Append("vertical-align:super;");
		}
		if (m_currPara == null || (m_currPara != null && (m_currPara.Text != "" || format.OwnerBase is WSymbol)))
		{
			if (format.UnderlineStyle != 0)
			{
				stringBuilder.Append("text-decoration: underline;");
			}
			else if (format.UnderlineStyle == UnderlineStyle.None)
			{
				stringBuilder.Append("text-decoration: none;");
			}
			if (format.DoubleStrike || format.Strikeout)
			{
				stringBuilder.Append("text-decoration: line-through;");
			}
		}
		if (m_currPara != null && Math.Abs(m_currPara.ParagraphFormat.LineSpacing) > 0f)
		{
			if (Math.Abs(m_currPara.ParagraphFormat.LineSpacing) != 12f && m_currPara.ParagraphFormat.LineSpacingRule == LineSpacingRule.Multiple)
			{
				stringBuilder.Append("line-height:" + (Math.Abs(m_currPara.ParagraphFormat.LineSpacing) / 12f * 100f).ToString(CultureInfo.InvariantCulture) + "%;");
			}
			else if (Math.Abs(m_currPara.ParagraphFormat.LineSpacing) != 12f && (m_currPara.ParagraphFormat.LineSpacingRule != 0 || Math.Abs(m_currPara.ParagraphFormat.LineSpacing) > 10f))
			{
				stringBuilder.Append("line-height:" + Math.Abs(m_currPara.ParagraphFormat.LineSpacing).ToString(CultureInfo.InvariantCulture) + "pt;");
			}
		}
		return stringBuilder.ToString();
	}

	private string GetHighlightColor(Color color)
	{
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
		dictionary.Add("green", Color.Lime);
		dictionary.Add("cyan", Color.Aqua);
		dictionary.Add("magenta", Color.Fuchsia);
		dictionary.Add("darkmagenta", Color.Purple);
		dictionary.Add("darkyellow", Color.Olive);
		dictionary.Add("gold", Color.Olive);
		string key = color.Name.ToLower();
		if (dictionary.ContainsKey(key))
		{
			color = dictionary[key];
		}
		return GetColor(color);
	}

	private void SetCheckBoxSize(string style, WCheckBox checkBox, ref float checkBoxSize)
	{
		style = style + "-sf-size-type:" + checkBox.SizeType.ToString().ToLowerInvariant() + ";";
		checkBoxSize = ((checkBox.SizeType != 0) ? ((float)checkBox.CheckBoxSize) : checkBox.CharacterFormat.FontSize);
		style = style + "width:" + checkBoxSize.ToString(CultureInfo.InvariantCulture) + "pt;";
		style = style + "height:" + checkBoxSize.ToString(CultureInfo.InvariantCulture) + "pt;";
		if (style.Length > 0)
		{
			m_writer.WriteAttributeString("style", style);
		}
	}

	private string GetColor(Color color)
	{
		return "#" + (color.ToArgb() & 0xFFFFFF).ToString("X6");
	}

	private string GetBordersStyle(Borders borders, StringBuilder sb)
	{
		WParagraphFormat wParagraphFormat = null;
		WParagraphFormat wParagraphFormat2 = null;
		if (borders.ParentFormat is WParagraphFormat && (borders.ParentFormat as WParagraphFormat).OwnerBase is WParagraph wParagraph)
		{
			if (wParagraph.PreviousSibling is WParagraph)
			{
				wParagraphFormat = (wParagraph.PreviousSibling as WParagraph).ParagraphFormat;
			}
			if (wParagraph.NextSibling is WParagraph)
			{
				wParagraphFormat2 = (wParagraph.NextSibling as WParagraph).ParagraphFormat;
			}
		}
		if (wParagraphFormat == null || wParagraphFormat.Borders.Top.BorderType != borders.Top.BorderType || wParagraphFormat.Borders.Top.LineWidth != borders.Top.LineWidth)
		{
			GetBorderStyle("top", borders.Top, sb);
		}
		GetBorderStyle("left", borders.Left, sb);
		GetBorderStyle("right", borders.Right, sb);
		if (wParagraphFormat2 == null || wParagraphFormat2.Borders.Bottom.BorderType != borders.Bottom.BorderType || wParagraphFormat2.Borders.Bottom.LineWidth != borders.Bottom.LineWidth)
		{
			GetBorderStyle("bottom", borders.Bottom, sb);
		}
		return sb.ToString();
	}

	private void GetBorderStyle(string suffix, Border border, StringBuilder sb)
	{
		if (border.BorderType == BorderStyle.Cleared)
		{
			return;
		}
		if (border.Color != Color.Empty)
		{
			sb.Append("border-" + suffix + "-color:" + GetColor(border.Color) + ";");
		}
		else if (border.BorderType != 0)
		{
			sb.Append("border-" + suffix + "-color:#000000;");
		}
		if (border.BorderType != 0 || border.HasNoneStyle)
		{
			sb.Append("border-" + suffix + "-style:" + ToParagraphBorderStyle(border.BorderType) + ";");
		}
		if (border.LineWidth > 0f)
		{
			float num = ((border.BorderType == BorderStyle.Double) ? (border.LineWidth * 3f) : border.LineWidth);
			if (num < 1f)
			{
				num = 1f;
			}
			sb.Append("border-" + suffix + "-width:" + num.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
		if (border.Space > 0f)
		{
			sb.Append("padding-" + suffix + ":" + border.Space.ToString(CultureInfo.InvariantCulture) + "pt;");
		}
	}

	private string ToBorderStyle(BorderStyle style)
	{
		switch (style)
		{
		case BorderStyle.None:
			return "hidden";
		case BorderStyle.DotDash:
			return "dashed";
		case BorderStyle.DotDotDash:
			return "dashed";
		case BorderStyle.Triple:
			return "double";
		case BorderStyle.ThinThickSmallGap:
			return "double";
		case BorderStyle.ThinThinSmallGap:
			return "double";
		case BorderStyle.ThinThickThinSmallGap:
			return "double";
		case BorderStyle.ThinThickMediumGap:
			return "double";
		case BorderStyle.ThickThinMediumGap:
			return "double";
		case BorderStyle.ThickThickThinMediumGap:
			return "double";
		case BorderStyle.ThinThickLargeGap:
			return "double;";
		case BorderStyle.ThickThinLargeGap:
			return "double";
		case BorderStyle.ThinThickThinLargeGap:
			return "double";
		case BorderStyle.DoubleWave:
			return "double";
		case BorderStyle.DashSmallGap:
			return "dashed";
		case BorderStyle.Single:
		case BorderStyle.Thick:
		case BorderStyle.Hairline:
		case BorderStyle.Wave:
		case BorderStyle.DashDotStroker:
			return "solid";
		case BorderStyle.DashLargeGap:
			return "dashed";
		case BorderStyle.Dot:
			return "dotted";
		case BorderStyle.Double:
			return "double";
		case BorderStyle.Engrave3D:
			return "groove";
		case BorderStyle.Inset:
			return "inset";
		case BorderStyle.Outset:
			return "outset";
		case BorderStyle.Emboss3D:
			return "ridge";
		case BorderStyle.Cleared:
			return "none";
		default:
			return "solid";
		}
	}

	private string ToParagraphBorderStyle(BorderStyle style)
	{
		return style switch
		{
			BorderStyle.None => "hidden", 
			BorderStyle.DotDash => "dashed", 
			BorderStyle.DotDotDash => "dashed", 
			BorderStyle.DashLargeGap => "dashed", 
			BorderStyle.Dot => "dotted", 
			BorderStyle.Double => "double", 
			BorderStyle.Inset => "inset", 
			BorderStyle.Outset => "outset", 
			BorderStyle.Cleared => "none", 
			_ => "solid", 
		};
	}

	private string EncodeName(string name)
	{
		name = name.Trim();
		name = CheckValidSymbols(name);
		if (name.StartsWith("-"))
		{
			name = name.Remove(0, 1);
		}
		if (char.IsDigit(name[0]))
		{
			name = "Style_" + name;
		}
		return name;
	}

	private string CheckValidSymbols(string name)
	{
		int i = 0;
		for (int length = name.Length; i < length; i++)
		{
			if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
			{
				name = name.Replace(name[i], '_');
			}
		}
		return name;
	}

	private void WriteEmptyPara(WCharacterFormat chFormat)
	{
		m_writer.WriteStartElement("span");
		string style = GetStyle(chFormat);
		if (style.Length > 0)
		{
			m_writer.WriteAttributeString("style", style);
		}
		m_writer.WriteRaw("&#xa0;");
		m_writer.WriteEndElement();
	}

	private void WriteText(string text)
	{
		text = text.Replace("&", "&amp;");
		if (text.Contains(ControlChar.Tab))
		{
			string tabText = GetTabText();
			text = text.Replace(ControlChar.Tab, tabText);
		}
		text = text.Replace("<", "&lt;");
		text = text.Replace(">", "&gt;");
		text = text.Replace(ControlChar.LineBreak, "</br>");
		text = text.Replace('\u001e'.ToString(), "&#8209;");
		text = text.Replace('\u001f'.ToString(), "&#xad;");
		text = text.Replace("\"", "&quot;");
		text = text.Replace('\u0002'.ToString(), ControlChar.Space);
		m_writer.WriteRaw(DocxSerializator.ReplaceInvalidSurrogateCharacters(text));
	}

	private string GetTabText()
	{
		string text = string.Empty;
		for (int i = 0; i < 15; i++)
		{
			text += ControlChar.NonBreakingSpace;
		}
		return text + ControlChar.Space;
	}

	private string ReplaceEmptySpace(string text)
	{
		if (text.StartsWith(ControlChar.Space))
		{
			string text2 = text.TrimStart(' ');
			string text3 = text[..text.IndexOf(text2)];
			if (text2.Length == 0)
			{
				text = text.Replace(ControlChar.Space, ControlChar.NonBreakingSpace);
			}
			else
			{
				text3 = text3.Replace(ControlChar.Space, ControlChar.NonBreakingSpace);
				text = text3 + text2;
			}
		}
		return text;
	}

	private string GetClassAttr(Style style)
	{
		List<string> list = new List<string>();
		string text = string.Empty;
		if (style != null)
		{
			list.Add(style.Name);
			UpdateStyleHierarchy(style.BaseStyle as Style, list);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				text += EncodeName(list[num]);
				if (num > 0)
				{
					text += " ";
				}
			}
		}
		return text;
	}

	private void UpdateStyleHierarchy(Style style, List<string> styleHirarchy)
	{
		if (style != null && style.StyleId != 0 && !style.Name.StartsWith("Normal"))
		{
			styleHirarchy.Add(style.Name);
			if (style.BaseStyle != null)
			{
				UpdateStyleHierarchy(style.BaseStyle as Style, styleHirarchy);
			}
		}
	}

	private bool CheckParentFormat(WParagraphFormat format)
	{
		if (format.OwnerBase != null)
		{
			if (format.OwnerBase.OwnerBase is WTableCell && format.OwnerBase is WParagraph)
			{
				WTableCell obj = format.OwnerBase.OwnerBase as WTableCell;
				WParagraph wParagraph = format.OwnerBase as WParagraph;
				WTextRange wTextRange = new WTextRange(wParagraph.Document);
				float height = obj.OwnerRow.Height;
				if (wParagraph.Items.Count > 0 && wParagraph.Items.FirstItem is WTextRange)
				{
					wTextRange = wParagraph.Items.FirstItem as WTextRange;
				}
				if (height <= wTextRange.CharacterFormat.FontSize)
				{
					return false;
				}
			}
			else if (format.OwnerBase.OwnerBase is WTextBody && format.OwnerBase is WParagraph)
			{
				WParagraph wParagraph2 = format.OwnerBase as WParagraph;
				if (wParagraph2.Items.Count > 0 && wParagraph2.ListFormat.ListType == ListType.NoList)
				{
					IEntity entity = wParagraph2.Items.FirstItem;
					while (entity != null && entity.NextSibling != null && entity.EntityType != EntityType.TextRange)
					{
						entity = entity.NextSibling as Entity;
						if (entity is WTextRange)
						{
							break;
						}
					}
					if (entity is WTextRange)
					{
						WTextRange wTextRange2 = entity as WTextRange;
						if (Math.Abs(format.LineSpacing) <= wTextRange2.CharacterFormat.FontSize)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private void CloseList(int paraLevelNum, WParagraph paragraph)
	{
		IEntity previousSibling = paragraph.PreviousSibling;
		if (previousSibling != null && previousSibling is WParagraph)
		{
			WParagraph para = (WParagraph)previousSibling;
			WListFormat listFormat = GetListFormat(para);
			WListFormat listFormat2 = GetListFormat(paragraph);
			if ((paraLevelNum != listFormat.ListLevelNumber || listFormat2.CustomStyleName != listFormat.CustomStyleName || listFormat2.LFOStyleName != listFormat.LFOStyleName) && listStack.Count > 0)
			{
				WriteEndElement(listStack.Count);
			}
		}
		else if (listStack.Count > 0)
		{
			WriteEndElement(listStack.Count);
		}
	}

	private void WriteEndElement(int levelDiff)
	{
		for (int i = 0; i < levelDiff; i++)
		{
			if (listStack.Count > 0)
			{
				m_writer.WriteEndElement();
				m_writer.WriteRaw(ControlChar.CrLf);
				listStack.Pop();
			}
		}
	}

	private void WriteListType(ListPatternType type, WListFormat listFormat)
	{
		string value;
		switch (type)
		{
		case ListPatternType.LowLetter:
			value = "a";
			break;
		case ListPatternType.UpLetter:
			value = "A";
			break;
		case ListPatternType.LowRoman:
			value = "i";
			break;
		case ListPatternType.UpRoman:
			value = "I";
			break;
		default:
			value = "1";
			break;
		case ListPatternType.Bullet:
			switch (listFormat.CurrentListLevel.LevelNumber)
			{
			case 0:
			case 3:
				value = "disc";
				break;
			case 1:
			case 4:
				value = "circle";
				break;
			case 2:
			case 5:
				value = "square";
				break;
			default:
				value = "disc";
				break;
			}
			break;
		}
		m_writer.WriteAttributeString("type", value);
	}

	private int GetLevelNumer(WListFormat listFormat)
	{
		if (listFormat.ListType != ListType.NoList)
		{
			return listFormat.ListLevelNumber;
		}
		return -1;
	}

	private int GetLstStartVal(WListFormat format)
	{
		if (format.CurrentListLevel.PatternType == ListPatternType.Bullet)
		{
			return 1;
		}
		if (!Lists.ContainsKey(format.CustomStyleName))
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Lists.Add(format.CustomStyleName, dictionary);
			WListLevel wListLevel = format.CurrentListStyle.Levels[format.ListLevelNumber];
			int num = 0;
			for (int i = 0; i <= wListLevel.LevelNumber; i++)
			{
				num = GetListStartAt(format, i);
				if (num >= 0)
				{
					dictionary.Add(i, num + 1);
				}
			}
			return num;
		}
		Dictionary<int, int> dictionary2 = Lists[format.CustomStyleName];
		if (dictionary2.ContainsKey(format.ListLevelNumber))
		{
			int num2 = dictionary2[format.ListLevelNumber];
			dictionary2[format.ListLevelNumber] = num2 + 1;
			for (int j = format.ListLevelNumber; dictionary2.ContainsKey(j + 1); j++)
			{
				dictionary2[j + 1] = 1;
			}
			return num2;
		}
		WListLevel wListLevel2 = format.CurrentListStyle.Levels[format.ListLevelNumber];
		int num3 = 0;
		for (int k = 0; k <= wListLevel2.LevelNumber; k++)
		{
			if (!dictionary2.ContainsKey(k))
			{
				num3 = GetListStartAt(format, k);
				if (num3 >= 0)
				{
					dictionary2.Add(k, num3 + 1);
				}
			}
		}
		return num3;
	}

	private int GetListStartAt(WListFormat format, int levelNumber)
	{
		WListLevel wListLevel = format.CurrentListStyle.Levels[levelNumber];
		ListOverrideStyle listOverrideStyle = null;
		if (format.LFOStyleName != null && format.LFOStyleName.Length > 0)
		{
			listOverrideStyle = m_document.ListOverrides.FindByName(format.LFOStyleName);
		}
		if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(levelNumber))
		{
			if (listOverrideStyle.OverrideLevels[levelNumber].OverrideFormatting)
			{
				return listOverrideStyle.OverrideLevels[levelNumber].OverrideListLevel.StartAt;
			}
			if (listOverrideStyle.OverrideLevels[levelNumber].OverrideStartAtValue)
			{
				return listOverrideStyle.OverrideLevels[levelNumber].StartAt;
			}
		}
		return wListLevel.StartAt;
	}

	private void EnsureLvlRestart(WListFormat format, bool fullRestart)
	{
		if (m_lists == null || !Lists.ContainsKey(format.CustomStyleName))
		{
			return;
		}
		Dictionary<int, int> dictionary = Lists[format.CustomStyleName];
		Dictionary<int, int>.KeyCollection keys = dictionary.Keys;
		IEnumerator enumerator = ((IEnumerable)keys).GetEnumerator();
		int count = ((ICollection)keys).Count;
		int[] array = new int[count];
		int num = 0;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != null)
			{
				array[num] = (int)enumerator.Current;
			}
			num++;
		}
		for (int i = 0; i < count; i++)
		{
			if (fullRestart || (array[i] != 0 && !format.CurrentListStyle.Levels[array[i]].NoRestartByHigher))
			{
				dictionary[array[i]] = format.CurrentListStyle.Levels[array[i]].StartAt;
			}
		}
	}

	private float Build_TextureStyle(TextureStyle ts)
	{
		switch (ts)
		{
		case TextureStyle.Texture5Percent:
		case TextureStyle.Texture2Pt5Percent:
		case TextureStyle.Texture7Pt5Percent:
			return 5f;
		case TextureStyle.Texture10Percent:
			return 10f;
		case TextureStyle.Texture12Pt5Percent:
			return 12.5f;
		case TextureStyle.Texture15Percent:
			return 15f;
		case TextureStyle.Texture17Pt5Percent:
			return 17.5f;
		case TextureStyle.Texture20Percent:
			return 20f;
		case TextureStyle.Texture25Percent:
		case TextureStyle.Texture27Pt5Percent:
			return 27.5f;
		case TextureStyle.Texture30Percent:
		case TextureStyle.Texture32Pt5Percent:
			return 32.5f;
		case TextureStyle.Texture35Percent:
			return 35f;
		case TextureStyle.Texture37Pt5Percent:
			return 37.5f;
		case TextureStyle.Texture40Percent:
		case TextureStyle.Texture42Pt5Percent:
			return 40f;
		case TextureStyle.Texture45Percent:
		case TextureStyle.Texture47Pt5Percent:
			return 45f;
		case TextureStyle.Texture50Percent:
		case TextureStyle.Texture52Pt5Percent:
			return 50f;
		case TextureStyle.Texture55Percent:
		case TextureStyle.Texture57Pt5Percent:
			return 55f;
		case TextureStyle.Texture60Percent:
			return 60f;
		case TextureStyle.Texture62Pt5Percent:
			return 62.5f;
		case TextureStyle.Texture65Percent:
		case TextureStyle.Texture67Pt5Percent:
			return 65f;
		case TextureStyle.Texture70Percent:
		case TextureStyle.Texture72Pt5Percent:
			return 70f;
		case TextureStyle.Texture75Percent:
		case TextureStyle.Texture77Pt5Percent:
			return 75f;
		case TextureStyle.Texture80Percent:
		case TextureStyle.Texture82Pt5Percent:
			return 80f;
		case TextureStyle.Texture85Percent:
			return 85f;
		case TextureStyle.Texture87Pt5Percent:
			return 87.5f;
		case TextureStyle.Texture90Percent:
		case TextureStyle.Texture92Pt5Percent:
			return 90f;
		case TextureStyle.Texture95Percent:
		case TextureStyle.Texture97Pt5Percent:
			return 95f;
		case TextureStyle.TextureSolid:
			return 100f;
		default:
			return 0f;
		}
	}

	private WListFormat GetListFormat(WParagraph para)
	{
		if (string.IsNullOrEmpty(para.StyleName) || para.StyleName == "Normal")
		{
			return para.ListFormat;
		}
		if (para.ListFormat.CurrentListLevel != null && para.ListFormat.CurrentListStyle != null)
		{
			return para.ListFormat;
		}
		if (!para.ListFormat.IsEmptyList)
		{
			WListFormat wListFormat = new WListFormat(para);
			WParagraphStyle wParagraphStyle = null;
			while (wListFormat.CurrentListLevel == null && wListFormat.CurrentListStyle == null)
			{
				wParagraphStyle = ((wParagraphStyle == null) ? (para.Document.Styles.FindByName(para.StyleName) as WParagraphStyle) : wParagraphStyle.BaseStyle);
				if (wParagraphStyle == null || wParagraphStyle.ListFormat == null || wParagraphStyle.ListFormat.IsEmptyList)
				{
					break;
				}
				if (wListFormat.CurrentListLevel == null && wListFormat.ListLevelNumber == 0 && wParagraphStyle.ListFormat.CurrentListLevel != null && wParagraphStyle.ListFormat.CurrentListLevel.LevelNumber >= 0 && wParagraphStyle.ListFormat.CurrentListLevel.LevelNumber <= 8)
				{
					wListFormat.ListLevelNumber = wParagraphStyle.ListFormat.ListLevelNumber;
				}
				else
				{
					for (WParagraphStyle wParagraphStyle2 = para.ParaStyle as WParagraphStyle; wParagraphStyle2 != null; wParagraphStyle2 = wParagraphStyle2.BaseStyle)
					{
						int outLineLevel = GetOutLineLevel(wParagraphStyle2.ParagraphFormat);
						if (outLineLevel != -1)
						{
							wListFormat.ListLevelNumber = outLineLevel;
							break;
						}
					}
				}
				if (wListFormat.CurrentListStyle == null && !string.IsNullOrEmpty(wParagraphStyle.ListFormat.CustomStyleName))
				{
					wListFormat.ApplyStyle(wParagraphStyle.ListFormat.CustomStyleName);
				}
				if (!string.IsNullOrEmpty(wParagraphStyle.ListFormat.LFOStyleName))
				{
					wListFormat.LFOStyleName = wParagraphStyle.ListFormat.LFOStyleName;
				}
			}
			return wListFormat;
		}
		return para.ListFormat;
	}

	private int GetOutLineLevel(WParagraphFormat paraFormat)
	{
		return paraFormat.OutlineLevel switch
		{
			OutlineLevel.Level1 => 0, 
			OutlineLevel.Level2 => 1, 
			OutlineLevel.Level3 => 2, 
			OutlineLevel.Level4 => 3, 
			OutlineLevel.Level5 => 4, 
			OutlineLevel.Level6 => 5, 
			OutlineLevel.Level7 => 6, 
			OutlineLevel.Level8 => 7, 
			OutlineLevel.Level9 => 8, 
			_ => -1, 
		};
	}

	private void SortBehindWrapStyleItemByZindex()
	{
		m_document.SortByZIndex(isFromHTMLExport: true);
		List<WPicture> list = new List<WPicture>();
		List<int> list2 = new List<int>();
		foreach (Entity floatingItem in m_document.FloatingItems)
		{
			if (floatingItem is WPicture && (floatingItem as WPicture).TextWrappingStyle == TextWrappingStyle.Behind)
			{
				WPicture wPicture = floatingItem as WPicture;
				int num = Math.Abs(wPicture.OrderIndex);
				list.Add(wPicture);
				list2.Add(-num);
			}
		}
		int num2 = list2.Count - 1;
		while (list.Count != 0)
		{
			BehindWrapStyleFloatingItems.Add(list[0], list2[num2]);
			list.RemoveAt(0);
			num2--;
		}
	}

	private void CreateNavigationPoint(WParagraph para)
	{
		if (!m_hasNavigationId || string.IsNullOrEmpty(para.Text))
		{
			return;
		}
		foreach (TableOfContent value in m_document.TOC.Values)
		{
			if (m_document.HasTOC && !value.UseHeadingStyles)
			{
				foreach (int key in value.TOCStyles.Keys)
				{
					foreach (WParagraphStyle item in value.TOCStyles[key])
					{
						if (item.Name == para.StyleName && key <= m_document.SaveOptions.EPubHeadingLevels)
						{
							string navigationPoint = GetNavigationPoint();
							m_writer.WriteAttributeString("id", navigationPoint);
							m_bookmarks.Add($"{key};{navigationPoint}", GetParagraphText(para.Text));
							break;
						}
					}
				}
			}
			else if (CheckHeadingStyle(para.StyleName))
			{
				int headingLevel = GetHeadingLevel(para.StyleName);
				if (headingLevel <= m_document.SaveOptions.EPubHeadingLevels)
				{
					string navigationPoint2 = GetNavigationPoint();
					m_writer.WriteAttributeString("id", navigationPoint2);
					m_bookmarks.Add($"{headingLevel.ToString(CultureInfo.InvariantCulture)};{navigationPoint2}", GetParagraphText(para.Text));
				}
			}
		}
	}

	private bool CheckHeadingStyle(string styleName)
	{
		bool result = false;
		string[] headingStyles = m_headingStyles;
		foreach (string value in headingStyles)
		{
			if (styleName.ToLowerInvariant().Replace(" ", string.Empty).Contains(value))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private string GetParagraphText(string text)
	{
		text = text.Replace(ControlChar.LineBreak, string.Empty);
		return text;
	}

	private int GetHeadingLevel(string p)
	{
		if (p.Contains(","))
		{
			p = p.Split(new char[1] { ',' })[0];
		}
		if (p.Contains("+"))
		{
			p = p.Split(new char[1] { '+' })[0];
		}
		char[] array = p.ToCharArray();
		string text = "";
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			char c = array2[i];
			if (c == '_')
			{
				break;
			}
			if (int.TryParse(string.Format(CultureInfo.InvariantCulture, c.ToString()), out var result))
			{
				text += result.ToString(CultureInfo.InvariantCulture);
			}
		}
		return int.Parse(text);
	}

	internal string GetNavigationPoint()
	{
		return "nav_Point" + m_nameID++;
	}
}
