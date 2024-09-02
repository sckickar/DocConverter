using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKImage : SKObject, ISKReferenceCounted
{
	public int Width => SkiaApi.sk_image_get_width(Handle);

	public int Height => SkiaApi.sk_image_get_height(Handle);

	public uint UniqueId => SkiaApi.sk_image_get_unique_id(Handle);

	public SKAlphaType AlphaType => SkiaApi.sk_image_get_alpha_type(Handle);

	public SKColorType ColorType => SkiaApi.sk_image_get_color_type(Handle).FromNative();

	public SKColorSpace ColorSpace => SKColorSpace.GetObject(SkiaApi.sk_image_get_colorspace(Handle));

	public bool IsAlphaOnly => SkiaApi.sk_image_is_alpha_only(Handle);

	public SKData EncodedData => SKData.GetObject(SkiaApi.sk_image_ref_encoded(Handle));

	public SKImageInfo Info => new SKImageInfo(Width, Height, ColorType, AlphaType, ColorSpace);

	public bool IsTextureBacked => SkiaApi.sk_image_is_texture_backed(Handle);

	public bool IsLazyGenerated => SkiaApi.sk_image_is_lazy_generated(Handle);

	internal SKImage(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public unsafe static SKImage Create(SKImageInfo info)
	{
		IntPtr addr = Marshal.AllocCoTaskMem(info.BytesSize);
		using SKPixmap sKPixmap = new SKPixmap(info, addr);
		return GetObject(SkiaApi.sk_image_new_raster(sKPixmap.Handle, DelegateProxies.SKImageRasterReleaseDelegateProxyForCoTaskMem, null));
	}

	public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.Create(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.Create(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.CreateCopy(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public unsafe static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		if (pixels == IntPtr.Zero)
		{
			throw new ArgumentNullException("pixels");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_image_new_raster_copy(&sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The Index8 color type and color table is no longer supported. Use FromPixelCopy(SKImageInfo, IntPtr, int) instead.")]
	public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable)
	{
		return FromPixelCopy(info, pixels, rowBytes);
	}

	public static SKImage FromPixelCopy(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		return GetObject(SkiaApi.sk_image_new_raster_copy_with_pixmap(pixmap.Handle));
	}

	public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.CreateCopy(pixels);
		return FromPixels(info, data, rowBytes);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromPixels (SKImageInfo, SKData, int) instead.")]
	public unsafe static SKImage FromPixelData(SKImageInfo info, SKData data, int rowBytes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_image_new_raster_data(&sKImageInfoNative, data.Handle, (IntPtr)rowBytes));
	}

	public static SKImage FromPixels(SKImageInfo info, SKData data)
	{
		return FromPixels(info, data, info.RowBytes);
	}

	public unsafe static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_image_new_raster_data(&sKImageInfoNative, data.Handle, (IntPtr)rowBytes));
	}

	public static SKImage FromPixels(SKImageInfo info, IntPtr pixels)
	{
		using SKPixmap pixmap = new SKPixmap(info, pixels, info.RowBytes);
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		using SKPixmap pixmap = new SKPixmap(info, pixels, rowBytes);
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKPixmap pixmap)
	{
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc)
	{
		return FromPixels(pixmap, releaseProc, null);
	}

	public unsafe static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc, object releaseContext)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		SKImageRasterReleaseDelegate managedDel = ((releaseProc != null && releaseContext != null) ? ((SKImageRasterReleaseDelegate)delegate(IntPtr addr, object _)
		{
			releaseProc(addr, releaseContext);
		}) : releaseProc);
		GCHandle gch;
		IntPtr contextPtr;
		SKImageRasterReleaseProxyDelegate releaseProc2 = DelegateProxies.Create(managedDel, DelegateProxies.SKImageRasterReleaseDelegateProxy, out gch, out contextPtr);
		return GetObject(SkiaApi.sk_image_new_raster(pixmap.Handle, releaseProc2, (void*)contextPtr));
	}

	public static SKImage FromEncodedData(SKData data, SKRectI subset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return FromEncodedData(data)?.Subset(subset);
	}

	public static SKImage FromEncodedData(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		IntPtr handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
		return GetObject(handle);
	}

	public static SKImage FromEncodedData(ReadOnlySpan<byte> data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("The data buffer was empty.");
		}
		using SKData data2 = SKData.CreateCopy(data);
		return FromEncodedData(data2);
	}

	public static SKImage FromEncodedData(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("The data buffer was empty.");
		}
		using SKData data2 = SKData.CreateCopy(data);
		return FromEncodedData(data2);
	}

	public static SKImage FromEncodedData(SKStream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKData sKData = SKData.Create(data);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromEncodedData(Stream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKData sKData = SKData.Create(data);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromEncodedData(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		using SKData sKData = SKData.Create(filename);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromBitmap(SKBitmap bitmap)
	{
		if (bitmap == null)
		{
			throw new ArgumentNullException("bitmap");
		}
		SKImage @object = GetObject(SkiaApi.sk_image_new_from_bitmap(bitmap.Handle));
		GC.KeepAlive(bitmap);
		return @object;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc)
	{
		return FromTexture(context, desc, SKAlphaType.Premul, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
	public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
	{
		return FromTexture(context, desc, alpha, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
	public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture(context, desc, alpha, releaseProc, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
	public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		GRBackendTexture texture = new GRBackendTexture(desc);
		return FromTexture(context, texture, desc.Origin, desc.Config.ToColorType(), alpha, null, releaseProc, releaseContext);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc)
	{
		return FromTexture(context, desc, SKAlphaType.Premul, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
	public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
	{
		return FromTexture(context, desc, alpha, null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
	public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture(context, desc, alpha, releaseProc, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
	public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		GRBackendTexture texture = new GRBackendTexture(desc);
		return FromTexture(context, texture, desc.Origin, desc.Config.ToColorType(), alpha, null, releaseProc, releaseContext);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromTexture((GRRecordingContext)context, texture, colorType);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc, releaseContext);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromTexture(context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromTexture(context, texture, origin, colorType, SKAlphaType.Premul, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromTexture(context, texture, origin, colorType, alpha, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromTexture(context, texture, origin, colorType, alpha, colorspace, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture(context, texture, origin, colorType, alpha, colorspace, releaseProc, null);
	}

	public unsafe static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		IntPtr colorSpace = colorspace?.Handle ?? IntPtr.Zero;
		SKImageTextureReleaseDelegate managedDel = ((releaseProc != null && releaseContext != null) ? ((SKImageTextureReleaseDelegate)delegate
		{
			releaseProc(releaseContext);
		}) : releaseProc);
		GCHandle gch;
		IntPtr contextPtr;
		SKImageTextureReleaseProxyDelegate releaseProc2 = DelegateProxies.Create(managedDel, DelegateProxies.SKImageTextureReleaseDelegateProxy, out gch, out contextPtr);
		return GetObject(SkiaApi.sk_image_new_from_texture(context.Handle, texture.Handle, origin, colorType.ToNative(), alpha, colorSpace, releaseProc2, (void*)contextPtr));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTextureDesc desc)
	{
		return FromAdoptedTexture(context, desc, SKAlphaType.Premul);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
	{
		GRBackendTexture texture = new GRBackendTexture(desc);
		return FromAdoptedTexture(context, texture, desc.Origin, desc.Config.ToColorType(), alpha, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKImage FromAdoptedTexture(GRContext context, GRGlBackendTextureDesc desc)
	{
		return FromAdoptedTexture(context, desc, SKAlphaType.Premul);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
	public static SKImage FromAdoptedTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
	{
		GRBackendTexture texture = new GRBackendTexture(desc);
		return FromAdoptedTexture(context, texture, desc.Origin, desc.Config.ToColorType(), alpha, null);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, colorType);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType, alpha);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromAdoptedTexture(context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromAdoptedTexture(context, texture, origin, colorType, SKAlphaType.Premul, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromAdoptedTexture(context, texture, origin, colorType, alpha, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		IntPtr colorSpace = colorspace?.Handle ?? IntPtr.Zero;
		return GetObject(SkiaApi.sk_image_new_from_adopted_texture(context.Handle, texture.Handle, origin, colorType.ToNative(), alpha, colorSpace));
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions)
	{
		return FromPicture(picture, dimensions, null, null);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix)
	{
		return FromPicture(picture, dimensions, &matrix, null);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKPaint paint)
	{
		return FromPicture(picture, dimensions, null, paint);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint)
	{
		return FromPicture(picture, dimensions, &matrix, paint);
	}

	private unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix* matrix, SKPaint paint)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		IntPtr paint2 = paint?.Handle ?? IntPtr.Zero;
		return GetObject(SkiaApi.sk_image_new_from_picture(picture.Handle, &dimensions, matrix, paint2));
	}

	public SKData Encode()
	{
		return SKData.GetObject(SkiaApi.sk_image_encode(Handle));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKData Encode(SKPixelSerializer serializer)
	{
		if (serializer == null)
		{
			throw new ArgumentNullException("serializer");
		}
		SKData encodedData = EncodedData;
		if (encodedData != null)
		{
			if (serializer.UseEncodedData(encodedData.Data, (ulong)encodedData.Size))
			{
				return encodedData;
			}
			encodedData.Dispose();
			encodedData = null;
		}
		if (!IsTextureBacked)
		{
			using (SKPixmap pixmap = PeekPixels())
			{
				return serializer.Encode(pixmap);
			}
		}
		if (IsTextureBacked)
		{
			SKImageInfo info = new SKImageInfo(Width, Height, ColorType, AlphaType, ColorSpace);
			using SKBitmap sKBitmap = new SKBitmap(info);
			using SKPixmap sKPixmap = sKBitmap.PeekPixels();
			if (sKPixmap != null && ReadPixels(sKPixmap, 0, 0))
			{
				return serializer.Encode(sKPixmap);
			}
		}
		return null;
	}

	public SKData Encode(SKEncodedImageFormat format, int quality)
	{
		return SKData.GetObject(SkiaApi.sk_image_encode_specific(Handle, format, quality));
	}

	public SKShader ToShader()
	{
		return ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY)
	{
		return SKShader.GetObject(SkiaApi.sk_image_make_shader(Handle, tileX, tileY, null));
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix)
	{
		return SKShader.GetObject(SkiaApi.sk_image_make_shader(Handle, tileX, tileY, &localMatrix));
	}

	public bool PeekPixels(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		bool flag = SkiaApi.sk_image_peek_pixels(Handle, pixmap.Handle);
		if (flag)
		{
			pixmap.pixelSource = this;
		}
		return flag;
	}

	public SKPixmap PeekPixels()
	{
		SKPixmap sKPixmap = new SKPixmap();
		if (!PeekPixels(sKPixmap))
		{
			sKPixmap.Dispose();
			sKPixmap = null;
		}
		return sKPixmap;
	}

	public bool IsValid(GRContext context)
	{
		return IsValid((GRRecordingContext)context);
	}

	public bool IsValid(GRRecordingContext context)
	{
		return SkiaApi.sk_image_is_valid(Handle, context?.Handle ?? IntPtr.Zero);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels)
	{
		return ReadPixels(dstInfo, dstPixels, dstInfo.RowBytes, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, srcX, srcY, SKImageCachingHint.Allow);
	}

	public unsafe bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref dstInfo);
		bool result = SkiaApi.sk_image_read_pixels(Handle, &sKImageInfoNative, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
		GC.KeepAlive(this);
		return result;
	}

	public bool ReadPixels(SKPixmap pixmap)
	{
		return ReadPixels(pixmap, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY)
	{
		return ReadPixels(pixmap, srcX, srcY, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		bool result = SkiaApi.sk_image_read_pixels_into_pixmap(Handle, pixmap.Handle, srcX, srcY, cachingHint);
		GC.KeepAlive(this);
		return result;
	}

	public bool ScalePixels(SKPixmap dst, SKFilterQuality quality)
	{
		return ScalePixels(dst, quality, SKImageCachingHint.Allow);
	}

	public bool ScalePixels(SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_image_scale_pixels(Handle, dst.Handle, quality, cachingHint);
	}

	public unsafe SKImage Subset(SKRectI subset)
	{
		return GetObject(SkiaApi.sk_image_make_subset(Handle, &subset));
	}

	public SKImage ToRasterImage()
	{
		return ToRasterImage(ensurePixelData: false);
	}

	public SKImage ToRasterImage(bool ensurePixelData)
	{
		if (!ensurePixelData)
		{
			return GetObject(SkiaApi.sk_image_make_non_texture_image(Handle));
		}
		return GetObject(SkiaApi.sk_image_make_raster_image(Handle));
	}

	public SKImage ToTextureImage(GRContext context)
	{
		return ToTextureImage(context, mipmapped: false);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ToTextureImage(GRContext) instead.")]
	public SKImage ToTextureImage(GRContext context, SKColorSpace colorspace)
	{
		return ToTextureImage(context, mipmapped: false);
	}

	public SKImage ToTextureImage(GRContext context, bool mipmapped)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return GetObject(SkiaApi.sk_image_make_texture_image(Handle, context.Handle, mipmapped));
	}

	public SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset)
	{
		SKPointI outOffset2;
		SKImage result = ApplyImageFilter(filter, subset, clipBounds, out outSubset, out outOffset2);
		outOffset = outOffset2;
		return result;
	}

	public unsafe SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		fixed (SKRectI* outSubset2 = &outSubset)
		{
			fixed (SKPointI* outOffset2 = &outOffset)
			{
				return GetObject(SkiaApi.sk_image_make_with_filter_legacy(Handle, filter.Handle, &subset, &clipBounds, outSubset2, outOffset2));
			}
		}
	}

	public SKImage ApplyImageFilter(GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		return ApplyImageFilter((GRRecordingContext)context, filter, subset, clipBounds, out outSubset, out outOffset);
	}

	public unsafe SKImage ApplyImageFilter(GRRecordingContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		fixed (SKRectI* outSubset2 = &outSubset)
		{
			fixed (SKPointI* outOffset2 = &outOffset)
			{
				return GetObject(SkiaApi.sk_image_make_with_filter(Handle, context?.Handle ?? IntPtr.Zero, filter.Handle, &subset, &clipBounds, outSubset2, outOffset2));
			}
		}
	}

	internal static SKImage GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKImage(h, o));
	}
}
