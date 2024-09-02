using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Office;

internal class TtfReader
{
	internal static readonly Encoding Encoding;

	internal const int WidthMultiplier = 1000;

	private const int c_ttfVersion1 = 65536;

	private const int c_ttfVersion2 = 1330926671;

	private const int c_macTtfVersion = 1953658213;

	private const int c_fp = 16;

	private static readonly string[] s_tableNames;

	private static readonly string[] m_tableNames;

	private static readonly short[] s_entrySelectors;

	private BigEndianReader m_reader;

	private Dictionary<string, TtfTableInfo> m_tableDirectory;

	private TtfMetrics m_metrics;

	private int[] m_width;

	private Dictionary<int, TtfGlyphInfo> m_macintosh;

	private Dictionary<int, TtfGlyphInfo> m_microsoft;

	private Dictionary<int, TtfGlyphInfo> m_macintoshGlyphs;

	private Dictionary<int, TtfGlyphInfo> m_microsoftGlyphs;

	internal bool m_bIsLocaShort;

	private bool m_subset;

	private char m_surrogateHigh;

	private long m_lowestPosition;

	private int m_maxMacIndex;

	private bool m_isFontPresent;

	internal int m_missedGlyphs;

	private string metricsName = string.Empty;

	private bool m_isTtcFont;

	private bool m_isMacTTF;

	private string m_fontName;

	private bool m_isAzureCompatible;

	private Dictionary<int, TtfGlyphInfo> m_completeGlyph;

	internal int unitsPerEM;

	internal List<ScriptTags> supportedScriptTags = new List<ScriptTags>();

	private Dictionary<int, TtfGlyphInfo> m_unicodeUCS4Glyph;

	private GSUBTable m_gsub;

	private GDEFTable m_gdef;

	private GPOSTable m_gpos;

	internal bool isOpenTypeFont;

	internal bool m_AnsiEncode;

	internal int m_missedGlyphCount;

	private static HashSet<string> glyphFonts;

	public BinaryReader Reader
	{
		get
		{
			return m_reader.Reader;
		}
		set
		{
			m_reader.Reader = value;
		}
	}

	internal bool IsFontPresent => m_isFontPresent;

	internal string OriginalFontName { get; set; }

	public BigEndianReader InternalReader => m_reader;

	public TtfMetrics Metrics => m_metrics;

	private Dictionary<string, TtfTableInfo> TableDirectory
	{
		get
		{
			if (m_tableDirectory == null)
			{
				m_tableDirectory = new Dictionary<string, TtfTableInfo>();
			}
			return m_tableDirectory;
		}
	}

	private Dictionary<int, TtfGlyphInfo> Macintosh
	{
		get
		{
			if (m_macintosh == null)
			{
				m_macintosh = new Dictionary<int, TtfGlyphInfo>();
			}
			return m_macintosh;
		}
	}

	private Dictionary<int, TtfGlyphInfo> Microsoft
	{
		get
		{
			if (m_microsoft == null)
			{
				m_microsoft = new Dictionary<int, TtfGlyphInfo>();
			}
			return m_microsoft;
		}
	}

	private Dictionary<int, TtfGlyphInfo> MacintoshGlyphs
	{
		get
		{
			if (m_macintoshGlyphs == null)
			{
				m_macintoshGlyphs = new Dictionary<int, TtfGlyphInfo>();
			}
			return m_macintoshGlyphs;
		}
	}

	private Dictionary<int, TtfGlyphInfo> MicrosoftGlyphs
	{
		get
		{
			if (m_microsoftGlyphs == null)
			{
				m_microsoftGlyphs = new Dictionary<int, TtfGlyphInfo>();
			}
			return m_microsoftGlyphs;
		}
	}

	internal GSUBTable GSUB
	{
		get
		{
			if (m_gsub == null)
			{
				TtfTableInfo table = GetTable("GSUB");
				m_gsub = new GSUBTable(m_reader, table.Offset, GDEF, this);
			}
			return m_gsub;
		}
	}

	internal GDEFTable GDEF
	{
		get
		{
			if (m_gdef == null)
			{
				TtfTableInfo table = GetTable("GDEF");
				m_gdef = new GDEFTable(m_reader, table);
			}
			return m_gdef;
		}
	}

	internal GPOSTable GPOS
	{
		get
		{
			if (m_gpos == null)
			{
				TtfTableInfo table = GetTable("GPOS");
				m_gpos = new GPOSTable(m_reader, table.Offset, GDEF, this);
			}
			return m_gpos;
		}
	}

	internal Dictionary<int, TtfGlyphInfo> UnicodeUCS4Glyph
	{
		get
		{
			if (m_unicodeUCS4Glyph == null)
			{
				m_unicodeUCS4Glyph = new Dictionary<int, TtfGlyphInfo>();
			}
			return m_unicodeUCS4Glyph;
		}
		set
		{
			m_unicodeUCS4Glyph = value;
		}
	}

	internal Dictionary<int, TtfGlyphInfo> CompleteGlyph
	{
		get
		{
			if (m_completeGlyph == null)
			{
				m_completeGlyph = new Dictionary<int, TtfGlyphInfo>();
				if (UnicodeUCS4Glyph.Count == 0)
				{
					if (Microsoft != null && Microsoft.Count > 0)
					{
						UnicodeUCS4Glyph = Microsoft;
					}
					else if (Macintosh != null && Macintosh.Count > 0)
					{
						UnicodeUCS4Glyph = Macintosh;
					}
				}
				foreach (KeyValuePair<int, TtfGlyphInfo> item in UnicodeUCS4Glyph)
				{
					if (!m_completeGlyph.ContainsKey(item.Value.Index))
					{
						m_completeGlyph.Add(item.Value.Index, item.Value);
					}
				}
				for (int i = 0; i < m_width.Length; i++)
				{
					if (!m_completeGlyph.ContainsKey(i))
					{
						TtfGlyphInfo value = default(TtfGlyphInfo);
						value.Width = m_width[i];
						value.CharCode = -1;
						value.Index = i;
						m_completeGlyph.Add(i, value);
					}
				}
			}
			return m_completeGlyph;
		}
	}

	private string[] TableNames => s_tableNames;

	static TtfReader()
	{
		Encoding = Encoding.Unicode;
		glyphFonts = new HashSet<string>(new string[6] { "gautami", "latha", "shruti", "mangal", "tunga", "vrinda" }, StringComparer.OrdinalIgnoreCase);
		s_tableNames = new string[9];
		s_tableNames[0] = "cvt ";
		s_tableNames[1] = "fpgm";
		s_tableNames[2] = "glyf";
		s_tableNames[3] = "head";
		s_tableNames[4] = "hhea";
		s_tableNames[5] = "hmtx";
		s_tableNames[6] = "loca";
		s_tableNames[7] = "maxp";
		s_tableNames[8] = "prep";
		m_tableNames = new string[10];
		m_tableNames[0] = "cmap";
		m_tableNames[1] = "cvt ";
		m_tableNames[2] = "fpgm";
		m_tableNames[3] = "glyf";
		m_tableNames[4] = "head";
		m_tableNames[5] = "hhea";
		m_tableNames[6] = "hmtx";
		m_tableNames[7] = "loca";
		m_tableNames[8] = "maxp";
		m_tableNames[9] = "prep";
		s_entrySelectors = new short[21]
		{
			0, 0, 1, 1, 2, 2, 2, 2, 3, 3,
			3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
			4
		};
	}

	internal TtfReader(BinaryReader reader, string name)
	{
		metricsName = name;
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		m_reader = new BigEndianReader(reader);
		Initialize();
	}

	public TtfReader(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		m_reader = new BigEndianReader(reader);
		Initialize();
	}

	public void Close()
	{
		if (m_reader != null)
		{
			m_reader.Close();
			m_reader = null;
		}
		if (m_tableDirectory != null)
		{
			m_tableDirectory.Clear();
			m_tableDirectory = null;
		}
		if (m_macintosh != null)
		{
			m_macintosh.Clear();
			m_macintosh = null;
		}
		if (m_microsoft != null)
		{
			m_microsoft.Clear();
			m_microsoft = null;
		}
		if (m_macintoshGlyphs != null)
		{
			m_macintoshGlyphs.Clear();
			m_macintoshGlyphs = null;
		}
		if (m_microsoftGlyphs != null)
		{
			m_microsoftGlyphs.Clear();
			m_microsoftGlyphs = null;
		}
		m_width = null;
	}

	public TtfGlyphInfo GetGlyph(char charCode)
	{
		object obj = null;
		int num = charCode;
		if (!m_metrics.IsSymbol && m_microsoft != null)
		{
			string fontName = m_fontName;
			if (m_microsoft.ContainsKey(num))
			{
				obj = m_microsoft[num];
				if (num != 32)
				{
					m_isFontPresent = true;
				}
			}
			else if (num != 32)
			{
				if (fontName != null && fontName.Equals("segoe ui symbol", StringComparison.OrdinalIgnoreCase) && m_width.Length > num)
				{
					TtfGlyphInfo result = default(TtfGlyphInfo);
					result.CharCode = num;
					result.Index = num;
					result.Width = m_width[num];
					return result;
				}
				if (m_unicodeUCS4Glyph != null && char.IsSurrogate(charCode))
				{
					TtfGlyphInfo result2 = default(TtfGlyphInfo);
					if (char.IsHighSurrogate(charCode))
					{
						m_surrogateHigh = charCode;
						m_isFontPresent = true;
						result2.Width = 0;
					}
					if (char.IsLowSurrogate(charCode) && char.IsSurrogatePair(m_surrogateHigh, charCode))
					{
						num = char.ConvertToUtf32(m_surrogateHigh, charCode);
						if (m_unicodeUCS4Glyph.ContainsKey(num))
						{
							return m_unicodeUCS4Glyph[num];
						}
					}
					return result2;
				}
				m_isFontPresent = false;
				m_missedGlyphCount++;
			}
		}
		else if (m_macintosh != null && (m_metrics.IsSymbol || m_isMacTTF))
		{
			num = ((m_maxMacIndex == 0) ? (((num & 0xFF00) == 61440) ? (num & 0xFF) : num) : (num % (m_maxMacIndex + 1)));
			if (m_macintosh.ContainsKey(num))
			{
				obj = m_macintosh[num];
				m_isFontPresent = true;
			}
		}
		if (charCode == ' ' && obj == null)
		{
			obj = default(TtfGlyphInfo);
		}
		if (obj == null && isOTFFont() && UnicodeUCS4Glyph != null && UnicodeUCS4Glyph.ContainsKey(charCode))
		{
			obj = UnicodeUCS4Glyph[charCode];
		}
		if (obj == null)
		{
			return GetDefaultGlyph();
		}
		return (TtfGlyphInfo)obj;
	}

	public TtfGlyphInfo GetGlyph(int glyphIndex)
	{
		object obj = null;
		if (!m_metrics.IsSymbol && m_microsoftGlyphs != null)
		{
			if (m_microsoftGlyphs.ContainsKey(glyphIndex))
			{
				obj = m_microsoftGlyphs[glyphIndex];
			}
		}
		else if (m_metrics.IsSymbol && m_macintoshGlyphs != null && m_macintoshGlyphs.ContainsKey(glyphIndex))
		{
			obj = m_macintoshGlyphs[glyphIndex];
		}
		if (obj == null && m_unicodeUCS4Glyph != null && m_unicodeUCS4Glyph.ContainsKey(glyphIndex))
		{
			obj = m_unicodeUCS4Glyph[glyphIndex];
		}
		else if (obj == null && m_completeGlyph != null && m_completeGlyph.ContainsKey(glyphIndex))
		{
			obj = m_completeGlyph[glyphIndex];
		}
		if (obj == null)
		{
			return GetDefaultGlyph();
		}
		return (TtfGlyphInfo)obj;
	}

	internal bool IsFontContainsChar(int code)
	{
		bool result = false;
		if (!m_metrics.IsSymbol && m_microsoft != null)
		{
			if (m_microsoft.ContainsKey(code))
			{
				if (code != 32)
				{
					result = true;
				}
			}
			else if (code != 32)
			{
				result = false;
			}
		}
		else if (m_metrics.IsSymbol && m_macintosh != null)
		{
			code = ((m_maxMacIndex == 0) ? (((code & 0xFF00) == 61440) ? (code & 0xFF) : code) : (code % (m_maxMacIndex + 1)));
			if (m_macintosh.ContainsKey(code))
			{
				result = true;
			}
		}
		if (!m_metrics.IsSymbol && m_microsoftGlyphs != null)
		{
			if (m_microsoftGlyphs.ContainsKey(code))
			{
				result = true;
			}
		}
		else if (m_metrics.IsSymbol && m_macintoshGlyphs != null && m_macintoshGlyphs.ContainsKey(code))
		{
			result = true;
		}
		return result;
	}

	internal Stream ReadCffTable()
	{
		TtfTableInfo ttfTableInfo = TableDirectory["CFF "];
		byte[] buffer = new byte[ttfTableInfo.Length];
		m_reader.Seek(ttfTableInfo.Offset);
		m_reader.Read(buffer, 0, ttfTableInfo.Length);
		return new MemoryStream(buffer);
	}

	internal List<TtfGlyphInfo> GetAllGlyphs()
	{
		List<TtfGlyphInfo> list = new List<TtfGlyphInfo>();
		foreach (TtfGlyphInfo value in CompleteGlyph.Values)
		{
			list.Add(value);
		}
		return list;
	}

	public void CreateInternals()
	{
		ReadMetrics();
	}

	public void CreateInternals(string originalFontName)
	{
		ReadMetrics();
	}

	public byte[] ReadFontProgram(Dictionary<char, char> chars)
	{
		Dictionary<int, int> glyphChars = GetGlyphChars(chars);
		TtfLocaTable locaTable = ReadLocaTable(m_bIsLocaShort);
		if (glyphChars.Count < chars.Count)
		{
			m_missedGlyphs = chars.Count - glyphChars.Count;
		}
		UpdateGlyphChars(glyphChars, locaTable);
		int[] newLocaTable = null;
		byte[] newGlyphTable = null;
		byte[] newLocaTableOut = null;
		uint glyphTableSize = GenerateGlyphTable(glyphChars, locaTable, out newLocaTable, out newGlyphTable);
		int locaTableSize = UpdateLocaTable(newLocaTable, m_bIsLocaShort, out newLocaTableOut);
		return GetFontProgram(newLocaTableOut, newGlyphTable, glyphTableSize, (uint)locaTableSize);
	}

	internal byte[] ReadOpenTypeFontProgram(Dictionary<int, OtfGlyphInfo> otGlyphs)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, OtfGlyphInfo> otGlyph in otGlyphs)
		{
			if (otGlyph.Value.CharCode != -1)
			{
				dictionary[otGlyph.Key] = otGlyph.Value.CharCode;
			}
			else
			{
				dictionary[otGlyph.Key] = otGlyph.Value.Characters[0];
			}
		}
		TtfLocaTable locaTable = ReadLocaTable(m_bIsLocaShort);
		if (dictionary.Count < otGlyphs.Count)
		{
			m_missedGlyphs = otGlyphs.Count - dictionary.Count;
		}
		UpdateGlyphChars(dictionary, locaTable);
		int[] newLocaTable = null;
		byte[] newGlyphTable = null;
		byte[] newLocaTableOut = null;
		uint glyphTableSize = GenerateGlyphTable(dictionary, locaTable, out newLocaTable, out newGlyphTable);
		int locaTableSize = UpdateLocaTable(newLocaTable, m_bIsLocaShort, out newLocaTableOut);
		return GetFontProgram(newLocaTableOut, newGlyphTable, glyphTableSize, (uint)locaTableSize);
	}

	public string ConvertString(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		char[] array = new char[text.Length];
		int length = 0;
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			char charCode = text[i];
			TtfGlyphInfo glyph = GetGlyph(charCode);
			if (!glyph.Empty)
			{
				array[length++] = (char)glyph.Index;
			}
		}
		return new string(array, 0, length);
	}

	internal bool IsFontContainsString(string text)
	{
		bool result = false;
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		_ = new char[text.Length];
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			int code = text[i];
			if (IsFontContainsChar(code))
			{
				result = true;
				continue;
			}
			result = false;
			break;
		}
		return result;
	}

	public int GetCharWidth(char code)
	{
		TtfGlyphInfo glyph = GetGlyph(code);
		glyph = ((!glyph.Empty) ? glyph : GetDefaultGlyph());
		if (glyph.Empty)
		{
			return 0;
		}
		return glyph.Width;
	}

	internal Dictionary<int, int> GetGlyphChars(Dictionary<char, char> chars)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<char, char> @char in chars)
		{
			char key = @char.Key;
			TtfGlyphInfo glyph = GetGlyph(key);
			if (!glyph.Empty)
			{
				dictionary[glyph.Index] = key;
			}
		}
		return dictionary;
	}

	private void Initialize()
	{
		ReadFontDirectory();
		TtfNameTable nameTable = ReadNameTable();
		TtfHeadTable ttfHeadTable = ReadHeadTable();
		InitializeFontName(nameTable);
		m_metrics.MacStyle = ttfHeadTable.MacStyle;
		AddSupportedTags();
	}

	private void AddSupportedTags()
	{
		supportedScriptTags.Add(ScriptTags.Bengali);
		supportedScriptTags.Add(ScriptTags.Devanagari);
		supportedScriptTags.Add(ScriptTags.Gujarati);
		supportedScriptTags.Add(ScriptTags.Gurmukhi);
		supportedScriptTags.Add(ScriptTags.Kannada);
		supportedScriptTags.Add(ScriptTags.Khmer);
		supportedScriptTags.Add(ScriptTags.Malayalam);
		supportedScriptTags.Add(ScriptTags.Oriya);
		supportedScriptTags.Add(ScriptTags.Tamil);
		supportedScriptTags.Add(ScriptTags.Telugu);
		supportedScriptTags.Add(ScriptTags.Thai);
	}

	private void ReadFontDirectory()
	{
		m_reader.Seek(0L);
		CheckPreambula();
		int num = m_reader.ReadInt16();
		bool flag = false;
		m_reader.Skip(6L);
		for (int i = 0; i < num; i++)
		{
			TtfTableInfo value = default(TtfTableInfo);
			string key = m_reader.ReadString(4);
			value.Checksum = m_reader.ReadInt32();
			value.Offset = m_reader.ReadInt32();
			value.Length = m_reader.ReadInt32();
			TableDirectory[key] = value;
		}
		if (!flag)
		{
			m_lowestPosition = m_reader.BaseStream.Position;
			if (!m_isTtcFont)
			{
				FixOffsets();
			}
		}
	}

	private void FixOffsets()
	{
		int num = int.MaxValue;
		foreach (KeyValuePair<string, TtfTableInfo> item in TableDirectory)
		{
			int offset = ((TtfTableInfo)(object)item.Value).Offset;
			if (num > offset)
			{
				num = offset;
				if (num <= m_lowestPosition)
				{
					break;
				}
			}
		}
		int num2 = num - (int)m_lowestPosition;
		if (num2 == 0)
		{
			return;
		}
		Dictionary<string, TtfTableInfo> dictionary = new Dictionary<string, TtfTableInfo>();
		foreach (KeyValuePair<string, TtfTableInfo> item2 in TableDirectory)
		{
			TtfTableInfo value = TableDirectory[item2.Key];
			value.Offset -= num2;
			dictionary[item2.Key] = value;
		}
		m_tableDirectory = dictionary;
	}

	private void ReadMetrics()
	{
		m_metrics = default(TtfMetrics);
		TtfNameTable nameTable = ReadNameTable();
		TtfHeadTable headTable = ReadHeadTable();
		m_bIsLocaShort = headTable.IndexToLocFormat == 0;
		TtfHorizontalHeaderTable horizontalHeadTable = ReadHorizontalHeaderTable();
		TtfOS2Table os2Table = ReadOS2Table();
		TtfPostTable postTable = ReadPostTable();
		m_width = ReadWidthTable(horizontalHeadTable.NumberOfHMetrics, headTable.UnitsPerEm);
		TtfCmapSubTable[] cmapTables = ReadCmapTable();
		InitializeMetrics(nameTable, headTable, horizontalHeadTable, os2Table, postTable, cmapTables);
	}

	internal bool isOTFFont()
	{
		if (GetTable("GDEF").Empty || GetTable("GSUB").Empty || GetTable("GPOS").Empty)
		{
			return false;
		}
		return true;
	}

	private void InitializeMetrics(TtfNameTable nameTable, TtfHeadTable headTable, TtfHorizontalHeaderTable horizontalHeadTable, TtfOS2Table os2Table, TtfPostTable postTable, TtfCmapSubTable[] cmapTables)
	{
		if (cmapTables == null)
		{
			throw new ArgumentNullException("cmapTables");
		}
		InitializeFontName(nameTable);
		bool isSymbol = false;
		for (int i = 0; i < cmapTables.Length; i++)
		{
			TtfCmapSubTable ttfCmapSubTable = cmapTables[i];
			if (GetCmapEncoding(ttfCmapSubTable.PlatformID, ttfCmapSubTable.EncodingID) == TtfCmapEncoding.Symbol)
			{
				isSymbol = true;
				break;
			}
		}
		m_metrics.IsSymbol = isSymbol;
		m_metrics.MacStyle = headTable.MacStyle;
		m_metrics.IsFixedPitch = postTable.IsFixedPitch != 0;
		m_metrics.ItalicAngle = postTable.ItalicAngle;
		float num = 1000f / (float)(int)headTable.UnitsPerEm;
		m_metrics.WinAscent = (int)os2Table.UsWinAscent;
		m_metrics.MacAscent = horizontalHeadTable.Ascender;
		m_metrics.UnitsPerEm = (int)headTable.UnitsPerEm;
		m_metrics.CapHeight = ((os2Table.SCapHeight != 0) ? ((float)os2Table.SCapHeight) : (0.7f * (float)(int)headTable.UnitsPerEm * num));
		m_metrics.WinDescent = (int)os2Table.UsWinDescent;
		m_metrics.MacDescent = horizontalHeadTable.Descender;
		m_metrics.Leading = os2Table.STypoAscender - os2Table.STypoDescender + os2Table.STypoLineGap;
		m_metrics.LineGap = (int)Math.Ceiling((decimal)horizontalHeadTable.LineGap);
		if ((m_metrics.FontFamily == "Cambria" || m_metrics.FontFamily == "Cambria Math") && OriginalFontName == "Cambria Math")
		{
			m_metrics.WinAscent = Math.Abs((int)os2Table.STypoAscender);
			m_metrics.WinDescent = Math.Abs((int)os2Table.STypoDescender);
		}
		int x = (int)((float)headTable.XMin * num);
		int y = (int)Math.Ceiling(m_metrics.MacAscent + (float)m_metrics.LineGap);
		int x2 = (int)((float)headTable.XMax * num);
		int y2 = (int)m_metrics.MacDescent;
		m_metrics.FontBox = new RECT(x, y, x2, y2);
		m_metrics.StemV = 80f;
		m_metrics.WidthTable = UpdateWidth();
		m_metrics.ContainsCFF = TableDirectory.ContainsKey("CFF ");
		m_metrics.SubScriptSizeFactor = (float)(int)headTable.UnitsPerEm / (float)os2Table.YSubscriptYSize;
		m_metrics.SuperscriptSizeFactor = (float)(int)headTable.UnitsPerEm / (float)os2Table.YSuperscriptYSize;
	}

	private bool CheckTypoValues(TtfOS2Table oS2Table, TtfHorizontalHeaderTable ttfHorizontal)
	{
		if (Math.Abs(oS2Table.STypoAscender) == Math.Abs(oS2Table.UsWinAscent) && Math.Abs(oS2Table.STypoDescender) == Math.Abs(oS2Table.UsWinDescent) && Math.Abs(ttfHorizontal.LineGap) == Math.Abs(oS2Table.STypoLineGap))
		{
			return true;
		}
		return false;
	}

	private TtfNameTable ReadNameTable()
	{
		TtfTableInfo table = GetTable("name");
		m_reader.Seek(table.Offset);
		TtfNameTable result = default(TtfNameTable);
		result.FormatSelector = m_reader.ReadUInt16();
		result.RecordsCount = m_reader.ReadUInt16();
		result.Offset = m_reader.ReadUInt16();
		result.NameRecords = new TtfNameRecord[result.RecordsCount];
		long num = m_reader.BaseStream.Position;
		int num2 = 12;
		int i = 0;
		for (int recordsCount = result.RecordsCount; i < recordsCount; i++)
		{
			m_reader.Seek(num);
			TtfNameRecord ttfNameRecord = default(TtfNameRecord);
			ttfNameRecord.PlatformID = m_reader.ReadUInt16();
			ttfNameRecord.EncodingID = m_reader.ReadUInt16();
			ttfNameRecord.LanguageID = m_reader.ReadUInt16();
			ttfNameRecord.NameID = m_reader.ReadUInt16();
			ttfNameRecord.Length = m_reader.ReadUInt16();
			ttfNameRecord.Offset = m_reader.ReadUInt16();
			long position = table.Offset + result.Offset + ttfNameRecord.Offset;
			m_reader.Seek(position);
			bool unicode = ttfNameRecord.PlatformID == 0 || ttfNameRecord.PlatformID == 3;
			ttfNameRecord.Name = m_reader.ReadString(ttfNameRecord.Length, unicode);
			result.NameRecords[i] = ttfNameRecord;
			num += num2;
		}
		return result;
	}

	private TtfHeadTable ReadHeadTable()
	{
		TtfTableInfo table = GetTable("head");
		m_reader.Seek(table.Offset);
		TtfHeadTable result = default(TtfHeadTable);
		result.Version = m_reader.ReadFixed();
		result.FontRevision = m_reader.ReadFixed();
		result.CheckSumAdjustment = m_reader.ReadUInt32();
		result.MagicNumber = m_reader.ReadUInt32();
		result.Flags = m_reader.ReadUInt16();
		unitsPerEM = (result.UnitsPerEm = m_reader.ReadUInt16());
		result.Created = m_reader.ReadInt64();
		result.Modified = m_reader.ReadInt64();
		result.XMin = m_reader.ReadInt16();
		result.YMin = m_reader.ReadInt16();
		result.XMax = m_reader.ReadInt16();
		result.YMax = m_reader.ReadInt16();
		result.MacStyle = m_reader.ReadUInt16();
		result.LowestRecPPEM = m_reader.ReadUInt16();
		result.FontDirectionHint = m_reader.ReadInt16();
		result.IndexToLocFormat = m_reader.ReadInt16();
		result.GlyphDataFormat = m_reader.ReadInt16();
		return result;
	}

	private TtfHorizontalHeaderTable ReadHorizontalHeaderTable()
	{
		TtfTableInfo table = GetTable("hhea");
		m_reader.Seek(table.Offset);
		TtfHorizontalHeaderTable result = default(TtfHorizontalHeaderTable);
		result.Version = m_reader.ReadFixed();
		result.Ascender = m_reader.ReadInt16();
		result.Descender = m_reader.ReadInt16();
		result.LineGap = m_reader.ReadInt16();
		result.AdvanceWidthMax = m_reader.ReadUInt16();
		result.MinLeftSideBearing = m_reader.ReadInt16();
		result.MinRightSideBearing = m_reader.ReadInt16();
		result.XMaxExtent = m_reader.ReadInt16();
		result.CaretSlopeRise = m_reader.ReadInt16();
		result.CaretSlopeRun = m_reader.ReadInt16();
		m_reader.Skip(10L);
		result.MetricDataFormat = m_reader.ReadInt16();
		result.NumberOfHMetrics = m_reader.ReadUInt16();
		return result;
	}

	internal TtfGlyphInfo ReadGlyph(int index, bool isOpenType)
	{
		TtfGlyphInfo value;
		if (isOpenType)
		{
			CompleteGlyph.TryGetValue(index, out value);
		}
		else
		{
			_ = CompleteGlyph.Count;
			UnicodeUCS4Glyph.TryGetValue(index, out value);
		}
		return value;
	}

	private TtfOS2Table ReadOS2Table()
	{
		TtfTableInfo table = GetTable("OS/2");
		m_reader.Seek(table.Offset);
		TtfOS2Table result = default(TtfOS2Table);
		result.Version = m_reader.ReadUInt16();
		result.XAvgCharWidth = m_reader.ReadInt16();
		result.UsWeightClass = m_reader.ReadUInt16();
		result.UsWidthClass = m_reader.ReadUInt16();
		result.FsType = m_reader.ReadInt16();
		result.YSubscriptXSize = m_reader.ReadInt16();
		result.YSubscriptYSize = m_reader.ReadInt16();
		result.YSubscriptXOffset = m_reader.ReadInt16();
		result.YSubscriptYOffset = m_reader.ReadInt16();
		result.ySuperscriptXSize = m_reader.ReadInt16();
		result.YSuperscriptYSize = m_reader.ReadInt16();
		result.YSuperscriptXOffset = m_reader.ReadInt16();
		result.YSuperscriptYOffset = m_reader.ReadInt16();
		result.YStrikeoutSize = m_reader.ReadInt16();
		result.YStrikeoutPosition = m_reader.ReadInt16();
		result.SFamilyClass = m_reader.ReadInt16();
		result.Panose = m_reader.ReadBytes(10);
		result.UlUnicodeRange1 = m_reader.ReadUInt32();
		result.UlUnicodeRange2 = m_reader.ReadUInt32();
		result.UlUnicodeRange3 = m_reader.ReadUInt32();
		result.UlUnicodeRange4 = m_reader.ReadUInt32();
		result.AchVendID = m_reader.ReadBytes(4);
		result.FsSelection = m_reader.ReadUInt16();
		result.UsFirstCharIndex = m_reader.ReadUInt16();
		result.UsLastCharIndex = m_reader.ReadUInt16();
		result.STypoAscender = m_reader.ReadInt16();
		result.STypoDescender = m_reader.ReadInt16();
		result.STypoLineGap = m_reader.ReadInt16();
		result.UsWinAscent = m_reader.ReadUInt16();
		result.UsWinDescent = m_reader.ReadUInt16();
		result.UlCodePageRange1 = m_reader.ReadUInt32();
		result.UlCodePageRange2 = m_reader.ReadUInt32();
		if (result.Version > 1)
		{
			result.SxHeight = m_reader.ReadInt16();
			result.SCapHeight = m_reader.ReadInt16();
			result.UsDefaultChar = m_reader.ReadUInt16();
			result.UsBreakChar = m_reader.ReadUInt16();
			result.UsMaxContext = m_reader.ReadUInt16();
		}
		return result;
	}

	private TtfPostTable ReadPostTable()
	{
		TtfTableInfo table = GetTable("post");
		m_reader.Seek(table.Offset);
		TtfPostTable result = default(TtfPostTable);
		result.FormatType = m_reader.ReadFixed();
		result.ItalicAngle = m_reader.ReadFixed();
		result.UnderlinePosition = m_reader.ReadInt16();
		result.UnderlineThickness = m_reader.ReadInt16();
		result.IsFixedPitch = m_reader.ReadUInt32();
		result.MinMemType42 = m_reader.ReadUInt32();
		result.MaxMemType42 = m_reader.ReadUInt32();
		result.MinMemType1 = m_reader.ReadUInt32();
		result.MaxMemType1 = m_reader.ReadUInt32();
		return result;
	}

	private int[] ReadWidthTable(int glyphCount, int unitsPerEm)
	{
		TtfTableInfo table = GetTable("hmtx");
		m_reader.Seek(table.Offset);
		int[] array = new int[glyphCount];
		for (int i = 0; i < glyphCount; i++)
		{
			TtfLongHorMertric ttfLongHorMertric = default(TtfLongHorMertric);
			ttfLongHorMertric.AdvanceWidth = m_reader.ReadUInt16();
			ttfLongHorMertric.Lsb = m_reader.ReadInt16();
			int advanceWidth = ttfLongHorMertric.AdvanceWidth;
			array[i] = advanceWidth;
		}
		return array;
	}

	private TtfCmapSubTable[] ReadCmapTable()
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset);
		TtfCmapTable ttfCmapTable = default(TtfCmapTable);
		ttfCmapTable.Version = m_reader.ReadUInt16();
		ttfCmapTable.TablesCount = m_reader.ReadUInt16();
		long position = m_reader.BaseStream.Position;
		TtfCmapSubTable[] array = new TtfCmapSubTable[ttfCmapTable.TablesCount];
		for (int i = 0; i < ttfCmapTable.TablesCount; i++)
		{
			m_reader.Seek(position);
			TtfCmapSubTable ttfCmapSubTable = default(TtfCmapSubTable);
			ttfCmapSubTable.PlatformID = m_reader.ReadUInt16();
			ttfCmapSubTable.EncodingID = m_reader.ReadUInt16();
			ttfCmapSubTable.Offset = m_reader.ReadUInt32();
			position = m_reader.BaseStream.Position;
			ReadCmapSubTable(ttfCmapSubTable);
			array[i] = ttfCmapSubTable;
		}
		return array;
	}

	private void ReadCmapSubTable(TtfCmapSubTable subTable)
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset + subTable.Offset);
		TtfCmapFormat ttfCmapFormat = (TtfCmapFormat)m_reader.ReadUInt16();
		TtfCmapEncoding cmapEncoding = GetCmapEncoding(subTable.PlatformID, subTable.EncodingID);
		_ = 3;
		if (cmapEncoding != 0)
		{
			switch (ttfCmapFormat)
			{
			case TtfCmapFormat.Apple:
				ReadAppleCmapTable(subTable, cmapEncoding);
				break;
			case TtfCmapFormat.Microsoft:
				ReadMicrosoftCmapTable(subTable, cmapEncoding);
				break;
			case TtfCmapFormat.Trimmed:
				ReadTrimmedCmapTable(subTable, cmapEncoding);
				break;
			case TtfCmapFormat.MicrosoftExt:
				ReadUCS4CmapTable(subTable, cmapEncoding);
				break;
			}
		}
	}

	private void ReadUCS4CmapTable(TtfCmapSubTable subTable, TtfCmapEncoding encoding)
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset + subTable.Offset);
		m_reader.ReadUInt16();
		m_reader.Skip(2L);
		m_reader.ReadInt32();
		m_reader.Skip(4L);
		int num = m_reader.ReadInt32();
		if (m_reader.BaseStream.Position >= m_reader.BaseStream.Length)
		{
			return;
		}
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			int num3 = m_reader.ReadInt32();
			int num4 = m_reader.ReadInt32();
			int num5 = m_reader.ReadInt32();
			for (int j = num3; j <= num4; j++)
			{
				TtfGlyphInfo glyph = default(TtfGlyphInfo);
				glyph.CharCode = j;
				glyph.Width = GetWidth(num5);
				glyph.Index = num5;
				AddGlyph(glyph, encoding, reverse: true);
				num5++;
			}
		}
	}

	private void ReadAppleCmapTable(TtfCmapSubTable subTable, TtfCmapEncoding encoding)
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset + subTable.Offset);
		TtfAppleCmapSubTable ttfAppleCmapSubTable = default(TtfAppleCmapSubTable);
		ttfAppleCmapSubTable.Format = m_reader.ReadUInt16();
		ttfAppleCmapSubTable.Length = m_reader.ReadUInt16();
		ttfAppleCmapSubTable.Version = m_reader.ReadUInt16();
		if (m_reader.BaseStream.Position < m_reader.BaseStream.Length)
		{
			int i = 0;
			for (int num = 256; i < num; i++)
			{
				TtfGlyphInfo ttfGlyphInfo = default(TtfGlyphInfo);
				ttfGlyphInfo.Index = m_reader.ReadByte();
				ttfGlyphInfo.Width = GetWidth(ttfGlyphInfo.Index);
				ttfGlyphInfo.CharCode = i;
				Macintosh[i] = ttfGlyphInfo;
				AddGlyph(ttfGlyphInfo, encoding);
				m_maxMacIndex = Math.Max(i, m_maxMacIndex);
			}
		}
	}

	private void ReadMicrosoftCmapTable(TtfCmapSubTable subTable, TtfCmapEncoding encoding)
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset + subTable.Offset);
		Dictionary<int, TtfGlyphInfo> dictionary = ((encoding == TtfCmapEncoding.Unicode) ? Microsoft : Macintosh);
		TtfMicrosoftCmapSubTable ttfMicrosoftCmapSubTable = default(TtfMicrosoftCmapSubTable);
		ttfMicrosoftCmapSubTable.Format = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.Length = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.Version = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.SegCountX2 = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.SearchRange = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.EntrySelector = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.RangeShift = m_reader.ReadUInt16();
		int num = ttfMicrosoftCmapSubTable.SegCountX2 / 2;
		ttfMicrosoftCmapSubTable.EndCount = ReadUshortArray(num);
		ttfMicrosoftCmapSubTable.ReservedPad = m_reader.ReadUInt16();
		ttfMicrosoftCmapSubTable.StartCount = ReadUshortArray(num);
		ttfMicrosoftCmapSubTable.IdDelta = ReadUshortArray(num);
		ttfMicrosoftCmapSubTable.IdRangeOffset = ReadUshortArray(num);
		int len = ttfMicrosoftCmapSubTable.Length / 2 - 8 - num * 4;
		ttfMicrosoftCmapSubTable.GlyphID = ReadUshortArray(len);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			int j = ttfMicrosoftCmapSubTable.StartCount[i];
			for (int num4 = ttfMicrosoftCmapSubTable.EndCount[i]; j <= num4 && j != 65535; j++)
			{
				if (ttfMicrosoftCmapSubTable.IdRangeOffset[i] == 0)
				{
					num2 = (j + ttfMicrosoftCmapSubTable.IdDelta[i]) & 0xFFFF;
				}
				else
				{
					num3 = i + ttfMicrosoftCmapSubTable.IdRangeOffset[i] / 2 - num + j - ttfMicrosoftCmapSubTable.StartCount[i];
					if (num3 >= ttfMicrosoftCmapSubTable.GlyphID.Length)
					{
						continue;
					}
					num2 = (ttfMicrosoftCmapSubTable.GlyphID[num3] + ttfMicrosoftCmapSubTable.IdDelta[i]) & 0xFFFF;
				}
				TtfGlyphInfo ttfGlyphInfo = default(TtfGlyphInfo);
				ttfGlyphInfo.Index = num2;
				ttfGlyphInfo.Width = GetWidth(ttfGlyphInfo.Index);
				dictionary[ttfGlyphInfo.CharCode = ((encoding != TtfCmapEncoding.Symbol) ? j : (((j & 0xFF00) == 61440) ? (j & 0xFF) : j))] = ttfGlyphInfo;
				AddGlyph(ttfGlyphInfo, encoding);
			}
		}
	}

	private void ReadTrimmedCmapTable(TtfCmapSubTable subTable, TtfCmapEncoding encoding)
	{
		TtfTableInfo table = GetTable("cmap");
		m_reader.Seek(table.Offset + subTable.Offset);
		TtfTrimmedCmapSubTable ttfTrimmedCmapSubTable = default(TtfTrimmedCmapSubTable);
		ttfTrimmedCmapSubTable.Format = m_reader.ReadUInt16();
		ttfTrimmedCmapSubTable.Length = m_reader.ReadUInt16();
		ttfTrimmedCmapSubTable.Version = m_reader.ReadUInt16();
		ttfTrimmedCmapSubTable.FirstCode = m_reader.ReadUInt16();
		ttfTrimmedCmapSubTable.EntryCount = m_reader.ReadUInt16();
		if (m_reader.BaseStream.Position < m_reader.BaseStream.Length)
		{
			int i = 0;
			for (int entryCount = ttfTrimmedCmapSubTable.EntryCount; i < entryCount; i++)
			{
				TtfGlyphInfo ttfGlyphInfo = default(TtfGlyphInfo);
				ttfGlyphInfo.Index = m_reader.ReadUInt16();
				ttfGlyphInfo.Width = GetWidth(ttfGlyphInfo.Index);
				ttfGlyphInfo.CharCode = i + ttfTrimmedCmapSubTable.FirstCode;
				Macintosh[i] = ttfGlyphInfo;
				AddGlyph(ttfGlyphInfo, encoding);
				m_maxMacIndex = Math.Max(i, m_maxMacIndex);
			}
		}
	}

	private TtfLocaTable ReadLocaTable(bool bShort)
	{
		TtfTableInfo table = GetTable("loca");
		m_reader.Seek(table.Offset);
		TtfLocaTable result = default(TtfLocaTable);
		uint[] array = null;
		if (bShort)
		{
			int num = table.Length / 2;
			array = new uint[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (uint)(m_reader.ReadUInt16() * 2);
			}
		}
		else
		{
			int num2 = table.Length / 4;
			array = new uint[num2];
			for (int j = 0; j < num2; j++)
			{
				array[j] = m_reader.ReadUInt32();
			}
		}
		result.Offsets = array;
		return result;
	}

	private ushort[] ReadUshortArray(int len)
	{
		ushort[] array = new ushort[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = m_reader.ReadUInt16();
		}
		return array;
	}

	private uint[] ReadUintArray(int len)
	{
		uint[] array = new uint[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = m_reader.ReadUInt32();
		}
		return array;
	}

	private void AddGlyph(TtfGlyphInfo glyph, TtfCmapEncoding encoding)
	{
		Dictionary<int, TtfGlyphInfo> dictionary = null;
		switch (encoding)
		{
		case TtfCmapEncoding.Unicode:
			dictionary = MicrosoftGlyphs;
			break;
		case TtfCmapEncoding.Symbol:
		case TtfCmapEncoding.Macintosh:
			dictionary = MacintoshGlyphs;
			break;
		case TtfCmapEncoding.UnicodeUCS4:
			dictionary = UnicodeUCS4Glyph;
			break;
		}
		dictionary[glyph.Index] = glyph;
	}

	private void AddGlyph(TtfGlyphInfo glyph, TtfCmapEncoding encoding, bool reverse)
	{
		Dictionary<int, TtfGlyphInfo> dictionary = null;
		switch (encoding)
		{
		case TtfCmapEncoding.Unicode:
			dictionary = MicrosoftGlyphs;
			break;
		case TtfCmapEncoding.Symbol:
		case TtfCmapEncoding.Macintosh:
			dictionary = MacintoshGlyphs;
			break;
		case TtfCmapEncoding.UnicodeUCS4:
			dictionary = UnicodeUCS4Glyph;
			break;
		}
		dictionary[glyph.CharCode] = glyph;
	}

	private int GetWidth(int glyphCode)
	{
		glyphCode = ((glyphCode < m_width.Length) ? glyphCode : (m_width.Length - 1));
		return m_width[glyphCode];
	}

	private int[] UpdateWidth()
	{
		int num = 256;
		int[] array = new int[num];
		if (m_metrics.IsSymbol)
		{
			for (int i = 0; i < num; i++)
			{
				TtfGlyphInfo glyph = GetGlyph((char)i);
				array[i] = ((!glyph.Empty) ? glyph.Width : 0);
			}
		}
		else
		{
			byte[] array2 = new byte[1];
			char c = '?';
			char charCode = ' ';
			for (int j = 0; j < num; j++)
			{
				array2[0] = (byte)j;
				string empty = string.Empty;
				empty = ((!m_AnsiEncode) ? Encoding.GetString(array2, 0, array2.Length) : new Windows1252Encoding().GetString(array2, 0, array2.Length));
				char charCode2 = ((empty.Length > 0) ? empty[0] : c);
				TtfGlyphInfo glyph2 = GetGlyph(charCode2);
				if (!glyph2.Empty)
				{
					array[j] = glyph2.Width;
					continue;
				}
				glyph2 = GetGlyph(charCode);
				array[j] = ((!glyph2.Empty) ? glyph2.Width : 0);
			}
		}
		return array;
	}

	private void CheckPreambula()
	{
		int num = m_reader.ReadInt32();
		if (num == 1953658213)
		{
			m_isMacTTF = true;
		}
		if (num != 65536 && num != 1330926671 && num != 1953658213)
		{
			m_isTtcFont = true;
			m_reader.Seek(0L);
			if (m_reader.ReadString(4) != "ttcf")
			{
				throw new Exception("Can't read TTF font data");
			}
			m_reader.Skip(4L);
			if (m_reader.ReadInt32() < 0)
			{
				throw new Exception("Can't read TTF font data");
			}
			m_reader.Seek(m_reader.ReadInt32());
			num = m_reader.ReadInt32();
		}
		if (num == 1330926671)
		{
			isOpenTypeFont = true;
		}
	}

	private TtfCmapEncoding GetCmapEncoding(int platformID, int encodingID)
	{
		TtfCmapEncoding result = TtfCmapEncoding.Unknown;
		if (platformID == 3 && encodingID == 0)
		{
			result = TtfCmapEncoding.Symbol;
		}
		else if (platformID == 3 && encodingID == 1)
		{
			result = TtfCmapEncoding.Unicode;
		}
		else if (platformID == 1 && encodingID == 0)
		{
			result = TtfCmapEncoding.Macintosh;
		}
		else if (platformID == 3 && encodingID == 10)
		{
			result = TtfCmapEncoding.UnicodeUCS4;
		}
		return result;
	}

	private TtfTableInfo GetTable(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		object obj = null;
		TtfTableInfo result = default(TtfTableInfo);
		if (TableDirectory.ContainsKey(name))
		{
			obj = TableDirectory[name];
		}
		if (obj != null)
		{
			return (TtfTableInfo)obj;
		}
		return result;
	}

	private void UpdateGlyphChars(Dictionary<int, int> glyphChars, TtfLocaTable locaTable)
	{
		if (glyphChars == null)
		{
			throw new ArgumentNullException("glyphChars");
		}
		if (!glyphChars.ContainsKey(0))
		{
			glyphChars.Add(0, 0);
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(glyphChars.Count);
		foreach (KeyValuePair<int, int> glyphChar in glyphChars)
		{
			dictionary.Add(glyphChar.Key, glyphChar.Value);
		}
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			int key = item.Key;
			ProcessCompositeGlyph(glyphChars, key, locaTable);
		}
	}

	private void ProcessCompositeGlyph(Dictionary<int, int> glyphChars, int glyph, TtfLocaTable locaTable)
	{
		if (glyphChars == null)
		{
			throw new ArgumentNullException("glyphChars");
		}
		if (glyph >= locaTable.Offsets.Length - 1)
		{
			return;
		}
		uint num = locaTable.Offsets[glyph];
		if (num == locaTable.Offsets[glyph + 1])
		{
			return;
		}
		TtfTableInfo table = GetTable("glyf");
		m_reader.Seek(table.Offset + num);
		TtfGlyphHeader ttfGlyphHeader = default(TtfGlyphHeader);
		ttfGlyphHeader.numberOfContours = m_reader.ReadInt16();
		ttfGlyphHeader.XMin = m_reader.ReadInt16();
		ttfGlyphHeader.YMin = m_reader.ReadInt16();
		ttfGlyphHeader.XMax = m_reader.ReadInt16();
		ttfGlyphHeader.YMax = m_reader.ReadInt16();
		if (ttfGlyphHeader.numberOfContours >= 0)
		{
			return;
		}
		int num2 = 0;
		while (true)
		{
			ushort num3 = m_reader.ReadUInt16();
			int key = m_reader.ReadUInt16();
			if (!glyphChars.ContainsKey(key))
			{
				glyphChars.Add(key, 0);
			}
			if ((num3 & 0x20u) != 0)
			{
				num2 = ((((uint)num3 & (true ? 1u : 0u)) != 0) ? 4 : 2);
				if ((num3 & 8u) != 0)
				{
					num2 += 2;
				}
				else if ((num3 & 0x40u) != 0)
				{
					num2 += 4;
				}
				else if ((num3 & 0x80u) != 0)
				{
					num2 += 8;
				}
				m_reader.Skip(num2);
				continue;
			}
			break;
		}
	}

	private uint GenerateGlyphTable(Dictionary<int, int> glyphChars, TtfLocaTable locaTable, out int[] newLocaTable, out byte[] newGlyphTable)
	{
		if (glyphChars == null)
		{
			throw new ArgumentNullException("glyphChars");
		}
		newLocaTable = new int[locaTable.Offsets.Length];
		int[] array = new List<int>(glyphChars.Keys).ToArray();
		Array.Sort(array);
		uint num = 0u;
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			int num3 = array[i];
			if (locaTable.Offsets.Length != 0)
			{
				num += locaTable.Offsets[num3 + 1] - locaTable.Offsets[num3];
			}
		}
		uint num4 = Align(num);
		newGlyphTable = new byte[num4];
		int num5 = 0;
		int num6 = 0;
		TtfTableInfo table = GetTable("glyf");
		int j = 0;
		for (int num7 = newLocaTable.Length; j < num7; j++)
		{
			newLocaTable[j] = num5;
			if (num6 < array.Length && array[num6] == j)
			{
				num6++;
				newLocaTable[j] = num5;
				int num8 = (int)locaTable.Offsets[j];
				int num9 = (int)locaTable.Offsets[j + 1] - num8;
				if (num9 > 0)
				{
					m_reader.Seek(table.Offset + num8);
					m_reader.Read(newGlyphTable, num5, num9);
					num5 += num9;
				}
			}
		}
		return num;
	}

	private int UpdateLocaTable(int[] newLocaTable, bool bLocaIsShort, out byte[] newLocaTableOut)
	{
		if (newLocaTable == null)
		{
			throw new ArgumentNullException("newLocaTable");
		}
		int num = (bLocaIsShort ? (newLocaTable.Length * 2) : (newLocaTable.Length * 4));
		BigEndianWriter bigEndianWriter = new BigEndianWriter((int)Align((uint)num));
		newLocaTableOut = bigEndianWriter.Data;
		for (int i = 0; i < newLocaTable.Length; i++)
		{
			int num2 = newLocaTable[i];
			if (bLocaIsShort)
			{
				num2 /= 2;
				bigEndianWriter.Write((short)num2);
			}
			else
			{
				bigEndianWriter.Write(num2);
			}
		}
		return num;
	}

	private byte[] GetFontProgram(byte[] newLocaTableOut, byte[] newGlyphTable, uint glyphTableSize, uint locaTableSize)
	{
		if (newLocaTableOut == null)
		{
			throw new ArgumentNullException("newLocaTableOut");
		}
		if (newGlyphTable == null)
		{
			throw new ArgumentNullException("newGlyphTable");
		}
		_ = TableNames;
		short numTables = 0;
		BigEndianWriter bigEndianWriter = new BigEndianWriter(GetFontProgramLength(newLocaTableOut, newGlyphTable, out numTables));
		bigEndianWriter.Write(65536);
		bigEndianWriter.Write(numTables);
		short num = s_entrySelectors[numTables];
		bigEndianWriter.Write((short)((1 << (int)num) * 16));
		bigEndianWriter.Write(num);
		bigEndianWriter.Write((short)((numTables - (1 << (int)num)) * 16));
		WriteCheckSums(bigEndianWriter, numTables, newLocaTableOut, newGlyphTable, glyphTableSize, locaTableSize);
		WriteGlyphs(bigEndianWriter, newLocaTableOut, newGlyphTable);
		return bigEndianWriter.Data;
	}

	private int GetFontProgramLength(byte[] newLocaTableOut, byte[] newGlyphTable, out short numTables)
	{
		if (newLocaTableOut == null)
		{
			throw new ArgumentNullException("newLocaTableOut");
		}
		if (newGlyphTable == null)
		{
			throw new ArgumentNullException("newGlyphTable");
		}
		numTables = 2;
		string[] tableNames = TableNames;
		int num = 0;
		int i = 0;
		for (int num2 = tableNames.Length; i < num2; i++)
		{
			string text = tableNames[i];
			if (text != "glyf" && text != "loca")
			{
				TtfTableInfo table = GetTable(text);
				if (!table.Empty)
				{
					numTables++;
					num += (int)Align((uint)table.Length);
				}
			}
		}
		num += newLocaTableOut.Length;
		num += newGlyphTable.Length;
		int num3 = numTables * 16 + 12;
		return num + num3;
	}

	private int CalculateCheckSum(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int i = 0;
		for (int num6 = (bytes.Length + 1) / 4; i < num6; i++)
		{
			num5 += bytes[num++] & 0xFF;
			num4 += bytes[num++] & 0xFF;
			num3 += bytes[num++] & 0xFF;
			num2 += bytes[num++] & 0xFF;
		}
		return num2 + (num3 << 8) + (num4 << 16) + (num5 << 24);
	}

	private void WriteCheckSums(BigEndianWriter writer, short numTables, byte[] newLocaTableOut, byte[] newGlyphTable, uint glyphTableSize, uint locaTableSize)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (newLocaTableOut == null)
		{
			throw new ArgumentNullException("newLocaTableOut");
		}
		if (newGlyphTable == null)
		{
			throw new ArgumentNullException("newGlyphTable");
		}
		string[] tableNames = TableNames;
		uint num = (uint)(numTables * 16 + 12);
		uint num2 = 0u;
		int i = 0;
		for (int num3 = tableNames.Length; i < num3; i++)
		{
			string text = tableNames[i];
			TtfTableInfo table = GetTable(text);
			if (!table.Empty)
			{
				writer.Write(text);
				if (text == "glyf")
				{
					int value = CalculateCheckSum(newGlyphTable);
					writer.Write(value);
					num2 = glyphTableSize;
				}
				else if (text == "loca")
				{
					int value2 = CalculateCheckSum(newLocaTableOut);
					writer.Write(value2);
					num2 = locaTableSize;
				}
				else
				{
					writer.Write(table.Checksum);
					num2 = (uint)table.Length;
				}
				writer.Write(num);
				writer.Write(num2);
				num += Align(num2);
			}
		}
	}

	private void WriteGlyphs(BigEndianWriter writer, byte[] newLocaTableOut, byte[] newGlyphTable)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (newLocaTableOut == null)
		{
			throw new ArgumentNullException("newLocaTableOut");
		}
		if (newGlyphTable == null)
		{
			throw new ArgumentNullException("newGlyphTable");
		}
		string[] tableNames = TableNames;
		int i = 0;
		for (int num = tableNames.Length; i < num; i++)
		{
			string text = tableNames[i];
			TtfTableInfo table = GetTable(text);
			if (!table.Empty)
			{
				if (text == "glyf")
				{
					writer.Write(newGlyphTable);
					continue;
				}
				if (text == "loca")
				{
					writer.Write(newLocaTableOut);
					continue;
				}
				byte[] array = new byte[Align((uint)table.Length)];
				m_reader.Seek(table.Offset);
				m_reader.Read(array, 0, table.Length);
				writer.Write(array);
			}
		}
	}

	private void InitializeFontName(TtfNameTable nameTable)
	{
		for (int i = 0; i < nameTable.RecordsCount; i++)
		{
			TtfNameRecord ttfNameRecord = nameTable.NameRecords[i];
			if (ttfNameRecord.NameID == 1)
			{
				if (metricsName != string.Empty)
				{
					m_metrics.FontFamily = ttfNameRecord.Name + metricsName;
				}
				else
				{
					m_metrics.FontFamily = ttfNameRecord.Name;
				}
			}
			else if (ttfNameRecord.NameID == 6)
			{
				m_metrics.PostScriptName = ttfNameRecord.Name;
			}
			if (m_metrics.FontFamily != null && m_metrics.PostScriptName != null)
			{
				break;
			}
		}
	}

	private uint Align(uint value)
	{
		return (uint)((value + 3) & -4);
	}

	private TtfGlyphInfo GetDefaultGlyph()
	{
		return GetGlyph(' ');
	}

	private bool CompareArrays(byte[] buff1, byte[] buff2)
	{
		bool result = false;
		if (buff1.Length == buff2.Length)
		{
			int i;
			for (i = 0; i < buff2.Length && buff2[i] == buff1[i]; i++)
			{
			}
			if (i == buff2.Length)
			{
				result = true;
			}
		}
		return result;
	}

	private uint FormatTableName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(name);
		BitConverter.ToUInt32(bytes, 0);
		return (uint)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
	}
}
