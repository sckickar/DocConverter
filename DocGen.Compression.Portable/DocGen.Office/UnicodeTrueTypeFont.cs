using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.Office;

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

	private string m_filePath;

	private float m_size;

	private TrueTypeFontMetrics m_metrics;

	private TrueTypeFontStream m_fontProgram;

	private TrueTypeFontStream m_cmap;

	private TrueTypeFontStream m_CidStream;

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

	internal bool m_isClearUsedChars;

	internal bool m_isXPSFontStream;

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

	public TrueTypeFontMetrics Metrics => m_metrics;

	internal TtfReader TtfReader => m_ttfReader;

	internal string FontFile => m_filePath;

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

	public UnicodeTrueTypeFont(Stream font, float size, CompositeFontType type)
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

	public ITrueTypeFontPrimitive GetInternals()
	{
		return null;
	}

	public bool EqualsToFont(Font font)
	{
		bool flag = false;
		TrueTypeFont trueTypeFont = font as TrueTypeFont;
		if (trueTypeFont != null && trueTypeFont.Unicode)
		{
			bool flag2 = false;
			bool flag3 = false;
			UnicodeTrueTypeFont unicodeTrueTypeFont = (UnicodeTrueTypeFont)trueTypeFont.InternalFont;
			bool num = m_ttfMetrics.FontFamily.Equals(unicodeTrueTypeFont.m_ttfMetrics.FontFamily);
			flag2 = m_ttfMetrics.MacStyle == unicodeTrueTypeFont.m_ttfMetrics.MacStyle;
			flag = num && flag2;
			if (flag)
			{
				flag3 = IsEqualFontStream(m_fontStream, unicodeTrueTypeFont.m_fontStream);
				flag = flag && flag3;
			}
		}
		else if (trueTypeFont != null && SkipFontEmbed)
		{
			bool flag4 = false;
			bool flag5 = false;
			UnicodeTrueTypeFont unicodeTrueTypeFont2 = (UnicodeTrueTypeFont)trueTypeFont.InternalFont;
			bool num2 = m_ttfMetrics.FontFamily.Equals(unicodeTrueTypeFont2.m_ttfMetrics.FontFamily);
			flag4 = m_ttfMetrics.MacStyle == unicodeTrueTypeFont2.m_ttfMetrics.MacStyle;
			flag = num2 && flag4;
			if (flag)
			{
				flag5 = IsEqualFontStream(m_fontStream, unicodeTrueTypeFont2.m_fontStream);
				flag = flag && flag5;
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
		m_fontProgram = new TrueTypeFontStream();
		m_cmap = new TrueTypeFontStream();
		m_metrics = new TrueTypeFontMetrics();
		m_ttfReader.Reader = new BinaryReader(m_fontStream, TtfReader.Encoding);
		m_ttfReader.m_AnsiEncode = SkipFontEmbed;
		m_ttfReader.CreateInternals();
		m_ttfMetrics = m_ttfReader.Metrics;
		InitializeMetrics();
		m_subsetName = GetFontName();
	}

	public void CreateInternals(string originalFontName)
	{
		m_fontProgram = new TrueTypeFontStream();
		m_cmap = new TrueTypeFontStream();
		m_metrics = new TrueTypeFontMetrics();
		m_ttfReader.Reader = new BinaryReader(m_fontStream, TtfReader.Encoding);
		m_ttfReader.OriginalFontName = originalFontName;
		m_ttfReader.m_AnsiEncode = SkipFontEmbed;
		m_ttfReader.CreateInternals();
		m_ttfMetrics = m_ttfReader.Metrics;
		InitializeMetrics();
		m_subsetName = GetFontName();
	}

	public int GetCharWidth(char charCode)
	{
		return m_ttfReader.GetCharWidth(charCode);
	}

	public int GetLineWidth(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		int num = 0;
		int i = 0;
		for (int length = line.Length; i < length; i++)
		{
			char charCode = line[i];
			int charWidth = ((ITrueTypeFont)this).GetCharWidth(charCode);
			num += charWidth;
		}
		return num;
	}

	public float GetLineWidth(string line, TrueTypeFontStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = 0f;
		float num2 = 0f;
		int i = 0;
		for (int length = line.Length; i < length; i++)
		{
			char charCode = line[i];
			float num3 = ((ITrueTypeFont)this).GetCharWidth(charCode);
			float size = Metrics.GetSize(format);
			if (i == 0)
			{
				num3 = num3 / Metrics.UnitPerEM * size;
			}
			else
			{
				float num4 = num3 / Metrics.UnitPerEM * size;
				num2 = num + (float)(int)num4;
				num3 = num4;
			}
			num += num3;
		}
		if (!line.StartsWith(" ") && !line.EndsWith(" "))
		{
			return num2 - 1f;
		}
		return num;
	}

	public float GetLineWidth(string line, TrueTypeFontStringFormat format, out float boundWidth)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		int i = 0;
		for (int length = line.Length; i < length; i++)
		{
			char charCode = line[i];
			float num3 = ((ITrueTypeFont)this).GetCharWidth(charCode);
			float size = Metrics.GetSize(format);
			if (i == 0)
			{
				num3 = num3 / Metrics.UnitPerEM * size;
				num2 = (int)num3;
			}
			else
			{
				float num4 = num3 / Metrics.UnitPerEM * size;
				num2 = num + (float)(int)num4;
				num3 = num4;
				flag = true;
			}
			num += num3;
		}
		boundWidth = (flag ? (num2 - SpecialCharLeft(line)) : num2);
		return num;
	}

	public float SpecialCharLeft(string input)
	{
		string text = "\\|!#$%&/()=?\ufffd\ufffd@\ufffd\ufffd\ufffd{}.-;'<>_,";
		for (int i = 0; i < text.Length; i++)
		{
			if (input.Contains(text[i].ToString()))
			{
				return 3f;
			}
		}
		return 1f;
	}

	public void Close()
	{
		if (m_fontProgram != null)
		{
			m_fontProgram.Clear();
			m_fontProgram = null;
		}
		if (m_cmap != null)
		{
			m_cmap.Clear();
			m_cmap = null;
		}
		if (m_CidStream != null)
		{
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
		using BinaryReader reader = new BinaryReader(font, TtfReader.Encoding);
		if (metricsName != string.Empty)
		{
			m_ttfReader = new TtfReader(reader, metricsName);
		}
		else
		{
			m_ttfReader = new TtfReader(reader);
		}
		m_ttfMetrics = m_ttfReader.Metrics;
	}

	private void InitializeMetrics()
	{
		TtfMetrics metrics = m_ttfReader.Metrics;
		m_metrics.Ascent = metrics.WinAscent;
		m_metrics.Descent = metrics.WinDescent;
		m_metrics.UnitPerEM = metrics.UnitsPerEm;
		m_metrics.Height = metrics.MacAscent - metrics.MacDescent + (float)metrics.LineGap;
		m_metrics.Leading = m_metrics.Height - (metrics.WinAscent + metrics.WinDescent);
		if (m_metrics.Leading < 0f)
		{
			m_metrics.Leading = 0f;
		}
		m_metrics.Name = metrics.FontFamily;
		m_metrics.PostScriptName = metrics.PostScriptName;
		m_metrics.Size = m_size;
		m_metrics.WidthTable = new StandardWidthTable(metrics.WidthTable);
		m_metrics.LineGap = metrics.LineGap;
		m_metrics.SubScriptSizeFactor = metrics.SubScriptSizeFactor;
		m_metrics.SuperscriptSizeFactor = metrics.SuperscriptSizeFactor;
		m_metrics.IsBold = metrics.IsBold;
	}

	internal int GetUsedCharsCount()
	{
		return m_usedChars.Count;
	}

	internal void SetGlyphInfo(List<TtfGlyphInfo> collection)
	{
		glyphInfo = collection;
	}

	private void GenerateCmap()
	{
		if (m_usedChars != null && m_usedChars.Count > 0 && m_openTypeGlyphs == null)
		{
			Dictionary<int, int> glyphChars = m_ttfReader.GetGlyphChars(m_usedChars);
			if (glyphChars.Count <= 0)
			{
				return;
			}
			int[] array = new int[glyphChars.Count];
			glyphChars.Keys.CopyTo(array, 0);
			Array.Sort(array);
			List<int> list = new List<int>(glyphChars.Keys.Count);
			list.AddRange(glyphChars.Keys);
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
				stringBuilder.AppendFormat("<{0:X04}><{0:X04}><{1:X04}>\n", num3, glyphChars[num3]);
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
			char[] characters = otfGlyphInfo.Characters;
			foreach (char c in characters)
			{
				stringBuilder2.AppendFormat("{0:X04}", (int)c);
			}
			stringBuilder2.Append(">\n");
			stringBuilder.Append(stringBuilder2.ToString());
		}
		stringBuilder.Append("endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\r\n");
		m_cmap.Clear();
		m_cmap.Write(stringBuilder.ToString());
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
		if (m_type == CompositeFontType.Type0)
		{
			if (!m_isEmbedFont)
			{
				for (int i = 0; i < 6; i++)
				{
					int index = 0;
					stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[index]);
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

	private TrueTypeFontArray GetOpenTypeDecendantWidth()
	{
		UpdateOpenTypeGlyphs();
		TrueTypeFontArray result = new TrueTypeFontArray();
		if (m_openTypeGlyphs != null && m_openTypeGlyphs.Count > 0)
		{
			int[] array = new int[m_openTypeGlyphs.Count];
			m_openTypeGlyphs.Keys.CopyTo(array, 0);
			Array.Sort(array);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				new TrueTypeFontArray();
			}
		}
		return result;
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
