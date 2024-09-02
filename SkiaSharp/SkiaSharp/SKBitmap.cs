using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKBitmap : SKObject, ISKSkipObjectRegistration
{
	private const string UnsupportedColorTypeMessage = "Setting the ColorTable is only supported for bitmaps with ColorTypes of Index8.";

	private const string UnableToAllocatePixelsMessage = "Unable to allocate pixels for the bitmap.";

	public bool ReadyToDraw => SkiaApi.sk_bitmap_ready_to_draw(Handle);

	public unsafe SKImageInfo Info
	{
		get
		{
			SKImageInfoNative native = default(SKImageInfoNative);
			SkiaApi.sk_bitmap_get_info(Handle, &native);
			return SKImageInfoNative.ToManaged(ref native);
		}
	}

	public int Width => Info.Width;

	public int Height => Info.Height;

	public SKColorType ColorType => Info.ColorType;

	public SKAlphaType AlphaType => Info.AlphaType;

	public SKColorSpace ColorSpace => Info.ColorSpace;

	public int BytesPerPixel => Info.BytesPerPixel;

	public int RowBytes => (int)SkiaApi.sk_bitmap_get_row_bytes(Handle);

	public int ByteCount => (int)SkiaApi.sk_bitmap_get_byte_count(Handle);

	public byte[] Bytes
	{
		get
		{
			byte[] result = GetPixelSpan().ToArray();
			GC.KeepAlive(this);
			return result;
		}
	}

	public unsafe SKColor[] Pixels
	{
		get
		{
			SKImageInfo info = Info;
			SKColor[] array = new SKColor[info.Width * info.Height];
			fixed (SKColor* colors = array)
			{
				SkiaApi.sk_bitmap_get_pixel_colors(Handle, (uint*)colors);
			}
			return array;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			SKImageInfo info = Info;
			if (info.Width * info.Height != value.Length)
			{
				throw new ArgumentException($"The number of pixels must equal Width x Height, or {info.Width * info.Height}.", "value");
			}
			fixed (SKColor* ptr = value)
			{
				SKImageInfo info2 = new SKImageInfo(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
				using SKBitmap sKBitmap = new SKBitmap();
				sKBitmap.InstallPixels(info2, (IntPtr)ptr);
				using SKShader shader = sKBitmap.ToShader();
				using SKCanvas sKCanvas = new SKCanvas(this);
				using SKPaint paint = new SKPaint
				{
					Shader = shader,
					BlendMode = SKBlendMode.Src
				};
				sKCanvas.DrawPaint(paint);
			}
		}
	}

	public bool IsEmpty => Info.IsEmpty;

	public bool IsNull => SkiaApi.sk_bitmap_is_null(Handle);

	public bool DrawsNothing
	{
		get
		{
			if (!IsEmpty)
			{
				return IsNull;
			}
			return true;
		}
	}

	public bool IsImmutable => SkiaApi.sk_bitmap_is_immutable(Handle);

	[Obsolete]
	public bool IsVolatile
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported.")]
	public SKColorTable ColorTable => null;

	internal SKBitmap(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKBitmap()
		: this(SkiaApi.sk_bitmap_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKBitmap instance.");
		}
	}

	public SKBitmap(int width, int height, bool isOpaque = false)
		: this(width, height, SKImageInfo.PlatformColorType, isOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul)
	{
	}

	public SKBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType)
		: this(new SKImageInfo(width, height, colorType, alphaType))
	{
	}

	public SKBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
		: this(new SKImageInfo(width, height, colorType, alphaType, colorspace))
	{
	}

	public SKBitmap(SKImageInfo info)
		: this(info, info.RowBytes)
	{
	}

	public SKBitmap(SKImageInfo info, int rowBytes)
		: this()
	{
		if (!TryAllocPixels(info, rowBytes))
		{
			throw new Exception("Unable to allocate pixels for the bitmap.");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo, SKBitmapAllocFlags) instead.")]
	public SKBitmap(SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags)
		: this(info, SKBitmapAllocFlags.None)
	{
	}

	public SKBitmap(SKImageInfo info, SKBitmapAllocFlags flags)
		: this()
	{
		if (!TryAllocPixels(info, flags))
		{
			throw new Exception("Unable to allocate pixels for the bitmap.");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo) instead.")]
	public SKBitmap(SKImageInfo info, SKColorTable ctable)
		: this(info, SKBitmapAllocFlags.None)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_bitmap_destructor(Handle);
	}

	public bool TryAllocPixels(SKImageInfo info)
	{
		return TryAllocPixels(info, info.RowBytes);
	}

	public unsafe bool TryAllocPixels(SKImageInfo info, int rowBytes)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return SkiaApi.sk_bitmap_try_alloc_pixels(Handle, &sKImageInfoNative, (IntPtr)rowBytes);
	}

	public unsafe bool TryAllocPixels(SKImageInfo info, SKBitmapAllocFlags flags)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return SkiaApi.sk_bitmap_try_alloc_pixels_with_flags(Handle, &sKImageInfoNative, (uint)flags);
	}

	public void Reset()
	{
		SkiaApi.sk_bitmap_reset(Handle);
	}

	public void SetImmutable()
	{
		SkiaApi.sk_bitmap_set_immutable(Handle);
	}

	public void Erase(SKColor color)
	{
		SkiaApi.sk_bitmap_erase(Handle, (uint)color);
	}

	public unsafe void Erase(SKColor color, SKRectI rect)
	{
		SkiaApi.sk_bitmap_erase_rect(Handle, (uint)color, &rect);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public unsafe byte GetAddr8(int x, int y)
	{
		return *SkiaApi.sk_bitmap_get_addr_8(Handle, x, y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public unsafe ushort GetAddr16(int x, int y)
	{
		return *SkiaApi.sk_bitmap_get_addr_16(Handle, x, y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public unsafe uint GetAddr32(int x, int y)
	{
		return *SkiaApi.sk_bitmap_get_addr_32(Handle, x, y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetAddress instead.")]
	public IntPtr GetAddr(int x, int y)
	{
		return GetAddress(x, y);
	}

	public unsafe IntPtr GetAddress(int x, int y)
	{
		return (IntPtr)SkiaApi.sk_bitmap_get_addr(Handle, x, y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use GetPixel(int, int) instead.")]
	public SKPMColor GetIndex8Color(int x, int y)
	{
		return (SKPMColor)GetPixel(x, y);
	}

	public SKColor GetPixel(int x, int y)
	{
		return SkiaApi.sk_bitmap_get_pixel_color(Handle, x, y);
	}

	public void SetPixel(int x, int y, SKColor color)
	{
		SKImageInfo info = Info;
		if (x < 0 || x >= info.Width)
		{
			throw new ArgumentOutOfRangeException("x");
		}
		if (y < 0 || y >= info.Height)
		{
			throw new ArgumentOutOfRangeException("y");
		}
		using SKCanvas sKCanvas = new SKCanvas(this);
		sKCanvas.DrawPoint(x, y, color);
	}

	public bool CanCopyTo(SKColorType colorType)
	{
		if (colorType == SKColorType.Unknown)
		{
			return false;
		}
		using SKBitmap sKBitmap = new SKBitmap();
		SKImageInfo info = Info.WithColorType(colorType).WithSize(1, 1);
		return sKBitmap.TryAllocPixels(info);
	}

	public SKBitmap Copy()
	{
		return Copy(ColorType);
	}

	public SKBitmap Copy(SKColorType colorType)
	{
		SKBitmap sKBitmap = new SKBitmap();
		if (!CopyTo(sKBitmap, colorType))
		{
			sKBitmap.Dispose();
			sKBitmap = null;
		}
		return sKBitmap;
	}

	public bool CopyTo(SKBitmap destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return CopyTo(destination, ColorType);
	}

	public bool CopyTo(SKBitmap destination, SKColorType colorType)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (colorType == SKColorType.Unknown)
		{
			return false;
		}
		using SKPixmap sKPixmap = PeekPixels();
		if (sKPixmap == null)
		{
			return false;
		}
		using SKBitmap sKBitmap = new SKBitmap();
		SKImageInfo info = sKPixmap.Info.WithColorType(colorType);
		if (!sKBitmap.TryAllocPixels(info))
		{
			return false;
		}
		using SKCanvas sKCanvas = new SKCanvas(sKBitmap);
		using SKPaint paint = new SKPaint
		{
			Shader = ToShader(),
			BlendMode = SKBlendMode.Src
		};
		sKCanvas.DrawPaint(paint);
		destination.Swap(sKBitmap);
		return true;
	}

	public unsafe bool ExtractSubset(SKBitmap destination, SKRectI subset)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return SkiaApi.sk_bitmap_extract_subset(Handle, destination.Handle, &subset);
	}

	public bool ExtractAlpha(SKBitmap destination)
	{
		SKPointI offset;
		return ExtractAlpha(destination, null, out offset);
	}

	public bool ExtractAlpha(SKBitmap destination, out SKPointI offset)
	{
		return ExtractAlpha(destination, null, out offset);
	}

	public bool ExtractAlpha(SKBitmap destination, SKPaint paint)
	{
		SKPointI offset;
		return ExtractAlpha(destination, paint, out offset);
	}

	public unsafe bool ExtractAlpha(SKBitmap destination, SKPaint paint, out SKPointI offset)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		fixed (SKPointI* offset2 = &offset)
		{
			return SkiaApi.sk_bitmap_extract_alpha(Handle, destination.Handle, paint?.Handle ?? IntPtr.Zero, offset2);
		}
	}

	public IntPtr GetPixels()
	{
		IntPtr length;
		return GetPixels(out length);
	}

	public unsafe ReadOnlySpan<byte> GetPixelSpan()
	{
		IntPtr length;
		return new ReadOnlySpan<byte>((void*)GetPixels(out length), (int)length);
	}

	public unsafe IntPtr GetPixels(out IntPtr length)
	{
		fixed (IntPtr* length2 = &length)
		{
			return (IntPtr)SkiaApi.sk_bitmap_get_pixels(Handle, length2);
		}
	}

	public unsafe void SetPixels(IntPtr pixels)
	{
		SkiaApi.sk_bitmap_set_pixels(Handle, (void*)pixels);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use SetPixels(IntPtr) instead.")]
	public void SetPixels(IntPtr pixels, SKColorTable ct)
	{
		SetPixels(pixels);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported.")]
	public void SetColorTable(SKColorTable ct)
	{
	}

	public static SKImageInfo DecodeBounds(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		return sKCodec?.Info ?? SKImageInfo.Empty;
	}

	public static SKImageInfo DecodeBounds(SKStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		return sKCodec?.Info ?? SKImageInfo.Empty;
	}

	public static SKImageInfo DecodeBounds(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKCodec sKCodec = SKCodec.Create(data);
		return sKCodec?.Info ?? SKImageInfo.Empty;
	}

	public static SKImageInfo DecodeBounds(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		using SKCodec sKCodec = SKCodec.Create(filename);
		return sKCodec?.Info ?? SKImageInfo.Empty;
	}

	public static SKImageInfo DecodeBounds(byte[] buffer)
	{
		return DecodeBounds(buffer.AsSpan());
	}

	public unsafe static SKImageInfo DecodeBounds(ReadOnlySpan<byte> buffer)
	{
		fixed (byte* ptr = buffer)
		{
			using SKData data = SKData.Create((IntPtr)ptr, buffer.Length);
			using SKCodec sKCodec = SKCodec.Create(data);
			return sKCodec?.Info ?? SKImageInfo.Empty;
		}
	}

	public static SKBitmap Decode(SKCodec codec)
	{
		if (codec == null)
		{
			throw new ArgumentNullException("codec");
		}
		SKImageInfo info = codec.Info;
		if (info.AlphaType == SKAlphaType.Unpremul)
		{
			info.AlphaType = SKAlphaType.Premul;
		}
		info.ColorSpace = null;
		return Decode(codec, info);
	}

	public static SKBitmap Decode(SKCodec codec, SKImageInfo bitmapInfo)
	{
		if (codec == null)
		{
			throw new ArgumentNullException("codec");
		}
		SKBitmap sKBitmap = new SKBitmap(bitmapInfo);
		IntPtr length;
		SKCodecResult pixels = codec.GetPixels(bitmapInfo, sKBitmap.GetPixels(out length));
		if (pixels != 0 && pixels != SKCodecResult.IncompleteInput)
		{
			sKBitmap.Dispose();
			sKBitmap = null;
		}
		return sKBitmap;
	}

	public static SKBitmap Decode(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec);
	}

	public static SKBitmap Decode(Stream stream, SKImageInfo bitmapInfo)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec, bitmapInfo);
	}

	public static SKBitmap Decode(SKStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec);
	}

	public static SKBitmap Decode(SKStream stream, SKImageInfo bitmapInfo)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using SKCodec sKCodec = SKCodec.Create(stream);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec, bitmapInfo);
	}

	public static SKBitmap Decode(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKCodec sKCodec = SKCodec.Create(data);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec);
	}

	public static SKBitmap Decode(SKData data, SKImageInfo bitmapInfo)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKCodec sKCodec = SKCodec.Create(data);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec, bitmapInfo);
	}

	public static SKBitmap Decode(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		using SKCodec sKCodec = SKCodec.Create(filename);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec);
	}

	public static SKBitmap Decode(string filename, SKImageInfo bitmapInfo)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		using SKCodec sKCodec = SKCodec.Create(filename);
		if (sKCodec == null)
		{
			return null;
		}
		return Decode(sKCodec, bitmapInfo);
	}

	public static SKBitmap Decode(byte[] buffer)
	{
		return Decode(buffer.AsSpan());
	}

	public static SKBitmap Decode(byte[] buffer, SKImageInfo bitmapInfo)
	{
		return Decode(buffer.AsSpan(), bitmapInfo);
	}

	public unsafe static SKBitmap Decode(ReadOnlySpan<byte> buffer)
	{
		fixed (byte* ptr = buffer)
		{
			using SKData data = SKData.Create((IntPtr)ptr, buffer.Length);
			using SKCodec codec = SKCodec.Create(data);
			return Decode(codec);
		}
	}

	public unsafe static SKBitmap Decode(ReadOnlySpan<byte> buffer, SKImageInfo bitmapInfo)
	{
		fixed (byte* ptr = buffer)
		{
			using SKData data = SKData.Create((IntPtr)ptr, buffer.Length);
			using SKCodec codec = SKCodec.Create(data);
			return Decode(codec, bitmapInfo);
		}
	}

	public bool InstallPixels(SKImageInfo info, IntPtr pixels)
	{
		return InstallPixels(info, pixels, info.RowBytes, null, null);
	}

	public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		return InstallPixels(info, pixels, rowBytes, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int) instead.")]
	public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable)
	{
		return InstallPixels(info, pixels, rowBytes, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int, SKBitmapReleaseDelegate, object) instead.")]
	public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context)
	{
		return InstallPixels(info, pixels, rowBytes, releaseProc, context);
	}

	public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc)
	{
		return InstallPixels(info, pixels, rowBytes, releaseProc, null);
	}

	public unsafe bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SKBitmapReleaseDelegate managedDel = ((releaseProc != null && context != null) ? ((SKBitmapReleaseDelegate)delegate(IntPtr addr, object _)
		{
			releaseProc(addr, context);
		}) : releaseProc);
		GCHandle gch;
		IntPtr contextPtr;
		SKBitmapReleaseProxyDelegate releaseProc2 = DelegateProxies.Create(managedDel, DelegateProxies.SKBitmapReleaseDelegateProxy, out gch, out contextPtr);
		return SkiaApi.sk_bitmap_install_pixels(Handle, &sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes, releaseProc2, (void*)contextPtr);
	}

	public bool InstallPixels(SKPixmap pixmap)
	{
		return SkiaApi.sk_bitmap_install_pixels_with_pixmap(Handle, pixmap.Handle);
	}

	public unsafe bool InstallMaskPixels(SKMask mask)
	{
		return SkiaApi.sk_bitmap_install_mask_pixels(Handle, &mask);
	}

	public void NotifyPixelsChanged()
	{
		SkiaApi.sk_bitmap_notify_pixels_changed(Handle);
	}

	public SKPixmap PeekPixels()
	{
		SKPixmap sKPixmap = new SKPixmap();
		if (PeekPixels(sKPixmap))
		{
			return sKPixmap;
		}
		sKPixmap.Dispose();
		return null;
	}

	public bool PeekPixels(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		bool flag = SkiaApi.sk_bitmap_peek_pixels(Handle, pixmap.Handle);
		if (flag)
		{
			pixmap.pixelSource = this;
		}
		return flag;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Resize(SKImageInfo, SKFilterQuality) instead.")]
	public SKBitmap Resize(SKImageInfo info, SKBitmapResizeMethod method)
	{
		return Resize(info, method.ToFilterQuality());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
	public bool Resize(SKBitmap dst, SKBitmapResizeMethod method)
	{
		return ScalePixels(dst, method.ToFilterQuality());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
	public static bool Resize(SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method)
	{
		return src.ScalePixels(dst, method.ToFilterQuality());
	}

	public SKBitmap Resize(SKImageInfo info, SKFilterQuality quality)
	{
		if (info.IsEmpty)
		{
			return null;
		}
		SKBitmap sKBitmap = new SKBitmap(info);
		if (ScalePixels(sKBitmap, quality))
		{
			return sKBitmap;
		}
		sKBitmap.Dispose();
		return null;
	}

	public SKBitmap Resize(SKSizeI size, SKFilterQuality quality)
	{
		return Resize(Info.WithSize(size), quality);
	}

	public bool ScalePixels(SKBitmap destination, SKFilterQuality quality)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		using SKPixmap destination2 = destination.PeekPixels();
		return ScalePixels(destination2, quality);
	}

	public bool ScalePixels(SKPixmap destination, SKFilterQuality quality)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		using SKPixmap sKPixmap = PeekPixels();
		return sKPixmap.ScalePixels(destination, quality);
	}

	public static SKBitmap FromImage(SKImage image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SKImageInfo sKImageInfo = new SKImageInfo(image.Width, image.Height, SKImageInfo.PlatformColorType, image.AlphaType);
		SKBitmap sKBitmap = new SKBitmap(sKImageInfo);
		if (!image.ReadPixels(sKImageInfo, sKBitmap.GetPixels(), sKImageInfo.RowBytes, 0, 0))
		{
			sKBitmap.Dispose();
			sKBitmap = null;
		}
		return sKBitmap;
	}

	public SKData Encode(SKEncodedImageFormat format, int quality)
	{
		using SKPixmap sKPixmap = PeekPixels();
		return sKPixmap?.Encode(format, quality);
	}

	public bool Encode(Stream dst, SKEncodedImageFormat format, int quality)
	{
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, format, quality);
	}

	public bool Encode(SKWStream dst, SKEncodedImageFormat format, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKPixmap sKPixmap = PeekPixels();
		return sKPixmap?.Encode(dst, format, quality) ?? false;
	}

	private void Swap(SKBitmap other)
	{
		SkiaApi.sk_bitmap_swap(Handle, other.Handle);
	}

	public SKShader ToShader()
	{
		return ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy)
	{
		return SKShader.GetObject(SkiaApi.sk_bitmap_make_shader(Handle, tmx, tmy, null));
	}

	public unsafe SKShader ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
	{
		return SKShader.GetObject(SkiaApi.sk_bitmap_make_shader(Handle, tmx, tmy, &localMatrix));
	}
}
