using System;
using System.ComponentModel;

namespace SkiaSharp;

public class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted, ISKReferenceCounted
{
	private sealed class SKColorSpaceStatic : SKColorSpace
	{
		internal SKColorSpaceStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly SKColorSpace srgb;

	private static readonly SKColorSpace srgbLinear;

	public bool GammaIsCloseToSrgb => SkiaApi.sk_colorspace_gamma_close_to_srgb(Handle);

	public bool GammaIsLinear => SkiaApi.sk_colorspace_gamma_is_linear(Handle);

	public bool IsSrgb => SkiaApi.sk_colorspace_is_srgb(Handle);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKColorSpaceType Type => SKColorSpaceType.Rgb;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetNumericalTransferFunction() instead.")]
	public SKNamedGamma NamedGamma
	{
		get
		{
			SKColorSpaceTransferFn numericalTransferFunction = GetNumericalTransferFunction();
			SKColorSpaceTransferFn sKColorSpaceTransferFn = numericalTransferFunction;
			if (numericalTransferFunction == SKColorSpaceTransferFn.Empty)
			{
				return SKNamedGamma.NonStandard;
			}
			if (numericalTransferFunction == SKColorSpaceTransferFn.Linear)
			{
				return SKNamedGamma.Linear;
			}
			if (numericalTransferFunction == SKColorSpaceTransferFn.Srgb)
			{
				return SKNamedGamma.Srgb;
			}
			if (numericalTransferFunction == SKColorSpaceTransferFn.TwoDotTwo)
			{
				return SKNamedGamma.TwoDotTwoCurve;
			}
			return SKNamedGamma.NonStandard;
		}
	}

	public bool IsNumericalTransferFunction
	{
		get
		{
			SKColorSpaceTransferFn fn;
			return GetNumericalTransferFunction(out fn);
		}
	}

	static SKColorSpace()
	{
		srgb = new SKColorSpaceStatic(SkiaApi.sk_colorspace_new_srgb());
		srgbLinear = new SKColorSpaceStatic(SkiaApi.sk_colorspace_new_srgb_linear());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKColorSpace(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	void ISKNonVirtualReferenceCounted.ReferenceNative()
	{
		SkiaApi.sk_colorspace_ref(Handle);
	}

	void ISKNonVirtualReferenceCounted.UnreferenceNative()
	{
		SkiaApi.sk_colorspace_unref(Handle);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static bool Equal(SKColorSpace left, SKColorSpace right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return SkiaApi.sk_colorspace_equals(left.Handle, right.Handle);
	}

	public static SKColorSpace CreateSrgb()
	{
		return srgb;
	}

	public static SKColorSpace CreateSrgbLinear()
	{
		return srgbLinear;
	}

	public static SKColorSpace CreateIcc(IntPtr input, long length)
	{
		return CreateIcc(SKColorSpaceIccProfile.Create(input, length));
	}

	public unsafe static SKColorSpace CreateIcc(byte[] input, long length)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		fixed (byte* ptr = input)
		{
			return CreateIcc(SKColorSpaceIccProfile.Create((IntPtr)ptr, length));
		}
	}

	public static SKColorSpace CreateIcc(byte[] input)
	{
		return CreateIcc(input.AsSpan());
	}

	public static SKColorSpace CreateIcc(ReadOnlySpan<byte> input)
	{
		return CreateIcc(SKColorSpaceIccProfile.Create(input));
	}

	public static SKColorSpace CreateIcc(SKData input)
	{
		return CreateIcc(SKColorSpaceIccProfile.Create(input));
	}

	public static SKColorSpace CreateIcc(SKColorSpaceIccProfile profile)
	{
		if (profile == null)
		{
			throw new ArgumentNullException("profile");
		}
		return SKObject.Referenced(GetObject(SkiaApi.sk_colorspace_new_icc(profile.Handle)), profile);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags)
	{
		return CreateRgb(gamma, toXyzD50);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags)
	{
		return CreateRgb(gamma, gamut);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags)
	{
		return CreateRgb(coeffs, toXyzD50);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags)
	{
		return CreateRgb(coeffs, gamut);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50)
	{
		if (toXyzD50 == null)
		{
			throw new ArgumentNullException("toXyzD50");
		}
		return CreateRgb(gamma.ToColorSpaceTransferFn(), toXyzD50.ToColorSpaceXyz());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut)
	{
		return CreateRgb(gamma.ToColorSpaceTransferFn(), gamut.ToColorSpaceXyz());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50)
	{
		if (toXyzD50 == null)
		{
			throw new ArgumentNullException("toXyzD50");
		}
		return CreateRgb(coeffs, toXyzD50.ToColorSpaceXyz());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut)
	{
		return CreateRgb(coeffs, gamut.ToColorSpaceXyz());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKNamedGamma gamma, SKMatrix44 toXyzD50)
	{
		if (toXyzD50 == null)
		{
			throw new ArgumentNullException("toXyzD50");
		}
		return CreateRgb(gamma.ToColorSpaceTransferFn(), toXyzD50.ToColorSpaceXyz());
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
	public static SKColorSpace CreateRgb(SKNamedGamma gamma, SKColorSpaceGamut gamut)
	{
		return CreateRgb(gamma.ToColorSpaceTransferFn(), gamut.ToColorSpaceXyz());
	}

	public unsafe static SKColorSpace CreateRgb(SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50)
	{
		return GetObject(SkiaApi.sk_colorspace_new_rgb(&transferFn, &toXyzD50));
	}

	public SKColorSpaceTransferFn GetNumericalTransferFunction()
	{
		if (!GetNumericalTransferFunction(out var fn))
		{
			return SKColorSpaceTransferFn.Empty;
		}
		return fn;
	}

	public unsafe bool GetNumericalTransferFunction(out SKColorSpaceTransferFn fn)
	{
		fixed (SKColorSpaceTransferFn* transferFn = &fn)
		{
			return SkiaApi.sk_colorspace_is_numerical_transfer_fn(Handle, transferFn);
		}
	}

	public SKColorSpaceIccProfile ToProfile()
	{
		SKColorSpaceIccProfile sKColorSpaceIccProfile = new SKColorSpaceIccProfile();
		SkiaApi.sk_colorspace_to_profile(Handle, sKColorSpaceIccProfile.Handle);
		return sKColorSpaceIccProfile;
	}

	public unsafe bool ToColorSpaceXyz(out SKColorSpaceXyz toXyzD50)
	{
		fixed (SKColorSpaceXyz* toXYZD = &toXyzD50)
		{
			return SkiaApi.sk_colorspace_to_xyzd50(Handle, toXYZD);
		}
	}

	public SKColorSpaceXyz ToColorSpaceXyz()
	{
		if (!ToColorSpaceXyz(out var toXyzD))
		{
			return SKColorSpaceXyz.Empty;
		}
		return toXyzD;
	}

	public SKColorSpace ToLinearGamma()
	{
		return GetObject(SkiaApi.sk_colorspace_make_linear_gamma(Handle));
	}

	public SKColorSpace ToSrgbGamma()
	{
		return GetObject(SkiaApi.sk_colorspace_make_srgb_gamma(Handle));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ToColorSpaceXyz() instead.")]
	public SKMatrix44 ToXyzD50()
	{
		return ToColorSpaceXyz().ToMatrix44();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
	public bool ToXyzD50(SKMatrix44 toXyzD50)
	{
		if (toXyzD50 == null)
		{
			throw new ArgumentNullException("toXyzD50");
		}
		if (ToColorSpaceXyz(out var toXyzD51))
		{
			SKMatrix44 sKMatrix = toXyzD51.ToMatrix44();
			if (sKMatrix != null)
			{
				toXyzD50.SetColumnMajor(sKMatrix.ToColumnMajor());
				return true;
			}
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKMatrix44 FromXyzD50()
	{
		return ToXyzD50()?.Invert();
	}

	internal static SKColorSpace GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new SKColorSpace(h, o));
	}
}
