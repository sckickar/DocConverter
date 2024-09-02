using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKImageFilter : SKObject, ISKReferenceCounted
{
	public class CropRect : SKObject
	{
		public unsafe SKRect Rect
		{
			get
			{
				SKRect result = default(SKRect);
				SkiaApi.sk_imagefilter_croprect_get_rect(Handle, &result);
				return result;
			}
		}

		public SKCropRectFlags Flags => (SKCropRectFlags)SkiaApi.sk_imagefilter_croprect_get_flags(Handle);

		internal CropRect(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		public CropRect()
			: this(SkiaApi.sk_imagefilter_croprect_new(), owns: true)
		{
		}

		public unsafe CropRect(SKRect rect, SKCropRectFlags flags = SKCropRectFlags.HasAll)
			: this(SkiaApi.sk_imagefilter_croprect_new_with_rect(&rect, (uint)flags), owns: true)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void DisposeNative()
		{
			SkiaApi.sk_imagefilter_croprect_destructor(Handle);
		}
	}

	internal SKImageFilter(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public unsafe static SKImageFilter CreateMatrix(SKMatrix matrix, SKFilterQuality quality, SKImageFilter input = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_matrix(&matrix, quality, input?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateAlphaThreshold(SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input = null)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetRect(region);
		return CreateAlphaThreshold(sKRegion, innerThreshold, outerThreshold, input);
	}

	public static SKImageFilter CreateAlphaThreshold(SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input = null)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_alpha_threshold(region.Handle, innerThreshold, outerThreshold, input?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter input = null, CropRect cropRect = null)
	{
		return CreateBlur(sigmaX, sigmaY, SKShaderTileMode.Decal, input, cropRect);
	}

	public static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_blur(sigmaX, sigmaY, tileMode, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter input = null, CropRect cropRect = null)
	{
		if (cf == null)
		{
			throw new ArgumentNullException("cf");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_color_filter(cf.Handle, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_compose(outer.Handle, inner.Handle));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateDisplacementMapEffect(SKColorChannel, SKColorChannel, float, SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
	public static SKImageFilter CreateDisplacementMapEffect(SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, CropRect cropRect = null)
	{
		return CreateDisplacementMapEffect(xChannelSelector.ToColorChannel(), yChannelSelector.ToColorChannel(), scale, displacement, input, cropRect);
	}

	public static SKImageFilter CreateDisplacementMapEffect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, CropRect cropRect = null)
	{
		if (displacement == null)
		{
			throw new ArgumentNullException("displacement");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_displacement_map_effect(xChannelSelector, yChannelSelector, scale, displacement.Handle, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateDropShadow or CreateDropShadowOnly instead.")]
	public static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, CropRect cropRect = null)
	{
		if (shadowMode != SKDropShadowImageFilterShadowMode.DrawShadowOnly)
		{
			return CreateDropShadow(dx, dy, sigmaX, sigmaY, color, input, cropRect);
		}
		return CreateDropShadowOnly(dx, dy, sigmaX, sigmaY, color, input, cropRect);
	}

	public static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_drop_shadow(dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateDropShadowOnly(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_drop_shadow_only(dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_diffuse(&direction, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_point_lit_diffuse(&location, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_diffuse(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_specular(&direction, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_point_lit_specular(&location, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_specular(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public unsafe static SKImageFilter CreateMagnifier(SKRect src, float inset, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_magnifier(&src, inset, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateMatrixConvolution(SKSizeI, float[], float, float, SKPointI, SKShaderTileMode, bool, SKImageFilter, SKImageFilter.CropRect) instead.")]
	public static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, CropRect cropRect = null)
	{
		return CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode.ToShaderTileMode(), convolveAlpha, input, cropRect);
	}

	public unsafe static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, CropRect cropRect = null)
	{
		if (kernel == null)
		{
			throw new ArgumentNullException("kernel");
		}
		if (kernel.Length != kernelSize.Width * kernelSize.Height)
		{
			throw new ArgumentException("Kernel length must match the dimensions of the kernel size (Width * Height).", "kernel");
		}
		fixed (float* kernel2 = kernel)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_matrix_convolution(&kernelSize, kernel2, gain, bias, &kernelOffset, tileMode, convolveAlpha, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
	public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKBlendMode mode, CropRect cropRect = null)
	{
		return CreateMerge(new SKImageFilter[2] { first, second }, cropRect);
	}

	public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, CropRect cropRect = null)
	{
		return CreateMerge(new SKImageFilter[2] { first, second }, cropRect);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateMerge(SKImageFilter[], SKImageFilter.CropRect) instead.")]
	public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKBlendMode[] modes, CropRect cropRect = null)
	{
		return CreateMerge(filters, cropRect);
	}

	public unsafe static SKImageFilter CreateMerge(SKImageFilter[] filters, CropRect cropRect = null)
	{
		if (filters == null)
		{
			throw new ArgumentNullException("filters");
		}
		IntPtr[] array = new IntPtr[filters.Length];
		for (int i = 0; i < filters.Length; i++)
		{
			array[i] = filters[i]?.Handle ?? IntPtr.Zero;
		}
		fixed (IntPtr* filters2 = array)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_merge(filters2, filters.Length, cropRect?.Handle ?? IntPtr.Zero));
		}
	}

	public static SKImageFilter CreateDilate(int radiusX, int radiusY, SKImageFilter input = null, CropRect cropRect = null)
	{
		return CreateDilate((float)radiusX, (float)radiusY, input, cropRect);
	}

	public static SKImageFilter CreateDilate(float radiusX, float radiusY, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_dilate(radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateErode(int radiusX, int radiusY, SKImageFilter input = null, CropRect cropRect = null)
	{
		return CreateErode((float)radiusX, (float)radiusY, input, cropRect);
	}

	public static SKImageFilter CreateErode(float radiusX, float radiusY, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_erode(radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateOffset(float dx, float dy, SKImageFilter input = null, CropRect cropRect = null)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_offset(dx, dy, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreatePicture(SKPicture picture)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_picture(picture.Handle));
	}

	public unsafe static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_picture_with_croprect(picture.Handle, &cropRect));
	}

	public unsafe static SKImageFilter CreateTile(SKRect src, SKRect dst, SKImageFilter input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input.Handle));
	}

	public static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter background, SKImageFilter foreground = null, CropRect cropRect = null)
	{
		if (background == null)
		{
			throw new ArgumentNullException("background");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_xfermode(mode, background.Handle, foreground?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground = null, CropRect cropRect = null)
	{
		if (background == null)
		{
			throw new ArgumentNullException("background");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_arithmetic(k1, k2, k3, k4, enforcePMColor, background.Handle, foreground?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
	}

	public static SKImageFilter CreateImage(SKImage image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_image_source_default(image.Handle));
	}

	public unsafe static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_image_source(image.Handle, &src, &dst, filterQuality));
	}

	public static SKImageFilter CreatePaint(SKPaint paint, CropRect cropRect = null)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_paint(paint.Handle, cropRect?.Handle ?? IntPtr.Zero));
	}

	internal static SKImageFilter GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKImageFilter(h, o));
	}
}
