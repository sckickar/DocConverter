using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.ReaderWriter.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class RtfWriter : RtfNavigator
{
	private enum BorderType
	{
		Right,
		Left,
		Top,
		Bottom
	}

	private const char DEF_FOOTNOTE_SYMBOL = '\u0002';

	private const string DEF_FONT_NAME = "Times New Roman";

	private const int MM_ANISOTROPIC = 8;

	private readonly string c_lineBreak = '\v'.ToString();

	private readonly string c_transfer = '\u00a0'.ToString();

	private readonly string c_symbol92 = '\\'.ToString();

	private readonly string c_symbol31 = '\u001f'.ToString();

	private readonly string c_symbol30 = '\u001e'.ToString();

	private readonly string c_symbol61553 = '\uf071'.ToString();

	private readonly string c_symbol61549 = '\uf06d'.ToString();

	private readonly string c_symbol123 = '{'.ToString();

	private readonly string c_symbol125 = '}'.ToString();

	private readonly string c_slashSymbol = '\\'.ToString();

	private readonly string c_symbol8226 = '•'.ToString();

	private readonly string c_enDash = '–'.ToString();

	private readonly string c_emDash = '—'.ToString();

	private readonly string c_enSpace = '\u2002'.ToString();

	private readonly string c_emSpace = '\u2003'.ToString();

	private readonly string c_section = '§'.ToString();

	private readonly string c_copyRight = '©'.ToString();

	private readonly string c_registered = '®'.ToString();

	private readonly string c_paraMark = '¶'.ToString();

	private readonly string c_tradeMark = '™'.ToString();

	private readonly string c_singleOpenQuote = '‘'.ToString();

	private readonly string c_singleCloseQuote = '’'.ToString();

	private readonly string c_doubleOpenQuote = '“'.ToString();

	private readonly string c_doubleCloseQuote = '”'.ToString();

	private WordDocument m_doc;

	private Stream m_stream;

	private Encoding m_encoding;

	private byte[] m_defStyleBytes;

	private byte[] m_listTableBytes;

	private byte[] m_listOverrideTableBytes;

	private byte[] m_styleBytes;

	private byte[] m_colorBytes;

	private byte[] m_fontBytes;

	private List<byte[]> m_mainBodyBytesList;

	private int m_fontId;

	private int m_uniqueId = 1;

	private int m_cellEndPos;

	private int m_tableNestedLevel;

	private int m_colorId = 1;

	private Dictionary<string, string> m_styles;

	private Dictionary<string, string> m_stylesNumb;

	private Dictionary<string, Dictionary<int, int>> m_listStart;

	private Dictionary<string, int> m_listsIds;

	private Dictionary<string, string> m_fontEntries;

	private Dictionary<string, string> m_associatedFontEntries;

	private bool m_hasFootnote;

	private bool m_hasEndnote;

	private bool m_isCyrillicText;

	private Dictionary<int, string> m_listOverride;

	private Dictionary<string, string> m_commentIds;

	private Stack<object> m_currentField;

	private Dictionary<Color, int> m_colorTable;

	private bool m_isField;

	private Dictionary<string, string> FontEntries
	{
		get
		{
			if (m_fontEntries == null)
			{
				m_fontEntries = new Dictionary<string, string>();
			}
			return m_fontEntries;
		}
	}

	private Dictionary<string, string> AssociatedFontEntries
	{
		get
		{
			if (m_associatedFontEntries == null)
			{
				m_associatedFontEntries = new Dictionary<string, string>();
			}
			return m_associatedFontEntries;
		}
	}

	private Dictionary<string, int> ListsIds
	{
		get
		{
			if (m_listsIds == null)
			{
				m_listsIds = new Dictionary<string, int>();
			}
			return m_listsIds;
		}
	}

	private Dictionary<int, string> ListOverrideAr
	{
		get
		{
			if (m_listOverride == null)
			{
				m_listOverride = new Dictionary<int, string>();
			}
			return m_listOverride;
		}
	}

	private Dictionary<string, string> Styles
	{
		get
		{
			if (m_styles == null)
			{
				m_styles = new Dictionary<string, string>();
			}
			return m_styles;
		}
	}

	private Dictionary<string, string> StyleNumb
	{
		get
		{
			if (m_stylesNumb == null)
			{
				m_stylesNumb = new Dictionary<string, string>();
			}
			return m_stylesNumb;
		}
	}

	private Dictionary<string, Dictionary<int, int>> ListStart
	{
		get
		{
			if (m_listStart == null)
			{
				m_listStart = new Dictionary<string, Dictionary<int, int>>();
			}
			return m_listStart;
		}
	}

	private Dictionary<string, string> CommentIds
	{
		get
		{
			if (m_commentIds == null)
			{
				m_commentIds = new Dictionary<string, string>();
			}
			return m_commentIds;
		}
	}

	private Stack<object> CurrentField
	{
		get
		{
			if (m_currentField == null)
			{
				m_currentField = new Stack<object>();
			}
			return m_currentField;
		}
	}

	private Dictionary<Color, int> ColorTable
	{
		get
		{
			if (m_colorTable == null)
			{
				m_colorTable = new Dictionary<Color, int>();
			}
			return m_colorTable;
		}
	}

	public RtfWriter()
	{
		m_encoding = WordDocument.GetEncoding("ASCII");
		m_styleBytes = m_encoding.GetBytes("{" + c_slashSymbol + "stylesheet");
		m_fontBytes = m_encoding.GetBytes("{" + c_slashSymbol + "fonttbl");
		m_colorBytes = m_encoding.GetBytes("{" + c_slashSymbol + "colortbl;");
	}

	internal void Write(Stream stream, IWordDocument document)
	{
		m_doc = document as WordDocument;
		m_stream = stream;
		BuildDefaultStyles();
		AppendListStyles();
		BuildStyleSheet();
		BuildSections();
		AppendOverrideList();
		byte[] bytes = m_encoding.GetBytes("{" + c_slashSymbol + "rtf1" + c_slashSymbol + "ansi");
		m_stream.Write(bytes, 0, bytes.Length);
		WriteBody();
		bytes = m_encoding.GetBytes("}");
		m_stream.Write(bytes, 0, bytes.Length);
	}

	internal string GetRtfText(IWordDocument document)
	{
		m_doc = document as WordDocument;
		m_stream = new MemoryStream();
		Write(m_stream, m_doc);
		m_stream.Position = 0L;
		byte[] array = new byte[m_stream.Length];
		m_stream.Read(array, 0, array.Length);
		m_stream.Dispose();
		return m_encoding.GetString(array, 0, array.Length);
	}

	private void WriteBody()
	{
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			m_stream.Write(m_fontBytes, 0, m_fontBytes.Length);
			m_stream.WriteByte(125);
			m_stream.WriteByte(13);
			m_stream.WriteByte(10);
			m_fontBytes = null;
			m_stream.Write(m_colorBytes, 0, m_colorBytes.Length);
			m_stream.WriteByte(125);
			m_stream.WriteByte(13);
			m_stream.WriteByte(10);
			m_colorBytes = null;
			m_stream.Write(m_defStyleBytes, 0, m_defStyleBytes.Length);
			m_defStyleBytes = null;
			m_stream.Write(m_styleBytes, 0, m_styleBytes.Length);
			m_stream.WriteByte(125);
			m_stream.WriteByte(13);
			m_stream.WriteByte(10);
			m_styleBytes = null;
			if (m_listTableBytes != null)
			{
				m_stream.Write(m_listTableBytes, 0, m_listTableBytes.Length);
				m_listTableBytes = null;
			}
			m_stream.Write(m_listOverrideTableBytes, 0, m_listOverrideTableBytes.Length);
			m_listOverrideTableBytes = null;
		}
		else
		{
			if (m_fontBytes.Length > 9)
			{
				m_stream.Write(m_fontBytes, 0, m_fontBytes.Length);
				m_stream.WriteByte(125);
				m_stream.WriteByte(13);
				m_stream.WriteByte(10);
				m_fontBytes = null;
			}
			if (m_colorBytes.Length > 11)
			{
				m_stream.Write(m_colorBytes, 0, m_colorBytes.Length);
				m_stream.WriteByte(125);
				m_stream.WriteByte(13);
				m_stream.WriteByte(10);
				m_colorBytes = null;
			}
			if (m_doc.HasStyleSheets)
			{
				m_stream.Write(m_defStyleBytes, 0, m_defStyleBytes.Length);
				m_defStyleBytes = null;
				m_stream.Write(m_styleBytes, 0, m_styleBytes.Length);
				m_stream.WriteByte(125);
				m_stream.WriteByte(13);
				m_stream.WriteByte(10);
				m_styleBytes = null;
			}
			if (m_listTableBytes != null && m_listOverrideTableBytes != null)
			{
				m_stream.Write(m_listTableBytes, 0, m_listTableBytes.Length);
				m_listTableBytes = null;
				m_stream.Write(m_listOverrideTableBytes, 0, m_listOverrideTableBytes.Length);
				m_listOverrideTableBytes = null;
			}
		}
		BuildDocProperties();
		if (m_doc.DOP.AutoHyphen)
		{
			byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "hyphauto1");
			m_stream.Write(bytes, 0, bytes.Length);
		}
		if (!m_doc.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark])
		{
			byte[] bytes2 = m_encoding.GetBytes(c_slashSymbol + "spltpgpar");
			m_stream.Write(bytes2, 0, bytes2.Length);
			bytes2 = null;
		}
		if (m_doc.Settings.CompatibilityOptions[CompatibilityOption.PrintMet])
		{
			byte[] bytes3 = m_encoding.GetBytes(c_slashSymbol + "lytprtmet");
			m_stream.Write(bytes3, 0, bytes3.Length);
			bytes3 = null;
		}
		if (!m_doc.Settings.CompatibilityOptions[CompatibilityOption.DontUseHTMLParagraphAutoSpacing])
		{
			byte[] bytes4 = m_encoding.GetBytes(c_slashSymbol + "htmautsp");
			m_stream.Write(bytes4, 0, bytes4.Length);
			bytes4 = null;
		}
		if (!m_doc.Settings.CompatibilityOptions[CompatibilityOption.AllowSpaceOfSameStyleInTable])
		{
			byte[] bytes5 = m_encoding.GetBytes(c_slashSymbol + "nocxsptable");
			m_stream.Write(bytes5, 0, bytes5.Length);
			bytes5 = null;
		}
		if (m_doc.DOP.Dop2000.Copts.Copts80.Copts60.NoSpaceRaiseLower)
		{
			byte[] bytes6 = m_encoding.GetBytes(c_slashSymbol + "noextrasprl");
			m_stream.Write(bytes6, 0, bytes6.Length);
			bytes6 = null;
		}
		int count = m_mainBodyBytesList.Count;
		for (int i = 0; i < count; i++)
		{
			m_stream.Write(m_mainBodyBytesList[0], 0, m_mainBodyBytesList[0].Length);
			m_mainBodyBytesList.RemoveAt(0);
		}
		m_mainBodyBytesList = null;
	}

	private void BuildDocProperties()
	{
		WPageSetup pageSetup = m_doc.Sections[0].PageSetup;
		byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "paperw" + (int)Math.Round(pageSetup.PageSize.Width * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "paperh" + (int)Math.Round(pageSetup.PageSize.Height * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margl" + (int)Math.Round(pageSetup.Margins.Left * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margr" + (int)Math.Round(pageSetup.Margins.Right * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margt" + (int)Math.Round(pageSetup.Margins.Top * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margb" + (int)Math.Round(pageSetup.Margins.Bottom * 20f));
		m_stream.Write(bytes, 0, bytes.Length);
		bytes = null;
	}

	private void BuildDefaultStyles()
	{
		WParagraphStyle wParagraphStyle = m_doc.Styles.FindByName("Normal") as WParagraphStyle;
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "defchp");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildCharacterFormat(wParagraphStyle.CharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "defpap");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildParagraphFormat(wParagraphStyle.ParagraphFormat, null));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		m_defStyleBytes = memoryStream.ToArray();
	}

	private void BuildSections()
	{
		m_mainBodyBytesList = new List<byte[]>();
		BuildBackground();
		int i = 0;
		for (int count = m_doc.Sections.Count; i < count; i++)
		{
			m_hasFootnote = (m_hasEndnote = false);
			WSection section = m_doc.Sections[i];
			BuildSectionProp(section);
			CheckFootEndnote();
			BuildSection(section);
		}
	}

	private void BuildBackground()
	{
		if (m_doc.Background.Type != 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "viewbksp1");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "background");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "shp");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(BuildShapeFill(m_doc.Background, isPageBackground: true));
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("}}}");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(Environment.NewLine);
			memoryStream.Write(bytes, 0, bytes.Length);
			m_mainBodyBytesList.Add(memoryStream.ToArray());
		}
	}

	private void BuildSection(WSection section)
	{
		_ = section.NextSibling;
		BuildHeadersFooters(section.HeadersFooters);
		BuildSectionBodyItems(section.Body.Items);
		m_mainBodyBytesList.Add(m_encoding.GetBytes(Environment.NewLine));
	}

	private void BuildSectionBodyItems(BodyItemCollection bodyItems)
	{
		for (int i = 0; i < bodyItems.Count; i++)
		{
			switch (bodyItems[i].EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = bodyItems[i] as WParagraph;
				wParagraph.SplitTextRange();
				m_mainBodyBytesList.Add(BuildParagraph(wParagraph));
				break;
			}
			case EntityType.Table:
				m_mainBodyBytesList.Add(BuildTable(bodyItems[i] as WTable));
				break;
			case EntityType.BlockContentControl:
				m_mainBodyBytesList.Add(BuildBodyItems((bodyItems[i] as BlockContentControl).TextBody.Items));
				break;
			}
		}
		if (bodyItems.LastItem is WTable)
		{
			m_mainBodyBytesList.Add(BuildParagraph(new WParagraph(bodyItems.Document)));
		}
	}

	private byte[] BuildBodyItems(BodyItemCollection bodyItems)
	{
		MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i < bodyItems.Count; i++)
		{
			switch (bodyItems[i].EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = bodyItems[i] as WParagraph;
				wParagraph.SplitTextRange();
				byte[] array = BuildParagraph(wParagraph);
				memoryStream.Write(array, 0, array.Length);
				break;
			}
			case EntityType.Table:
			{
				byte[] array = BuildTable(bodyItems[i] as WTable);
				memoryStream.Write(array, 0, array.Length);
				break;
			}
			case EntityType.BlockContentControl:
			{
				byte[] array = BuildBodyItems((bodyItems[i] as BlockContentControl).TextBody.Items);
				memoryStream.Write(array, 0, array.Length);
				break;
			}
			}
		}
		if (bodyItems.LastItem is WTable)
		{
			byte[] array = BuildParagraph(new WParagraph(bodyItems.Document));
			memoryStream.Write(array, 0, array.Length);
		}
		return memoryStream.ToArray();
	}

	private void BuildHeadersFooters(WHeadersFooters headerFooters)
	{
		if (headerFooters != null || m_doc.Watermark != null)
		{
			if (headerFooters.EvenHeader.Items.Count > 0 || (headerFooters.EvenHeader.Watermark.Type != 0 && headerFooters.EvenHeader.WriteWatermark))
			{
				BuildHeaderFooter("{" + c_slashSymbol + "headerl", headerFooters.EvenHeader.Items, GetWatermark(headerFooters.EvenHeader.Watermark), headerFooters.EvenHeader.WriteWatermark);
			}
			if (headerFooters.OddHeader.Items.Count > 0 || (headerFooters.OddHeader.Watermark.Type != 0 && headerFooters.OddHeader.WriteWatermark))
			{
				BuildHeaderFooter("{" + c_slashSymbol + "headerr", headerFooters.OddHeader.Items, GetWatermark(headerFooters.OddHeader.Watermark), headerFooters.OddHeader.WriteWatermark);
			}
			if (headerFooters.EvenFooter.Items.Count > 0)
			{
				BuildHeaderFooter("{" + c_slashSymbol + "footerl", headerFooters.EvenFooter.Items, string.Empty, writeWaterMark: false);
			}
			if (headerFooters.OddFooter.Items.Count > 0)
			{
				BuildHeaderFooter("{" + c_slashSymbol + "footerr", headerFooters.OddFooter.Items, string.Empty, writeWaterMark: false);
			}
			if (headerFooters.FirstPageHeader.Items.Count > 0 || (headerFooters.FirstPageHeader.Watermark.Type != 0 && headerFooters.FirstPageHeader.WriteWatermark))
			{
				BuildHeaderFooter("{" + c_slashSymbol + "headerf", headerFooters.FirstPageHeader.Items, GetWatermark(headerFooters.FirstPageHeader.Watermark), headerFooters.FirstPageHeader.WriteWatermark);
			}
			if (headerFooters.FirstPageFooter.Items.Count > 0)
			{
				BuildHeaderFooter("{" + c_slashSymbol + "footerf", headerFooters.FirstPageFooter.Items, string.Empty, writeWaterMark: false);
			}
		}
	}

	private string GetWatermark(Watermark waterMark)
	{
		if (waterMark != null && waterMark.Type != 0 && (!(waterMark is PictureWatermark) || (waterMark as PictureWatermark).Picture != null))
		{
			if (waterMark.Type == WatermarkType.TextWatermark)
			{
				return BuildTextWtrmarkBody(waterMark as TextWatermark);
			}
			return BuildPictWtrmarkBody(waterMark as PictureWatermark);
		}
		return string.Empty;
	}

	private void BuildHeaderFooter(string name, BodyItemCollection collect, string watermarkStr, bool writeWaterMark)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes(name);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (writeWaterMark)
		{
			bytes = m_encoding.GetBytes(watermarkStr);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = BuildBodyItems(collect);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		m_mainBodyBytesList.Add(memoryStream.ToArray());
	}

	private byte[] BuildParagraph(WParagraph para)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes(BuildListText(para, para.GetListFormatValue()));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (para.PreviousSibling == null || !(para.PreviousSibling is WParagraph) || !para.ParagraphFormat.Compare((para.PreviousSibling as WParagraph).ParagraphFormat) || !CompareListFormat(para) || IsPreviousParagraphHasFieldEnd(para.PreviousSibling as WParagraph) || para.ParaStyle == null || !(para.ParaStyle as Style).Compare((para.PreviousSibling as WParagraph).ParaStyle as Style))
		{
			bytes = m_encoding.GetBytes(BuildParagraphFormat(para.ParagraphFormat, para));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		ParagraphItem paragraphItem = null;
		for (int i = 0; i < para.Items.Count; i++)
		{
			paragraphItem = para.Items[i];
			bytes = BuildParagraphItem(paragraphItem);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (HasParaEnd(para))
		{
			bytes = BuildParagraphEnd(para);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes(BuildCharacterFormat(para.BreakCharacterFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
			if (para.Owner != null && para.OwnerTextBody.OwnerBase is WSection && (para.OwnerTextBody.OwnerBase as WSection).PreviousSibling == null && para.Items.Count == 0 && para.NextSibling == null && !(para.PreviousSibling is WTable) && para.Document.Sections.Count > 1)
			{
				BuildSectToken(memoryStream);
			}
		}
		return memoryStream.ToArray();
	}

	private bool CompareListFormat(WParagraph paragraph)
	{
		WListFormat listFormat = (paragraph.PreviousSibling as WParagraph).ListFormat;
		if (paragraph.ListFormat.Compare(listFormat) && ((paragraph.ListFormat.CurrentListStyle != null && listFormat.CurrentListStyle != null && paragraph.ListFormat.CurrentListStyle.Compare(listFormat.CurrentListStyle)) || (paragraph.ListFormat.CurrentListStyle == null && listFormat.CurrentListStyle == null)))
		{
			return true;
		}
		return false;
	}

	private bool IsPreviousParagraphHasFieldEnd(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i].EntityType == EntityType.FieldMark && (paragraph.Items[i] as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				return true;
			}
		}
		return false;
	}

	private byte[] BuildParagraphEnd(WParagraph para)
	{
		WSection wSection = para.GetOwnerTextBody(para) as WSection;
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildCharacterFormat(para.BreakCharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (!(para.GetOwnerTextBody(para) is HeaderFooter) && para.NextSibling == null && wSection != null && wSection.NextSibling != null)
		{
			BuildSectToken(memoryStream);
		}
		else
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "par");
		}
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private void BuildSectToken(MemoryStream memoryStream)
	{
		byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "sect");
		memoryStream.Write(bytes, 0, bytes.Length);
	}

	private string BuildCharacterFormat(WCharacterFormat cFormat)
	{
		if (cFormat == null)
		{
			return string.Empty;
		}
		WCharacterFormat wCharacterFormat = null;
		if (!string.IsNullOrEmpty(cFormat.CharStyleName) && m_doc.Styles.FindByName(cFormat.CharStyleName) is Style { CharacterFormat: not null } style)
		{
			wCharacterFormat = style.CharacterFormat;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			stringBuilder.Append(c_slashSymbol + "rtlch" + c_slashSymbol + (cFormat.ComplexScript ? "fcs0" : "fcs1") + c_slashSymbol + "lang1033");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "rtlch" + (cFormat.HasValue(99) ? (c_slashSymbol + (cFormat.ComplexScript ? "fcs0" : "fcs1")) : ""));
		}
		if ((cFormat.Bold && cFormat.ComplexScript) || (cFormat.BoldBidi && !cFormat.ComplexScript))
		{
			stringBuilder.Append(c_slashSymbol + "ab");
		}
		if ((cFormat.Italic && cFormat.ComplexScript) || (cFormat.ItalicBidi && !cFormat.ComplexScript))
		{
			stringBuilder.Append(c_slashSymbol + "ai");
		}
		if (cFormat.Bidi && cFormat.HasValue(58) && !m_isField)
		{
			stringBuilder.Insert(0, WriteFontNameBidi(cFormat));
		}
		else
		{
			stringBuilder.Append(WriteFontNameBidi(cFormat));
		}
		if (cFormat.OwnerBase is WParagraphStyle)
		{
			stringBuilder.Append(c_slashSymbol + "afs");
			stringBuilder.Append((short)Math.Round(cFormat.FontSizeBidi * 2f));
		}
		else if (cFormat.HasValue(3))
		{
			stringBuilder.Append(c_slashSymbol + "afs");
			stringBuilder.Append((short)Math.Round(cFormat.FontSizeBidi * 2f));
		}
		string empty = string.Empty;
		empty = (m_doc.SaveOptions.OptimizeRtfFileSize ? (c_slashSymbol + "ltrch" + (cFormat.HasValue(99) ? (c_slashSymbol + (cFormat.ComplexScript ? "fcs1" : "fcs0")) : "")) : (c_slashSymbol + "ltrch" + c_slashSymbol + (cFormat.ComplexScript ? "fcs1" : "fcs0") + c_slashSymbol + "lang1033"));
		if (cFormat.Bidi && cFormat.HasValue(58) && !m_isField)
		{
			stringBuilder.Insert(0, empty);
		}
		else
		{
			stringBuilder.Append(empty);
		}
		stringBuilder.Append(WriteFontName(cFormat));
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && cFormat.HasValue(18)) || (cFormat.HasValue(18) && cFormat.CharacterSpacing != 0f))
		{
			short num = (short)Math.Round(cFormat.CharacterSpacing * 4f);
			stringBuilder.Append(c_slashSymbol + "expnd" + num);
			short num2 = (short)Math.Round(cFormat.CharacterSpacing * 20f);
			stringBuilder.Append(c_slashSymbol + "expndtw" + num2);
		}
		if (cFormat.OwnerBase is WParagraphStyle)
		{
			stringBuilder.Append(c_slashSymbol + "fs");
			stringBuilder.Append((short)Math.Round(cFormat.FontSize * 2f));
		}
		else if (cFormat.HasValue(3))
		{
			stringBuilder.Append(c_slashSymbol + "fs");
			stringBuilder.Append((short)Math.Round(cFormat.FontSize * 2f));
		}
		else if (cFormat.Bidi && cFormat.HasValue(58) && !m_isField && cFormat.OwnerBase is WTextRange)
		{
			stringBuilder.Append(c_slashSymbol + "fs");
			stringBuilder.Append((short)Math.Round(cFormat.FontSizeBidi * 2f));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			if (cFormat.HasValue(73))
			{
				stringBuilder.Append(c_slashSymbol + "lang" + cFormat.LocaleIdASCII);
			}
			if (cFormat.HasValue(74))
			{
				stringBuilder.Append(c_slashSymbol + "langfenp" + cFormat.LocaleIdFarEast);
			}
			if (cFormat.HasValue(74))
			{
				stringBuilder.Append(c_slashSymbol + "langfe" + cFormat.LocaleIdBidi);
			}
		}
		if (cFormat.HasValue(17))
		{
			if (cFormat.Position > 0f)
			{
				stringBuilder.Append(c_slashSymbol + "up");
				stringBuilder.Append(cFormat.Position * 2f);
			}
			if (cFormat.Position < 0f)
			{
				stringBuilder.Append(c_slashSymbol + "dn");
				stringBuilder.Append((0f - cFormat.Position) * 2f);
			}
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && cFormat.HasValue(127)) || (cFormat.HasValue(127) && cFormat.Scaling != 100f))
		{
			stringBuilder.Append(c_slashSymbol + "charscalex");
			stringBuilder.Append(cFormat.Scaling);
		}
		if (!string.IsNullOrEmpty(cFormat.CharStyleName) && Styles.ContainsKey(cFormat.CharStyleName))
		{
			stringBuilder.Append(Styles[cFormat.CharStyleName]);
		}
		bool flag = false;
		if ((!cFormat.Bold || !cFormat.ComplexScript || cFormat.BoldBidi) && (cFormat.Bold || cFormat.ComplexScript || !cFormat.BoldBidi))
		{
			if (!m_doc.SaveOptions.OptimizeRtfFileSize)
			{
				if ((flag = cFormat.Bold) || cFormat.HasValue(4) || (flag = cFormat.BoldBidi) || cFormat.HasValue(59))
				{
					stringBuilder.Append(c_slashSymbol + (flag ? "b" : "b0"));
				}
			}
			else if ((cFormat.HasValue(4) && (flag = cFormat.Bold)) || (cFormat.HasValue(59) && (flag = cFormat.BoldBidi)))
			{
				stringBuilder.Append(c_slashSymbol + "b");
			}
		}
		else if (cFormat.HasValue(4) && !cFormat.Bold)
		{
			stringBuilder.Append(c_slashSymbol + "b0");
		}
		if ((!cFormat.Italic || !cFormat.ComplexScript || cFormat.ItalicBidi) && (cFormat.Italic || cFormat.ComplexScript || !cFormat.ItalicBidi))
		{
			if (!m_doc.SaveOptions.OptimizeRtfFileSize)
			{
				if ((flag = cFormat.Italic) || cFormat.HasValue(5) || (flag = cFormat.ItalicBidi) || cFormat.HasValue(60))
				{
					stringBuilder.Append(c_slashSymbol + (flag ? "i" : "i0"));
				}
			}
			else if ((cFormat.HasValue(5) && (flag = cFormat.Italic)) || (cFormat.HasValue(60) && (flag = cFormat.ItalicBidi)))
			{
				stringBuilder.Append(c_slashSymbol + "i");
			}
		}
		else if (cFormat.HasValue(5) && !cFormat.Italic)
		{
			stringBuilder.Append(c_slashSymbol + "i0");
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && cFormat.HasValue(7)) || (cFormat.HasValue(7) && cFormat.UnderlineStyle != 0))
		{
			BuildUnderLineStyle(cFormat.UnderlineStyle, stringBuilder);
		}
		else if ((!m_doc.SaveOptions.OptimizeRtfFileSize && wCharacterFormat != null && wCharacterFormat.HasValue(7)) || (wCharacterFormat != null && wCharacterFormat.HasValue(7) && cFormat.UnderlineStyle != 0))
		{
			BuildUnderLineStyle(wCharacterFormat.UnderlineStyle, stringBuilder);
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.Strikeout) || cFormat.HasValue(6))) || (cFormat.HasValue(6) && (flag = cFormat.Strikeout)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "strike" : "strike0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.DoubleStrike) || cFormat.HasValue(14))) || (cFormat.HasValue(14) && (flag = cFormat.DoubleStrike)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "striked1" : "striked0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.Shadow) || cFormat.HasValue(50))) || (cFormat.HasValue(50) && (flag = cFormat.Shadow)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "shad" : "shad0"));
		}
		if (cFormat.HasValue(10))
		{
			if (cFormat.SubSuperScript == SubSuperScript.SuperScript)
			{
				stringBuilder.Append(c_slashSymbol + "super");
			}
			else if (cFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				stringBuilder.Append(c_slashSymbol + "sub");
			}
			else if (!m_doc.SaveOptions.OptimizeRtfFileSize && cFormat.SubSuperScript == SubSuperScript.None)
			{
				stringBuilder.Append(c_slashSymbol + "nosupersub");
			}
		}
		else if (wCharacterFormat != null && wCharacterFormat.HasValue(10))
		{
			if (wCharacterFormat.SubSuperScript == SubSuperScript.SuperScript)
			{
				stringBuilder.Append(c_slashSymbol + "super");
			}
			else if (wCharacterFormat.SubSuperScript == SubSuperScript.SubScript)
			{
				stringBuilder.Append(c_slashSymbol + "sub");
			}
			else if (!m_doc.SaveOptions.OptimizeRtfFileSize && wCharacterFormat.SubSuperScript == SubSuperScript.None)
			{
				stringBuilder.Append(c_slashSymbol + "nosupersub");
			}
		}
		Color baseCFormatColor = Color.Empty;
		if (wCharacterFormat != null)
		{
			baseCFormatColor = wCharacterFormat.TextColor;
		}
		stringBuilder.Append(BuildColorValue(cFormat, cFormat.TextColor, wCharacterFormat, baseCFormatColor, 1, c_slashSymbol + "cf"));
		if (wCharacterFormat != null)
		{
			baseCFormatColor = wCharacterFormat.TextBackgroundColor;
		}
		stringBuilder.Append(BuildColorValue(cFormat, cFormat.TextBackgroundColor, wCharacterFormat, baseCFormatColor, 9, c_slashSymbol + "chcbpat"));
		if (wCharacterFormat != null)
		{
			baseCFormatColor = wCharacterFormat.ForeColor;
		}
		stringBuilder.Append(BuildColorValue(cFormat, cFormat.ForeColor, wCharacterFormat, baseCFormatColor, 77, c_slashSymbol + "chcfpat"));
		if (wCharacterFormat != null)
		{
			baseCFormatColor = wCharacterFormat.HighlightColor;
		}
		stringBuilder.Append(BuildColorValue(cFormat, cFormat.HighlightColor, wCharacterFormat, baseCFormatColor, 63, c_slashSymbol + "highlight"));
		stringBuilder.Append(BuildTextBorder(cFormat.Border));
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.SmallCaps) || cFormat.HasValue(55))) || (cFormat.HasValue(55) && (flag = cFormat.SmallCaps)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "scaps" : "scaps0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.Hidden) || cFormat.HasValue(53))) || (cFormat.HasValue(53) && (flag = cFormat.Hidden)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "v" : "v0"));
		}
		if ((flag = cFormat.OutLine) || cFormat.HasValue(71))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "outl" : "outl0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.AllCaps) || cFormat.HasValue(54))) || (cFormat.HasValue(54) && (flag = cFormat.AllCaps)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "caps" : "caps0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.Emboss) || cFormat.HasValue(51))) || (cFormat.HasValue(51) && (flag = cFormat.Emboss)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "embo" : "embo0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.Engrave) || cFormat.HasValue(52))) || (cFormat.HasValue(52) && (flag = cFormat.Engrave)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "impr" : "impr0"));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && ((flag = cFormat.SpecVanish) || cFormat.HasValue(24))) || (cFormat.HasValue(24) && (flag = cFormat.SpecVanish)))
		{
			stringBuilder.Append(c_slashSymbol + (flag ? "spv" : "spv0"));
		}
		return stringBuilder.ToString();
	}

	private string GetParagraphAlignment(HorizontalAlignment hAlignment)
	{
		switch ((byte)hAlignment)
		{
		case 1:
			return "qc";
		case 2:
			return "qr";
		case 3:
		case 6:
			return "qj";
		case 4:
			return "qd";
		case 5:
			return "qk10";
		case 7:
			return "qk20";
		case 8:
			return "qk0";
		case 9:
			return "qt";
		default:
			return "ql";
		}
	}

	private string BuildParagraphFormat(WParagraphFormat pFormat, WParagraph para, bool isParaText)
	{
		if (pFormat == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string value = string.Empty;
		string empty = string.Empty;
		if (para != null)
		{
			empty = para.StyleName;
			if (string.IsNullOrEmpty(empty))
			{
				empty = "Normal";
			}
			if (!Styles.ContainsKey(empty))
			{
				BuildStyle(empty);
			}
			if (Styles.ContainsKey(empty))
			{
				value = Styles[empty];
			}
		}
		WParagraphFormat wParagraphFormat = null;
		if (para != null && !string.IsNullOrEmpty(para.StyleName) && m_doc.Styles.FindByName(para.StyleName) is Style style && style is WParagraphStyle && (style as WParagraphStyle).ParagraphFormat != null)
		{
			wParagraphFormat = (style as WParagraphStyle).ParagraphFormat;
		}
		if (isParaText)
		{
			stringBuilder.Append(c_slashSymbol + "pard");
			stringBuilder.Append(c_slashSymbol + "plain");
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			stringBuilder.Append(c_slashSymbol + "lang1033");
		}
		if (pFormat.WidowControl)
		{
			stringBuilder.Append(c_slashSymbol + "widctlpar");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "nowidctlpar");
		}
		stringBuilder.Append(value);
		if (pFormat.Bidi)
		{
			stringBuilder.Append(c_slashSymbol + "rtlpar");
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || (m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.HorizontalAlignment != 0))
		{
			stringBuilder.Append(c_slashSymbol + GetParagraphAlignment(pFormat.HorizontalAlignment));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || pFormat.IsFrame)
		{
			if (pFormat.HasValue(71))
			{
				switch (pFormat.FrameHorizontalPos)
				{
				case 0:
					stringBuilder.Append(c_slashSymbol + "phcol");
					break;
				case 1:
					stringBuilder.Append(c_slashSymbol + "phmrg");
					break;
				case 2:
					stringBuilder.Append(c_slashSymbol + "phpg");
					break;
				}
			}
			if (pFormat.HasValue(72))
			{
				switch (pFormat.FrameVerticalPos)
				{
				case 0:
					stringBuilder.Append(c_slashSymbol + "pvmrg");
					break;
				case 1:
					stringBuilder.Append(c_slashSymbol + "pvpg");
					break;
				case 2:
					stringBuilder.Append(c_slashSymbol + "pvpara");
					break;
				}
			}
			if (pFormat.HasValue(76))
			{
				int num = (int)Math.Round(pFormat.FrameWidth * 20f);
				stringBuilder.Append(c_slashSymbol + "absw" + num);
			}
			if (pFormat.HasValue(77))
			{
				ushort num2 = (ushort)Math.Round(pFormat.FrameHeight * 20f);
				if ((num2 & 0x8000u) != 0)
				{
					int num3 = num2 & 0x7FFF;
					stringBuilder.Append(c_slashSymbol + "absh" + num3);
				}
				else
				{
					stringBuilder.Append(c_slashSymbol + "absh-" + num2);
				}
			}
			if (pFormat.HasValue(73))
			{
				if (pFormat.IsFrameXAlign(pFormat.FrameX))
				{
					switch ((PageNumberAlignment)(int)pFormat.FrameX)
					{
					case PageNumberAlignment.Center:
						stringBuilder.Append(c_slashSymbol + "posxc");
						break;
					case PageNumberAlignment.Inside:
						stringBuilder.Append(c_slashSymbol + "posxi");
						break;
					case PageNumberAlignment.Left:
						stringBuilder.Append(c_slashSymbol + "posxl");
						break;
					case PageNumberAlignment.Outside:
						stringBuilder.Append(c_slashSymbol + "posxo");
						break;
					case PageNumberAlignment.Right:
						stringBuilder.Append(c_slashSymbol + "posxr");
						break;
					}
				}
				else
				{
					int num4 = (int)Math.Round(pFormat.FrameX * 20f);
					if (num4 < 0)
					{
						stringBuilder.Append(c_slashSymbol + "posnegx" + num4);
					}
					else
					{
						stringBuilder.Append(c_slashSymbol + "posx" + num4);
					}
				}
			}
			if (pFormat.HasValue(74))
			{
				if (pFormat.IsFrameYAlign(pFormat.FrameY))
				{
					switch ((FrameVerticalPosition)(int)pFormat.FrameY)
					{
					case FrameVerticalPosition.Bottom:
						stringBuilder.Append(c_slashSymbol + "posyb");
						break;
					case FrameVerticalPosition.Center:
						stringBuilder.Append(c_slashSymbol + "posyc");
						break;
					case FrameVerticalPosition.Inline:
						stringBuilder.Append(c_slashSymbol + "posyil");
						break;
					case FrameVerticalPosition.Inside:
						stringBuilder.Append(c_slashSymbol + "posyin");
						break;
					case FrameVerticalPosition.Outside:
						stringBuilder.Append(c_slashSymbol + "posyout");
						break;
					case FrameVerticalPosition.Top:
						stringBuilder.Append(c_slashSymbol + "posyt");
						break;
					}
				}
				else
				{
					int num5 = (int)Math.Round(pFormat.FrameY * 20f);
					if (num5 < 0)
					{
						stringBuilder.Append(c_slashSymbol + "posnegy" + num5);
					}
					else
					{
						stringBuilder.Append(c_slashSymbol + "posy" + num5);
					}
				}
			}
			if (pFormat.HasValue(88))
			{
				switch (pFormat.WrapFrameAround)
				{
				case FrameWrapMode.Around:
					stringBuilder.Append(c_slashSymbol + "wraparound");
					break;
				case FrameWrapMode.Tight:
					stringBuilder.Append(c_slashSymbol + "wraptight");
					break;
				case FrameWrapMode.Through:
					stringBuilder.Append(c_slashSymbol + "wrapthrough");
					break;
				case FrameWrapMode.None:
					stringBuilder.Append(c_slashSymbol + "nowrap");
					break;
				case FrameWrapMode.Auto:
					stringBuilder.Append(c_slashSymbol + "wrapdefault");
					break;
				}
			}
			if (pFormat.HasValue(83))
			{
				int num6 = (int)Math.Round(pFormat.FrameHorizontalDistanceFromText * 20f);
				stringBuilder.Append(c_slashSymbol + "dfrmtxtx" + num6);
			}
			if (pFormat.HasValue(84))
			{
				int num7 = (int)Math.Round(pFormat.FrameVerticalDistanceFromText * 20f);
				stringBuilder.Append(c_slashSymbol + "dfrmtxty" + num7);
			}
		}
		int num8 = (int)Math.Round(pFormat.FirstLineIndent * 20f);
		int num9 = (int)Math.Round(pFormat.LeftIndent * 20f);
		int num10 = (int)Math.Round(pFormat.RightIndent * 20f);
		if (para != null && !para.ListFormat.IsEmptyList)
		{
			WListFormat listFormatValue = para.GetListFormatValue();
			if (listFormatValue != null && listFormatValue.CurrentListStyle != null)
			{
				ListStyle currentListStyle = listFormatValue.CurrentListStyle;
				WListLevel listLevel = para.GetListLevel(listFormatValue);
				if (currentListStyle.ListType == ListType.Numbered || currentListStyle.ListType == ListType.Bulleted)
				{
					if (listLevel.ParagraphFormat.HasValue(5))
					{
						num8 = (int)Math.Round(listLevel.ParagraphFormat.FirstLineIndent * 20f);
					}
					if (listLevel.ParagraphFormat.HasValue(2))
					{
						num9 = (int)Math.Round(listLevel.ParagraphFormat.LeftIndent * 20f);
					}
					if (listLevel.ParagraphFormat.HasValue(3))
					{
						num10 = (int)Math.Round(listLevel.ParagraphFormat.RightIndent * 20f);
					}
				}
			}
		}
		if (pFormat.HasValue(5))
		{
			num8 = (int)Math.Round(pFormat.FirstLineIndent * 20f);
		}
		if (pFormat.HasValue(2))
		{
			num9 = (int)Math.Round(pFormat.LeftIndent * 20f);
		}
		if (pFormat.HasValue(3))
		{
			num10 = (int)Math.Round(pFormat.RightIndent * 20f);
		}
		if (pFormat.Bidi && (pFormat.Document.ActualFormatType == FormatType.Doc || pFormat.Document.ActualFormatType == FormatType.Docx || pFormat.Document.ActualFormatType == FormatType.Rtf))
		{
			int num11 = num9;
			num9 = num10;
			num10 = num11;
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || num8 != 0)
		{
			stringBuilder.Append(c_slashSymbol + "fi");
			stringBuilder.Append(num8);
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || num9 != 0)
		{
			stringBuilder.Append(c_slashSymbol + "li");
			stringBuilder.Append(num9);
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || num10 != 0)
		{
			stringBuilder.Append(c_slashSymbol + "ri");
			stringBuilder.Append(num10);
		}
		if (pFormat.SuppressAutoHyphens)
		{
			stringBuilder.Append(c_slashSymbol + "hyphpar0");
		}
		if (pFormat.MirrorIndents)
		{
			stringBuilder.Append(c_slashSymbol + "indmirror");
		}
		stringBuilder.Append(BuildFrameProps(pFormat));
		bool flag = ((para != null && para.IsInCell) ? true : false);
		if (flag)
		{
			stringBuilder.Append(c_slashSymbol + "intbl");
		}
		stringBuilder.Append(BuildParaBorders(pFormat));
		stringBuilder.Append(BuildParaSpacing(pFormat, flag));
		if (pFormat.Keep)
		{
			stringBuilder.Append(c_slashSymbol + "keep");
		}
		if (pFormat.KeepFollow)
		{
			stringBuilder.Append(c_slashSymbol + "keepn");
		}
		if (pFormat.PageBreakBefore)
		{
			stringBuilder.Append(c_slashSymbol + "pagebb");
		}
		if ((byte)pFormat.OutlineLevel < 9)
		{
			stringBuilder.Append(c_slashSymbol + "outlinelevel");
			stringBuilder.Append((byte)pFormat.OutlineLevel);
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		if (para != null && para.ParaStyle.ParagraphFormat.HasShading())
		{
			if (!para.ParaStyle.ParagraphFormat.BackColor.IsEmpty)
			{
				text = BuildColor(para.ParaStyle.ParagraphFormat.BackColor, c_slashSymbol + "cbpat");
			}
			if (!para.ParaStyle.ParagraphFormat.ForeColor.IsEmpty)
			{
				text2 = BuildColor(para.ParaStyle.ParagraphFormat.ForeColor, c_slashSymbol + "cfpat");
			}
			text3 = BuildTextureStyle(para.ParaStyle.ParagraphFormat.TextureStyle);
		}
		if (pFormat.HasShading())
		{
			if (!pFormat.BackColor.IsEmpty)
			{
				text = BuildColor(pFormat.BackColor, c_slashSymbol + "cbpat");
			}
			if (!pFormat.ForeColor.IsEmpty)
			{
				text2 = BuildColor(pFormat.ForeColor, c_slashSymbol + "cfpat");
			}
			text3 = BuildTextureStyle(pFormat.TextureStyle);
		}
		if (text != string.Empty)
		{
			stringBuilder.Append(text);
		}
		if (text2 != string.Empty)
		{
			stringBuilder.Append(text2);
		}
		if (text3 != string.Empty)
		{
			stringBuilder.Append(text3);
		}
		stringBuilder.Append(BuildParaListId(para, pFormat));
		TabCollection tabCollection = wParagraphFormat?.Tabs;
		tabCollection = ((pFormat.Tabs.Count > 0) ? pFormat.Tabs : tabCollection);
		stringBuilder.Append(BuildTabs(tabCollection));
		WListFormat wListFormat = para?.GetListFormatValue();
		tabCollection = ((wListFormat != null && wListFormat.ListType != ListType.NoList && wListFormat.CurrentListLevel.ParagraphFormat.Tabs.Count > 0) ? wListFormat.CurrentListLevel.ParagraphFormat.Tabs : null);
		stringBuilder.Append(BuildTabs(tabCollection));
		if (para != null && para.ParaStyle != null)
		{
			WCharacterFormat characterFormat = para.ParaStyle.CharacterFormat;
			stringBuilder.Append(BuildCharacterFormat(characterFormat));
		}
		else if (m_doc.DefCharFormat != null)
		{
			stringBuilder.Append(BuildCharacterFormat(m_doc.DefCharFormat));
		}
		if (wParagraphFormat != null && wParagraphFormat.ContextualSpacing)
		{
			stringBuilder.Append(c_slashSymbol + "contextualspace");
		}
		if (para != null && m_tableNestedLevel > 1)
		{
			int num12 = ((m_tableNestedLevel <= 0) ? 1 : m_tableNestedLevel);
			stringBuilder.Append(c_slashSymbol + "itap" + num12);
		}
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildParagraphFormat(WParagraphFormat pFormat, WParagraph para)
	{
		return BuildParagraphFormat(pFormat, para, isParaText: true);
	}

	private string BuildParaSpacing(WParagraphFormat pFormat, bool isInCell)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.HasValueWithParent(8)) || (pFormat.HasValueWithParent(8) && pFormat.BeforeSpacing != 0f))
		{
			stringBuilder.Append(BuildSpacing(c_slashSymbol + "sb", pFormat.BeforeSpacing));
		}
		else if (!m_doc.SaveOptions.OptimizeRtfFileSize && m_doc.m_defParaFormat != null && m_doc.m_defParaFormat.HasValue(8) && !isInCell)
		{
			stringBuilder.Append(BuildSpacing(c_slashSymbol + "sb", m_doc.m_defParaFormat.BeforeSpacing));
		}
		if (pFormat.HasValueWithParent(9))
		{
			stringBuilder.Append(BuildSpacing(c_slashSymbol + "sa", pFormat.AfterSpacing));
		}
		else if (m_doc.m_defParaFormat != null && m_doc.m_defParaFormat.HasValue(9) && !isInCell)
		{
			stringBuilder.Append(BuildSpacing(c_slashSymbol + "sa", m_doc.m_defParaFormat.AfterSpacing));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.HasValueWithParent(54)) || (pFormat.HasValueWithParent(54) && pFormat.SpaceBeforeAuto))
		{
			stringBuilder.Append(BuildAutoSpacing(c_slashSymbol + "sbauto", pFormat.SpaceBeforeAuto));
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.HasValueWithParent(55)) || (pFormat.HasValueWithParent(55) && pFormat.SpaceAfterAuto))
		{
			stringBuilder.Append(BuildAutoSpacing(c_slashSymbol + "saauto", pFormat.SpaceAfterAuto));
		}
		if (pFormat.HasValueWithParent(52))
		{
			stringBuilder.Append(BuildLineSpacing(pFormat));
		}
		else if (m_doc.m_defParaFormat != null && m_doc.m_defParaFormat.HasValue(52) && !isInCell)
		{
			stringBuilder.Append(BuildLineSpacing(m_doc.m_defParaFormat));
		}
		if (pFormat.ContextualSpacing)
		{
			stringBuilder.Append(c_slashSymbol + "contextualspace");
		}
		return stringBuilder.ToString();
	}

	private string BuildSpacing(string attribute, float value)
	{
		return attribute + (int)Math.Round(value * 20f);
	}

	private string BuildAutoSpacing(string value, bool hasSpacing)
	{
		if (hasSpacing)
		{
			return value + "1";
		}
		return value + "0";
	}

	private string BuildLineSpacing(WParagraphFormat pFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(c_slashSymbol + "sl");
		int value = (int)Math.Abs(Math.Round(pFormat.LineSpacing * 20f));
		if (pFormat.LineSpacingRule == LineSpacingRule.Exactly)
		{
			stringBuilder.Append("-" + value);
		}
		else
		{
			stringBuilder.Append(value);
		}
		switch (pFormat.LineSpacingRule)
		{
		case LineSpacingRule.AtLeast:
		case LineSpacingRule.Exactly:
			stringBuilder.Append(c_slashSymbol + "slmult0");
			break;
		case LineSpacingRule.Multiple:
			stringBuilder.Append(c_slashSymbol + "slmult1");
			break;
		}
		return stringBuilder.ToString();
	}

	private string BuildTextureStyle(TextureStyle style)
	{
		return style switch
		{
			TextureStyle.Texture5Percent => c_slashSymbol + "shading500", 
			TextureStyle.Texture2Pt5Percent => c_slashSymbol + "shading250", 
			TextureStyle.Texture7Pt5Percent => c_slashSymbol + "shading750", 
			TextureStyle.Texture10Percent => c_slashSymbol + "shading1000", 
			TextureStyle.Texture12Pt5Percent => c_slashSymbol + "shading1250", 
			TextureStyle.Texture15Percent => c_slashSymbol + "shading1500", 
			TextureStyle.Texture17Pt5Percent => c_slashSymbol + "shading1750", 
			TextureStyle.Texture20Percent => c_slashSymbol + "shading2000", 
			TextureStyle.Texture25Percent => c_slashSymbol + "shading2500", 
			TextureStyle.Texture27Pt5Percent => c_slashSymbol + "shading2750", 
			TextureStyle.Texture30Percent => c_slashSymbol + "shading3000", 
			TextureStyle.Texture32Pt5Percent => c_slashSymbol + "shading3250", 
			TextureStyle.Texture35Percent => c_slashSymbol + "shading3500", 
			TextureStyle.Texture37Pt5Percent => c_slashSymbol + "shading3750", 
			TextureStyle.Texture40Percent => c_slashSymbol + "shading4000", 
			TextureStyle.Texture42Pt5Percent => c_slashSymbol + "shading4250", 
			TextureStyle.Texture45Percent => c_slashSymbol + "shading4500", 
			TextureStyle.Texture47Pt5Percent => c_slashSymbol + "shading4750", 
			TextureStyle.Texture50Percent => c_slashSymbol + "shading5000", 
			TextureStyle.Texture52Pt5Percent => c_slashSymbol + "shading5250", 
			TextureStyle.Texture55Percent => c_slashSymbol + "shading5500", 
			TextureStyle.Texture57Pt5Percent => c_slashSymbol + "shading5750", 
			TextureStyle.Texture60Percent => c_slashSymbol + "shading6000", 
			TextureStyle.Texture62Pt5Percent => c_slashSymbol + "shading6250", 
			TextureStyle.Texture65Percent => c_slashSymbol + "shading6500", 
			TextureStyle.Texture67Pt5Percent => c_slashSymbol + "shading6750", 
			TextureStyle.Texture70Percent => c_slashSymbol + "shading7000", 
			TextureStyle.Texture72Pt5Percent => c_slashSymbol + "shading7250", 
			TextureStyle.Texture75Percent => c_slashSymbol + "shading7500", 
			TextureStyle.Texture77Pt5Percent => c_slashSymbol + "shading7750", 
			TextureStyle.Texture80Percent => c_slashSymbol + "shading8000", 
			TextureStyle.Texture82Pt5Percent => c_slashSymbol + "shading8250", 
			TextureStyle.Texture85Percent => c_slashSymbol + "shading8500", 
			TextureStyle.Texture87Pt5Percent => c_slashSymbol + "shading8750", 
			TextureStyle.Texture90Percent => c_slashSymbol + "shading9000", 
			TextureStyle.Texture92Pt5Percent => c_slashSymbol + "shading9250", 
			TextureStyle.Texture95Percent => c_slashSymbol + "shading9500", 
			TextureStyle.Texture97Pt5Percent => c_slashSymbol + "shading9750", 
			TextureStyle.TextureCross => c_slashSymbol + "bgcross", 
			TextureStyle.TextureDarkCross => c_slashSymbol + "bgdkcross", 
			TextureStyle.TextureDarkDiagonalCross => c_slashSymbol + "bgdkdcross", 
			TextureStyle.TextureDarkDiagonalDown => c_slashSymbol + "bgdkbdiag", 
			TextureStyle.TextureDarkDiagonalUp => c_slashSymbol + "bgdkfdiag", 
			TextureStyle.TextureDarkHorizontal => c_slashSymbol + "bgdkhoriz", 
			TextureStyle.TextureDarkVertical => c_slashSymbol + "bgdkvert", 
			TextureStyle.TextureDiagonalCross => c_slashSymbol + "bgdcross", 
			TextureStyle.TextureDiagonalDown => c_slashSymbol + "bgbdiag", 
			TextureStyle.TextureDiagonalUp => c_slashSymbol + "bgfdiag", 
			TextureStyle.TextureHorizontal => c_slashSymbol + "bghoriz", 
			TextureStyle.TextureVertical => c_slashSymbol + "bgvert", 
			_ => string.Empty, 
		};
	}

	private void BuildSectionProp(WSection section)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "sectd");
		memoryStream.Write(bytes, 0, bytes.Length);
		switch (section.BreakCode)
		{
		case SectionBreakCode.EvenPage:
			bytes = m_encoding.GetBytes(c_slashSymbol + "sbkeven");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case SectionBreakCode.Oddpage:
			bytes = m_encoding.GetBytes(c_slashSymbol + "sbkodd");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case SectionBreakCode.NewColumn:
			bytes = m_encoding.GetBytes(c_slashSymbol + "sbkcol");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case SectionBreakCode.NoBreak:
			bytes = m_encoding.GetBytes(c_slashSymbol + "sbknone");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		}
		if (section.PageSetup.Orientation == PageOrientation.Landscape)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "lndscpsxn");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (section.TextDirection == DocTextDirection.LeftToRight)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "ltrsect");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (section.TextDirection == DocTextDirection.RightToLeft)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "rtlsect");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (section.PageSetup.FirstPageTray > PrinterPaperTray.DefaultBin)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "binfsxn" + section.PageSetup.FirstPageTray);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (section.PageSetup.OtherPagesTray > PrinterPaperTray.DefaultBin)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "binsxn" + section.PageSetup.OtherPagesTray);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "nofeaturethrottle1");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "formshade");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "splytwnine");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = BuildPageSetup(section.PageSetup);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		m_mainBodyBytesList.Add(memoryStream.ToArray());
	}

	private byte[] BuildPageSetup(WPageSetup pSetup)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes;
		if (pSetup.HeaderDistance >= 0f)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "headery" + (int)Math.Round(pSetup.HeaderDistance * 20f));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (pSetup.FooterDistance >= 0f)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "footery" + (int)Math.Round(pSetup.FooterDistance * 20f));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		switch (pSetup.VerticalAlignment)
		{
		case PageAlignment.Top:
			bytes = m_encoding.GetBytes(c_slashSymbol + "vertalt");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case PageAlignment.Bottom:
			bytes = m_encoding.GetBytes(c_slashSymbol + "vertalb");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case PageAlignment.Middle:
			bytes = m_encoding.GetBytes(c_slashSymbol + "vertalc");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case PageAlignment.Justified:
			bytes = m_encoding.GetBytes(c_slashSymbol + "vertalj");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		}
		if (pSetup.DifferentFirstPage)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "titlepg");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (m_doc.DifferentOddAndEvenPages)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "facingp");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "paperw" + (int)Math.Round(pSetup.PageSize.Width * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "paperh" + (int)Math.Round(pSetup.PageSize.Height * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margl" + (int)Math.Round(pSetup.Margins.Left * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margr" + (int)Math.Round(pSetup.Margins.Right * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margt" + (int)Math.Round(pSetup.Margins.Top * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "margb" + (int)Math.Round(pSetup.Margins.Bottom * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "gutter" + (int)Math.Round(pSetup.Margins.Gutter * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "deftab" + (int)Math.Round(m_doc.DefaultTabWidth * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (m_doc.DOP.GutterAtTop)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "gutterprl");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		switch (m_doc.MultiplePage)
		{
		case MultiplePage.MirrorMargins:
			bytes = m_encoding.GetBytes(c_slashSymbol + "margmirror");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case MultiplePage.BookFold:
			bytes = m_encoding.GetBytes(c_slashSymbol + "bookfold");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case MultiplePage.ReverseBookFold:
			bytes = m_encoding.GetBytes(c_slashSymbol + "bookfoldrev");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case MultiplePage.TwoPagesPerSheet:
			bytes = m_encoding.GetBytes(c_slashSymbol + "twoonone");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		}
		if (m_doc.SheetsPerBooklet != 0)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "bookfoldsheets" + m_doc.SheetsPerBooklet);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (pSetup.RestartPageNumbering)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "pgnrestart");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "pgnstarts" + pSetup.PageStartingNumber);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(BuildPageNumStyle(pSetup.PageNumberStyle));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "pgncont");
		memoryStream.Write(bytes, 0, bytes.Length);
		if (pSetup.LineNumberingMode != LineNumberingMode.None || pSetup.LineNumberingStep > 0)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "linemod" + pSetup.LineNumberingStep);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "linex" + pSetup.LineNumberingDistanceFromText);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "linestarts" + pSetup.LineNumberingStartValue);
			memoryStream.Write(bytes, 0, bytes.Length);
			switch (pSetup.LineNumberingMode)
			{
			case LineNumberingMode.Continuous:
				bytes = m_encoding.GetBytes(c_slashSymbol + "linecont");
				memoryStream.Write(bytes, 0, bytes.Length);
				break;
			case LineNumberingMode.RestartPage:
				bytes = m_encoding.GetBytes(c_slashSymbol + "lineppage");
				memoryStream.Write(bytes, 0, bytes.Length);
				break;
			case LineNumberingMode.RestartSection:
				bytes = m_encoding.GetBytes(c_slashSymbol + "linerestart");
				memoryStream.Write(bytes, 0, bytes.Length);
				break;
			}
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "sectlinegrid" + (int)Math.Round(pSetup.LinePitch * 20f));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (pSetup.PitchType == GridPitchType.LinesOnly)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "sectspecifyl");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(BuildPageBorders(pSetup.Borders));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = BuildColumns((pSetup.OwnerBase as WSection).Columns);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private string BuildPageNumStyle(PageNumberStyle pageNumSt)
	{
		return pageNumSt switch
		{
			PageNumberStyle.LetterLower => c_slashSymbol + "pgnlcltr", 
			PageNumberStyle.LetterUpper => c_slashSymbol + "pgnucltr", 
			PageNumberStyle.RomanLower => c_slashSymbol + "pgnlcrm", 
			PageNumberStyle.RomanUpper => c_slashSymbol + "pgnucrm", 
			_ => c_slashSymbol + "pgndec", 
		};
	}

	private byte[] BuildColumns(ColumnCollection cols)
	{
		MemoryStream memoryStream = new MemoryStream();
		new StringBuilder();
		WPageSetup pageSetup = cols.OwnerSection.PageSetup;
		byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "cols" + cols.Count);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (!pageSetup.EqualColumnWidth && cols.Count != 1)
		{
			int i = 0;
			for (int count = cols.Count; i < count; i++)
			{
				Column column = cols[i];
				bytes = m_encoding.GetBytes(c_slashSymbol + "colno" + (i + 1));
				memoryStream.Write(bytes, 0, bytes.Length);
				bytes = m_encoding.GetBytes(c_slashSymbol + "colw" + (int)Math.Round(column.Width * 20f));
				memoryStream.Write(bytes, 0, bytes.Length);
				bytes = m_encoding.GetBytes(c_slashSymbol + "colsr" + (int)Math.Round(column.Space * 20f));
				memoryStream.Write(bytes, 0, bytes.Length);
			}
		}
		if (pageSetup.DrawLinesBetweenCols)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "linebetcol");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}

	private void BuildUnderLineStyle(UnderlineStyle style, StringBuilder strBuilder)
	{
		switch (style)
		{
		case UnderlineStyle.Single:
			strBuilder.Append(c_slashSymbol + "ul");
			break;
		case UnderlineStyle.Dash:
			strBuilder.Append(c_slashSymbol + "uldash");
			break;
		case UnderlineStyle.Dotted:
			strBuilder.Append(c_slashSymbol + "uld");
			break;
		case UnderlineStyle.Double:
			strBuilder.Append(c_slashSymbol + "uldb");
			break;
		case UnderlineStyle.DashLong:
			strBuilder.Append(c_slashSymbol + "ulldash");
			break;
		case UnderlineStyle.None:
			strBuilder.Append(c_slashSymbol + "ulnone");
			break;
		case UnderlineStyle.Thick:
			strBuilder.Append(c_slashSymbol + "ulth");
			break;
		case UnderlineStyle.Wavy:
			strBuilder.Append(c_slashSymbol + "ulwave");
			break;
		case UnderlineStyle.WavyDouble:
			strBuilder.Append(c_slashSymbol + "ululdbwave");
			break;
		case UnderlineStyle.WavyHeavy:
			strBuilder.Append(c_slashSymbol + "ulhwave");
			break;
		case UnderlineStyle.Words:
			strBuilder.Append(c_slashSymbol + "ulw");
			break;
		}
	}

	private string BuildTabs(TabCollection tabs)
	{
		if (tabs == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (tabs.Count > 0)
		{
			int i = 0;
			for (int count = tabs.Count; i < count; i++)
			{
				Tab tab = tabs[i];
				switch (tab.Justification)
				{
				case TabJustification.Centered:
					stringBuilder.Append(c_slashSymbol + "tqc");
					break;
				case TabJustification.Right:
					stringBuilder.Append(c_slashSymbol + "tqr");
					break;
				case TabJustification.Decimal:
					stringBuilder.Append(c_slashSymbol + "tqdec");
					break;
				}
				if (tab.TabLeader != 0)
				{
					switch (tab.TabLeader)
					{
					case TabLeader.Dotted:
						stringBuilder.Append(c_slashSymbol + "tldot");
						break;
					case TabLeader.Hyphenated:
						stringBuilder.Append(c_slashSymbol + "tlhyph");
						break;
					case TabLeader.Single:
						stringBuilder.Append(c_slashSymbol + "tlth");
						break;
					case TabLeader.Heavy:
						stringBuilder.Append(c_slashSymbol + "tleq");
						break;
					}
				}
				stringBuilder.Append(c_slashSymbol + "tx");
				stringBuilder.Append(Math.Round(tab.Position * 20f));
			}
		}
		return stringBuilder.ToString();
	}

	private string BuildParaBorders(WParagraphFormat pFormat)
	{
		Borders borders = pFormat.Borders;
		StringBuilder stringBuilder = new StringBuilder();
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.Borders.Top.IsBorderDefined) || (pFormat.Borders.Top.IsBorderDefined && pFormat.Borders.Top.BorderType != 0))
		{
			stringBuilder.Append(c_slashSymbol + "brdrt");
			stringBuilder.Append(BuildBorder(borders.Top, isTable: false));
		}
		if (pFormat.Borders.Top.Shadow)
		{
			stringBuilder.Append(c_slashSymbol + "brdrsh");
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.Borders.Left.IsBorderDefined) || (pFormat.Borders.Left.IsBorderDefined && pFormat.Borders.Left.BorderType != 0))
		{
			stringBuilder.Append(c_slashSymbol + "brdrl");
			stringBuilder.Append(BuildBorder(borders.Left, isTable: false));
		}
		if (pFormat.Borders.Left.Shadow)
		{
			stringBuilder.Append(c_slashSymbol + "brdrsh");
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.Borders.Bottom.IsBorderDefined) || (pFormat.Borders.Bottom.IsBorderDefined && pFormat.Borders.Bottom.BorderType != 0))
		{
			stringBuilder.Append(c_slashSymbol + "brdrb");
			stringBuilder.Append(BuildBorder(borders.Bottom, isTable: false));
		}
		if (pFormat.Borders.Bottom.Shadow)
		{
			stringBuilder.Append(c_slashSymbol + "brdrsh");
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && pFormat.Borders.Right.IsBorderDefined) || (pFormat.Borders.Right.IsBorderDefined && pFormat.Borders.Right.BorderType != 0))
		{
			stringBuilder.Append(c_slashSymbol + "brdrr");
			stringBuilder.Append(BuildBorder(borders.Right, isTable: false));
		}
		if (pFormat.Borders.Right.Shadow)
		{
			stringBuilder.Append(c_slashSymbol + "brdrsh");
		}
		return stringBuilder.ToString();
	}

	private string BuildPageBorders(Borders borders)
	{
		if (borders.NoBorder)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(c_slashSymbol + "pgbrdropt32");
		stringBuilder.Append(c_slashSymbol + "pgbrdrt");
		stringBuilder.Append(BuildBorder(borders.Top, isTable: false));
		stringBuilder.Append(c_slashSymbol + "pgbrdrb");
		stringBuilder.Append(BuildBorder(borders.Bottom, isTable: false));
		stringBuilder.Append(c_slashSymbol + "pgbrdrl");
		stringBuilder.Append(BuildBorder(borders.Left, isTable: false));
		stringBuilder.Append(c_slashSymbol + "pgbrdrr");
		stringBuilder.Append(BuildBorder(borders.Right, isTable: false));
		return stringBuilder.ToString();
	}

	private string BuildBorder(Border border, bool isTable)
	{
		BorderStyle borderType = border.BorderType;
		int value = (int)(border.LineWidth * 20f);
		float num = border.Space * 20f;
		if (borderType == BorderStyle.None && !border.HasNoneStyle)
		{
			return BuildColor(Color.Black, c_slashSymbol + "brdrs" + c_slashSymbol + "brdrw10" + c_slashSymbol + "brdrcf");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (borderType != 0 && borderType != BorderStyle.Cleared)
		{
			stringBuilder.Append(BuildBorderStyle(borderType));
			stringBuilder.Append(c_slashSymbol + "brdrw");
			stringBuilder.Append(value);
			stringBuilder.Append(BuildColor(border.Color, c_slashSymbol + "brdrcf"));
		}
		if (num > 0f)
		{
			stringBuilder.Append(c_slashSymbol + "brsp");
			stringBuilder.Append(num);
		}
		return stringBuilder.ToString();
	}

	private string BuildBorderStyle(BorderStyle borderStyle)
	{
		return borderStyle switch
		{
			BorderStyle.Double => c_slashSymbol + "brdrdb", 
			BorderStyle.Dot => c_slashSymbol + "brdrdot", 
			BorderStyle.Hairline => c_slashSymbol + "brdrhair", 
			BorderStyle.DashSmallGap => c_slashSymbol + "brdrdashsm", 
			BorderStyle.DotDash => c_slashSymbol + "brdrdashd", 
			BorderStyle.DotDotDash => c_slashSymbol + "brdrdashdd", 
			BorderStyle.Inset => c_slashSymbol + "brdrinset", 
			BorderStyle.None => c_slashSymbol + "brdrnone", 
			BorderStyle.Outset => c_slashSymbol + "brdroutset", 
			BorderStyle.Triple => c_slashSymbol + "brdrtriple", 
			BorderStyle.Wave => c_slashSymbol + "brdrwavy", 
			BorderStyle.DoubleWave => c_slashSymbol + "brdrwavydb", 
			BorderStyle.Emboss3D => c_slashSymbol + "brdremboss", 
			BorderStyle.Engrave3D => c_slashSymbol + "brdrengrave", 
			BorderStyle.ThickThinMediumGap => c_slashSymbol + "brdrtnthmg", 
			BorderStyle.ThinThickMediumGap => c_slashSymbol + "brdrthtnmg", 
			BorderStyle.ThickThinLargeGap => c_slashSymbol + "brdrtnthlg", 
			BorderStyle.ThinThickLargeGap => c_slashSymbol + "brdrthtnlg", 
			BorderStyle.ThinThickSmallGap => c_slashSymbol + "brdrthtnsg", 
			BorderStyle.ThinThickThinSmallGap => c_slashSymbol + "brdrtnthtnsg", 
			BorderStyle.ThinThickThinLargeGap => c_slashSymbol + "brdrtnthtnlg", 
			BorderStyle.DashDotStroker => c_slashSymbol + "brdrdashdotstr", 
			BorderStyle.DashLargeGap => c_slashSymbol + "brdrdash", 
			BorderStyle.Thick => c_slashSymbol + "brdrth", 
			BorderStyle.ThinThinSmallGap => c_slashSymbol + "brdrtnthsg", 
			BorderStyle.ThickThickThinMediumGap => c_slashSymbol + "brdrtnthtnmg", 
			_ => c_slashSymbol + "brdrs", 
		};
	}

	private void BuildStyleSheet()
	{
		int num = 1;
		foreach (Style style3 in m_doc.Styles)
		{
			if (!StyleNumb.ContainsKey(style3.Name))
			{
				StyleNumb.Add(style3.Name, num.ToString());
				num++;
			}
		}
		foreach (Style style4 in m_doc.Styles)
		{
			BuildStyle(style4);
		}
	}

	private void BuildStyle(Style style)
	{
		if (Styles.ContainsKey(style.Name))
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(m_styleBytes, 0, m_styleBytes.Length);
		string text = string.Empty;
		if (style.StyleType == StyleType.ParagraphStyle)
		{
			if (StyleNumb.ContainsKey(style.Name))
			{
				text = c_slashSymbol + "s" + StyleNumb[style.Name];
			}
		}
		else if (style.StyleType == StyleType.CharacterStyle && StyleNumb.ContainsKey(style.Name))
		{
			text = c_slashSymbol + "cs" + StyleNumb[style.Name];
		}
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(text);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (style.StyleType == StyleType.ParagraphStyle)
		{
			WParagraphStyle wParagraphStyle = style as WParagraphStyle;
			bytes = m_encoding.GetBytes(BuildParagraphFormat(wParagraphStyle.ParagraphFormat, null, isParaText: false));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (style.StyleType == StyleType.CharacterStyle)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "additive");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(BuildCharacterFormat(style.CharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (style.BaseStyle != null && !string.IsNullOrEmpty(style.BaseStyle.Name) && StyleNumb.ContainsKey(style.BaseStyle.Name))
		{
			string text2 = StyleNumb[style.BaseStyle.Name];
			bytes = m_encoding.GetBytes(c_slashSymbol + "sbasedon" + text2);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (!string.IsNullOrEmpty(style.LinkedStyleName) && StyleNumb.ContainsKey(style.LinkedStyleName))
		{
			string text3 = StyleNumb[style.LinkedStyleName];
			bytes = m_encoding.GetBytes(c_slashSymbol + "slink" + text3);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "sqformat");
		memoryStream.Write(bytes, 0, bytes.Length);
		string text4 = PrepareText(style.Name);
		if (m_isCyrillicText)
		{
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "falt " + text4 + "}");
			memoryStream.Write(bytes, 0, bytes.Length);
			m_isCyrillicText = false;
		}
		else
		{
			bytes = m_encoding.GetBytes(" " + text4);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(";}");
		memoryStream.Write(bytes, 0, bytes.Length);
		m_styleBytes = memoryStream.ToArray();
		Styles.Add(style.Name, text);
	}

	private void BuildStyle(string styleName)
	{
		if (m_doc.Styles.FindByName(styleName) is Style style)
		{
			BuildStyle(style);
		}
	}

	private string BuildTextBorder(Border brd)
	{
		if (brd == null || (brd.BorderType == BorderStyle.None && !brd.HasNoneStyle))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(c_slashSymbol + "chbrdr");
		stringBuilder.Append(BuildBorder(brd, isTable: false));
		return stringBuilder.ToString();
	}

	private string BuildFrameProps(WParagraphFormat pFormat)
	{
		if (!pFormat.IsFrame)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		switch (pFormat.FrameHorizontalPos)
		{
		case 0:
			stringBuilder.Append(c_slashSymbol + "phcol");
			break;
		case 1:
			stringBuilder.Append(c_slashSymbol + "phmrg");
			break;
		case 2:
			stringBuilder.Append(c_slashSymbol + "phpg");
			break;
		}
		switch (pFormat.FrameVerticalPos)
		{
		case 0:
			stringBuilder.Append(c_slashSymbol + "pvmrg");
			break;
		case 1:
			stringBuilder.Append(c_slashSymbol + "pvpg");
			break;
		case 2:
			stringBuilder.Append(c_slashSymbol + "pvpara");
			break;
		}
		if (pFormat.FrameX < 0f)
		{
			switch ((short)pFormat.FrameX)
			{
			case 0:
				stringBuilder.Append(c_slashSymbol + "posxl");
				break;
			case -4:
				stringBuilder.Append(c_slashSymbol + "posxc");
				break;
			case -8:
				stringBuilder.Append(c_slashSymbol + "posxr");
				break;
			case -12:
				stringBuilder.Append(c_slashSymbol + "posxi");
				break;
			case -16:
				stringBuilder.Append(c_slashSymbol + "posxo");
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private string BuildParaListId(WParagraph para, WParagraphFormat pFormat)
	{
		if (pFormat.OwnerBase == null)
		{
			return string.Empty;
		}
		WParagraphStyle wParagraphStyle = null;
		int num = -1;
		if (pFormat.OwnerBase is WParagraphStyle)
		{
			wParagraphStyle = pFormat.OwnerBase as WParagraphStyle;
		}
		else if (pFormat.OwnerBase is WParagraph)
		{
			wParagraphStyle = m_doc.Styles.FindByName((pFormat.OwnerBase as WParagraph).StyleName) as WParagraphStyle;
		}
		string value = string.Empty;
		if (para != null && para.ListFormat.IsEmptyList)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (para != null && para.ListFormat.ListType != ListType.NoList && !string.IsNullOrEmpty(para.ListFormat.CurrentListStyle.Name))
		{
			num = ListsIds[para.ListFormat.CurrentListStyle.Name] + 1;
			value = para.ListFormat.LFOStyleName;
		}
		else if (wParagraphStyle != null && wParagraphStyle.ListFormat != null && wParagraphStyle.ListFormat.CurrentListStyle != null && !string.IsNullOrEmpty(wParagraphStyle.ListFormat.CurrentListStyle.Name))
		{
			num = ListsIds[wParagraphStyle.ListFormat.CurrentListStyle.Name] + 1;
			value = wParagraphStyle.ListFormat.LFOStyleName;
		}
		if (num != -1)
		{
			stringBuilder.Append(c_slashSymbol + "ls");
			stringBuilder.Append(num);
			if (!ListOverrideAr.ContainsKey(num))
			{
				ListOverrideAr.Add(num, value);
			}
			else if (string.IsNullOrEmpty(ListOverrideAr[num]) && !string.IsNullOrEmpty(value))
			{
				ListOverrideAr[num] = value;
			}
			if (para != null)
			{
				stringBuilder.Append(c_slashSymbol + "ilvl");
				stringBuilder.Append(para.ListFormat.ListLevelNumber);
			}
		}
		return stringBuilder.ToString();
	}

	private byte[] BuildTable(WTable table)
	{
		MemoryStream memoryStream = new MemoryStream();
		m_tableNestedLevel++;
		table.IsUpdateCellWidthByPartitioning = true;
		table.UpdateGridSpan();
		if (!table.IsUpdateCellWidthByPartitioning)
		{
			table.UpdateUnDefinedCellWidth();
		}
		int i = 0;
		for (int count = table.Rows.Count; i < count; i++)
		{
			WTableRow row = table.Rows[i];
			byte[] array = BuildTableRow(row);
			memoryStream.Write(array, 0, array.Length);
		}
		m_tableNestedLevel--;
		return memoryStream.ToArray();
	}

	private byte[] BuildTableRow(WTableRow row)
	{
		MemoryStream memoryStream = new MemoryStream();
		string text = BuildTRowFormat(row.RowFormat);
		byte[] bytes;
		if (m_tableNestedLevel == 1)
		{
			bytes = m_encoding.GetBytes(text);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		int i = 0;
		for (int count = row.Cells.Count; i < count; i++)
		{
			WTableCell wTableCell = row.Cells[i];
			if (wTableCell.CellFormat.HorizontalMerge != CellMerge.Continue)
			{
				bytes = BuildTableCell(wTableCell);
				memoryStream.Write(bytes, 0, bytes.Length);
			}
		}
		if (m_tableNestedLevel != 1)
		{
			text = "{" + c_slashSymbol + "*" + c_slashSymbol + "nesttableprops" + text + c_slashSymbol + "nestrow}";
		}
		bytes = m_encoding.GetBytes(text);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (row.OwnerTable != null && row.OwnerTable.Owner != null && row.OwnerTable.Owner.OwnerBase != null && row.OwnerTable.Owner.OwnerBase is WTableRow)
		{
			WTableRow wTableRow = row.OwnerTable.Owner.OwnerBase as WTableRow;
			bytes = m_encoding.GetBytes(BuildTRowFormat(wTableRow.RowFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "row}");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}

	private string BuildTRowFormat(RowFormat rowFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		WTableRow ownerRow = rowFormat.OwnerRow;
		stringBuilder.Append(c_slashSymbol + "trowd");
		if (ownerRow != null && ownerRow.OwnerTable != null && ownerRow == ownerRow.OwnerTable.LastRow)
		{
			stringBuilder.Append(c_slashSymbol + "lastrow");
		}
		if (ownerRow.IsHeader)
		{
			stringBuilder.Append(c_slashSymbol + "trhdr");
		}
		if (!ownerRow.RowFormat.IsBreakAcrossPages)
		{
			stringBuilder.Append(c_slashSymbol + "trkeep");
		}
		if (ownerRow.RowFormat.Bidi)
		{
			stringBuilder.Append(c_slashSymbol + "rtlrow");
		}
		float num = 0f;
		if (ownerRow.OwnerTable.FirstRow.Cells.Count > 0)
		{
			WTableCell wTableCell = ownerRow.OwnerTable.FirstRow.Cells[0];
			num = wTableCell.CellFormat.Paddings.Left;
			if (wTableCell.CellFormat.SamePaddingsAsTable)
			{
				num = ownerRow.OwnerTable.TableFormat.Paddings.Left;
			}
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			stringBuilder.Append(c_slashSymbol + "tblind" + Math.Round(rowFormat.LeftIndent * 20f));
			stringBuilder.Append(c_slashSymbol + "tblindtype3");
		}
		if (rowFormat.LeftIndent != 0f && !ownerRow.OwnerTable.TableFormat.WrapTextAround && ownerRow.OwnerTable.TableFormat.HorizontalAlignment == RowAlignment.Left)
		{
			int num2 = (int)Math.Round((rowFormat.LeftIndent - num) * 20f);
			num2 -= ((num == 0f) ? 5 : 0);
			stringBuilder.Append(c_slashSymbol + "trleft" + num2);
			m_cellEndPos = num2;
		}
		else
		{
			int num3 = (int)Math.Round((0f - num) * 20f);
			num3 = ((num != 0f) ? num3 : (-5));
			stringBuilder.Append(c_slashSymbol + "trleft" + num3);
			m_cellEndPos = num3;
		}
		if (rowFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
		{
			stringBuilder.Append(c_slashSymbol + "tpvpara");
		}
		else if (rowFormat.Positioning.VertRelationTo == VerticalRelation.Page)
		{
			stringBuilder.Append(c_slashSymbol + "tpvpg");
		}
		if (rowFormat.Positioning.HorizRelationTo == HorizontalRelation.Page)
		{
			stringBuilder.Append(c_slashSymbol + "tphpg");
		}
		else if (rowFormat.Positioning.HorizRelationTo == HorizontalRelation.Margin)
		{
			stringBuilder.Append(c_slashSymbol + "tphmrg");
		}
		RowFormat.TablePositioning tablePositioning = (rowFormat.WrapTextAround ? rowFormat.Positioning : (ownerRow.OwnerTable.TableFormat.WrapTextAround ? ownerRow.OwnerTable.TableFormat.Positioning : null));
		if (tablePositioning != null)
		{
			switch (tablePositioning.HorizPositionAbs)
			{
			case HorizontalPosition.Center:
				stringBuilder.Append(c_slashSymbol + "tposxc");
				break;
			case HorizontalPosition.Inside:
				stringBuilder.Append(c_slashSymbol + "tposxi");
				break;
			case HorizontalPosition.Outside:
				stringBuilder.Append(c_slashSymbol + "tposxo");
				break;
			case HorizontalPosition.Right:
				stringBuilder.Append(c_slashSymbol + "tposxr");
				break;
			default:
			{
				if (tablePositioning.HorizPositionAbs == HorizontalPosition.Left && tablePositioning.HorizPosition == 0f)
				{
					stringBuilder.Append(c_slashSymbol + "tposxl");
					break;
				}
				int num4 = (int)Math.Round(tablePositioning.HorizPosition * 20f);
				stringBuilder.Append(c_slashSymbol + "tposx" + num4);
				break;
			}
			}
			switch (tablePositioning.VertPositionAbs)
			{
			case VerticalPosition.Bottom:
				stringBuilder.Append(c_slashSymbol + "tposyb");
				break;
			case VerticalPosition.Center:
				stringBuilder.Append(c_slashSymbol + "tposyc");
				break;
			case VerticalPosition.Inside:
				stringBuilder.Append(c_slashSymbol + "tposyin");
				break;
			case VerticalPosition.Outside:
				stringBuilder.Append(c_slashSymbol + "tposyout");
				break;
			case VerticalPosition.Top:
				stringBuilder.Append(c_slashSymbol + "tposyt");
				break;
			default:
			{
				int num5 = (int)Math.Round(tablePositioning.VertPosition * 20f);
				stringBuilder.Append(c_slashSymbol + "tposy" + num5);
				break;
			}
			}
			int num6 = (int)Math.Round(tablePositioning.DistanceFromLeft * 20f);
			if (num6 != 0)
			{
				stringBuilder.Append(c_slashSymbol + "tdfrmtxtLeft" + num6);
			}
			if ((int)Math.Round(tablePositioning.DistanceFromRight * 20f) != 0)
			{
				stringBuilder.Append(c_slashSymbol + "tdfrmtxtRight" + num6);
			}
			if ((int)Math.Round(tablePositioning.DistanceFromTop * 20f) != 0)
			{
				stringBuilder.Append(c_slashSymbol + "tdfrmtxtTop" + num6);
			}
			if ((int)Math.Round(tablePositioning.DistanceFromBottom * 20f) != 0)
			{
				stringBuilder.Append(c_slashSymbol + "tdfrmtxtBottom" + num6);
			}
		}
		if (!rowFormat.Positioning.AllowOverlap)
		{
			stringBuilder.Append(c_slashSymbol + "tabsnoovrlp1");
		}
		if (rowFormat.CellSpacing != -1f)
		{
			string text = ((int)Math.Round(rowFormat.CellSpacing * 20f)).ToString();
			stringBuilder.Append(c_slashSymbol + "trspdl" + text);
			stringBuilder.Append(c_slashSymbol + "trspdr" + text);
			stringBuilder.Append(c_slashSymbol + "trspdb" + text);
			stringBuilder.Append(c_slashSymbol + "trspdt" + text);
			stringBuilder.Append(c_slashSymbol + "trspdfl3" + c_slashSymbol + "trspdft3" + c_slashSymbol + "trspdfb3" + c_slashSymbol + "trspdfr3");
		}
		else if (rowFormat.LeftIndent > 0f)
		{
			stringBuilder.Append(c_slashSymbol + "trgaph108");
		}
		if (rowFormat.Height != 0f)
		{
			if (ownerRow.HeightType == TableRowHeightType.Exactly && ownerRow.Height > 0f)
			{
				stringBuilder.Append(c_slashSymbol + "trrh" + (int)Math.Round(0f - ownerRow.Height * 20f));
			}
			else
			{
				stringBuilder.Append(c_slashSymbol + "trrh" + (int)Math.Round(ownerRow.Height * 20f));
			}
		}
		if (ownerRow.OwnerTable != null)
		{
			int num7 = 0;
			string text2 = string.Empty;
			if (ownerRow.OwnerTable.TableFormat.PreferredWidth.WidthType != FtsWidth.Auto)
			{
				if (ownerRow.OwnerTable.TableGrid.Count > 0 && ownerRow.OwnerTable.DocxTableFormat.HasFormat)
				{
					num7 = (int)ownerRow.OwnerTable.TableGrid[ownerRow.OwnerTable.TableGrid.Count - 1].EndOffset;
					text2 = "trftsWidth3";
				}
				else if (ownerRow.OwnerTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					num7 = (int)Math.Round(ownerRow.OwnerTable.PreferredTableWidth.Width * 50f);
					text2 = "trftsWidth2";
				}
				else if (ownerRow.OwnerTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Point)
				{
					num7 = (int)Math.Round(ownerRow.OwnerTable.PreferredTableWidth.Width * 20f);
					text2 = "trftsWidth3";
				}
				if (text2 != string.Empty)
				{
					stringBuilder.Append(c_slashSymbol + text2);
				}
				if (!m_doc.SaveOptions.OptimizeRtfFileSize || num7 != 0)
				{
					stringBuilder.Append(c_slashSymbol + "trwWidth" + num7);
				}
			}
		}
		if (rowFormat.IsAutoResized)
		{
			stringBuilder.Append(c_slashSymbol + "trautofit1");
		}
		if (rowFormat.GridBeforeWidth.Width > 0f && rowFormat.GridBeforeWidth.WidthType != FtsWidth.Auto && rowFormat.GridBeforeWidth.WidthType != 0)
		{
			int num8 = 0;
			string text3 = string.Empty;
			if (rowFormat.GridBeforeWidth.WidthType == FtsWidth.Percentage)
			{
				num8 = (int)Math.Round(rowFormat.GridBeforeWidth.Width * 50f);
				text3 = "trftsWidthB2";
			}
			else if (rowFormat.GridBeforeWidth.WidthType == FtsWidth.Point)
			{
				num8 = (int)Math.Round(rowFormat.GridBeforeWidth.Width * 20f);
				text3 = "trftsWidthB3";
			}
			if (text3 != string.Empty)
			{
				stringBuilder.Append(c_slashSymbol + text3);
			}
			stringBuilder.Append(c_slashSymbol + "trftsWidthB" + num8);
		}
		if (rowFormat.GridAfterWidth.Width > 0f && rowFormat.GridAfterWidth.WidthType != FtsWidth.Auto && rowFormat.GridAfterWidth.WidthType != 0)
		{
			int num9 = 0;
			string text4 = string.Empty;
			if (rowFormat.GridAfterWidth.WidthType == FtsWidth.Percentage)
			{
				num9 = (int)Math.Round(rowFormat.GridAfterWidth.Width * 50f);
				text4 = "trftsWidthA2";
			}
			else if (rowFormat.GridAfterWidth.WidthType == FtsWidth.Point)
			{
				num9 = (int)Math.Round(rowFormat.GridAfterWidth.Width * 20f);
				text4 = "trftsWidthA3";
			}
			if (text4 != string.Empty)
			{
				stringBuilder.Append(c_slashSymbol + text4);
			}
			stringBuilder.Append(c_slashSymbol + "trftsWidthA" + num9);
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize)
		{
			stringBuilder.Append(BuildCharacterFormat(ownerRow.CharacterFormat));
		}
		RowAlignment rowAlignment = rowFormat.HorizontalAlignment;
		if (rowFormat.Bidi && (rowFormat.Document.ActualFormatType == FormatType.Doc || rowFormat.Document.ActualFormatType == FormatType.Docx || rowFormat.Document.ActualFormatType == FormatType.Rtf))
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
		switch (rowAlignment)
		{
		case RowAlignment.Right:
			stringBuilder.Append(c_slashSymbol + "trqr");
			break;
		case RowAlignment.Center:
			stringBuilder.Append(c_slashSymbol + "trqc");
			break;
		default:
			if (!m_doc.SaveOptions.OptimizeRtfFileSize)
			{
				stringBuilder.Append(c_slashSymbol + "trql");
			}
			break;
		}
		stringBuilder.Append(BuildTRowBorders(rowFormat.Borders));
		stringBuilder.Append(BuildPadding(rowFormat.Paddings, isRow: true));
		GetOwnerSection(ownerRow);
		int i = 0;
		for (int count = ownerRow.Cells.Count; i < count; i++)
		{
			WTableCell wTableCell2 = ownerRow.Cells[i];
			if (wTableCell2 != null && wTableCell2.CellFormat != null && wTableCell2.CellFormat.HorizontalMerge != CellMerge.Continue)
			{
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(BuildTCellFormat(wTableCell2.CellFormat));
			}
		}
		m_cellEndPos = 0;
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildTCellFormat(CellFormat cFormat)
	{
		StringBuilder stringBuilder = new StringBuilder();
		WTableCell wTableCell = cFormat.OwnerBase as WTableCell;
		if (wTableCell.CellFormat.VerticalMerge == CellMerge.Start)
		{
			stringBuilder.Append(c_slashSymbol + "clvmgf");
			stringBuilder.Append(BuildVertAlignment(wTableCell.CellFormat.VerticalAlignment));
		}
		else if (wTableCell.CellFormat.VerticalMerge == CellMerge.Continue)
		{
			stringBuilder.Append(c_slashSymbol + "clvmrg");
			stringBuilder.Append(BuildVertAlignment(cFormat.VerticalAlignment));
		}
		else if (!m_doc.SaveOptions.OptimizeRtfFileSize || cFormat.VerticalAlignment != 0)
		{
			stringBuilder.Append(BuildVertAlignment(cFormat.VerticalAlignment));
		}
		if (cFormat.Borders.IsCellHasNoBorder)
		{
			stringBuilder.Append(BuildTCellBorders(wTableCell, wTableCell.OwnerRow.RowFormat.Borders, null));
		}
		else
		{
			stringBuilder.Append(BuildTCellBorders(wTableCell, cFormat.Borders, wTableCell.OwnerRow.RowFormat.Borders));
		}
		switch (wTableCell.CellFormat.TextDirection)
		{
		case TextDirection.VerticalTopToBottom:
			stringBuilder.Append(c_slashSymbol + "cltxtbrl");
			break;
		case TextDirection.VerticalBottomToTop:
			stringBuilder.Append(c_slashSymbol + "cltxbtlr");
			break;
		case TextDirection.HorizontalFarEast:
			stringBuilder.Append(c_slashSymbol + "cltxlrtbv");
			break;
		case TextDirection.VerticalFarEast:
			stringBuilder.Append(c_slashSymbol + "cltxtbrlv");
			break;
		default:
			if (!m_doc.SaveOptions.OptimizeRtfFileSize)
			{
				stringBuilder.Append(c_slashSymbol + "cltxlrtb");
			}
			break;
		}
		if (!cFormat.BackColor.IsEmpty)
		{
			stringBuilder.Append(BuildColor(cFormat.BackColor, c_slashSymbol + "clcbpat"));
		}
		if (cFormat.FitText)
		{
			stringBuilder.Append(c_slashSymbol + "clFitText");
		}
		if (!cFormat.TextWrap)
		{
			stringBuilder.Append(c_slashSymbol + "clNoWrap");
		}
		int num = 0;
		string text = string.Empty;
		int num2 = (int)Math.Round(wTableCell.Width * 20f);
		if (wTableCell.CellFormat.PreferredWidth.WidthType == FtsWidth.None)
		{
			text = "clftsWidth0";
		}
		else if (wTableCell.CellFormat.PreferredWidth.WidthType == FtsWidth.Auto)
		{
			text = "clftsWidth1";
		}
		else if (wTableCell.CellFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
		{
			num = (int)Math.Round(wTableCell.CellFormat.PreferredWidth.Width * 50f);
			text = "clftsWidth2";
		}
		else if (wTableCell.CellFormat.PreferredWidth.WidthType == FtsWidth.Point)
		{
			num = (int)Math.Round(wTableCell.CellFormat.PreferredWidth.Width * 20f);
			text = "clftsWidth3";
		}
		if (wTableCell.CellFormat.HorizontalMerge == CellMerge.Start)
		{
			for (int i = wTableCell.GetCellIndex() + 1; i < (wTableCell.Owner as WTableRow).ChildEntities.Count; i++)
			{
				WTableCell wTableCell2 = (wTableCell.Owner as WTableRow).ChildEntities[i] as WTableCell;
				if (wTableCell2.CellFormat.HorizontalMerge != CellMerge.Continue)
				{
					break;
				}
				num2 += (int)Math.Round(wTableCell2.Width * 20f);
			}
		}
		if ((text != string.Empty && !m_doc.SaveOptions.OptimizeRtfFileSize) || text != "clftsWidth0")
		{
			stringBuilder.Append(c_slashSymbol + text);
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || num != 0)
		{
			stringBuilder.Append(c_slashSymbol + "clwWidth" + num);
		}
		m_cellEndPos += num2;
		if (!cFormat.SamePaddingsAsTable)
		{
			stringBuilder.Append(BuildPadding(cFormat.Paddings, isRow: false));
		}
		if (cFormat.HideMark)
		{
			stringBuilder.Append(c_slashSymbol + "clhidemark");
		}
		stringBuilder.Append(c_slashSymbol + "cellx");
		stringBuilder.Append(m_cellEndPos);
		return stringBuilder.ToString();
	}

	private string BuildTRowBorders(Borders borders)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Top.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrt");
			stringBuilder.Append(BuildBorder(borders.Top, isTable: true));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Bottom.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrb");
			stringBuilder.Append(BuildBorder(borders.Bottom, isTable: true));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Left.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrl");
			stringBuilder.Append(BuildBorder(borders.Left, isTable: true));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Right.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrr");
			stringBuilder.Append(BuildBorder(borders.Right, isTable: true));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Horizontal.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrh");
			stringBuilder.Append(BuildBorder(borders.Horizontal, isTable: true));
		}
		if (!m_doc.SaveOptions.OptimizeRtfFileSize || borders.Vertical.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "trbrdrv");
			stringBuilder.Append(BuildBorder(borders.Vertical, isTable: true));
		}
		return stringBuilder.ToString();
	}

	private string BuildTCellBorders(WTableCell cell, Borders borders, Borders rowBorders)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (0 == 0 && (!m_doc.SaveOptions.OptimizeRtfFileSize || !borders.Top.HasNoneStyle))
		{
			stringBuilder.Append(c_slashSymbol + "clbrdrt");
			if (CheckCellBorders(cell, BorderType.Top) && borders.Top.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(cell.OwnerRow.RowFormat.Borders.Horizontal, isTable: true));
			}
			else if (!borders.Top.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(borders.Top, isTable: true));
			}
			else if (rowBorders != null)
			{
				stringBuilder.Append(BuildBorder(rowBorders.Top, isTable: true));
			}
		}
		if (!flag && (!m_doc.SaveOptions.OptimizeRtfFileSize || !borders.Bottom.HasNoneStyle))
		{
			stringBuilder.Append(c_slashSymbol + "clbrdrb");
			if (CheckCellBorders(cell, BorderType.Bottom) && borders.Bottom.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(cell.OwnerRow.RowFormat.Borders.Horizontal, isTable: true));
			}
			else if (!borders.Bottom.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(borders.Bottom, isTable: true));
			}
			else if (rowBorders != null)
			{
				stringBuilder.Append(BuildBorder(rowBorders.Bottom, isTable: true));
			}
		}
		if (!flag2 && (!m_doc.SaveOptions.OptimizeRtfFileSize || !borders.Left.HasNoneStyle))
		{
			stringBuilder.Append(c_slashSymbol + "clbrdrl");
			if (CheckCellBorders(cell, BorderType.Left) && borders.Left.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(cell.OwnerRow.RowFormat.Borders.Vertical, isTable: true));
			}
			else if (!borders.Left.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(borders.Left, isTable: true));
			}
			else if (rowBorders != null)
			{
				stringBuilder.Append(BuildBorder(rowBorders.Left, isTable: true));
			}
		}
		if (!flag3 && (!m_doc.SaveOptions.OptimizeRtfFileSize || !borders.Right.HasNoneStyle))
		{
			stringBuilder.Append(c_slashSymbol + "clbrdrr");
			if (CheckCellBorders(cell, BorderType.Right) && borders.Right.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(cell.OwnerRow.RowFormat.Borders.Vertical, isTable: true));
			}
			else if (!borders.Right.HasNoneStyle)
			{
				stringBuilder.Append(BuildBorder(borders.Right, isTable: true));
			}
			else if (rowBorders != null)
			{
				stringBuilder.Append(BuildBorder(rowBorders.Right, isTable: true));
			}
		}
		if (borders.DiagonalDown.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "cldglu");
			stringBuilder.Append(BuildBorder(borders.DiagonalDown, isTable: true));
		}
		if (borders.DiagonalUp.BorderType != 0)
		{
			stringBuilder.Append(c_slashSymbol + "cldgll");
			stringBuilder.Append(BuildBorder(borders.DiagonalUp, isTable: true));
		}
		return stringBuilder.ToString();
	}

	private byte[] BuildTableCell(WTableCell cell)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = BuildBodyItems(cell.Items);
		memoryStream.Write(array, 0, array.Length);
		if (m_tableNestedLevel > 1)
		{
			array = m_encoding.GetBytes("{" + c_slashSymbol + "nestcell}");
			memoryStream.Write(array, 0, array.Length);
		}
		else
		{
			array = m_encoding.GetBytes("{" + c_slashSymbol + "cell}");
			memoryStream.Write(array, 0, array.Length);
		}
		array = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(array, 0, array.Length);
		return memoryStream.ToArray();
	}

	private string BuildPadding(Paddings paddings, bool isRow)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = c_slashSymbol + (isRow ? "trpadd" : "clpad");
		bool flag = false;
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && paddings.HasKey(1)) || (paddings.HasKey(1) && paddings.Left != 0f))
		{
			stringBuilder.Append(text + (isRow ? "l" : "t"));
			stringBuilder.Append((int)Math.Round(paddings.Left * 20f));
			flag = true;
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && paddings.HasKey(2)) || (paddings.HasKey(2) && paddings.Top != 0f))
		{
			stringBuilder.Append(text + (isRow ? "t" : "l"));
			stringBuilder.Append((int)Math.Round(paddings.Top * 20f));
			flag = true;
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && paddings.HasKey(3)) || (paddings.HasKey(3) && paddings.Bottom != 0f))
		{
			stringBuilder.Append(text + "b");
			stringBuilder.Append((int)Math.Round(paddings.Bottom * 20f));
			flag = true;
		}
		if ((!m_doc.SaveOptions.OptimizeRtfFileSize && paddings.HasKey(4)) || (paddings.HasKey(4) && paddings.Right != 0f))
		{
			stringBuilder.Append(text + "r");
			stringBuilder.Append((int)Math.Round(paddings.Right * 20f));
			flag = true;
		}
		if (flag)
		{
			if (isRow)
			{
				stringBuilder.Append(c_slashSymbol + "trpaddfl3" + c_slashSymbol + "trpaddft3" + c_slashSymbol + "trpaddfb3" + c_slashSymbol + "trpaddfr3");
			}
			else
			{
				stringBuilder.Append(c_slashSymbol + "clpadfl3" + c_slashSymbol + "clpadft3" + c_slashSymbol + "clpadfb3" + c_slashSymbol + "clpadfr3");
			}
		}
		return stringBuilder.ToString();
	}

	private string BuildVertAlignment(VerticalAlignment alignment)
	{
		return alignment switch
		{
			VerticalAlignment.Top => c_slashSymbol + "clvertalt", 
			VerticalAlignment.Middle => c_slashSymbol + "clvertalc", 
			VerticalAlignment.Bottom => c_slashSymbol + "clvertalb", 
			_ => string.Empty, 
		};
	}

	private bool CheckCellBorders(WTableCell cell, BorderType borderType)
	{
		WTableRow ownerRow = cell.OwnerRow;
		switch (borderType)
		{
		case BorderType.Top:
			if (ownerRow.PreviousSibling is WTableRow)
			{
				return true;
			}
			break;
		case BorderType.Bottom:
			if (ownerRow.NextSibling is WTableRow)
			{
				return true;
			}
			break;
		case BorderType.Left:
			if (cell.PreviousSibling is WTableCell)
			{
				return true;
			}
			break;
		case BorderType.Right:
			if (cell.NextSibling is WTableCell)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private byte[] BuildParagraphItem(ParagraphItem item)
	{
		MemoryStream memoryStream = new MemoryStream();
		new StringBuilder();
		switch (item.EntityType)
		{
		case EntityType.TextRange:
			if (item is WTextRange textRange)
			{
				byte[] array = m_encoding.GetBytes(BuildTextRange(textRange));
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		case EntityType.BookmarkStart:
		{
			byte[] array = InsertBkmkStart(item as BookmarkStart);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.BookmarkEnd:
		{
			byte[] array = InsertBkmkEnd(item as BookmarkEnd);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Break:
		{
			byte[] array = InsertLineBreak(item as Break);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Field:
		case EntityType.MergeField:
		case EntityType.SeqField:
		case EntityType.ControlField:
		case EntityType.OleObject:
		{
			if (item is WMergeField)
			{
				(item as WMergeField).UpdateFieldMarks();
			}
			WField wField = ((item is WField) ? (item as WField) : (item as WOleObject).Field);
			if (wField.FieldEnd != null && wField.FieldType != FieldType.FieldUnknown)
			{
				CurrentField.Push(wField);
				byte[] array = m_encoding.GetBytes(BuildField(wField));
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		}
		case EntityType.FieldMark:
		{
			byte[] array = m_encoding.GetBytes(BuildFieldMark(item as WFieldMark));
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Picture:
			if ((item as WPicture).ImageRecord != null)
			{
				byte[] array = m_encoding.GetBytes(BuildPicture(item as WPicture, isFieldShape: false));
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		case EntityType.AutoShape:
			if (item is Shape)
			{
				byte[] array = m_encoding.GetBytes(BuildShape(item as Shape));
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		case EntityType.TextBox:
		{
			byte[] array = BuildTextBox(item as WTextBox);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Footnote:
		{
			byte[] array = BuildFootnoteEndnote(item as WFootnote);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Symbol:
		{
			byte[] array = BuildSymbol(item as WSymbol);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.TOC:
		{
			byte[] array = BuildTocField(item as TableOfContent);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.CheckBox:
		{
			byte[] array = m_encoding.GetBytes(BuildFormField(item as WCheckBox));
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.TextFormField:
		{
			byte[] array = m_encoding.GetBytes(BuildFormField(item as WTextFormField));
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.DropDownFormField:
		{
			byte[] array = m_encoding.GetBytes(BuildFormField(item as WDropDownFormField));
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.CommentMark:
		{
			byte[] array = BuildCommentMark(item as WCommentMark);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.Comment:
		{
			byte[] array = BuildComment(item as WComment);
			memoryStream.Write(array, 0, array.Length);
			break;
		}
		case EntityType.InlineContentControl:
		{
			ParagraphItemCollection paragraphItems = (item as InlineContentControl).ParagraphItems;
			for (int i = 0; i < paragraphItems.Count; i++)
			{
				byte[] array = BuildParagraphItem(paragraphItems[i]);
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		}
		case EntityType.XmlParaItem:
			if (!(item is XmlParagraphItem { MathParaItemsCollection: not null } xmlParagraphItem) || xmlParagraphItem.MathParaItemsCollection.Count <= 0)
			{
				break;
			}
			foreach (ParagraphItem item2 in xmlParagraphItem.MathParaItemsCollection)
			{
				byte[] array = BuildParagraphItem(item2);
				memoryStream.Write(array, 0, array.Length);
			}
			break;
		}
		return memoryStream.ToArray();
	}

	private byte[] BuildSymbol(WSymbol symbol)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildCharacterFormat(symbol.CharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "field{" + c_slashSymbol + "*" + c_slashSymbol + "fldinst SYMBOL ");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(symbol.CharacterCode.ToString());
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(" " + c_slashSymbol + c_slashSymbol + "f \"" + symbol.FontName + "\"}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "fldrslt}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}}");
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private byte[] BuildFootnoteEndnote(WFootnote footnote)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		if (string.IsNullOrEmpty(footnote.CustomMarker) && !footnote.CustomMarkerIsSymbol)
		{
			bytes = m_encoding.GetBytes(BuildCharacterFormat(footnote.ParaItemCharFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "chftn");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (footnote.CustomMarkerIsSymbol)
		{
			bytes = m_encoding.GetBytes(BuildCharacterFormat(footnote.MarkerCharacterFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
			WSymbol wSymbol = new WSymbol(m_doc);
			wSymbol.CharacterCode = footnote.SymbolCode;
			wSymbol.FontName = footnote.SymbolFontName;
			bytes = BuildSymbol(wSymbol);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes(BuildCharacterFormat(footnote.MarkerCharacterFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(" " + footnote.m_strCustomMarker);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "footnote");
		memoryStream.Write(bytes, 0, bytes.Length);
		if (footnote.FootnoteType == FootnoteType.Endnote)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "ftnalt");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (string.IsNullOrEmpty(footnote.CustomMarker) && !footnote.CustomMarkerIsSymbol)
		{
			bytes = m_encoding.GetBytes(BuildCharacterFormat(footnote.MarkerCharacterFormat));
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "chftn");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = BuildBodyItems(footnote.TextBody.Items);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (footnote.FootnoteType == FootnoteType.Footnote)
		{
			bytes = m_encoding.GetBytes(BuildFootnoteProp());
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes(BuildEndnoteProp());
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}

	private string BuildFootnoteProp()
	{
		if (m_hasFootnote)
		{
			return string.Empty;
		}
		m_hasFootnote = true;
		StringBuilder stringBuilder = new StringBuilder();
		if (m_doc.RestartIndexForFootnotes == FootnoteRestartIndex.DoNotRestart)
		{
			stringBuilder.Append(c_slashSymbol + "sftnrstcont");
		}
		else if (m_doc.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachPage)
		{
			stringBuilder.Append(c_slashSymbol + "sftnrstpg");
		}
		else if (m_doc.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachSection)
		{
			stringBuilder.Append(c_slashSymbol + "sftnrestart");
		}
		stringBuilder.Append(c_slashSymbol + "sftnstart");
		stringBuilder.Append(m_doc.InitialFootnoteNumber);
		switch (m_doc.FootnoteNumberFormat)
		{
		case FootEndNoteNumberFormat.Arabic:
			stringBuilder.Append(c_slashSymbol + "sftnnar");
			break;
		case FootEndNoteNumberFormat.LowerCaseLetter:
			stringBuilder.Append(c_slashSymbol + "sftnnalc");
			break;
		case FootEndNoteNumberFormat.LowerCaseRoman:
			stringBuilder.Append(c_slashSymbol + "sftnnrlc");
			break;
		case FootEndNoteNumberFormat.UpperCaseLetter:
			stringBuilder.Append(c_slashSymbol + "sftnnauc");
			break;
		case FootEndNoteNumberFormat.UpperCaseRoman:
			stringBuilder.Append(c_slashSymbol + "sftnnruc");
			break;
		default:
			stringBuilder.Append(c_slashSymbol + "sftnnchi");
			break;
		}
		if (m_doc.FootnotePosition == FootnotePosition.PrintAtBottomOfPage)
		{
			stringBuilder.Append(c_slashSymbol + "ftnbj");
		}
		else if (m_doc.FootnotePosition == FootnotePosition.PrintImmediatelyBeneathText)
		{
			stringBuilder.Append(c_slashSymbol + "ftntj");
		}
		else if (m_doc.FootnotePosition == FootnotePosition.PrintAsEndnotes)
		{
			if (m_doc.EndnotePosition == EndnotePosition.DisplayEndOfDocument)
			{
				stringBuilder.Append(c_slashSymbol + "enddoc");
			}
			else if (m_doc.EndnotePosition == EndnotePosition.DisplayEndOfSection)
			{
				stringBuilder.Append(c_slashSymbol + "endnotes");
			}
		}
		return stringBuilder.ToString();
	}

	private string BuildEndnoteProp()
	{
		if (m_hasEndnote)
		{
			return string.Empty;
		}
		m_hasEndnote = true;
		StringBuilder stringBuilder = new StringBuilder();
		if (m_doc.RestartIndexForEndnote == EndnoteRestartIndex.DoNotRestart)
		{
			stringBuilder.Append(c_slashSymbol + "saftnrstcont");
		}
		else if (m_doc.RestartIndexForEndnote == EndnoteRestartIndex.RestartForEachSection)
		{
			stringBuilder.Append(c_slashSymbol + "saftnrestart");
		}
		stringBuilder.Append(c_slashSymbol + "saftnstart");
		stringBuilder.Append(m_doc.InitialEndnoteNumber);
		switch (m_doc.EndnoteNumberFormat)
		{
		case FootEndNoteNumberFormat.Arabic:
			stringBuilder.Append(c_slashSymbol + "saftnnar");
			break;
		case FootEndNoteNumberFormat.LowerCaseLetter:
			stringBuilder.Append(c_slashSymbol + "saftnnalc");
			break;
		case FootEndNoteNumberFormat.LowerCaseRoman:
			stringBuilder.Append(c_slashSymbol + "saftnnrlc");
			break;
		case FootEndNoteNumberFormat.UpperCaseLetter:
			stringBuilder.Append(c_slashSymbol + "saftnnauc");
			break;
		case FootEndNoteNumberFormat.UpperCaseRoman:
			stringBuilder.Append(c_slashSymbol + "saftnnruc");
			break;
		}
		if (m_doc.EndnotePosition == EndnotePosition.DisplayEndOfDocument)
		{
			stringBuilder.Append(c_slashSymbol + "aenddoc");
		}
		else if (m_doc.EndnotePosition == EndnotePosition.DisplayEndOfSection)
		{
			stringBuilder.Append(c_slashSymbol + "aendnotes");
		}
		return stringBuilder.ToString();
	}

	private string BuildFieldMark(WFieldMark fieldMark)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_currentField == null || CurrentField.Count == 0)
		{
			return string.Empty;
		}
		if (fieldMark.Type == FieldMarkType.FieldSeparator || (fieldMark.ParentField != null && fieldMark.ParentField.FieldSeparator == null))
		{
			string value = string.Empty;
			if (fieldMark.ParentField is WTextFormField)
			{
				value = BuildTextFormField(fieldMark.ParentField as WTextFormField);
			}
			else if (fieldMark.ParentField is WCheckBox)
			{
				value = BuildCheckBox(fieldMark.ParentField as WCheckBox);
			}
			else if (fieldMark.ParentField is WDropDownFormField)
			{
				value = BuildDropDownField(fieldMark.ParentField as WDropDownFormField);
			}
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(value);
			}
		}
		if (fieldMark.Type == FieldMarkType.FieldSeparator)
		{
			if (!(CurrentField.Peek() as WField).IsFormField())
			{
				stringBuilder.Append("}");
			}
			stringBuilder.Append("}{" + c_slashSymbol + "fldrslt");
		}
		else if (fieldMark.ParentField != null && fieldMark.ParentField.FieldSeparator == null)
		{
			if (!(CurrentField.Peek() as WField).IsFormField())
			{
				stringBuilder.Append("}");
			}
			stringBuilder.Append("}{" + c_slashSymbol + "fldrslt}}");
			CurrentField.Pop();
		}
		else if (!(fieldMark.PreviousSibling is WField))
		{
			stringBuilder.Append("}}");
			CurrentField.Pop();
		}
		else if (WriteFieldEnd(fieldMark))
		{
			stringBuilder.Append("{" + c_slashSymbol + "fldrslt}}");
			CurrentField.Pop();
		}
		else
		{
			stringBuilder.Append("}");
			CurrentField.Pop();
		}
		if (m_isField && CurrentField.Count == 0)
		{
			m_isField = false;
		}
		return stringBuilder.ToString();
	}

	private string BuildField(WField field)
	{
		StringBuilder stringBuilder = new StringBuilder();
		m_isField = true;
		stringBuilder.Append("{" + c_slashSymbol + "field");
		if (WordDocument.DisableDateTimeUpdating && (field.FieldType == FieldType.FieldDate || field.FieldType == FieldType.FieldTime))
		{
			stringBuilder.Append(c_slashSymbol + "fldlock");
		}
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "fldinst{");
		if (field.FieldType != FieldType.FieldNoteRef)
		{
			stringBuilder.Append(BuildCharacterFormat(field.CharacterFormat));
		}
		return stringBuilder.ToString();
	}

	private byte[] InsertLineBreak(Break brk)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildCharacterFormat(brk.CharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		switch (brk.BreakType)
		{
		case BreakType.LineBreak:
		case BreakType.TextWrappingBreak:
			if (brk.CharacterFormat.BreakClear != 0)
			{
				switch (brk.CharacterFormat.BreakClear)
				{
				case BreakClearType.Left:
					bytes = m_encoding.GetBytes(c_slashSymbol + "lbr1");
					memoryStream.Write(bytes, 0, bytes.Length);
					break;
				case BreakClearType.Right:
					bytes = m_encoding.GetBytes(c_slashSymbol + "lbr2");
					memoryStream.Write(bytes, 0, bytes.Length);
					break;
				case BreakClearType.All:
					bytes = m_encoding.GetBytes(c_slashSymbol + "lbr3");
					memoryStream.Write(bytes, 0, bytes.Length);
					break;
				}
			}
			bytes = m_encoding.GetBytes(c_slashSymbol + "line");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case BreakType.PageBreak:
			bytes = m_encoding.GetBytes(c_slashSymbol + "page");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		case BreakType.ColumnBreak:
			bytes = m_encoding.GetBytes(c_slashSymbol + "column");
			memoryStream.Write(bytes, 0, bytes.Length);
			break;
		}
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private string BuildTextRange(WTextRange textRange)
	{
		textRange.Text = textRange.Text.Replace('\u0002'.ToString(), string.Empty);
		if (textRange.Text == string.Empty)
		{
			return string.Empty;
		}
		string text = textRange.Text;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(BuildTextRangeStr(textRange.CharacterFormat, PrepareText(text)));
		return stringBuilder.ToString();
	}

	private byte[] InsertBkmkEnd(BookmarkEnd bkmkEnd)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "*" + c_slashSymbol + "bkmkend ");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(bkmkEnd.Name);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private byte[] InsertBkmkStart(BookmarkStart bkmkStart)
	{
		new StringBuilder();
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "*" + c_slashSymbol + "bkmkstart ");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(bkmkStart.Name);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (bkmkStart.ColumnFirst >= 0)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "bkmkcolf" + bkmkStart.ColumnFirst);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (bkmkStart.ColumnLast >= 0)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "bkmkcoll" + (bkmkStart.ColumnLast + 1));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private byte[] BuildTocField(TableOfContent toc)
	{
		CurrentField.Push(toc.TOCField);
		return m_encoding.GetBytes(BuildField(toc.TOCField));
	}

	private string BuildPicture(WPicture pic, bool isFieldShape)
	{
		if (pic.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			return BuildInLineImage(pic, isFieldShape);
		}
		return BuildShapeImage(pic, isFieldShape);
	}

	private string BuildShapeImage(WPicture pic, bool isFielsShape)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		stringBuilder.Append(BuildCharacterFormat(pic.CharacterFormat));
		stringBuilder.Append("{" + c_slashSymbol + "shp");
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
		int num = 0;
		int num2 = 0;
		if ((pic.Rotation >= 44f && pic.Rotation <= 134f) || (pic.Rotation >= 225f && pic.Rotation <= 314f))
		{
			float num3 = Math.Abs(pic.Width - pic.Height) / 2f;
			if (pic.Width < pic.Height)
			{
				pic.HorizontalPosition -= num3;
				pic.VerticalPosition += num3;
			}
			else if (pic.Width > pic.Height)
			{
				pic.VerticalPosition -= num3;
				pic.HorizontalPosition += num3;
			}
			num = (int)Math.Round(pic.Height * 20f);
			num2 = (int)Math.Round(pic.Width * 20f);
		}
		else
		{
			num = (int)Math.Round(pic.Width * 20f);
			num2 = (int)Math.Round(pic.Height * 20f);
		}
		stringBuilder.Append(BuildShapePosition(pic.HorizontalPosition, pic.VerticalPosition, num, num2));
		if (pic.IsHeaderPicture)
		{
			stringBuilder.Append(c_slashSymbol + "shpfhdr1");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "shpfhdr0");
		}
		if (pic.HorizontalOrigin == HorizontalOrigin.Page)
		{
			stringBuilder.Append(c_slashSymbol + "shpbxpage");
		}
		else if (pic.HorizontalOrigin == HorizontalOrigin.Margin || pic.HorizontalOrigin == HorizontalOrigin.LeftMargin || pic.HorizontalOrigin == HorizontalOrigin.RightMargin || pic.HorizontalOrigin == HorizontalOrigin.InsideMargin || pic.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			stringBuilder.Append(c_slashSymbol + "shpbxmargin");
		}
		else if (pic.HorizontalOrigin == HorizontalOrigin.Column)
		{
			stringBuilder.Append(c_slashSymbol + "shpbxcolumn");
		}
		if (pic.VerticalOrigin == VerticalOrigin.Page)
		{
			stringBuilder.Append(c_slashSymbol + "shpbypage");
		}
		else if (pic.VerticalOrigin == VerticalOrigin.Margin)
		{
			stringBuilder.Append(c_slashSymbol + "shpbymargin");
		}
		else if (pic.VerticalOrigin == VerticalOrigin.Paragraph)
		{
			stringBuilder.Append(c_slashSymbol + "shpbypara");
		}
		stringBuilder.Append(BuildWrappingStyle(pic.TextWrappingStyle, pic.TextWrappingType));
		stringBuilder.Append(c_slashSymbol + "shpz" + pic.OrderIndex / 1024);
		stringBuilder.Append(BuildShapeProp("shapeType", "75"));
		double num4 = pic.Rotation * 65536f;
		if (num4 != 0.0)
		{
			stringBuilder.Append(BuildShapeProp("rotation", num4.ToString()));
		}
		if (!string.IsNullOrEmpty(pic.ExternalLink))
		{
			stringBuilder.Append("{" + c_slashSymbol + "sp");
			stringBuilder.Append("{" + c_slashSymbol + "sn ");
			stringBuilder.Append("pibName");
			stringBuilder.Append("}");
			stringBuilder.Append("{" + c_slashSymbol + "sv ");
			stringBuilder.Append(pic.ExternalLink);
			stringBuilder.Append("}}");
		}
		if (!string.IsNullOrEmpty(pic.LinkType))
		{
			stringBuilder.Append("{" + c_slashSymbol + "sp");
			stringBuilder.Append("{" + c_slashSymbol + "sn ");
			stringBuilder.Append("pibFlags");
			stringBuilder.Append("}");
			stringBuilder.Append("{" + c_slashSymbol + "sv ");
			stringBuilder.Append(pic.LinkType);
			stringBuilder.Append("}}");
		}
		if (!string.IsNullOrEmpty(pic.Href))
		{
			stringBuilder.Append(BuildShapeProp("pihlShape", BuildPictureLink(pic)));
		}
		stringBuilder.Append(BuildShapeProp("pib", BuildPictureProp(pic, isFielsShape)));
		if (pic.PictureShape.PictureDescriptor.BorderTop.IsDefault && pic.PictureShape.PictureDescriptor.BorderLeft.IsDefault && pic.PictureShape.PictureDescriptor.BorderRight.IsDefault && pic.PictureShape.PictureDescriptor.BorderBottom.IsDefault)
		{
			stringBuilder.Append(BuildShapeProp("fLine", "0"));
		}
		else
		{
			stringBuilder.Append(BuildShapeProp("fLine", "1"));
		}
		stringBuilder.Append(BuildHorAlignm(pic.HorizontalAlignment));
		stringBuilder.Append(BuildHorPos(pic.HorizontalOrigin));
		stringBuilder.Append(BuildVertAlignm(pic.VerticalAlignment));
		stringBuilder.Append(BuildVertPos(pic.VerticalOrigin));
		stringBuilder.Append(BuildLayoutInCell(pic.LayoutInCell));
		if (pic.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			stringBuilder.Append(BuildShapeProp("fBehindDocument", "1"));
		}
		stringBuilder.Append("}}}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildInLineImage(WPicture pic, bool isfieldShape)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		stringBuilder.Append(BuildCharacterFormat(pic.CharacterFormat));
		if (pic.IsMetaFile && IsWmfImage(pic))
		{
			stringBuilder.Append(BuildMetafileProp(pic));
		}
		else
		{
			stringBuilder.Append("{" + c_slashSymbol + "shppict");
			if (pic.Size.Height > 1638f || pic.Size.Width > 1638f)
			{
				WPicture wPicture = pic.Clone() as WPicture;
				wPicture.WidthScale = 2f * wPicture.WidthScale;
				wPicture.HeightScale = 2f * wPicture.HeightScale;
				wPicture.Size = new SizeF(wPicture.Size.Width / 2f, wPicture.Size.Height / 2f);
				stringBuilder.Append(BuildPictureProp(wPicture, isfieldShape));
			}
			else
			{
				stringBuilder.Append(BuildPictureProp(pic, isfieldShape));
			}
			stringBuilder.Append("}");
			stringBuilder.Append("{" + c_slashSymbol + "nonshppict");
			stringBuilder.Append(BuildMetafileProp(pic));
			stringBuilder.Append("}");
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private string BuildShape(Shape shape)
	{
		if (shape.AutoShapeType == AutoShapeType.Unknown)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (shape.FallbackPic == null && shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			shape.WrapFormat.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
		}
		stringBuilder.Append("{" + c_slashSymbol + "shp");
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
		BuildShapeBasicTokens(shape, stringBuilder);
		BuildShapePositionTokens(shape, stringBuilder);
		BuildShapeObjectTypeTokens(shape, stringBuilder);
		BuildShapeHorizontalLineTokens(shape, stringBuilder);
		BuildShapeLineTokens(shape, stringBuilder);
		if (shape.IsFillStyleInline)
		{
			BuildShapeFillTokens(shape, stringBuilder);
		}
		if (shape.ShapeGuide.Count != 0)
		{
			BuildShapeAdjustValuesTokens(shape, stringBuilder);
		}
		if (shape.EffectList.Count >= 1 && shape.EffectList[0].IsShadowEffect)
		{
			BuildShapeShadowTokens(shape, stringBuilder);
		}
		if (shape.EffectList.Count == 2 && shape.EffectList[1].IsShapeProperties)
		{
			BuildShape3DTokens(shape, stringBuilder);
		}
		stringBuilder.Append(BuildLayoutInCell(shape.LayoutInCell));
		stringBuilder.Append("}");
		int tableNestedLevel = m_tableNestedLevel;
		m_tableNestedLevel = 0;
		if (shape.TextBody.Items.Count > 0)
		{
			stringBuilder.Append("{" + c_slashSymbol + "shptxt");
			byte[] array = BuildBodyItems(shape.TextBody.Items);
			string @string = m_encoding.GetString(array, 0, array.Length);
			stringBuilder.Append(@string + "}");
		}
		m_tableNestedLevel = tableNestedLevel;
		stringBuilder.Append("}");
		if (shape.FallbackPic != null)
		{
			stringBuilder.Append(BuildPicture(shape.FallbackPic, isFieldShape: true));
		}
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private void BuildShapeBasicTokens(Shape shape, StringBuilder strBuilder)
	{
		if ((shape.Rotation >= 44f && shape.Rotation <= 134f) || (shape.Rotation >= 225f && shape.Rotation <= 314f))
		{
			float height = shape.Height;
			shape.Height = shape.Width;
			shape.Width = height;
			float num = Math.Abs(shape.Height - shape.Width) / 2f;
			if (shape.Height < shape.Width)
			{
				shape.HorizontalPosition -= num;
				shape.VerticalPosition += num;
			}
			if (shape.Height > shape.Width)
			{
				shape.VerticalPosition -= num;
				shape.HorizontalPosition += num;
			}
		}
		int shapeW = (int)Math.Round(shape.Width * 20f);
		int shapeH = (int)Math.Round(shape.Height * 20f);
		strBuilder.Append(BuildShapePosition(shape.HorizontalPosition, shape.VerticalPosition, shapeW, shapeH));
		if (shape.HorizontalOrigin == HorizontalOrigin.Page)
		{
			strBuilder.Append(c_slashSymbol + "shpbxpage");
		}
		else if (shape.HorizontalOrigin == HorizontalOrigin.Margin || shape.HorizontalOrigin == HorizontalOrigin.LeftMargin || shape.HorizontalOrigin == HorizontalOrigin.RightMargin || shape.HorizontalOrigin == HorizontalOrigin.InsideMargin || shape.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			strBuilder.Append(c_slashSymbol + "shpbxmargin");
		}
		else if (shape.HorizontalOrigin == HorizontalOrigin.Column)
		{
			strBuilder.Append(c_slashSymbol + "shpbxcolumn");
		}
		if (shape.HorizontalOrigin == HorizontalOrigin.Character || shape.HorizontalOrigin.ToString().Contains("Margin") || shape.IsRelativeHeight || shape.IsRelativeHorizontalPosition)
		{
			strBuilder.Append(c_slashSymbol + "shpbxignore");
		}
		if (shape.VerticalOrigin == VerticalOrigin.Page)
		{
			strBuilder.Append(c_slashSymbol + "shpbypage");
		}
		else if (shape.VerticalOrigin == VerticalOrigin.Margin)
		{
			strBuilder.Append(c_slashSymbol + "shpbymargin");
		}
		else if (shape.VerticalOrigin == VerticalOrigin.Paragraph)
		{
			strBuilder.Append(c_slashSymbol + "shpbypara");
		}
		if (shape.VerticalOrigin.ToString().Contains("Margin") || shape.IsRelativeWidth || shape.IsRelativeVerticalPosition)
		{
			strBuilder.Append(c_slashSymbol + "shpbyignore");
		}
		strBuilder.Append(BuildWrappingStyle(shape.WrapFormat.TextWrappingStyle, shape.WrapFormat.TextWrappingType));
		strBuilder.Append(c_slashSymbol + "shpz");
		strBuilder.Append(shape.ZOrderPosition);
		strBuilder.Append(c_slashSymbol + "shplid");
		strBuilder.Append(shape.ShapeID);
		if (shape.LockAnchor)
		{
			strBuilder.Append(c_slashSymbol + "shplockanchor");
		}
		strBuilder.Append(BuildShapeProp("shapeType", GetAutoShapeType(shape.AutoShapeType.ToString()).ToString()));
	}

	private void BuildShapePositionTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		strBuilder.Append(BuildHorAlignm(shape.HorizontalAlignment));
		strBuilder.Append(BuildVertAlignm(shape.VerticalAlignment));
		if (shape.IsRelativeHorizontalPosition)
		{
			strBuilder.Append(BuildShapeProp("posrelh", ((int)shape.RelativeHorizontalOrigin).ToString()));
		}
		else
		{
			strBuilder.Append(BuildShapeProp("posrelh", ((int)shape.HorizontalOrigin).ToString()));
		}
		if (shape.IsRelativeVerticalPosition)
		{
			strBuilder.Append(BuildShapeProp("posrelv", ((int)shape.RelativeHeightVerticalOrigin).ToString()));
		}
		else
		{
			strBuilder.Append(BuildShapeProp("posrelv", ((int)shape.VerticalOrigin).ToString()));
		}
		if (shape.RelativeWidth != 0f)
		{
			strBuilder.Append(BuildShapeProp("pctHoriz", (shape.RelativeWidth * 10f).ToString()));
		}
		if (shape.RelativeHeight != 0f)
		{
			strBuilder.Append(BuildShapeProp("pctVert", (shape.RelativeHeight * 10f).ToString()));
		}
		if (shape.RelativeVerticalPosition != 0f)
		{
			strBuilder.Append(BuildShapeProp("pctVertPos", (shape.RelativeVerticalPosition * 10f).ToString()));
		}
		if (shape.RelativeHorizontalPosition != 0f)
		{
			strBuilder.Append(BuildShapeProp("pctHorizPos", (shape.RelativeHorizontalPosition * 10f).ToString()));
		}
		if (shape.IsRelativeHeight)
		{
			strBuilder.Append(BuildShapeProp("sizerelv", ((int)shape.RelativeHeightVerticalOrigin).ToString()));
		}
		if (shape.IsRelativeWidth)
		{
			strBuilder.Append(BuildShapeProp("sizerelh", ((int)shape.RelativeWidthHorizontalOrigin).ToString()));
		}
		if (!shape.WrapFormat.AllowOverlap)
		{
			strBuilder.Append(BuildShapeProp("fAllowOverlap", "0"));
		}
		if (shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			strBuilder.Append(BuildShapeProp("fPseudoInline", "1"));
		}
	}

	private void BuildShapeObjectTypeTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		double num = shape.Rotation * 65536f;
		if (num != 0.0)
		{
			strBuilder.Append(BuildShapeProp("rotation", num.ToString()));
		}
		strBuilder.Append(BuildShapeProp("fFlipV", (!shape.FlipVertical) ? "0" : "1"));
		strBuilder.Append(BuildShapeProp("fFlipH", (!shape.FlipHorizontal) ? "0" : "1"));
		if (shape.WrapFormat.WrapPolygon.Vertices.Count != 0)
		{
			strBuilder.Append("{" + c_slashSymbol + "sp");
			strBuilder.Append("{" + c_slashSymbol + "sn ");
			strBuilder.Append("pWrapPolygonVertices");
			strBuilder.Append("}");
			strBuilder.Append("{" + c_slashSymbol + "sv ");
			foreach (PointF vertex in shape.WrapFormat.WrapPolygon.Vertices)
			{
				string text = vertex.ToString().Replace("{X=", "(");
				text = text.Replace("}", ");");
				text = text.Replace("Y=", null);
				strBuilder.Append(text);
			}
			strBuilder.Append("}");
			strBuilder.Append("}");
		}
		if (shape.WrapFormat.DistanceLeft != 0f)
		{
			strBuilder.Append(BuildShapeProp("dxWrapDistLeft", Math.Round(shape.WrapFormat.DistanceLeft * 12700f, 2).ToString()));
		}
		if (shape.WrapFormat.DistanceTop != 0f)
		{
			strBuilder.Append(BuildShapeProp("dyWrapDistTop", Math.Round(shape.WrapFormat.DistanceTop * 12700f, 2).ToString()));
		}
		if (shape.WrapFormat.DistanceRight != 0f)
		{
			strBuilder.Append(BuildShapeProp("dxWrapDistRight", Math.Round(shape.WrapFormat.DistanceRight * 12700f, 2).ToString()));
		}
		if (shape.WrapFormat.DistanceBottom != 0f)
		{
			strBuilder.Append(BuildShapeProp("dyWrapDistBottom", Math.Round(shape.WrapFormat.DistanceBottom * 12700f, 2).ToString()));
		}
		if (shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			strBuilder.Append(BuildShapeProp("fBehindDocument", "1"));
		}
		if (shape.WrapFormat.WrapPolygon.Edited)
		{
			strBuilder.Append(BuildShapeProp("fEditedWrap", "1"));
		}
		if (!shape.Visible)
		{
			strBuilder.Append(BuildShapeProp("fHidden", "1"));
		}
	}

	private void BuildShapeHorizontalLineTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		switch (shape.HorizontalAlignment)
		{
		case ShapeHorizontalAlignment.Left:
			strBuilder.Append(BuildShapeProp("alignHR", "0"));
			break;
		case ShapeHorizontalAlignment.Center:
			strBuilder.Append(BuildShapeProp("alignHR", "1"));
			break;
		case ShapeHorizontalAlignment.Right:
			strBuilder.Append(BuildShapeProp("alignHR", "2"));
			break;
		}
		if (shape.IsHorizontalRule)
		{
			strBuilder.Append(BuildShapeProp("fHorizRule", shape.IsHorizontalRule ? "1" : "0"));
		}
		if (shape.UseStandardColorHR)
		{
			strBuilder.Append(BuildShapeProp("fStandardHR", shape.UseStandardColorHR ? "1" : "0"));
		}
		if (shape.UseNoShadeHR)
		{
			strBuilder.Append(BuildShapeProp("fNoShadeHR", shape.UseNoShadeHR ? "1" : "0"));
		}
		if (shape.WidthScale != 0f)
		{
			strBuilder.Append(BuildShapeProp("pctHR", (shape.WidthScale * 10f).ToString()));
		}
	}

	private void BuildShapeLineTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		strBuilder.Append(BuildShapeProp("fLine", shape.LineFormat.Line ? "1" : "0"));
		if (shape.LineFormat.Line)
		{
			strBuilder.Append(BuildShapeProp("lineColor", GetRTFAutoShapeColor(shape.LineFormat.Color)));
			strBuilder.Append(BuildShapeProp("lineBackColor", shape.LineFormat.ForeColor.ToArgb().ToString()));
			if (shape.LineFormat.LineFormatType != LineFormatType.Solid)
			{
				strBuilder.Append(BuildShapeProp("lineType", ((int)(shape.LineFormat.LineFormatType - 1)).ToString()));
			}
			if (shape.LineFormat.Weight != 0f)
			{
				strBuilder.Append(BuildShapeProp("lineWidth", (shape.LineFormat.Weight * 12700f).ToString()));
			}
			if (shape.LineFormat.LineJoin != DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Round)
			{
				strBuilder.Append(BuildShapeProp("lineJoinStyle", ((int)shape.LineFormat.LineJoin).ToString()));
			}
			if (shape.LineFormat.MiterJoinLimit != null)
			{
				strBuilder.Append(BuildShapeProp("MiterJoinLimit", shape.LineFormat.MiterJoinLimit));
			}
			if (shape.LineFormat.Style != LineStyle.Single)
			{
				strBuilder.Append(BuildShapeProp("lineStyle", GetLineStyle(shape.LineFormat.Style.ToString()).ToString()));
			}
			if (shape.LineFormat.DashStyle != 0)
			{
				strBuilder.Append(BuildShapeProp("lineDashing", ((int)shape.LineFormat.DashStyle).ToString()));
			}
			if (shape.LineFormat.BeginArrowheadStyle != 0)
			{
				strBuilder.Append(BuildShapeProp("lineStartArrowhead", ((int)shape.LineFormat.BeginArrowheadStyle).ToString()));
			}
			if (shape.LineFormat.EndArrowheadStyle != 0)
			{
				strBuilder.Append(BuildShapeProp("lineEndArrowhead", ((int)shape.LineFormat.EndArrowheadStyle).ToString()));
			}
			if (shape.LineFormat.BeginArrowheadWidth != LineEndWidth.MediumWidthArrow)
			{
				strBuilder.Append(BuildShapeProp("lineStartArrowWidth", ((int)shape.LineFormat.BeginArrowheadWidth).ToString()));
			}
			if (shape.LineFormat.BeginArrowheadLength != LineEndLength.MediumLenArrow)
			{
				strBuilder.Append(BuildShapeProp("lineStartArrowLength", ((int)shape.LineFormat.BeginArrowheadLength).ToString()));
			}
			if (shape.LineFormat.LineCap != DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat)
			{
				strBuilder.Append(BuildShapeProp("lineEndCapStyle", ((int)shape.LineFormat.LineCap).ToString()));
			}
			if (shape.LineFormat.EndArrowheadWidth != LineEndWidth.MediumWidthArrow)
			{
				strBuilder.Append(BuildShapeProp("lineEndArrowWidth", ((int)shape.LineFormat.EndArrowheadWidth).ToString()));
			}
			if (shape.LineFormat.EndArrowheadLength != LineEndLength.MediumLenArrow)
			{
				strBuilder.Append(BuildShapeProp("lineEndArrowLength", ((int)shape.LineFormat.EndArrowheadLength).ToString()));
			}
			if (shape.LineFormat.Transparency != 0f)
			{
				strBuilder.Append(BuildShapeProp("lineOpacity", shape.LineFormat.Transparency.ToString()));
			}
		}
	}

	private void BuildShapeFillTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		if (shape.FillFormat.Fill)
		{
			strBuilder.Append(BuildShapeProp("fillType", shape.FillFormat.FillType.ToString()));
			strBuilder.Append(BuildShapeProp("fillColor", GetRTFAutoShapeColor(shape.FillFormat.Color)));
			if (!shape.FillFormat.ForeColor.IsEmpty)
			{
				strBuilder.Append(BuildShapeProp("fillBackColor", shape.FillFormat.ForeColor.ToString()));
			}
		}
		if (shape.FillFormat.Transparency != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillOpacity", Convert.ToUInt32(65536f * (1f - shape.FillFormat.Transparency / 100f)).ToString()));
		}
		if (shape.FillFormat.SecondaryOpacity != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillBackOpacity", shape.FillFormat.SecondaryOpacity.ToString()));
		}
		if (shape.FillFormat.Focus != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillFocus", shape.FillFormat.Focus.ToString()));
		}
		if (shape.FillFormat.TextureHorizontalScale != 0.0)
		{
			strBuilder.Append(BuildShapeProp("fillAngle", shape.FillFormat.TextureHorizontalScale.ToString()));
		}
		strBuilder.Append(BuildShapeProp("fRecolorFillAsPicture", shape.FillFormat.ReColor ? "1" : "0"));
		if (shape.FillFormat.FillRectangle.BottomOffset != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillRectBottom", shape.FillFormat.FillRectangle.BottomOffset.ToString()));
		}
		if (shape.FillFormat.FillRectangle.RightOffset != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillRectRight", shape.FillFormat.FillRectangle.RightOffset.ToString()));
		}
		if (shape.FillFormat.FillRectangle.LeftOffset != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillRectLeft", shape.FillFormat.FillRectangle.LeftOffset.ToString()));
		}
		if (shape.FillFormat.FillRectangle.TopOffset != 0f)
		{
			strBuilder.Append(BuildShapeProp("fillRectTop", shape.FillFormat.FillRectangle.TopOffset.ToString()));
		}
		if (!shape.FillFormat.Fill && shape.FillFormat.Color == Color.Empty)
		{
			strBuilder.Append(BuildShapeProp("fFilled", "0"));
		}
	}

	private string GetRTFAutoShapeColor(Color color)
	{
		return (65536 * color.B + 256 * color.G + color.R).ToString();
	}

	private void BuildShapeAdjustValuesTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		foreach (KeyValuePair<string, string> item in shape.ShapeGuide)
		{
			if (item.Key.Contains("adjust"))
			{
				strBuilder.Append(BuildShapeProp(item.Key, item.Value));
				continue;
			}
			ConvertDocxAdjustValues(shape, strBuilder);
			break;
		}
	}

	private void BuildShapeShadowTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		if (shape.EffectList[0].ShadowFormat.ShadowType != 0)
		{
			strBuilder.Append(BuildShapeProp("shadowType", ((int)shape.EffectList[0].ShadowFormat.ShadowType).ToString()));
		}
		strBuilder.Append(BuildShapeProp("shadowColor", GetRTFAutoShapeColor(shape.EffectList[0].ShadowFormat.Color)));
		if (shape.EffectList[0].ShadowFormat.Transparency != 0f)
		{
			strBuilder.Append(BuildShapeProp("shadowOpacity", "32768"));
		}
		if (shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetX != 0f || shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetY != 0f)
		{
			GenerateOffsetXandY(shape, strBuilder);
		}
		else
		{
			strBuilder.Append(BuildShapeProp("shadowOffsetX", shape.EffectList[0].ShadowFormat.ShadowOffsetX.ToString()));
			strBuilder.Append(BuildShapeProp("shadowOffsetY", shape.EffectList[0].ShadowFormat.ShadowOffsetY.ToString()));
		}
		strBuilder.Append(BuildShapeProp("shadowSecondOffsetX", shape.EffectList[0].ShadowFormat.ShadowOffset2X.ToString()));
		strBuilder.Append(BuildShapeProp("shadowSecondOffsetY", shape.EffectList[0].ShadowFormat.ShadowOffset2Y.ToString()));
		if (shape.EffectList[0].ShadowFormat.HorizontalSkewAngle != 0)
		{
			strBuilder.Append(BuildShapeProp("shadowScaleYToX", shape.EffectList[0].ShadowFormat.HorizontalSkewAngle.ToString()));
		}
		if (shape.EffectList[0].ShadowFormat.VerticalSkewAngle != 0)
		{
			strBuilder.Append(BuildShapeProp("shadowScaleXToY", shape.EffectList[0].ShadowFormat.VerticalSkewAngle.ToString()));
		}
		if (shape.EffectList[0].ShadowFormat.VerticalScalingFactor != 100.0)
		{
			strBuilder.Append(BuildShapeProp("shadowScaleYToY", shape.EffectList[0].ShadowFormat.VerticalScalingFactor.ToString()));
		}
		if (shape.EffectList[0].ShadowFormat.HorizontalScalingFactor != 100.0)
		{
			strBuilder.Append(BuildShapeProp("shadowScaleXToX", shape.EffectList[0].ShadowFormat.HorizontalScalingFactor.ToString()));
		}
		if (shape.EffectList[0].ShadowFormat.ShadowPerspectiveMatrix != null)
		{
			strBuilder.Append(BuildShapeProp("shadowPerspectiveY", shape.EffectList[0].ShadowFormat.ShadowPerspectiveMatrix));
		}
		strBuilder.Append(BuildShapeProp("shadowOriginX", shape.EffectList[0].ShadowFormat.OriginX.ToString()));
		strBuilder.Append(BuildShapeProp("ShadowOriginY", shape.EffectList[0].ShadowFormat.OriginY.ToString()));
		if (shape.EffectList[0].ShadowFormat.Obscured)
		{
			strBuilder.Append(BuildShapeProp("fshadowObscured", shape.EffectList[0].ShadowFormat.Obscured.ToString()));
		}
		if (shape.EffectList[0].ShadowFormat.Visible)
		{
			strBuilder.Append(BuildShapeProp("fShadow", (shape.EffectList[0].ShadowFormat.Visible ? "1" : "0").ToString()));
		}
	}

	private void GenerateOffsetXandY(Shape shape, StringBuilder strBuilder)
	{
		if (!shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetX.ToString().Contains("25400") || !shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetY.ToString().Contains("25400"))
		{
			double num = 12700f * shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetX;
			double num2 = 12700f * shape.EffectList[0].ShadowFormat.NonChoiceShadowOffsetY;
			strBuilder.Append(BuildShapeProp("shadowOffsetX", num.ToString()));
			strBuilder.Append(BuildShapeProp("shadowOffsetY", num2.ToString()));
		}
	}

	private void BuildShape3DTokens(Shape shape, StringBuilder strBuilder)
	{
		strBuilder.Append(Environment.NewLine);
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(61))
		{
			strBuilder.Append(BuildShapeProp("c3DSpecularAmt", shape.EffectList[1].ThreeDFormat.Specularity.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(37))
		{
			strBuilder.Append(BuildShapeProp("c3DDiffuseAmt", shape.EffectList[1].ThreeDFormat.Diffusity.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(58))
		{
			strBuilder.Append(BuildShapeProp("c3DShininess", shape.EffectList[1].ThreeDFormat.Shininess.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(38))
		{
			strBuilder.Append(BuildShapeProp("c3DEdgeThickness", shape.EffectList[1].ThreeDFormat.Edge.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(40))
		{
			strBuilder.Append(BuildShapeProp("c3DExtrudeForward", shape.EffectList[1].ThreeDFormat.ForeDepth.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(74))
		{
			strBuilder.Append(BuildShapeProp("c3DExtrudeBackward", Math.Round(shape.EffectList[1].ThreeDFormat.BackDepth * 12700f).ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(50))
		{
			strBuilder.Append(BuildExtrusionplane(shape.EffectList[1].ThreeDFormat.ExtrusionPlane));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(12))
		{
			strBuilder.Append(BuildShapeProp("c3DExtrusionColor", GetRtfPageBackgroundColor(shape.EffectList[1].ThreeDFormat.ExtrusionColor)));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(36))
		{
			strBuilder.Append(BuildShapeProp("c3DExtrusionColorExtMod", shape.EffectList[1].ThreeDFormat.ColorMode.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(67))
		{
			strBuilder.Append(BuildShapeProp("f3D", (!shape.EffectList[1].ThreeDFormat.Visible) ? "0" : "1"));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(72))
		{
			strBuilder.Append(BuildShapeProp("fc3DMetallic", (!shape.EffectList[1].ThreeDFormat.Metal) ? "0" : "1"));
		}
		if (shape.EffectList[1].ThreeDFormat.HasExtrusionColor)
		{
			strBuilder.Append(BuildShapeProp("fc3DUseExtrusionColor", "1"));
		}
		if (!shape.EffectList[1].ThreeDFormat.HasLightRigEffect)
		{
			strBuilder.Append(BuildShapeProp("fc3DLightFace", "0"));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(54))
		{
			strBuilder.Append(BuildShapeProp("c3DYRotationAngle", shape.EffectList[1].ThreeDFormat.RotationAngleY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(53))
		{
			strBuilder.Append(BuildShapeProp("c3DXRotationAngle", shape.EffectList[1].ThreeDFormat.RotationAngleX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(46))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationAxisX", shape.EffectList[1].ThreeDFormat.RotationX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(47))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationAxisY", shape.EffectList[1].ThreeDFormat.RotationY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(48))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationAxisZ", shape.EffectList[1].ThreeDFormat.RotationZ.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(49))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationAngle", shape.EffectList[1].ThreeDFormat.OrientationAngle.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(75))
		{
			strBuilder.Append(BuildShapeProp("fc3DRotationCenterAuto", (!shape.EffectList[1].ThreeDFormat.AutoRotationCenter) ? "0" : "1"));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(55))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationCenterX", shape.EffectList[1].ThreeDFormat.RotationCenterX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(56))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationCenterY", shape.EffectList[1].ThreeDFormat.RotationCenterY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(57))
		{
			strBuilder.Append(BuildShapeProp("c3DRotationCenterZ", shape.EffectList[1].ThreeDFormat.RotationCenterZ.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(52))
		{
			strBuilder.Append(BuildRenderMode(shape.EffectList[1].ThreeDFormat.ExtrusionRenderMode));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(62))
		{
			strBuilder.Append(BuildShapeProp("c3DXViewpoint", shape.EffectList[1].ThreeDFormat.ViewPointX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(63))
		{
			strBuilder.Append(BuildShapeProp("c3DYViewpoint", shape.EffectList[1].ThreeDFormat.ViewPointY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(64))
		{
			strBuilder.Append(BuildShapeProp("c3DZViewpoint", shape.EffectList[1].ThreeDFormat.ViewPointZ.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(65))
		{
			strBuilder.Append(BuildShapeProp("c3DOriginX", shape.EffectList[1].ThreeDFormat.ViewPointOriginX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(66))
		{
			strBuilder.Append(BuildShapeProp("c3DOriginY", shape.EffectList[1].ThreeDFormat.ViewPointOriginY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(60))
		{
			strBuilder.Append(BuildShapeProp("c3DSkewAngle", shape.EffectList[1].ThreeDFormat.SkewAngle.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(59))
		{
			strBuilder.Append(BuildShapeProp("c3DSkewAmount", shape.EffectList[1].ThreeDFormat.SkewAmount.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(34))
		{
			strBuilder.Append(BuildShapeProp("c3DAmbientIntensity", shape.EffectList[1].ThreeDFormat.Brightness.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(31))
		{
			strBuilder.Append(BuildShapeProp("c3DKeyX", shape.EffectList[1].ThreeDFormat.LightRigRotationX.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(32))
		{
			strBuilder.Append(BuildShapeProp("c3DKeyY", shape.EffectList[1].ThreeDFormat.LightRigRotationY.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(33))
		{
			strBuilder.Append(BuildShapeProp("c3DKeyZ", shape.EffectList[1].ThreeDFormat.LightRigRotationZ.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(41))
		{
			strBuilder.Append(BuildShapeProp("c3DKeyIntensity", shape.EffectList[1].ThreeDFormat.LightLevel.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(43))
		{
			strBuilder.Append(BuildShapeProp("c3DFillX", shape.EffectList[1].ThreeDFormat.LightRigRotation2X.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(44))
		{
			strBuilder.Append(BuildShapeProp("c3DFillY", shape.EffectList[1].ThreeDFormat.LightRigRotation2Y.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(45))
		{
			strBuilder.Append(BuildShapeProp("c3DFillZ", shape.EffectList[1].ThreeDFormat.LightRigRotation2Z.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(42))
		{
			strBuilder.Append(BuildShapeProp("c3DFillIntensity", shape.EffectList[1].ThreeDFormat.LightLevel2.ToString()));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(69))
		{
			strBuilder.Append(BuildShapeProp("fc3DKeyHarsh", (!shape.EffectList[1].ThreeDFormat.LightHarsh) ? "0" : "1"));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(70))
		{
			strBuilder.Append(BuildShapeProp("fc3DFillHarsh", (!shape.EffectList[1].ThreeDFormat.LightHarsh2) ? "0" : "1"));
		}
		if (shape.EffectList[1].ThreeDFormat.PropertiesHash.ContainsKey(24))
		{
			if (shape.EffectList[1].ThreeDFormat.CameraPresetType.ToString().ToLower().Contains("oblique"))
			{
				strBuilder.Append(BuildShapeProp("fc3DParallel", "1"));
			}
			else
			{
				strBuilder.Append(BuildShapeProp("fc3DParallel", "0"));
			}
		}
	}

	private void ConvertDocxAdjustValues(Shape shape, StringBuilder strBuilder)
	{
		switch (shape.AutoShapeType)
		{
		case AutoShapeType.RoundedRectangle:
		case AutoShapeType.Octagon:
		case AutoShapeType.IsoscelesTriangle:
		case AutoShapeType.Cross:
		case AutoShapeType.Cube:
		case AutoShapeType.Bevel:
		case AutoShapeType.Sun:
		case AutoShapeType.Moon:
		case AutoShapeType.DoubleBracket:
		case AutoShapeType.DoubleBrace:
		case AutoShapeType.Plaque:
		case AutoShapeType.ElbowConnector:
		case AutoShapeType.CurvedConnector:
		{
			foreach (KeyValuePair<string, string> item in shape.ShapeGuide)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", ((double)int.Parse(item.Value.Substring(4)) / 4.63).ToString()));
			}
			break;
		}
		case AutoShapeType.FoldedCorner:
		{
			foreach (KeyValuePair<string, string> item2 in shape.ShapeGuide)
			{
				double num29 = int.Parse(item2.Value.Substring(4));
				strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(Math.Abs(num29 / 4.63 - 21600.0)).ToString()));
			}
			break;
		}
		case AutoShapeType.SmileyFace:
		{
			foreach (KeyValuePair<string, string> item3 in shape.ShapeGuide)
			{
				double num16 = Math.Abs(int.Parse(item3.Value.Substring(4)));
				if (num16 != 4653.0)
				{
					num16 = Math.Abs((num16 + 4563.0) / 4.63 - 17520.0);
					strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(num16).ToString()));
				}
			}
			break;
		}
		case AutoShapeType.Parallelogram:
		{
			if (shape.Height < shape.Width)
			{
				float num11 = shape.Width - shape.Height;
				double num12 = Math.Round((double)(25000f / shape.Height) * (double)num11 + 25000.0);
				break;
			}
			using Dictionary<string, string>.Enumerator enumerator = shape.ShapeGuide.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current4 = enumerator.Current;
				if (int.Parse(current4.Value.Substring(4)) == 25000)
				{
					shape.ShapeGuide.Clear();
					break;
				}
				double a2 = (double)int.Parse(current4.Value.Substring(4)) / 4.63;
				strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a2).ToString()));
			}
			break;
		}
		case AutoShapeType.Hexagon:
		{
			if (shape.Height < shape.Width)
			{
				float num30 = shape.Width - shape.Height;
				double num31 = Math.Round((double)(25000f / shape.Height) * (double)num30 + 25000.0);
				break;
			}
			using Dictionary<string, string>.Enumerator enumerator = shape.ShapeGuide.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current13 = enumerator.Current;
				if (int.Parse(current13.Value.Substring(4)) == 25000)
				{
					shape.ShapeGuide.Clear();
				}
				else
				{
					strBuilder.Append(BuildShapeProp("adjustValue", ((double)int.Parse(current13.Value.Substring(4)) / 4.63).ToString()));
				}
			}
			break;
		}
		case AutoShapeType.Can:
		{
			if (shape.Height > shape.Width)
			{
				float num44 = shape.Height - shape.Width;
				double num45 = Math.Round((double)(25000f / shape.Width) * (double)num44 + 25000.0);
				{
					foreach (KeyValuePair<string, string> item4 in shape.ShapeGuide)
					{
						if (num45 != (double)int.Parse(item4.Value.Substring(4)))
						{
							double num46 = int.Parse(item4.Value.Substring(4));
							double num47 = 4.62962963 / (double)shape.Width * (double)num44 + 4.62962963;
							double a10 = num46 / num47;
							strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a10).ToString()));
							break;
						}
					}
					break;
				}
			}
			using Dictionary<string, string>.Enumerator enumerator = shape.ShapeGuide.GetEnumerator();
			if (enumerator.MoveNext())
			{
				KeyValuePair<string, string> current19 = enumerator.Current;
				if (int.Parse(current19.Value.Substring(4)) == 25000)
				{
					shape.ShapeGuide.Clear();
				}
				else
				{
					strBuilder.Append(BuildShapeProp("adjustValue", ((double)int.Parse(current19.Value.Substring(4)) / 4.63).ToString()));
				}
			}
			break;
		}
		case AutoShapeType.LeftBracket:
		case AutoShapeType.RightBracket:
			if (shape.Height > shape.Width)
			{
				float num34 = shape.Height - shape.Width;
				double num35 = Math.Round(8333f / shape.Width * num34 + 8333f);
				{
					foreach (KeyValuePair<string, string> item5 in shape.ShapeGuide)
					{
						if (num35 != (double)int.Parse(item5.Value.Substring(4)))
						{
							float num36 = int.Parse(item5.Value.Substring(4));
							double num37 = 4.62962963 / (double)shape.Width * (double)num34 + 4.62962963;
							double a8 = (double)num36 / num37;
							strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a8).ToString()));
						}
					}
					break;
				}
			}
			{
				foreach (KeyValuePair<string, string> item6 in shape.ShapeGuide)
				{
					if (item6.Value.Substring(4) == "8333")
					{
						shape.ShapeGuide.Clear();
						break;
					}
					double a9 = (double)int.Parse(item6.Value.Substring(4)) / 4.63;
					strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a9).ToString()));
				}
				break;
			}
		case AutoShapeType.LeftBrace:
		case AutoShapeType.RightBrace:
			if (shape.Height > shape.Width)
			{
				float num24 = shape.Height - shape.Width;
				double num25 = Math.Round(8333f / shape.Width * num24 + 8333f);
				{
					foreach (KeyValuePair<string, string> item7 in shape.ShapeGuide)
					{
						if (num25 != (double)int.Parse(item7.Value.Substring(4)))
						{
							if (item7.Key == "adj1")
							{
								float num26 = int.Parse(item7.Value.Substring(4));
								double num27 = 4.62962963 / (double)shape.Width * (double)num24 + 4.62962963;
								double a4 = (double)num26 / num27;
								strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a4).ToString()));
							}
							else
							{
								double a5 = (double)int.Parse(item7.Value.Substring(4)) / 4.63;
								strBuilder.Append(BuildShapeProp("adjust2Value", Math.Round(a5).ToString()));
							}
						}
					}
					break;
				}
			}
			{
				foreach (KeyValuePair<string, string> item8 in shape.ShapeGuide)
				{
					if (item8.Value.Substring(4) == "8333")
					{
						shape.ShapeGuide.Clear();
						break;
					}
					double a6 = (double)int.Parse(item8.Value.Substring(4)) / 4.63;
					if (item8.Key == "adj1")
					{
						strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a6).ToString()));
					}
					else if (item8.Key == "adj2" && item8.Value.Substring(4) == "50000")
					{
						strBuilder.Append(BuildShapeProp("adjust2Value", Math.Round(a6).ToString()));
					}
				}
				break;
			}
		case AutoShapeType.Star4Point:
		case AutoShapeType.Star8Point:
		case AutoShapeType.Star16Point:
		case AutoShapeType.Star24Point:
		case AutoShapeType.Star32Point:
		{
			foreach (KeyValuePair<string, string> item9 in shape.ShapeGuide)
			{
				double a3 = (double)(50000 - int.Parse(item9.Value.Substring(4))) / 4.62962963;
				strBuilder.Append(BuildShapeProp("adjustValue", Math.Round(a3).ToString()));
			}
			break;
		}
		case AutoShapeType.UpRibbon:
		{
			double num = -1.0;
			double num2 = -1.0;
			foreach (KeyValuePair<string, string> item10 in shape.ShapeGuide)
			{
				float num41 = int.Parse(item10.Value.Substring(4));
				if (item10.Key == "adj2")
				{
					double num42 = Math.Abs((double)(num41 - 75000f) / 9.25925926);
					num = Math.Round(2700.0 + num42);
				}
				else
				{
					double num43 = (double)num41 / 4.62962963;
					num2 = Math.Round(21600.0 - num43);
				}
			}
			if (num >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			}
			if (num2 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			}
			break;
		}
		case AutoShapeType.DownRibbon:
		{
			double num = -1.0;
			double num2 = -1.0;
			foreach (KeyValuePair<string, string> item11 in shape.ShapeGuide)
			{
				if (item11.Value != "val 12500" && item11.Value != "val 50000")
				{
					double num28 = Math.Abs((double)((float)int.Parse(item11.Value.Substring(4)) - 75000f) / 9.25925926);
					num28 = Math.Round(2700.0 + num28);
					if (item11.Key == "adj2")
					{
						num = num28;
					}
					else
					{
						num2 = num28;
					}
				}
			}
			if (num >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			}
			if (num2 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			}
			break;
		}
		case AutoShapeType.CurvedUpRibbon:
		{
			double num = -1.0;
			double num2 = -1.0;
			double num3 = -1.0;
			foreach (KeyValuePair<string, string> item12 in shape.ShapeGuide)
			{
				float num38 = int.Parse(item12.Value.Substring(4));
				if (item12.Key == "adj1" && num38 != 25000f)
				{
					double num39 = (double)(41667f - num38) / 4.62962963;
					num2 = Math.Round(12600.0 + num39);
				}
				else if (item12.Key == "adj2" && num38 != 50000f)
				{
					double num40 = (double)(75000f - num38) / 9.25925926;
					num = Math.Round(2700.0 + num40);
				}
				else if (item12.Key == "adj3" && num38 != 12500f)
				{
					num3 = Math.Round((double)num38 / 4.62962963);
				}
			}
			if (num >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			}
			if (num2 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			}
			if (num3 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust3Value", num3.ToString()));
			}
			break;
		}
		case AutoShapeType.CurvedDownRibbon:
		{
			double num = -1.0;
			double num2 = -1.0;
			double num3 = -1.0;
			foreach (KeyValuePair<string, string> item13 in shape.ShapeGuide)
			{
				float num20 = int.Parse(item13.Value.Substring(4));
				if (item13.Key == "adj1" && num20 != 25000f)
				{
					double num21 = (double)(41667f - num20) / 4.62962963;
					num2 = Math.Round(9000.0 - num21);
				}
				else if (item13.Key == "adj2" && num20 != 50000f)
				{
					double num22 = (double)(75000f - num20) / 9.25925926;
					num = Math.Round(2700.0 + num22);
				}
				else if (item13.Key == "adj3" && num20 != 12500f)
				{
					double num23 = (double)(num20 - 3125f) / 4.62962963;
					num3 = Math.Round(20925.0 - num23);
				}
			}
			if (num >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			}
			if (num2 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			}
			if (num3 >= 0.0)
			{
				strBuilder.Append(BuildShapeProp("adjust3Value", num3.ToString()));
			}
			break;
		}
		case AutoShapeType.VerticalScroll:
		case AutoShapeType.HorizontalScroll:
		{
			foreach (KeyValuePair<string, string> item14 in shape.ShapeGuide)
			{
				strBuilder.Append(BuildShapeProp("adjustValue", Math.Round((double)(float)int.Parse(item14.Value.Substring(4)) / 4.62962963).ToString()));
			}
			break;
		}
		case AutoShapeType.Wave:
		case AutoShapeType.DoubleWave:
		{
			foreach (KeyValuePair<string, string> item15 in shape.ShapeGuide)
			{
				float num18 = int.Parse(item15.Value.Substring(4));
				if (item15.Key == "adj1")
				{
					strBuilder.Append(BuildShapeProp("adjustValue", Math.Round((double)num18 / 4.62962963).ToString()));
					continue;
				}
				double num19 = Math.Round((double)num18 / 4.62962963);
				strBuilder.Append(BuildShapeProp("adjustValue", (10800.0 + num19).ToString()));
			}
			break;
		}
		case AutoShapeType.RectangularCallout:
		case AutoShapeType.RoundedRectangularCallout:
		case AutoShapeType.OvalCallout:
		case AutoShapeType.CloudCallout:
		{
			foreach (KeyValuePair<string, string> item16 in shape.ShapeGuide)
			{
				double num17 = Math.Round((double)((float)int.Parse(item16.Value.Substring(4)) + 50000f) / 4.62962963);
				if (item16.Key == "adj1")
				{
					strBuilder.Append(BuildShapeProp("adjustValue", num17.ToString()));
				}
				else if (item16.Key == "adj2")
				{
					strBuilder.Append(BuildShapeProp("adjust2Value", num17.ToString()));
				}
			}
			break;
		}
		case AutoShapeType.LineCallout1:
		case AutoShapeType.LineCallout1NoBorder:
		case AutoShapeType.LineCallout1AccentBar:
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			foreach (KeyValuePair<string, string> item17 in shape.ShapeGuide)
			{
				double num15 = Math.Round((double)(float)int.Parse(item17.Value.Substring(4)) / 4.62962963);
				if (item17.Key == "adj1")
				{
					num4 = num15;
				}
				else if (item17.Key == "adj2")
				{
					num3 = num15;
				}
				else if (item17.Key == "adj3")
				{
					num2 = num15;
				}
				else if (item17.Key == "adj4")
				{
					num = num15;
				}
			}
			strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			strBuilder.Append(BuildShapeProp("adjust3Value", num3.ToString()));
			strBuilder.Append(BuildShapeProp("adjust4Value", num4.ToString()));
			break;
		}
		case AutoShapeType.LineCallout2:
		case AutoShapeType.LineCallout2AccentBar:
		case AutoShapeType.LineCallout2NoBorder:
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			double num5 = 0.0;
			double num6 = 0.0;
			foreach (KeyValuePair<string, string> item18 in shape.ShapeGuide)
			{
				double num10 = Math.Round((double)(float)int.Parse(item18.Value.Substring(4)) / 4.62962963);
				if (item18.Key == "adj1")
				{
					num6 = num10;
				}
				else if (item18.Key == "adj2")
				{
					num5 = num10;
				}
				else if (item18.Key == "adj3")
				{
					num4 = num10;
				}
				else if (item18.Key == "adj4")
				{
					num3 = num10;
				}
				else if (item18.Key == "adj5")
				{
					num2 = num10;
				}
				else if (item18.Key == "adj6")
				{
					num = num10;
				}
			}
			strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			strBuilder.Append(BuildShapeProp("adjust3Value", num3.ToString()));
			strBuilder.Append(BuildShapeProp("adjust4Value", num4.ToString()));
			strBuilder.Append(BuildShapeProp("adjust5Value", num5.ToString()));
			strBuilder.Append(BuildShapeProp("adjust6Value", num6.ToString()));
			break;
		}
		case AutoShapeType.LineCallout3:
		case AutoShapeType.LineCallout3AccentBar:
		case AutoShapeType.LineCallout3NoBorder:
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			double num5 = 0.0;
			double num6 = 0.0;
			double num7 = 0.0;
			double num8 = 0.0;
			foreach (KeyValuePair<string, string> item19 in shape.ShapeGuide)
			{
				double num9 = Math.Round((double)(float)int.Parse(item19.Value.Substring(4)) / 4.62962963);
				if (item19.Key == "adj1")
				{
					num8 = num9;
				}
				else if (item19.Key == "adj2")
				{
					num7 = num9;
				}
				else if (item19.Key == "adj3")
				{
					num6 = num9;
				}
				else if (item19.Key == "adj4")
				{
					num5 = num9;
				}
				else if (item19.Key == "adj5")
				{
					num4 = num9;
				}
				else if (item19.Key == "adj6")
				{
					num3 = num9;
				}
				else if (item19.Key == "adj7")
				{
					num2 = num9;
				}
				else if (item19.Key == "adj8")
				{
					num = num9;
				}
			}
			strBuilder.Append(BuildShapeProp("adjustValue", num.ToString()));
			strBuilder.Append(BuildShapeProp("adjust2Value", num2.ToString()));
			strBuilder.Append(BuildShapeProp("adjust3Value", num3.ToString()));
			strBuilder.Append(BuildShapeProp("adjust4Value", num4.ToString()));
			strBuilder.Append(BuildShapeProp("adjust5Value", num5.ToString()));
			strBuilder.Append(BuildShapeProp("adjust6Value", num6.ToString()));
			strBuilder.Append(BuildShapeProp("adjust7Value", num7.ToString()));
			strBuilder.Append(BuildShapeProp("adjust8Value", num8.ToString()));
			break;
		}
		}
	}

	private string BuildRenderMode(ExtrusionRenderMode extrusionRenderMode)
	{
		return extrusionRenderMode switch
		{
			ExtrusionRenderMode.Solid => BuildShapeProp("c3DRenderMode", "0"), 
			ExtrusionRenderMode.Wireframe => BuildShapeProp("c3DRenderMode", "1"), 
			ExtrusionRenderMode.BoundingCube => BuildShapeProp("c3DRenderMode", "2"), 
			_ => string.Empty, 
		};
	}

	private string BuildExtrusionplane(ExtrusionPlane extrusionPlane)
	{
		return extrusionPlane switch
		{
			ExtrusionPlane.XY => BuildShapeProp("c3DExtrudePlane", "0"), 
			ExtrusionPlane.YZ => BuildShapeProp("c3DExtrudePlane", "1"), 
			ExtrusionPlane.ZX => BuildShapeProp("c3DExtrudePlane", "2"), 
			_ => string.Empty, 
		};
	}

	private bool IsWmfImage(WPicture picture)
	{
		if (WordDocument.EnablePartialTrustCode)
		{
			if (picture.ImageForPartialTrustMode.Format != DocGen.DocIO.DLS.Entities.ImageFormat.Wmf)
			{
				if (picture.PictureShape != null && picture.PictureShape.ShapeContainer != null && picture.PictureShape.ShapeContainer.Bse != null && picture.PictureShape.ShapeContainer.Bse.Blip != null && picture.PictureShape.ShapeContainer.Bse.Blip != null && picture.PictureShape.ShapeContainer.Bse.Blip.Header != null)
				{
					return picture.PictureShape.ShapeContainer.Bse.Blip.Header.Type == MSOFBT.msofbtBlipWMF;
				}
				return false;
			}
			return true;
		}
		if (picture.ImageRecord.ImageFormat != DocGen.DocIO.DLS.Entities.ImageFormat.Wmf)
		{
			if (picture.PictureShape != null && picture.PictureShape.ShapeContainer != null && picture.PictureShape.ShapeContainer.Bse != null && picture.PictureShape.ShapeContainer.Bse.Blip != null && picture.PictureShape.ShapeContainer.Bse.Blip != null && picture.PictureShape.ShapeContainer.Bse.Blip.Header != null)
			{
				return picture.PictureShape.ShapeContainer.Bse.Blip.Header.Type == MSOFBT.msofbtBlipWMF;
			}
			return false;
		}
		return true;
	}

	private string BuildPictureProp(WPicture pic, bool isFieldshape)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "pict");
		if (isFieldshape)
		{
			stringBuilder.Append("{\\*\\picprop\\defshp{\\sp{\\sn fPseudoInline}{\\sv 1}}{\\sp{\\sn fLayoutInCell}{\\sv 1}}{\\sp{\\sn fLockPosition}{\\sv 1}}{\\sp{\\sn fLockRotation}{\\sv 1}}}");
		}
		stringBuilder.Append(c_slashSymbol + "picscalex");
		stringBuilder.Append(Math.Round(pic.WidthScale));
		stringBuilder.Append(c_slashSymbol + "picscaley");
		stringBuilder.Append(Math.Round(pic.HeightScale));
		stringBuilder.Append(c_slashSymbol + "picwgoal");
		stringBuilder.Append(Math.Round(pic.Size.Width * 20f));
		stringBuilder.Append(c_slashSymbol + "pichgoal");
		stringBuilder.Append(Math.Round(pic.Size.Height * 20f));
		stringBuilder.Append(c_slashSymbol + "picw");
		stringBuilder.Append(Math.Round(pic.Size.Width * 35.5f));
		stringBuilder.Append(c_slashSymbol + "pich");
		stringBuilder.Append(Math.Round(pic.Size.Height * 35.5f));
		if (pic.IsMetaFile)
		{
			stringBuilder.Append(c_slashSymbol + "wmetafile8");
		}
		else if (pic.Image.RawFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Png))
		{
			stringBuilder.Append(c_slashSymbol + "pngblip");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "jpegblip");
		}
		stringBuilder.Append(" ");
		byte[] array = pic.ImageBytes;
		if (pic.ImageRecord != null && pic.IsMetaFile && IsWmfImage(pic) && pic.ImageRecord.IsMetafileHeaderPresent(pic.ImageBytes))
		{
			byte[] array2 = new byte[pic.ImageBytes.Length - 22];
			Buffer.BlockCopy(pic.ImageBytes, 22, array2, 0, pic.ImageBytes.Length - 22);
			array = array2;
		}
		StringBuilder stringBuilder2 = new StringBuilder(BitConverter.ToString(array));
		stringBuilder2.Replace("-", "");
		stringBuilder.Append(stringBuilder2);
		stringBuilder.Append("}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildPictureLink(WPicture pic)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "hl");
		stringBuilder.Append("{" + c_slashSymbol + "hlfr ");
		stringBuilder.Append(pic.Href);
		stringBuilder.Append("}");
		stringBuilder.Append("{" + c_slashSymbol + "hlsrc ");
		stringBuilder.Append(pic.Href);
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildMetafileProp(WPicture pic)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "pict");
		stringBuilder.Append(c_slashSymbol + "picscalex");
		stringBuilder.Append(Math.Round(pic.WidthScale));
		stringBuilder.Append(c_slashSymbol + "picscaley");
		stringBuilder.Append(Math.Round(pic.HeightScale));
		stringBuilder.Append(c_slashSymbol + "picwgoal");
		stringBuilder.Append(Math.Round(pic.Size.Width * 20f));
		stringBuilder.Append(c_slashSymbol + "pichgoal");
		stringBuilder.Append(Math.Round(pic.Size.Height * 20f));
		stringBuilder.Append(c_slashSymbol + "picw");
		stringBuilder.Append(Math.Round(pic.Size.Width * 35.5f));
		stringBuilder.Append(c_slashSymbol + "pich");
		stringBuilder.Append(Math.Round(pic.Size.Height * 35.5f));
		stringBuilder.Append(c_slashSymbol + "wmetafile8");
		stringBuilder.Append(" ");
		byte[] array = null;
		if (pic.IsMetaFile && IsWmfImage(pic))
		{
			array = pic.ImageBytes;
			if (pic.ImageRecord.IsMetafileHeaderPresent(pic.ImageBytes))
			{
				byte[] array2 = new byte[pic.ImageBytes.Length - 22];
				Buffer.BlockCopy(pic.ImageBytes, 22, array2, 0, pic.ImageBytes.Length - 22);
				array = array2;
			}
		}
		if (array != null)
		{
			StringBuilder stringBuilder2 = new StringBuilder(BitConverter.ToString(array));
			stringBuilder2.Replace("-", "");
			stringBuilder.Append(stringBuilder2);
		}
		stringBuilder.Append("}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildWrappingStyle(TextWrappingStyle style, TextWrappingType type)
	{
		return style switch
		{
			TextWrappingStyle.TopAndBottom => c_slashSymbol + "shpwr1" + BuildWrappingType(type), 
			TextWrappingStyle.Square => c_slashSymbol + "shpwr2" + BuildWrappingType(type), 
			TextWrappingStyle.InFrontOfText => c_slashSymbol + "shpwr3" + BuildWrappingType(type) + "\\shpfblwtxt0", 
			TextWrappingStyle.Tight => c_slashSymbol + "shpwr4" + BuildWrappingType(type), 
			TextWrappingStyle.Through => c_slashSymbol + "shpwr5" + BuildWrappingType(type), 
			TextWrappingStyle.Behind => c_slashSymbol + "shpwr3" + BuildWrappingType(type) + "\\shpfblwtxt1", 
			TextWrappingStyle.Inline => c_slashSymbol + "shpwr3" + BuildWrappingType(type) + "\\shpfblwtxt1", 
			_ => string.Empty, 
		};
	}

	private string BuildWrappingType(TextWrappingType type)
	{
		return type switch
		{
			TextWrappingType.Both => c_slashSymbol + "shpwrk0", 
			TextWrappingType.Left => c_slashSymbol + "shpwrk1", 
			TextWrappingType.Right => c_slashSymbol + "shpwrk2", 
			TextWrappingType.Largest => c_slashSymbol + "shpwrk3", 
			_ => string.Empty, 
		};
	}

	private string BuildShapeProp(string propName, string propValue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "sp");
		stringBuilder.Append("{" + c_slashSymbol + "sn ");
		stringBuilder.Append(propName);
		stringBuilder.Append("}");
		stringBuilder.Append("{" + c_slashSymbol + "sv ");
		stringBuilder.Append(propValue);
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private int GetAutoShapeType(string shapeValue)
	{
		return shapeValue switch
		{
			"Rectangle" => 1, 
			"RoundedRectangle" => 2, 
			"Oval" => 3, 
			"Diamond" => 4, 
			"IsoscelesTriangle" => 5, 
			"RightTriangle" => 6, 
			"Parallelogram" => 7, 
			"Trapezoid" => 8, 
			"Hexagon" => 9, 
			"Octagon" => 10, 
			"Cross" => 11, 
			"Star5Point" => 12, 
			"RightArrow" => 13, 
			"Pentagon" => 15, 
			"Cube" => 16, 
			"RoundedRectangularCallout" => 17, 
			"Star16Point" => 18, 
			"Arc" => 19, 
			"Line" => 20, 
			"Plaque" => 21, 
			"Can" => 22, 
			"Donut" => 23, 
			"StraightConnector" => 32, 
			"BentConnector2" => 33, 
			"ElbowConnector" => 34, 
			"BentConnector4" => 35, 
			"BentConnector5" => 36, 
			"CurvedConnector2" => 37, 
			"CurvedConnector" => 38, 
			"CurvedConnector4" => 39, 
			"CurvedConnector5" => 40, 
			"LineCallout1NoBorder" => 41, 
			"LineCallout2NoBorder" => 42, 
			"LineCallout3NoBorder" => 43, 
			"LineCallout1AccentBar" => 44, 
			"LineCallout2AccentBar" => 45, 
			"LineCallout3AccentBar" => 46, 
			"LineCallout1" => 47, 
			"LineCallout2" => 48, 
			"LineCallout3" => 49, 
			"LineCallout1BorderAndAccentBar" => 50, 
			"LineCallout2BorderAndAccentBar" => 51, 
			"LineCallout3BorderAndAccentBar" => 52, 
			"DownRibbon" => 53, 
			"UpRibbon" => 54, 
			"Chevron" => 55, 
			"RegularPentagon" => 56, 
			"NoSymbol" => 57, 
			"Star8Point" => 58, 
			"Star32Point" => 60, 
			"RectangularCallout" => 61, 
			"OvalCallout" => 63, 
			"Wave" => 64, 
			"FoldedCorner" => 65, 
			"LeftArrow" => 66, 
			"DownArrow" => 67, 
			"UpArrow" => 68, 
			"LeftRightArrow" => 69, 
			"UpDownArrow" => 70, 
			"Explosion1" => 71, 
			"Explosion2" => 72, 
			"LightningBolt" => 73, 
			"Heart" => 74, 
			"QuadArrow" => 76, 
			"LeftArrowCallout" => 77, 
			"RightArrowCallout" => 78, 
			"UpArrowCallout" => 79, 
			"DownArrowCallout" => 80, 
			"LeftRightArrowCallout" => 81, 
			"UpDownArrowCallout" => 82, 
			"QuadArrowCallout" => 83, 
			"Bevel" => 84, 
			"LeftBracket" => 85, 
			"RightBracket" => 86, 
			"LeftBrace" => 87, 
			"RightBrace" => 88, 
			"LeftUpArrow" => 89, 
			"BentUpArrow" => 90, 
			"BentArrow" => 91, 
			"Star24Point" => 92, 
			"StripedRightArrow" => 93, 
			"NotchedRightArrow" => 94, 
			"BlockArc" => 95, 
			"SmileyFace" => 96, 
			"VerticalScroll" => 97, 
			"HorizontalScroll" => 98, 
			"CircularArrow" => 99, 
			"UTurnArrow" => 101, 
			"CurvedRightArrow" => 102, 
			"CurvedLeftArrow" => 103, 
			"CurvedUpArrow" => 104, 
			"CurvedDownArrow" => 105, 
			"CloudCallout" => 106, 
			"CurvedDownRibbon" => 107, 
			"CurvedUpRibbon" => 108, 
			"FlowChartProcess" => 109, 
			"FlowChartDecision" => 110, 
			"FlowChartData" => 111, 
			"FlowChartPredefinedProcess" => 112, 
			"FlowChartInternalStorage" => 113, 
			"FlowChartDocument" => 114, 
			"FlowChartMultiDocument" => 115, 
			"FlowChartTerminator" => 116, 
			"FlowChartPreparation" => 117, 
			"FlowChartManualInput" => 118, 
			"FlowChartManualOperation" => 119, 
			"FlowChartConnector" => 120, 
			"FlowChartCard" => 121, 
			"FlowChartPunchedTape" => 122, 
			"FlowChartSummingJunction" => 123, 
			"FlowChartOr" => 124, 
			"FlowChartCollate" => 125, 
			"FlowChartSort" => 126, 
			"FlowChartExtract" => 127, 
			"FlowChartMerge" => 128, 
			"FlowChartStoredData" => 130, 
			"FlowChartSequentialAccessStorage" => 131, 
			"FlowChartMagneticDisk" => 132, 
			"FlowChartDirectAccessStorage" => 133, 
			"FlowChartDisplay" => 134, 
			"FlowChartDelay" => 135, 
			"FlowChartAlternateProcess" => 176, 
			"FlowChartOffPageConnector" => 177, 
			"LeftRightUpArrow" => 182, 
			"Sun" => 183, 
			"Moon" => 184, 
			"DoubleBracket" => 185, 
			"DoubleBrace" => 186, 
			"Star4Point" => 187, 
			"DoubleWave" => 188, 
			_ => 0, 
		};
	}

	private int GetLineStyle(string lineStyle)
	{
		return lineStyle switch
		{
			"ThinThin" => 1, 
			"ThickThin" => 2, 
			"ThinThick" => 3, 
			"ThickBetweenThin" => 4, 
			_ => 0, 
		};
	}

	private string BuildLayoutInCell(bool isLayoutInCell)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "sp");
		stringBuilder.Append("{" + c_slashSymbol + "sn ");
		stringBuilder.Append("fLayoutInCell");
		stringBuilder.Append("}");
		stringBuilder.Append("{" + c_slashSymbol + "sv ");
		stringBuilder.Append(isLayoutInCell ? "1" : "0");
		stringBuilder.Append("}}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildHorAlignm(ShapeHorizontalAlignment hAlignm)
	{
		return hAlignm switch
		{
			ShapeHorizontalAlignment.Left => BuildShapeProp("posh", "1"), 
			ShapeHorizontalAlignment.Center => BuildShapeProp("posh", "2"), 
			ShapeHorizontalAlignment.Right => BuildShapeProp("posh", "3"), 
			ShapeHorizontalAlignment.Inside => BuildShapeProp("posh", "4"), 
			ShapeHorizontalAlignment.Outside => BuildShapeProp("posh", "5"), 
			_ => string.Empty, 
		};
	}

	private string BuildVertAlignm(ShapeVerticalAlignment vAlignm)
	{
		return vAlignm switch
		{
			ShapeVerticalAlignment.Top => BuildShapeProp("posv", "1"), 
			ShapeVerticalAlignment.Center => BuildShapeProp("posv", "2"), 
			ShapeVerticalAlignment.Bottom => BuildShapeProp("posv", "3"), 
			ShapeVerticalAlignment.Inside => BuildShapeProp("posv", "4"), 
			ShapeVerticalAlignment.Outside => BuildShapeProp("posv", "5"), 
			_ => string.Empty, 
		};
	}

	private string BuildHiddenTextBox(bool value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "sp");
		stringBuilder.Append("{" + c_slashSymbol + "sn ");
		stringBuilder.Append("fHidden");
		stringBuilder.Append("}");
		stringBuilder.Append("{" + c_slashSymbol + "sv ");
		if (value)
		{
			stringBuilder.Append("0");
		}
		else
		{
			stringBuilder.Append("1");
		}
		stringBuilder.Append("}}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string BuildHorPos(HorizontalOrigin hPos)
	{
		return hPos switch
		{
			HorizontalOrigin.Margin => BuildShapeProp("posrelh", "0"), 
			HorizontalOrigin.Page => BuildShapeProp("posrelh", "1"), 
			HorizontalOrigin.Column => BuildShapeProp("posrelh", "2"), 
			HorizontalOrigin.Character => BuildShapeProp("posrelh", "3"), 
			_ => string.Empty, 
		};
	}

	private string BuildVertPos(VerticalOrigin vPos)
	{
		return vPos switch
		{
			VerticalOrigin.Margin => BuildShapeProp("posrelv", "0"), 
			VerticalOrigin.Page => BuildShapeProp("posrelv", "1"), 
			VerticalOrigin.Paragraph => BuildShapeProp("posrelv", "2"), 
			VerticalOrigin.Line => BuildShapeProp("posrelv", "3"), 
			_ => string.Empty, 
		};
	}

	private string BuildShapePosition(float horPos, float vertPos, int shapeW, int shapeH)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (horPos == 0f && vertPos == 0f)
		{
			stringBuilder.Append(c_slashSymbol + "shpright");
			stringBuilder.Append(shapeW);
			stringBuilder.Append(c_slashSymbol + "shpbottom");
			stringBuilder.Append(shapeH);
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "shpleft");
			stringBuilder.Append(Math.Round(horPos * 20f));
			stringBuilder.Append(c_slashSymbol + "shptop");
			stringBuilder.Append(Math.Round(vertPos * 20f));
			stringBuilder.Append(c_slashSymbol + "shpright");
			stringBuilder.Append(Math.Round(horPos * 20f + (float)shapeW));
			stringBuilder.Append(c_slashSymbol + "shpbottom");
			stringBuilder.Append(Math.Round(vertPos * 20f + (float)shapeH));
		}
		return stringBuilder.ToString();
	}

	private byte[] BuildTextBox(WTextBox textBox)
	{
		MemoryStream memoryStream = new MemoryStream();
		StringBuilder strBuilder = new StringBuilder();
		if (((textBox.IsShape && textBox.Shape != null && textBox.Shape.FallbackPic == null) || !textBox.IsShape) && textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			textBox.TextBoxFormat.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
		}
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildCharacterFormat(textBox.CharacterFormat));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "shp");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
		memoryStream.Write(bytes, 0, bytes.Length);
		int shapeW = (int)Math.Round(textBox.TextBoxFormat.Width * 20f);
		int shapeH = (int)Math.Round(textBox.TextBoxFormat.Height * 20f);
		bytes = m_encoding.GetBytes(BuildShapePosition(textBox.TextBoxFormat.HorizontalPosition, textBox.TextBoxFormat.VerticalPosition, shapeW, shapeH));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (textBox.TextBoxFormat.IsHeaderTextBox)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpfhdr1");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpfhdr0");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.Page)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbxpage");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.Margin || textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.LeftMargin || textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.RightMargin || textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.InsideMargin || textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbxmargin");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.Column)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbxcolumn");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.HorizontalOrigin == HorizontalOrigin.Character || textBox.TextBoxFormat.HorizontalOrigin.ToString().Contains("Margin"))
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbxignore");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.Page)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbypage");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (textBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.Margin)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbymargin");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else if (textBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shpbypara");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(c_slashSymbol + "shpbyignore");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "shpz" + textBox.TextBoxFormat.OrderIndex);
		memoryStream.Write(bytes, 0, bytes.Length);
		if (textBox.Shape != null && textBox.IsShape && textBox.Shape.LockAnchor)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "shplockanchor");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes(BuildWrappingStyle(textBox.TextBoxFormat.TextWrappingStyle, textBox.TextBoxFormat.TextWrappingType));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildShapeProp("shapeType", "202"));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (textBox.Shape != null && !textBox.Shape.FillFormat.Fill)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("fFilled", "0"));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		BuildTextBoxPositionTokens(bytes, textBox, memoryStream);
		BuildTextBoxLineTokens(bytes, textBox, memoryStream);
		bytes = m_encoding.GetBytes(BuildShapeFill(textBox.TextBoxFormat.FillEfects, isPageBackground: false));
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(BuildHiddenTextBox(textBox.Visible));
		memoryStream.Write(bytes, 0, bytes.Length);
		if (textBox.TextBoxFormat.InternalMargin.Left != 0f)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("dxTextLeft", Math.Round(textBox.TextBoxFormat.InternalMargin.Left * 12700f).ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.InternalMargin.Top != 0f)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("dyTextTop", Math.Round(textBox.TextBoxFormat.InternalMargin.Top * 12700f).ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.InternalMargin.Right != 0f)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("dxTextRight", Math.Round(textBox.TextBoxFormat.InternalMargin.Right * 12700f).ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.InternalMargin.Bottom != 0f)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("dyTextBottom", Math.Round(textBox.TextBoxFormat.InternalMargin.Bottom * 12700f).ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText)
		{
			bytes = m_encoding.GetBytes(BuildWrapText(textBox.TextBoxFormat.TextWrappingStyle));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.TextDirection != 0)
		{
			bytes = m_encoding.GetBytes(BuildTextFlow(textBox.TextBoxFormat.TextDirection));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.CharacterFormat.Scaling != 0f)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("scaleText", textBox.CharacterFormat.Scaling.ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.Shape != null && textBox.IsShape && textBox.Shape.TextFrame.TextVerticalAlignment != 0)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("anchorText", ((int)textBox.Shape.TextFrame.TextVerticalAlignment).ToString()));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.TextBoxFormat.AutoFit)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("fFitShapeToText", "1"));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		if (textBox.Shape != null && textBox.IsShape && textBox.Shape.TextFrame.NoWrap)
		{
			bytes = m_encoding.GetBytes(BuildShapeProp("WrapText", "2"));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		BuildTextBoxFillTokens(bytes, textBox, memoryStream);
		if (textBox.Shape != null && textBox.Shape.EffectList.Count >= 1 && textBox.Shape.EffectList[0].IsShadowEffect)
		{
			BuildShapeShadowTokens(textBox.Shape, strBuilder);
		}
		if (textBox.Shape != null && textBox.Shape.EffectList.Count == 2 && textBox.Shape.EffectList[1].IsShapeProperties)
		{
			BuildShape3DTokens(textBox.Shape, strBuilder);
		}
		bytes = m_encoding.GetBytes(BuildLayoutInCell(textBox.TextBoxFormat.AllowInCell));
		memoryStream.Write(bytes, 0, bytes.Length);
		int tableNestedLevel = m_tableNestedLevel;
		m_tableNestedLevel = 0;
		if (textBox.TextBoxBody.Items.Count > 0)
		{
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "shptxt");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = BuildBodyItems(textBox.TextBoxBody.Items);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("}");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		m_tableNestedLevel = tableNestedLevel;
		if (textBox.IsShape && textBox.Shape != null && textBox.Shape.FallbackPic != null)
		{
			bytes = m_encoding.GetBytes(BuildPicture(textBox.Shape.FallbackPic, isFieldShape: true));
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes("}}}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		return memoryStream.ToArray();
	}

	private void BuildTextBoxPositionTokens(byte[] byteArr, WTextBox textBox, MemoryStream textBoxStream)
	{
		byteArr = m_encoding.GetBytes(BuildShapeProp("posh", ((int)textBox.TextBoxFormat.HorizontalAlignment).ToString()));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		byteArr = m_encoding.GetBytes(BuildShapeProp("posv", ((int)textBox.TextBoxFormat.VerticalAlignment).ToString()));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		byteArr = m_encoding.GetBytes(BuildShapeProp("posrelh", ((int)textBox.TextBoxFormat.HorizontalOrigin).ToString()));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		byteArr = m_encoding.GetBytes(BuildShapeProp("posrelv", ((int)textBox.TextBoxFormat.VerticalOrigin).ToString()));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		if (textBox.Shape != null && textBox.IsShape)
		{
			if (textBox.Shape.IsRelativeHorizontalPosition)
			{
				byteArr = m_encoding.GetBytes(BuildShapeProp("pctHorizPos", ((int)Math.Round(textBox.Shape.RelativeHorizontalPosition * 10f)).ToString()));
				textBoxStream.Write(byteArr, 0, byteArr.Length);
			}
			if (textBox.Shape.IsRelativeVerticalPosition)
			{
				byteArr = m_encoding.GetBytes(BuildShapeProp("pctVertPos", ((int)Math.Round(textBox.Shape.RelativeVerticalPosition * 10f)).ToString()));
				textBoxStream.Write(byteArr, 0, byteArr.Length);
			}
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fBehindDocument", "1"));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Square || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Through || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Tight)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("dxWrapDistLeft", Math.Round(textBox.TextBoxFormat.WrapDistanceLeft * 12700f, 2).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Square || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Through || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Tight)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("dxWrapDistRight", Math.Round(textBox.TextBoxFormat.WrapDistanceRight * 12700f, 2).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Square || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.TopAndBottom)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("dyWrapDistTop", Math.Round(textBox.TextBoxFormat.WrapDistanceTop * 12700f, 2).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Square || textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.TopAndBottom)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("dyWrapDistBottom", Math.Round(textBox.TextBoxFormat.WrapDistanceBottom * 12700f, 2).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.HeightRelativePercent != 0f)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("pctHoriz", (textBox.TextBoxFormat.HeightRelativePercent * 10f).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.WidthRelativePercent != 0f)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("pctVert", (textBox.TextBoxFormat.WidthRelativePercent * 10f).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.HeightRelativePercent != 0f)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("sizerelh", ((int)textBox.TextBoxFormat.WidthOrigin).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.WidthRelativePercent != 0f)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("sizerelv", ((int)textBox.TextBoxFormat.HeightOrigin).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (!textBox.TextBoxFormat.AllowOverlap)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fAllowOverlap", "0"));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fPseudoInline", "1"));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
	}

	private void BuildTextBoxFillTokens(byte[] byteArr, WTextBox textBox, MemoryStream textBoxStream)
	{
		if (textBox.TextBoxFormat.FillColor != Color.Empty)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fillColor", GetRTFAutoShapeColor(textBox.TextBoxFormat.FillColor)));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		else
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fillOpacity", "0"));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.IsShape)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fillOpacity", Convert.ToUInt32(65536f * (1f - textBox.Shape.FillFormat.Transparency / 100f)).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
	}

	private void BuildTextBoxLineTokens(byte[] byteArr, WTextBox textBox, MemoryStream textBoxStream)
	{
		byteArr = m_encoding.GetBytes(BuildShapeLines(textBox.TextBoxFormat.LineColor, textBox.TextBoxFormat.LineWidth));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		byteArr = m_encoding.GetBytes(BuildTextBoxLineStyle(textBox.TextBoxFormat.LineStyle));
		textBoxStream.Write(byteArr, 0, byteArr.Length);
		if (textBox.Shape != null && textBox.IsShape && textBox.Shape.LineFormat.LineCap != DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineCap.Flat)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("lineEndCapStyle", ((int)textBox.Shape.LineFormat.LineCap).ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		LineDashing lineDashing = textBox.TextBoxFormat.LineDashing;
		if (lineDashing == LineDashing.DotGEL)
		{
			lineDashing = LineDashing.Dot;
		}
		if (lineDashing != 0)
		{
			Encoding encoding = m_encoding;
			int num = (int)lineDashing;
			byteArr = encoding.GetBytes(BuildShapeProp("lineDashing", num.ToString()));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
		if (textBox.TextBoxFormat.NoLine)
		{
			byteArr = m_encoding.GetBytes(BuildShapeProp("fLine", "0"));
			textBoxStream.Write(byteArr, 0, byteArr.Length);
		}
	}

	private string BuildTextFlow(TextDirection textDirection)
	{
		return textDirection switch
		{
			TextDirection.VerticalFarEast => BuildShapeProp("txflTextFlow", "1"), 
			TextDirection.VerticalBottomToTop => BuildShapeProp("txflTextFlow", "2"), 
			TextDirection.VerticalTopToBottom => BuildShapeProp("txflTextFlow", "3"), 
			TextDirection.HorizontalFarEast => BuildShapeProp("txflTextFlow", "4"), 
			TextDirection.Vertical => BuildShapeProp("txflTextFlow", "5"), 
			_ => BuildShapeProp("txflTextFlow", "0"), 
		};
	}

	private string BuildWrapText(TextWrappingStyle textWrappingStyle)
	{
		return textWrappingStyle switch
		{
			TextWrappingStyle.Square => BuildShapeProp("WrapText", "0"), 
			TextWrappingStyle.Tight => BuildShapeProp("WrapText", "1"), 
			TextWrappingStyle.TopAndBottom => BuildShapeProp("WrapText", "3"), 
			TextWrappingStyle.Through => BuildShapeProp("WrapText", "4"), 
			_ => string.Empty, 
		};
	}

	private string BuildTextBoxLineStyle(TextBoxLineStyle style)
	{
		return style switch
		{
			TextBoxLineStyle.Double => BuildShapeProp("lineStyle", "1"), 
			TextBoxLineStyle.ThickThin => BuildShapeProp("lineStyle", "2"), 
			TextBoxLineStyle.ThinThick => BuildShapeProp("lineStyle", "3"), 
			TextBoxLineStyle.Triple => BuildShapeProp("lineStyle", "4"), 
			_ => BuildShapeProp("lineStyle", "0"), 
		};
	}

	private string BuildShapeLines(Color col, float lineWidth)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!col.IsEmpty && col.Name != Color.Black.Name)
		{
			stringBuilder.Append(BuildShapeProp("lineColor", GetRTFAutoShapeColor(col)));
		}
		stringBuilder.Append(BuildShapeProp("lineWidth", (lineWidth * 12700f).ToString()));
		return stringBuilder.ToString();
	}

	private string BuildShapeFill(Background backgr, bool isPageBackground)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (backgr.Type)
		{
		case BackgroundType.Color:
			if (backgr.Color.IsEmpty || backgr.Color.Name == Color.White.Name)
			{
				return string.Empty;
			}
			if (isPageBackground)
			{
				stringBuilder.Append(BuildShapeProp("fillColor", GetRtfPageBackgroundColor(backgr.Color)));
			}
			else
			{
				stringBuilder.Append(BuildShapeProp("fillColor", GetRtfShapeColor(backgr.Color)));
			}
			break;
		case BackgroundType.Picture:
		case BackgroundType.Texture:
		{
			if (backgr.Type == BackgroundType.Picture)
			{
				stringBuilder.Append(BuildShapeProp("fillType", "3"));
			}
			else
			{
				stringBuilder.Append(BuildShapeProp("fillType", "2"));
			}
			WPicture wPicture = new WPicture(m_doc);
			wPicture.LoadImage(backgr.Picture);
			stringBuilder.Append(BuildShapeProp("fillBlip", BuildPicture(wPicture, isFieldShape: false)));
			break;
		}
		case BackgroundType.Gradient:
			switch (backgr.Gradient.ShadingStyle)
			{
			case GradientShadingStyle.FromCorner:
				stringBuilder.Append(BuildShapeProp("fillType", "5"));
				stringBuilder.Append(BuildShapeProp("fillFocus", "100"));
				if (backgr.Gradient.ShadingVariant == GradientShadingVariant.ShadingDown)
				{
					stringBuilder.Append(BuildShapeProp("fillToLeft", "65536"));
					stringBuilder.Append(BuildShapeProp("fillToRight", "65536"));
				}
				else if (backgr.Gradient.ShadingVariant == GradientShadingVariant.ShadingMiddle)
				{
					stringBuilder.Append(BuildShapeProp("fillToLeft", "65536"));
					stringBuilder.Append(BuildShapeProp("fillToTop", "65536"));
					stringBuilder.Append(BuildShapeProp("fillToRight", "65536"));
					stringBuilder.Append(BuildShapeProp("fillToBottom", "65536"));
				}
				else if (backgr.Gradient.ShadingVariant == GradientShadingVariant.ShadingOut)
				{
					stringBuilder.Append(BuildShapeProp("fillToTop", "65536"));
					stringBuilder.Append(BuildShapeProp("fillToBottom", "65536"));
				}
				break;
			case GradientShadingStyle.FromCenter:
				stringBuilder.Append(BuildShapeProp("fillType", "6"));
				if (backgr.Gradient.ShadingVariant == GradientShadingVariant.ShadingUp)
				{
					stringBuilder.Append(BuildShapeProp("fillFocus", "100"));
				}
				stringBuilder.Append(BuildShapeProp("fillToLeft", "32768"));
				stringBuilder.Append(BuildShapeProp("fillToRight", "32768"));
				stringBuilder.Append(BuildShapeProp("fillToTop", "32768"));
				stringBuilder.Append(BuildShapeProp("fillToBottom", "32768"));
				break;
			case GradientShadingStyle.Horizontal:
				stringBuilder.Append(BuildShapeProp("fillType", "7"));
				stringBuilder.Append(BuildGradientVariant(backgr.Gradient.ShadingVariant));
				break;
			case GradientShadingStyle.Vertical:
				stringBuilder.Append(BuildShapeProp("fillType", "7"));
				stringBuilder.Append(BuildShapeProp("fillAngle", "-5898240"));
				stringBuilder.Append(BuildGradientVariant(backgr.Gradient.ShadingVariant));
				break;
			case GradientShadingStyle.DiagonalDown:
				stringBuilder.Append(BuildShapeProp("fillType", "7"));
				stringBuilder.Append(BuildShapeProp("fillAngle", "-2949120"));
				stringBuilder.Append(BuildGradientVariant(backgr.Gradient.ShadingVariant));
				break;
			case GradientShadingStyle.DiagonalUp:
				stringBuilder.Append(BuildShapeProp("fillType", "7"));
				stringBuilder.Append(BuildShapeProp("fillAngle", "-8847360"));
				stringBuilder.Append(BuildGradientVariant(backgr.Gradient.ShadingVariant));
				break;
			}
			stringBuilder.Append(BuildShapeProp("fillColor", GetRtfShapeColor(backgr.Gradient.Color1)));
			stringBuilder.Append(BuildShapeProp("fillBackColor", GetRtfShapeColor(backgr.Gradient.Color2)));
			break;
		default:
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	private string BuildGradientVariant(GradientShadingVariant variant)
	{
		return variant switch
		{
			GradientShadingVariant.ShadingUp => BuildShapeProp("fillFocus", "100"), 
			GradientShadingVariant.ShadingMiddle => BuildShapeProp("fillFocus", "50"), 
			GradientShadingVariant.ShadingOut => BuildShapeProp("fillFocus", "-50"), 
			_ => string.Empty, 
		};
	}

	private void AppendListStyles()
	{
		ListStyleCollection listStyles = m_doc.ListStyles;
		MemoryStream memoryStream = new MemoryStream();
		if (listStyles.Count == 0)
		{
			return;
		}
		byte[] bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "listtable");
		memoryStream.Write(bytes, 0, bytes.Length);
		int i = 0;
		for (int count = listStyles.Count; i < count; i++)
		{
			ListStyle listStyle = listStyles[i];
			bytes = m_encoding.GetBytes(Environment.NewLine);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "list");
			memoryStream.Write(bytes, 0, bytes.Length);
			if (listStyle.IsSimple)
			{
				bytes = m_encoding.GetBytes(c_slashSymbol + "listsimple1");
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			else
			{
				bytes = m_encoding.GetBytes(c_slashSymbol + "listsimple0");
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			if (listStyle.IsHybrid)
			{
				bytes = m_encoding.GetBytes(c_slashSymbol + "listhybrid");
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			int j = 0;
			for (int count2 = listStyle.Levels.Count; j < count2; j++)
			{
				WListLevel listLevel = listStyle.Levels[j];
				bytes = m_encoding.GetBytes(BuildListLevel(listLevel));
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "listname ");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(listStyle.Name);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(" ;}");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "listid" + i);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(Environment.NewLine);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("}");
			memoryStream.Write(bytes, 0, bytes.Length);
			ListsIds.Add(listStyle.Name, i);
		}
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		m_listTableBytes = memoryStream.ToArray();
	}

	private string BuildListLevel(WListLevel listLevel)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "listlevel");
		stringBuilder.Append(BuildLevelFormatting(listLevel.PatternType));
		if (listLevel.NumberAlignment == ListNumberAlignment.Left)
		{
			stringBuilder.Append(c_slashSymbol + "leveljc0" + c_slashSymbol + "leveljcn0");
		}
		else if (listLevel.NumberAlignment == ListNumberAlignment.Center)
		{
			stringBuilder.Append(c_slashSymbol + "leveljc1" + c_slashSymbol + "leveljcn1");
		}
		else if (listLevel.NumberAlignment == ListNumberAlignment.Right)
		{
			stringBuilder.Append(c_slashSymbol + "leveljc2" + c_slashSymbol + "leveljcn2");
		}
		if (listLevel.FollowCharacter == FollowCharacterType.Tab)
		{
			stringBuilder.Append(c_slashSymbol + "levelfollow0");
		}
		else if (listLevel.FollowCharacter == FollowCharacterType.Space)
		{
			stringBuilder.Append(c_slashSymbol + "levelfollow1");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "levelfollow2");
		}
		stringBuilder.Append(c_slashSymbol + "levelstartat");
		stringBuilder.Append(listLevel.StartAt.ToString());
		if (listLevel.Word6Legacy)
		{
			stringBuilder.Append(c_slashSymbol + "levelold");
		}
		stringBuilder.Append(c_slashSymbol + "levelspace");
		stringBuilder.Append(listLevel.LegacySpace);
		stringBuilder.Append(c_slashSymbol + "levelindent");
		stringBuilder.Append(listLevel.LegacyIndent);
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(BuildLevelText(listLevel));
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(BuildLevelNumbers(listLevel));
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(BuildParagraphFormat(listLevel.ParagraphFormat, null));
		stringBuilder.Append(BuildCharacterFormat(listLevel.CharacterFormat));
		stringBuilder.Append("}");
		stringBuilder.Append(Environment.NewLine);
		return stringBuilder.ToString();
	}

	private string UpdateNumberPrefix(string prefix)
	{
		return prefix.Replace("\0", c_slashSymbol + "'00").Replace("\u0001", c_slashSymbol + "'01").Replace("\u0002", c_slashSymbol + "'02")
			.Replace("\u0003", c_slashSymbol + "'03")
			.Replace("\u0004", c_slashSymbol + "'04")
			.Replace("\u0005", c_slashSymbol + "'05")
			.Replace("\u0006", c_slashSymbol + "'06")
			.Replace("\a", c_slashSymbol + "'07")
			.Replace("\b", c_slashSymbol + "'08");
	}

	private string BuildLevelText(WListLevel listLevel)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "leveltext");
		if (listLevel.PatternType == ListPatternType.Bullet)
		{
			stringBuilder.Append(c_slashSymbol + "'01");
			string listLText = listLevel.BulletCharacter;
			if (IsChanged(ref listLText))
			{
				stringBuilder.Append(listLText);
			}
			else if (!string.IsNullOrEmpty(listLevel.BulletCharacter) && listLevel.BulletCharacter.Length == 1 && listLevel.BulletCharacter[0] != ';' && listLevel.BulletCharacter[0] > '\u001f' && listLevel.BulletCharacter[0] < 'A')
			{
				stringBuilder.Append(listLevel.BulletCharacter);
			}
			else
			{
				byte b = (byte)((!string.IsNullOrEmpty(listLevel.BulletCharacter)) ? ((byte)listLevel.BulletCharacter[0]) : 0);
				if (!string.IsNullOrEmpty(listLevel.BulletCharacter) && (listLevel.CharacterFormat.FontName == "Symbol" || ((b <= 64 || b >= 91) && (b <= 96 || b >= 123))))
				{
					if (listLevel.BulletCharacter[0] > '\u007f')
					{
						string value = "\\u" + Convert.ToUInt32(listLevel.BulletCharacter[0]) + "?";
						stringBuilder.Append(value);
					}
					else
					{
						stringBuilder.Append(c_slashSymbol + "u-" + (4096 - b) + " ?");
					}
				}
				else if (!string.IsNullOrEmpty(listLevel.BulletCharacter))
				{
					stringBuilder.Append(listLevel.BulletCharacter[0].ToString());
				}
			}
		}
		else
		{
			string levelText = GetLevelText(listLevel, isLevelNumbers: false);
			int levelTextLeng = GetLevelTextLeng(levelText);
			stringBuilder.Append(c_slashSymbol + "'" + levelTextLeng.ToString("X2") + levelText);
		}
		stringBuilder.Append(";}");
		return stringBuilder.ToString();
	}

	private string BuildLevelNumbers(WListLevel listLevel)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string levelText = GetLevelText(listLevel, isLevelNumbers: true);
		stringBuilder.Append("{" + c_slashSymbol + "levelnumbers");
		if (listLevel.PatternType != ListPatternType.Bullet && levelText != string.Empty)
		{
			levelText = levelText.Trim();
			int levelTextLeng = GetLevelTextLeng(levelText);
			if (!string.IsNullOrEmpty(listLevel.NumberPrefix) && !IsComplexList(listLevel.NumberPrefix))
			{
				for (int i = listLevel.NumberPrefix.Length + 1; i <= levelTextLeng; i += 2)
				{
					stringBuilder.Append(c_slashSymbol + "'" + i.ToString("X2"));
				}
			}
			else
			{
				for (int j = 1; j <= levelTextLeng; j += 2)
				{
					stringBuilder.Append(c_slashSymbol + "'" + j.ToString("X2"));
				}
			}
		}
		stringBuilder.Append(";}");
		return stringBuilder.ToString();
	}

	private int GetLevelTextLeng(string levelText)
	{
		levelText = levelText.Replace(c_slashSymbol + "'0", string.Empty);
		return levelText.Length;
	}

	private string GetLevelText(WListLevel listLevel, bool isLevelNumbers)
	{
		string text = (string.IsNullOrEmpty(listLevel.NumberPrefix) ? listLevel.NumberPrefix : UpdateNumberPrefix(listLevel.NumberPrefix));
		return text + c_slashSymbol + "'0" + listLevel.LevelNumber + (isLevelNumbers ? string.Empty : listLevel.NumberSuffix);
	}

	private string BuildLevelFormatting(ListPatternType type)
	{
		return type switch
		{
			ListPatternType.Arabic => c_slashSymbol + "levelnfc0" + c_slashSymbol + "levelnfcn0", 
			ListPatternType.UpRoman => c_slashSymbol + "levelnfc1" + c_slashSymbol + "levelnfcn1", 
			ListPatternType.LowRoman => c_slashSymbol + "levelnfc2" + c_slashSymbol + "levelnfcn2", 
			ListPatternType.UpLetter => c_slashSymbol + "levelnfc3" + c_slashSymbol + "levelnfcn3", 
			ListPatternType.LowLetter => c_slashSymbol + "levelnfc4" + c_slashSymbol + "levelnfcn4", 
			ListPatternType.Ordinal => c_slashSymbol + "levelnfc5" + c_slashSymbol + "levelnfcn5", 
			ListPatternType.Number => c_slashSymbol + "levelnfc6" + c_slashSymbol + "levelnfcn6", 
			ListPatternType.OrdinalText => c_slashSymbol + "levelnfc7" + c_slashSymbol + "levelnfcn7", 
			ListPatternType.LeadingZero => c_slashSymbol + "levelnfc22" + c_slashSymbol + "levelnfcn22", 
			ListPatternType.Bullet => c_slashSymbol + "levelnfc23" + c_slashSymbol + "levelnfcn23", 
			_ => c_slashSymbol + "levelnfc255" + c_slashSymbol + "levelnfcn255", 
		};
	}

	private void AppendOverrideList()
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "listoverridetable");
		memoryStream.Write(bytes, 0, bytes.Length);
		foreach (int key in ListOverrideAr.Keys)
		{
			bytes = m_encoding.GetBytes(Environment.NewLine);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("{" + c_slashSymbol + "listoverride");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(c_slashSymbol + "listid" + (key - 1));
			memoryStream.Write(bytes, 0, bytes.Length);
			string text = ListOverrideAr[key];
			if (!string.IsNullOrEmpty(text))
			{
				ListOverrideStyle listOverrideStyle = m_doc.ListOverrides.FindByName(text);
				bytes = m_encoding.GetBytes(c_slashSymbol + "listoverridecount" + listOverrideStyle.OverrideLevels.Count);
				memoryStream.Write(bytes, 0, bytes.Length);
				bytes = m_encoding.GetBytes("{" + c_slashSymbol + "lfolevel" + c_slashSymbol + "listoverrideformat");
				memoryStream.Write(bytes, 0, bytes.Length);
				bytes = m_encoding.GetBytes(Environment.NewLine);
				memoryStream.Write(bytes, 0, bytes.Length);
				foreach (OverrideLevelFormat overrideLevel in listOverrideStyle.OverrideLevels)
				{
					bytes = m_encoding.GetBytes(BuildListLevel(overrideLevel.OverrideListLevel));
					memoryStream.Write(bytes, 0, bytes.Length);
				}
				bytes = m_encoding.GetBytes("}");
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			else
			{
				bytes = m_encoding.GetBytes(c_slashSymbol + "listoverridecount0");
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			bytes = m_encoding.GetBytes(c_slashSymbol + "ls" + key);
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("}");
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		bytes = m_encoding.GetBytes("}");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		m_listOverrideTableBytes = memoryStream.ToArray();
	}

	private string BuildListText(WParagraph paragraph, WListFormat listFormat)
	{
		if (listFormat == null || listFormat.ListType == ListType.NoList || listFormat.CurrentListStyle == null)
		{
			return string.Empty;
		}
		WListLevel listLevel = paragraph.GetListLevel(listFormat);
		StringBuilder stringBuilder = new StringBuilder();
		if (listFormat.CurrentListStyle.ListType == ListType.Numbered || listFormat.CurrentListStyle.ListType == ListType.Bulleted)
		{
			stringBuilder.Append("{" + c_slashSymbol + "listtext");
			stringBuilder.Append(BuildParagraphFormat(listLevel.ParagraphFormat, null));
			stringBuilder.Append(BuildCharacterFormat(listLevel.CharacterFormat));
			stringBuilder.Append(BuildListText(listLevel, listFormat, paragraph));
			stringBuilder.Append(c_slashSymbol + "tab}");
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder.ToString();
	}

	private string BuildListText(WListLevel listLevel, WListFormat listFormat, WParagraph paragraph)
	{
		if (listLevel == null && listFormat == null)
		{
			return string.Empty;
		}
		if (listLevel.PatternType == ListPatternType.LowLetter || listLevel.PatternType == ListPatternType.UpLetter)
		{
			return BuildLstLetterSymbol(listFormat);
		}
		bool isPicBullet = false;
		if (listLevel.PatternType == ListPatternType.Arabic)
		{
			return c_slashSymbol + paragraph.GetListText(isFromTextConverter: false, ref isPicBullet);
		}
		return " " + paragraph.GetListText(isFromTextConverter: false, ref isPicBullet);
	}

	private int GetLstStartVal(WListFormat format)
	{
		if (!ListStart.ContainsKey(format.CustomStyleName))
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			ListStart.Add(format.CustomStyleName, dictionary);
			WListLevel wListLevel = format.CurrentListStyle.Levels[format.ListLevelNumber];
			dictionary.Add(format.ListLevelNumber, wListLevel.StartAt + 1);
			return wListLevel.StartAt;
		}
		Dictionary<int, int> dictionary2 = ListStart[format.CustomStyleName];
		if (dictionary2.ContainsKey(format.ListLevelNumber))
		{
			int num = dictionary2[format.ListLevelNumber];
			dictionary2[format.ListLevelNumber] = num + 1;
			return num;
		}
		WListLevel wListLevel2 = format.CurrentListStyle.Levels[format.ListLevelNumber];
		dictionary2.Add(format.ListLevelNumber, wListLevel2.StartAt + 1);
		return wListLevel2.StartAt + 1;
	}

	private string BuildLstLetterSymbol(WListFormat format)
	{
		int num = ((format.CurrentListLevel.PatternType == ListPatternType.LowLetter) ? 96 : 64);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(format.CurrentListLevel.NumberPrefix);
		int num2 = GetLstStartVal(format);
		int num3 = 1;
		while (num2 > 26)
		{
			num2 -= 26;
			num3++;
		}
		string value = ((char)(num2 + num)).ToString();
		for (int i = 0; i < num3; i++)
		{
			stringBuilder.Append(value);
		}
		stringBuilder.Append(format.CurrentListLevel.NumberSuffix);
		return stringBuilder.ToString();
	}

	private bool IsChanged(ref string listLText)
	{
		string obj = listLText;
		listLText = listLText.Replace(c_symbol8226, c_slashSymbol + "'95");
		if (obj == listLText)
		{
			return false;
		}
		return true;
	}

	private bool IsComplexList(string prefix)
	{
		if (string.IsNullOrEmpty(prefix))
		{
			return true;
		}
		if (prefix.Contains("\0"))
		{
			return true;
		}
		if (prefix.Contains("\u0001"))
		{
			return true;
		}
		if (prefix.Contains("\u0002"))
		{
			return true;
		}
		if (prefix.Contains("\u0003"))
		{
			return true;
		}
		if (prefix.Contains("\u0004"))
		{
			return true;
		}
		if (prefix.Contains("\u0005"))
		{
			return true;
		}
		if (prefix.Contains("\u0006"))
		{
			return true;
		}
		if (prefix.Contains("\a"))
		{
			return true;
		}
		if (prefix.Contains("\b"))
		{
			return true;
		}
		return false;
	}

	private string BuildPictWtrmarkBody(PictureWatermark picWatermark)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "shp");
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
		stringBuilder.Append(c_slashSymbol + "shpleft0");
		stringBuilder.Append(c_slashSymbol + "shptop0");
		stringBuilder.Append(c_slashSymbol + "shpwr3");
		stringBuilder.Append(c_slashSymbol + "shpright");
		stringBuilder.Append(Math.Round(picWatermark.WordPicture.Width * 20f));
		stringBuilder.Append(c_slashSymbol + "shpbottom");
		stringBuilder.Append(Math.Round(picWatermark.WordPicture.Height * 20f));
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(BuildShapeProp("shapeType", "75"));
		stringBuilder.Append(BuildShapeProp("pib", BuildPictureProp(picWatermark.WordPicture, isFieldshape: false)));
		stringBuilder.Append(BuildDefWtrmarkProp());
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildTextWtrmarkBody(TextWatermark textWatermark)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "shp");
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "shpinst");
		stringBuilder.Append(c_slashSymbol + "shpleft0");
		stringBuilder.Append(c_slashSymbol + "shptop0");
		stringBuilder.Append(c_slashSymbol + "shpright");
		stringBuilder.Append(textWatermark.Width * 20f);
		stringBuilder.Append(c_slashSymbol + "shpbottom");
		stringBuilder.Append(textWatermark.Height * 20f);
		stringBuilder.Append(c_slashSymbol + "shpwr3");
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append(BuildShapeProp("shapeType", "136"));
		if (textWatermark.Layout == WatermarkLayout.Diagonal)
		{
			stringBuilder.Append(BuildShapeProp("rotation", "20643840"));
		}
		string propValue = textWatermark.Text.Replace(c_slashSymbol + "0", string.Empty);
		stringBuilder.Append(BuildShapeProp("gtextUNICODE", propValue));
		stringBuilder.Append(BuildShapeProp("gtextSize", ((int)Math.Round(textWatermark.Size * 65536f)).ToString()));
		string propValue2 = textWatermark.FontName.Replace(c_slashSymbol + "0", string.Empty);
		stringBuilder.Append(BuildShapeProp("gtextFont", propValue2));
		stringBuilder.Append(BuildShapeProp("fillColor", GetRtfShapeColor(textWatermark.Color)));
		if (textWatermark.Semitransparent)
		{
			stringBuilder.Append(BuildShapeProp("fillOpacity", "32768"));
		}
		stringBuilder.Append(BuildDefWtrmarkProp());
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildDefWtrmarkProp()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(BuildShapeProp("fLine", "0"));
		stringBuilder.Append(BuildShapeProp("posh", "2"));
		stringBuilder.Append(BuildShapeProp("posrelh", "0"));
		stringBuilder.Append(BuildShapeProp("posv", "2"));
		stringBuilder.Append(BuildShapeProp("posrelv", "0"));
		return stringBuilder.ToString();
	}

	private string BuildTextFormField(WTextFormField textField)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "formfield");
		stringBuilder.Append("{" + c_slashSymbol + "fftype0");
		switch (textField.Type)
		{
		case TextFormFieldType.RegularText:
			stringBuilder.Append(c_slashSymbol + "fftypetxt0");
			break;
		case TextFormFieldType.NumberText:
			stringBuilder.Append(c_slashSymbol + "fftypetxt1");
			break;
		case TextFormFieldType.DateText:
			stringBuilder.Append(c_slashSymbol + "fftypetxt2");
			break;
		case TextFormFieldType.CurrentDateText:
			stringBuilder.Append(c_slashSymbol + "fftypetxt3");
			break;
		case TextFormFieldType.CurrentTimeText:
			stringBuilder.Append(c_slashSymbol + "fftypetxt4");
			break;
		case TextFormFieldType.Calculation:
			stringBuilder.Append(c_slashSymbol + "fftypetxt5");
			break;
		}
		if (textField.CalculateOnExit)
		{
			stringBuilder.Append(c_slashSymbol + "ffrecalc");
		}
		if (textField.MaximumLength != 0)
		{
			stringBuilder.Append(c_slashSymbol + "ffmaxlen" + textField.MaximumLength);
		}
		stringBuilder.Append(c_slashSymbol + "ffhps20");
		if (!string.IsNullOrEmpty(textField.Name))
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffname ");
			stringBuilder.Append(textField.Name + "}");
		}
		if (!string.IsNullOrEmpty(textField.DefaultText))
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffdeftext ");
			stringBuilder.Append(textField.DefaultText + "}");
		}
		if (textField.TextFormat != 0)
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffformat ");
			switch (textField.TextFormat)
			{
			case TextFormat.Uppercase:
				stringBuilder.Append("Uppercase");
				break;
			case TextFormat.Lowercase:
				stringBuilder.Append("Lowercase");
				break;
			case TextFormat.Titlecase:
				stringBuilder.Append("Titlecase");
				break;
			case TextFormat.FirstCapital:
				stringBuilder.Append("FirstCapital");
				break;
			}
			stringBuilder.Append("}");
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildCheckBox(WCheckBox checkBox)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "formfield");
		stringBuilder.Append("{" + c_slashSymbol + "fftype1");
		stringBuilder.Append(c_slashSymbol + "ffres25");
		if (checkBox.SizeType == CheckBoxSizeType.Auto)
		{
			stringBuilder.Append(c_slashSymbol + "ffsize0");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "ffsize1");
		}
		stringBuilder.Append(c_slashSymbol + "fftypetxt0");
		if (checkBox.CalculateOnExit)
		{
			stringBuilder.Append(c_slashSymbol + "ffrecalc");
		}
		stringBuilder.Append(c_slashSymbol + "ffhps" + checkBox.CheckBoxSize * 2);
		if (!string.IsNullOrEmpty(checkBox.Name))
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffname ");
			stringBuilder.Append(checkBox.Name + "}");
		}
		if (checkBox.Checked)
		{
			stringBuilder.Append(c_slashSymbol + "ffdefres1");
		}
		else
		{
			stringBuilder.Append(c_slashSymbol + "ffdefres0");
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildDropDownField(WDropDownFormField dropDownField)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "formfield");
		stringBuilder.Append("{" + c_slashSymbol + "fftype2");
		if (dropDownField.DropDownSelectedIndex >= 0)
		{
			stringBuilder.Append(c_slashSymbol + "ffres" + dropDownField.DropDownSelectedIndex);
		}
		stringBuilder.Append(c_slashSymbol + "fftypetxt0");
		if (dropDownField.CalculateOnExit)
		{
			stringBuilder.Append(c_slashSymbol + "ffrecalc");
		}
		stringBuilder.Append(c_slashSymbol + "ffhaslistbox");
		stringBuilder.Append(c_slashSymbol + "ffhps20");
		if (!string.IsNullOrEmpty(dropDownField.Name))
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffname ");
			stringBuilder.Append(dropDownField.Name + "}");
		}
		stringBuilder.Append(c_slashSymbol + "ffdefres0");
		int i = 0;
		for (int count = dropDownField.DropDownItems.Count; i < count; i++)
		{
			stringBuilder.Append("{" + c_slashSymbol + "ffl ");
			stringBuilder.Append(dropDownField.DropDownItems[i].Text);
			stringBuilder.Append("}");
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private string BuildFormField(WFormField formField)
	{
		CurrentField.Push(formField);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{" + c_slashSymbol + "field");
		stringBuilder.Append("{" + c_slashSymbol + "*" + c_slashSymbol + "fldinst");
		return stringBuilder.ToString();
	}

	private byte[] BuildCommentMark(WCommentMark cMark)
	{
		MemoryStream memoryStream = new MemoryStream();
		if (cMark.CommentId != "-1")
		{
			byte[] bytes;
			if (cMark.Type == CommentMarkType.CommentStart)
			{
				bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "atrfstart ");
				memoryStream.Write(bytes, 0, bytes.Length);
				string text = GetNextId().ToString();
				bytes = m_encoding.GetBytes(text.ToString());
				memoryStream.Write(bytes, 0, bytes.Length);
				CommentIds.Add(cMark.CommentId, text);
			}
			else
			{
				bytes = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "atrfend ");
				memoryStream.Write(bytes, 0, bytes.Length);
				string text2 = CommentIds[cMark.CommentId];
				bytes = m_encoding.GetBytes(text2.ToString());
				memoryStream.Write(bytes, 0, bytes.Length);
			}
			bytes = m_encoding.GetBytes("}");
			memoryStream.Write(bytes, 0, bytes.Length);
			return memoryStream.ToArray();
		}
		return memoryStream.ToArray();
	}

	private byte[] BuildComment(WComment comment)
	{
		new StringBuilder();
		MemoryStream memoryStream = new MemoryStream();
		string text = null;
		if (m_commentIds != null && CommentIds.ContainsKey(comment.Format.TagBkmk))
		{
			text = CommentIds[comment.Format.TagBkmk].ToString();
		}
		if (text == null && comment.CommentedItems.Count != 0)
		{
			text = GetNextId().ToString();
		}
		byte[] array;
		if (comment.AppendItems)
		{
			array = BuildComItems(comment, text);
			memoryStream.Write(array, 0, array.Length);
		}
		array = m_encoding.GetBytes("{");
		memoryStream.Write(array, 0, array.Length);
		if (!string.IsNullOrEmpty(comment.Format.UserInitials))
		{
			array = m_encoding.GetBytes(BuildCharacterFormat(comment.ParaItemCharFormat));
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "atnid ");
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes(comment.Format.UserInitials);
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes("}");
			memoryStream.Write(array, 0, array.Length);
		}
		if (!string.IsNullOrEmpty(comment.Format.User))
		{
			array = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "atnauthor ");
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes(comment.Format.User);
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes("}");
			memoryStream.Write(array, 0, array.Length);
		}
		array = m_encoding.GetBytes(c_slashSymbol + "chatn ");
		memoryStream.Write(array, 0, array.Length);
		array = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "annotation");
		memoryStream.Write(array, 0, array.Length);
		if (text != null && comment.Format.TagBkmk != "-1" && !HasEmptyCommentedItems(comment))
		{
			array = m_encoding.GetBytes("{" + c_slashSymbol + "*" + c_slashSymbol + "atnref ");
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes(text);
			memoryStream.Write(array, 0, array.Length);
			array = m_encoding.GetBytes("}");
			memoryStream.Write(array, 0, array.Length);
		}
		array = BuildBodyItems(comment.TextBody.Items);
		memoryStream.Write(array, 0, array.Length);
		array = m_encoding.GetBytes("}}");
		memoryStream.Write(array, 0, array.Length);
		return memoryStream.ToArray();
	}

	private byte[] BuildComItems(WComment comment, string id)
	{
		MemoryStream memoryStream = new MemoryStream();
		WCommentMark cMark = new WCommentMark(comment.Document, id, CommentMarkType.CommentStart);
		WCommentMark cMark2 = new WCommentMark(comment.Document, id, CommentMarkType.CommentEnd);
		byte[] array = BuildCommentMark(cMark);
		memoryStream.Write(array, 0, array.Length);
		foreach (ParagraphItem commentedItem in comment.CommentedItems)
		{
			array = BuildParagraphItem(commentedItem);
			memoryStream.Write(array, 0, array.Length);
		}
		array = BuildCommentMark(cMark2);
		memoryStream.Write(array, 0, array.Length);
		return memoryStream.ToArray();
	}

	private bool HasEmptyCommentedItems(WComment comment)
	{
		if (comment.CommentRangeStart.OwnerParagraph == comment.CommentRangeEnd.OwnerParagraph && comment.CommentedItems.Count > 0)
		{
			foreach (ParagraphItem commentedItem in comment.CommentedItems)
			{
				if (!(commentedItem is WTextRange) || !((commentedItem as WTextRange).Text == string.Empty))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private string BuildColorValue(WCharacterFormat cFormat, Color cFormatColor, WCharacterFormat baseCFormat, Color baseCFormatColor, int optionKey, string value)
	{
		if (optionKey == 63 && !cFormatColor.IsEmpty)
		{
			cFormatColor = WordColor.ColorsArray[(byte)WordColor.ConvertColorToId(cFormatColor)];
		}
		if (cFormat.HasValue(optionKey) && !cFormatColor.IsEmpty)
		{
			if (cFormatColor.IsNamedColor && cFormatColor.IsKnownColor)
			{
				if (63 == optionKey)
				{
					return BuildHighlightNamedColor(cFormatColor, value);
				}
				return BuildNamedColor(cFormatColor, value);
			}
			return BuildColor(cFormatColor, value);
		}
		if (baseCFormat != null && baseCFormat.HasValue(optionKey) && !baseCFormatColor.IsEmpty)
		{
			if (baseCFormatColor.IsNamedColor && baseCFormatColor.IsKnownColor)
			{
				if (63 == optionKey)
				{
					return BuildHighlightNamedColor(baseCFormatColor, value);
				}
				return BuildNamedColor(baseCFormatColor, value);
			}
			return BuildColor(baseCFormatColor, value);
		}
		return string.Empty;
	}

	private string BuildHighlightNamedColor(Color color, string value)
	{
		Color color2 = default(Color);
		switch (color.Name)
		{
		case "Maroon":
			color2 = Color.FromArgb(255, 0, 0);
			break;
		case "Green":
			color2 = Color.FromArgb(0, 255, 0);
			break;
		case "Olive":
			color2 = Color.FromArgb(128, 128, 0);
			break;
		case "Navy":
			color2 = Color.FromArgb(0, 0, 128);
			break;
		case "Purple":
			color2 = Color.FromArgb(255, 0, 255);
			break;
		case "Teal":
			color2 = Color.FromArgb(0, 255, 255);
			break;
		case "Red":
			color2 = Color.FromArgb(255, 0, 0);
			break;
		case "Lime":
			color2 = Color.FromArgb(0, 128, 0);
			break;
		case "Yellow":
			color2 = Color.FromArgb(255, 255, 0);
			break;
		case "Blue":
			color2 = Color.FromArgb(0, 0, 255);
			break;
		case "Fuchsia":
			color2 = Color.FromArgb(128, 0, 128);
			break;
		case "Aqua":
			color2 = Color.FromArgb(0, 128, 128);
			break;
		case "Gold":
			color2 = Color.FromArgb(128, 100, 0);
			break;
		}
		if (!color2.IsEmpty)
		{
			return BuildColor(color2, value);
		}
		return BuildColor(color, value);
	}

	private string BuildNamedColor(Color color, string value)
	{
		Color color2 = default(Color);
		switch (color.Name)
		{
		case "Maroon":
			color2 = Color.FromArgb(128, 0, 0);
			break;
		case "Green":
			color2 = Color.FromArgb(0, 128, 0);
			break;
		case "Olive":
			color2 = Color.FromArgb(128, 128, 0);
			break;
		case "Navy":
			color2 = Color.FromArgb(0, 0, 128);
			break;
		case "Purple":
			color2 = Color.FromArgb(128, 0, 128);
			break;
		case "Teal":
			color2 = Color.FromArgb(0, 128, 128);
			break;
		case "Red":
			color2 = Color.FromArgb(255, 0, 0);
			break;
		case "Lime":
			color2 = Color.FromArgb(0, 255, 0);
			break;
		case "Yellow":
			color2 = Color.FromArgb(255, 255, 0);
			break;
		case "Blue":
			color2 = Color.FromArgb(0, 0, 255);
			break;
		case "Fuchsia":
			color2 = Color.FromArgb(255, 0, 255);
			break;
		case "Aqua":
			color2 = Color.FromArgb(0, 255, 255);
			break;
		case "Gold":
			color2 = Color.FromArgb(255, 215, 0);
			break;
		}
		if (!color2.IsEmpty)
		{
			return BuildColor(color2, value);
		}
		return BuildColor(color, value);
	}

	private void CheckFootEndnote()
	{
		byte[] item = ((m_hasFootnote && m_hasEndnote) ? m_encoding.GetBytes(c_slashSymbol + "fet2") : ((!m_hasEndnote) ? m_encoding.GetBytes(c_slashSymbol + "fet0") : m_encoding.GetBytes(c_slashSymbol + "fet1")));
		m_mainBodyBytesList.Add(item);
	}

	private string BuildFieldType(FieldType type)
	{
		return type switch
		{
			FieldType.FieldAdvance => "ADVANCE ", 
			FieldType.FieldAuthor => "AUTHOR ", 
			FieldType.FieldAutoNum => "AUTONUM ", 
			FieldType.FieldAutoNumLegal => "AUTONUMLGL ", 
			FieldType.FieldAutoNumOutline => "AUTONUMOUT ", 
			FieldType.FieldAutoText => "AUTOTEXT ", 
			FieldType.FieldAutoTextList => "AUTOTEXTLIST ", 
			FieldType.FieldAsk => "ASK ", 
			FieldType.FieldBarCode => "BARCODE ", 
			FieldType.FieldComments => "COMMENTS ", 
			FieldType.FieldCreateDate => "CREATEDATE ", 
			FieldType.FieldDate => "DATE ", 
			FieldType.FieldDocProperty => "DOCPROPERTY ", 
			FieldType.FieldDocVariable => "DOCVARIABLE ", 
			FieldType.FieldEditTime => "EDITTIME ", 
			FieldType.FieldIf => "IF ", 
			FieldType.FieldFillIn => "FILLIN ", 
			FieldType.FieldFileName => "FILENAME ", 
			FieldType.FieldFileSize => "FILESIZE ", 
			FieldType.FieldFormCheckBox => "FORMCHECKBOX ", 
			FieldType.FieldFormDropDown => "FORMDROPDOWN ", 
			FieldType.FieldFormTextInput => "FORMTEXT ", 
			FieldType.FieldGoToButton => "GOTOBUTTON ", 
			FieldType.FieldHyperlink => "HYPERLINK ", 
			FieldType.FieldIncludePicture => "INCLUDEPICTURE ", 
			FieldType.FieldIncludeText => "INCLUDETEXT ", 
			FieldType.FieldIndex => "INDEX ", 
			FieldType.FieldInfo => "INFO ", 
			FieldType.FieldKeyWord => "KEYWORDS ", 
			FieldType.FieldLastSavedBy => "LASTSAVEDBY ", 
			FieldType.FieldLink => "LINK ", 
			FieldType.FieldListNum => "LISTNUM ", 
			FieldType.FieldMacroButton => "MACROBUTTON ", 
			FieldType.FieldMergeField => "MERGEFIELD ", 
			FieldType.FieldNoteRef => "NOTEREF ", 
			FieldType.FieldNumChars => "NUMCHARS ", 
			FieldType.FieldNumPages => "NUMPAGES ", 
			FieldType.FieldNumWords => "NUMWORDS ", 
			FieldType.FieldPage => "PAGE ", 
			FieldType.FieldPageRef => "PAGEREF ", 
			FieldType.FieldPrint => "PRINT ", 
			FieldType.FieldPrintDate => "PRINTDATE ", 
			FieldType.FieldPrivate => "PRIVATE ", 
			FieldType.FieldQuote => "QUOTE ", 
			FieldType.FieldRef => "REF ", 
			FieldType.FieldRevisionNum => "REVNUM ", 
			FieldType.FieldSaveDate => "SAVEDATE ", 
			FieldType.FieldSection => "SECTION ", 
			FieldType.FieldSectionPages => "SECTIONPAGES ", 
			FieldType.FieldSequence => "SEQ ", 
			FieldType.FieldStyleRef => "STYLEREF ", 
			FieldType.FieldSubject => "SUBJECT ", 
			FieldType.FieldSymbol => "SYMBOL ", 
			FieldType.FieldTemplate => "TEMPLATE ", 
			FieldType.FieldTime => "TIME ", 
			FieldType.FieldTitle => "TITLE ", 
			FieldType.FieldTOA => "TOA ", 
			FieldType.FieldTOC => "TOC ", 
			FieldType.FieldUserAddress => "USERADDRESS ", 
			FieldType.FieldUserInitials => "USERINITIALS ", 
			FieldType.FieldUserName => "USERNAME ", 
			FieldType.FieldAddin => "ADDIN ", 
			_ => string.Empty, 
		};
	}

	private string GetRtfShapeColor(Color color)
	{
		string text = color.R.ToString("X");
		string text2 = color.G.ToString("X");
		int num = int.Parse(color.B.ToString("X") + text2 + text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		if (color != Color.Red && color != Color.Green && color != Color.Blue && color.R != 128 && color.G != 128 && color.B != 128 && color.R != 192 && color.G != 192 && color.B != 192)
		{
			num *= 16;
		}
		return num.ToString();
	}

	private string GetRtfPageBackgroundColor(Color color)
	{
		string text = color.R.ToString("X");
		string text2 = color.G.ToString("X");
		return int.Parse(color.B.ToString("X") + text2 + text, NumberStyles.HexNumber, CultureInfo.InvariantCulture).ToString();
	}

	private void WriteElements(string param)
	{
		if (!string.IsNullOrEmpty(param))
		{
			byte[] bytes = m_encoding.GetBytes("{");
			m_stream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(param);
			m_stream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes("}");
			m_stream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(Environment.NewLine);
			m_stream.Write(bytes, 0, bytes.Length);
		}
	}

	private string GetNextFontId(bool isBidi)
	{
		if (isBidi)
		{
			return c_slashSymbol + "af" + m_fontId++;
		}
		return c_slashSymbol + "f" + m_fontId++;
	}

	private int GetNextId()
	{
		return m_uniqueId++;
	}

	private int GetNextColorId()
	{
		return m_colorId++;
	}

	private string IsFontEntryExits(string fontName, bool IsBidi)
	{
		string text = (m_isCyrillicText ? "fcharset204" : (((fontName[0] < 'A' || fontName[0] > 'Z') && (fontName[0] < 'a' || fontName[0] > 'z')) ? "fcharset134" : "fcharset0"));
		text = text + "-" + fontName;
		if (!IsBidi)
		{
			if (FontEntries.ContainsKey(text))
			{
				return FontEntries[text];
			}
		}
		else if (AssociatedFontEntries.ContainsKey(text))
		{
			return AssociatedFontEntries[text];
		}
		return null;
	}

	private void AppendFont(string fontId, string fontName)
	{
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(m_fontBytes, 0, m_fontBytes.Length);
		byte[] bytes = m_encoding.GetBytes("{");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(fontId);
		memoryStream.Write(bytes, 0, bytes.Length);
		string text;
		if (m_isCyrillicText)
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "fcharset204");
			memoryStream.Write(bytes, 0, bytes.Length);
			text = "fcharset204";
		}
		else if ((fontName[0] >= 'A' && fontName[0] <= 'Z') || (fontName[0] >= 'a' && fontName[0] <= 'z'))
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "fcharset0");
			memoryStream.Write(bytes, 0, bytes.Length);
			text = "fcharset0";
		}
		else
		{
			bytes = m_encoding.GetBytes(c_slashSymbol + "fcharset134");
			memoryStream.Write(bytes, 0, bytes.Length);
			text = "fcharset134";
		}
		bytes = m_encoding.GetBytes(" ");
		memoryStream.Write(bytes, 0, bytes.Length);
		if ((fontName[0] >= 'A' && fontName[0] <= 'Z') || (fontName[0] >= 'a' && fontName[0] <= 'z'))
		{
			bytes = m_encoding.GetBytes(fontName);
			memoryStream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			bytes = m_encoding.GetBytes("{\\*\\falt ");
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(PrepareText(fontName));
			memoryStream.Write(bytes, 0, bytes.Length);
			bytes = m_encoding.GetBytes(" }");
			memoryStream.Write(bytes, 0, bytes.Length);
			byte[] bytes2 = Encoding.GetEncoding("GB2312").GetBytes(fontName);
			foreach (byte b in bytes2)
			{
				bytes = m_encoding.GetBytes("\\'");
				memoryStream.Write(bytes, 0, bytes.Length);
				bytes = m_encoding.GetBytes(b.ToString("X").ToLower());
				memoryStream.Write(bytes, 0, bytes.Length);
			}
		}
		text = text + "-" + fontName;
		bytes = m_encoding.GetBytes(";}");
		memoryStream.Write(bytes, 0, bytes.Length);
		if (fontId.StartsWith("\\a"))
		{
			AssociatedFontEntries.Add(text, fontId);
		}
		else
		{
			FontEntries.Add(text, fontId);
		}
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		m_fontBytes = memoryStream.ToArray();
	}

	private string BuildColor(Color color, string attributeStr)
	{
		if (color.IsEmpty)
		{
			return string.Empty;
		}
		if (ColorTable.ContainsKey(color))
		{
			return attributeStr + ColorTable[color];
		}
		ColorTable.Add(color, m_colorId);
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(m_colorBytes, 0, m_colorBytes.Length);
		byte[] bytes = m_encoding.GetBytes(c_slashSymbol + "red" + color.R);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "green" + color.G);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(c_slashSymbol + "blue" + color.B);
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(";");
		memoryStream.Write(bytes, 0, bytes.Length);
		bytes = m_encoding.GetBytes(Environment.NewLine);
		memoryStream.Write(bytes, 0, bytes.Length);
		m_colorBytes = memoryStream.ToArray();
		return attributeStr + GetNextColorId();
	}

	private string WriteFontName(WCharacterFormat cFormat)
	{
		string text = IsFontEntryExits(cFormat.FontName, IsBidi: false);
		if (text == null)
		{
			text = GetNextFontId(isBidi: false);
			AppendFont(text, cFormat.FontName);
			return text;
		}
		return text;
	}

	private string WriteFontNameBidi(WCharacterFormat cFormat)
	{
		string text = IsFontEntryExits(cFormat.FontName, IsBidi: true);
		if (text == null)
		{
			text = GetNextFontId(isBidi: true);
			AppendFont(text, cFormat.FontName);
			return text;
		}
		return text;
	}

	private bool HasParaEnd(WParagraph para)
	{
		if ((para.IsInCell && para.NextSibling == null) || para.OwnerTextBody == null)
		{
			return false;
		}
		if (para.OwnerTextBody.OwnerBase is WFootnote && para.NextSibling == null)
		{
			return false;
		}
		if (para.OwnerTextBody.OwnerBase is WComment && para.NextSibling == null)
		{
			return false;
		}
		if (para.OwnerTextBody.OwnerBase is WSection && (para.OwnerTextBody.OwnerBase as WSection).PreviousSibling == null && para.Items.Count == 0 && para.NextSibling == null && !(para.PreviousSibling is WTable) && para.Document.Sections.Count > 1)
		{
			return false;
		}
		return true;
	}

	private string PrepareText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		text = text.Replace(c_symbol92, c_symbol92 + c_symbol92);
		string text2 = string.Empty;
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (c >= '\u0350' && c <= 'я')
			{
				string text3 = c_slashSymbol;
				int num = c;
				string text4 = text3 + "u" + num + "\\'3f";
				text2 += text4;
				m_isCyrillicText = true;
			}
			else
			{
				text2 += c;
			}
		}
		text2 = " " + text2;
		for (int num2 = text2.IndexOf('\t'); num2 != -1; num2 = text2.IndexOf('\t'))
		{
			text2 = text2.Remove(num2, 1);
			text2 = text2.Insert(num2, c_slashSymbol + "tab ");
		}
		text2 = text2.Replace(c_transfer, c_slashSymbol + "~");
		text2 = text2.Replace(c_symbol123, c_slashSymbol + "{");
		text2 = text2.Replace(c_symbol125, c_slashSymbol + "}");
		text2 = text2.Replace(c_symbol30, c_slashSymbol + "_");
		text2 = text2.Replace(c_symbol31, c_slashSymbol + "-");
		text2 = text2.Replace(c_symbol61553, c_slashSymbol + "u-3983" + c_slashSymbol + "'3f");
		text2 = text2.Replace(c_symbol61549, c_slashSymbol + "u-3987" + c_slashSymbol + "'3f");
		text2 = text2.Replace(c_singleOpenQuote, c_slashSymbol + "lquote ");
		text2 = text2.Replace(c_singleCloseQuote, c_slashSymbol + "rquote ");
		text2 = text2.Replace(c_enDash, c_slashSymbol + "endash ");
		text2 = text2.Replace(c_emDash, c_slashSymbol + "emdash ");
		text2 = text2.Replace(c_enSpace, c_slashSymbol + "u8194" + c_slashSymbol + "'20");
		text2 = text2.Replace(c_emSpace, c_slashSymbol + "u8195" + c_slashSymbol + "'20");
		text2 = text2.Replace(c_copyRight, c_slashSymbol + "'a9");
		text2 = text2.Replace(c_registered, c_slashSymbol + "'ae");
		text2 = text2.Replace(c_tradeMark, c_slashSymbol + "'99");
		text2 = text2.Replace(c_section, c_slashSymbol + "'a7");
		text2 = text2.Replace(c_lineBreak, c_slashSymbol + "line");
		text2 = text2.Replace(c_paraMark, c_slashSymbol + "'b6");
		text2 = text2.Replace(c_doubleOpenQuote, c_slashSymbol + "'93");
		text2 = text2.Replace(c_doubleCloseQuote, c_slashSymbol + "'94");
		return ReplaceUnicode(text2);
	}

	private string ReplaceUnicode(string text)
	{
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (c > '\u007f')
			{
				string newValue = "\\u" + Convert.ToUInt32(c) + "?";
				text = text.Replace(c.ToString(), newValue);
			}
		}
		return text;
	}

	private string BuildTextRangeStr(WCharacterFormat cFormat, string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		stringBuilder.Append(text);
		stringBuilder.Insert(1, BuildCharacterFormat(cFormat));
		stringBuilder.Append("}");
		stringBuilder.Append(Environment.NewLine);
		m_isCyrillicText = false;
		return stringBuilder.ToString();
	}

	private bool WriteFieldEnd(WFieldMark mark)
	{
		if (mark.PreviousSibling != null)
		{
			if (mark.PreviousSibling is WCheckBox || mark.PreviousSibling is WDropDownFormField)
			{
				return true;
			}
			if (mark.PreviousSibling is WField && (mark.PreviousSibling as WField).FieldType == FieldType.FieldGoToButton)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private WSection GetOwnerSection(Entity entity)
	{
		while (entity != null && entity.EntityType != EntityType.Section)
		{
			entity = entity.Owner;
		}
		if (entity != null)
		{
			return entity as WSection;
		}
		return null;
	}

	private void InitCellEndPos()
	{
	}
}
