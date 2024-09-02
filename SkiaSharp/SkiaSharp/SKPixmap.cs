using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp;

public class SKPixmap : SKObject
{
	private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

	internal SKObject pixelSource;

	public unsafe SKImageInfo Info
	{
		get
		{
			SKImageInfoNative native = default(SKImageInfoNative);
			SkiaApi.sk_pixmap_get_info(Handle, &native);
			return SKImageInfoNative.ToManaged(ref native);
		}
	}

	public int Width => Info.Width;

	public int Height => Info.Height;

	public SKSizeI Size
	{
		get
		{
			SKImageInfo info = Info;
			return new SKSizeI(info.Width, info.Height);
		}
	}

	public SKRectI Rect => SKRectI.Create(Size);

	public SKColorType ColorType => Info.ColorType;

	public SKAlphaType AlphaType => Info.AlphaType;

	public SKColorSpace ColorSpace => Info.ColorSpace;

	public int BytesPerPixel => Info.BytesPerPixel;

	public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes(Handle);

	public int BytesSize => Info.BytesSize;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported.")]
	public SKColorTable ColorTable => null;

	internal SKPixmap(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPixmap()
		: this(SkiaApi.sk_pixmap_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPixmap instance.");
		}
	}

	public SKPixmap(SKImageInfo info, IntPtr addr)
		: this(info, addr, info.RowBytes)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use SKPixmap(SKImageInfo, IntPtr, int) instead.")]
	public SKPixmap(SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
		: this(info, addr, info.RowBytes)
	{
	}

	public unsafe SKPixmap(SKImageInfo info, IntPtr addr, int rowBytes)
		: this(IntPtr.Zero, owns: true)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		Handle = SkiaApi.sk_pixmap_new_with_params(&sKImageInfoNative, (void*)addr, (IntPtr)rowBytes);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPixmap instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_pixmap_destructor(Handle);
	}

	protected override void DisposeManaged()
	{
		base.DisposeManaged();
		pixelSource = null;
	}

	public void Reset()
	{
		SkiaApi.sk_pixmap_reset(Handle);
		pixelSource = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
	public void Reset(SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable)
	{
		Reset(info, addr, rowBytes);
	}

	public unsafe void Reset(SKImageInfo info, IntPtr addr, int rowBytes)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SkiaApi.sk_pixmap_reset_with_params(Handle, &sKImageInfoNative, (void*)addr, (IntPtr)rowBytes);
		pixelSource = null;
	}

	public unsafe IntPtr GetPixels()
	{
		return (IntPtr)SkiaApi.sk_pixmap_get_pixels(Handle);
	}

	public unsafe IntPtr GetPixels(int x, int y)
	{
		return (IntPtr)SkiaApi.sk_pixmap_get_pixels_with_xy(Handle, x, y);
	}

	public unsafe ReadOnlySpan<byte> GetPixelSpan()
	{
		return new ReadOnlySpan<byte>(SkiaApi.sk_pixmap_get_pixels(Handle), BytesSize);
	}

	public unsafe Span<T> GetPixelSpan<T>() where T : unmanaged
	{
		SKImageInfo info = Info;
		if (info.IsEmpty)
		{
			return null;
		}
		int bytesPerPixel = info.BytesPerPixel;
		if (bytesPerPixel <= 0)
		{
			return null;
		}
		if (typeof(T) == typeof(byte))
		{
			return new Span<T>(SkiaApi.sk_pixmap_get_writable_addr(Handle), info.BytesSize);
		}
		int num = sizeof(T);
		if (bytesPerPixel != num)
		{
			throw new ArgumentException($"Size of T ({num}) is not the same as the size of each pixel ({bytesPerPixel}).", "T");
		}
		return new Span<T>(SkiaApi.sk_pixmap_get_writable_addr(Handle), info.Width * info.Height);
	}

	public SKColor GetPixelColor(int x, int y)
	{
		return SkiaApi.sk_pixmap_get_pixel_color(Handle, x, y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
	public static bool Resize(SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.ScalePixels(dst, method.ToFilterQuality());
	}

	public bool ScalePixels(SKPixmap destination, SKFilterQuality quality)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return SkiaApi.sk_pixmap_scale_pixels(Handle, destination.Handle, quality);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ReadPixels(SKImageInfo, IntPtr, int, int, int) instead.")]
	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, srcX, srcY);
	}

	public unsafe bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref dstInfo);
		return SkiaApi.sk_pixmap_read_pixels(Handle, &sKImageInfoNative, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, 0, 0);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY)
	{
		return ReadPixels(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, srcX, srcY);
	}

	public bool ReadPixels(SKPixmap pixmap)
	{
		return ReadPixels(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, 0, 0);
	}

	public SKData Encode(SKEncodedImageFormat encoder, int quality)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, encoder, quality) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKEncodedImageFormat encoder, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, encoder, quality);
	}

	public bool Encode(SKWStream dst, SKEncodedImageFormat encoder, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_pixmap_encode_image(dst.Handle, Handle, encoder, quality);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
	public static bool Encode(SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.Encode(dst, format, quality);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
	public static bool Encode(SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.Encode(dst, encoder, quality);
	}

	public SKData Encode(SKWebpEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKWebpEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKWebpEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_webpencoder_encode(dst.Handle, Handle, &options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Encode(SKWStream, SKWebpEncoderOptions) instead.")]
	public static bool Encode(SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.Encode(dst, options);
	}

	public SKData Encode(SKJpegEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKJpegEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKJpegEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_jpegencoder_encode(dst.Handle, Handle, &options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Encode(SKWStream, SKJpegEncoderOptions) instead.")]
	public static bool Encode(SKWStream dst, SKPixmap src, SKJpegEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.Encode(dst, options);
	}

	public SKData Encode(SKPngEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKPngEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKPngEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_pngencoder_encode(dst.Handle, Handle, &options);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Encode(SKWStream, SKPngEncoderOptions) instead.")]
	public static bool Encode(SKWStream dst, SKPixmap src, SKPngEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		return src.Encode(dst, options);
	}

	public SKPixmap ExtractSubset(SKRectI subset)
	{
		SKPixmap sKPixmap = new SKPixmap();
		if (!ExtractSubset(sKPixmap, subset))
		{
			sKPixmap.Dispose();
			sKPixmap = null;
		}
		return sKPixmap;
	}

	public unsafe bool ExtractSubset(SKPixmap result, SKRectI subset)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return SkiaApi.sk_pixmap_extract_subset(Handle, result.Handle, &subset);
	}

	public bool Erase(SKColor color)
	{
		return Erase(color, Rect);
	}

	public unsafe bool Erase(SKColor color, SKRectI subset)
	{
		return SkiaApi.sk_pixmap_erase_color(Handle, (uint)color, &subset);
	}

	public bool Erase(SKColorF color)
	{
		return Erase(color, null, Rect);
	}

	public bool Erase(SKColorF color, SKRectI subset)
	{
		return Erase(color, null, subset);
	}

	public unsafe bool Erase(SKColorF color, SKColorSpace colorspace, SKRectI subset)
	{
		return SkiaApi.sk_pixmap_erase_color4f(Handle, &color, colorspace?.Handle ?? IntPtr.Zero, &subset);
	}

	public SKPixmap WithColorType(SKColorType newColorType)
	{
		return new SKPixmap(Info.WithColorType(newColorType), GetPixels(), RowBytes);
	}

	public SKPixmap WithColorSpace(SKColorSpace newColorSpace)
	{
		return new SKPixmap(Info.WithColorSpace(newColorSpace), GetPixels(), RowBytes);
	}

	public SKPixmap WithAlphaType(SKAlphaType newAlphaType)
	{
		return new SKPixmap(Info.WithAlphaType(newAlphaType), GetPixels(), RowBytes);
	}
}
