using System;
using System.ComponentModel;

namespace SkiaSharp;

public static class SkiaExtensions
{
	public static bool IsBgr(this SKPixelGeometry pg)
	{
		if (pg != SKPixelGeometry.BgrHorizontal)
		{
			return pg == SKPixelGeometry.BgrVertical;
		}
		return true;
	}

	public static bool IsRgb(this SKPixelGeometry pg)
	{
		if (pg != SKPixelGeometry.RgbHorizontal)
		{
			return pg == SKPixelGeometry.RgbVertical;
		}
		return true;
	}

	public static bool IsVertical(this SKPixelGeometry pg)
	{
		if (pg != SKPixelGeometry.BgrVertical)
		{
			return pg == SKPixelGeometry.RgbVertical;
		}
		return true;
	}

	public static bool IsHorizontal(this SKPixelGeometry pg)
	{
		if (pg != SKPixelGeometry.BgrHorizontal)
		{
			return pg == SKPixelGeometry.RgbHorizontal;
		}
		return true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKTextEncoding ToTextEncoding(this SKEncoding encoding)
	{
		return encoding switch
		{
			SKEncoding.Utf8 => SKTextEncoding.Utf8, 
			SKEncoding.Utf16 => SKTextEncoding.Utf16, 
			SKEncoding.Utf32 => SKTextEncoding.Utf32, 
			_ => throw new ArgumentOutOfRangeException("encoding"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	internal static SKEncoding ToEncoding(this SKTextEncoding encoding)
	{
		return encoding switch
		{
			SKTextEncoding.Utf8 => SKEncoding.Utf8, 
			SKTextEncoding.Utf16 => SKEncoding.Utf16, 
			SKTextEncoding.Utf32 => SKEncoding.Utf32, 
			_ => throw new ArgumentOutOfRangeException("encoding"), 
		};
	}

	public static int GetBytesPerPixel(this SKColorType colorType)
	{
		return colorType switch
		{
			SKColorType.Unknown => 0, 
			SKColorType.Alpha8 => 1, 
			SKColorType.Gray8 => 1, 
			SKColorType.Rgb565 => 2, 
			SKColorType.Argb4444 => 2, 
			SKColorType.Rg88 => 2, 
			SKColorType.Alpha16 => 2, 
			SKColorType.AlphaF16 => 2, 
			SKColorType.Bgra8888 => 4, 
			SKColorType.Bgra1010102 => 4, 
			SKColorType.Bgr101010x => 4, 
			SKColorType.Rgba8888 => 4, 
			SKColorType.Rgb888x => 4, 
			SKColorType.Rgba1010102 => 4, 
			SKColorType.Rgb101010x => 4, 
			SKColorType.Rg1616 => 4, 
			SKColorType.RgF16 => 4, 
			SKColorType.RgbaF16Clamped => 8, 
			SKColorType.RgbaF16 => 8, 
			SKColorType.Rgba16161616 => 8, 
			SKColorType.RgbaF32 => 16, 
			_ => throw new ArgumentOutOfRangeException("colorType"), 
		};
	}

	public static SKAlphaType GetAlphaType(this SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Premul)
	{
		switch (colorType)
		{
		case SKColorType.Unknown:
			alphaType = SKAlphaType.Unknown;
			break;
		case SKColorType.Alpha8:
		case SKColorType.AlphaF16:
		case SKColorType.Alpha16:
			if (SKAlphaType.Unpremul == alphaType)
			{
				alphaType = SKAlphaType.Premul;
			}
			break;
		case SKColorType.Rgb565:
		case SKColorType.Rgb888x:
		case SKColorType.Rgb101010x:
		case SKColorType.Gray8:
		case SKColorType.Rg88:
		case SKColorType.RgF16:
		case SKColorType.Rg1616:
		case SKColorType.Bgr101010x:
			alphaType = SKAlphaType.Opaque;
			break;
		default:
			throw new ArgumentOutOfRangeException("colorType");
		case SKColorType.Argb4444:
		case SKColorType.Rgba8888:
		case SKColorType.Bgra8888:
		case SKColorType.Rgba1010102:
		case SKColorType.RgbaF16:
		case SKColorType.RgbaF16Clamped:
		case SKColorType.RgbaF32:
		case SKColorType.Rgba16161616:
		case SKColorType.Bgra1010102:
			break;
		}
		return alphaType;
	}

	internal static GRBackendNative ToNative(this GRBackend backend)
	{
		return backend switch
		{
			GRBackend.Metal => GRBackendNative.Metal, 
			GRBackend.OpenGL => GRBackendNative.OpenGL, 
			GRBackend.Vulkan => GRBackendNative.Vulkan, 
			GRBackend.Dawn => GRBackendNative.Dawn, 
			GRBackend.Direct3D => GRBackendNative.Direct3D, 
			_ => throw new ArgumentOutOfRangeException("backend"), 
		};
	}

	internal static GRBackend FromNative(this GRBackendNative backend)
	{
		return backend switch
		{
			GRBackendNative.Metal => GRBackend.Metal, 
			GRBackendNative.OpenGL => GRBackend.OpenGL, 
			GRBackendNative.Vulkan => GRBackend.Vulkan, 
			GRBackendNative.Dawn => GRBackend.Dawn, 
			GRBackendNative.Direct3D => GRBackend.Direct3D, 
			_ => throw new ArgumentOutOfRangeException("backend"), 
		};
	}

	internal static SKColorTypeNative ToNative(this SKColorType colorType)
	{
		return colorType switch
		{
			SKColorType.Unknown => SKColorTypeNative.Unknown, 
			SKColorType.Alpha8 => SKColorTypeNative.Alpha8, 
			SKColorType.Rgb565 => SKColorTypeNative.Rgb565, 
			SKColorType.Argb4444 => SKColorTypeNative.Argb4444, 
			SKColorType.Rgba8888 => SKColorTypeNative.Rgba8888, 
			SKColorType.Rgb888x => SKColorTypeNative.Rgb888x, 
			SKColorType.Bgra8888 => SKColorTypeNative.Bgra8888, 
			SKColorType.Rgba1010102 => SKColorTypeNative.Rgba1010102, 
			SKColorType.Rgb101010x => SKColorTypeNative.Rgb101010x, 
			SKColorType.Gray8 => SKColorTypeNative.Gray8, 
			SKColorType.RgbaF16Clamped => SKColorTypeNative.RgbaF16Norm, 
			SKColorType.RgbaF16 => SKColorTypeNative.RgbaF16, 
			SKColorType.RgbaF32 => SKColorTypeNative.RgbaF32, 
			SKColorType.Rg88 => SKColorTypeNative.R8g8Unorm, 
			SKColorType.AlphaF16 => SKColorTypeNative.A16Float, 
			SKColorType.RgF16 => SKColorTypeNative.R16g16Float, 
			SKColorType.Alpha16 => SKColorTypeNative.A16Unorm, 
			SKColorType.Rg1616 => SKColorTypeNative.R16g16Unorm, 
			SKColorType.Rgba16161616 => SKColorTypeNative.R16g16b16a16Unorm, 
			SKColorType.Bgra1010102 => SKColorTypeNative.Bgra1010102, 
			SKColorType.Bgr101010x => SKColorTypeNative.Bgr101010x, 
			_ => throw new ArgumentOutOfRangeException("colorType"), 
		};
	}

	internal static SKColorType FromNative(this SKColorTypeNative colorType)
	{
		return colorType switch
		{
			SKColorTypeNative.Unknown => SKColorType.Unknown, 
			SKColorTypeNative.Alpha8 => SKColorType.Alpha8, 
			SKColorTypeNative.Rgb565 => SKColorType.Rgb565, 
			SKColorTypeNative.Argb4444 => SKColorType.Argb4444, 
			SKColorTypeNative.Rgba8888 => SKColorType.Rgba8888, 
			SKColorTypeNative.Rgb888x => SKColorType.Rgb888x, 
			SKColorTypeNative.Bgra8888 => SKColorType.Bgra8888, 
			SKColorTypeNative.Rgba1010102 => SKColorType.Rgba1010102, 
			SKColorTypeNative.Rgb101010x => SKColorType.Rgb101010x, 
			SKColorTypeNative.Gray8 => SKColorType.Gray8, 
			SKColorTypeNative.RgbaF16Norm => SKColorType.RgbaF16Clamped, 
			SKColorTypeNative.RgbaF16 => SKColorType.RgbaF16, 
			SKColorTypeNative.RgbaF32 => SKColorType.RgbaF32, 
			SKColorTypeNative.R8g8Unorm => SKColorType.Rg88, 
			SKColorTypeNative.A16Float => SKColorType.AlphaF16, 
			SKColorTypeNative.R16g16Float => SKColorType.RgF16, 
			SKColorTypeNative.A16Unorm => SKColorType.Alpha16, 
			SKColorTypeNative.R16g16Unorm => SKColorType.Rg1616, 
			SKColorTypeNative.R16g16b16a16Unorm => SKColorType.Rgba16161616, 
			SKColorTypeNative.Bgra1010102 => SKColorType.Bgra1010102, 
			SKColorTypeNative.Bgr101010x => SKColorType.Bgr101010x, 
			_ => throw new ArgumentOutOfRangeException("colorType"), 
		};
	}

	public static uint ToGlSizedFormat(this SKColorType colorType)
	{
		return colorType switch
		{
			SKColorType.Unknown => 0u, 
			SKColorType.Alpha8 => 32828u, 
			SKColorType.Gray8 => 32832u, 
			SKColorType.Rgb565 => 36194u, 
			SKColorType.Argb4444 => 32854u, 
			SKColorType.Rgba8888 => 32856u, 
			SKColorType.Rgb888x => 32849u, 
			SKColorType.Bgra8888 => 37793u, 
			SKColorType.Rgba1010102 => 32857u, 
			SKColorType.AlphaF16 => 33325u, 
			SKColorType.RgbaF16 => 34842u, 
			SKColorType.RgbaF16Clamped => 34842u, 
			SKColorType.Alpha16 => 33322u, 
			SKColorType.Rg1616 => 33324u, 
			SKColorType.Rgba16161616 => 32859u, 
			SKColorType.RgF16 => 33327u, 
			SKColorType.Rg88 => 33323u, 
			SKColorType.Rgb101010x => 0u, 
			SKColorType.RgbaF32 => 0u, 
			_ => throw new ArgumentOutOfRangeException("colorType"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKColorType instead.")]
	public static uint ToGlSizedFormat(this GRPixelConfig config)
	{
		return config switch
		{
			GRPixelConfig.Unknown => 0u, 
			GRPixelConfig.Alpha8 => 32828u, 
			GRPixelConfig.Alpha8AsAlpha => 32828u, 
			GRPixelConfig.Alpha8AsRed => 32828u, 
			GRPixelConfig.Gray8 => 32832u, 
			GRPixelConfig.Gray8AsLum => 32832u, 
			GRPixelConfig.Gray8AsRed => 32832u, 
			GRPixelConfig.Rgb565 => 36194u, 
			GRPixelConfig.Rgba4444 => 32854u, 
			GRPixelConfig.Rgba8888 => 32856u, 
			GRPixelConfig.Rgb888 => 32849u, 
			GRPixelConfig.Rgb888x => 32856u, 
			GRPixelConfig.Rg88 => 33323u, 
			GRPixelConfig.Bgra8888 => 37793u, 
			GRPixelConfig.Srgba8888 => 35907u, 
			GRPixelConfig.Rgba1010102 => 32857u, 
			GRPixelConfig.AlphaHalf => 33325u, 
			GRPixelConfig.AlphaHalfAsLum => 34846u, 
			GRPixelConfig.AlphaHalfAsRed => 33325u, 
			GRPixelConfig.RgbaHalf => 34842u, 
			GRPixelConfig.RgbaHalfClamped => 34842u, 
			GRPixelConfig.RgbEtc1 => 36196u, 
			GRPixelConfig.Alpha16 => 33322u, 
			GRPixelConfig.Rg1616 => 33324u, 
			GRPixelConfig.Rgba16161616 => 32859u, 
			GRPixelConfig.RgHalf => 33327u, 
			_ => throw new ArgumentOutOfRangeException("config"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKColorType instead.")]
	public static GRPixelConfig ToPixelConfig(this SKColorType colorType)
	{
		return colorType switch
		{
			SKColorType.Unknown => GRPixelConfig.Unknown, 
			SKColorType.Alpha8 => GRPixelConfig.Alpha8, 
			SKColorType.Gray8 => GRPixelConfig.Gray8, 
			SKColorType.Rgb565 => GRPixelConfig.Rgb565, 
			SKColorType.Argb4444 => GRPixelConfig.Rgba4444, 
			SKColorType.Rgba8888 => GRPixelConfig.Rgba8888, 
			SKColorType.Rgb888x => GRPixelConfig.Rgb888, 
			SKColorType.Bgra8888 => GRPixelConfig.Bgra8888, 
			SKColorType.Rgba1010102 => GRPixelConfig.Rgba1010102, 
			SKColorType.Bgra1010102 => GRPixelConfig.Unknown, 
			SKColorType.AlphaF16 => GRPixelConfig.AlphaHalf, 
			SKColorType.RgbaF16 => GRPixelConfig.RgbaHalf, 
			SKColorType.RgbaF16Clamped => GRPixelConfig.RgbaHalfClamped, 
			SKColorType.Alpha16 => GRPixelConfig.Alpha16, 
			SKColorType.Rg1616 => GRPixelConfig.Rg1616, 
			SKColorType.Rgba16161616 => GRPixelConfig.Rgba16161616, 
			SKColorType.RgF16 => GRPixelConfig.RgHalf, 
			SKColorType.Rg88 => GRPixelConfig.Rg88, 
			SKColorType.Rgb101010x => GRPixelConfig.Unknown, 
			SKColorType.Bgr101010x => GRPixelConfig.Unknown, 
			SKColorType.RgbaF32 => GRPixelConfig.Unknown, 
			_ => throw new ArgumentOutOfRangeException("colorType"), 
		};
	}

	[Obsolete("Use SKColorType instead.")]
	public static SKColorType ToColorType(this GRPixelConfig config)
	{
		return config switch
		{
			GRPixelConfig.Unknown => SKColorType.Unknown, 
			GRPixelConfig.Alpha8 => SKColorType.Alpha8, 
			GRPixelConfig.Gray8 => SKColorType.Gray8, 
			GRPixelConfig.Rgb565 => SKColorType.Rgb565, 
			GRPixelConfig.Rgba4444 => SKColorType.Argb4444, 
			GRPixelConfig.Rgba8888 => SKColorType.Rgba8888, 
			GRPixelConfig.Rgb888 => SKColorType.Rgb888x, 
			GRPixelConfig.Bgra8888 => SKColorType.Bgra8888, 
			GRPixelConfig.Srgba8888 => SKColorType.Rgba8888, 
			GRPixelConfig.Rgba1010102 => SKColorType.Rgba1010102, 
			GRPixelConfig.AlphaHalf => SKColorType.AlphaF16, 
			GRPixelConfig.RgbaHalf => SKColorType.RgbaF16, 
			GRPixelConfig.Alpha8AsAlpha => SKColorType.Alpha8, 
			GRPixelConfig.Alpha8AsRed => SKColorType.Alpha8, 
			GRPixelConfig.AlphaHalfAsLum => SKColorType.AlphaF16, 
			GRPixelConfig.AlphaHalfAsRed => SKColorType.AlphaF16, 
			GRPixelConfig.Gray8AsLum => SKColorType.Gray8, 
			GRPixelConfig.Gray8AsRed => SKColorType.Gray8, 
			GRPixelConfig.RgbaHalfClamped => SKColorType.RgbaF16Clamped, 
			GRPixelConfig.Alpha16 => SKColorType.Alpha16, 
			GRPixelConfig.Rg1616 => SKColorType.Rg1616, 
			GRPixelConfig.Rgba16161616 => SKColorType.Rgba16161616, 
			GRPixelConfig.RgHalf => SKColorType.RgF16, 
			GRPixelConfig.Rg88 => SKColorType.Rg88, 
			GRPixelConfig.Rgb888x => SKColorType.Rgb888x, 
			GRPixelConfig.RgbEtc1 => SKColorType.Rgb888x, 
			_ => throw new ArgumentOutOfRangeException("config"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKFilterQuality ToFilterQuality(this SKBitmapResizeMethod method)
	{
		switch (method)
		{
		case SKBitmapResizeMethod.Box:
		case SKBitmapResizeMethod.Triangle:
			return SKFilterQuality.Low;
		case SKBitmapResizeMethod.Lanczos3:
			return SKFilterQuality.Medium;
		case SKBitmapResizeMethod.Hamming:
		case SKBitmapResizeMethod.Mitchell:
			return SKFilterQuality.High;
		default:
			return SKFilterQuality.Medium;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKColorSpaceTransferFn ToColorSpaceTransferFn(this SKColorSpaceRenderTargetGamma gamma)
	{
		return gamma switch
		{
			SKColorSpaceRenderTargetGamma.Linear => SKColorSpaceTransferFn.Linear, 
			SKColorSpaceRenderTargetGamma.Srgb => SKColorSpaceTransferFn.Srgb, 
			_ => throw new ArgumentOutOfRangeException("gamma"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKColorSpaceTransferFn ToColorSpaceTransferFn(this SKNamedGamma gamma)
	{
		return gamma switch
		{
			SKNamedGamma.Linear => SKColorSpaceTransferFn.Linear, 
			SKNamedGamma.Srgb => SKColorSpaceTransferFn.Srgb, 
			SKNamedGamma.TwoDotTwoCurve => SKColorSpaceTransferFn.TwoDotTwo, 
			SKNamedGamma.NonStandard => SKColorSpaceTransferFn.Empty, 
			_ => throw new ArgumentOutOfRangeException("gamma"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKColorSpaceXyz ToColorSpaceXyz(this SKColorSpaceGamut gamut)
	{
		return gamut switch
		{
			SKColorSpaceGamut.AdobeRgb => SKColorSpaceXyz.AdobeRgb, 
			SKColorSpaceGamut.Dcip3D65 => SKColorSpaceXyz.Dcip3, 
			SKColorSpaceGamut.Rec2020 => SKColorSpaceXyz.Rec2020, 
			SKColorSpaceGamut.Srgb => SKColorSpaceXyz.Srgb, 
			_ => throw new ArgumentOutOfRangeException("gamut"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public static SKColorSpaceXyz ToColorSpaceXyz(this SKMatrix44 matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		float[] array = matrix.ToRowMajor();
		return new SKColorSpaceXyz(array[0], array[1], array[2], array[4], array[5], array[6], array[8], array[9], array[10]);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKColorChannel instead.")]
	public static SKColorChannel ToColorChannel(this SKDisplacementMapEffectChannelSelectorType channelSelectorType)
	{
		return channelSelectorType switch
		{
			SKDisplacementMapEffectChannelSelectorType.R => SKColorChannel.R, 
			SKDisplacementMapEffectChannelSelectorType.G => SKColorChannel.G, 
			SKDisplacementMapEffectChannelSelectorType.B => SKColorChannel.B, 
			SKDisplacementMapEffectChannelSelectorType.A => SKColorChannel.A, 
			_ => SKColorChannel.B, 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKShaderTileMode instead.")]
	public static SKShaderTileMode ToShaderTileMode(this SKMatrixConvolutionTileMode tileMode)
	{
		return tileMode switch
		{
			SKMatrixConvolutionTileMode.Clamp => SKShaderTileMode.Clamp, 
			SKMatrixConvolutionTileMode.Repeat => SKShaderTileMode.Repeat, 
			_ => SKShaderTileMode.Decal, 
		};
	}
}
