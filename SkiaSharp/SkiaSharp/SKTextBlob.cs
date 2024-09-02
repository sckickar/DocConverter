using System;

namespace SkiaSharp;

public class SKTextBlob : SKObject, ISKNonVirtualReferenceCounted, ISKReferenceCounted, ISKSkipObjectRegistration
{
	public unsafe SKRect Bounds
	{
		get
		{
			SKRect result = default(SKRect);
			SkiaApi.sk_textblob_get_bounds(Handle, &result);
			return result;
		}
	}

	public uint UniqueId => SkiaApi.sk_textblob_get_unique_id(Handle);

	internal SKTextBlob(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	void ISKNonVirtualReferenceCounted.ReferenceNative()
	{
		SkiaApi.sk_textblob_ref(Handle);
	}

	void ISKNonVirtualReferenceCounted.UnreferenceNative()
	{
		SkiaApi.sk_textblob_unref(Handle);
	}

	public static SKTextBlob Create(string text, SKFont font, SKPoint origin = default(SKPoint))
	{
		return Create(text.AsSpan(), font, origin);
	}

	public unsafe static SKTextBlob Create(ReadOnlySpan<char> text, SKFont font, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return Create(text2, text.Length * 2, SKTextEncoding.Utf16, font, origin);
		}
	}

	public static SKTextBlob Create(IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default(SKPoint))
	{
		return Create(text.AsReadOnlySpan(length), encoding, font, origin);
	}

	public unsafe static SKTextBlob Create(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return Create(text2, text.Length, encoding, font, origin);
		}
	}

	internal unsafe static SKTextBlob Create(void* text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		int num = font.CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return null;
		}
		using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
		SKPositionedRunBuffer sKPositionedRunBuffer = sKTextBlobBuilder.AllocatePositionedRun(font, num);
		font.GetGlyphs(text, length, encoding, sKPositionedRunBuffer.GetGlyphSpan());
		font.GetGlyphPositions(sKPositionedRunBuffer.GetGlyphSpan(), sKPositionedRunBuffer.GetPositionSpan(), origin);
		return sKTextBlobBuilder.Build();
	}

	public static SKTextBlob CreateHorizontal(string text, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		return CreateHorizontal(text.AsSpan(), font, positions, y);
	}

	public unsafe static SKTextBlob CreateHorizontal(ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return CreateHorizontal(text2, text.Length * 2, SKTextEncoding.Utf16, font, positions, y);
		}
	}

	public static SKTextBlob CreateHorizontal(IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		return CreateHorizontal(text.AsReadOnlySpan(length), encoding, font, positions, y);
	}

	public unsafe static SKTextBlob CreateHorizontal(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return CreateHorizontal(text2, text.Length, encoding, font, positions, y);
		}
	}

	internal unsafe static SKTextBlob CreateHorizontal(void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		int num = font.CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return null;
		}
		using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
		SKHorizontalRunBuffer sKHorizontalRunBuffer = sKTextBlobBuilder.AllocateHorizontalRun(font, num, y);
		font.GetGlyphs(text, length, encoding, sKHorizontalRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKHorizontalRunBuffer.GetPositionSpan());
		return sKTextBlobBuilder.Build();
	}

	public static SKTextBlob CreatePositioned(string text, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		return CreatePositioned(text.AsSpan(), font, positions);
	}

	public unsafe static SKTextBlob CreatePositioned(ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return CreatePositioned(text2, text.Length * 2, SKTextEncoding.Utf16, font, positions);
		}
	}

	public static SKTextBlob CreatePositioned(IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		return CreatePositioned(text.AsReadOnlySpan(length), encoding, font, positions);
	}

	public unsafe static SKTextBlob CreatePositioned(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return CreatePositioned(text2, text.Length, encoding, font, positions);
		}
	}

	internal unsafe static SKTextBlob CreatePositioned(void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		int num = font.CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return null;
		}
		using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
		SKPositionedRunBuffer sKPositionedRunBuffer = sKTextBlobBuilder.AllocatePositionedRun(font, num);
		font.GetGlyphs(text, length, encoding, sKPositionedRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKPositionedRunBuffer.GetPositionSpan());
		return sKTextBlobBuilder.Build();
	}

	public static SKTextBlob CreateRotationScale(string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		return CreateRotationScale(text.AsSpan(), font, positions);
	}

	public unsafe static SKTextBlob CreateRotationScale(ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return CreateRotationScale(text2, text.Length * 2, SKTextEncoding.Utf16, font, positions);
		}
	}

	public static SKTextBlob CreateRotationScale(IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		return CreateRotationScale(text.AsReadOnlySpan(length), encoding, font, positions);
	}

	public unsafe static SKTextBlob CreateRotationScale(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return CreateRotationScale(text2, text.Length, encoding, font, positions);
		}
	}

	internal unsafe static SKTextBlob CreateRotationScale(void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		int num = font.CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return null;
		}
		using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
		SKRotationScaleRunBuffer sKRotationScaleRunBuffer = sKTextBlobBuilder.AllocateRotationScaleRun(font, num);
		font.GetGlyphs(text, length, encoding, sKRotationScaleRunBuffer.GetGlyphSpan());
		positions.CopyTo(sKRotationScaleRunBuffer.GetRotationScaleSpan());
		return sKTextBlobBuilder.Build();
	}

	public static SKTextBlob CreatePathPositioned(string text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		return CreatePathPositioned(text.AsSpan(), font, path, textAlign, origin);
	}

	public unsafe static SKTextBlob CreatePathPositioned(ReadOnlySpan<char> text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		fixed (char* ptr = text)
		{
			void* text2 = ptr;
			return CreatePathPositioned(text2, text.Length * 2, SKTextEncoding.Utf16, font, path, textAlign, origin);
		}
	}

	public static SKTextBlob CreatePathPositioned(IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		return CreatePathPositioned(text.AsReadOnlySpan(length), encoding, font, path, textAlign, origin);
	}

	public unsafe static SKTextBlob CreatePathPositioned(ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		fixed (byte* ptr = text)
		{
			void* text2 = ptr;
			return CreatePathPositioned(text2, text.Length, encoding, font, path, textAlign, origin);
		}
	}

	internal unsafe static SKTextBlob CreatePathPositioned(void* text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default(SKPoint))
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		int num = font.CountGlyphs(text, length, encoding);
		if (num <= 0)
		{
			return null;
		}
		Utils.RentedArray<ushort> rentedArray = Utils.RentArray<ushort>(num);
		try
		{
			Utils.RentedArray<float> rentedArray2 = Utils.RentArray<float>(rentedArray.Length);
			try
			{
				Utils.RentedArray<SKPoint> rentedArray3 = Utils.RentArray<SKPoint>(rentedArray.Length);
				try
				{
					font.GetGlyphs(text, length, encoding, rentedArray);
					font.GetGlyphWidths(rentedArray, rentedArray2, Span<SKRect>.Empty);
					font.GetGlyphPositions(rentedArray, rentedArray3, origin);
					using SKTextBlobBuilder sKTextBlobBuilder = new SKTextBlobBuilder();
					sKTextBlobBuilder.AddPathPositionedRun(rentedArray, font, rentedArray2, rentedArray3, path, textAlign);
					return sKTextBlobBuilder.Build();
				}
				finally
				{
					rentedArray3.Dispose();
				}
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

	public float[] GetIntercepts(float upperBounds, float lowerBounds, SKPaint paint = null)
	{
		int num = CountIntercepts(upperBounds, lowerBounds, paint);
		float[] array = new float[num];
		GetIntercepts(upperBounds, lowerBounds, array, paint);
		return array;
	}

	public unsafe void GetIntercepts(float upperBounds, float lowerBounds, Span<float> intervals, SKPaint paint = null)
	{
		float* ptr = stackalloc float[2];
		*ptr = upperBounds;
		ptr[1] = lowerBounds;
		fixed (float* intervals2 = intervals)
		{
			SkiaApi.sk_textblob_get_intercepts(Handle, ptr, intervals2, paint?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe int CountIntercepts(float upperBounds, float lowerBounds, SKPaint paint = null)
	{
		float* ptr = stackalloc float[2];
		*ptr = upperBounds;
		ptr[1] = lowerBounds;
		return SkiaApi.sk_textblob_get_intercepts(Handle, ptr, null, paint?.Handle ?? IntPtr.Zero);
	}

	internal static SKTextBlob GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKTextBlob(handle, owns: true);
		}
		return null;
	}
}
