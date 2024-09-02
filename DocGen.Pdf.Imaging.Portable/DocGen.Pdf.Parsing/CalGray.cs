using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class CalGray : CIEBasedColorspace
{
	private PdfArray m_whitePoint;

	private PdfArray m_blackPoint;

	private PdfNumber m_gamma;

	internal override int Components => 1;

	internal override PdfArray WhitePoint
	{
		get
		{
			return m_whitePoint;
		}
		set
		{
			m_whitePoint = value;
		}
	}

	internal override PdfArray BlackPoint
	{
		get
		{
			if (m_blackPoint == null)
			{
				PdfArray pdfArray = new PdfArray();
				pdfArray.Add(new PdfNumber(0));
				pdfArray.Add(new PdfNumber(0));
				pdfArray.Add(new PdfNumber(0));
				m_blackPoint = new PdfArray(pdfArray);
			}
			return m_blackPoint;
		}
		set
		{
			m_blackPoint = value;
		}
	}

	internal PdfNumber Gamma
	{
		get
		{
			return m_gamma;
		}
		set
		{
			m_gamma = value;
		}
	}

	internal override Color GetColor(string[] pars)
	{
		return Colorspace.GetRgbColor(ColorTransferFunction(pars));
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetGrayColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}

	private double[] ColorTransferFunction(string[] values)
	{
		float floatValue = (BlackPoint[1] as PdfNumber).FloatValue;
		float.TryParse(values[0], out var result);
		float num = (float)Math.Pow(result, m_gamma.FloatValue);
		double num2 = Math.Max(116.0 * Math.Pow(floatValue + ((m_whitePoint[1] as PdfNumber).FloatValue - floatValue) * num, 1.0 / 3.0) - 16.0, 0.0) / 100.0;
		return new double[3]
		{
			(float)num2,
			(float)num2,
			(float)num2
		};
	}

	internal void SetValue(PdfArray colorspaceArray)
	{
		if (colorspaceArray[1] is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = colorspaceArray[1] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary)
			{
				PdfDictionary pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
				if (pdfDictionary.ContainsKey("WhitePoint"))
				{
					m_whitePoint = pdfDictionary["WhitePoint"] as PdfArray;
				}
				if (pdfDictionary.ContainsKey("BlackPoint"))
				{
					m_blackPoint = pdfDictionary["BlackPoint"] as PdfArray;
				}
				if (pdfDictionary.ContainsKey("Gamma"))
				{
					m_gamma = pdfDictionary["Gamma"] as PdfNumber;
				}
			}
		}
		else if (colorspaceArray[1] is PdfDictionary)
		{
			PdfDictionary pdfDictionary2 = colorspaceArray[1] as PdfDictionary;
			if (pdfDictionary2.ContainsKey("WhitePoint"))
			{
				m_whitePoint = pdfDictionary2["WhitePoint"] as PdfArray;
			}
			if (pdfDictionary2.ContainsKey("BlackPoint"))
			{
				m_blackPoint = pdfDictionary2["BlackPoint"] as PdfArray;
			}
			if (pdfDictionary2.ContainsKey("Gamma"))
			{
				m_gamma = pdfDictionary2["Gamma"] as PdfNumber;
			}
		}
	}
}
