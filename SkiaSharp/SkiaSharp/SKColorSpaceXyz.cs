using System;

namespace SkiaSharp;

public struct SKColorSpaceXyz : IEquatable<SKColorSpaceXyz>
{
	public static readonly SKColorSpaceXyz Empty;

	private float fM00;

	private float fM01;

	private float fM02;

	private float fM10;

	private float fM11;

	private float fM12;

	private float fM20;

	private float fM21;

	private float fM22;

	public unsafe static SKColorSpaceXyz Srgb
	{
		get
		{
			SKColorSpaceXyz result = default(SKColorSpaceXyz);
			SkiaApi.sk_colorspace_xyz_named_srgb(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceXyz AdobeRgb
	{
		get
		{
			SKColorSpaceXyz result = default(SKColorSpaceXyz);
			SkiaApi.sk_colorspace_xyz_named_adobe_rgb(&result);
			return result;
		}
	}

	[Obsolete("Use DisplayP3 instead.")]
	public static SKColorSpaceXyz Dcip3 => DisplayP3;

	public unsafe static SKColorSpaceXyz DisplayP3
	{
		get
		{
			SKColorSpaceXyz result = default(SKColorSpaceXyz);
			SkiaApi.sk_colorspace_xyz_named_display_p3(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceXyz Rec2020
	{
		get
		{
			SKColorSpaceXyz result = default(SKColorSpaceXyz);
			SkiaApi.sk_colorspace_xyz_named_rec2020(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceXyz Xyz
	{
		get
		{
			SKColorSpaceXyz result = default(SKColorSpaceXyz);
			SkiaApi.sk_colorspace_xyz_named_xyz(&result);
			return result;
		}
	}

	public float[] Values
	{
		readonly get
		{
			return new float[9] { fM00, fM01, fM02, fM10, fM11, fM12, fM20, fM21, fM22 };
		}
		set
		{
			if (value.Length != 9)
			{
				throw new ArgumentException("The matrix array must have a length of 9.", "value");
			}
			fM00 = value[0];
			fM01 = value[1];
			fM02 = value[2];
			fM10 = value[3];
			fM11 = value[4];
			fM12 = value[5];
			fM20 = value[6];
			fM21 = value[7];
			fM22 = value[8];
		}
	}

	public readonly float this[int x, int y]
	{
		get
		{
			if (x < 0 || x >= 3)
			{
				throw new ArgumentOutOfRangeException("x");
			}
			if (y < 0 || y >= 3)
			{
				throw new ArgumentOutOfRangeException("y");
			}
			return (x + y * 3) switch
			{
				0 => fM00, 
				1 => fM01, 
				2 => fM02, 
				3 => fM10, 
				4 => fM11, 
				5 => fM12, 
				6 => fM20, 
				7 => fM21, 
				8 => fM22, 
				_ => throw new ArgumentOutOfRangeException("index"), 
			};
		}
	}

	public SKColorSpaceXyz(float value)
	{
		fM00 = value;
		fM01 = value;
		fM02 = value;
		fM10 = value;
		fM11 = value;
		fM12 = value;
		fM20 = value;
		fM21 = value;
		fM22 = value;
	}

	public SKColorSpaceXyz(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 9)
		{
			throw new ArgumentException("The matrix array must have a length of 9.", "values");
		}
		fM00 = values[0];
		fM01 = values[1];
		fM02 = values[2];
		fM10 = values[3];
		fM11 = values[4];
		fM12 = values[5];
		fM20 = values[6];
		fM21 = values[7];
		fM22 = values[8];
	}

	public SKColorSpaceXyz(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
	{
		fM00 = m00;
		fM01 = m01;
		fM02 = m02;
		fM10 = m10;
		fM11 = m11;
		fM12 = m12;
		fM20 = m20;
		fM21 = m21;
		fM22 = m22;
	}

	public unsafe readonly SKColorSpaceXyz Invert()
	{
		SKColorSpaceXyz result = default(SKColorSpaceXyz);
		fixed (SKColorSpaceXyz* src = &this)
		{
			SkiaApi.sk_colorspace_xyz_invert(src, &result);
		}
		return result;
	}

	public unsafe static SKColorSpaceXyz Concat(SKColorSpaceXyz a, SKColorSpaceXyz b)
	{
		SKColorSpaceXyz result = default(SKColorSpaceXyz);
		SkiaApi.sk_colorspace_xyz_concat(&a, &b, &result);
		return result;
	}

	internal readonly SKMatrix44 ToMatrix44()
	{
		SKMatrix44 sKMatrix = new SKMatrix44();
		sKMatrix.Set3x3RowMajor(Values);
		return sKMatrix;
	}

	public readonly bool Equals(SKColorSpaceXyz obj)
	{
		if (fM00 == obj.fM00 && fM01 == obj.fM01 && fM02 == obj.fM02 && fM10 == obj.fM10 && fM11 == obj.fM11 && fM12 == obj.fM12 && fM20 == obj.fM20 && fM21 == obj.fM21)
		{
			return fM22 == obj.fM22;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKColorSpaceXyz obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKColorSpaceXyz left, SKColorSpaceXyz right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKColorSpaceXyz left, SKColorSpaceXyz right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fM00);
		hashCode.Add(fM01);
		hashCode.Add(fM02);
		hashCode.Add(fM10);
		hashCode.Add(fM11);
		hashCode.Add(fM12);
		hashCode.Add(fM20);
		hashCode.Add(fM21);
		hashCode.Add(fM22);
		return hashCode.ToHashCode();
	}
}
