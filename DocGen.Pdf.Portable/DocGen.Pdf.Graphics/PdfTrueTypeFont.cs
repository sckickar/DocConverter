using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfTrueTypeFont : PdfFont, IDisposable
{
	internal static readonly Encoding Encoding = new Windows1252Encoding();

	private const int c_codePage = 1252;

	protected static object s_rtlRenderLock = new object();

	private bool m_embed;

	private bool m_unicode = true;

	internal ITrueTypeFont m_fontInternal;

	private bool m_bUseTrueType;

	private bool m_isContainsFont;

	private PdfFontStyle m_style;

	private string metricsName = string.Empty;

	private bool m_isEmbedFont;

	private TtfReader m_ttfReader;

	private bool m_isXPSFontStream;

	private bool m_isEMFFontStream;

	private bool m_conformanceEnabled;

	private bool m_isSkipFontEmbed;

	private bool is_filePath;

	private bool m_fullEmbed;

	internal bool m_isStyleBold;

	private bool m_useFloatingFactorForMeasure;

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

	internal bool IsEmbedFont => m_isEmbedFont;

	public PdfTrueTypeFont(Stream fontStream, float size)
		: this(fontStream, size, PdfFontStyle.Regular)
	{
	}

	public PdfTrueTypeFont(string fontFile, float size)
		: base(size)
	{
		CreateFontInternal(fontFile, PdfFontStyle.Regular);
	}

	public PdfTrueTypeFont(string fontFile, float size, PdfFontStyle style)
		: base(size)
	{
		if (style == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		CreateFontInternal(fontFile, style);
	}

	public PdfTrueTypeFont(string fontFile, bool embed, PdfFontStyle style, float size)
		: base(size)
	{
		m_unicode = embed;
		if (!embed)
		{
			m_bUseTrueType = !embed;
		}
		if (style == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		m_isSkipFontEmbed = !embed;
		m_isEmbedFont = embed;
		CreateFontInternal(fontFile, style);
	}

	internal PdfTrueTypeFont(Stream fontStream, float size, bool isTrueType)
		: this(fontStream, size, PdfFontStyle.Regular, isTrueType)
	{
	}

	public PdfTrueTypeFont(Stream fontStream, float size, PdfFontStyle style)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new PdfException("Unable to parse the given font stream");
		}
		if (style == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		fontStream.Seek(0L, SeekOrigin.Begin);
		CreateFontInternal(fontStream, style);
	}

	public PdfTrueTypeFont(Stream fontStream, bool embed, PdfFontStyle style, float size)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		m_unicode = embed;
		if (!embed)
		{
			m_bUseTrueType = !embed;
		}
		if (style == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		m_isSkipFontEmbed = !embed;
		m_isEmbedFont = embed;
		CreateFontInternal(fontStream, style);
	}

	internal PdfTrueTypeFont(Stream fontStream, float size, PdfFontStyle style, bool useTrueType)
		: base(size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new PdfException("Unable to parse the given font stream");
		}
		m_bUseTrueType = useTrueType;
		m_unicode = true;
		if (style == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		CreateFontInternal(fontStream, style);
	}

	public PdfTrueTypeFont(Stream fontStream, float size, bool embed, bool subset)
		: base(size)
	{
		if (!embed)
		{
			m_bUseTrueType = !embed;
			m_isSkipFontEmbed = !embed;
		}
		else
		{
			m_fullEmbed = !subset;
		}
		m_isEmbedFont = !subset;
		m_unicode = embed;
		CreateFontInternal(fontStream, PdfFontStyle.Regular);
	}

	internal PdfTrueTypeFont(Stream fontStream, float size, string metricsName, bool isEnableEmbedding)
		: base(size)
	{
		m_unicode = true;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(fontStream, PdfFontStyle.Regular);
	}

	internal PdfTrueTypeFont(Stream fontStream, float size, string metricsName, bool isEnableEmbedding, PdfFontStyle fontStyle)
		: base(size)
	{
		m_unicode = true;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		if (fontStyle == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		CreateFontInternal(fontStream, fontStyle);
	}

	internal PdfTrueTypeFont(Stream fontStream, PdfFontStyle fontStyle, float size, string metricsName, bool useTrueType, bool isEnableEmbedding)
		: base(size)
	{
		m_unicode = true;
		m_bUseTrueType = useTrueType;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		if (fontStyle == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		CreateFontInternal(fontStream, fontStyle);
	}

	internal PdfTrueTypeFont(Stream fontStream, PdfFontStyle fontStyle, float size, string metricsName, bool useTrueType, bool isEnableEmbedding, bool isConformanceEnabled)
		: base(size)
	{
		m_unicode = true;
		m_bUseTrueType = useTrueType;
		this.metricsName = metricsName;
		m_isXPSFontStream = true;
		m_isEmbedFont = isEnableEmbedding;
		m_conformanceEnabled = isConformanceEnabled;
		if (fontStyle == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		CreateFontInternal(fontStream, fontStyle);
	}

	internal PdfTrueTypeFont(Stream fontStream, float size, bool isUnicode, string metricsName, PdfFontStyle fontStyle)
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

	internal PdfTrueTypeFont(Stream fontStream, float size, bool isEnableEmbedding, PdfFontStyle fontStyle)
		: base(size)
	{
		m_unicode = true;
		if (fontStyle == PdfFontStyle.Bold)
		{
			m_isStyleBold = true;
		}
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(fontStream, fontStyle);
	}

	public PdfTrueTypeFont(PdfTrueTypeFont prototype, float size)
		: base(size, prototype.Style)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		if (prototype.m_isXPSFontStream)
		{
			CreateXpsFontInternal(prototype);
			return;
		}
		is_filePath = prototype.is_filePath;
		m_unicode = prototype.Unicode;
		CreateFontInternal(prototype);
	}

	internal PdfTrueTypeFont(PdfTrueTypeFont prototype, bool isEnableEmbedding, float size)
		: base(size, prototype.Style)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_unicode = prototype.Unicode;
		m_isEmbedFont = isEnableEmbedding;
		CreateFontInternal(prototype);
	}

	internal PdfTrueTypeFont(PdfTrueTypeFont prototype, float size, bool isXpsFontstream)
		: base(size, prototype.Style)
	{
		m_isXPSFontStream = isXpsFontstream;
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		CreateXpsFontInternal(prototype);
	}

	public PdfTrueTypeFont(Stream fontStream, PdfFontSettings fontSettings)
		: base(fontSettings.Size)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontStream");
		}
		if (fontSettings == null)
		{
			throw new ArgumentNullException("fontSettings");
		}
		if (!fontSettings.Embed)
		{
			m_bUseTrueType = !fontSettings.Embed;
			m_isSkipFontEmbed = !fontSettings.Embed;
		}
		else
		{
			m_fullEmbed = !fontSettings.Subset;
		}
		m_isEmbedFont = !fontSettings.Subset;
		m_unicode = fontSettings.Embed;
		m_useFloatingFactorForMeasure = fontSettings.UseFloatingFactorForMeasure;
		CreateFontInternal(fontStream, fontSettings.Style);
	}

	~PdfTrueTypeFont()
	{
		if (!m_isXPSFontStream)
		{
			Dispose();
		}
	}

	public void Dispose()
	{
		if (m_fontInternal == null)
		{
			return;
		}
		lock (PdfFont.s_syncObject)
		{
			if (PdfDocument.EnableCache)
			{
				PdfDocument.Cache.Remove(this);
				if (PdfDocument.Cache.GroupCount(this) == 0)
				{
					m_fontInternal.Close();
				}
				m_fontInternal = null;
			}
		}
	}

	protected internal override float GetCharWidth(char charCode, PdfStringFormat format)
	{
		float charWidth = InternalFont.GetCharWidth(charCode);
		float size = base.Metrics.GetSize(format);
		return charWidth * (0.001f * size);
	}

	private void CreateXpsFontInternal(PdfTrueTypeFont prototype)
	{
		m_unicode = prototype.Unicode;
		IPdfPrimitive internals = null;
		if (prototype != null)
		{
			internals = ((IPdfCache)prototype).GetInternals();
			PdfFontMetrics metrics = ((PdfFont)prototype).Metrics;
			metrics = (PdfFontMetrics)metrics.Clone();
			metrics.Size = base.Size;
			base.Metrics = metrics;
			m_fontInternal = prototype.InternalFont;
			m_ttfReader = prototype.m_ttfReader;
		}
		((IPdfCache)this).SetInternals(internals);
	}

	protected internal override float GetLineWidth(string line, PdfStringFormat format)
	{
		float width = 0f;
		if (format == null || format.TextDirection == PdfTextDirection.None)
		{
			width = ((!(InternalFont is UnicodeTrueTypeFont unicodeFont)) ? InternalFont.GetLineWidth(line) : GetWidth(unicodeFont, line));
		}
		else
		{
			GetUnicodeLineWidth(line, out width, format);
		}
		float size = base.Metrics.GetSize(format);
		width *= 0.001f * size;
		return ApplyFormatSettings(line, format, width);
	}

	internal float GetLineWidth(string line, PdfStringFormat format, out OtfGlyphInfoList glyphList, ScriptTags[] tags)
	{
		float width = 0f;
		glyphList = null;
		if (TtfReader.isOTFFont())
		{
			bool flag = false;
			ScriptTags[] array = tags;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == ScriptTags.Arabic)
				{
					flag = true;
					if (format == null)
					{
						format = new PdfStringFormat();
					}
					if (format.TextDirection == PdfTextDirection.None)
					{
						format.TextDirection = PdfTextDirection.RightToLeft;
					}
					break;
				}
			}
			ScriptLayouter scriptLayouter = new ScriptLayouter();
			List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
			string text = line;
			if (format != null && (format.RightToLeft || format.TextDirection != 0) && !flag)
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
				bool flag2 = false;
				array = tags;
				foreach (ScriptTags scriptTags in array)
				{
					if (TtfReader.supportedScriptTags.Contains(scriptTags))
					{
						bool flag3 = scriptLayouter.DoLayout(InternalFont as UnicodeTrueTypeFont, glyphList, scriptTags);
						if (flag3)
						{
							flag2 = flag3;
						}
					}
				}
				if (flag2)
				{
					foreach (OtfGlyphInfo glyph3 in glyphList.Glyphs)
					{
						width += glyph3.Width;
					}
					m_isContainsFont = true;
				}
				else
				{
					list = new List<OtfGlyphInfo>();
					text = new ArabicShapeRenderer().Shape(line.ToCharArray(), 0);
					TtfReader.m_missedGlyphCount = 0;
					text2 = text;
					foreach (char c2 in text2)
					{
						TtfGlyphInfo glyph2 = TtfReader.GetGlyph(c2);
						OtfGlyphInfo otfGlyphInfo2 = new OtfGlyphInfo(glyph2.CharCode, glyph2.Index, glyph2.Width);
						if (c2 != ' ' && glyph2.CharCode == 32)
						{
							otfGlyphInfo2.unsupportedGlyph = true;
						}
						list.Add(otfGlyphInfo2);
					}
					if (TtfReader.m_missedGlyphCount > 0)
					{
						m_isContainsFont = false;
					}
					else
					{
						glyphList = new OtfGlyphInfoList(list);
						foreach (OtfGlyphInfo glyph4 in glyphList.Glyphs)
						{
							width += glyph4.Width;
						}
						m_isContainsFont = true;
					}
				}
			}
		}
		if (width == 0f)
		{
			if (format == null || format.TextDirection == PdfTextDirection.None)
			{
				width = ((!(InternalFont is UnicodeTrueTypeFont unicodeFont)) ? InternalFont.GetLineWidth(line) : GetWidth(unicodeFont, line));
			}
			else
			{
				GetUnicodeLineWidth(line, out width, format);
			}
		}
		float size = base.Metrics.GetSize(format);
		width *= 0.001f * size;
		return ApplyFormatSettings(line, format, width);
	}

	protected override bool EqualsToFont(PdfFont font)
	{
		return m_fontInternal.EqualsToFont(font);
	}

	internal void SetSymbols(string text)
	{
		if (m_fontInternal is UnicodeTrueTypeFont && m_fontInternal is UnicodeTrueTypeFont unicodeTrueTypeFont)
		{
			unicodeTrueTypeFont.SetSymbols(text);
		}
	}

	internal void SetSymbols(ushort[] glyphs)
	{
		if (m_fontInternal is UnicodeTrueTypeFont unicodeTrueTypeFont)
		{
			unicodeTrueTypeFont.SetSymbols(glyphs);
		}
	}

	private void CreateFontInternal(PdfTrueTypeFont prototype)
	{
		if (prototype == null)
		{
			throw new ArgumentNullException("prototype");
		}
		m_fontInternal = new UnicodeTrueTypeFont(prototype.InternalFont as UnicodeTrueTypeFont);
		InitializeInternals();
	}

	private void CreateFontInternal(Stream fontStream, PdfFontStyle style)
	{
		if (fontStream == null)
		{
			throw new ArgumentNullException("fontFile");
		}
		if (!fontStream.CanSeek || !fontStream.CanRead)
		{
			throw new PdfException("Unable to parse the given font stream");
		}
		if (!m_bUseTrueType)
		{
			if (metricsName != string.Empty)
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0, metricsName);
			}
			else
			{
				m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.Type0, m_useFloatingFactorForMeasure);
			}
		}
		else if (metricsName != string.Empty)
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType, metricsName);
		}
		else
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontStream, base.Size, CompositeFontType.TrueType, m_useFloatingFactorForMeasure);
		}
		if (m_fontInternal != null)
		{
			UnicodeTrueTypeFont obj = m_fontInternal as UnicodeTrueTypeFont;
			obj.SkipFontEmbed = m_isSkipFontEmbed;
			obj.ForceFullFontEmbed = m_fullEmbed;
		}
		CalculateStyle(style);
		InitializeInternals();
	}

	private void InitializeInternals()
	{
		IPdfCache pdfCache = null;
		if (PdfDocument.EnableCache)
		{
			lock (PdfDocument.Cache)
			{
				pdfCache = PdfDocument.Cache.Search(this);
			}
		}
		else if (!PdfDocument.EnableCache)
		{
			lock (PdfDocument.Cache)
			{
				if (PdfDocument.Cache.ContainsFont(this) is PdfTrueTypeFont pdfTrueTypeFont)
				{
					pdfCache = pdfTrueTypeFont;
				}
			}
		}
		IPdfPrimitive internals = null;
		if (pdfCache != null)
		{
			internals = pdfCache.GetInternals();
			PdfFontMetrics metrics = ((PdfFont)pdfCache).Metrics;
			metrics = (PdfFontMetrics)metrics.Clone();
			metrics.Size = base.Size;
			base.Metrics = metrics;
			m_fontInternal = ((PdfTrueTypeFont)pdfCache).InternalFont;
		}
		else
		{
			if (pdfCache == null || m_bUseTrueType)
			{
				if (PdfDocument.EnableCache && m_bUseTrueType)
				{
					PdfDocument.Cache.Remove(pdfCache);
				}
				if (m_fontInternal is UnicodeTrueTypeFont)
				{
					(m_fontInternal as UnicodeTrueTypeFont).IsEmbed = m_isEmbedFont;
					(m_fontInternal as UnicodeTrueTypeFont).conformanceEnabled = m_conformanceEnabled;
					(m_fontInternal as UnicodeTrueTypeFont).is_filePath = is_filePath;
				}
				m_fontInternal.CreateInternals();
				internals = m_fontInternal.GetInternals();
				base.Metrics = m_fontInternal.Metrics;
				base.Metrics.Size = base.Size;
			}
			if (!PdfDocument.EnableCache)
			{
				lock (PdfDocument.Cache)
				{
					PdfDocument.Cache.AddFont(this);
				}
			}
		}
		base.Metrics.isUnicodeFont = Unicode;
		((IPdfCache)this).SetInternals(internals);
		if (m_fontInternal is UnicodeTrueTypeFont)
		{
			m_ttfReader = (m_fontInternal as UnicodeTrueTypeFont).TtfReader;
		}
	}

	private void CalculateStyle(PdfFontStyle style)
	{
		int num = ((UnicodeTrueTypeFont)m_fontInternal).TtfMetrics.MacStyle;
		if ((style & PdfFontStyle.Underline) != 0)
		{
			num |= 4;
		}
		if ((style & PdfFontStyle.Strikeout) != 0)
		{
			num |= 8;
		}
		m_style = style;
		SetStyle(style);
	}

	private float GetSymbolSize(char ch, PdfStringFormat format)
	{
		return GetCharWidth(ch, format);
	}

	private bool GetUnicodeLineWidth(string line, out float width, PdfStringFormat format)
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
			flag = RtlRenderer.GetGlyphIndices(line, this, format.TextDirection == PdfTextDirection.RightToLeft, out glyphs, custom: true);
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
		float lineWidth = InternalFont.GetLineWidth(line);
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

	private void CreateFontInternal(string fontFile, PdfFontStyle style)
	{
		if (fontFile == null)
		{
			throw new ArgumentNullException("fontFile");
		}
		if (fontFile.Length == 0)
		{
			throw new ArgumentException("fontFile - string can not be empty");
		}
		is_filePath = true;
		if (!m_bUseTrueType)
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontFile, base.Size, CompositeFontType.Type0);
		}
		else
		{
			m_fontInternal = new UnicodeTrueTypeFont(fontFile, base.Size, CompositeFontType.TrueType);
		}
		m_style = style;
		CalculateStyle(style);
		InitializeInternals();
	}
}
