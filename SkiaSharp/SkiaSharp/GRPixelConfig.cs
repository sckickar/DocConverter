using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKColorType instead.")]
public enum GRPixelConfig
{
	Unknown,
	Alpha8,
	Gray8,
	Rgb565,
	Rgba4444,
	Rgba8888,
	Rgb888,
	Bgra8888,
	Srgba8888,
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The pixel configuration 'sBGRA 8888' is no longer supported in the native library.", true)]
	Sbgra8888,
	Rgba1010102,
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The pixel configuration 'floating-point RGBA' is no longer supported in the native library.", true)]
	RgbaFloat,
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("The pixel configuration 'floating-point RG' is no longer supported in the native library.", true)]
	RgFloat,
	AlphaHalf,
	RgbaHalf,
	Alpha8AsAlpha,
	Alpha8AsRed,
	AlphaHalfAsLum,
	AlphaHalfAsRed,
	Gray8AsLum,
	Gray8AsRed,
	RgbaHalfClamped,
	Alpha16,
	Rg1616,
	Rgba16161616,
	RgHalf,
	Rg88,
	Rgb888x,
	RgbEtc1
}
