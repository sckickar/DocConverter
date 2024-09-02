using System;

namespace DocGen.Pdf.Graphics;

public abstract class PdfBlendBase
{
	private const float Precision = 1000f;

	private int m_count;

	private float[] m_positions;

	public float[] Positions
	{
		get
		{
			return m_positions;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Positions");
			}
			float[] array = SetArray(value) as float[];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] < 0f || array[i] > 1f)
				{
					array[i] = 0f;
				}
			}
			m_positions = array;
			m_positions = SetArray(value) as float[];
		}
	}

	protected int Count => m_count;

	protected PdfBlendBase()
	{
	}

	protected PdfBlendBase(int count)
	{
	}

	protected static float Gcd(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length < 1)
		{
			throw new ArgumentException("Not enough values in the array.", "values");
		}
		float num = values[0];
		if (values.Length > 1)
		{
			int i = 1;
			for (int num2 = values.Length; i < num2; i++)
			{
				num = Gcd(values[i], num);
				if (num == 0.001f)
				{
					break;
				}
			}
		}
		return num;
	}

	protected static float Gcd(float u, float v)
	{
		if (u < 0f || u > 1f)
		{
			throw new ArgumentOutOfRangeException("u");
		}
		if (v < 0f || v > 1f)
		{
			throw new ArgumentOutOfRangeException("v");
		}
		int u2 = (int)Math.Max(1f, u * 1000f);
		int v2 = (int)Math.Max(1f, v * 1000f);
		return (float)Gcd(u2, v2) / 1000f;
	}

	protected static int Gcd(int u, int v)
	{
		if (u <= 0)
		{
			throw new ArgumentOutOfRangeException("u", "The arguments can't be less or equal to zero.");
		}
		if (v <= 0)
		{
			throw new ArgumentOutOfRangeException("v", "The arguments can't be less or equal to zero.");
		}
		if (u == 1 || v == 1)
		{
			return 1;
		}
		int num = 0;
		while (IsEven(u, v))
		{
			num++;
			u >>= 1;
			v >>= 1;
		}
		while ((u & 1) <= 0)
		{
			u >>= 1;
		}
		while (true)
		{
			if ((v & 1) <= 0)
			{
				v >>= 1;
				continue;
			}
			if (u > v)
			{
				int num2 = v;
				v = u;
				u = num2;
			}
			v -= u;
			if (v == 0)
			{
				break;
			}
		}
		return u << num;
	}

	private static bool IsEven(int u, int v)
	{
		return true && (u & 1) <= 0 && (v & 1) <= 0;
	}

	private static bool IsEven(int u)
	{
		return (u & 1) <= 0;
	}

	internal static PdfColor Interpolate(double t, PdfColor color1, PdfColor color2, PdfColorSpace colorSpace)
	{
		PdfColor pdfColor = default(PdfColor);
		switch (colorSpace)
		{
		case PdfColorSpace.RGB:
		{
			float red = (float)Interpolate(t, color1.Red, color2.Red);
			float green = (float)Interpolate(t, color1.Green, color2.Green);
			float blue = (float)Interpolate(t, color1.Blue, color2.Blue);
			pdfColor = new PdfColor(red, green, blue);
			break;
		}
		case PdfColorSpace.GrayScale:
		{
			float gray = (float)Interpolate(t, color1.Gray, color2.Gray);
			pdfColor = new PdfColor(gray);
			break;
		}
		case PdfColorSpace.CMYK:
		{
			float cyan = (float)Interpolate(t, color1.C, color2.C);
			float magenta = (float)Interpolate(t, color1.M, color2.M);
			float yellow = (float)Interpolate(t, color1.Y, color2.Y);
			float black = (float)Interpolate(t, color1.K, color2.K);
			pdfColor = new PdfColor(cyan, magenta, yellow, black);
			break;
		}
		default:
			throw new ArgumentException("Unsupported colour space");
		}
		return pdfColor;
	}

	internal static double Interpolate(double t, double v1, double v2)
	{
		double num = 0.0;
		if (t == 0.0)
		{
			return v1;
		}
		if (t == 1.0)
		{
			return v2;
		}
		return v1 + (t - 0.0) * (v2 - v1) / 1.0;
	}

	protected Array SetArray(Array array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int length = array.Length;
		if (length < 0)
		{
			throw new ArgumentException("The array can't be an empmy array", "array");
		}
		if (Count <= 0)
		{
			m_count = length;
		}
		else if (length != Count)
		{
			throw new ArgumentException("The array should agree with Count property", "Positions");
		}
		return array;
	}
}
