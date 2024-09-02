using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Type2 : Function
{
	private PdfArray m_c0;

	private PdfArray m_c1;

	private PdfNumber m_n;

	private PdfDictionary m_functionResource;

	internal PdfDictionary FunctionResource
	{
		get
		{
			return m_functionResource;
		}
		set
		{
			m_functionResource = value;
		}
	}

	internal PdfArray C0
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

	internal PdfArray C1
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

	internal PdfNumber N
	{
		get
		{
			return m_n;
		}
		set
		{
			m_n = value;
		}
	}

	internal int ResultantValue => C0.Count;

	public Type2()
	{
	}

	public Type2(PdfDictionary functionDictionary)
		: base(functionDictionary)
	{
		m_functionResource = functionDictionary;
		m_c0 = new PdfArray(new double[1]);
		m_c1 = new PdfArray(new double[1] { 1.0 });
		m_n = new PdfNumber(2.0);
		if (m_functionResource != null)
		{
			if (m_functionResource.ContainsKey("C0"))
			{
				m_c0 = PdfCrossTable.Dereference(m_functionResource["C0"]) as PdfArray;
			}
			if (m_functionResource.ContainsKey("C1"))
			{
				m_c1 = PdfCrossTable.Dereference(m_functionResource["C1"]) as PdfArray;
			}
			if (m_functionResource.ContainsKey("N"))
			{
				m_n = PdfCrossTable.Dereference(m_functionResource["N"]) as PdfNumber;
			}
		}
	}

	protected override double[] PerformFunction(double[] inputData)
	{
		double[] array = new double[inputData.Length * ResultantValue];
		for (int i = 0; i < inputData.Length; i++)
		{
			double[] array2 = PerformFunctionSingleValue(inputData[i]);
			for (int j = 0; j < array2.Length; j++)
			{
				array[i * ResultantValue + j] = array2[j];
			}
		}
		return array;
	}

	private double[] PerformFunctionSingleValue(double x)
	{
		double[] array = new double[ResultantValue];
		for (int i = 0; i < array.Length; i++)
		{
			double c = (C0[i] as PdfNumber).FloatValue;
			double c2 = (C1[i] as PdfNumber).FloatValue;
			array[i] = CalculateExponentialInterpolation(c, c2, x);
		}
		return array;
	}

	private double CalculateExponentialInterpolation(double c0, double c1, double x)
	{
		return c0 + Math.Pow(x, N.FloatValue) * (c1 - c0);
	}
}
