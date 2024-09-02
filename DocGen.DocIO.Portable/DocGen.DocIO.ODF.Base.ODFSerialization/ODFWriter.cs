using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ODF.Base.ODFImplementation;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation;
using DocGen.Drawing;

namespace DocGen.DocIO.ODF.Base.ODFSerialization;

internal class ODFWriter
{
	private ZipArchive m_archieve;

	private XmlWriter m_writer;

	private ODocument m_document;

	private ODFStyleCollection m_odfStyles;

	private Dictionary<string, string> m_dateFormat;

	internal Dictionary<string, string> DateFormat
	{
		get
		{
			if (m_dateFormat == null)
			{
				m_dateFormat = new Dictionary<string, string>();
			}
			return m_dateFormat;
		}
	}

	public ODFWriter()
	{
		m_archieve = new ZipArchive();
		m_archieve.DefaultCompressionLevel = CompressionLevel.Best;
	}

	private XmlWriter CreateWriter(Stream data)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		xmlWriterSettings.Encoding = encoding;
		XmlWriter xmlWriter = XmlWriter.Create(data, xmlWriterSettings);
		xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"");
		return xmlWriter;
	}

	internal void SaveDocument(Stream stream)
	{
		m_archieve.Save(stream, closeStream: false);
		m_archieve.Dispose();
	}

	internal void SerializeDocumentManifest()
	{
		m_archieve.AddItem("META-INF/", new MemoryStream(), bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		MemoryStream data = new MemoryStream();
		m_writer = CreateWriter(data);
		m_writer.WriteStartElement("manifest", "manifest", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteAttributeString("manifest", "full-path", null, "/");
		m_writer.WriteAttributeString("manifest", "media-type", null, "application/vnd.oasis.opendocument.text");
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteAttributeString("manifest", "full-path", null, "styles.xml");
		m_writer.WriteAttributeString("manifest", "media-type", null, "text/xml");
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteAttributeString("manifest", "full-path", null, "content.xml");
		m_writer.WriteAttributeString("manifest", "media-type", null, "text/xml");
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteAttributeString("manifest", "full-path", null, "settings.xml");
		m_writer.WriteAttributeString("manifest", "media-type", null, "text/xml");
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
		m_writer.WriteAttributeString("manifest", "full-path", null, "meta.xml");
		m_writer.WriteAttributeString("manifest", "media-type", null, "text/xml");
		m_writer.WriteEndElement();
		if (m_document != null && m_document.DocumentImages != null && m_document.DocumentImages.Count > 0)
		{
			foreach (KeyValuePair<string, ImageRecord> documentImage in m_document.DocumentImages)
			{
				string empty = string.Empty;
				ImageRecord value = documentImage.Value;
				if (value == null)
				{
					empty = "media/image0.jpeg";
					continue;
				}
				string text = (value.IsMetafile ? ".wmf" : ".jpeg");
				if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Bmp))
				{
					text = ".bmp";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Emf))
				{
					text = ".emf";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Exif))
				{
					text = ".exif";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Gif))
				{
					text = ".gif";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Icon))
				{
					text = ".ico";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Jpeg))
				{
					text = ".jpeg";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.MemoryBmp))
				{
					text = ".bmp";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Png))
				{
					text = ".png";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff))
				{
					text = ".tif";
				}
				else if (value.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Wmf))
				{
					text = ".wmf";
				}
				empty = "media/image" + documentImage.Key.Replace("rId", "") + text;
				m_writer.WriteStartElement("manifest", "file-entry", "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0");
				m_writer.WriteAttributeString("manifest", "full-path", null, empty);
				m_writer.WriteAttributeString("manifest", "media-type", null, "image/" + text.Substring(1));
				m_writer.WriteEndElement();
				if (m_archieve.Find(empty.Replace("\\", "/")) == -1)
				{
					m_archieve.AddItem(empty, new MemoryStream(value.ImageBytes), bControlStream: false, DocGen.Compression.FileAttributes.Archive);
				}
			}
		}
		m_writer.WriteEndElement();
		m_writer.Flush();
		m_archieve.AddItem("META-INF/manifest.xml", data, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal void SerializeMimeType()
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		streamWriter.Write("application/vnd.oasis.opendocument.text");
		streamWriter.Flush();
		m_archieve.AddItem("mimetype", memoryStream, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal void SerializeContent(MemoryStream stream)
	{
		m_archieve.AddItem("content.xml", stream, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal void SerializeMetaData()
	{
		MemoryStream data = new MemoryStream();
		m_writer = CreateWriter(data);
		m_writer.WriteStartElement("office", "document-meta", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
		m_writer.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
		m_writer.WriteEndElement();
		m_writer.Flush();
		m_archieve.AddItem("meta.xml", data, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal void SerializeSettings()
	{
		MemoryStream data = new MemoryStream();
		m_writer = CreateWriter(data);
		m_writer.WriteStartElement("office", "document-settings", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "anim", null, "urn:oasis:names:tc:opendocument:xmlns:animation:1.0");
		m_writer.WriteAttributeString("xmlns", "chart", null, "urn:oasis:names:tc:opendocument:xmlns:chart:1.0");
		m_writer.WriteAttributeString("xmlns", "onfig", null, "urn:oasis:names:tc:opendocument:xmlns:config:1.0");
		m_writer.WriteAttributeString("xmlns", "db", null, "urn:oasis:names:tc:opendocument:xmlns:database:1.0");
		m_writer.WriteAttributeString("xmlns", "dr3d", null, "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0");
		m_writer.WriteAttributeString("xmlns", "draw", null, "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
		m_writer.WriteAttributeString("xmlns", "fo", null, "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "form", null, "urn:oasis:names:tc:opendocument:xmlns:form:1.0");
		m_writer.WriteAttributeString("xmlns", "meta", null, "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
		m_writer.WriteAttributeString("xmlns", "number", null, "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
		m_writer.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "presentation", null, "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0");
		m_writer.WriteAttributeString("xmlns", "script", null, "urn:oasis:names:tc:opendocument:xmlns:script:1.0");
		m_writer.WriteAttributeString("xmlns", "smil", null, "urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "style", null, "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("xmlns", "svg", null, "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "table", null, "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
		m_writer.WriteAttributeString("xmlns", "text", null, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
		m_writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
		m_writer.WriteEndElement();
		m_writer.Flush();
		m_archieve.AddItem("settings.xml", data, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal MemoryStream SerializeContentNameSpace()
	{
		MemoryStream memoryStream = new MemoryStream();
		m_writer = CreateWriter(memoryStream);
		m_writer.WriteStartElement("office", "document-content", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "table", null, "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
		m_writer.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "style", null, "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("xmlns", "draw", null, "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
		m_writer.WriteAttributeString("xmlns", "fo", null, "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
		m_writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
		m_writer.WriteAttributeString("xmlns", "number", null, "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
		m_writer.WriteAttributeString("xmlns", "svg", null, "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "text", null, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("xmlns", "of", null, "urn:oasis:names:tc:opendocument:xmlns:of:1.2");
		m_writer.WriteAttributeString("xmlns", "anim", null, "urn:oasis:names:tc:opendocument:xmlns:animation:1.0");
		m_writer.WriteAttributeString("xmlns", "chart", null, "urn:oasis:names:tc:opendocument:xmlns:chart:1.0");
		m_writer.WriteAttributeString("xmlns", "onfig", null, "urn:oasis:names:tc:opendocument:xmlns:config:1.0");
		m_writer.WriteAttributeString("xmlns", "db", null, "urn:oasis:names:tc:opendocument:xmlns:database:1.0");
		m_writer.WriteAttributeString("xmlns", "dr3d", null, "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0");
		m_writer.WriteAttributeString("xmlns", "form", null, "urn:oasis:names:tc:opendocument:xmlns:form:1.0");
		m_writer.WriteAttributeString("xmlns", "meta", null, "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
		m_writer.WriteAttributeString("xmlns", "presentation", null, "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0");
		m_writer.WriteAttributeString("xmlns", "script", null, "urn:oasis:names:tc:opendocument:xmlns:script:1.0");
		m_writer.WriteAttributeString("xmlns", "smil", null, "urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
		return memoryStream;
	}

	internal void SerializeContentEnd(MemoryStream stream)
	{
		m_writer.WriteEndElement();
		m_writer.Flush();
		m_archieve.AddItem("content.xml", stream, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	internal void SerializeBodyStart()
	{
		m_writer.WriteStartElement("office", "body", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
	}

	internal void SerializeHeaderFooterContent(HeaderFooterContent headerFooter)
	{
		Stack<int> listStack = null;
		string m_previousParaListStyleName = string.Empty;
		for (int i = 0; i < headerFooter.ChildItems.Count; i++)
		{
			OTextBodyItem oTextBodyItem = headerFooter.ChildItems[i];
			if (oTextBodyItem is OParagraph)
			{
				OParagraph paragraph = (OParagraph)oTextBodyItem;
				SerializeOParagraph(paragraph, ref listStack, ref m_previousParaListStyleName);
			}
			if (oTextBodyItem is OTable)
			{
				OTable item = oTextBodyItem as OTable;
				List<OTable> list = new List<OTable>();
				list.Add(item);
				SerializeTables(list);
			}
		}
	}

	internal void SerializeDocIOContent(ODocument document)
	{
		m_document = document;
		SerializeBodyStart();
		Stack<int> listStack = null;
		m_writer.WriteStartElement("office", "text", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("text", "use-soft-page-breaks", null, "true");
		string m_previousParaListStyleName = string.Empty;
		for (int i = 0; i < document.Body.TextBodyItems.Count; i++)
		{
			OTextBodyItem oTextBodyItem = document.Body.TextBodyItems[i];
			if (oTextBodyItem.IsFirstItemOfSection)
			{
				m_writer.WriteStartElement("text", "section", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				m_writer.WriteAttributeString("text", "name", null, "Sect" + Regex.Match(oTextBodyItem.SectionStyleName, "\\d+").Value);
				m_writer.WriteAttributeString("text", "style-name", null, oTextBodyItem.SectionStyleName);
			}
			if (oTextBodyItem is OParagraph)
			{
				OParagraph paragraph = (OParagraph)oTextBodyItem;
				SerializeOParagraph(paragraph, ref listStack, ref m_previousParaListStyleName);
			}
			if (oTextBodyItem is OTable)
			{
				OTable item = oTextBodyItem as OTable;
				List<OTable> list = new List<OTable>();
				list.Add(item);
				SerializeTables(list);
			}
			if (oTextBodyItem.IsLastItemOfSection)
			{
				m_writer.WriteEndElement();
			}
		}
		if (listStack != null && listStack.Count > 0)
		{
			SerializeEndList(ref listStack);
		}
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void SerializeList(OParagraph paragraph, ref Stack<int> listStack, ref string m_previousParaListStyleName)
	{
		if (!string.IsNullOrEmpty(paragraph.ListStyleName))
		{
			if (listStack == null)
			{
				m_previousParaListStyleName = paragraph.ListStyleName;
				SerializeListStartStyle(ref listStack, paragraph);
			}
			else if (paragraph.ListStyleName == m_previousParaListStyleName)
			{
				int num = listStack.Peek();
				int listLevelNumber = paragraph.ListLevelNumber;
				if (num == listLevelNumber)
				{
					m_writer.WriteEndElement();
					m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				}
				else if (num > listLevelNumber)
				{
					while (num > listLevelNumber)
					{
						m_writer.WriteEndElement();
						m_writer.WriteEndElement();
						num--;
					}
					m_writer.WriteEndElement();
					listStack.Pop();
					listStack.Push(listLevelNumber);
					m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				}
				else if (num < listLevelNumber)
				{
					m_writer.WriteStartElement("text", "list", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					m_writer.WriteAttributeString("text", "continue-numbering", null, "true");
					m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					for (int i = num + 1; i < listLevelNumber; i++)
					{
						m_writer.WriteStartElement("text", "list", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
						m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					}
					listStack.Push(listLevelNumber);
				}
			}
			else
			{
				SerializeEndList(ref listStack);
				m_previousParaListStyleName = paragraph.ListStyleName;
				SerializeListStartStyle(ref listStack, paragraph);
			}
		}
		else if (listStack != null && listStack.Count > 0)
		{
			SerializeEndList(ref listStack);
			listStack = null;
			m_previousParaListStyleName = string.Empty;
		}
	}

	private void SerializeListStartStyle(ref Stack<int> listStack, OParagraph paragraph)
	{
		listStack = new Stack<int>();
		listStack.Push(paragraph.ListLevelNumber);
		m_writer.WriteStartElement("text", "list", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("text", "style-name", null, paragraph.ListStyleName);
		m_writer.WriteAttributeString("text", "continue-numbering", null, "true");
		m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		int num = listStack.Peek();
		for (int i = 0; i < num; i++)
		{
			m_writer.WriteStartElement("text", "list", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteStartElement("text", "list-item", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		}
	}

	private void SerializeEndList(ref Stack<int> listStack)
	{
		for (int num = listStack.Peek(); num > -1; num--)
		{
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
		listStack.Clear();
	}

	private void SerializeTableOfContentSource()
	{
		m_writer.WriteStartElement("text", "table-of-content-source", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("text", "outline-level", null, "9");
		m_writer.WriteAttributeString("text", "use-outline-level", null, "true");
		m_writer.WriteAttributeString("text", "use-index-marks", null, "false");
		m_writer.WriteAttributeString("text", "use-index-source-styles", null, "false");
		m_writer.WriteAttributeString("text", "index-scope", null, "document");
		for (int i = 1; i < 10; i++)
		{
			m_writer.WriteStartElement("text", "table-of-content-entry-template", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteAttributeString("text", "outline-level", null, i.ToString());
			m_writer.WriteAttributeString("text", "style-name", null, (i < m_document.TOCStyles.Count) ? ("TOC" + i) : "Normal");
			m_writer.WriteStartElement("text", "index-entry-text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteEndElement();
			TabStops tabStops = null;
			foreach (ODFStyle tOCStyle in m_document.TOCStyles)
			{
				if (tOCStyle.Name.EndsWith(i.ToString()))
				{
					tabStops = tOCStyle.ParagraphProperties.TabStops[0];
					m_writer.WriteStartElement("text", "index-entry-tab-stop", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
					m_writer.WriteAttributeString("style", "type", null, tabStops.TextAlignType.ToString());
					if (tabStops.TabStopLeader != 0)
					{
						m_writer.WriteAttributeString("style", "leader-char", null, (tabStops.TabStopLeader == TabStopLeader.Dotted) ? "." : "");
					}
					m_writer.WriteAttributeString("style", "position", null, tabStops.TextPosition + "in");
					m_writer.WriteEndElement();
					break;
				}
			}
			if (tabStops == null)
			{
				m_writer.WriteStartElement("text", "index-entry-tab-stop", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				m_writer.WriteAttributeString("style", "type", null, "right");
				m_writer.WriteAttributeString("style", "leader-char", null, ".");
				m_writer.WriteEndElement();
				break;
			}
			m_writer.WriteStartElement("text", "index-entry-page-number", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
		m_writer.WriteEndElement();
	}

	private string GetNumberFormat(PageNumberFormat pageNumberFormat)
	{
		return pageNumberFormat switch
		{
			PageNumberFormat.UpperRoman => "I", 
			PageNumberFormat.LowerRoman => "i", 
			PageNumberFormat.UpperCase => "", 
			PageNumberFormat.LowerCase => "", 
			PageNumberFormat.Arabic => "1", 
			PageNumberFormat.LowerAlphabet => "a", 
			PageNumberFormat.UpperAlphabet => "A", 
			PageNumberFormat.Ordinal => "1st, 2nd, 3rd, ...", 
			PageNumberFormat.CardinalText => "One, Two, Three, ...", 
			PageNumberFormat.OrdinalText => "First, Second, Third, ...", 
			PageNumberFormat.Hexa => "1, A, B, ...", 
			PageNumberFormat.DollorText => "One, Two, Three, ...", 
			PageNumberFormat.ArabicDash => "- 1 -, - 2 -, - 3 -, ...", 
			_ => "1", 
		};
	}

	internal void SerializePicture(OPicture picture)
	{
		m_writer.WriteStartElement("draw", "frame", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
		m_writer.WriteAttributeString("draw", "z-index", null, picture.OrderIndex.ToString());
		if (!string.IsNullOrEmpty(picture.Name))
		{
			m_writer.WriteAttributeString("draw", "name", null, picture.Name);
		}
		m_writer.WriteAttributeString("text", "anchor-type", null, (picture.TextWrappingStyle == TextWrappingStyle.Inline) ? "as-char" : "paragraph");
		m_writer.WriteAttributeString("svg", "x", null, picture.HorizontalPosition / 72f + "in");
		m_writer.WriteAttributeString("svg", "y", null, picture.VerticalPosition / 72f + "in");
		m_writer.WriteAttributeString("svg", "height", null, picture.Height / 72f + "in");
		m_writer.WriteAttributeString("svg", "width", null, picture.Width / 72f + "in");
		m_writer.WriteAttributeString("style", "rel-height", null, "scale");
		m_writer.WriteAttributeString("style", "rel-width", null, "scale");
		m_writer.WriteStartElement("draw", "image", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
		if (!string.IsNullOrEmpty(picture.OPictureHRef) && !picture.OPictureHRef.Contains("rId"))
		{
			m_writer.WriteAttributeString("xlink", "href", null, picture.OPictureHRef);
		}
		else
		{
			string key = picture.OPictureHRef.Substring(picture.OPictureHRef.IndexOf("rId"));
			if (m_document != null && m_document.DocumentImages != null && m_document.DocumentImages.Count > 0)
			{
				ImageRecord imageRecord = m_document.DocumentImages[key];
				if (imageRecord == null)
				{
					_ = picture.OPictureHRef.Replace("rId", "") + "0.jpeg";
				}
				else
				{
					string text = (imageRecord.IsMetafile ? ".wmf" : ".jpeg");
					if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Bmp))
					{
						text = ".bmp";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Emf))
					{
						text = ".emf";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Exif))
					{
						text = ".exif";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Gif))
					{
						text = ".gif";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Icon))
					{
						text = ".ico";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Jpeg))
					{
						text = ".jpeg";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.MemoryBmp))
					{
						text = ".bmp";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Png))
					{
						text = ".png";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff))
					{
						text = ".tif";
					}
					else if (imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Wmf))
					{
						text = ".wmf";
					}
					picture.OPictureHRef = picture.OPictureHRef.Replace("rId", "") + text;
				}
			}
			m_writer.WriteAttributeString("xlink", "href", null, picture.OPictureHRef);
		}
		m_writer.WriteAttributeString("xlink", "type", null, "simple");
		m_writer.WriteAttributeString("xlink", "show", null, "embed");
		m_writer.WriteAttributeString("xlink", "actuate", null, "onLoad");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	internal void SerializeMergeField(OMergeField mergeField)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(mergeField.TextBefore))
		{
			m_writer.WriteString(mergeField.TextBefore);
			if (!string.IsNullOrEmpty(mergeField.Text))
			{
				text += mergeField.Text.Replace(mergeField.TextBefore, "");
			}
		}
		else if (!string.IsNullOrEmpty(mergeField.Text))
		{
			text += mergeField.Text;
		}
		m_writer.WriteStartElement("text", "database-display", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("text", "table-name", null, "");
		m_writer.WriteAttributeString("text", "table-type", null, "table");
		m_writer.WriteAttributeString("text", "column-name", null, mergeField.FieldName);
		text = ((!string.IsNullOrEmpty(mergeField.TextAfter)) ? text.Replace(mergeField.TextAfter, "") : text);
		m_writer.WriteString(text);
		m_writer.WriteEndElement();
		if (!string.IsNullOrEmpty(mergeField.TextAfter))
		{
			m_writer.WriteString(mergeField.TextAfter);
		}
	}

	internal void SerializeDateTimeField(OField field)
	{
		_ = field.FormattingString;
		_ = string.Empty;
		m_writer.WriteStartElement("text", "date", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteString(field.Text);
		m_writer.WriteEndElement();
	}

	internal void SerializeHyperlink(OField field)
	{
		m_writer.WriteStartElement("text", "a", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		string text = field.FieldValue.TrimStart(new char[1] { '"' });
		text = text.TrimEnd(new char[1] { '"' });
		m_writer.WriteAttributeString("xlink", "href", null, text);
		m_writer.WriteAttributeString("office", "target-frame-name", null, "_top");
		m_writer.WriteAttributeString("xlink", "show", null, "replace");
		m_writer.WriteStartElement("text", "span", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("text", "style-name", null, "Hyperlink");
		m_writer.WriteString(field.Text);
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	private void DateStyle(string formattingString, string styleName, CultureInfo culture)
	{
		m_writer.WriteStartElement("number", "date-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
		m_writer.WriteAttributeString("style", "name", null, styleName);
		m_writer.WriteAttributeString("number", "language", null, culture.Parent.ToString());
		m_writer.WriteAttributeString("number", "country", null, culture.Name.Substring(3, 2));
		if (formattingString.Contains("dddd"))
		{
			m_writer.WriteStartElement("number", "day-of-week", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("number", "number:style", null, "long");
			m_writer.WriteAttributeString("number", "calendar", null, "gregorian");
			m_writer.WriteEndElement();
		}
		if (formattingString.Contains("MMMM"))
		{
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeDefaultStyles(DefaultStyleCollection defaultStyle)
	{
		int count = defaultStyle.DefaultStyles.Values.Count;
		DefaultStyle[] array = new DefaultStyle[count];
		defaultStyle.DefaultStyles.Values.CopyTo(array, 0);
		if (m_writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		for (int i = 0; i < count; i++)
		{
			DefaultStyle defaultStyle2 = array[i];
			m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			m_writer.WriteAttributeString("style", "family", null, "paragraph");
			SerializeDefaultParagraphProperties(defaultStyle2.ParagraphProperties);
			SerializeDefaultTextProperties(defaultStyle2.Textproperties);
			m_writer.WriteEndElement();
		}
	}

	private void SerializeCalculationSettings()
	{
		m_writer.WriteStartElement("table", "calculation-settings", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
		m_writer.WriteAttributeString("table", "use-regular-expressions", null, bool.FalseString.ToLower());
		m_writer.WriteEndElement();
	}

	private void SerializeDefaultParagraphProperties(ODFParagraphProperties paragraphProperties)
	{
		if (paragraphProperties == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "paragraph-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (paragraphProperties.HasKey(9, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "keep-together", null, paragraphProperties.KeepTogether.ToString());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "keep-together", null, "auto");
		}
		if (paragraphProperties.HasKey(3, paragraphProperties.m_CommonstyleFlags))
		{
			m_writer.WriteAttributeString("fo", "keep-with-next", null, paragraphProperties.KeepWithNext.ToString());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "keep-with-next", null, "auto");
		}
		if (paragraphProperties.BeforeBreak == BeforeBreak.page)
		{
			m_writer.WriteAttributeString("fo", "break-before", null, "page");
		}
		else if (paragraphProperties.BeforeBreak == BeforeBreak.column)
		{
			m_writer.WriteAttributeString("fo", "break-before", null, "column");
		}
		else
		{
			m_writer.WriteAttributeString("fo", "break-before", null, "auto");
		}
		if (paragraphProperties.HasKey(2, paragraphProperties.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-top", null, paragraphProperties.MarginTop + "in");
		}
		if (paragraphProperties.HasKey(3, paragraphProperties.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-bottom", null, paragraphProperties.MarginBottom + "in");
		}
		if (paragraphProperties.HasKey(0, paragraphProperties.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-left", null, paragraphProperties.MarginBottom + "in");
		}
		if (paragraphProperties.HasKey(1, paragraphProperties.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-right", null, paragraphProperties.MarginRight + "in");
		}
		if (paragraphProperties.HasKey(0, paragraphProperties.borderFlags) && paragraphProperties.Border != null)
		{
			m_writer.WriteAttributeString("fo", "border", null, paragraphProperties.Border.LineWidth + "in " + paragraphProperties.Border.LineStyle.ToString() + " " + HexConverter(paragraphProperties.Border.LineColor));
			m_writer.WriteAttributeString("fo", "padding-left", null, paragraphProperties.PaddingLeft + "in");
			m_writer.WriteAttributeString("fo", "padding-right", null, paragraphProperties.PaddingRight + "in");
			m_writer.WriteAttributeString("fo", "padding-top", null, paragraphProperties.PaddingTop + "in");
			m_writer.WriteAttributeString("fo", "padding-bottom", null, paragraphProperties.PaddingBottom + "in");
		}
		if (paragraphProperties.HasKey(1, paragraphProperties.borderFlags) || paragraphProperties.HasKey(2, paragraphProperties.borderFlags) || paragraphProperties.HasKey(3, paragraphProperties.borderFlags) || paragraphProperties.HasKey(4, paragraphProperties.borderFlags))
		{
			if (paragraphProperties.BorderLeft != null)
			{
				m_writer.WriteAttributeString("fo", "border-left", null, paragraphProperties.BorderLeft.LineWidth + "in " + paragraphProperties.BorderLeft.LineStyle.ToString() + " " + HexConverter(paragraphProperties.BorderLeft.LineColor));
				m_writer.WriteAttributeString("fo", "padding-left", null, paragraphProperties.PaddingLeft + "in");
			}
			if (paragraphProperties.BorderRight != null)
			{
				m_writer.WriteAttributeString("fo", "border-right", null, paragraphProperties.BorderRight.LineWidth + "in " + paragraphProperties.BorderRight.LineStyle.ToString() + " " + HexConverter(paragraphProperties.BorderRight.LineColor));
				m_writer.WriteAttributeString("fo", "padding-right", null, paragraphProperties.PaddingRight + "in");
			}
			if (paragraphProperties.BorderTop != null)
			{
				m_writer.WriteAttributeString("fo", "border-top", null, paragraphProperties.BorderTop.LineWidth + "in " + paragraphProperties.BorderTop.LineStyle.ToString() + " " + HexConverter(paragraphProperties.BorderTop.LineColor));
				m_writer.WriteAttributeString("fo", "padding-top", null, paragraphProperties.PaddingTop + "in");
			}
			if (paragraphProperties.BorderBottom != null)
			{
				m_writer.WriteAttributeString("fo", "border-bottom", null, paragraphProperties.BorderBottom.LineWidth + "in " + paragraphProperties.BorderBottom.LineStyle.ToString() + " " + HexConverter(paragraphProperties.BorderBottom.LineColor));
				m_writer.WriteAttributeString("fo", "padding-bottom", null, paragraphProperties.PaddingBottom + "in");
			}
		}
		if (paragraphProperties.HasKey(21, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-align", null, paragraphProperties.TextAlign.ToString().ToLower());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "text-align", null, "start");
		}
		if (paragraphProperties.HasKey(6, paragraphProperties.m_CommonstyleFlags))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, paragraphProperties.BackgroundColor);
		}
		else
		{
			m_writer.WriteAttributeString("fo", "background-color", null, "transparent");
		}
		if (paragraphProperties.HasKey(19, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-indent", null, paragraphProperties.TextIndent + "in");
		}
		if (paragraphProperties.HasKey(10, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("style", "line-height-at-least", null, paragraphProperties.LineHeightAtLeast + "in");
		}
		if (paragraphProperties.HasKey(28, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "line-height", null, paragraphProperties.LineHeight + "in");
		}
		else if (paragraphProperties.HasKey(9, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "line-height", null, paragraphProperties.LineSpacing + "%");
		}
		else
		{
			m_writer.WriteAttributeString("fo", "line-height", null, "100%");
		}
		if (paragraphProperties.HasKey(0, paragraphProperties.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-top", null, paragraphProperties.BeforeSpacing + "in");
		}
		if (paragraphProperties.HasKey(1, paragraphProperties.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-bottom", null, paragraphProperties.AfterSpacing + "in");
		}
		if (paragraphProperties.HasKey(2, paragraphProperties.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-left", null, paragraphProperties.LeftIndent + "in");
		}
		if (paragraphProperties.HasKey(3, paragraphProperties.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-right", null, paragraphProperties.RightIndent + "in");
		}
		if (paragraphProperties.WritingMode == WritingMode.RLTB)
		{
			m_writer.WriteAttributeString("style", "writing-mode", null, "rl-tb");
		}
		if (paragraphProperties != null && paragraphProperties.TabStops != null && paragraphProperties.TabStops.Count > 0)
		{
			m_writer.WriteStartElement("style", "tab-stops", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			for (int i = 0; i < paragraphProperties.TabStops.Count; i++)
			{
				TabStops tabStops = paragraphProperties.TabStops[i];
				m_writer.WriteStartElement("style", "tab-stop", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				m_writer.WriteAttributeString("style", "type", null, tabStops.TextAlignType.ToString());
				m_writer.WriteAttributeString("style", "position", null, tabStops.TextPosition + "in");
				if (tabStops.TabStopLeader != 0)
				{
					m_writer.WriteAttributeString("style", "leader-char", null, (tabStops.TabStopLeader == TabStopLeader.Dotted) ? "." : "");
				}
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
		}
		if (paragraphProperties.HasKey(18, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "widows", null, paragraphProperties.Windows.ToString());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "widows", null, "2");
		}
		if (paragraphProperties.HasKey(27, paragraphProperties.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "orphans", null, paragraphProperties.Orphans.ToString());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "orphans", null, "2");
		}
		m_writer.WriteAttributeString("style", "tab-stop-distance", null, "0.5in");
		m_writer.WriteEndElement();
	}

	private void SerializeDefaultTextProperties(TextProperties textProperties)
	{
		if (textProperties == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "text-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (textProperties.HasKey(16, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "font-name", null, textProperties.FontName);
		}
		else
		{
			m_writer.WriteAttributeString("style", "font-name", null, "Calibri");
			m_writer.WriteAttributeString("style", "font-name-asian", null, "Calibri");
			m_writer.WriteAttributeString("style", "font-name-complex", null, "Times New Roman");
		}
		if (textProperties.HasKey(17, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-size", null, textProperties.FontSize + "pt");
			m_writer.WriteAttributeString("style", "font-size-asian", null, textProperties.FontSize + "pt");
			m_writer.WriteAttributeString("style", "font-size-complex", null, textProperties.FontSize + "pt");
		}
		else
		{
			m_writer.WriteAttributeString("fo", "font-size", null, "10pt");
			m_writer.WriteAttributeString("style", "font-size-asian", null, "10pt");
			m_writer.WriteAttributeString("style", "font-size-complex", null, "10pt");
		}
		if (textProperties.HasKey(23, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "color", null, HexConverter(textProperties.Color));
		}
		if (textProperties.HasKey(22, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-weight", null, textProperties.FontWeight.ToString());
			m_writer.WriteAttributeString("style", "font-weight-asian", null, textProperties.FontWeightAsian.ToString());
			m_writer.WriteAttributeString("style", "font-style-asian", null, textProperties.FontStyleAsian.ToString());
		}
		else
		{
			m_writer.WriteAttributeString("fo", "font-weight", null, "normal");
			m_writer.WriteAttributeString("style", "font-weight-asian", null, "normal");
			m_writer.WriteAttributeString("style", "font-weight-complex", null, "normal");
		}
		if (textProperties.FontStyle == ODFFontStyle.italic)
		{
			m_writer.WriteAttributeString("fo", "font-style", null, textProperties.FontStyle.ToString());
		}
		if (textProperties.HasKey(5, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-underline-type", null, textProperties.TextUnderlineType.ToString().ToLower());
		}
		if (textProperties.HasKey(6, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-underline-syle", null, textProperties.TextUnderlineStyle.ToString());
			m_writer.WriteAttributeString("style", "text-underline-color", null, textProperties.TextUnderlineColor);
			m_writer.WriteAttributeString("style", "text-underline-type", null, "single");
		}
		if (textProperties.HasKey(0, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "font-relief", null, textProperties.FontRelief.ToString());
		}
		if (textProperties.HasKey(20, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, HexConverter(textProperties.BackgroundColor));
		}
		else
		{
			m_writer.WriteAttributeString("fo", "background-color", null, "transparent");
		}
		if (textProperties.HasKey(0, textProperties.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "letter-kerning", null, textProperties.LetterKerning.ToString().ToLower());
		}
		if (textProperties.HasKey(9, textProperties.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "text-line-through-type", null, textProperties.LinethroughType.ToString());
			if (textProperties.LinethroughType != 0)
			{
				m_writer.WriteAttributeString("style", "text-line-through-style", null, textProperties.LinethroughStyle.ToString());
				m_writer.WriteAttributeString("style", "text-line-through-color", null, textProperties.LinethroughColor);
			}
		}
		if (textProperties.HasKey(3, textProperties.m_textFlag2) && !textProperties.HasKey(28, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-transform", null, textProperties.TextTransform.ToString());
		}
		if (textProperties.HasKey(9, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-scale", null, textProperties.TextScale + "%");
		}
		if (textProperties.HasKey(3, textProperties.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "text-outline", null, textProperties.TextOutline.ToString().ToLower());
		}
		if (textProperties.HasKey(27, textProperties.m_textFlag1) && !textProperties.IsTextDisplay)
		{
			m_writer.WriteAttributeString("text", "display", null, "none");
		}
		if (textProperties.HasKey(21, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-position", null, textProperties.TextPosition.ToString());
		}
		if (textProperties.HasKey(28, textProperties.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-variant", null, "small-caps");
		}
		m_writer.WriteEndElement();
	}

	internal void SerializeTables(List<OTable> tables)
	{
		for (int i = 0; i < tables.Count; i++)
		{
			OTable oTable = tables[i];
			m_writer.WriteStartElement("table", "table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
			m_writer.WriteAttributeString("table", "style-name", null, oTable.StyleName);
			int count = oTable.Columns.Count;
			m_writer.WriteStartElement("table", "table-columns", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
			for (int j = 0; j < count; j++)
			{
				OTableColumn oTableColumn = oTable.Columns[j];
				m_writer.WriteStartElement("table", "table-column", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
				_ = 1;
				if (!string.IsNullOrEmpty(oTableColumn.StyleName))
				{
					m_writer.WriteAttributeString("table", "style-name", null, oTableColumn.StyleName);
				}
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			int count2 = oTable.Rows.Count;
			for (int k = 0; k < count2; k++)
			{
				OTableRow oTableRow = oTable.Rows[k];
				m_writer.WriteStartElement("table", "table-row", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
				if (!string.IsNullOrEmpty(oTableRow.StyleName))
				{
					m_writer.WriteAttributeString("table", "style-name", null, oTableRow.StyleName);
				}
				if (!string.IsNullOrEmpty(oTableRow.DefaultCellStyleName))
				{
					m_writer.WriteAttributeString("table", "default-cell-style-name", null, oTableRow.DefaultCellStyleName);
				}
				if (oTableRow.IsCollapsed)
				{
					m_writer.WriteAttributeString("table", "visibility", null, "collapse");
				}
				int count3 = oTableRow.Cells.Count;
				OTableCell oTableCell = null;
				for (int l = 0; l < count3; l++)
				{
					oTableCell = oTableRow.Cells[l];
					m_writer.WriteStartElement("table", "table-cell", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
					if (!string.IsNullOrEmpty(oTableCell.StyleName))
					{
						m_writer.WriteAttributeString("table", "style-name", null, oTableCell.StyleName);
					}
					if (oTableCell.ColumnsSpanned > 1)
					{
						m_writer.WriteAttributeString("table", "number-columns-spanned", null, oTableCell.ColumnsSpanned.ToString());
					}
					if (!oTableCell.IsBlank)
					{
						OTextBodyItem oTextBodyItem = null;
						OParagraph oParagraph = null;
						Stack<int> listStack = null;
						string m_previousParaListStyleName = string.Empty;
						for (int m = 0; m < oTableCell.TextBodyIetm.Count; m++)
						{
							oTextBodyItem = oTableCell.TextBodyIetm[m];
							if (oTextBodyItem is OParagraph)
							{
								oParagraph = (OParagraph)oTextBodyItem;
								SerializeOParagraph(oParagraph, ref listStack, ref m_previousParaListStyleName);
							}
							if (oTextBodyItem is OTable)
							{
								OTable item = oTextBodyItem as OTable;
								List<OTable> list = new List<OTable>();
								list.Add(item);
								SerializeTables(list);
							}
						}
					}
					m_writer.WriteEndElement();
				}
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
		}
	}

	private void WriteCellType(OTableCell curCell)
	{
		switch (curCell.Type)
		{
		case CellValueType.Boolean:
			m_writer.WriteAttributeString("office", "boolean-value", null, curCell.BooleanValue.ToString());
			break;
		case CellValueType.Date:
		{
			string value = curCell.DateValue.ToString("yyyy-MM-ddTHH:mm:ss");
			m_writer.WriteAttributeString("office", "date-value", null, value);
			break;
		}
		case CellValueType.Float:
		case CellValueType.Percentage:
		case CellValueType.Currency:
			m_writer.WriteAttributeString("office", "value", null, (curCell.Value != null) ? curCell.Value.ToString() : string.Empty);
			break;
		case CellValueType.Time:
			m_writer.WriteAttributeString("office", "time-value", null, ToReadableString(curCell.TimeValue));
			break;
		}
	}

	private void WriteRepeatedCells(OTableRow row, OTableCell cell, int colsRepeated)
	{
		if (cell != null)
		{
			m_writer.WriteStartElement("table", "table-cell", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
			if (cell.ColumnsRepeated != 0 && cell.ColumnsRepeated > 1)
			{
				m_writer.WriteAttributeString("table", "number-columns-repeated", null, cell.ColumnsRepeated.ToString());
			}
			else
			{
				m_writer.WriteAttributeString("table", "number-columns-repeated", null, colsRepeated.ToString());
			}
			if (!string.IsNullOrEmpty(row.DefaultCellStyleName))
			{
				m_writer.WriteAttributeString("table", "style-name", null, row.DefaultCellStyleName);
			}
			m_writer.WriteEndElement();
		}
	}

	private void SerializeOParagraph(OParagraph paragraph, ref Stack<int> listStack, ref string m_previousParaListStyleName)
	{
		SerializeList(paragraph, ref listStack, ref m_previousParaListStyleName);
		if (!string.IsNullOrEmpty(paragraph.TocMark) && paragraph.TocMark == m_document.TOCStyles[0].Name)
		{
			m_writer.WriteStartElement("text", "table-of-content", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteAttributeString("text", "name", null, "_TOC0");
			SerializeTableOfContentSource();
			m_writer.WriteStartElement("text", "index-body", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		}
		if (paragraph.Header != null && paragraph.Header.StyleName != null)
		{
			m_writer.WriteStartElement("text", "h", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteAttributeString("text", "style-name", null, paragraph.StyleName);
		}
		else
		{
			m_writer.WriteStartElement("text", "p", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			if (paragraph.StyleName != null)
			{
				m_writer.WriteAttributeString("text", "style-name", null, paragraph.StyleName);
			}
		}
		List<OParagraphItem> oParagraphItemCollection = paragraph.OParagraphItemCollection;
		int count = oParagraphItemCollection.Count;
		bool flag = false;
		if (count > 0)
		{
			for (int i = 0; i < oParagraphItemCollection.Count; i++)
			{
				OParagraphItem oParagraphItem = oParagraphItemCollection[i];
				if (oParagraphItem is OBreak)
				{
					OBreakType breakType = (oParagraphItem as OBreak).BreakType;
					if (breakType == OBreakType.LineBreak && oParagraphItem.ParagraphProperties.LineBreak)
					{
						m_writer.WriteStartElement("text", "span", null);
						m_writer.WriteStartElement("text", "line-break", null);
						m_writer.WriteEndElement();
						m_writer.WriteEndElement();
					}
					else if (breakType == OBreakType.ColumnBreak || breakType == OBreakType.PageBreak)
					{
						flag = true;
					}
				}
				if (oParagraphItem is OTextRange)
				{
					if (oParagraphItem.Text == "\t")
					{
						m_writer.WriteStartElement("text", "tab", null);
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteStartElement("text", "span", null);
						if (oParagraphItem.StyleName != null)
						{
							m_writer.WriteAttributeString("text", "style-name", null, oParagraphItem.StyleName);
						}
						m_writer.WriteString(oParagraphItem.Text);
						m_writer.WriteEndElement();
					}
					else
					{
						m_writer.WriteString(oParagraphItem.Text);
					}
				}
				if (oParagraphItem is OPicture)
				{
					if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteStartElement("text", "span", null);
						if (oParagraphItem.StyleName != null)
						{
							m_writer.WriteAttributeString("text", "style-name", null, oParagraphItem.StyleName);
						}
					}
					SerializePicture(oParagraphItem as OPicture);
					if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteEndElement();
					}
					if (flag)
					{
						m_writer.WriteElementString("text", "soft-page-break");
					}
				}
				if (oParagraphItem is OMergeField)
				{
					if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteStartElement("text", "span", null);
						if (oParagraphItem.StyleName != null)
						{
							m_writer.WriteAttributeString("text", "style-name", null, oParagraphItem.StyleName);
						}
					}
					SerializeMergeField(oParagraphItem as OMergeField);
					m_writer.WriteEndElement();
				}
				if (oParagraphItem is OField)
				{
					if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteStartElement("text", "span", null);
						if (oParagraphItem.StyleName != null)
						{
							m_writer.WriteAttributeString("text", "style-name", null, oParagraphItem.StyleName);
						}
					}
					if ((oParagraphItem as OField).OFieldType == OFieldType.FieldDate)
					{
						m_writer.WriteStartElement("text", "span", null);
						m_writer.WriteString((oParagraphItem as OField).Text);
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldHyperlink)
					{
						SerializeHyperlink(oParagraphItem as OField);
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldNumPages)
					{
						m_writer.WriteStartElement("text", "page-count", null);
						string numberFormat = GetNumberFormat((oParagraphItem as OField).PageNumberFormat);
						if (!string.IsNullOrEmpty(numberFormat))
						{
							m_writer.WriteAttributeString("style", "num-format", null, numberFormat);
						}
						m_writer.WriteString((oParagraphItem as OField).Text);
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldPage)
					{
						m_writer.WriteStartElement("text", "page-number", null);
						string numberFormat2 = GetNumberFormat((oParagraphItem as OField).PageNumberFormat);
						if (!string.IsNullOrEmpty(numberFormat2))
						{
							m_writer.WriteAttributeString("style", "num-format", null, numberFormat2);
						}
						m_writer.WriteString((oParagraphItem as OField).Text);
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldAuthor)
					{
						m_writer.WriteStartElement("text", "initial-creator", null);
						m_writer.WriteAttributeString("style", "fixed", null, "false");
						m_writer.WriteString((oParagraphItem as OField).Text);
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldTitle)
					{
						m_writer.WriteStartElement("text", "title", null);
						m_writer.WriteAttributeString("style", "fixed", null, "false");
						if (!string.IsNullOrEmpty((oParagraphItem as OField).Text))
						{
							m_writer.WriteString((oParagraphItem as OField).Text);
						}
						m_writer.WriteEndElement();
					}
					else if ((oParagraphItem as OField).OFieldType == OFieldType.FieldTOC || (oParagraphItem as OField).OFieldType == OFieldType.FieldPageRef)
					{
						m_writer.WriteStartElement("text", "tab", null);
						m_writer.WriteEndElement();
						m_writer.WriteString((oParagraphItem as OField).Text);
						if (paragraph.TocMark == m_document.TOCStyles[m_document.TOCStyles.Count - 1].Name)
						{
							m_writer.WriteEndElement();
							m_writer.WriteEndElement();
						}
					}
					else
					{
						m_writer.WriteStartElement("text", "span", null);
						m_writer.WriteString((oParagraphItem as OField).Text);
						m_writer.WriteEndElement();
					}
					if ((oParagraphItem.TextProperties != null && oParagraphItem.TextProperties.CharStyleName != null) || oParagraphItem.StyleName != null)
					{
						m_writer.WriteEndElement();
					}
				}
				if (oParagraphItem is OBookmarkStart)
				{
					m_writer.WriteStartElement("text", "bookmark-start", null);
					m_writer.WriteAttributeString("text", "name", null, (oParagraphItem as OBookmarkStart).Name);
					m_writer.WriteEndElement();
				}
				if (oParagraphItem is OBookmarkEnd)
				{
					m_writer.WriteStartElement("text", "bookmark-end", null);
					m_writer.WriteAttributeString("text", "name", null, (oParagraphItem as OBookmarkEnd).Name);
					m_writer.WriteEndElement();
				}
			}
		}
		m_writer.WriteEndElement();
	}

	private void SerializeParagraph(OParagraph para)
	{
		if (para == null)
		{
			return;
		}
		m_writer.WriteStartElement("text", "p", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		if (para.StyleName != null)
		{
			m_writer.WriteAttributeString("text", "style-name", null, para.StyleName);
		}
		if (para.OParagraphItemCollection.Count > 0)
		{
			for (int i = 0; i < para.OParagraphItemCollection.Count; i++)
			{
				OParagraphItem oParagraphItem = para.OParagraphItemCollection[i];
				if (oParagraphItem != null)
				{
					m_writer.WriteString(oParagraphItem.Text);
				}
			}
		}
		m_writer.WriteEndElement();
	}

	internal void SerializeExcelBody(List<OTable> tables)
	{
		m_writer.WriteStartElement("office", "spreadsheet", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		SerializeCalculationSettings();
		SerializeTables(tables);
		m_writer.WriteEndElement();
	}

	internal MemoryStream SerializeStyleStart()
	{
		MemoryStream memoryStream = new MemoryStream();
		m_writer = CreateWriter(memoryStream);
		m_writer.WriteStartElement("office", "document-styles", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "table", null, "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
		m_writer.WriteAttributeString("xmlns", "office", null, "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		m_writer.WriteAttributeString("xmlns", "style", null, "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("xmlns", "draw", null, "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
		m_writer.WriteAttributeString("xmlns", "fo", null, "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
		m_writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
		m_writer.WriteAttributeString("xmlns", "number", null, "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
		m_writer.WriteAttributeString("xmlns", "svg", null, "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "of", null, "urn:oasis:names:tc:opendocument:xmlns:of:1.2");
		m_writer.WriteAttributeString("xmlns", "anim", null, "urn:oasis:names:tc:opendocument:xmlns:animation:1.0");
		m_writer.WriteAttributeString("xmlns", "chart", null, "urn:oasis:names:tc:opendocument:xmlns:chart:1.0");
		m_writer.WriteAttributeString("xmlns", "onfig", null, "urn:oasis:names:tc:opendocument:xmlns:config:1.0");
		m_writer.WriteAttributeString("xmlns", "db", null, "urn:oasis:names:tc:opendocument:xmlns:database:1.0");
		m_writer.WriteAttributeString("xmlns", "dr3d", null, "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0");
		m_writer.WriteAttributeString("xmlns", "form", null, "urn:oasis:names:tc:opendocument:xmlns:form:1.0");
		m_writer.WriteAttributeString("xmlns", "meta", null, "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
		m_writer.WriteAttributeString("xmlns", "presentation", null, "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0");
		m_writer.WriteAttributeString("xmlns", "script", null, "urn:oasis:names:tc:opendocument:xmlns:script:1.0");
		m_writer.WriteAttributeString("xmlns", "smil", null, "urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0");
		m_writer.WriteAttributeString("xmlns", "text", null, "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
		m_writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
		return memoryStream;
	}

	internal void SerializeStylesEnd(MemoryStream stream)
	{
		m_writer.WriteEndElement();
		m_writer.Flush();
		m_archieve.AddItem("styles.xml", stream, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
	}

	public static string ToReadableString(TimeSpan span)
	{
		string text = string.Format("{0}{1}{2}", (span.TotalHours > 0.0) ? string.Format("{00}{1}", span.TotalHours, "H") : string.Empty, (span.TotalMinutes > 0.0) ? string.Format("{00}{1}", span.Minutes, "M") : string.Empty, (span.TotalSeconds > 0.0) ? string.Format("{00}{1}", span.Seconds, "S") : string.Empty);
		return "PT" + text;
	}

	internal void SerializeFontFaceDecls(List<FontFace> fonts)
	{
		if (m_writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (fonts != null)
		{
			m_writer.WriteStartElement("office", "font-face-decls", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
			for (int i = 0; i < fonts.Count; i++)
			{
				SerializeFontface(fonts[i]);
			}
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeFontface(FontFace font)
	{
		if (m_writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_writer.WriteStartElement("style", "font-face", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "name", null, font.Name);
		m_writer.WriteAttributeString("svg", "font-family", null, font.Name.ToString());
		m_writer.WriteAttributeString("style", "font-family-generic", null, font.FontFamilyGeneric.ToString());
		m_writer.WriteAttributeString("style", "font-pitch", null, font.FontPitch.ToString());
		m_writer.WriteEndElement();
	}

	internal void SerializeDataStyles(ODFStyleCollection styles)
	{
		SerializeCommonStyles(styles);
	}

	internal void SerializeDataStylesStart()
	{
		m_writer.WriteStartElement("office", "styles", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
	}

	internal void SerializeGeneralStyle(NumberStyle style)
	{
		if (style.Number.nFormatFlags != 0)
		{
			m_writer.WriteStartElement("number", "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("number", "min-integer-digits", null, style.Number.MinIntegerDigits.ToString());
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeNumberStyle(DataStyle nFormat)
	{
		if (nFormat is NumberStyle)
		{
			NumberStyle numberStyle = nFormat as NumberStyle;
			m_writer.WriteStartElement("number", "number-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("style", "name", null, numberStyle.Name);
			if (numberStyle.ScientificNumber.nFormatFlags != 0)
			{
				m_writer.WriteStartElement("number", "scientific-number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
				m_writer.WriteAttributeString("number", "min-integer-digits", null, numberStyle.ScientificNumber.MinIntegerDigits.ToString());
				m_writer.WriteAttributeString("number", "decimal-places", null, numberStyle.ScientificNumber.DecimalPlaces.ToString());
				m_writer.WriteAttributeString("number", "min-exponent-digits", null, numberStyle.ScientificNumber.MinExponentDigits.ToString());
				m_writer.WriteAttributeString("number", "grouping", null, numberStyle.ScientificNumber.Grouping.ToString().ToLower());
				m_writer.WriteEndElement();
			}
			else if (numberStyle.Number.nFormatFlags != 0)
			{
				m_writer.WriteStartElement("number", "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
				m_writer.WriteAttributeString("number", "min-integer-digits", null, numberStyle.Number.MinIntegerDigits.ToString());
				m_writer.WriteAttributeString("number", "decimal-places", null, numberStyle.Number.DecimalPlaces.ToString());
				m_writer.WriteAttributeString("number", "grouping", null, numberStyle.Number.Grouping.ToString().ToLower());
				m_writer.WriteEndElement();
			}
			else if (numberStyle.Fraction.nFormatFlags != 0)
			{
				m_writer.WriteStartElement("number", "fraction", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
				m_writer.WriteAttributeString("number", "min-integer-digits", null, numberStyle.Fraction.MinIntegerDigits.ToString());
				m_writer.WriteAttributeString("number", "min-numerator-digits", null, numberStyle.Fraction.MinNumeratorDigits.ToString());
				if (numberStyle.Fraction.DenominatorValue > 0)
				{
					m_writer.WriteAttributeString("number", "denominator-value", null, numberStyle.Fraction.DenominatorValue.ToString().ToLower());
				}
				else
				{
					m_writer.WriteAttributeString("number", "min-denominator-digits", null, numberStyle.Fraction.MinDenominatorDigits.ToString().ToLower());
				}
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
		}
		else if (nFormat is PercentageStyle)
		{
			PercentageStyle percentageStyle = nFormat as PercentageStyle;
			m_writer.WriteStartElement("number", "percentage-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("style", "name", null, percentageStyle.Name);
			m_writer.WriteStartElement("number", "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("number", "min-integer-digits", null, percentageStyle.Number.MinIntegerDigits.ToString());
			m_writer.WriteAttributeString("number", "decimal-places", null, percentageStyle.Number.DecimalPlaces.ToString());
			m_writer.WriteAttributeString("number", "grouping", null, percentageStyle.Number.Grouping.ToString().ToLower());
			m_writer.WriteEndElement();
			m_writer.WriteEndElement();
		}
		else if (nFormat is CurrencyStyle)
		{
			CurrencyStyle currencyStyle = nFormat as CurrencyStyle;
			m_writer.WriteStartElement("number", "currency-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("style", "name", null, currencyStyle.Name);
			m_writer.WriteStartElement("number", "currency-symbol", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteString(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
			m_writer.WriteEndElement();
			SerializeNumberToken(currencyStyle);
			if (currencyStyle.HasSections)
			{
				for (int i = 0; i < currencyStyle.Map.Count; i++)
				{
					m_writer.WriteStartElement("style", "map", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
					m_writer.WriteAttributeString("style", "condition", null, currencyStyle.Map[i].Condition);
					m_writer.WriteAttributeString("style", "apply-style-name", null, currencyStyle.Map[i].ApplyStyleName);
					m_writer.WriteEndElement();
				}
			}
			m_writer.WriteEndElement();
		}
		else if (nFormat is TextStyle)
		{
			TextStyle textStyle = nFormat as TextStyle;
			m_writer.WriteStartElement("number", "text-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("style", "name", null, textStyle.Name);
			if (textStyle.TextContent)
			{
				m_writer.WriteElementString("number", "text-content", null, string.Empty);
			}
			if (textStyle.HasSections)
			{
				for (int j = 0; j < textStyle.Map.Count; j++)
				{
					m_writer.WriteStartElement("style", "map", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
					m_writer.WriteAttributeString("style", "condition", null, textStyle.Map[j].Condition);
					m_writer.WriteAttributeString("style", "apply-style-name", null, textStyle.Map[j].ApplyStyleName);
					m_writer.WriteEndElement();
				}
			}
			m_writer.WriteEndElement();
		}
		else if (nFormat is DateStyle)
		{
			DateStyle dateStyle = nFormat as DateStyle;
			m_writer.WriteStartElement("number", "date-style", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
			m_writer.WriteAttributeString("style", "name", null, dateStyle.Name);
			SerializeDateToken();
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeNumberToken(CurrencyStyle style)
	{
		m_writer.WriteStartElement("number", "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
		m_writer.WriteAttributeString("number", "min-integer-digits", null, style.Number.MinIntegerDigits.ToString());
		m_writer.WriteAttributeString("number", "decimal-places", null, style.Number.DecimalPlaces.ToString());
		m_writer.WriteAttributeString("number", "grouping", null, style.Number.Grouping.ToString().ToLower());
		m_writer.WriteEndElement();
	}

	internal void SerializeDateToken()
	{
	}

	internal void SerializeCommonStyles(ODFStyleCollection styles)
	{
		if (m_writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		SerializeODFStyles(styles);
	}

	internal void SerializeODFStyles(ODFStyleCollection ODFStyles)
	{
		m_odfStyles = ODFStyles;
		int count = ODFStyles.DictStyles.Values.Count;
		ODFStyle[] array = new ODFStyle[count];
		ODFStyles.DictStyles.Values.CopyTo(array, 0);
		for (int i = 0; i < count; i++)
		{
			ODFStyle oDFStyle = array[i];
			m_writer.WriteStartElement("style", "style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			m_writer.WriteAttributeString("style", "name", null, oDFStyle.Name.Replace('_', '-'));
			m_writer.WriteAttributeString("style", "family", null, oDFStyle.Family.ToString().ToLower().Replace('_', '-'));
			switch (oDFStyle.Family)
			{
			case ODFFontFamily.Paragraph:
				if (!string.IsNullOrEmpty(oDFStyle.ParentStyleName))
				{
					m_writer.WriteAttributeString("style", "parent-style-name", null, oDFStyle.ParentStyleName);
				}
				if (oDFStyle.MasterPageName != null)
				{
					m_writer.WriteAttributeString("style", "master-page-name", null, oDFStyle.MasterPageName);
				}
				SerializeParagraphProperties(oDFStyle.ParagraphProperties);
				break;
			case ODFFontFamily.Table_Cell:
				if (!string.IsNullOrEmpty(oDFStyle.ParentStyleName))
				{
					m_writer.WriteAttributeString("style", "parent-style-name", null, oDFStyle.ParentStyleName);
				}
				m_writer.WriteAttributeString("style", "data-style-name", null, oDFStyle.DataStyleName);
				SerializeTableCellProperties(oDFStyle.TableCellProperties);
				SerializeParagraphProperties(oDFStyle.ParagraphProperties);
				break;
			case ODFFontFamily.Table:
				m_writer.WriteAttributeString("style", "master-page-name", null, oDFStyle.MasterPageName);
				SerializeTableProperties(oDFStyle.TableProperties);
				break;
			case ODFFontFamily.Table_Column:
				SerializeColumnProprties(oDFStyle.TableColumnProperties);
				break;
			case ODFFontFamily.Table_Row:
				SerializeRowProprties(oDFStyle.TableRowProperties);
				break;
			case ODFFontFamily.Section:
				SerializeSectionProperties(oDFStyle.ODFSectionProperties);
				break;
			}
			if (oDFStyle.Textproperties != null && oDFStyle.Textproperties.CharStyleName != null)
			{
				m_writer.WriteAttributeString("style", "parent-style-name", null, oDFStyle.Textproperties.CharStyleName);
			}
			SerializeTextProperties(oDFStyle.Textproperties);
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeTableDefaultStyle()
	{
		m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "family", null, "table");
		m_writer.WriteStartElement("style", "table-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("fo", "margin-left", null, "0in");
		m_writer.WriteAttributeString("table", "border-model", null, "collapsing");
		m_writer.WriteAttributeString("style", "writing-mode", null, "lr-tb");
		m_writer.WriteAttributeString("table", "align", null, "left");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "family", null, "table-column");
		m_writer.WriteStartElement("style", "table-column-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "use-optimal-column-width", null, "true");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "family", null, "table-row");
		m_writer.WriteStartElement("style", "table-row-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "min-row-height", null, "0in");
		m_writer.WriteAttributeString("style", "use-optimal-column-height", null, "true");
		m_writer.WriteAttributeString("fo", "keep-together", null, "auto");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
		m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "family", null, "table-cell");
		m_writer.WriteStartElement("style", "table-cell-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("fo", "background-color", null, "transparent");
		m_writer.WriteAttributeString("style", "glyph-orientation-vertical", null, "auto");
		m_writer.WriteAttributeString("fo", "vertical-align", null, "top");
		m_writer.WriteAttributeString("fo", "wrap-option", null, "wrap");
		m_writer.WriteEndElement();
		m_writer.WriteEndElement();
	}

	internal void SerializeDefaultGraphicStyle()
	{
		m_writer.WriteStartElement("style", "default-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("style", "family", null, "graphic");
		m_writer.WriteAttributeString("draw", "fill", null, "solid");
		m_writer.WriteAttributeString("draw", "fill-color", null, "#5b9bd5");
		m_writer.WriteAttributeString("draw", "opacity", null, "100%");
		m_writer.WriteAttributeString("draw", "stroke", null, "solid");
		m_writer.WriteAttributeString("draw", "stroke-width", null, "0.01389in");
		m_writer.WriteAttributeString("svg", "stroke-color", null, "#41719c");
		m_writer.WriteAttributeString("draw", "stoke-opacity", null, "100%");
		m_writer.WriteAttributeString("draw", "stroke-linejoin", null, "miter");
		m_writer.WriteAttributeString("draw", "stroke-linecap", null, "butt");
		m_writer.WriteEndElement();
	}

	private void SerializeTableProperties(OTableProperties tableProp)
	{
		m_writer.WriteStartElement("style", "table-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (tableProp != null)
		{
			m_writer.WriteAttributeString("style", "width", null, tableProp.TableWidth.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("fo", "margin-left", null, tableProp.MarginLeft + "in");
			m_writer.WriteAttributeString("table", "align", null, "left");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeColumnProprties(OTableColumnProperties tableColumnProp)
	{
		m_writer.WriteStartElement("style", "table-column-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (tableColumnProp != null)
		{
			m_writer.WriteAttributeString("style", "column-width", null, tableColumnProp.ColumnWidth.ToString(CultureInfo.InvariantCulture) + "in");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeRowProprties(OTableRowProperties tableRowProp)
	{
		m_writer.WriteStartElement("style", "table-row-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (tableRowProp != null)
		{
			if (tableRowProp.RowHeight > 0.0)
			{
				m_writer.WriteAttributeString("style", "row-height", null, tableRowProp.RowHeight + "in");
			}
			m_writer.WriteAttributeString("style", "min-row-height", null, tableRowProp.RowHeight + "in");
			if (tableRowProp.IsBreakAcrossPages)
			{
				m_writer.WriteAttributeString("fo", "keep-together", null, "always");
			}
			if (tableRowProp.IsHeaderRow)
			{
				m_writer.WriteAttributeString("style", "use-optimal-row-height", null, "false");
			}
		}
		m_writer.WriteEndElement();
	}

	private void SerializeSectionProperties(SectionProperties sectionProps)
	{
		if (sectionProps != null)
		{
			m_writer.WriteStartElement("style", "section-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			m_writer.WriteAttributeString("fo", "margin-left", null, sectionProps.MarginLeft + "in");
			m_writer.WriteAttributeString("fo", "margin-right", null, sectionProps.MarginRight + "in");
			m_writer.WriteAttributeString("style", "writing-mode", null, "lr-tb");
			m_writer.WriteEndElement();
		}
	}

	private void SerializeParagraphProperties(ODFParagraphProperties paraProp)
	{
		if (paraProp == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "paragraph-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (paraProp.HasKey(9, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "keep-together", null, paraProp.KeepTogether.ToString());
		}
		if (paraProp.HasKey(3, paraProp.m_CommonstyleFlags))
		{
			m_writer.WriteAttributeString("fo", "keep-with-next", null, paraProp.KeepWithNext.ToString());
		}
		if (paraProp.BeforeBreak == BeforeBreak.page)
		{
			m_writer.WriteAttributeString("fo", "break-before", null, "page");
		}
		if (paraProp.BeforeBreak == BeforeBreak.column)
		{
			m_writer.WriteAttributeString("fo", "break-before", null, "column");
		}
		if (paraProp.HasKey(2, paraProp.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-top", null, paraProp.MarginTop + "in");
		}
		if (paraProp.HasKey(3, paraProp.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-bottom", null, paraProp.MarginBottom + "in");
		}
		if (paraProp.HasKey(0, paraProp.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-left", null, paraProp.MarginBottom + "in");
		}
		if (paraProp.HasKey(1, paraProp.m_marginFlag))
		{
			m_writer.WriteAttributeString("fo", "margin-right", null, paraProp.MarginRight + "in");
		}
		if (paraProp.HasKey(0, paraProp.borderFlags) && paraProp.Border != null)
		{
			m_writer.WriteAttributeString("fo", "border", null, paraProp.Border.LineWidth + "in " + paraProp.Border.LineStyle.ToString() + " " + HexConverter(paraProp.Border.LineColor));
			m_writer.WriteAttributeString("fo", "padding-left", null, paraProp.PaddingLeft + "in");
			m_writer.WriteAttributeString("fo", "padding-right", null, paraProp.PaddingRight + "in");
			m_writer.WriteAttributeString("fo", "padding-top", null, paraProp.PaddingTop + "in");
			m_writer.WriteAttributeString("fo", "padding-bottom", null, paraProp.PaddingBottom + "in");
		}
		if (paraProp.HasKey(1, paraProp.borderFlags) || paraProp.HasKey(2, paraProp.borderFlags) || paraProp.HasKey(3, paraProp.borderFlags) || paraProp.HasKey(4, paraProp.borderFlags))
		{
			if (paraProp.BorderLeft != null)
			{
				m_writer.WriteAttributeString("fo", "border-left", null, paraProp.BorderLeft.LineWidth + "in " + paraProp.BorderLeft.LineStyle.ToString() + " " + HexConverter(paraProp.BorderLeft.LineColor));
				m_writer.WriteAttributeString("fo", "padding-left", null, paraProp.PaddingLeft + "in");
			}
			if (paraProp.BorderRight != null)
			{
				m_writer.WriteAttributeString("fo", "border-right", null, paraProp.BorderRight.LineWidth + "in " + paraProp.BorderRight.LineStyle.ToString() + " " + HexConverter(paraProp.BorderRight.LineColor));
				m_writer.WriteAttributeString("fo", "padding-right", null, paraProp.PaddingRight + "in");
			}
			if (paraProp.BorderTop != null)
			{
				m_writer.WriteAttributeString("fo", "border-top", null, paraProp.BorderTop.LineWidth + "in " + paraProp.BorderTop.LineStyle.ToString() + " " + HexConverter(paraProp.BorderTop.LineColor));
				m_writer.WriteAttributeString("fo", "padding-top", null, paraProp.PaddingTop + "in");
			}
			if (paraProp.BorderBottom != null)
			{
				m_writer.WriteAttributeString("fo", "border-bottom", null, paraProp.BorderBottom.LineWidth + "in " + paraProp.BorderBottom.LineStyle.ToString() + " " + HexConverter(paraProp.BorderBottom.LineColor));
				m_writer.WriteAttributeString("fo", "padding-bottom", null, paraProp.PaddingBottom + "in");
			}
		}
		if (paraProp.HasKey(21, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-align", null, paraProp.TextAlign.ToString().ToLower());
		}
		if (paraProp.HasKey(6, paraProp.m_CommonstyleFlags))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, paraProp.BackgroundColor);
		}
		if (paraProp.HasKey(19, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-indent", null, paraProp.TextIndent + "in");
		}
		if (paraProp.HasKey(10, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("style", "line-height-at-least", null, paraProp.LineHeightAtLeast + "in");
		}
		if (paraProp.HasKey(28, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "line-height", null, paraProp.LineHeight + "in");
		}
		if (paraProp.HasKey(9, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "line-height", null, paraProp.LineSpacing + "%");
		}
		if (paraProp.HasKey(0, paraProp.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-top", null, paraProp.BeforeSpacing + "in");
		}
		if (paraProp.HasKey(1, paraProp.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-bottom", null, paraProp.AfterSpacing + "in");
		}
		if (paraProp.HasKey(2, paraProp.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-left", null, paraProp.LeftIndent + "in");
		}
		if (paraProp.HasKey(3, paraProp.m_styleFlag2))
		{
			m_writer.WriteAttributeString("fo", "margin-right", null, paraProp.RightIndent + "in");
		}
		if (paraProp.WritingMode == WritingMode.RLTB)
		{
			m_writer.WriteAttributeString("style", "writing-mode", null, "rl-tb");
		}
		if (paraProp != null && paraProp.TabStops != null && paraProp.TabStops.Count > 0)
		{
			m_writer.WriteStartElement("style", "tab-stops", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			for (int i = 0; i < paraProp.TabStops.Count; i++)
			{
				TabStops tabStops = paraProp.TabStops[i];
				m_writer.WriteStartElement("style", "tab-stop", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				m_writer.WriteAttributeString("style", "type", null, tabStops.TextAlignType.ToString());
				m_writer.WriteAttributeString("style", "position", null, tabStops.TextPosition + "in");
				if (tabStops.TabStopLeader != 0)
				{
					m_writer.WriteAttributeString("style", "leader-char", null, (tabStops.TabStopLeader == TabStopLeader.Dotted) ? "." : "");
				}
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
		}
		if (paraProp.HasKey(18, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "widows", null, paraProp.Windows.ToString());
		}
		if (paraProp.HasKey(27, paraProp.m_styleFlag1))
		{
			m_writer.WriteAttributeString("fo", "orphans", null, paraProp.Orphans.ToString());
		}
		m_writer.WriteEndElement();
	}

	private void SerializeExcelTableCellProperties(OTableCellProperties cellProp)
	{
		if (cellProp == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "table-cell-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		VerticalAlign? verticalAlign = cellProp.VerticalAlign;
		if (verticalAlign.HasValue)
		{
			m_writer.WriteAttributeString("style", "vertical-align", null, verticalAlign.ToString());
		}
		if (cellProp.HasKey(8, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, (cellProp.BackColor == Color.Transparent) ? cellProp.BackColor.Name.ToString().ToLower() : HexConverter(cellProp.BackColor));
		}
		if (cellProp.HasKey(7, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("style", "shrink-to-fit", null, true.ToString().ToLower());
		}
		bool flag = false;
		if (cellProp.BorderLeft != null && cellProp.BorderRight != null && cellProp.BorderBottom != null && cellProp.BorderRight != null)
		{
			flag = cellProp.BorderLeft.Equals(cellProp.BorderRight) && cellProp.BorderRight.Equals(cellProp.BorderTop) && cellProp.BorderTop.Equals(cellProp.BorderBottom);
		}
		if (cellProp.Border != null || flag)
		{
			ODFBorder borderLeft = cellProp.BorderLeft;
			if (borderLeft != null && borderLeft.LineStyle != 0)
			{
				m_writer.WriteAttributeString("fo", "border", null, borderLeft.LineWidth + " " + borderLeft.LineStyle.ToString().ToLower() + " " + HexConverter(borderLeft.LineColor));
			}
		}
		else
		{
			ODFBorder oDFBorder = null;
			if (cellProp.BorderLeft != null)
			{
				oDFBorder = cellProp.BorderLeft;
				if (oDFBorder != null && oDFBorder.LineStyle != 0)
				{
					m_writer.WriteAttributeString("fo", "border-left", null, oDFBorder.LineWidth + " " + oDFBorder.LineStyle.ToString().ToLower() + " " + HexConverter(oDFBorder.LineColor));
				}
			}
			if (cellProp.BorderRight != null)
			{
				oDFBorder = cellProp.BorderRight;
				if (oDFBorder != null && oDFBorder.LineStyle != 0)
				{
					m_writer.WriteAttributeString("fo", "border-right", null, oDFBorder.LineWidth + " " + oDFBorder.LineStyle.ToString().ToLower() + " " + HexConverter(oDFBorder.LineColor));
				}
			}
			if (cellProp.BorderTop != null)
			{
				oDFBorder = cellProp.BorderTop;
				if (oDFBorder != null && oDFBorder.LineStyle != 0)
				{
					m_writer.WriteAttributeString("fo", "border-top", null, oDFBorder.LineWidth + " " + oDFBorder.LineStyle.ToString().ToLower() + " " + HexConverter(oDFBorder.LineColor));
				}
			}
			if (cellProp.BorderBottom != null)
			{
				oDFBorder = cellProp.BorderBottom;
				if (oDFBorder != null && oDFBorder.LineStyle != 0)
				{
					m_writer.WriteAttributeString("fo", "border-bottom", null, oDFBorder.LineWidth + " " + oDFBorder.LineStyle.ToString().ToLower() + " " + HexConverter(oDFBorder.LineColor));
				}
			}
		}
		if (cellProp.DiagonalLeft != null)
		{
			ODFBorder diagonalLeft = cellProp.DiagonalLeft;
			if (diagonalLeft != null && diagonalLeft.LineStyle != 0)
			{
				m_writer.WriteAttributeString("style", "diagonal-tl-br", null, diagonalLeft.LineWidth + " " + diagonalLeft.LineStyle.ToString().ToLower() + " " + HexConverter(diagonalLeft.LineColor));
			}
		}
		if (cellProp.DiagonalRight != null)
		{
			ODFBorder diagonalRight = cellProp.DiagonalRight;
			if (diagonalRight != null && diagonalRight.LineStyle != 0)
			{
				m_writer.WriteAttributeString("style", "diagonal-bl-tr", null, diagonalRight.LineWidth + " " + diagonalRight.LineStyle.ToString().ToLower() + " " + HexConverter(diagonalRight.LineColor));
			}
		}
		if (cellProp.HasKey(1, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("fo", "wrap-option", null, "wrap");
		}
		if (cellProp.HasKey(0, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("style", "rotation-angle", null, cellProp.RotationAngle.ToString());
		}
		if (cellProp.HasKey(15, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("style", "direction", null, cellProp.Direction.ToString());
		}
		if (cellProp.HasKey(14, cellProp.tableCellFlags) && cellProp.RepeatContent)
		{
			m_writer.WriteAttributeString("style", "repeat-content", null, "true");
		}
		m_writer.WriteEndElement();
	}

	private void SerializeTableCellProperties(OTableCellProperties cellProp)
	{
		if (cellProp == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "table-cell-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		VerticalAlign? verticalAlign = cellProp.VerticalAlign;
		if (verticalAlign.HasValue)
		{
			m_writer.WriteAttributeString("style", "vertical-align", null, verticalAlign.ToString());
		}
		if (cellProp.HasKey(8, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, (cellProp.BackColor == Color.Transparent) ? cellProp.BackColor.Name.ToString().ToLower() : HexConverter(cellProp.BackColor));
		}
		if (cellProp.HasKey(7, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("style", "shrink-to-fit", null, true.ToString().ToLower());
		}
		if (cellProp.Border != null && cellProp.Border.LineStyle != 0)
		{
			if (cellProp.Border != null)
			{
				m_writer.WriteAttributeString("fo", "border", null, cellProp.Border.LineWidth + "in " + cellProp.Border.LineStyle.ToString() + " " + HexConverter(cellProp.Border.LineColor));
			}
			m_writer.WriteAttributeString("fo", "padding-top", null, cellProp.PaddingTop + "in");
			m_writer.WriteAttributeString("fo", "padding-bottom", null, cellProp.PaddingBottom + "in");
			m_writer.WriteAttributeString("fo", "padding-left", null, cellProp.PaddingLeft + "in");
			m_writer.WriteAttributeString("fo", "padding-right", null, cellProp.PaddingRight + "in");
		}
		else
		{
			if (cellProp.BorderLeft != null)
			{
				m_writer.WriteAttributeString("fo", "border-left", null, cellProp.BorderLeft.LineWidth + "in " + cellProp.BorderLeft.LineStyle.ToString().ToLower() + " " + HexConverter(cellProp.BorderLeft.LineColor));
				m_writer.WriteAttributeString("fo", "padding-left", null, cellProp.PaddingLeft + "in");
			}
			if (cellProp.BorderRight != null)
			{
				m_writer.WriteAttributeString("fo", "border-right", null, cellProp.BorderRight.LineWidth + "in " + cellProp.BorderRight.LineStyle.ToString().ToLower() + " " + HexConverter(cellProp.BorderRight.LineColor));
				m_writer.WriteAttributeString("fo", "padding-right", null, cellProp.PaddingRight + "in");
			}
			if (cellProp.BorderTop != null)
			{
				m_writer.WriteAttributeString("fo", "border-top", null, cellProp.BorderTop.LineWidth + "in " + cellProp.BorderTop.LineStyle.ToString().ToLower() + " " + HexConverter(cellProp.BorderTop.LineColor));
				m_writer.WriteAttributeString("fo", "padding-top", null, cellProp.PaddingTop + "in");
			}
			if (cellProp.BorderBottom != null)
			{
				m_writer.WriteAttributeString("fo", "border-bottom", null, cellProp.BorderBottom.LineWidth + "in " + cellProp.BorderBottom.LineStyle.ToString().ToLower() + " " + HexConverter(cellProp.BorderBottom.LineColor));
				m_writer.WriteAttributeString("fo", "padding-bottom", null, cellProp.PaddingBottom + "in");
			}
		}
		if (cellProp.HasKey(1, cellProp.tableCellFlags))
		{
			m_writer.WriteAttributeString("fo", "wrap-option", null, "wrap");
		}
		m_writer.WriteEndElement();
	}

	internal void SerializeTextProperties(TextProperties txtProp)
	{
		if (txtProp == null)
		{
			return;
		}
		m_writer.WriteStartElement("style", "text-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		if (txtProp.HasKey(16, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "font-name", null, txtProp.FontName);
		}
		if (txtProp.HasKey(17, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-size", null, txtProp.FontSize + "pt");
			m_writer.WriteAttributeString("style", "font-size-asian", null, txtProp.FontSize + "pt");
			m_writer.WriteAttributeString("style", "font-size-complex", null, txtProp.FontSize + "pt");
		}
		if (txtProp.HasKey(23, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "color", null, HexConverter(txtProp.Color));
		}
		if (txtProp.HasKey(22, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-weight", null, txtProp.FontWeight.ToString());
		}
		if (txtProp.FontStyle == ODFFontStyle.italic)
		{
			m_writer.WriteAttributeString("fo", "font-style", null, txtProp.FontStyle.ToString());
		}
		if (txtProp.HasKey(5, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-underline-type", null, txtProp.TextUnderlineType.ToString().ToLower());
		}
		if (txtProp.HasKey(6, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-underline-syle", null, txtProp.TextUnderlineStyle.ToString());
		}
		if (txtProp.HasKey(22, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "font-weight-asian", null, txtProp.FontWeightAsian.ToString());
			m_writer.WriteAttributeString("style", "font-style-asian", null, txtProp.FontStyleAsian.ToString());
		}
		if (txtProp.HasKey(0, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "font-relief", null, txtProp.FontRelief.ToString());
		}
		if (txtProp.HasKey(20, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "background-color", null, HexConverter(txtProp.BackgroundColor));
		}
		if (txtProp.HasKey(0, txtProp.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "letter-kerning", null, txtProp.LetterKerning.ToString().ToLower());
		}
		if (txtProp.HasKey(9, txtProp.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "text-line-through-type", null, txtProp.LinethroughType.ToString());
			if (txtProp.LinethroughType != 0)
			{
				m_writer.WriteAttributeString("style", "text-line-through-style", null, txtProp.LinethroughStyle.ToString());
				m_writer.WriteAttributeString("style", "text-line-through-color", null, txtProp.LinethroughColor);
			}
		}
		if (txtProp.HasKey(6, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-underline-style", null, txtProp.TextUnderlineStyle.ToString());
			m_writer.WriteAttributeString("style", "text-underline-color", null, txtProp.TextUnderlineColor);
			m_writer.WriteAttributeString("style", "text-underline-type", null, "single");
		}
		if (txtProp.HasKey(3, txtProp.m_textFlag2) && !txtProp.HasKey(28, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "text-transform", null, txtProp.TextTransform.ToString());
		}
		if (txtProp.HasKey(9, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-scale", null, txtProp.TextScale + "%");
		}
		if (txtProp.HasKey(3, txtProp.m_textFlag3))
		{
			m_writer.WriteAttributeString("style", "text-outline", null, txtProp.TextOutline.ToString().ToLower());
		}
		if (txtProp.HasKey(27, txtProp.m_textFlag1) && !txtProp.IsTextDisplay)
		{
			m_writer.WriteAttributeString("text", "display", null, "none");
		}
		if (txtProp.HasKey(21, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("style", "text-position", null, txtProp.TextPosition.ToString());
		}
		if (txtProp.HasKey(28, txtProp.m_textFlag1))
		{
			m_writer.WriteAttributeString("fo", "font-variant", null, "small-caps");
		}
		m_writer.WriteEndElement();
	}

	internal void SerializeAutomaticStyles(PageLayoutCollection layouts)
	{
		m_writer.WriteStartElement("office", "automatic-styles", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
		SerializePageLayouts(layouts);
	}

	internal void SerializeAutoStyleStart()
	{
		m_writer.WriteStartElement("office", "automatic-styles", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
	}

	internal void SerializeContentAutoStyles(ODFStyleCollection styles)
	{
		SerializeODFStyles(styles);
	}

	internal void SerializeContentListStyles(List<OListStyle> listStyles)
	{
		for (int i = 0; i < listStyles.Count; i++)
		{
			OListStyle oListStyle = listStyles[i];
			m_writer.WriteStartElement("text", "list-style", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
			m_writer.WriteAttributeString("style", "name", null, oListStyle.CurrentStyleName);
			int num = 1;
			foreach (ListLevelProperties listLevel in oListStyle.ListLevels)
			{
				m_writer.WriteStartElement("text", (listLevel.NumberFormat == ListNumberFormat.Bullet && !listLevel.IsPictureBullet && listLevel.PictureBullet == null) ? "list-level-style-bullet" : ((listLevel.PictureBullet != null) ? "list-level-style-image" : "list-level-style-number"), "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
				m_writer.WriteAttributeString("text", "level", null, num.ToString());
				if (listLevel.Style != null && !string.IsNullOrEmpty(listLevel.Style.Name))
				{
					m_writer.WriteAttributeString("text", "style-name", null, listLevel.Style.Name.ToString());
				}
				string text = "";
				text = ((listLevel.NumberFormat == ListNumberFormat.Decimal) ? "1" : ((listLevel.NumberFormat == ListNumberFormat.LowerLetter) ? "a" : ((listLevel.NumberFormat == ListNumberFormat.UpperLetter) ? "A" : ((listLevel.NumberFormat == ListNumberFormat.UpperRoman) ? "I" : ((listLevel.NumberFormat == ListNumberFormat.LowerRoman) ? "i" : "")))));
				if (!string.IsNullOrEmpty(text))
				{
					m_writer.WriteAttributeString("style", "num-format", null, text);
				}
				if (listLevel.NumberFormat == ListNumberFormat.Bullet && !string.IsNullOrEmpty(listLevel.BulletCharacter))
				{
					m_writer.WriteAttributeString("text", "bullet-char", null, listLevel.BulletCharacter);
				}
				if (listLevel.IsPictureBullet && listLevel.PictureBullet != null && !string.IsNullOrEmpty(listLevel.PictureHRef))
				{
					m_writer.WriteAttributeString("xlink", "href", null, listLevel.PictureHRef);
					m_writer.WriteAttributeString("xlink", "type", null, "simple");
					m_writer.WriteAttributeString("xlink", "show", null, "embed");
					m_writer.WriteAttributeString("xlink", "actuate", null, "onLoad");
				}
				if (!string.IsNullOrEmpty(listLevel.NumberSufix))
				{
					m_writer.WriteAttributeString("style", "num-suffix", null, listLevel.NumberSufix);
				}
				m_writer.WriteAttributeString("style", "num-letter-sync", null, "true");
				m_writer.WriteStartElement("style", "list-level-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				if (listLevel.TextAlignment != 0)
				{
					m_writer.WriteAttributeString("fo", "text-align", null, listLevel.TextAlignment.ToString().ToLower());
				}
				m_writer.WriteAttributeString("text", "space-before", null, listLevel.SpaceBefore + "in");
				m_writer.WriteAttributeString("text", "min-label-width", null, listLevel.MinimumLabelWidth + "in");
				m_writer.WriteAttributeString("text", "list-level-position-and-space-mode", null, "label-alignment");
				m_writer.WriteStartElement("style", "list-level-label-alignment", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				m_writer.WriteAttributeString("text", "label-followed-by", null, "listtab");
				m_writer.WriteAttributeString("fo", "margin-left", null, listLevel.LeftMargin + "in");
				m_writer.WriteAttributeString("fo", "text-indent", null, listLevel.TextIndent + "in");
				m_writer.WriteEndElement();
				m_writer.WriteEndElement();
				if (listLevel.TextProperties != null)
				{
					SerializeTextProperties(listLevel.TextProperties);
				}
				m_writer.WriteEndElement();
				num++;
			}
			m_writer.WriteEndElement();
		}
	}

	internal void SerializeMasterStyles(MasterPageCollection mPages, List<string> pageNames)
	{
		MasterPage[] array = new MasterPage[mPages.DictMasterPages.Values.Count];
		mPages.DictMasterPages.Values.CopyTo(array, 0);
		foreach (MasterPage masterPage in array)
		{
			m_writer.WriteStartElement("style", "master-page", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			m_writer.WriteAttributeString("style", "name", null, masterPage.Name);
			m_writer.WriteAttributeString("style", "page-layout-name", null, masterPage.PageLayoutName);
			if (masterPage.Header != null)
			{
				SerializeHeaderFooter(masterPage.Header, isHeader: true);
			}
			if (masterPage.HeaderLeft != null)
			{
				SerializeHeaderLeftStart();
				SerializeHeaderFooterContent(masterPage.HeaderLeft);
				SerializeEnd();
			}
			if (masterPage.Footer != null)
			{
				SerializeHeaderFooter(masterPage.Footer, isHeader: false);
			}
			if (masterPage.FooterLeft != null)
			{
				SerializeFooterLeftStart();
				SerializeHeaderFooterContent(masterPage.FooterLeft);
				SerializeEnd();
			}
			SerializeEnd();
			if (masterPage.FirstPageHeader != null || masterPage.FirstPageFooter != null)
			{
				m_writer.WriteStartElement("style", "master-page", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				m_writer.WriteAttributeString("style", "next-style-name", null, masterPage.Name);
				int index = pageNames.IndexOf(masterPage.Name);
				pageNames.RemoveAt(index);
				masterPage.Name = "MPF" + Regex.Match(masterPage.Name, "\\d+").Value;
				pageNames.Insert(index, masterPage.Name);
				m_writer.WriteAttributeString("style", "name", null, masterPage.Name);
				m_writer.WriteAttributeString("style", "page-layout-name", null, masterPage.PageLayoutName);
				if (masterPage.FirstPageHeader != null)
				{
					SerializeHeaderFooter(masterPage.FirstPageHeader, isHeader: true);
				}
				if (masterPage.FirstPageFooter != null)
				{
					SerializeHeaderFooter(masterPage.FirstPageFooter, isHeader: false);
				}
				SerializeEnd();
			}
		}
	}

	private void SerializeHeaderFooter(HeaderFooterContent headerFooter, bool isHeader)
	{
		if (isHeader)
		{
			SerializeHeaderStart();
		}
		else
		{
			SerializeFooterStart();
		}
		SerializeHeaderFooterContent(headerFooter);
		SerializeEnd();
	}

	internal void SerializeHeaderLeftStart()
	{
		m_writer.WriteStartElement("style", "header-left", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
	}

	internal void SerializeFooterLeftStart()
	{
		m_writer.WriteStartElement("style", "footer-left", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
	}

	internal void SerializeHeaderStart()
	{
		m_writer.WriteStartElement("style", "header", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
	}

	internal void SerializeFooterStart()
	{
		m_writer.WriteStartElement("style", "footer", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
	}

	internal void SerializeMasterStylesStart()
	{
		m_writer.WriteStartElement("office", "master-styles", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
	}

	internal void SerializeEnd()
	{
		m_writer.WriteEndElement();
	}

	private void SerializePageLayouts(PageLayoutCollection layouts)
	{
		PageLayout[] array = new PageLayout[layouts.DictStyles.Values.Count];
		layouts.DictStyles.Values.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			m_writer.WriteStartElement("style", "page-layout", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			m_writer.WriteAttributeString("style", "name", null, array[i].Name);
			m_writer.WriteStartElement("style", "page-layout-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
			PageLayoutProperties pageLayoutProperties = array[i].PageLayoutProperties;
			m_writer.WriteAttributeString("fo", "page-width", null, pageLayoutProperties.PageWidth.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("fo", "page-height", null, pageLayoutProperties.PageHeight.ToString(CultureInfo.InvariantCulture) + "in");
			if (pageLayoutProperties.Border != null)
			{
				m_writer.WriteAttributeString("fo", "border", null, pageLayoutProperties.Border.LineWidth + "in " + pageLayoutProperties.Border.LineStyle.ToString() + " " + HexConverter(pageLayoutProperties.Border.LineColor));
				m_writer.WriteAttributeString("fo", "padding-top", null, pageLayoutProperties.PaddingTop + "in");
				m_writer.WriteAttributeString("fo", "padding-bottom", null, pageLayoutProperties.PaddingBottom + "in");
				m_writer.WriteAttributeString("fo", "padding-left", null, pageLayoutProperties.PaddingLeft + "in");
				m_writer.WriteAttributeString("fo", "padding-right", null, pageLayoutProperties.PaddingRight + "in");
			}
			m_writer.WriteAttributeString("fo", "margin-top", null, pageLayoutProperties.MarginTop.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("fo", "margin-bottom", null, pageLayoutProperties.MarginBottom.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("fo", "margin-left", null, pageLayoutProperties.MarginLeft.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("fo", "margin-right", null, pageLayoutProperties.MarginRight.ToString(CultureInfo.InvariantCulture) + "in");
			m_writer.WriteAttributeString("style", "print-orientation", null, pageLayoutProperties.PageOrientation.ToString());
			if (array[i].ColumnsCount > 1)
			{
				m_writer.WriteStartElement("style", "columns", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
				m_writer.WriteAttributeString("fo", "column-count", null, array[i].ColumnsCount.ToString());
				m_writer.WriteAttributeString("fo", "column-gap", null, array[i].ColumnsGap.ToString());
				m_writer.WriteEndElement();
			}
			m_writer.WriteEndElement();
			SerializeHeaderFooterStyles(array[i]);
			m_writer.WriteEndElement();
		}
	}

	private void SerializeHeaderFooterStyles(PageLayout layout)
	{
		HeaderFooterStyle headerStyle = layout.HeaderStyle;
		m_writer.WriteStartElement("style", "header-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		SerializeHeaderFooterProperties(headerStyle);
		m_writer.WriteEndElement();
		HeaderFooterStyle footerStyle = layout.FooterStyle;
		m_writer.WriteStartElement("style", "footer-style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		SerializeHeaderFooterProperties(footerStyle);
		m_writer.WriteEndElement();
	}

	private void SerializeHeaderFooterProperties(HeaderFooterStyle HFStyle)
	{
		m_writer.WriteStartElement("style", "header-footer-properties", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
		m_writer.WriteAttributeString("fo", "margin-left", null, HFStyle.HeaderFooterproperties.MarginLeft + "in");
		m_writer.WriteAttributeString("fo", "margin-right", null, HFStyle.HeaderFooterproperties.MarginRight + "in");
		if (HFStyle.IsHeader)
		{
			m_writer.WriteAttributeString("fo", "margin-bottom", null, HFStyle.HeaderFooterproperties.MarginBottom + "in");
			if (HFStyle.HeaderDistance != 0.0)
			{
				m_writer.WriteAttributeString("fo", "min-height", null, HFStyle.HeaderDistance + "in");
			}
		}
		else
		{
			m_writer.WriteAttributeString("fo", "margin-top", null, HFStyle.HeaderFooterproperties.MarginTop + "in");
			if (HFStyle.FooterDistance != 0.0)
			{
				m_writer.WriteAttributeString("fo", "min-height", null, HFStyle.FooterDistance + "in");
			}
		}
		m_writer.WriteEndElement();
	}

	private static string HexConverter(Color c)
	{
		return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
	}

	internal void Dispose()
	{
		if (m_archieve != null)
		{
			m_archieve.Dispose();
			m_archieve = null;
		}
		if (m_writer != null)
		{
			m_writer = null;
		}
	}
}
