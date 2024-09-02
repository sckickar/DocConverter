using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using DocGen.Drawing.DocIOHelper;
using DocGen.Office;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class FontExtension : SKPaint, IFontExtension
{
	private string m_fontName;

	private FontFamily m_fontFamily;

	private FontStyle m_fontStyle;

	private static SKTypeface DefaultLinuxFont;

	private Stream m_stream;

	private static readonly object m_threadLocker = new object();

	[ThreadStatic]
	private static Dictionary<string, SKTypeface> typeFaceCache;

	[ThreadStatic]
	private static Dictionary<string, Stream> fontStreams;

	[ThreadStatic]
	private static Dictionary<string, TrueTypeFont> ttfFontCache;

	internal TrueTypeFont TTFFont;

	internal static bool IsLinuxOS = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public float Size => base.TextSize;

	public Dictionary<string, SKTypeface> TypeFaceCache
	{
		get
		{
			if (typeFaceCache == null)
			{
				lock (m_threadLocker)
				{
					if (typeFaceCache == null)
					{
						typeFaceCache = new Dictionary<string, SKTypeface>(StringComparer.CurrentCultureIgnoreCase);
					}
				}
			}
			return typeFaceCache;
		}
	}

	public Dictionary<string, TrueTypeFont> TTFFontCache
	{
		get
		{
			if (ttfFontCache == null)
			{
				ttfFontCache = new Dictionary<string, TrueTypeFont>(StringComparer.CurrentCultureIgnoreCase);
			}
			return ttfFontCache;
		}
	}

	public Dictionary<string, Stream> FontStreams
	{
		get
		{
			if (fontStreams == null)
			{
				lock (m_threadLocker)
				{
					if (fontStreams == null)
					{
						fontStreams = new Dictionary<string, Stream>(StringComparer.CurrentCultureIgnoreCase);
					}
				}
			}
			return fontStreams;
		}
	}

	public new FontStyle Style => base.Typeface.Style switch
	{
		SKTypefaceStyle.Bold => FontStyle.Bold, 
		SKTypefaceStyle.Italic => FontStyle.Italic, 
		SKTypefaceStyle.BoldItalic => FontStyle.Regular, 
		_ => FontStyle.Regular, 
	};

	internal string Key
	{
		get
		{
			string fontStyleValue = GetFontStyleValue(m_fontStyle);
			return m_fontName + "_" + fontStyleValue;
		}
	}

	public Stream FontStream
	{
		get
		{
			if (m_stream == null)
			{
				lock (m_threadLocker)
				{
					if (FontStreams.ContainsKey(Key))
					{
						Stream stream = new MemoryStream();
						FontStreams[Key].Position = 0L;
						FontStreams[Key].CopyTo(stream);
						stream.Position = 0L;
						return stream;
					}
					SKStreamAsset sKStreamAsset = base.Typeface.OpenStream();
					if (sKStreamAsset != null && sKStreamAsset.Length > 0)
					{
						byte[] buffer = new byte[sKStreamAsset.Length - 1];
						sKStreamAsset.Read(buffer, sKStreamAsset.Length);
						sKStreamAsset.Dispose();
						m_stream = new MemoryStream(buffer);
						return m_stream;
					}
					return null;
				}
			}
			return m_stream;
		}
	}

	public float Height
	{
		get
		{
			if (IsLinuxOS && TTFFontCache != null && TTFFontCache.ContainsKey(Key))
			{
				TTFFont = TTFFontCache[Key];
				if (TTFFont != null)
				{
					TTFFont.Size = Size;
					return TTFFont.Metrics.GetHeight(null);
				}
				return 0f;
			}
			return (float)Math.Abs(Math.Round(base.FontMetrics.Top));
		}
	}

	public float ExactHeight => Math.Abs(base.FontMetrics.Ascent) + Math.Abs(base.FontMetrics.Descent) + Math.Abs(base.FontMetrics.Leading);

	internal FontExtension(string fontName, float fontSize, FontStyle style, GraphicsUnit unit)
	{
		InitializeSkiaSharpProperties(fontName, fontSize, style, unit);
		InitializeTTFFont(fontName, fontSize, style, unit);
	}

	internal FontExtension(string fontName, float fontSize, FontStyle style, GraphicsUnit unit, FontScriptType scriptType)
	{
		InitializeSkiaSharpProperties(fontName, fontSize, style, unit);
		if (IsLinuxOS && base.Typeface != null && !Extension.IsHarfBuzzSupportedScript(scriptType))
		{
			InitializeTTFFont(fontName, fontSize, style, unit);
		}
	}

	internal FontExtension(string fontName, float fontSize, FontStyle style, GraphicsUnit unit, FontScriptType scriptType, ref bool hasStylesAndWeights)
	{
		InitializeSkiaSharpProperties(fontName, fontSize, style, unit, ref hasStylesAndWeights);
		if (IsLinuxOS && base.Typeface != null && !Extension.IsHarfBuzzSupportedScript(scriptType))
		{
			InitializeTTFFont(fontName, fontSize, style, unit);
		}
	}

	private void InitializeSkiaSharpProperties(string fontName, float fontSize, FontStyle style, GraphicsUnit unit)
	{
		bool hasStylesAndWeights = true;
		InitializeSkiaSharpProperties(fontName, fontSize, style, unit, ref hasStylesAndWeights);
	}

	private void InitializeSkiaSharpProperties(string fontName, float fontSize, FontStyle style, GraphicsUnit unit, ref bool hasStylesAndWeights)
	{
		base.IsAntialias = true;
		m_fontName = fontName;
		m_fontStyle = style;
		if (TypeFaceCache != null && TypeFaceCache.ContainsKey(Key))
		{
			base.Typeface = TypeFaceCache[Key];
		}
		else
		{
			base.Typeface = GetTypeface(fontName, style, ref hasStylesAndWeights);
			if (base.Typeface == null)
			{
				base.Typeface = GetTypeface("sans-serif", style, ref hasStylesAndWeights);
			}
			if (base.Typeface != null)
			{
				TypeFaceCache.Add(Key, base.Typeface);
			}
		}
		if (unit == GraphicsUnit.Point)
		{
			base.TextSize = fontSize;
		}
		else
		{
			base.TextSize = (float)((double)fontSize * 1.3333333333333333);
		}
	}

	private void InitializeTTFFont(string fontName, float fontSize, FontStyle style, GraphicsUnit unit)
	{
		if (!IsLinuxOS || base.Typeface == null)
		{
			return;
		}
		m_fontFamily = new FontFamily(base.Typeface, fontSize);
		if (TTFFontCache != null && TTFFontCache.ContainsKey(Key))
		{
			TTFFont = TTFFontCache[Key];
		}
		else
		{
			if (!HasFontStream())
			{
				return;
			}
			try
			{
				TTFFont = new TrueTypeFont(FontStream, fontSize, GetFontStyle(style), fontName);
				TTFFontCache.Add(Key, TTFFont);
			}
			catch (Exception ex)
			{
				if (ex.Message == "Can't read TTF font data")
				{
					TTFFont = null;
				}
			}
		}
	}

	internal FontExtension(Stream stream, string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit unit, bool isUnicode)
	{
		base.SubpixelText = true;
		m_fontName = fontName;
		m_fontStyle = fontStyle;
		lock (m_threadLocker)
		{
			stream.Position = 0L;
			if (TTFFontCache != null && TTFFontCache.ContainsKey(Key))
			{
				TTFFont = TTFFontCache[Key];
			}
			else if (stream != null)
			{
				try
				{
					TTFFont = new TrueTypeFont(stream, fontSize, isUnicode, "", GetFontStyle(fontStyle), fontName);
					TTFFontCache.Add(Key, TTFFont);
				}
				catch (Exception ex)
				{
					if (ex.Message == "Can't read TTF font data")
					{
						TTFFont = null;
					}
				}
			}
			if (!TypeFaceCache.ContainsKey(Key))
			{
				stream.Position = 0L;
				SKData sKData = SKData.Create(stream);
				base.Typeface = SKTypeface.FromData(sKData);
				sKData.Dispose();
				stream.Position = 0L;
				if (base.Typeface != null)
				{
					TypeFaceCache.Add(Key, base.Typeface);
				}
			}
			else
			{
				base.Typeface = TypeFaceCache[Key];
			}
			if (unit == GraphicsUnit.Point)
			{
				base.TextSize = fontSize;
			}
			else
			{
				base.TextSize = (float)((double)fontSize * 1.3333333333333333);
			}
			if (IsLinuxOS && base.Typeface != null)
			{
				m_fontFamily = new FontFamily(base.Typeface, fontSize);
			}
		}
	}

	internal SKTypeface GetTypeface(string fontName, FontStyle style)
	{
		bool hasStylesAndWeights = true;
		return GetTypeface(fontName, style, ref hasStylesAndWeights);
	}

	internal SKTypeface GetTypeface(string fontName, FontStyle style, ref bool hasStylesAndWeights)
	{
		SKTypeface sKTypeface = null;
		if ((style & FontStyle.Bold) == FontStyle.Bold)
		{
			sKTypeface = GetTypeface(fontName, SKTypefaceStyle.Bold, ref hasStylesAndWeights);
		}
		if (sKTypeface != null && (style & FontStyle.Italic) == FontStyle.Italic)
		{
			sKTypeface = GetTypeface(fontName, SKTypefaceStyle.BoldItalic, ref hasStylesAndWeights);
		}
		else if ((style & FontStyle.Italic) == FontStyle.Italic)
		{
			sKTypeface = GetTypeface(fontName, SKTypefaceStyle.Italic, ref hasStylesAndWeights);
		}
		else if (sKTypeface == null)
		{
			sKTypeface = GetTypeface(fontName, SKTypefaceStyle.Normal, ref hasStylesAndWeights);
		}
		if (IsLinuxOS)
		{
			if (sKTypeface != null)
			{
				return sKTypeface;
			}
			if (Environment.OSVersion.VersionString.Contains("Unix"))
			{
				DefaultLinuxFont = SKTypeface.FromFamilyName("Tahoma", SKTypefaceStyle.Normal);
				return DefaultLinuxFont;
			}
			if (DefaultLinuxFont == null)
			{
				DefaultLinuxFont = SKTypeface.FromFamilyName("DejaVu Sans", SKTypefaceStyle.Normal);
				return DefaultLinuxFont;
			}
			return DefaultLinuxFont;
		}
		return sKTypeface;
	}

	internal TTFFontStyle GetFontStyle(FontStyle style)
	{
		TTFFontStyle tTFFontStyle = TTFFontStyle.Regular;
		if ((style & FontStyle.Bold) == FontStyle.Bold)
		{
			tTFFontStyle = TTFFontStyle.Bold;
		}
		if ((style & FontStyle.Italic) == FontStyle.Italic)
		{
			tTFFontStyle |= TTFFontStyle.Italic;
		}
		if ((style & FontStyle.Underline) == FontStyle.Underline)
		{
			tTFFontStyle |= TTFFontStyle.Underline;
		}
		if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
		{
			tTFFontStyle |= TTFFontStyle.Strikeout;
		}
		return tTFFontStyle;
	}

	internal FontExtension(Stream stream, string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit unit)
	{
		InitlaizeFontSubsSkiaProperties(stream, fontName, fontSize, fontStyle, unit);
		InitializeFontSubsTTFFont(stream, fontName, fontSize, fontStyle, unit);
		if (base.Typeface != null && !fontName.ToLower().Contains(base.Typeface.FamilyName.ToLower()))
		{
			new FontExtension(stream, base.Typeface.FamilyName, fontSize, fontStyle, GraphicsUnit.Point).Close();
		}
	}

	internal FontExtension(Stream stream, string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit unit, FontScriptType scriptType)
	{
		InitlaizeFontSubsSkiaProperties(stream, fontName, fontSize, fontStyle, unit);
		if (IsLinuxOS && base.Typeface != null && !Extension.IsHarfBuzzSupportedScript(scriptType))
		{
			InitializeFontSubsTTFFont(stream, fontName, fontSize, fontStyle, unit);
		}
		if (base.Typeface != null && !fontName.ToLower().Contains(base.Typeface.FamilyName.ToLower()))
		{
			new FontExtension(stream, base.Typeface.FamilyName, fontSize, fontStyle, GraphicsUnit.Point).Close();
		}
	}

	private void InitlaizeFontSubsSkiaProperties(Stream stream, string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit unit)
	{
		base.SubpixelText = true;
		base.IsAntialias = true;
		m_fontName = fontName;
		m_fontStyle = fontStyle;
		stream.Position = 0L;
		SKData sKData = SKData.Create(stream);
		base.Typeface = SKTypeface.FromData(sKData);
		sKData.Dispose();
		stream.Position = 0L;
		if (!FontStreams.ContainsKey(Key))
		{
			FontStreams.Add(Key, stream);
		}
		if (!TypeFaceCache.ContainsKey(Key) && base.Typeface != null)
		{
			TypeFaceCache.Add(Key, base.Typeface);
		}
		else
		{
			TypeFaceCache[Key].Dispose();
			TypeFaceCache[Key] = base.Typeface;
		}
		if (unit == GraphicsUnit.Point)
		{
			base.TextSize = fontSize;
		}
		else
		{
			base.TextSize = (float)((double)fontSize * 1.3333333333333333);
		}
	}

	private void InitializeFontSubsTTFFont(Stream stream, string fontName, float fontSize, FontStyle fontStyle, GraphicsUnit unit)
	{
		if (IsLinuxOS && base.Typeface != null)
		{
			m_fontFamily = new FontFamily(base.Typeface, fontSize);
			TTFFont = new TrueTypeFont(FontStream, fontSize, GetFontStyle(fontStyle));
			if (TTFFontCache != null && TTFFontCache.ContainsKey(Key))
			{
				TTFFontCache[Key] = TTFFont;
			}
			else
			{
				TTFFontCache.Add(Key, TTFFont);
			}
		}
	}

	internal string GetSKFontName(string fontName, SKTypefaceStyle style, ref SKFontStyleWeight sKFontStyleWeight, ref SKFontStyleSlant sKFontStyleSlant, ref SKFontStyleWidth sKFontStyleWidth)
	{
		ModifyFontName(ref fontName, ref sKFontStyleWeight);
		if (style == SKTypefaceStyle.Italic || style == SKTypefaceStyle.BoldItalic || fontName.Contains("Italic") || fontName.Contains("It"))
		{
			sKFontStyleSlant = SKFontStyleSlant.Italic;
			if (fontName.Contains("Italic"))
			{
				fontName = fontName.Replace("Italic", string.Empty).Trim();
			}
			else if (fontName.Contains("It"))
			{
				fontName = fontName.Replace("It", string.Empty).Trim();
			}
		}
		if (style == SKTypefaceStyle.Bold || style == SKTypefaceStyle.BoldItalic || fontName.Contains("Bold"))
		{
			sKFontStyleWeight = SKFontStyleWeight.Bold;
			if (fontName.Contains("Bold") && !fontName.ToLower().Contains("semibold") && !fontName.ToLower().Contains("extra bold") && !fontName.ToLower().Contains("ultra bold"))
			{
				fontName = fontName.Replace("Bold", string.Empty).Trim();
			}
		}
		if (fontName.ToLower().Contains("semibold") || fontName.Contains("Demi"))
		{
			sKFontStyleWeight = SKFontStyleWeight.SemiBold;
			fontName = ((!fontName.ToLower().Contains("semibold")) ? fontName.Replace("Demi", string.Empty).Trim() : fontName.ToLower().Replace("semibold", string.Empty).Trim());
		}
		else if (fontName.Contains("Extra Bold") || fontName.Contains("Ultra Bold"))
		{
			sKFontStyleWeight = SKFontStyleWeight.ExtraBold;
			fontName = ((!fontName.Contains("Extra Bold")) ? fontName.Replace("Ultra Bold", string.Empty).Trim() : fontName.Replace("Extra Bold", string.Empty).Trim());
		}
		else if (fontName.Contains("Medium"))
		{
			sKFontStyleWeight = SKFontStyleWeight.Medium;
			fontName = fontName.Replace("Medium", string.Empty).Trim();
		}
		else if (fontName.Contains("UltraLight"))
		{
			sKFontStyleWeight = SKFontStyleWeight.ExtraLight;
			fontName = fontName.Replace("UltraLight", string.Empty).Trim();
		}
		else if (fontName.Contains("Regular"))
		{
			fontName = fontName.Replace("Regular", string.Empty).Trim();
		}
		else if (fontName.Contains("Light") || fontName.Contains("Semilight"))
		{
			sKFontStyleWeight = SKFontStyleWeight.Light;
			fontName = ((!fontName.Contains("Light")) ? fontName.Replace("Semilight", string.Empty).Trim() : fontName.Replace("Light", string.Empty).Trim());
		}
		else if (fontName.Contains("Extra Light"))
		{
			sKFontStyleWeight = SKFontStyleWeight.ExtraLight;
			fontName = fontName.Replace("Extra Light", string.Empty).Trim();
		}
		else if (fontName.Contains("Black"))
		{
			sKFontStyleWeight = SKFontStyleWeight.Black;
			fontName = fontName.Replace("Black", string.Empty).Trim();
		}
		else if (fontName.Contains("Extra Black"))
		{
			sKFontStyleWeight = SKFontStyleWeight.ExtraBlack;
			fontName = fontName.Replace("Extra Black", string.Empty).Trim();
		}
		else if (fontName.Contains("Heavy"))
		{
			sKFontStyleWeight = SKFontStyleWeight.ExtraBold;
			fontName = fontName.Replace("Heavy", string.Empty).Trim();
		}
		else if (fontName.Contains("Thin"))
		{
			sKFontStyleWeight = SKFontStyleWeight.Thin;
			fontName = fontName.Replace("Thin", string.Empty).Trim();
		}
		else if (fontName.ToLower().Split(' ').Contains("extended") || fontName.ToLower().Split(' ').Contains("expanded"))
		{
			sKFontStyleWidth = SKFontStyleWidth.Expanded;
			fontName = fontName.ToLower();
			fontName = ((!fontName.Contains("extended")) ? fontName.Replace("expanded", string.Empty).Trim() : fontName.Replace("extended", string.Empty).Trim());
		}
		if (!IsLinuxOS && (fontName.Contains("Condensed") || fontName.Contains("Cond") || fontName.Contains("Cn") || fontName.Contains("Narrow")))
		{
			sKFontStyleWidth = SKFontStyleWidth.Condensed;
			fontName = (fontName.Contains("Condensed") ? fontName.Replace("Condensed", string.Empty).Trim() : (fontName.Contains("Cn") ? fontName.Replace("Cn", string.Empty).Trim() : ((!fontName.Contains("Narrow")) ? fontName.Replace("Cond", string.Empty).Trim() : fontName.Replace("Narrow", string.Empty).Trim())));
		}
		return fontName;
	}

	private void ModifyFontName(ref string fontName, ref SKFontStyleWeight sKFontStyleWeight)
	{
		if (fontName == "Times")
		{
			fontName = "Times New Roman";
		}
		else if (fontName.ToLower().Contains("frutiger lt arabic"))
		{
			if (fontName.ToLower().Contains("45 light"))
			{
				sKFontStyleWeight = SKFontStyleWeight.Light;
			}
			else if (fontName.ToLower().Contains("65 bold"))
			{
				sKFontStyleWeight = SKFontStyleWeight.Bold;
			}
			else if (fontName.ToLower().Contains("75 black"))
			{
				sKFontStyleWeight = SKFontStyleWeight.Black;
			}
			else if (fontName.ToLower().Contains("55 roman"))
			{
				sKFontStyleWeight = SKFontStyleWeight.Normal;
			}
			fontName = "frutiger lt arabic";
		}
	}

	internal SKTypeface GetTypeface(string fontName, SKTypefaceStyle style)
	{
		bool hasStylesAndWeights = true;
		return GetTypeface(fontName, style, ref hasStylesAndWeights);
	}

	internal SKTypeface GetTypeface(string fontName, SKTypefaceStyle style, ref bool hasStylesAndWeights)
	{
		SKFontStyleWeight sKFontStyleWeight = SKFontStyleWeight.Normal;
		SKFontStyleSlant sKFontStyleSlant = SKFontStyleSlant.Upright;
		SKFontStyleWidth sKFontStyleWidth = SKFontStyleWidth.Normal;
		string text = fontName;
		fontName = GetSKFontName(fontName, style, ref sKFontStyleWeight, ref sKFontStyleSlant, ref sKFontStyleWidth);
		SKTypeface sKTypeface = SKTypeface.FromFamilyName(text, sKFontStyleWeight, sKFontStyleWidth, sKFontStyleSlant);
		if (sKTypeface != null && sKTypeface.FamilyName == text)
		{
			return sKTypeface;
		}
		sKTypeface = SKTypeface.FromFamilyName(fontName, sKFontStyleWeight, sKFontStyleWidth, sKFontStyleSlant);
		if (fontName != string.Empty && text != fontName && text.ToLower().Contains(fontName.ToLower()))
		{
			hasStylesAndWeights = ((sKTypeface != null && sKTypeface.FontWidth == (int)sKFontStyleWidth && sKTypeface.FontWeight == (int)sKFontStyleWeight && sKTypeface.FontSlant == sKFontStyleSlant) ? true : false);
		}
		return sKTypeface;
	}

	internal bool IsContainGlyphs(string inputText)
	{
		if (base.Typeface != null)
		{
			using (SKFontManager sKFontManager = SKFontManager.Default)
			{
				for (int i = 0; i < inputText.Length; i++)
				{
					SKTypeface sKTypeface = sKFontManager.MatchCharacter(base.Typeface.FamilyName, inputText[i]);
					if (sKTypeface == null || !(base.Typeface.FamilyName == sKTypeface.FamilyName))
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	private bool HasFontStream()
	{
		if (m_stream == null)
		{
			lock (m_threadLocker)
			{
				if (FontStreams.ContainsKey(Key))
				{
					return true;
				}
				SKStreamAsset sKStreamAsset = base.Typeface.OpenStream();
				return sKStreamAsset != null && sKStreamAsset.Length > 0;
			}
		}
		return true;
	}

	private string GetFontStyleValue(FontStyle fontStyle)
	{
		string text = string.Empty;
		if ((fontStyle & FontStyle.Bold) == FontStyle.Bold)
		{
			text = "Bold";
		}
		if (!string.IsNullOrEmpty(text) && (fontStyle & FontStyle.Italic) == FontStyle.Italic)
		{
			text = "BoldItalic";
		}
		else if ((fontStyle & FontStyle.Italic) == FontStyle.Italic)
		{
			text = "Italic";
		}
		else if (string.IsNullOrEmpty(text))
		{
			text = "Regular";
		}
		return text;
	}

	public SizeF MeasureText(string text, StringFormat format)
	{
		SetTextAlign(format.Alignment);
		return new SizeF(MeasureText(text), Height);
	}

	internal SizeF MeasureText(string text, FontScriptType scriptType)
	{
		float width = 0f;
		if (base.Typeface != null)
		{
			Blob blob = base.Typeface.OpenStream().ToHarfBuzzBlob();
			using Face face = new Face(blob, 0);
			blob.Dispose();
			using HarfBuzzSharp.Font font = new HarfBuzzSharp.Font(face);
			using HarfBuzzSharp.Buffer buffer = new HarfBuzzSharp.Buffer();
			buffer.AddUtf16(text);
			buffer.GuessSegmentProperties();
			font.Shape(buffer);
			font.GetScale(out var xScale, out var _);
			float num = base.TextSize / (float)xScale;
			float num2 = 0f;
			for (int i = 0; i < buffer.GlyphPositions.Length; i++)
			{
				num2 += (float)buffer.GlyphPositions[i].XAdvance;
			}
			width = num2 * num;
		}
		return new SizeF(width, ExactHeight);
	}

	public SizeF MeasureText(string text, StringFormat format, Font font)
	{
		SKRect bounds = default(SKRect);
		SetTextAlign(format.Alignment);
		float width = 0f;
		if (text != string.Empty)
		{
			width = MeasureText(text, ref bounds);
		}
		return new SizeF(width, ExactHeight);
	}

	public void SetTextAlign(StringAlignment align)
	{
		switch (align)
		{
		case StringAlignment.Center:
			base.TextAlign = SKTextAlign.Center;
			break;
		case StringAlignment.Far:
			base.TextAlign = SKTextAlign.Right;
			break;
		default:
			base.TextAlign = SKTextAlign.Left;
			break;
		}
	}

	public new void Dispose()
	{
		base.Dispose();
		if (typeFaceCache != null)
		{
			foreach (KeyValuePair<string, SKTypeface> item in typeFaceCache)
			{
				item.Value.Dispose();
			}
			typeFaceCache.Clear();
			typeFaceCache = null;
		}
		if (fontStreams != null)
		{
			foreach (KeyValuePair<string, Stream> fontStream in fontStreams)
			{
				fontStream.Value.Dispose();
			}
			fontStreams.Clear();
			fontStreams = null;
		}
		if (IsLinuxOS && ttfFontCache != null)
		{
			foreach (string key in ttfFontCache.Keys)
			{
				TrueTypeFont trueTypeFont = ttfFontCache[key];
				if (trueTypeFont != null)
				{
					trueTypeFont.Dispose();
					trueTypeFont = null;
				}
			}
			ttfFontCache.Clear();
			ttfFontCache = null;
		}
		if (DefaultLinuxFont != null)
		{
			DefaultLinuxFont.Dispose();
			DefaultLinuxFont = null;
		}
		SKGraphics.PurgeFontCache();
	}

	public void Close()
	{
		base.Dispose();
	}

	public float GetCellAscent(FontStyle style)
	{
		if (IsLinuxOS && TTFFontCache != null && TTFFontCache.ContainsKey(Key))
		{
			TTFFont = TTFFontCache[Key];
			if (TTFFont != null)
			{
				TTFFont.Size = Size;
				return TTFFont.Metrics.GetAscent(null);
			}
			return 0f;
		}
		return Math.Abs(base.FontMetrics.Ascent);
	}

	public float GetCellLeading(FontStyle style)
	{
		if (IsLinuxOS)
		{
			if (TTFFontCache != null && TTFFontCache.ContainsKey(Key))
			{
				TTFFont = TTFFontCache[Key];
			}
			if (TTFFont != null)
			{
				TTFFont.Size = Size;
				return TTFFont.Metrics.GetLineGap(null);
			}
		}
		return 0f;
	}

	public float GetCellDescent(FontStyle style)
	{
		if (IsLinuxOS && TTFFontCache != null && TTFFontCache.ContainsKey(Key))
		{
			TTFFont = TTFFontCache[Key];
			if (TTFFont != null)
			{
				TTFFont.Size = Size;
				return TTFFont.Metrics.GetDescent(null);
			}
			return 0f;
		}
		return Math.Abs(base.FontMetrics.Descent);
	}

	public float GetLineSpacing(FontStyle style)
	{
		SKFontMetrics metrics = default(SKFontMetrics);
		return GetFontMetrics(out metrics);
	}

	public float GetEmHeight(FontStyle style)
	{
		return base.Typeface.UnitsPerEm;
	}
}
