using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKFont : SKObject
{
	internal sealed class GlyphPathCache : Dictionary<ushort, SKPath>, IDisposable
	{
		public SKFont Font { get; }

		public GlyphPathCache(SKFont font)
		{
			Font = font;
		}

		public SKPath GetPath(ushort glyph)
		{
			if (!TryGetValue(glyph, out var value))
			{
				value = (base[glyph] = Font.GetGlyphPath(glyph));
			}
			return value;
		}

		public void Dispose()
		{
			foreach (SKPath value in base.Values)
			{
				value?.Dispose();
			}
			Clear();
		}
	}

	internal const float DefaultSize = 12f;

	internal const float DefaultScaleX = 1f;

	internal const float DefaultSkewX = 0f;

	public bool ForceAutoHinting
	{
		get
		{
			return SkiaApi.sk_font_is_force_auto_hinting(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_force_auto_hinting(Handle, value);
		}
	}

	public bool EmbeddedBitmaps
	{
		get
		{
			return SkiaApi.sk_font_is_embedded_bitmaps(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_embedded_bitmaps(Handle, value);
		}
	}

	public bool Subpixel
	{
		get
		{
			return SkiaApi.sk_font_is_subpixel(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_subpixel(Handle, value);
		}
	}

	public bool LinearMetrics
	{
		get
		{
			return SkiaApi.sk_font_is_linear_metrics(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_linear_metrics(Handle, value);
		}
	}

	public bool Embolden
	{
		get
		{
			return SkiaApi.sk_font_is_embolden(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_embolden(Handle, value);
		}
	}

	public bool BaselineSnap
	{
		get
		{
			return SkiaApi.sk_font_is_baseline_snap(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_baseline_snap(Handle, value);
		}
	}

	public SKFontEdging Edging
	{
		get
		{
			return SkiaApi.sk_font_get_edging(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_edging(Handle, value);
		}
	}

	public SKFontHinting Hinting
	{
		get
		{
			return SkiaApi.sk_font_get_hinting(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_hinting(Handle, value);
		}
	}

	public SKTypeface Typeface
	{
		get
		{
			return SKTypeface.GetObject(SkiaApi.sk_font_get_typeface(Handle));
		}
		set
		{
			SkiaApi.sk_font_set_typeface(Handle, value?.Handle ?? IntPtr.Zero);
		}
	}

	public float Size
	{
		get
		{
			return SkiaApi.sk_font_get_size(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_size(Handle, value);
		}
	}

	public float ScaleX
	{
		get
		{
			return SkiaApi.sk_font_get_scale_x(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_scale_x(Handle, value);
		}
	}

	public float SkewX
	{
		get
		{
			return SkiaApi.sk_font_get_skew_x(Handle);
		}
		set
		{
			SkiaApi.sk_font_set_skew_x(Handle, value);
		}
	}

	public unsafe float Spacing => SkiaApi.sk_font_get_metrics(Handle, null);

	public SKFontMetrics Metrics
	{
		get
		{
			GetFontMetrics(out var metrics);
			return metrics;
		}
	}

	internal SKFont(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKFont()
		: this(SkiaApi.sk_font_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKFont instance.");
		}
	}

	public SKFont(SKTypeface typeface, float size = 12f, float scaleX = 1f, float skewX = 0f)
		: this(SkiaApi.sk_font_new_with_values(typeface?.Handle ?? IntPtr.Zero, size, scaleX, skewX), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKFont instance.");
		}
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_font_delete(Handle);
	}

	public unsafe float GetFontMetrics(out SKFontMetrics metrics)
	{
		fixed (SKFontMetrics* metrics2 = &metrics)
		{
			return SkiaApi.sk_font_get_metrics(Handle, metrics2);
		}
	}

	public ushort GetGlyph(int codepoint)
	{
		return SkiaApi.sk_font_unichar_to_glyph(Handle, codepoint);
	}

	internal ushort[] GetGlyphs(ReadOnlySpan<int> codepoints)
	{
		ushort[] array = new ushort[codepoints.Length];
		GetGlyphs(codepoints, array);
		return array;
	}

	public unsafe void GetGlyphs(ReadOnlySpan<int> codepoints, Span<ushort> glyphs)
	{
		if (codepoints.IsEmpty)
		{
			return;
		}
		if (glyphs.Length != codepoints.Length)
		{
			throw new ArgumentException("The length of glyphs must be the same as the length of codepoints.", "glyphs");
		}
		fixed (int* uni = codepoints)
		{
			fixed (ushort* glyphs2 = glyphs)
			{
				SkiaApi.sk_font_unichars_to_glyphs(Handle, uni, codepoints.Length, glyphs2);
			}
		}
	}

	internal ushort[] GetGlyphs(string text)
	{
		return GetGlyphs(text.AsSpan());
	}

	internal unsafe ushort[] GetGlyphs(ReadOnlySpan<char> text)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphs(text2, text.Length * 2, SKTextEncoding.Utf16);
		}
	}

	internal unsafe ushort[] GetGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphs(text2, text.Length, encoding);
		}
	}

	internal unsafe ushort[] GetGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		return GetGlyphs((void*)text, length, encoding);
	}

	public void GetGlyphs(string text, Span<ushort> glyphs)
	{
		GetGlyphs(text.AsSpan(), glyphs);
	}

	public unsafe void GetGlyphs(ReadOnlySpan<char> text, Span<ushort> glyphs)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphs(text2, text.Length * 2, SKTextEncoding.Utf16, glyphs);
		}
	}

	public unsafe void GetGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<ushort> glyphs)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphs(text2, text.Length, encoding, glyphs);
		}
	}

	public unsafe void GetGlyphs(IntPtr text, int length, SKTextEncoding encoding, Span<ushort> glyphs)
	{
		GetGlyphs((void*)text, length, encoding, glyphs);
	}

	internal unsafe ushort[] GetGlyphs(void* text, int length, SKTextEncoding encoding)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new ushort[0];
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return new ushort[0];
		}
		ushort[] array = new ushort[num];
		GetGlyphs(text, length, encoding, array);
		return array;
	}

	internal unsafe void GetGlyphs(void* text, int length, SKTextEncoding encoding, Span<ushort> glyphs)
	{
		if (ValidateTextArgs(text, length, encoding))
		{
			fixed (ushort* glyphs2 = glyphs)
			{
				SkiaApi.sk_font_text_to_glyphs(Handle, text, (IntPtr)length, encoding, glyphs2, glyphs.Length);
			}
		}
	}

	public bool ContainsGlyph(int codepoint)
	{
		return GetGlyph(codepoint) != 0;
	}

	public bool ContainsGlyphs(ReadOnlySpan<int> codepoints)
	{
		return ContainsGlyphs(GetGlyphs(codepoints));
	}

	public bool ContainsGlyphs(string text)
	{
		return ContainsGlyphs(GetGlyphs(text));
	}

	public bool ContainsGlyphs(ReadOnlySpan<char> text)
	{
		return ContainsGlyphs(GetGlyphs(text));
	}

	public bool ContainsGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		return ContainsGlyphs(GetGlyphs(text, encoding));
	}

	public bool ContainsGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		return ContainsGlyphs(GetGlyphs(text, length, encoding));
	}

	private bool ContainsGlyphs(ushort[] glyphs)
	{
		return Array.IndexOf(glyphs, (ushort)0) == -1;
	}

	public int CountGlyphs(string text)
	{
		return CountGlyphs(text.AsSpan());
	}

	public unsafe int CountGlyphs(ReadOnlySpan<char> text)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return CountGlyphs(text2, text.Length * 2, SKTextEncoding.Utf16);
		}
	}

	public unsafe int CountGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return CountGlyphs(text2, text.Length, encoding);
		}
	}

	public unsafe int CountGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		return CountGlyphs((void*)text, length, encoding);
	}

	internal unsafe int CountGlyphs(void* text, int length, SKTextEncoding encoding)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return 0;
		}
		return SkiaApi.sk_font_text_to_glyphs(Handle, text, (IntPtr)length, encoding, null, 0);
	}

	internal float MeasureText(string text, SKPaint paint = null)
	{
		return MeasureText(text.AsSpan(), paint);
	}

	internal unsafe float MeasureText(ReadOnlySpan<char> text, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return MeasureText(text2, text.Length * 2, SKTextEncoding.Utf16, null, paint);
		}
	}

	internal unsafe float MeasureText(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return MeasureText(text2, text.Length, encoding, null, paint);
		}
	}

	internal unsafe float MeasureText(IntPtr text, int length, SKTextEncoding encoding, SKPaint paint = null)
	{
		return MeasureText((void*)text, length, encoding, null, paint);
	}

	internal float MeasureText(string text, out SKRect bounds, SKPaint paint = null)
	{
		return MeasureText(text.AsSpan(), out bounds, paint);
	}

	internal unsafe float MeasureText(ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			fixed (SKRect* bounds2 = &bounds)
			{
				return MeasureText(text2, text.Length * 2, SKTextEncoding.Utf16, bounds2, paint);
			}
		}
	}

	internal unsafe float MeasureText(ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			fixed (SKRect* bounds2 = &bounds)
			{
				return MeasureText(text2, text.Length, encoding, bounds2, paint);
			}
		}
	}

	internal unsafe float MeasureText(IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint = null)
	{
		fixed (SKRect* bounds2 = &bounds)
		{
			return MeasureText((void*)text, length, encoding, bounds2, paint);
		}
	}

	internal unsafe float MeasureText(void* text, int length, SKTextEncoding encoding, SKRect* bounds, SKPaint paint)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return 0f;
		}
		float result = default(float);
		SkiaApi.sk_font_measure_text_no_return(Handle, text, (IntPtr)length, encoding, bounds, paint?.Handle ?? IntPtr.Zero, &result);
		return result;
	}

	public unsafe float MeasureText(ReadOnlySpan<ushort> glyphs, SKPaint paint = null)
	{
		fixed (ushort* text = glyphs)
		{
			return MeasureText(text, glyphs.Length * 2, SKTextEncoding.GlyphId, null, paint);
		}
	}

	public unsafe float MeasureText(ReadOnlySpan<ushort> glyphs, out SKRect bounds, SKPaint paint = null)
	{
		fixed (ushort* text = glyphs)
		{
			fixed (SKRect* bounds2 = &bounds)
			{
				return MeasureText(text, glyphs.Length * 2, SKTextEncoding.GlyphId, bounds2, paint);
			}
		}
	}

	internal int BreakText(string text, float maxWidth, out float measuredWidth, SKPaint paint = null)
	{
		return BreakText(text.AsSpan(), maxWidth, out measuredWidth, paint);
	}

	internal unsafe int BreakText(ReadOnlySpan<char> text, float maxWidth, out float measuredWidth, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			fixed (float* measuredWidth2 = &measuredWidth)
			{
				int num = BreakText(text2, text.Length * 2, SKTextEncoding.Utf16, maxWidth, measuredWidth2, paint);
				return num / 2;
			}
		}
	}

	internal unsafe int BreakText(ReadOnlySpan<byte> text, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			fixed (float* measuredWidth2 = &measuredWidth)
			{
				return BreakText(text2, text.Length, encoding, maxWidth, measuredWidth2, paint);
			}
		}
	}

	internal unsafe int BreakText(IntPtr text, int length, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint = null)
	{
		fixed (float* measuredWidth2 = &measuredWidth)
		{
			return BreakText((void*)text, length, encoding, maxWidth, measuredWidth2, paint);
		}
	}

	internal unsafe int BreakText(void* text, int length, SKTextEncoding encoding, float maxWidth, float* measuredWidth, SKPaint paint)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return 0;
		}
		return (int)SkiaApi.sk_font_break_text(Handle, text, (IntPtr)length, encoding, maxWidth, measuredWidth, paint?.Handle ?? IntPtr.Zero);
	}

	internal SKPoint[] GetGlyphPositions(string text, SKPoint origin = default(SKPoint))
	{
		return GetGlyphPositions(text.AsSpan(), origin);
	}

	internal unsafe SKPoint[] GetGlyphPositions(ReadOnlySpan<char> text, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphPositions(text2, text.Length * 2, SKTextEncoding.Utf16, origin);
		}
	}

	internal unsafe SKPoint[] GetGlyphPositions(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphPositions(text2, text.Length, encoding, origin);
		}
	}

	internal unsafe SKPoint[] GetGlyphPositions(IntPtr text, int length, SKTextEncoding encoding, SKPoint origin = default(SKPoint))
	{
		return GetGlyphPositions((void*)text, length, encoding, origin);
	}

	internal void GetGlyphPositions(string text, Span<SKPoint> offsets, SKPoint origin = default(SKPoint))
	{
		GetGlyphPositions(text.AsSpan(), offsets, origin);
	}

	internal unsafe void GetGlyphPositions(ReadOnlySpan<char> text, Span<SKPoint> offsets, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphPositions(text2, text.Length * 2, SKTextEncoding.Utf16, offsets, origin);
		}
	}

	internal unsafe void GetGlyphPositions(ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphPositions(text2, text.Length, encoding, offsets, origin);
		}
	}

	internal unsafe void GetGlyphPositions(IntPtr text, int length, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin = default(SKPoint))
	{
		GetGlyphPositions((void*)text, length, encoding, offsets, origin);
	}

	internal unsafe SKPoint[] GetGlyphPositions(void* text, int length, SKTextEncoding encoding, SKPoint origin)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new SKPoint[0];
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return new SKPoint[0];
		}
		SKPoint[] array = new SKPoint[num];
		GetGlyphPositions(text, length, encoding, array, origin);
		return array;
	}

	internal unsafe void GetGlyphPositions(void* text, int length, SKTextEncoding encoding, Span<SKPoint> offsets, SKPoint origin)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return;
		}
		int length2 = offsets.Length;
		if (length2 <= 0)
		{
			return;
		}
		Utils.RentedArray<ushort> rentedArray = Utils.RentArray<ushort>(length2);
		try
		{
			GetGlyphs(text, length, encoding, rentedArray);
			GetGlyphPositions(rentedArray, offsets, origin);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal SKPoint[] GetGlyphPositions(ReadOnlySpan<ushort> glyphs, SKPoint origin = default(SKPoint))
	{
		SKPoint[] array = new SKPoint[glyphs.Length];
		GetGlyphPositions(glyphs, array, origin);
		return array;
	}

	public unsafe void GetGlyphPositions(ReadOnlySpan<ushort> glyphs, Span<SKPoint> positions, SKPoint origin = default(SKPoint))
	{
		if (glyphs.Length != positions.Length)
		{
			throw new ArgumentException("The length of glyphs must be the same as the length of positions.", "positions");
		}
		fixed (ushort* glyphs2 = glyphs)
		{
			fixed (SKPoint* pos = positions)
			{
				SkiaApi.sk_font_get_pos(Handle, glyphs2, glyphs.Length, pos, &origin);
			}
		}
	}

	internal float[] GetGlyphOffsets(string text, float origin = 0f)
	{
		return GetGlyphOffsets(text.AsSpan(), origin);
	}

	internal unsafe float[] GetGlyphOffsets(ReadOnlySpan<char> text, float origin = 0f)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphOffsets(text2, text.Length * 2, SKTextEncoding.Utf16, origin);
		}
	}

	internal unsafe float[] GetGlyphOffsets(ReadOnlySpan<byte> text, SKTextEncoding encoding, float origin = 0f)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphOffsets(text2, text.Length, encoding, origin);
		}
	}

	internal unsafe float[] GetGlyphOffsets(IntPtr text, int length, SKTextEncoding encoding, float origin = 0f)
	{
		return GetGlyphOffsets((void*)text, length, encoding, origin);
	}

	internal void GetGlyphOffsets(string text, Span<float> offsets, float origin = 0f)
	{
		GetGlyphOffsets(text.AsSpan(), offsets, origin);
	}

	internal unsafe void GetGlyphOffsets(ReadOnlySpan<char> text, Span<float> offsets, float origin = 0f)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphOffsets(text2, text.Length * 2, SKTextEncoding.Utf16, offsets, origin);
		}
	}

	internal unsafe void GetGlyphOffsets(ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<float> offsets, float origin = 0f)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphOffsets(text2, text.Length, encoding, offsets, origin);
		}
	}

	internal unsafe void GetGlyphOffsets(IntPtr text, int length, SKTextEncoding encoding, Span<float> offsets, float origin = 0f)
	{
		GetGlyphOffsets((void*)text, length, encoding, offsets, origin);
	}

	internal unsafe float[] GetGlyphOffsets(void* text, int length, SKTextEncoding encoding, float origin)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new float[0];
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return new float[0];
		}
		float[] array = new float[num];
		GetGlyphOffsets(text, length, encoding, array, origin);
		return array;
	}

	internal unsafe void GetGlyphOffsets(void* text, int length, SKTextEncoding encoding, Span<float> offsets, float origin)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return;
		}
		int length2 = offsets.Length;
		if (length2 <= 0)
		{
			return;
		}
		Utils.RentedArray<ushort> rentedArray = Utils.RentArray<ushort>(length2);
		try
		{
			GetGlyphs(text, length, encoding, rentedArray);
			GetGlyphOffsets(rentedArray, offsets, origin);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal float[] GetGlyphOffsets(ReadOnlySpan<ushort> glyphs, float origin = 0f)
	{
		float[] array = new float[glyphs.Length];
		GetGlyphOffsets(glyphs, array, origin);
		return array;
	}

	public unsafe void GetGlyphOffsets(ReadOnlySpan<ushort> glyphs, Span<float> offsets, float origin = 0f)
	{
		if (glyphs.Length != offsets.Length)
		{
			throw new ArgumentException("The length of glyphs must be the same as the length of offsets.", "offsets");
		}
		fixed (ushort* glyphs2 = glyphs)
		{
			fixed (float* xpos = offsets)
			{
				SkiaApi.sk_font_get_xpos(Handle, glyphs2, glyphs.Length, xpos, origin);
			}
		}
	}

	internal float[] GetGlyphWidths(string text, SKPaint paint = null)
	{
		return GetGlyphWidths(text.AsSpan(), paint);
	}

	internal unsafe float[] GetGlyphWidths(ReadOnlySpan<char> text, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphWidths(text2, text.Length * 2, SKTextEncoding.Utf16, paint);
		}
	}

	internal unsafe float[] GetGlyphWidths(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphWidths(text2, text.Length, encoding, paint);
		}
	}

	internal unsafe float[] GetGlyphWidths(IntPtr text, int length, SKTextEncoding encoding, SKPaint paint = null)
	{
		return GetGlyphWidths((void*)text, length, encoding, paint);
	}

	internal float[] GetGlyphWidths(string text, out SKRect[] bounds, SKPaint paint = null)
	{
		return GetGlyphWidths(text.AsSpan(), out bounds, paint);
	}

	internal unsafe float[] GetGlyphWidths(ReadOnlySpan<char> text, out SKRect[] bounds, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphWidths(text2, text.Length * 2, SKTextEncoding.Utf16, out bounds, paint);
		}
	}

	internal unsafe float[] GetGlyphWidths(ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetGlyphWidths(text2, text.Length, encoding, out bounds, paint);
		}
	}

	internal unsafe float[] GetGlyphWidths(IntPtr text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint = null)
	{
		return GetGlyphWidths((void*)text, length, encoding, out bounds, paint);
	}

	internal void GetGlyphWidths(string text, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
	{
		GetGlyphWidths(text.AsSpan(), widths, bounds, paint);
	}

	internal unsafe void GetGlyphWidths(ReadOnlySpan<char> text, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphWidths(text2, text.Length * 2, SKTextEncoding.Utf16, widths, bounds, paint);
		}
	}

	internal unsafe void GetGlyphWidths(ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			GetGlyphWidths(text2, text.Length, encoding, widths, bounds, paint);
		}
	}

	internal unsafe void GetGlyphWidths(IntPtr text, int length, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
	{
		GetGlyphWidths((void*)text, length, encoding, widths, bounds, paint);
	}

	internal unsafe float[] GetGlyphWidths(void* text, int length, SKTextEncoding encoding, SKPaint paint)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new float[0];
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return new float[0];
		}
		float[] array = new float[num];
		GetGlyphWidths(text, length, encoding, array, Span<SKRect>.Empty, paint);
		return array;
	}

	internal unsafe float[] GetGlyphWidths(void* text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			bounds = new SKRect[0];
			return new float[0];
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			bounds = new SKRect[0];
			return new float[0];
		}
		bounds = new SKRect[num];
		float[] array = new float[num];
		GetGlyphWidths(text, length, encoding, array, bounds, paint);
		return array;
	}

	internal unsafe void GetGlyphWidths(void* text, int length, SKTextEncoding encoding, Span<float> widths, Span<SKRect> bounds, SKPaint paint)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return;
		}
		int num = Math.Max(widths.Length, bounds.Length);
		if (num <= 0)
		{
			return;
		}
		if (widths.Length != 0 && widths.Length != num)
		{
			throw new ArgumentException("The length of widths must be equal to the length of bounds or empty.", "widths");
		}
		if (bounds.Length != 0 && bounds.Length != num)
		{
			throw new ArgumentException("The length of bounds must be equal to the length of widths or empty.", "bounds");
		}
		Utils.RentedArray<ushort> rentedArray = Utils.RentArray<ushort>(num);
		try
		{
			GetGlyphs(text, length, encoding, rentedArray);
			GetGlyphWidths(rentedArray, widths, bounds, paint);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal float[] GetGlyphWidths(ReadOnlySpan<ushort> glyphs, SKPaint paint = null)
	{
		float[] array = new float[glyphs.Length];
		GetGlyphWidths(glyphs, array, Span<SKRect>.Empty, paint);
		return array;
	}

	internal float[] GetGlyphWidths(ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint = null)
	{
		float[] array = new float[glyphs.Length];
		bounds = new SKRect[glyphs.Length];
		GetGlyphWidths(glyphs, array, bounds, paint);
		return array;
	}

	public unsafe void GetGlyphWidths(ReadOnlySpan<ushort> glyphs, Span<float> widths, Span<SKRect> bounds, SKPaint paint = null)
	{
		fixed (ushort* glyphs2 = glyphs)
		{
			fixed (float* ptr = widths)
			{
				fixed (SKRect* ptr2 = bounds)
				{
					float* widths2 = ((widths.Length > 0) ? ptr : null);
					SKRect* bounds2 = ((bounds.Length > 0) ? ptr2 : null);
					SkiaApi.sk_font_get_widths_bounds(Handle, glyphs2, glyphs.Length, widths2, bounds2, paint?.Handle ?? IntPtr.Zero);
				}
			}
		}
	}

	public SKPath GetGlyphPath(ushort glyph)
	{
		SKPath sKPath = new SKPath();
		if (!SkiaApi.sk_font_get_path(Handle, glyph, sKPath.Handle))
		{
			sKPath.Dispose();
			sKPath = null;
		}
		return sKPath;
	}

	internal SKPath GetTextPath(string text, SKPoint origin = default(SKPoint))
	{
		return GetTextPath(text.AsSpan(), origin);
	}

	internal unsafe SKPath GetTextPath(ReadOnlySpan<char> text, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPath(text2, text.Length * 2, SKTextEncoding.Utf16, origin);
		}
	}

	internal unsafe SKPath GetTextPath(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPath(text2, text.Length, encoding, origin);
		}
	}

	internal unsafe SKPath GetTextPath(IntPtr text, int length, SKTextEncoding encoding, SKPoint origin = default(SKPoint))
	{
		return GetTextPath((void*)text, length, encoding, origin);
	}

	internal unsafe SKPath GetTextPath(void* text, int length, SKTextEncoding encoding, SKPoint origin)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new SKPath();
		}
		SKPath sKPath = new SKPath();
		SkiaApi.sk_text_utils_get_path(text, (IntPtr)length, encoding, origin.X, origin.Y, Handle, sKPath.Handle);
		return sKPath;
	}

	internal SKPath GetTextPath(string text, ReadOnlySpan<SKPoint> positions)
	{
		return GetTextPath(text.AsSpan(), positions);
	}

	internal unsafe SKPath GetTextPath(ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPath(text2, text.Length * 2, SKTextEncoding.Utf16, positions);
		}
	}

	internal unsafe SKPath GetTextPath(ReadOnlySpan<byte> text, SKTextEncoding encoding, ReadOnlySpan<SKPoint> positions)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPath(text2, text.Length, encoding, positions);
		}
	}

	internal unsafe SKPath GetTextPath(IntPtr text, int length, SKTextEncoding encoding, ReadOnlySpan<SKPoint> positions)
	{
		return GetTextPath((void*)text, length, encoding, positions);
	}

	internal unsafe SKPath GetTextPath(void* text, int length, SKTextEncoding encoding, ReadOnlySpan<SKPoint> positions)
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new SKPath();
		}
		SKPath sKPath = new SKPath();
		fixed (SKPoint* pos = positions)
		{
			SkiaApi.sk_text_utils_get_pos_path(text, (IntPtr)length, encoding, pos, Handle, sKPath.Handle);
		}
		return sKPath;
	}

	public unsafe void GetGlyphPaths(ReadOnlySpan<ushort> glyphs, SKGlyphPathDelegate glyphPathDelegate)
	{
		GCHandle gch;
		IntPtr contextPtr;
		SKGlyphPathProxyDelegate glyphPathProc = DelegateProxies.Create(glyphPathDelegate, DelegateProxies.SKGlyphPathDelegateProxy, out gch, out contextPtr);
		try
		{
			fixed (ushort* glyphs2 = glyphs)
			{
				SkiaApi.sk_font_get_paths(Handle, glyphs2, glyphs.Length, glyphPathProc, (void*)contextPtr);
			}
		}
		finally
		{
			gch.Free();
		}
	}

	internal SKPath GetTextPathOnPath(string text, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		return GetTextPathOnPath(text.AsSpan(), path, textAlign, origin);
	}

	internal unsafe SKPath GetTextPathOnPath(ReadOnlySpan<char> text, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPathOnPath(text2, text.Length * 2, SKTextEncoding.Utf16, path, textAlign, origin);
		}
	}

	internal unsafe SKPath GetTextPathOnPath(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return GetTextPathOnPath(text2, text.Length, encoding, path, textAlign, origin);
		}
	}

	internal unsafe SKPath GetTextPathOnPath(IntPtr text, int length, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		return GetTextPathOnPath((void*)text, length, encoding, path, textAlign, origin);
	}

	internal unsafe SKPath GetTextPathOnPath(void* text, int length, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		if (!ValidateTextArgs(text, length, encoding))
		{
			return new SKPath();
		}
		int num = CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return new SKPath();
		}
		Utils.RentedArray<ushort> rentedArray = Utils.RentArray<ushort>(num);
		try
		{
			GetGlyphs(text, length, encoding, rentedArray);
			return GetTextPathOnPath(rentedArray, path, textAlign, origin);
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal SKPath GetTextPathOnPath(ReadOnlySpan<ushort> glyphs, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (glyphs.Length == 0)
		{
			return new SKPath();
		}
		Utils.RentedArray<float> rentedArray = Utils.RentArray<float>(glyphs.Length);
		try
		{
			Utils.RentedArray<SKPoint> rentedArray2 = Utils.RentArray<SKPoint>(glyphs.Length);
			try
			{
				GetGlyphWidths(glyphs, rentedArray, Span<SKRect>.Empty);
				GetGlyphPositions(glyphs, rentedArray2, origin);
				return GetTextPathOnPath(glyphs, rentedArray, rentedArray2, path, textAlign);
			}
			finally
			{
				rentedArray2.Dispose();
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal SKPath GetTextPathOnPath(ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> glyphWidths, ReadOnlySpan<SKPoint> glyphPositions, SKPath path, SKTextAlign textAlign = SKTextAlign.Left)
	{
		if (glyphs.Length != glyphWidths.Length)
		{
			throw new ArgumentException("The number of glyphs and glyph widths must be the same.");
		}
		if (glyphs.Length != glyphPositions.Length)
		{
			throw new ArgumentException("The number of glyphs and glyph offsets must be the same.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (glyphs.Length == 0)
		{
			return new SKPath();
		}
		using (GlyphPathCache glyphPathCache = new GlyphPathCache(this))
		{
			using SKPathMeasure sKPathMeasure = new SKPathMeasure(path);
			float length = sKPathMeasure.Length;
			float num = glyphPositions[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];
			float num2 = (float)textAlign * 0.5f;
			float num3 = glyphPositions[0].X + (length - num) * num2;
			SKPath sKPath = new SKPath();
			for (int i = 0; i < glyphPositions.Length; i++)
			{
				SKPoint sKPoint = glyphPositions[i];
				float num4 = glyphWidths[i];
				float num5 = num3 + sKPoint.X;
				float num6 = num5 + num4;
				if (num6 >= 0f && num5 <= length)
				{
					ushort glyph = glyphs[i];
					SKPath path2 = glyphPathCache.GetPath(glyph);
					if (path2 != null)
					{
						SKMatrix matrix2 = SKMatrix.CreateTranslation(num5, sKPoint.Y);
						MorphPath(sKPath, path2, sKPathMeasure, in matrix2);
					}
				}
			}
			return sKPath;
		}
		static void MorphPath(SKPath dst, SKPath src, SKPathMeasure meas, in SKMatrix matrix)
		{
			using SKPath.Iterator iterator = src.CreateIterator(forceClose: false);
			Span<SKPoint> span = stackalloc SKPoint[4];
			Span<SKPoint> dst2 = stackalloc SKPoint[4];
			SKPathVerb sKPathVerb;
			while ((sKPathVerb = iterator.Next(span)) != SKPathVerb.Done)
			{
				switch (sKPathVerb)
				{
				case SKPathVerb.Move:
					MorphPoints(dst2, span, 1, meas, in matrix);
					dst.MoveTo(dst2[0]);
					break;
				case SKPathVerb.Line:
					span[0].X = (span[0].X + span[1].X) * 0.5f;
					span[0].Y = (span[0].Y + span[1].Y) * 0.5f;
					MorphPoints(dst2, span, 2, meas, in matrix);
					dst.QuadTo(dst2[0], dst2[1]);
					break;
				case SKPathVerb.Quad:
					MorphPoints(dst2, span.Slice(1, 2), 2, meas, in matrix);
					dst.QuadTo(dst2[0], dst2[1]);
					break;
				case SKPathVerb.Conic:
					MorphPoints(dst2, span.Slice(1, 2), 2, meas, in matrix);
					dst.ConicTo(dst2[0], dst2[1], iterator.ConicWeight());
					break;
				case SKPathVerb.Cubic:
					MorphPoints(dst2, span.Slice(1, 3), 3, meas, in matrix);
					dst.CubicTo(dst2[0], dst2[1], dst2[2]);
					break;
				case SKPathVerb.Close:
					dst.Close();
					break;
				}
			}
		}
		static void MorphPoints(Span<SKPoint> dst, Span<SKPoint> src, int count, SKPathMeasure meas, in SKMatrix matrix)
		{
			for (int j = 0; j < count; j++)
			{
				SKPoint sKPoint2 = matrix.MapPoint(src[j].X, src[j].Y);
				if (!meas.GetPositionAndTangent(sKPoint2.X, out var position, out var tangent))
				{
					tangent = SKPoint.Empty;
				}
				dst[j] = new SKPoint(position.X - tangent.Y * sKPoint2.Y, position.Y + tangent.X * sKPoint2.Y);
			}
		}
	}

	private unsafe bool ValidateTextArgs(void* text, int length, SKTextEncoding encoding)
	{
		if (length == 0)
		{
			return false;
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return true;
	}

	internal static SKFont GetObject(IntPtr handle, bool owns = true)
	{
		return SKObject.GetOrAddObject(handle, owns, (IntPtr h, bool o) => new SKFont(h, o));
	}
}
