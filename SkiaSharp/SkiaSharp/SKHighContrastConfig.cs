using System;

namespace SkiaSharp;

public struct SKHighContrastConfig : IEquatable<SKHighContrastConfig>
{
	public static readonly SKHighContrastConfig Default;

	private byte fGrayscale;

	private SKHighContrastConfigInvertStyle fInvertStyle;

	private float fContrast;

	public readonly bool IsValid
	{
		get
		{
			if (fInvertStyle >= SKHighContrastConfigInvertStyle.NoInvert && fInvertStyle <= SKHighContrastConfigInvertStyle.InvertLightness && (double)fContrast >= -1.0)
			{
				return (double)fContrast <= 1.0;
			}
			return false;
		}
	}

	public bool Grayscale
	{
		readonly get
		{
			return fGrayscale > 0;
		}
		set
		{
			fGrayscale = (value ? ((byte)1) : ((byte)0));
		}
	}

	public SKHighContrastConfigInvertStyle InvertStyle
	{
		readonly get
		{
			return fInvertStyle;
		}
		set
		{
			fInvertStyle = value;
		}
	}

	public float Contrast
	{
		readonly get
		{
			return fContrast;
		}
		set
		{
			fContrast = value;
		}
	}

	static SKHighContrastConfig()
	{
		Default = new SKHighContrastConfig(grayscale: false, SKHighContrastConfigInvertStyle.NoInvert, 0f);
	}

	public SKHighContrastConfig(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
	{
		fGrayscale = (grayscale ? ((byte)1) : ((byte)0));
		fInvertStyle = invertStyle;
		fContrast = contrast;
	}

	public readonly bool Equals(SKHighContrastConfig obj)
	{
		if (fGrayscale == obj.fGrayscale && fInvertStyle == obj.fInvertStyle)
		{
			return fContrast == obj.fContrast;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKHighContrastConfig obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKHighContrastConfig left, SKHighContrastConfig right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKHighContrastConfig left, SKHighContrastConfig right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fGrayscale);
		hashCode.Add(fInvertStyle);
		hashCode.Add(fContrast);
		return hashCode.ToHashCode();
	}
}
