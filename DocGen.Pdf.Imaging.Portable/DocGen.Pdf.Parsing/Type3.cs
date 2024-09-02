using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Type3 : Function
{
	private PdfArray m_encode;

	private PdfArray m_bounds;

	private PdfArray m_pdfFunction;

	internal PdfArray Encode
	{
		get
		{
			return m_encode;
		}
		set
		{
			m_encode = value;
		}
	}

	internal PdfArray Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	internal PdfArray PdfFunction
	{
		get
		{
			return m_pdfFunction;
		}
		set
		{
			m_pdfFunction = value;
		}
	}

	public Type3(PdfDictionary dic)
		: base(dic)
	{
		if (dic.Items.ContainsKey(new PdfName("Bounds")))
		{
			if (dic.Items[new PdfName("Bounds")] is PdfReferenceHolder)
			{
				m_bounds = (dic.Items[new PdfName("Bounds")] as PdfReferenceHolder).Object as PdfArray;
			}
			else
			{
				m_bounds = dic.Items[new PdfName("Bounds")] as PdfArray;
			}
		}
		if (dic.Items.ContainsKey(new PdfName("Encode")))
		{
			if (dic.Items[new PdfName("Encode")] is PdfReferenceHolder)
			{
				m_encode = (dic.Items[new PdfName("Encode")] as PdfReferenceHolder).Object as PdfArray;
			}
			else
			{
				m_encode = dic.Items[new PdfName("Encode")] as PdfArray;
			}
		}
		if (dic.Items.ContainsKey(new PdfName("Functions")))
		{
			if (dic.Items[new PdfName("Functions")] is PdfReferenceHolder)
			{
				m_pdfFunction = (dic.Items[new PdfName("Functions")] as PdfReferenceHolder).Object as PdfArray;
			}
			else
			{
				m_pdfFunction = dic.Items[new PdfName("Functions")] as PdfArray;
			}
		}
	}

	protected override double[] PerformFunction(double[] inputData)
	{
		double x = inputData[0];
		int index = 0;
		Function function = null;
		function = ((PdfFunction == null) ? Function.CreateFunction((m_functionDictionary["Functions"] as PdfReferenceHolder).Object as PdfArray) : FindFunction(x, out index));
		double xMin = ((index <= 0) ? ((double)(base.Domain[0] as PdfNumber).FloatValue) : ((double)(Bounds[index - 1] as PdfNumber).FloatValue));
		double xMax = ((PdfFunction == null || index >= PdfFunction.Count - 1) ? ((double)(base.Domain[1] as PdfNumber).FloatValue) : ((double)(Bounds[index] as PdfNumber).FloatValue));
		double yMin;
		double yMax;
		if (Encode != null)
		{
			yMin = (Encode[2 * index] as PdfNumber).FloatValue;
			yMax = (Encode[2 * index + 1] as PdfNumber).FloatValue;
		}
		else
		{
			PdfArray obj = PdfCrossTable.Dereference(m_functionDictionary["Encode"]) as PdfArray;
			yMin = (obj[2 * index] as PdfNumber).FloatValue;
			yMax = (obj[2 * index + 1] as PdfNumber).FloatValue;
		}
		return function.ColorTransferFunction(new double[1] { Function.FindIntermediateData(x, xMin, xMax, yMin, yMax) });
	}

	private Function FindFunction(double x, out int index)
	{
		for (int i = 0; i < Bounds.Count; i++)
		{
			double num = (Bounds[i] as PdfNumber).FloatValue;
			if (x < num)
			{
				index = i;
				return Function.CreateFunction(PdfFunction[i]);
			}
		}
		index = PdfFunction.Count - 1;
		return Function.CreateFunction(PdfFunction[index]);
	}
}
