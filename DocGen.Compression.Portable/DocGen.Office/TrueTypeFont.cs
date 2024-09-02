using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Office;

internal class TrueTypeFont : Font, IDisposable
{
	internal static readonly Encoding Encoding = new Windows1252Encoding();

	private const int c_codePage = 1252;

	protected static object s_rtlRenderLock = new object();

	private bool m_embed;

	private bool m_unicode = true;

	internal ITrueTypeFont m_fontInternal;

	private bool m_bUseTrueType;

	private bool m_isContainsFont;

	private TTFFontStyle m_style;

	private string metricsName = string.Empty;

	private bool m_isEmbedFont;

	private TtfReader m_ttfReader;

	private bool m_isXPSFontStream;

	private bool m_isEMFFontStream;

	private bool m_conformanceEnabled;

	private bool m_isSkipFontEmbed;

	public bool Unicode => m_unicode;

	internal bool Embed => m_embed;

	internal ITrueTypeFont InternalFont => m_fontInternal;

	internal bool IsContainsFont => m_isContainsFont;

	internal string FontFile
	{
		get
		{
			string result = null;
			if (InternalFont is UnicodeTrueTypeFont unicodeTrueTypeFont)
			{
				result = unicodeTrueTypeFont.FontFile;
			}
			return result;
		}
	}

	internal TtfReader TtfReader => m_ttfReader;

	public TrueTypeFont(Stream fontStream, float size)
		: this(fontStream, size, TTFFontStyle.Regular)
	{
	}

	public TrueTypeFont(Stream fontStream, float size, bool isTrueType)
		: this(fontStream, size, TTFFontStyle.Regular, isTrueType)
	{
	}

	public TrueTypeFont(Stream fontStream, float size, TTFFontStyle style)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new Exception("Unable to parse the given font stream");
		}
		fontStream.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[fontStream.Length];
		fontStream.Read(array, 0, array.Length);
		CreateFontInternal(new MemoryStream(array), style);
	}

	public TrueTypeFont(Stream fontStream, float size, TTFFontStyle style, string originalFontName)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new Exception("Unable to parse the given font stream");
		}
		fontStream.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[fontStream.Length];
		fontStream.Read(array, 0, array.Length);
		CreateFontInternal(new MemoryStream(array), style, originalFontName);
	}

	internal TrueTypeFont(Stream fontStream, float size, TTFFontStyle style, bool useTrueType)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new Exception("Unable to parse the given font stream");
		}
		m_bUseTrueType = useTrueType;
		m_unicode = true;
		CreateFontInternal(fontStream, style);
	}

	internal TrueTypeFont(Stream fontStream, float size, string metricsName, bool isEnableEmbedding)
		: base(size)
	{
		m_unicode = true;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(fontStream, TTFFontStyle.Regular);
	}

	internal TrueTypeFont(Stream fontStream, float size, string metricsName, bool isEnableEmbedding, TTFFontStyle fontStyle)
		: base(size)
	{
		m_unicode = true;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(fontStream, fontStyle);
	}

	internal TrueTypeFont(Stream fontStream, float size, bool isUnicode, string metricsName, TTFFontStyle fontStyle)
		: base(size)
	{
		m_unicode = isUnicode;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		if (!isUnicode)
		{
			m_bUseTrueType = !isUnicode;
		}
		m_isSkipFontEmbed = !isUnicode;
		CreateFontInternal(fontStream, fontStyle);
	}

	internal TrueTypeFont(Stream fontStream, float size, bool isUnicode, string metricsName, TTFFontStyle fontStyle, string originalFontName)
		: base(size)
	{
		m_unicode = isUnicode;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		if (!isUnicode)
		{
			m_bUseTrueType = !isUnicode;
		}
		m_isSkipFontEmbed = !isUnicode;
		byte[] array = new byte[fontStream.Length];
		fontStream.Read(array, 0, array.Length);
		CreateFontInternal(new MemoryStream(array), fontStyle, originalFontName);
	}

	internal TrueTypeFont(Stream fontStream, float size, bool isEnableEmbedding, TTFFontStyle fontStyle)
		: base(size)
	{
		m_unicode = true;
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(fontStream, fontStyle);
	}

	public TrueTypeFont(TrueTypeFont prototype, float size)
		: base(size, prototype.Style)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_unicode = prototype.Unicode;
	}

	internal TrueTypeFont(TrueTypeFont prototype, bool isEnableEmbedding, float size)
		: base(size, prototype.Style)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_unicode = prototype.Unicode;
		m_isEmbedFont = isEnableEmbedding;
	}

	internal TrueTypeFont(TrueTypeFont prototype, float size, bool isXpsFontstream)
		: base(size, prototype.Style)
	{
		m_isXPSFontStream = isXpsFontstream;
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_unicode = prototype.Unicode;
		ITrueTypeFontPrimitive internals = null;
		if (prototype != null)
		{
			internals = ((ITrueTypeFontCache)prototype).GetInternals();
			TrueTypeFontMetrics metrics = ((Font)prototype).Metrics;
			metrics = (TrueTypeFontMetrics)metrics.Clone();
			metrics.Size = base.Size;
			base.Metrics = metrics;
			m_fontInternal = prototype.InternalFont;
			m_ttfReader = prototype.m_ttfReader;
		}
		((ITrueTypeFontCache)this).SetInternals(internals);
	}

	~TrueTypeFont()
	{
		if (!m_isXPSFontStream)
		{
			Dispose();
		}
	}

	public void Dispose()
	{
		if (m_fontInternal != null)
		{
			lock (Font.s_syncObject)
			{
				m_fontInternal.Close();
				m_fontInternal = null;
			}
		}
	}

	protected internal override float GetCharWidth(char charCode, TrueTypeFontStringFormat format)
	{
		float num = InternalFont.GetCharWidth(charCode);
		float size = base.Metrics.GetSize(format);
		return num * (0.001f * size);
	}

	protected internal override float GetLineWidth(string line, TrueTypeFontStringFormat format)
	{
		float width = 0f;
		if (format == null || format.TextDirection == TextDirection.None)
		{
			width = ((!(InternalFont is UnicodeTrueTypeFont unicodeFont)) ? InternalFont.GetLineWidth(line, format) : GetWidth(unicodeFont, line));
		}
		else
		{
			GetUnicodeLineWidth(line, out width, format);
		}
		float size = base.Metrics.GetSize(format);
		width = width / base.Metrics.UnitPerEM * size;
		return ApplyFormatSettings(line, format, width);
	}

	protected internal override float GetLineWidth(string line, TrueTypeFontStringFormat format, out float boundWidth)
	{
		float width = 0f;
		float boundWidth2 = 0f;
		if (format == null || format.TextDirection == TextDirection.None)
		{
			width = ((!(InternalFont is UnicodeTrueTypeFont unicodeFont)) ? InternalFont.GetLineWidth(line, format, out boundWidth2) : GetWidth(unicodeFont, line, format, out boundWidth2));
		}
		else
		{
			GetUnicodeLineWidth(line, out width, format);
		}
		boundWidth = boundWidth2;
		return ApplyFormatSettings(line, format, width);
	}

	internal float GetLineWidth(string line, TrueTypeFontStringFormat format, out OtfGlyphInfoList glyphList, ScriptTags[] tags)
	{
		float width = 0f;
		glyphList = null;
		if (TtfReader.isOTFFont())
		{
			ScriptLayouter scriptLayouter = new ScriptLayouter();
			List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
			string text = line;
			if (format != null && (format.RightToLeft || format.TextDirection != 0))
			{
				text = new ArabicShapeRenderer().Shape(line.ToCharArray(), 0);
			}
			string text2 = text;
			foreach (char c in text2)
			{
				TtfGlyphInfo glyph = TtfReader.GetGlyph(c);
				OtfGlyphInfo otfGlyphInfo = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
				if (c != ' ' && glyph.CharCode == 32)
				{
					otfGlyphInfo.unsupportedGlyph = true;
				}
				list.Add(otfGlyphInfo);
			}
			glyphList = new OtfGlyphInfoList(list);
			if (tags.Length != 0)
			{
				bool flag = false;
				foreach (ScriptTags scriptTags in tags)
				{
					if (TtfReader.supportedScriptTags.Contains(scriptTags))
					{
						bool flag2 = scriptLayouter.DoLayout(InternalFont as UnicodeTrueTypeFont, glyphList, scriptTags);
						if (flag2)
						{
							flag = flag2;
						}
					}
				}
				if (flag)
				{
					foreach (OtfGlyphInfo glyph2 in glyphList.Glyphs)
					{
						width += glyph2.Width;
					}
					m_isContainsFont = true;
				}
				else
				{
					glyphList = null;
					m_isContainsFont = false;
				}
			}
		}
		if (width == 0f)
		{
			if (format == null || format.TextDirection == TextDirection.None)
			{
				width = ((!(InternalFont is UnicodeTrueTypeFont unicodeFont)) ? ((float)InternalFont.GetLineWidth(line)) : GetWidth(unicodeFont, line));
			}
			else
			{
				GetUnicodeLineWidth(line, out width, format);
			}
		}
		float size = base.Metrics.GetSize(format);
		width = width / base.Metrics.UnitPerEM * size;
		return ApplyFormatSettings(line, format, width);
	}

	protected override bool EqualsToFont(Font font)
	{
		return m_fontInternal.EqualsToFont(font);
	}

	internal void SetSymbols(string text)
	{
		if (m_fontInternal is UnicodeTrueTypeFont)
		{
			_ = m_fontInternal is UnicodeTrueTypeFont;
		}
	}

	internal void SetSymbols(ushort[] glyphs)
	{
		if (m_fontInternal is UnicodeTrueTypeFont unicodeTrueTypeFont)
		{
			unicodeTrueTypeFont.SetSymbols(glyphs);
		}
	}

	private void CreateFontInternal(Stream fontStream, TTFFontStyle style)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontFile");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new Exception("Unable to parse the given font stream");
		}
		if (!m_bUseTrueType)
		{
			if (metricsName != string.Empty)
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0, metricsName);
			}
			else
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0);
			}
		}
		else if (metricsName != string.Empty)
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType, metricsName);
		}
		else
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType);
		}
		if (m_fontInternal != null)
		{
			(m_fontInternal as UnicodeTrueTypeFont).SkipFontEmbed = m_isSkipFontEmbed;
		}
		CalculateStyle(style);
		InitializeInternals();
	}

	private void CreateFontInternal(Stream fontStream, TTFFontStyle style, string originalFontName)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontFile");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new Exception("Unable to parse the given font stream");
		}
		if (!m_bUseTrueType)
		{
			if (metricsName != string.Empty)
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0, metricsName);
			}
			else
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0);
			}
		}
		else if (metricsName != string.Empty)
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType, metricsName);
		}
		else
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType);
		}
		if (m_fontInternal != null)
		{
			(m_fontInternal as UnicodeTrueTypeFont).SkipFontEmbed = m_isSkipFontEmbed;
		}
		CalculateStyle(style);
		InitializeInternals(originalFontName);
	}

	private void InitializeInternals()
	{
		ITrueTypeFontCache trueTypeFontCache = null;
		if (trueTypeFontCache != null)
		{
			trueTypeFontCache.GetInternals();
			TrueTypeFontMetrics metrics = ((Font)trueTypeFontCache).Metrics;
			metrics = (TrueTypeFontMetrics)metrics.Clone();
			metrics.Size = base.Size;
			base.Metrics = metrics;
			m_fontInternal = ((TrueTypeFont)trueTypeFontCache).InternalFont;
		}
		else if (trueTypeFontCache == null || m_bUseTrueType)
		{
			if (m_fontInternal is UnicodeTrueTypeFont)
			{
				(m_fontInternal as UnicodeTrueTypeFont).IsEmbed = m_isEmbedFont;
			}
			m_fontInternal.CreateInternals();
			m_fontInternal.GetInternals();
			base.Metrics = m_fontInternal.Metrics;
		}
		base.Metrics.isUnicodeFont = Unicode;
		if (m_fontInternal is UnicodeTrueTypeFont)
		{
			m_ttfReader = (m_fontInternal as UnicodeTrueTypeFont).TtfReader;
		}
	}

	private void InitializeInternals(string originalFontName)
	{
		ITrueTypeFontCache trueTypeFontCache = null;
		if (trueTypeFontCache != null)
		{
			trueTypeFontCache.GetInternals();
			TrueTypeFontMetrics metrics = ((Font)trueTypeFontCache).Metrics;
			metrics = (TrueTypeFontMetrics)metrics.Clone();
			metrics.Size = base.Size;
			base.Metrics = metrics;
			m_fontInternal = ((TrueTypeFont)trueTypeFontCache).InternalFont;
		}
		else if (trueTypeFontCache == null || m_bUseTrueType)
		{
			if (m_fontInternal is UnicodeTrueTypeFont)
			{
				(m_fontInternal as UnicodeTrueTypeFont).IsEmbed = m_isEmbedFont;
			}
			m_fontInternal.CreateInternals(originalFontName);
			m_fontInternal.GetInternals();
			base.Metrics = m_fontInternal.Metrics;
		}
		base.Metrics.isUnicodeFont = Unicode;
		if (m_fontInternal is UnicodeTrueTypeFont)
		{
			m_ttfReader = (m_fontInternal as UnicodeTrueTypeFont).TtfReader;
		}
	}

	private void CalculateStyle(TTFFontStyle style)
	{
		int num = ((UnicodeTrueTypeFont)m_fontInternal).TtfMetrics.MacStyle;
		if ((style & TTFFontStyle.Underline) != 0)
		{
			num |= 4;
		}
		if ((style & TTFFontStyle.Strikeout) != 0)
		{
			num |= 8;
		}
		m_style = style;
		SetStyle(style);
	}

	private float GetSymbolSize(char ch, TrueTypeFontStringFormat format)
	{
		return GetCharWidth(ch, format);
	}

	private bool GetUnicodeLineWidth(string line, out float width, TrueTypeFontStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		width = 0f;
		ushort[] glyphs = null;
		bool flag = false;
		lock (s_rtlRenderLock)
		{
			flag = RtlRenderer.GetGlyphIndices(line, this, format.TextDirection == TextDirection.RightToLeft, out glyphs, custom: true);
			if (flag && glyphs != null)
			{
				TtfReader ttfReader = (InternalFont as UnicodeTrueTypeFont).TtfReader;
				int i = 0;
				for (int num = glyphs.Length; i < num; i++)
				{
					int glyphIndex = glyphs[i];
					TtfGlyphInfo glyph = ttfReader.GetGlyph(glyphIndex);
					if (!glyph.Empty)
					{
						width += glyph.Width;
					}
				}
				if (!new List<ushort>(glyphs).Contains(0) && ttfReader.m_missedGlyphCount == 0)
				{
					m_isContainsFont = true;
				}
				else
				{
					m_isContainsFont = false;
				}
			}
		}
		return flag;
	}

	private float GetWidth(UnicodeTrueTypeFont unicodeFont, string line)
	{
		bool flag = false;
		if (unicodeFont != null && unicodeFont.TtfReader != null)
		{
			flag = true;
			unicodeFont.TtfReader.m_missedGlyphCount = 0;
		}
		float result = InternalFont.GetLineWidth(line);
		if (flag && unicodeFont.TtfReader.m_missedGlyphCount == 0)
		{
			m_isContainsFont = true;
			return result;
		}
		if (unicodeFont != null)
		{
			m_isContainsFont = false;
		}
		return result;
	}

	private float GetWidth(UnicodeTrueTypeFont unicodeFont, string line, TrueTypeFontStringFormat format, out float boundWidth)
	{
		bool flag = false;
		if (unicodeFont != null && unicodeFont.TtfReader != null)
		{
			flag = true;
			unicodeFont.TtfReader.m_missedGlyphCount = 0;
		}
		float lineWidth = InternalFont.GetLineWidth(line, format, out boundWidth);
		if (flag && unicodeFont.TtfReader.m_missedGlyphCount == 0)
		{
			m_isContainsFont = true;
			return lineWidth;
		}
		if (unicodeFont != null)
		{
			m_isContainsFont = false;
		}
		return lineWidth;
	}
}
