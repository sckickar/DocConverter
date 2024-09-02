using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Functions;

public class PdfExponentialInterpolationFunction : PdfFunction
{
	protected float[] m_c0;

	protected float[] m_c1;

	private float m_interpolationExp;

	public float[] C0
	{
		get
		{
			return m_c0;
		}
		set
		{
			m_c0 = value;
		}
	}

	public float[] C1
	{
		get
		{
			return m_c1;
		}
		set
		{
			m_c1 = value;
		}
	}

	public float Exponent
	{
		get
		{
			return m_interpolationExp;
		}
		set
		{
			m_interpolationExp = value;
		}
	}

	public PdfExponentialInterpolationFunction(bool Init)
		: base(new PdfDictionary())
	{
		m_interpolationExp = 1f;
		base.Domain = new PdfArray(new float[2] { 0f, 1f });
		base.Range = new PdfArray(new float[8] { 0f, 1f, 0f, 1f, 0f, 1f, 0f, 1f });
		m_interpolationExp = 1f;
		C0 = new float[4];
	}

	internal PdfExponentialInterpolationFunction()
		: base(new PdfDictionary())
	{
	}

	internal float[] InterpolationExponent(float[] singleArray1)
	{
		int num = base.Range.Count / 2;
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = C0[i] + (float)Math.Pow(singleArray1[0], m_interpolationExp) * (C1[i] - C0[i]);
		}
		return array;
	}
}
