using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKPaint : SKObject, ISKSkipObjectRegistration
{
	private SKFont font;

	private bool lcdRenderText;

	public bool IsAntialias
	{
		get
		{
			return SkiaApi.sk_paint_is_antialias(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_antialias(Handle, value);
			UpdateFontEdging(value);
		}
	}

	public bool IsDither
	{
		get
		{
			return SkiaApi.sk_paint_is_dither(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_dither(Handle, value);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public bool IsVerticalText
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool IsLinearText
	{
		get
		{
			return GetFont().LinearMetrics;
		}
		set
		{
			GetFont().LinearMetrics = value;
		}
	}

	public bool SubpixelText
	{
		get
		{
			return GetFont().Subpixel;
		}
		set
		{
			GetFont().Subpixel = value;
		}
	}

	public bool LcdRenderText
	{
		get
		{
			return lcdRenderText;
		}
		set
		{
			lcdRenderText = value;
			UpdateFontEdging(IsAntialias);
		}
	}

	public bool IsEmbeddedBitmapText
	{
		get
		{
			return GetFont().EmbeddedBitmaps;
		}
		set
		{
			GetFont().EmbeddedBitmaps = value;
		}
	}

	public bool IsAutohinted
	{
		get
		{
			return GetFont().ForceAutoHinting;
		}
		set
		{
			GetFont().ForceAutoHinting = value;
		}
	}

	public SKPaintHinting HintingLevel
	{
		get
		{
			return (SKPaintHinting)GetFont().Hinting;
		}
		set
		{
			GetFont().Hinting = (SKFontHinting)value;
		}
	}

	public bool FakeBoldText
	{
		get
		{
			return GetFont().Embolden;
		}
		set
		{
			GetFont().Embolden = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public bool DeviceKerningEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool IsStroke
	{
		get
		{
			return Style != SKPaintStyle.Fill;
		}
		set
		{
			Style = (value ? SKPaintStyle.Stroke : SKPaintStyle.Fill);
		}
	}

	public SKPaintStyle Style
	{
		get
		{
			return SkiaApi.sk_paint_get_style(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_style(Handle, value);
		}
	}

	public SKColor Color
	{
		get
		{
			return SkiaApi.sk_paint_get_color(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_color(Handle, (uint)value);
		}
	}

	public unsafe SKColorF ColorF
	{
		get
		{
			SKColorF result = default(SKColorF);
			SkiaApi.sk_paint_get_color4f(Handle, &result);
			return result;
		}
		set
		{
			SkiaApi.sk_paint_set_color4f(Handle, &value, IntPtr.Zero);
		}
	}

	public float StrokeWidth
	{
		get
		{
			return SkiaApi.sk_paint_get_stroke_width(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_stroke_width(Handle, value);
		}
	}

	public float StrokeMiter
	{
		get
		{
			return SkiaApi.sk_paint_get_stroke_miter(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_stroke_miter(Handle, value);
		}
	}

	public SKStrokeCap StrokeCap
	{
		get
		{
			return SkiaApi.sk_paint_get_stroke_cap(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_stroke_cap(Handle, value);
		}
	}

	public SKStrokeJoin StrokeJoin
	{
		get
		{
			return SkiaApi.sk_paint_get_stroke_join(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_stroke_join(Handle, value);
		}
	}

	public SKShader Shader
	{
		get
		{
			return SKShader.GetObject(SkiaApi.sk_paint_get_shader(Handle));
		}
		set
		{
			SkiaApi.sk_paint_set_shader(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public SKMaskFilter MaskFilter
	{
		get
		{
			return SKMaskFilter.GetObject(SkiaApi.sk_paint_get_maskfilter(Handle));
		}
		set
		{
			SkiaApi.sk_paint_set_maskfilter(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public SKColorFilter ColorFilter
	{
		get
		{
			return SKColorFilter.GetObject(SkiaApi.sk_paint_get_colorfilter(Handle));
		}
		set
		{
			SkiaApi.sk_paint_set_colorfilter(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public SKImageFilter ImageFilter
	{
		get
		{
			return SKImageFilter.GetObject(SkiaApi.sk_paint_get_imagefilter(Handle));
		}
		set
		{
			SkiaApi.sk_paint_set_imagefilter(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public SKBlendMode BlendMode
	{
		get
		{
			return SkiaApi.sk_paint_get_blendmode(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_blendmode(Handle, value);
		}
	}

	public SKFilterQuality FilterQuality
	{
		get
		{
			return SkiaApi.sk_paint_get_filter_quality(Handle);
		}
		set
		{
			SkiaApi.sk_paint_set_filter_quality(Handle, value);
		}
	}

	public SKTypeface Typeface
	{
		get
		{
			return GetFont().Typeface;
		}
		set
		{
			GetFont().Typeface = value;
		}
	}

	public float TextSize
	{
		get
		{
			return GetFont().Size;
		}
		set
		{
			GetFont().Size = value;
		}
	}

	public SKTextAlign TextAlign
	{
		get
		{
			return SkiaApi.sk_compatpaint_get_text_align(Handle);
		}
		set
		{
			SkiaApi.sk_compatpaint_set_text_align(Handle, value);
		}
	}

	public SKTextEncoding TextEncoding
	{
		get
		{
			return SkiaApi.sk_compatpaint_get_text_encoding(Handle);
		}
		set
		{
			SkiaApi.sk_compatpaint_set_text_encoding(Handle, value);
		}
	}

	public float TextScaleX
	{
		get
		{
			return GetFont().ScaleX;
		}
		set
		{
			GetFont().ScaleX = value;
		}
	}

	public float TextSkewX
	{
		get
		{
			return GetFont().SkewX;
		}
		set
		{
			GetFont().SkewX = value;
		}
	}

	public SKPathEffect PathEffect
	{
		get
		{
			return SKPathEffect.GetObject(SkiaApi.sk_paint_get_path_effect(Handle));
		}
		set
		{
			SkiaApi.sk_paint_set_path_effect(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public float FontSpacing => GetFont().Spacing;

	public SKFontMetrics FontMetrics => GetFont().Metrics;

	internal SKPaint(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPaint()
		: this(SkiaApi.sk_compatpaint_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPaint instance.");
		}
	}

	public SKPaint(SKFont font)
		: this(IntPtr.Zero, owns: true)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		Handle = SkiaApi.sk_compatpaint_new_with_font(font.Handle);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPaint instance.");
		}
		LcdRenderText = font.Edging == SKFontEdging.SubpixelAntialias;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_compatpaint_delete(Handle);
	}

	public void Reset()
	{
		SkiaApi.sk_compatpaint_reset(Handle);
	}

	public unsafe void SetColor(SKColorF color, SKColorSpace colorspace)
	{
		SkiaApi.sk_paint_set_color4f(Handle, &color, colorspace?.Handle ?? IntPtr.Zero);
	}

	public float GetFontMetrics(out SKFontMetrics metrics)
	{
		return GetFont().GetFontMetrics(out metrics);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetFontMetrics (out SKFontMetrics) instead.")]
	public float GetFontMetrics(out SKFontMetrics metrics, float scale)
	{
		return GetFontMetrics(out metrics);
	}

	public SKPaint Clone()
	{
		return GetObject(SkiaApi.sk_compatpaint_clone(Handle));
	}

	public float MeasureText(string text)
	{
		return GetFont().MeasureText(text, this);
	}

	public float MeasureText(ReadOnlySpan<char> text)
	{
		return GetFont().MeasureText(text, this);
	}

	public float MeasureText(byte[] text)
	{
		return GetFont().MeasureText(text, TextEncoding, this);
	}

	public float MeasureText(ReadOnlySpan<byte> text)
	{
		return GetFont().MeasureText(text, TextEncoding, this);
	}

	public float MeasureText(IntPtr buffer, int length)
	{
		return GetFont().MeasureText(buffer, length, TextEncoding, this);
	}

	public float MeasureText(IntPtr buffer, IntPtr length)
	{
		return GetFont().MeasureText(buffer, (int)length, TextEncoding, this);
	}

	public float MeasureText(string text, ref SKRect bounds)
	{
		return GetFont().MeasureText(text, out bounds, this);
	}

	public float MeasureText(ReadOnlySpan<char> text, ref SKRect bounds)
	{
		return GetFont().MeasureText(text, out bounds, this);
	}

	public float MeasureText(byte[] text, ref SKRect bounds)
	{
		return GetFont().MeasureText(text, TextEncoding, out bounds, this);
	}

	public float MeasureText(ReadOnlySpan<byte> text, ref SKRect bounds)
	{
		return GetFont().MeasureText(text, TextEncoding, out bounds, this);
	}

	public float MeasureText(IntPtr buffer, int length, ref SKRect bounds)
	{
		return GetFont().MeasureText(buffer, length, TextEncoding, out bounds, this);
	}

	public float MeasureText(IntPtr buffer, IntPtr length, ref SKRect bounds)
	{
		return GetFont().MeasureText(buffer, (int)length, TextEncoding, out bounds, this);
	}

	public long BreakText(string text, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(text, maxWidth, out measuredWidth, this);
	}

	public long BreakText(string text, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(text, maxWidth, out measuredWidth, this);
	}

	public long BreakText(string text, float maxWidth, out float measuredWidth, out string measuredText)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		int num = GetFont().BreakText(text, maxWidth, out measuredWidth, this);
		if (num == 0)
		{
			measuredText = string.Empty;
			return 0L;
		}
		if (num == text.Length)
		{
			measuredText = text;
			return text.Length;
		}
		measuredText = text.Substring(0, num);
		return num;
	}

	public long BreakText(ReadOnlySpan<char> text, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(text, maxWidth, out measuredWidth, this);
	}

	public long BreakText(ReadOnlySpan<char> text, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(text, maxWidth, out measuredWidth, this);
	}

	public long BreakText(byte[] text, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(text, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(byte[] text, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(text, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(ReadOnlySpan<byte> text, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(text, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(text, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(IntPtr buffer, int length, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(buffer, length, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(IntPtr buffer, int length, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(buffer, length, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(IntPtr buffer, IntPtr length, float maxWidth)
	{
		float measuredWidth;
		return GetFont().BreakText(buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public long BreakText(IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth)
	{
		return GetFont().BreakText(buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);
	}

	public SKPath GetTextPath(string text, float x, float y)
	{
		return GetFont().GetTextPath(text, new SKPoint(x, y));
	}

	public SKPath GetTextPath(ReadOnlySpan<char> text, float x, float y)
	{
		return GetFont().GetTextPath(text, new SKPoint(x, y));
	}

	public SKPath GetTextPath(byte[] text, float x, float y)
	{
		return GetFont().GetTextPath(text, TextEncoding, new SKPoint(x, y));
	}

	public SKPath GetTextPath(ReadOnlySpan<byte> text, float x, float y)
	{
		return GetFont().GetTextPath(text, TextEncoding, new SKPoint(x, y));
	}

	public SKPath GetTextPath(IntPtr buffer, int length, float x, float y)
	{
		return GetFont().GetTextPath(buffer, length, TextEncoding, new SKPoint(x, y));
	}

	public SKPath GetTextPath(IntPtr buffer, IntPtr length, float x, float y)
	{
		return GetFont().GetTextPath(buffer, (int)length, TextEncoding, new SKPoint(x, y));
	}

	public SKPath GetTextPath(string text, SKPoint[] points)
	{
		return GetFont().GetTextPath(text, points);
	}

	public SKPath GetTextPath(ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> points)
	{
		return GetFont().GetTextPath(text, points);
	}

	public SKPath GetTextPath(byte[] text, SKPoint[] points)
	{
		return GetFont().GetTextPath(text, TextEncoding, points);
	}

	public SKPath GetTextPath(ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> points)
	{
		return GetFont().GetTextPath(text, TextEncoding, points);
	}

	public SKPath GetTextPath(IntPtr buffer, int length, SKPoint[] points)
	{
		return GetFont().GetTextPath(buffer, length, TextEncoding, points);
	}

	public SKPath GetTextPath(IntPtr buffer, int length, ReadOnlySpan<SKPoint> points)
	{
		return GetFont().GetTextPath(buffer, length, TextEncoding, points);
	}

	public SKPath GetTextPath(IntPtr buffer, IntPtr length, SKPoint[] points)
	{
		return GetFont().GetTextPath(buffer, (int)length, TextEncoding, points);
	}

	public SKPath GetFillPath(SKPath src)
	{
		return GetFillPath(src, 1f);
	}

	public SKPath GetFillPath(SKPath src, float resScale)
	{
		SKPath sKPath = new SKPath();
		if (GetFillPath(src, sKPath, resScale))
		{
			return sKPath;
		}
		sKPath.Dispose();
		return null;
	}

	public SKPath GetFillPath(SKPath src, SKRect cullRect)
	{
		return GetFillPath(src, cullRect, 1f);
	}

	public SKPath GetFillPath(SKPath src, SKRect cullRect, float resScale)
	{
		SKPath sKPath = new SKPath();
		if (GetFillPath(src, sKPath, cullRect, resScale))
		{
			return sKPath;
		}
		sKPath.Dispose();
		return null;
	}

	public bool GetFillPath(SKPath src, SKPath dst)
	{
		return GetFillPath(src, dst, 1f);
	}

	public unsafe bool GetFillPath(SKPath src, SKPath dst, float resScale)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_paint_get_fill_path(Handle, src.Handle, dst.Handle, null, resScale);
	}

	public bool GetFillPath(SKPath src, SKPath dst, SKRect cullRect)
	{
		return GetFillPath(src, dst, cullRect, 1f);
	}

	public unsafe bool GetFillPath(SKPath src, SKPath dst, SKRect cullRect, float resScale)
	{
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_paint_get_fill_path(Handle, src.Handle, dst.Handle, &cullRect, resScale);
	}

	public int CountGlyphs(string text)
	{
		return GetFont().CountGlyphs(text);
	}

	public int CountGlyphs(ReadOnlySpan<char> text)
	{
		return GetFont().CountGlyphs(text);
	}

	public int CountGlyphs(byte[] text)
	{
		return GetFont().CountGlyphs(text, TextEncoding);
	}

	public int CountGlyphs(ReadOnlySpan<byte> text)
	{
		return GetFont().CountGlyphs(text, TextEncoding);
	}

	public int CountGlyphs(IntPtr text, int length)
	{
		return GetFont().CountGlyphs(text, length, TextEncoding);
	}

	public int CountGlyphs(IntPtr text, IntPtr length)
	{
		return GetFont().CountGlyphs(text, (int)length, TextEncoding);
	}

	public ushort[] GetGlyphs(string text)
	{
		return GetFont().GetGlyphs(text);
	}

	public ushort[] GetGlyphs(ReadOnlySpan<char> text)
	{
		return GetFont().GetGlyphs(text);
	}

	public ushort[] GetGlyphs(byte[] text)
	{
		return GetFont().GetGlyphs(text, TextEncoding);
	}

	public ushort[] GetGlyphs(ReadOnlySpan<byte> text)
	{
		return GetFont().GetGlyphs(text, TextEncoding);
	}

	public ushort[] GetGlyphs(IntPtr text, int length)
	{
		return GetFont().GetGlyphs(text, length, TextEncoding);
	}

	public ushort[] GetGlyphs(IntPtr text, IntPtr length)
	{
		return GetFont().GetGlyphs(text, (int)length, TextEncoding);
	}

	public bool ContainsGlyphs(string text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(ReadOnlySpan<char> text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(byte[] text)
	{
		return GetFont().ContainsGlyphs(text, TextEncoding);
	}

	public bool ContainsGlyphs(ReadOnlySpan<byte> text)
	{
		return GetFont().ContainsGlyphs(text, TextEncoding);
	}

	public bool ContainsGlyphs(IntPtr text, int length)
	{
		return GetFont().ContainsGlyphs(text, length, TextEncoding);
	}

	public bool ContainsGlyphs(IntPtr text, IntPtr length)
	{
		return GetFont().ContainsGlyphs(text, (int)length, TextEncoding);
	}

	public SKPoint[] GetGlyphPositions(string text, SKPoint origin = default(SKPoint))
	{
		return GetFont().GetGlyphPositions(text, origin);
	}

	public SKPoint[] GetGlyphPositions(ReadOnlySpan<char> text, SKPoint origin = default(SKPoint))
	{
		return GetFont().GetGlyphPositions(text, origin);
	}

	public SKPoint[] GetGlyphPositions(ReadOnlySpan<byte> text, SKPoint origin = default(SKPoint))
	{
		return GetFont().GetGlyphPositions(text, TextEncoding, origin);
	}

	public SKPoint[] GetGlyphPositions(IntPtr text, int length, SKPoint origin = default(SKPoint))
	{
		return GetFont().GetGlyphPositions(text, length, TextEncoding, origin);
	}

	public float[] GetGlyphOffsets(string text, float origin = 0f)
	{
		return GetFont().GetGlyphOffsets(text, origin);
	}

	public float[] GetGlyphOffsets(ReadOnlySpan<char> text, float origin = 0f)
	{
		return GetFont().GetGlyphOffsets(text, origin);
	}

	public float[] GetGlyphOffsets(ReadOnlySpan<byte> text, float origin = 0f)
	{
		return GetFont().GetGlyphOffsets(text, TextEncoding, origin);
	}

	public float[] GetGlyphOffsets(IntPtr text, int length, float origin = 0f)
	{
		return GetFont().GetGlyphOffsets(text, length, TextEncoding, origin);
	}

	public float[] GetGlyphWidths(string text)
	{
		return GetFont().GetGlyphWidths(text, this);
	}

	public float[] GetGlyphWidths(ReadOnlySpan<char> text)
	{
		return GetFont().GetGlyphWidths(text, this);
	}

	public float[] GetGlyphWidths(byte[] text)
	{
		return GetFont().GetGlyphWidths(text, TextEncoding, this);
	}

	public float[] GetGlyphWidths(ReadOnlySpan<byte> text)
	{
		return GetFont().GetGlyphWidths(text, TextEncoding, this);
	}

	public float[] GetGlyphWidths(IntPtr text, int length)
	{
		return GetFont().GetGlyphWidths(text, length, TextEncoding, this);
	}

	public float[] GetGlyphWidths(IntPtr text, IntPtr length)
	{
		return GetFont().GetGlyphWidths(text, (int)length, TextEncoding, this);
	}

	public float[] GetGlyphWidths(string text, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, out bounds, this);
	}

	public float[] GetGlyphWidths(ReadOnlySpan<char> text, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, out bounds, this);
	}

	public float[] GetGlyphWidths(byte[] text, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, TextEncoding, out bounds, this);
	}

	public float[] GetGlyphWidths(ReadOnlySpan<byte> text, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, TextEncoding, out bounds, this);
	}

	public float[] GetGlyphWidths(IntPtr text, int length, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, length, TextEncoding, out bounds, this);
	}

	public float[] GetGlyphWidths(IntPtr text, IntPtr length, out SKRect[] bounds)
	{
		return GetFont().GetGlyphWidths(text, (int)length, TextEncoding, out bounds, this);
	}

	public float[] GetTextIntercepts(string text, float x, float y, float upperBounds, float lowerBounds)
	{
		return GetTextIntercepts(text.AsSpan(), x, y, upperBounds, lowerBounds);
	}

	public float[] GetTextIntercepts(ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, GetFont(), new SKPoint(x, y));
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetTextIntercepts(byte[] text, float x, float y, float upperBounds, float lowerBounds)
	{
		return GetTextIntercepts(text.AsSpan(), x, y, upperBounds, lowerBounds);
	}

	public float[] GetTextIntercepts(ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, TextEncoding, GetFont(), new SKPoint(x, y));
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetTextIntercepts(IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds)
	{
		return GetTextIntercepts(text, (int)length, x, y, upperBounds, lowerBounds);
	}

	public float[] GetTextIntercepts(IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
	{
		if (text == IntPtr.Zero && length != 0)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, length, TextEncoding, GetFont(), new SKPoint(x, y));
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetTextIntercepts(SKTextBlob text, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return text.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetPositionedTextIntercepts(string text, SKPoint[] positions, float upperBounds, float lowerBounds)
	{
		return GetPositionedTextIntercepts(text.AsSpan(), positions, upperBounds, lowerBounds);
	}

	public float[] GetPositionedTextIntercepts(ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(text, GetFont(), positions);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetPositionedTextIntercepts(byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds)
	{
		return GetPositionedTextIntercepts(text.AsSpan(), positions, upperBounds, lowerBounds);
	}

	public float[] GetPositionedTextIntercepts(ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(text, TextEncoding, GetFont(), positions);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetPositionedTextIntercepts(IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds)
	{
		return GetPositionedTextIntercepts(text, (IntPtr)length, positions, upperBounds, lowerBounds);
	}

	public float[] GetPositionedTextIntercepts(IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
	{
		if (text == IntPtr.Zero && length != IntPtr.Zero)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePositioned(text, (int)length, TextEncoding, GetFont(), positions);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetHorizontalTextIntercepts(string text, float[] xpositions, float y, float upperBounds, float lowerBounds)
	{
		return GetHorizontalTextIntercepts(text.AsSpan(), xpositions, y, upperBounds, lowerBounds);
	}

	public float[] GetHorizontalTextIntercepts(ReadOnlySpan<char> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreateHorizontal(text, GetFont(), xpositions, y);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetHorizontalTextIntercepts(byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds)
	{
		return GetHorizontalTextIntercepts(text.AsSpan(), xpositions, y, upperBounds, lowerBounds);
	}

	public float[] GetHorizontalTextIntercepts(ReadOnlySpan<byte> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreateHorizontal(text, TextEncoding, GetFont(), xpositions, y);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public float[] GetHorizontalTextIntercepts(IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds)
	{
		return GetHorizontalTextIntercepts(text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);
	}

	public float[] GetHorizontalTextIntercepts(IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
	{
		if (text == IntPtr.Zero && length != IntPtr.Zero)
		{
			throw new ArgumentNullException("text");
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreateHorizontal(text, (int)length, TextEncoding, GetFont(), xpositions, y);
		return sKTextBlob.GetIntercepts(upperBounds, lowerBounds, this);
	}

	public SKFont ToFont()
	{
		return SKFont.GetObject(SkiaApi.sk_compatpaint_make_font(Handle));
	}

	internal SKFont GetFont()
	{
		return font ?? (font = SKObject.OwnedBy(SKFont.GetObject(SkiaApi.sk_compatpaint_get_font(Handle), owns: false), this));
	}

	private void UpdateFontEdging(bool antialias)
	{
		SKFontEdging edging = SKFontEdging.Alias;
		if (antialias)
		{
			edging = ((!lcdRenderText) ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias);
		}
		GetFont().Edging = edging;
	}

	internal static SKPaint GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKPaint(handle, owns: true);
		}
		return null;
	}
}
