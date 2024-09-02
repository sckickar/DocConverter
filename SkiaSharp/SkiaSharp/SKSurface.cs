using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKSurface : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	public SKCanvas Canvas => SKObject.OwnedBy(SKCanvas.GetObject(SkiaApi.sk_surface_get_canvas(Handle), owns: false, unrefExisting: false), this);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SurfaceProperties instead.")]
	public SKSurfaceProps SurfaceProps
	{
		get
		{
			SKSurfaceProperties surfaceProperties = SurfaceProperties;
			SKSurfaceProps result = default(SKSurfaceProps);
			result.Flags = surfaceProperties.Flags;
			result.PixelGeometry = surfaceProperties.PixelGeometry;
			return result;
		}
	}

	public SKSurfaceProperties SurfaceProperties => SKObject.OwnedBy(SKSurfaceProperties.GetObject(SkiaApi.sk_surface_get_props(Handle), owns: false), this);

	public GRRecordingContext Context => GRRecordingContext.GetObject(SkiaApi.sk_surface_get_recording_context(Handle), owns: false, unrefExisting: false);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo) instead.")]
	public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType)
	{
		return Create(new SKImageInfo(width, height, colorType, alphaType));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
	public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props)
	{
		return Create(new SKImageInfo(width, height, colorType, alphaType), props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo, IntPtr, int) instead.")]
	public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes)
	{
		return Create(new SKImageInfo(width, height, colorType, alphaType), pixels, rowBytes);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProperties) instead.")]
	public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props)
	{
		return Create(new SKImageInfo(width, height, colorType, alphaType), pixels, rowBytes, props);
	}

	internal SKSurface(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
	public static SKSurface Create(SKImageInfo info, SKSurfaceProps props)
	{
		return Create(info, 0, new SKSurfaceProperties(props));
	}

	public static SKSurface Create(SKImageInfo info)
	{
		return Create(info, 0, null);
	}

	public static SKSurface Create(SKImageInfo info, int rowBytes)
	{
		return Create(info, rowBytes, null);
	}

	public static SKSurface Create(SKImageInfo info, SKSurfaceProperties props)
	{
		return Create(info, 0, props);
	}

	public unsafe static SKSurface Create(SKImageInfo info, int rowBytes, SKSurfaceProperties props)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_surface_new_raster(&sKImageInfoNative, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKPixmap, SKSurfaceProperties) instead.")]
	public static SKSurface Create(SKPixmap pixmap, SKSurfaceProps props)
	{
		return Create(pixmap, new SKSurfaceProperties(props));
	}

	public static SKSurface Create(SKPixmap pixmap)
	{
		return Create(pixmap, null);
	}

	public static SKSurface Create(SKPixmap pixmap, SKSurfaceProperties props)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		return Create(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, null, null, props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKImageInfo, IntPtr, rowBytes, SKSurfaceProperties) instead.")]
	public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
	{
		return Create(info, pixels, rowBytes, null, null, new SKSurfaceProperties(props));
	}

	public static SKSurface Create(SKImageInfo info, IntPtr pixels)
	{
		return Create(info, pixels, info.RowBytes, null, null, null);
	}

	public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		return Create(info, pixels, rowBytes, null, null, null);
	}

	public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context)
	{
		return Create(info, pixels, rowBytes, releaseProc, context, null);
	}

	public static SKSurface Create(SKImageInfo info, IntPtr pixels, SKSurfaceProperties props)
	{
		return Create(info, pixels, info.RowBytes, null, null, props);
	}

	public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props)
	{
		return Create(info, pixels, rowBytes, null, null, props);
	}

	public unsafe static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SKSurfaceReleaseDelegate managedDel = ((releaseProc != null && context != null) ? ((SKSurfaceReleaseDelegate)delegate(IntPtr addr, object _)
		{
			releaseProc(addr, context);
		}) : releaseProc);
		GCHandle gch;
		IntPtr contextPtr;
		SKSurfaceRasterReleaseProxyDelegate releaseProc2 = DelegateProxies.Create(managedDel, DelegateProxies.SKSurfaceReleaseDelegateProxy, out gch, out contextPtr);
		return GetObject(SkiaApi.sk_surface_new_raster_direct(&sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes, releaseProc2, (void*)contextPtr, props?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKSurface Create(GRContext context, GRBackendRenderTargetDesc desc)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		GRBackendRenderTarget renderTarget = new GRBackendRenderTarget(context.Backend, desc);
		return Create(context, renderTarget, desc.Origin, desc.Config.ToColorType(), null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface Create(GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		GRBackendRenderTarget renderTarget = new GRBackendRenderTarget(context.Backend, desc);
		return Create(context, renderTarget, desc.Origin, desc.Config.ToColorType(), null, new SKSurfaceProperties(props));
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType)
	{
		return Create((GRRecordingContext)context, renderTarget, colorType);
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return Create((GRRecordingContext)context, renderTarget, origin, colorType);
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace)
	{
		return Create((GRRecordingContext)context, renderTarget, origin, colorType, colorspace);
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, renderTarget, colorType, props);
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, renderTarget, origin, colorType, props);
	}

	public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, renderTarget, origin, colorType, colorspace, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType)
	{
		return Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return Create(context, renderTarget, origin, colorType, null, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace)
	{
		return Create(context, renderTarget, origin, colorType, colorspace, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, renderTarget, origin, colorType, null, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (renderTarget == null)
		{
			throw new ArgumentNullException("renderTarget");
		}
		return GetObject(SkiaApi.sk_surface_new_backend_render_target(context.Handle, renderTarget.Handle, origin, colorType.ToNative(), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
	public static SKSurface Create(GRContext context, GRGlBackendTextureDesc desc)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
	public static SKSurface Create(GRContext context, GRBackendTextureDesc desc)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), null, null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface Create(GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), null, new SKSurfaceProperties(props));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface Create(GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), null, new SKSurfaceProperties(props));
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return Create((GRRecordingContext)context, texture, colorType);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return Create((GRRecordingContext)context, texture, origin, colorType);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType)
	{
		return Create((GRRecordingContext)context, texture, origin, sampleCount, colorType);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace)
	{
		return Create((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, texture, colorType, props);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, texture, origin, colorType, props);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, texture, origin, sampleCount, colorType, props);
	}

	public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return Create(context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return Create(context, texture, origin, 0, colorType, null, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType)
	{
		return Create(context, texture, origin, sampleCount, colorType, null, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace)
	{
		return Create(context, texture, origin, sampleCount, colorType, colorspace, null);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, origin, 0, colorType, null, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, origin, sampleCount, colorType, null, props);
	}

	public static SKSurface Create(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return GetObject(SkiaApi.sk_surface_new_backend_texture(context.Handle, texture.Handle, origin, sampleCount, colorType.ToNative(), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRGlBackendTextureDesc desc)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTextureDesc desc)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), new SKSurfaceProperties(props));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props)
	{
		return Create(context, new GRBackendTexture(desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType(), new SKSurfaceProperties(props));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, SKColorType) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return Create(context, texture, colorType);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return Create(context, texture, origin, colorType);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType)
	{
		return Create(context, texture, origin, sampleCount, colorType);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace)
	{
		return Create(context, texture, origin, sampleCount, colorType, colorspace);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, colorType, props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, origin, colorType, props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props)
	{
		return Create(context, texture, origin, sampleCount, colorType, null, props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace, SKSurfaceProperties) instead.")]
	public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		return Create(context, texture, origin, sampleCount, colorType, colorspace, props);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(GRContext, bool, SKImageInfo, int, SKSurfaceProperties) instead.")]
	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props)
	{
		return Create(context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, new SKSurfaceProperties(props), shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info)
	{
		return Create((GRRecordingContext)context, budgeted, info);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount)
	{
		return Create((GRRecordingContext)context, budgeted, info, sampleCount);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin)
	{
		return Create((GRRecordingContext)context, budgeted, info, sampleCount, origin);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, budgeted, info, props);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props)
	{
		return Create((GRRecordingContext)context, budgeted, info, sampleCount, props);
	}

	public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
	{
		return Create((GRRecordingContext)context, budgeted, info, sampleCount, origin, props, shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info)
	{
		return Create(context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, null, shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount)
	{
		return Create(context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin)
	{
		return Create(context, budgeted, info, sampleCount, origin, null, shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props)
	{
		return Create(context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, shouldCreateWithMips: false);
	}

	public static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props)
	{
		return Create(context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, shouldCreateWithMips: false);
	}

	public unsafe static SKSurface Create(GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_surface_new_render_target(context.Handle, budgeted, &sKImageInfoNative, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
	}

	public static SKSurface CreateNull(int width, int height)
	{
		return GetObject(SkiaApi.sk_surface_new_null(width, height));
	}

	public SKImage Snapshot()
	{
		return SKImage.GetObject(SkiaApi.sk_surface_new_image_snapshot(Handle));
	}

	public unsafe SKImage Snapshot(SKRectI bounds)
	{
		return SKImage.GetObject(SkiaApi.sk_surface_new_image_snapshot_with_crop(Handle, &bounds));
	}

	public void Draw(SKCanvas canvas, float x, float y, SKPaint paint)
	{
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		SkiaApi.sk_surface_draw(Handle, canvas.Handle, x, y, paint?.Handle ?? IntPtr.Zero);
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
		bool flag = SkiaApi.sk_surface_peek_pixels(Handle, pixmap.Handle);
		if (flag)
		{
			pixmap.pixelSource = this;
		}
		return flag;
	}

	public unsafe bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref dstInfo);
		bool result = SkiaApi.sk_surface_read_pixels(Handle, &sKImageInfoNative, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		GC.KeepAlive(this);
		return result;
	}

	public void Flush()
	{
		Flush(submit: true);
	}

	public void Flush(bool submit, bool synchronous = false)
	{
		if (submit)
		{
			SkiaApi.sk_surface_flush_and_submit(Handle, synchronous);
		}
		else
		{
			SkiaApi.sk_surface_flush(Handle);
		}
	}

	internal static SKSurface GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new SKSurface(handle, owns: true);
		}
		return null;
	}
}
