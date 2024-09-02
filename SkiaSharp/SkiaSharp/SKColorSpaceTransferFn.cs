using System;

namespace SkiaSharp;

public struct SKColorSpaceTransferFn : IEquatable<SKColorSpaceTransferFn>
{
	public static readonly SKColorSpaceTransferFn Empty;

	private float fG;

	private float fA;

	private float fB;

	private float fC;

	private float fD;

	private float fE;

	private float fF;

	public unsafe static SKColorSpaceTransferFn Srgb
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_srgb(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceTransferFn TwoDotTwo
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_2dot2(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceTransferFn Linear
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_linear(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceTransferFn Rec2020
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_rec2020(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceTransferFn Pq
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_pq(&result);
			return result;
		}
	}

	public unsafe static SKColorSpaceTransferFn Hlg
	{
		get
		{
			SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
			SkiaApi.sk_colorspace_transfer_fn_named_hlg(&result);
			return result;
		}
	}

	public readonly float[] Values => new float[7] { fG, fA, fB, fC, fD, fE, fF };

	public float G
	{
		readonly get
		{
			return fG;
		}
		set
		{
			fG = value;
		}
	}

	public float A
	{
		readonly get
		{
			return fA;
		}
		set
		{
			fA = value;
		}
	}

	public float B
	{
		readonly get
		{
			return fB;
		}
		set
		{
			fB = value;
		}
	}

	public float C
	{
		readonly get
		{
			return fC;
		}
		set
		{
			fC = value;
		}
	}

	public float D
	{
		readonly get
		{
			return fD;
		}
		set
		{
			fD = value;
		}
	}

	public float E
	{
		readonly get
		{
			return fE;
		}
		set
		{
			fE = value;
		}
	}

	public float F
	{
		readonly get
		{
			return fF;
		}
		set
		{
			fF = value;
		}
	}

	public SKColorSpaceTransferFn(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 7)
		{
			throw new ArgumentException("The values must have exactly 7 items, one for each of [G, A, B, C, D, E, F].", "values");
		}
		fG = values[0];
		fA = values[1];
		fB = values[2];
		fC = values[3];
		fD = values[4];
		fE = values[5];
		fF = values[6];
	}

	public SKColorSpaceTransferFn(float g, float a, float b, float c, float d, float e, float f)
	{
		fG = g;
		fA = a;
		fB = b;
		fC = c;
		fD = d;
		fE = e;
		fF = f;
	}

	public unsafe readonly SKColorSpaceTransferFn Invert()
	{
		SKColorSpaceTransferFn result = default(SKColorSpaceTransferFn);
		fixed (SKColorSpaceTransferFn* src = &this)
		{
			SkiaApi.sk_colorspace_transfer_fn_invert(src, &result);
		}
		return result;
	}

	public unsafe readonly float Transform(float x)
	{
		fixed (SKColorSpaceTransferFn* transferFn = &this)
		{
			return SkiaApi.sk_colorspace_transfer_fn_eval(transferFn, x);
		}
	}

	public readonly bool Equals(SKColorSpaceTransferFn obj)
	{
		if (fG == obj.fG && fA == obj.fA && fB == obj.fB && fC == obj.fC && fD == obj.fD && fE == obj.fE)
		{
			return fF == obj.fF;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKColorSpaceTransferFn obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKColorSpaceTransferFn left, SKColorSpaceTransferFn right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKColorSpaceTransferFn left, SKColorSpaceTransferFn right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fG);
		hashCode.Add(fA);
		hashCode.Add(fB);
		hashCode.Add(fC);
		hashCode.Add(fD);
		hashCode.Add(fE);
		hashCode.Add(fF);
		return hashCode.ToHashCode();
	}
}
