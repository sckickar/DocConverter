using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal abstract class Function
{
	private PdfArray m_domain;

	private PdfArray m_range;

	internal PdfDictionary m_functionDictionary;

	internal PdfArray Domain
	{
		get
		{
			return m_domain;
		}
		set
		{
			m_domain = value;
		}
	}

	internal PdfArray Range
	{
		get
		{
			return m_range;
		}
		set
		{
			m_range = value;
		}
	}

	public Function()
	{
	}

	public Function(PdfDictionary dictionary)
	{
		m_functionDictionary = dictionary;
		m_domain = null;
		m_range = null;
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("Domain"))
			{
				m_domain = PdfCrossTable.Dereference(dictionary["Domain"]) as PdfArray;
			}
			if (dictionary.ContainsKey("Range"))
			{
				m_range = PdfCrossTable.Dereference(dictionary["Range"]) as PdfArray;
			}
		}
	}

	internal static Function CreateFunction(IPdfPrimitive array)
	{
		PdfDictionary function = GetFunction(array);
		if (function != null)
		{
			if (function.Items.ContainsKey(new PdfName("FunctionType")))
			{
				switch ((function.Items[new PdfName("FunctionType")] as PdfNumber).IntValue)
				{
				case 0:
					return new Type0(function);
				case 2:
					return new Type2(function);
				case 3:
					return new Type3(function);
				case 4:
					return new Type4(function);
				}
			}
		}
		else
		{
			PdfArray pdfArray = array as PdfArray;
			for (int i = 0; i < pdfArray.Elements.Count; i++)
			{
				function = (pdfArray.Elements[i] as PdfReferenceHolder).Object as PdfDictionary;
				if (function.Items.ContainsKey(new PdfName("FunctionType")))
				{
					switch ((function.Items[new PdfName("FunctionType")] as PdfNumber).IntValue)
					{
					case 0:
						return new Type0(function);
					case 2:
						return new Type2(function);
					case 3:
						return new Type3(function);
					case 4:
						return new Type4(function);
					}
				}
			}
		}
		return null;
	}

	internal static Function CreateFunction(PdfDictionary array)
	{
		if (array.Items.ContainsKey(new PdfName("FunctionType")))
		{
			switch ((array.Items[new PdfName("FunctionType")] as PdfNumber).IntValue)
			{
			case 0:
				return new Type0(array);
			case 2:
				return new Type2(array);
			case 3:
				return new Type3(array);
			case 4:
				return new Type4(array);
			}
		}
		return null;
	}

	private static PdfDictionary GetFunction(IPdfPrimitive function)
	{
		PdfDictionary pdfDictionary;
		if (function is PdfArray { Count: >=4 } pdfArray)
		{
			if (pdfArray[3] is PdfDictionary result)
			{
				return result;
			}
			PdfReferenceHolder pdfReferenceHolder = pdfArray[3] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				return pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
			}
		}
		PdfReferenceHolder pdfReferenceHolder2 = function as PdfReferenceHolder;
		if (pdfReferenceHolder2 != null)
		{
			return pdfDictionary = pdfReferenceHolder2.Object as PdfDictionary;
		}
		return function as PdfDictionary;
	}

	internal double[] ColorTransferFunction(double[] inputValues)
	{
		return ExtractOutputData(PerformFunction(ExtractInputData(inputValues)));
	}

	protected abstract double[] PerformFunction(double[] clippedInputValues);

	private double[] ExtractInputData(double[] inputValues)
	{
		double[] array = new double[inputValues.Length];
		for (int i = 0; i < inputValues.Length; i++)
		{
			double min = (Domain[2 * i] as PdfNumber).FloatValue;
			double max = (Domain[2 * i + 1] as PdfNumber).FloatValue;
			array[i] = ExtractData(inputValues[i], min, max);
		}
		return array;
	}

	private double[] ExtractOutputData(double[] outputValues)
	{
		if (Range == null)
		{
			return outputValues;
		}
		double[] array = new double[outputValues.Length];
		for (int i = 0; i < outputValues.Length; i++)
		{
			double min = (Range[2 * i] as PdfNumber).FloatValue;
			double max = (Range[2 * i + 1] as PdfNumber).FloatValue;
			array[i] = ExtractData(outputValues[i], min, max);
		}
		return array;
	}

	internal static double FindIntermediateData(double x, double xMin, double xMax, double yMin, double yMax)
	{
		return yMin + (x - xMin) * ((yMax - yMin) / (xMax - xMin));
	}

	internal static double ExtractData(double value, double min, double max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}
}
