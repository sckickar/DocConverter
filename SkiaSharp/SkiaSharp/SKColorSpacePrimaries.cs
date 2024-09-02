using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKColorSpacePrimaries : IEquatable<SKColorSpacePrimaries>
{
	public static readonly SKColorSpacePrimaries Empty;

	private float fRX;

	private float fRY;

	private float fGX;

	private float fGY;

	private float fBX;

	private float fBY;

	private float fWX;

	private float fWY;

	public readonly float[] Values => new float[8] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

	public float RX
	{
		readonly get
		{
			return fRX;
		}
		set
		{
			fRX = value;
		}
	}

	public float RY
	{
		readonly get
		{
			return fRY;
		}
		set
		{
			fRY = value;
		}
	}

	public float GX
	{
		readonly get
		{
			return fGX;
		}
		set
		{
			fGX = value;
		}
	}

	public float GY
	{
		readonly get
		{
			return fGY;
		}
		set
		{
			fGY = value;
		}
	}

	public float BX
	{
		readonly get
		{
			return fBX;
		}
		set
		{
			fBX = value;
		}
	}

	public float BY
	{
		readonly get
		{
			return fBY;
		}
		set
		{
			fBY = value;
		}
	}

	public float WX
	{
		readonly get
		{
			return fWX;
		}
		set
		{
			fWX = value;
		}
	}

	public float WY
	{
		readonly get
		{
			return fWY;
		}
		set
		{
			fWY = value;
		}
	}

	public SKColorSpacePrimaries(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 8)
		{
			throw new ArgumentException("The values must have exactly 8 items, one for each of [RX, RY, GX, GY, BX, BY, WX, WY].", "values");
		}
		fRX = values[0];
		fRY = values[1];
		fGX = values[2];
		fGY = values[3];
		fBX = values[4];
		fBY = values[5];
		fWX = values[6];
		fWY = values[7];
	}

	public SKColorSpacePrimaries(float rx, float ry, float gx, float gy, float bx, float by, float wx, float wy)
	{
		fRX = rx;
		fRY = ry;
		fGX = gx;
		fGY = gy;
		fBX = bx;
		fBY = by;
		fWX = wx;
		fWY = wy;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ToColorSpaceXyz() instead.")]
	public readonly SKMatrix44 ToXyzD50()
	{
		return ToMatrix44();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
	public readonly bool ToXyzD50(SKMatrix44 toXyzD50)
	{
		if (toXyzD50 == null)
		{
			throw new ArgumentNullException("toXyzD50");
		}
		SKMatrix44 sKMatrix = ToMatrix44();
		if (sKMatrix != null)
		{
			toXyzD50.SetColumnMajor(sKMatrix.ToColumnMajor());
		}
		return sKMatrix != null;
	}

	internal readonly SKMatrix44 ToMatrix44()
	{
		if (!ToMatrix44(out var toXyzD))
		{
			return null;
		}
		return toXyzD;
	}

	internal readonly bool ToMatrix44(out SKMatrix44 toXyzD50)
	{
		if (!ToColorSpaceXyz(out var toXyzD51))
		{
			toXyzD50 = null;
			return false;
		}
		toXyzD50 = toXyzD51.ToMatrix44();
		return true;
	}

	public unsafe readonly bool ToColorSpaceXyz(out SKColorSpaceXyz toXyzD50)
	{
		fixed (SKColorSpacePrimaries* primaries = &this)
		{
			fixed (SKColorSpaceXyz* toXYZD = &toXyzD50)
			{
				return SkiaApi.sk_colorspace_primaries_to_xyzd50(primaries, toXYZD);
			}
		}
	}

	public readonly SKColorSpaceXyz ToColorSpaceXyz()
	{
		if (!ToColorSpaceXyz(out var toXyzD))
		{
			return SKColorSpaceXyz.Empty;
		}
		return toXyzD;
	}

	public readonly bool Equals(SKColorSpacePrimaries obj)
	{
		if (fRX == obj.fRX && fRY == obj.fRY && fGX == obj.fGX && fGY == obj.fGY && fBX == obj.fBX && fBY == obj.fBY && fWX == obj.fWX)
		{
			return fWY == obj.fWY;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKColorSpacePrimaries obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKColorSpacePrimaries left, SKColorSpacePrimaries right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKColorSpacePrimaries left, SKColorSpacePrimaries right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fRX);
		hashCode.Add(fRY);
		hashCode.Add(fGX);
		hashCode.Add(fGY);
		hashCode.Add(fBX);
		hashCode.Add(fBY);
		hashCode.Add(fWX);
		hashCode.Add(fWY);
		return hashCode.ToHashCode();
	}
}
