using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.IO;
using DocGen.Pdf.Native;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Graphics.Fonts;

internal class UnicodeTrueTypeFont : ITrueTypeFont
{
	private const string c_driverName = "DISPLAY";

	private const string c_nameString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private const string c_cmapPrefix = "/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\r\n/CIDSystemInfo << /Registry (Adobe)/Ordering (UCS)/Supplement 0>> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\r\n";

	private const string c_cmapEndCodespaceRange = "endcodespacerange\r\n";

	private const string c_cmapSuffix = "endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\r\n";

	private const string c_cmapBeginRange = "beginbfrange\r\n";

	private const string c_cmapEndRange = "endbfrange\r\n";

	private const int c_cmapNextRangeValue = 100;

	private const string c_registry = "Adobe";

	private const int c_defWidthIndex = 32;

	private const int c_cidStreamLength = 11;

	private static object s_syncLock = new object();

	private Stream m_fontStream;

	internal bool is_filePath;

	private string m_filePath;

	private float m_size;

	private PdfFontMetrics m_metrics;

	private PdfDictionary m_fontDictionary;

	private PdfDictionary m_descendantFont;

	private PdfDictionary m_fontDescriptor;

	private PdfStream m_fontProgram;

	private PdfStream m_cmap;

	private PdfStream m_CidStream;

	private TtfReader m_ttfReader;

	private Dictionary<int, OtfGlyphInfo> m_openTypeGlyphs;

	private List<TtfGlyphInfo> glyphInfo;

	internal bool m_isIncreasedUsedChar;

	internal Dictionary<char, char> m_usedChars;

	private string m_subsetName;

	internal TtfMetrics m_ttfMetrics;

	private CompositeFontType m_type;

	private string metricsName = string.Empty;

	private bool m_isEmbedFont;

	private bool m_isAzureCompatible;

	private bool m_isFontFilePath;

	private bool m_isCompress;

	private bool m_isSkipFontEmbed;

	internal bool conformanceEnabled;

	internal bool m_isXPSFontStream;

	private bool m_fullEmbed;

	private bool m_useFloatingFactorForMeasure;

	internal bool m_isClearUsedChars;

	public float Size => m_size;

	internal bool IsEmbed
	{
		get
		{
			return m_isEmbedFont;
		}
		set
		{
			m_isEmbedFont = value;
		}
	}

	public PdfFontMetrics Metrics => m_metrics;

	internal TtfReader TtfReader => m_ttfReader;

	internal string FontFile => m_filePath;

	internal Stream FontStream => m_fontStream;

	internal TtfMetrics TtfMetrics => m_ttfMetrics;

	internal CompositeFontType FontType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	internal bool SkipFontEmbed
	{
		get
		{
			return m_isSkipFontEmbed;
		}
		set
		{
			m_isSkipFontEmbed = value;
		}
	}

	internal bool ForceFullFontEmbed
	{
		get
		{
			return m_fullEmbed;
		}
		set
		{
			m_fullEmbed = value;
		}
	}

	internal UnicodeTrueTypeFont(Stream font, float size, CompositeFontType type, string name)
	{
		metricsName = name;
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_fontStream = font;
		m_size = size;
		m_type = type;
		byte[] array = new byte[font.Length];
		font.Read(array, 0, array.Length);
		using MemoryStream font2 = new MemoryStream(array);
		Initialize(font2);
	}

	public UnicodeTrueTypeFont(Stream font, float size, CompositeFontType type, bool isFloatingPoint)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_fontStream = font;
		m_size = size;
		m_type = type;
		byte[] array = new byte[font.Length];
		font.Read(array, 0, array.Length);
		m_useFloatingFactorForMeasure = isFloatingPoint;
		using MemoryStream font2 = new MemoryStream(array);
		Initialize(font2);
	}

	public UnicodeTrueTypeFont(UnicodeTrueTypeFont prototype)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_ttfReader = prototype.m_ttfReader;
		m_fontStream = prototype.m_fontStream;
		m_ttfMetrics = prototype.TtfMetrics;
		m_filePath = prototype.FontFile;
		m_size = ((ITrueTypeFont)prototype).Size;
	}

	internal UnicodeTrueTypeFont(string filePath, float size, CompositeFontType type)
	{
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		if (filePath.Length == 0)
		{
			throw new ArgumentException("filePath - string can not be empty");
		}
		m_filePath = filePath;
		is_filePath = true;
		m_size = size;
		m_type = type;
		Initialize();
	}

	public void SetSymbols(string text)
	{
		lock (PdfDocument.Cache)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (m_usedChars == null)
			{
				m_usedChars = new Dictionary<char, char>();
			}
			foreach (char key in text)
			{
				m_usedChars[key] = '\0';
			}
		}
	}

	public void SetSymbols(ushort[] glyphs)
	{
		if (glyphs == null)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (m_usedChars == null)
		{
			m_usedChars = new Dictionary<char, char>();
		}
		foreach (int glyphIndex in glyphs)
		{
			TtfGlyphInfo glyph = m_ttfReader.GetGlyph(glyphIndex);
			if (!glyph.Empty)
			{
				char key = (char)glyph.CharCode;
				m_usedChars[key] = '\0';
			}
		}
		if (!m_isEmbedFont)
		{
			GetDescendantWidth();
		}
	}

	internal void SetSymbols(string text, bool opentype)
	{
		if (m_openTypeGlyphs == null)
		{
			m_openTypeGlyphs = new Dictionary<int, OtfGlyphInfo>();
		}
		foreach (char charCode in text)
		{
			TtfGlyphInfo glyph = m_ttfReader.GetGlyph(charCode);
			if (glyph.Index > -1)
			{
				m_openTypeGlyphs[glyph.Index] = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
			}
		}
	}

	internal void SetSymbols(ushort[] glyphs, bool openType)
	{
		if (m_openTypeGlyphs == null)
		{
			m_openTypeGlyphs = new Dictionary<int, OtfGlyphInfo>();
		}
		for (int i = 0; i < glyphs.Length; i++)
		{
			TtfGlyphInfo glyph = m_ttfReader.GetGlyph(glyphs[i]);
			if (!glyph.Empty && glyph.Index > -1)
			{
				m_openTypeGlyphs[glyph.Index] = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
			}
		}
	}

	internal void SetSymbols(OtfGlyphInfoList line)
	{
		if (m_openTypeGlyphs == null)
		{
			m_openTypeGlyphs = new Dictionary<int, OtfGlyphInfo>();
		}
		foreach (OtfGlyphInfo glyph in line.Glyphs)
		{
			m_openTypeGlyphs[glyph.Index] = glyph;
		}
	}

	public IPdfPrimitive GetInternals()
	{
		return m_fontDictionary;
	}

	public bool EqualsToFont(PdfFont font)
	{
		bool flag = false;
		PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
		if (pdfTrueTypeFont != null && pdfTrueTypeFont.Unicode)
		{
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			UnicodeTrueTypeFont unicodeTrueTypeFont = (UnicodeTrueTypeFont)pdfTrueTypeFont.InternalFont;
			if (unicodeTrueTypeFont != null)
			{
				if (unicodeTrueTypeFont.m_ttfMetrics.FontFamily != null)
				{
					flag2 = m_ttfMetrics.FontFamily.Equals(unicodeTrueTypeFont.m_ttfMetrics.FontFamily);
				}
				flag3 = m_ttfMetrics.MacStyle == unicodeTrueTypeFont.m_ttfMetrics.MacStyle;
				flag = flag2 && flag3;
				if (flag)
				{
					flag4 = IsEqualFontStream(m_fontStream, unicodeTrueTypeFont.m_fontStream);
					flag = flag && flag4;
				}
			}
		}
		else if (pdfTrueTypeFont != null && SkipFontEmbed)
		{
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			UnicodeTrueTypeFont unicodeTrueTypeFont2 = (UnicodeTrueTypeFont)pdfTrueTypeFont.InternalFont;
			if (unicodeTrueTypeFont2.m_ttfMetrics.FontFamily != null)
			{
				flag5 = m_ttfMetrics.FontFamily.Equals(unicodeTrueTypeFont2.m_ttfMetrics.FontFamily);
			}
			flag6 = m_ttfMetrics.MacStyle == unicodeTrueTypeFont2.m_ttfMetrics.MacStyle;
			flag = flag5 && flag6;
			if (flag)
			{
				flag7 = IsEqualFontStream(m_fontStream, unicodeTrueTypeFont2.m_fontStream);
				flag = flag && flag7;
			}
		}
		return flag;
	}

	private bool IsEqualFontStream(Stream currentFont, Stream previousFont)
	{
		if (currentFont != null && currentFont.CanRead && previousFont != null && previousFont.CanRead)
		{
			if (currentFont.Length != previousFont.Length)
			{
				return false;
			}
			currentFont.Position = 0L;
			previousFont.Position = 0L;
			long num = ((currentFont.Length > 1024) ? 1024 : currentFont.Length);
			for (int i = 0; i < num; i++)
			{
				int num2 = currentFont.ReadByte();
				if (currentFont.Position == previousFont.Position)
				{
					return true;
				}
				int value = previousFont.ReadByte();
				if (num2.CompareTo(value) != 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void CreateInternals()
	{
		m_fontDictionary = new PdfDictionary();
		m_fontProgram = new PdfStream();
		m_cmap = new PdfStream();
		if ((PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001) || PdfDocument.FontEmbeddingEnabled)
		{
			m_CidStream = new PdfStream();
		}
		m_descendantFont = new PdfDictionary();
		m_metrics = new PdfFontMetrics();
		if (m_fontStream != null)
		{
			m_ttfReader.Reader = new BinaryReader(m_fontStream, DocGen.Pdf.Graphics.TtfReader.Encoding);
			m_ttfReader.m_AnsiEncode = SkipFontEmbed;
			m_ttfReader.conformanceEnabled = conformanceEnabled;
		}
		else if (is_filePath)
		{
			m_ttfReader.Reader = GetFontData();
		}
		m_ttfReader.CreateInternals();
		m_ttfMetrics = m_ttfReader.Metrics;
		InitializeMetrics();
		m_subsetName = GetFontName();
		if ((PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001) || PdfDocument.FontEmbeddingEnabled)
		{
			CreateCidSet();
		}
		CreateDescendantFont();
		CreateCmap();
		CreateFontDictionary();
		CreateFontProgram();
		if (is_filePath)
		{
			m_ttfReader.Reader.Dispose();
		}
	}

	public float GetCharWidth(char charCode)
	{
		return m_ttfReader.GetCharWidth(charCode);
	}

	public float GetLineWidth(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = 0f;
		int i = 0;
		for (int length = line.Length; i < length; i++)
		{
			char charCode = line[i];
			float charWidth = ((ITrueTypeFont)this).GetCharWidth(charCode);
			num += charWidth;
		}
		return num;
	}

	public void Close()
	{
		if (m_fontDictionary != null)
		{
			if (!SkipFontEmbed)
			{
				m_fontDictionary.BeginSave -= FontDictionaryBeginSave;
			}
			m_fontDictionary.Clear();
			m_fontDictionary = null;
		}
		if (m_fontDescriptor != null)
		{
			if ((PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001) || PdfDocument.FontEmbeddingEnabled)
			{
				m_fontDescriptor.BeginSave -= FontDescriptorBeginSave;
			}
			m_fontDescriptor.Clear();
			m_fontDescriptor = null;
		}
		if (m_descendantFont != null)
		{
			m_descendantFont.BeginSave -= DescendantFontBeginSave;
			m_descendantFont.Clear();
			m_descendantFont = null;
		}
		if (m_fontProgram != null)
		{
			m_fontProgram.BeginSave -= FontProgramBeginSave;
			m_fontProgram.Clear();
			m_fontProgram = null;
		}
		if (m_cmap != null)
		{
			m_cmap.BeginSave -= CmapBeginSave;
			m_cmap.Clear();
			m_cmap = null;
		}
		if (m_CidStream != null)
		{
			m_CidStream.BeginSave -= CidBeginSave;
			m_CidStream.Clear();
			m_CidStream = null;
		}
		if (m_ttfReader != null)
		{
			m_ttfReader.Close();
			m_ttfReader = null;
		}
		if (m_usedChars != null)
		{
			m_usedChars.Clear();
			m_usedChars = null;
		}
		if (m_fontStream != null)
		{
			m_fontStream = null;
		}
		m_filePath = null;
		m_metrics = null;
		m_subsetName = null;
	}

	private void Initialize(Stream font)
	{
		using BinaryReader reader = new BinaryReader(font, DocGen.Pdf.Graphics.TtfReader.Encoding);
		if (metricsName != string.Empty)
		{
			m_ttfReader = new TtfReader(reader, metricsName);
		}
		else
		{
			m_ttfReader = new TtfReader(reader, m_useFloatingFactorForMeasure);
		}
		m_ttfMetrics = m_ttfReader.Metrics;
	}

	private void InitializeMetrics()
	{
		TtfMetrics metrics = m_ttfReader.Metrics;
		m_metrics.Ascent = metrics.MacAscent;
		m_metrics.Descent = metrics.MacDescent;
		m_metrics.Height = metrics.MacAscent - metrics.MacDescent + (float)metrics.LineGap;
		m_metrics.Name = metrics.FontFamily;
		m_metrics.PostScriptName = metrics.PostScriptName;
		m_metrics.Size = m_size;
		m_metrics.WidthTable = new StandardWidthTable(metrics.WidthTable);
		m_metrics.LineGap = metrics.LineGap;
		m_metrics.SubScriptSizeFactor = metrics.SubScriptSizeFactor;
		m_metrics.SuperscriptSizeFactor = metrics.SuperscriptSizeFactor;
		m_metrics.IsBold = metrics.IsBold;
	}

	private void CreateFontProgram()
	{
		m_fontProgram.BeginSave += FontProgramBeginSave;
	}

	private void GenerateFontProgram()
	{
		byte[] array = null;
		if (ForceFullFontEmbed)
		{
			m_ttfReader.InternalReader.Seek(0L);
			if (!m_ttfMetrics.ContainsCFF || !m_ttfReader.isOpenTypeFont)
			{
				array = m_ttfReader.GetType0FontProgram();
			}
			else
			{
				Stream stream = TtfReader.ReadCffTable();
				array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				m_fontProgram["Subtype"] = new PdfName("CIDFontType0C");
			}
			m_fontProgram.Clear();
			m_fontProgram.Write(array);
			if (m_usedChars != null)
			{
				m_usedChars.Clear();
			}
			return;
		}
		m_usedChars = ((m_usedChars == null) ? new Dictionary<char, char>() : m_usedChars);
		if (is_filePath)
		{
			m_ttfReader.Reader = GetFontData();
		}
		m_ttfReader.InternalReader.Seek(0L);
		if (m_type == CompositeFontType.Type0 && m_usedChars.Count > 0 && (m_openTypeGlyphs == null || m_openTypeGlyphs.Count <= 0))
		{
			if (m_ttfMetrics.ContainsCFF && m_ttfReader.isOpenTypeFont)
			{
				Stream stream2 = TtfReader.ReadCffTable();
				array = new byte[stream2.Length];
				stream2.Read(array, 0, array.Length);
				m_fontProgram["Subtype"] = new PdfName("CIDFontType0C");
			}
			else if (m_isXPSFontStream && m_fontStream != null)
			{
				array = new byte[m_fontStream.Length];
				m_fontStream.Read(array, 0, array.Length);
				m_isXPSFontStream = false;
			}
			else
			{
				array = m_ttfReader.ReadFontProgram(m_usedChars);
			}
		}
		else if (m_type == CompositeFontType.Type0 && m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0)
		{
			if (m_ttfMetrics.ContainsCFF && m_ttfReader.isOpenTypeFont)
			{
				Stream stream3 = TtfReader.ReadCffTable();
				array = new byte[stream3.Length];
				stream3.Read(array, 0, array.Length);
				m_fontProgram["Subtype"] = new PdfName("CIDFontType0C");
			}
			else
			{
				array = m_ttfReader.ReadOpenTypeFontProgram(m_openTypeGlyphs);
			}
		}
		else if (m_ttfMetrics.ContainsCFF && m_ttfReader.isOpenTypeFont)
		{
			Stream stream4 = TtfReader.ReadCffTable();
			array = new byte[stream4.Length];
			stream4.Read(array, 0, array.Length);
			m_fontProgram["Subtype"] = new PdfName("CIDFontType0C");
		}
		else
		{
			Stream stream5 = m_fontStream;
			if (is_filePath)
			{
				stream5 = GetFontData().BaseStream;
			}
			array = new byte[stream5.Length];
			m_fontProgram["Length1"] = new PdfNumber(array.Length);
			stream5.Read(array, 0, (int)stream5.Length - 1);
			if (PdfDocument.EnableCache && m_fontStream == null)
			{
				stream5.Dispose();
			}
		}
		m_fontProgram.Clear();
		m_fontProgram.Write(array);
		if (m_isClearUsedChars)
		{
			m_usedChars.Clear();
		}
	}

	private void CreateFontDictionary()
	{
		m_fontDictionary.IsFont = true;
		if (SkipFontEmbed)
		{
			m_fontDictionary["Widths"] = new PdfArray(m_ttfMetrics.WidthTable);
		}
		else
		{
			m_fontDictionary.BeginSave += FontDictionaryBeginSave;
		}
		m_fontDictionary["Type"] = new PdfName("Font");
		m_fontDictionary["BaseFont"] = new PdfName(m_subsetName);
		if (m_type == CompositeFontType.Type0)
		{
			m_fontDictionary["Subtype"] = new PdfName("Type0");
			m_fontDictionary["Encoding"] = new PdfName("Identity-H");
			PdfArray pdfArray = new PdfArray();
			PdfReferenceHolder element = new PdfReferenceHolder(m_descendantFont);
			pdfArray.IsFont = true;
			pdfArray.Add(element);
			m_fontDictionary["DescendantFonts"] = pdfArray;
			return;
		}
		m_fontDictionary["Name"] = new PdfName(m_subsetName);
		m_fontDictionary["Subtype"] = new PdfName("TrueType");
		if (m_subsetName != "SymbolMT")
		{
			m_fontDictionary["Encoding"] = new PdfName("WinAnsiEncoding");
		}
		m_fontDictionary["Widths"] = new PdfArray(m_ttfMetrics.WidthTable);
		m_fontDictionary["FirstChar"] = new PdfNumber(0);
		m_fontDictionary["LastChar"] = new PdfNumber(255);
		IPdfPrimitive obj = CreateFontDescriptor();
		m_fontDictionary["FontDescriptor"] = new PdfReferenceHolder(obj);
	}

	private void CreateDescendantFont()
	{
		m_descendantFont.IsFont = true;
		m_descendantFont.BeginSave += DescendantFontBeginSave;
		m_descendantFont["Type"] = new PdfName("Font");
		string value = "CIDFontType2";
		if (TtfReader != null && TtfReader.isOpenTypeFont)
		{
			value = "CIDFontType0";
		}
		m_descendantFont["Subtype"] = new PdfName(value);
		m_descendantFont["BaseFont"] = new PdfName(m_subsetName);
		m_descendantFont["CIDToGIDMap"] = new PdfName("Identity");
		m_descendantFont["DW"] = new PdfNumber(1000);
		m_fontDescriptor = CreateFontDescriptor();
		if ((PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001) || PdfDocument.FontEmbeddingEnabled)
		{
			m_fontDescriptor.BeginSave += FontDescriptorBeginSave;
		}
		m_descendantFont["FontDescriptor"] = new PdfReferenceHolder(m_fontDescriptor);
		IPdfPrimitive value2 = CreateSystemInfo();
		m_descendantFont["CIDSystemInfo"] = value2;
	}

	internal int GetUsedCharsCount()
	{
		return m_usedChars.Count;
	}

	internal void SetGlyphInfo(List<TtfGlyphInfo> collection)
	{
		glyphInfo = collection;
	}

	private void CreateCmap()
	{
		m_cmap.BeginSave += CmapBeginSave;
	}

	private void CreateCidSet()
	{
		m_CidStream.BeginSave += CidBeginSave;
	}

	private void GenerateCmap()
	{
		if ((m_usedChars != null && m_usedChars.Count > 0 && m_openTypeGlyphs == null) || ForceFullFontEmbed)
		{
			Dictionary<int, int> dictionary = null;
			dictionary = ((!ForceFullFontEmbed) ? m_ttfReader.GetGlyphChars(m_usedChars) : m_ttfReader.GetAllGlyphChars());
			if (dictionary.Count <= 0)
			{
				return;
			}
			int[] array = new int[dictionary.Count];
			dictionary.Keys.CopyTo(array, 0);
			Array.Sort(array);
			List<int> list = new List<int>(dictionary.Keys.Count);
			list.AddRange(dictionary.Keys);
			Array.Sort(list.ToArray());
			int n = array[0];
			int n2 = array[^1];
			string value = ToHexString(n) + ToHexString(n2) + "\r\n";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\r\n/CIDSystemInfo << /Registry (Adobe)/Ordering (UCS)/Supplement 0>> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\r\n");
			stringBuilder.Append(value);
			stringBuilder.Append("endcodespacerange\r\n");
			int num = 0;
			int i = 0;
			for (int num2 = array.Length; i < num2; i++)
			{
				if (num == 0)
				{
					if (i != 0)
					{
						stringBuilder.Append("endbfrange\r\n");
					}
					num = Math.Min(100, array.Length - i);
					stringBuilder.Append(num);
					stringBuilder.Append(" ");
					stringBuilder.Append("beginbfrange\r\n");
				}
				num--;
				int num3 = array[i];
				stringBuilder.AppendFormat("<{0:X04}><{0:X04}><{1:X04}>\n", num3, dictionary[num3]);
			}
			stringBuilder.Append("endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\r\n");
			m_cmap.Clear();
			m_cmap.Write(stringBuilder.ToString());
		}
		else
		{
			GenerateOpenTypeCmap();
		}
	}

	private void GenerateOpenTypeCmap()
	{
		UpdateOpenTypeGlyphs();
		if (m_openTypeGlyphs == null || m_openTypeGlyphs.Count <= 0)
		{
			return;
		}
		int[] array = new int[m_openTypeGlyphs.Count];
		m_openTypeGlyphs.Keys.CopyTo(array, 0);
		Array.Sort(array);
		int n = array[0];
		int n2 = array[^1];
		string value = ToHexString(n) + ToHexString(n2) + "\r\n";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\r\n/CIDSystemInfo << /Registry (Adobe)/Ordering (UCS)/Supplement 0>> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\r\n");
		stringBuilder.Append(value);
		stringBuilder.Append("endcodespacerange\r\n");
		int num = 0;
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (num == 0)
			{
				if (i != 0)
				{
					stringBuilder.Append("endbfrange\r\n");
				}
				num = Math.Min(100, array.Length - i);
				stringBuilder.Append(num);
				stringBuilder.Append(" ");
				stringBuilder.Append("beginbfrange\r\n");
			}
			num--;
			int num3 = array[i];
			stringBuilder.AppendFormat("<{0:X04}><{0:X04}>", num3);
			OtfGlyphInfo otfGlyphInfo = m_openTypeGlyphs[num3];
			if (otfGlyphInfo.CharCode != -1)
			{
				stringBuilder.AppendFormat("<{0:X04}>\n", otfGlyphInfo.CharCode);
				continue;
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("<");
			if (otfGlyphInfo.Characters != null)
			{
				char[] characters = otfGlyphInfo.Characters;
				foreach (char c in characters)
				{
					stringBuilder2.AppendFormat("{0:X04}", (int)c);
				}
			}
			stringBuilder2.Append(">\n");
			stringBuilder.Append(stringBuilder2.ToString());
		}
		stringBuilder.Append("endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\r\n");
		m_cmap.Clear();
		m_cmap.Write(stringBuilder.ToString());
	}

	private IPdfPrimitive CreateSystemInfo()
	{
		return new PdfDictionary
		{
			["Registry"] = new PdfString("Adobe"),
			["Ordering"] = new PdfString("Identity"),
			["Supplement"] = new PdfNumber(0)
		};
	}

	private PdfDictionary CreateFontDescriptor()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		TtfMetrics metrics = m_ttfReader.Metrics;
		pdfDictionary.IsFont = true;
		pdfDictionary["Type"] = new PdfName("FontDescriptor");
		pdfDictionary["FontName"] = new PdfName(m_subsetName);
		pdfDictionary["Flags"] = new PdfNumber(GetDescriptorFlags());
		pdfDictionary["FontBBox"] = PdfArray.FromRectangle(GetBoundBox());
		pdfDictionary["MissingWidth"] = new PdfNumber(metrics.WidthTable[32]);
		pdfDictionary["StemV"] = new PdfNumber(metrics.IsBold ? 160 : ((int)metrics.StemV));
		pdfDictionary["ItalicAngle"] = new PdfNumber((int)metrics.ItalicAngle);
		pdfDictionary["CapHeight"] = new PdfNumber((int)metrics.CapHeight);
		pdfDictionary["Ascent"] = new PdfNumber((int)metrics.WinAscent);
		pdfDictionary["Descent"] = new PdfNumber((int)metrics.WinDescent);
		pdfDictionary["Leading"] = new PdfNumber((int)metrics.Leading);
		pdfDictionary["AvgWidth"] = new PdfNumber(metrics.WidthTable[32]);
		if (!m_isSkipFontEmbed)
		{
			if (m_ttfMetrics.ContainsCFF)
			{
				pdfDictionary["FontFile3"] = new PdfReferenceHolder(m_fontProgram);
			}
			else
			{
				pdfDictionary["FontFile2"] = new PdfReferenceHolder(m_fontProgram);
			}
		}
		pdfDictionary["MaxWidth"] = new PdfNumber(metrics.WidthTable[32]);
		pdfDictionary["XHeight"] = new PdfNumber(0);
		pdfDictionary["StemH"] = new PdfNumber(0);
		return pdfDictionary;
	}

	private string FormatName(string fontName)
	{
		if (fontName == null)
		{
			throw new ArgumentNullException("fontName");
		}
		if (fontName == string.Empty)
		{
			throw new ArgumentOutOfRangeException("fontName", "Parameter can not be empty");
		}
		return fontName.Replace("(", "#28").Replace(")", "#29").Replace("[", "#5B")
			.Replace("]", "#5D")
			.Replace("<", "#3C")
			.Replace(">", "#3E")
			.Replace("{", "#7B")
			.Replace("}", "#7D")
			.Replace("/", "#2F")
			.Replace("%", "#25")
			.Replace(" ", "#20");
	}

	private string GetFontName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		SecureRandomAlgorithm secureRandomAlgorithm = new SecureRandomAlgorithm();
		bool enableUniqueResourceNaming = PdfDocument.EnableUniqueResourceNaming;
		if (m_type == CompositeFontType.Type0)
		{
			if (!m_isEmbedFont)
			{
				if (enableUniqueResourceNaming)
				{
					for (int i = 0; i < 6; i++)
					{
						int index = secureRandomAlgorithm.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length);
						stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[index]);
					}
				}
				else
				{
					stringBuilder.Append("BCD");
					string value = "";
					int count = PdfDocument.Cache.FontCollection.Count;
					int num = 0;
					if (count == 0)
					{
						value = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[0].ToString() + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[0] + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[0];
					}
					else
					{
						for (char c = 'A'; c <= 'Z'; c = (char)(c + 1))
						{
							for (char c2 = 'A'; c2 <= 'Z'; c2 = (char)(c2 + 1))
							{
								for (char c3 = 'A'; c3 <= 'Z'; c3 = (char)(c3 + 1))
								{
									value = c.ToString() + c2 + c3;
									num++;
									if (num == count)
									{
										break;
									}
								}
								if (num == count)
								{
									break;
								}
							}
							if (num == count)
							{
								break;
							}
						}
					}
					stringBuilder.Append(value);
				}
				stringBuilder.Append('+');
			}
			stringBuilder.Append(m_ttfReader.Metrics.PostScriptName);
		}
		else
		{
			stringBuilder.Append(m_ttfReader.Metrics.PostScriptName);
		}
		if (SkipFontEmbed)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.Append(m_ttfReader.Metrics.PostScriptName);
		}
		string text = stringBuilder.ToString();
		if (text == string.Empty)
		{
			text = m_ttfReader.Metrics.FontFamily;
		}
		return FormatName(text);
	}

	public PdfArray GetDescendantWidth()
	{
		lock (s_syncLock)
		{
			PdfArray pdfArray = null;
			if ((m_usedChars != null && m_usedChars.Count > 0 && m_openTypeGlyphs == null) || ForceFullFontEmbed)
			{
				pdfArray = new PdfArray();
				if (glyphInfo == null)
				{
					glyphInfo = new List<TtfGlyphInfo>();
				}
				if (!m_isEmbedFont && !ForceFullFontEmbed)
				{
					foreach (KeyValuePair<char, char> usedChar in m_usedChars)
					{
						char key = usedChar.Key;
						TtfGlyphInfo glyph = m_ttfReader.GetGlyph(key);
						if (!glyph.Empty)
						{
							glyphInfo.Add(glyph);
						}
					}
					glyphInfo = GetGlyphInfo();
				}
				else if (!m_isIncreasedUsedChar || ForceFullFontEmbed)
				{
					glyphInfo = m_ttfReader.GetAllGlyphs();
				}
				else
				{
					glyphInfo = GetGlyphInfo();
				}
				glyphInfo.Sort();
				int value = 0;
				int num = 0;
				bool flag = false;
				PdfArray pdfArray2 = new PdfArray();
				if (!m_isEmbedFont && !ForceFullFontEmbed)
				{
					int i = 0;
					for (int count = glyphInfo.Count; i < count; i++)
					{
						TtfGlyphInfo ttfGlyphInfo = glyphInfo[i];
						if (!flag)
						{
							flag = true;
							value = ttfGlyphInfo.Index;
							num = ttfGlyphInfo.Index - 1;
						}
						if ((num + 1 != ttfGlyphInfo.Index || i + 1 == count) && count > 1)
						{
							pdfArray.Add(new PdfNumber(value));
							if (i != 0)
							{
								pdfArray.Add(pdfArray2);
							}
							value = ttfGlyphInfo.Index;
							pdfArray2 = new PdfArray();
						}
						pdfArray2.Add(new PdfNumber(ttfGlyphInfo.Width));
						if (i + 1 == count)
						{
							pdfArray.Add(new PdfNumber(value));
							pdfArray.Add(pdfArray2);
						}
						num = ttfGlyphInfo.Index;
					}
				}
				else
				{
					bool flag2 = false;
					if ((PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001) || PdfDocument.FontEmbeddingEnabled)
					{
						flag2 = true;
					}
					List<int> list = new List<int>();
					int j = 0;
					for (int count2 = glyphInfo.Count; j < count2; j++)
					{
						TtfGlyphInfo ttfGlyphInfo2 = glyphInfo[j];
						if (!flag)
						{
							flag = true;
							num = ttfGlyphInfo2.Index - 1;
						}
						value = ttfGlyphInfo2.Index;
						if ((num + 1 == ttfGlyphInfo2.Index || j + 1 == count2 || !list.Contains(ttfGlyphInfo2.Index)) && count2 > 1)
						{
							pdfArray2.Add(new PdfNumber(ttfGlyphInfo2.Width));
							pdfArray.Add(new PdfNumber(value));
							pdfArray.Add(pdfArray2);
							list.Add(value);
							pdfArray2 = new PdfArray();
							if (flag2 && pdfArray.Count >= 8190)
							{
								break;
							}
						}
						num = ttfGlyphInfo2.Index;
					}
					list.Clear();
					list = null;
				}
			}
			else
			{
				pdfArray = GetOpenTypeDecendantWidth();
			}
			return pdfArray;
		}
	}

	internal List<TtfGlyphInfo> GetGlyphInfo()
	{
		List<TtfGlyphInfo> list = new List<TtfGlyphInfo>();
		foreach (KeyValuePair<char, char> usedChar in m_usedChars)
		{
			char key = usedChar.Key;
			TtfGlyphInfo glyph = m_ttfReader.GetGlyph(key);
			if (!glyph.Empty)
			{
				list.Add(glyph);
			}
		}
		return list;
	}

	private PdfArray GetOpenTypeDecendantWidth()
	{
		UpdateOpenTypeGlyphs();
		PdfArray pdfArray = new PdfArray();
		if (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0)
		{
			int[] array = new int[m_openTypeGlyphs.Count];
			m_openTypeGlyphs.Keys.CopyTo(array, 0);
			Array.Sort(array);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				PdfArray pdfArray2 = new PdfArray();
				pdfArray2.Add(new PdfNumber(m_openTypeGlyphs[array[i]].Width));
				pdfArray.Add(new PdfNumber(array[i]));
				pdfArray.Add(pdfArray2);
				if ((PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A || PdfDocument.FontEmbeddingEnabled) && pdfArray.Count >= 8190)
				{
					break;
				}
			}
		}
		return pdfArray;
	}

	private void UpdateOpenTypeGlyphs()
	{
		if (m_openTypeGlyphs == null || m_usedChars == null || m_usedChars.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<char, char> usedChar in m_usedChars)
		{
			char key = usedChar.Key;
			TtfGlyphInfo glyph = m_ttfReader.GetGlyph(key);
			if (!glyph.Empty && !m_openTypeGlyphs.ContainsKey(glyph.Index))
			{
				OtfGlyphInfo value = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
				m_openTypeGlyphs[glyph.Index] = value;
			}
		}
	}

	private string ToHexString(int n)
	{
		string text = Convert.ToString(n, 16);
		return "<0000".Substring(0, 5 - text.Length) + text + ">";
	}

	private int GetDescriptorFlags()
	{
		int num = 0;
		TtfMetrics metrics = m_ttfReader.Metrics;
		if (metrics.IsFixedPitch)
		{
			num |= 1;
		}
		num = ((!metrics.IsSymbol) ? (num | 0x20) : (num | 4));
		if (metrics.IsItalic)
		{
			num |= 0x40;
		}
		if (metrics.IsBold)
		{
			num |= 0x40000;
		}
		return num;
	}

	private RectangleF GetBoundBox()
	{
		RECT fontBox = m_ttfReader.Metrics.FontBox;
		int num = Math.Abs(fontBox.right - fontBox.left);
		int num2 = Math.Abs(fontBox.top - fontBox.bottom);
		return new RectangleF(fontBox.left, fontBox.bottom, num, num2);
	}

	private void Initialize()
	{
		using BinaryReader reader = GetFontData();
		m_ttfReader = new TtfReader(reader);
		m_ttfMetrics = m_ttfReader.Metrics;
	}

	private BinaryReader GetFontData()
	{
		Stream stream = null;
		if (m_fontStream != null)
		{
			stream = m_fontStream;
			if (stream.CanRead)
			{
				stream.Position = 0L;
			}
		}
		else
		{
			try
			{
				stream = new FileStream(m_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch (Exception)
			{
				throw new Exception("Cannot open file: " + m_filePath + " for reading.");
			}
		}
		return new BinaryReader(stream, DocGen.Pdf.Graphics.TtfReader.Encoding);
	}

	private void FontDictionaryBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if ((m_usedChars != null && m_usedChars.Count > 0) || (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0) || (PdfDocument.FontEmbeddingEnabled && !m_fontDescriptor.ContainsKey("ToUnicode")) || ForceFullFontEmbed)
		{
			m_fontDictionary["ToUnicode"] = new PdfReferenceHolder(m_cmap);
		}
	}

	private void FontDescriptorBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (((m_usedChars != null && m_usedChars.Count > 0) || (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0)) && !m_fontDescriptor.ContainsKey("CIDSet") && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4E && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A4F && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3A && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A3U && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2A && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A2U)
		{
			m_fontDescriptor["CIDSet"] = new PdfReferenceHolder(m_CidStream);
		}
	}

	private void FontProgramBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (ars.Writer != null && ars.Writer is PdfWriter)
		{
			m_isCompress = (ars.Writer as PdfWriter).isCompress;
		}
		GenerateFontProgram();
	}

	private void CmapBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		GenerateCmap();
	}

	private void CidBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		GenerateCidSet();
	}

	private void DescendantFontBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if ((m_usedChars != null && m_usedChars.Count > 0) || (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0) || ForceFullFontEmbed)
		{
			PdfArray descendantWidth = GetDescendantWidth();
			if (descendantWidth != null)
			{
				m_descendantFont["W"] = descendantWidth;
			}
		}
	}

	private void GenerateCidSet()
	{
		byte[] array = new byte[8] { 128, 64, 32, 16, 8, 4, 2, 1 };
		if (m_usedChars != null && m_usedChars.Count > 0 && m_openTypeGlyphs == null)
		{
			_ = new byte[11];
			Dictionary<int, int> glyphChars = m_ttfReader.GetGlyphChars(m_usedChars);
			byte[] array2 = null;
			if (glyphChars.Count > 0)
			{
				int[] array3 = new int[glyphChars.Count];
				glyphChars.Keys.CopyTo(array3, 0);
				Array.Sort(array3);
				array2 = new byte[array3[^1] / 8 + 1];
				foreach (int num in array3)
				{
					array2[num / 8] |= array[num % 8];
				}
			}
			m_CidStream.Write(array2);
		}
		else
		{
			GenerateOpenTypeCidSet(array);
		}
	}

	private void GenerateOpenTypeCidSet(byte[] dummyBits)
	{
		UpdateOpenTypeGlyphs();
		if (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0)
		{
			_ = new byte[11];
			byte[] array = null;
			int[] array2 = new int[m_openTypeGlyphs.Count];
			m_openTypeGlyphs.Keys.CopyTo(array2, 0);
			Array.Sort(array2);
			array = new byte[array2[^1] / 8 + 1];
			foreach (int num in array2)
			{
				array[num / 8] |= dummyBits[num % 8];
			}
			m_CidStream.Write(array);
		}
	}
}
