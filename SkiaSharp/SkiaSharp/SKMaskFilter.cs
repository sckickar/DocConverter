using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKMaskFilter : SKObject, ISKReferenceCounted
{
	private const float BlurSigmaScale = 0.57735f;

	public const int TableMaxLength = 256;

	internal SKMaskFilter(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static float ConvertRadiusToSigma(float radius)
	{
		if (!(radius > 0f))
		{
			return 0f;
		}
		return 0.57735f * radius + 0.5f;
	}

	public static float ConvertSigmaToRadius(float sigma)
	{
		if (!(sigma > 0.5f))
		{
			return 0f;
		}
		return (sigma - 0.5f) / 0.57735f;
	}

	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma)
	{
		return GetObject(SkiaApi.sk_maskfilter_new_blur(blurStyle, sigma));
	}

	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, bool respectCTM)
	{
		return GetObject(SkiaApi.sk_maskfilter_new_blur_with_flags(blurStyle, sigma, respectCTM));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateBlur(SKBlurStyle, float) instead.")]
	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags)
	{
		return CreateBlur(blurStyle, sigma, respectCTM: true);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateBlur(SKBlurStyle, float) instead.")]
	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder)
	{
		return CreateBlur(blurStyle, sigma, respectCTM: true);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateBlur(SKBlurStyle, float) instead.")]
	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags)
	{
		return CreateBlur(blurStyle, sigma, respectCTM: true);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateBlur(SKBlurStyle, float, bool) instead.")]
	public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM)
	{
		return CreateBlur(blurStyle, sigma, respectCTM);
	}

	public unsafe static SKMaskFilter CreateTable(byte[] table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		if (table.Length != 256)
		{
			throw new ArgumentException("Table must have a length of {SKColorTable.MaxLength}.", "table");
		}
		fixed (byte* table2 = table)
		{
			return GetObject(SkiaApi.sk_maskfilter_new_table(table2));
		}
	}

	public static SKMaskFilter CreateGamma(float gamma)
	{
		return GetObject(SkiaApi.sk_maskfilter_new_gamma(gamma));
	}

	public static SKMaskFilter CreateClip(byte min, byte max)
	{
		return GetObject(SkiaApi.sk_maskfilter_new_clip(min, max));
	}

	internal static SKMaskFilter GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKMaskFilter(h, o));
	}
}
