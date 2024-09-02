using System;

namespace SkiaSharp;

public struct SKRotationScaleMatrix : IEquatable<SKRotationScaleMatrix>
{
	private float fSCos;

	private float fSSin;

	private float fTX;

	private float fTY;

	public static readonly SKRotationScaleMatrix Empty;

	public static readonly SKRotationScaleMatrix Identity = new SKRotationScaleMatrix(1f, 0f, 0f, 0f);

	public float SCos
	{
		readonly get
		{
			return fSCos;
		}
		set
		{
			fSCos = value;
		}
	}

	public float SSin
	{
		readonly get
		{
			return fSSin;
		}
		set
		{
			fSSin = value;
		}
	}

	public float TX
	{
		readonly get
		{
			return fTX;
		}
		set
		{
			fTX = value;
		}
	}

	public float TY
	{
		readonly get
		{
			return fTY;
		}
		set
		{
			fTY = value;
		}
	}

	public readonly bool Equals(SKRotationScaleMatrix obj)
	{
		if (fSCos == obj.fSCos && fSSin == obj.fSSin && fTX == obj.fTX)
		{
			return fTY == obj.fTY;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRotationScaleMatrix obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRotationScaleMatrix left, SKRotationScaleMatrix right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRotationScaleMatrix left, SKRotationScaleMatrix right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fSCos);
		hashCode.Add(fSSin);
		hashCode.Add(fTX);
		hashCode.Add(fTY);
		return hashCode.ToHashCode();
	}

	public SKRotationScaleMatrix(float scos, float ssin, float tx, float ty)
	{
		fSCos = scos;
		fSSin = ssin;
		fTX = tx;
		fTY = ty;
	}

	public readonly SKMatrix ToMatrix()
	{
		return new SKMatrix(fSCos, 0f - fSSin, fTX, fSSin, fSCos, fTY, 0f, 0f, 1f);
	}

	public static SKRotationScaleMatrix CreateDegrees(float scale, float degrees, float tx, float ty, float anchorX, float anchorY)
	{
		return Create(scale, degrees * ((float)Math.PI / 180f), tx, ty, anchorX, anchorY);
	}

	public static SKRotationScaleMatrix Create(float scale, float radians, float tx, float ty, float anchorX, float anchorY)
	{
		float num = (float)Math.Sin(radians) * scale;
		float num2 = (float)Math.Cos(radians) * scale;
		float tx2 = tx + (0f - num2) * anchorX + num * anchorY;
		float ty2 = ty + (0f - num) * anchorX - num2 * anchorY;
		return new SKRotationScaleMatrix(num2, num, tx2, ty2);
	}

	public static SKRotationScaleMatrix CreateIdentity()
	{
		return new SKRotationScaleMatrix(1f, 0f, 0f, 0f);
	}

	public static SKRotationScaleMatrix CreateTranslation(float x, float y)
	{
		return new SKRotationScaleMatrix(1f, 0f, x, y);
	}

	public static SKRotationScaleMatrix CreateScale(float s)
	{
		return new SKRotationScaleMatrix(s, 0f, 0f, 0f);
	}

	public static SKRotationScaleMatrix CreateRotation(float radians, float anchorX, float anchorY)
	{
		return Create(1f, radians, 0f, 0f, anchorX, anchorY);
	}

	public static SKRotationScaleMatrix CreateRotationDegrees(float degrees, float anchorX, float anchorY)
	{
		return CreateDegrees(1f, degrees, 0f, 0f, anchorX, anchorY);
	}
}
