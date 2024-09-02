using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class CalRGB : CIEBasedColorspace
{
	private PdfArray m_whitePoint;

	private PdfArray m_blackPoint;

	private PdfArray m_gamma;

	private ColorspaceMatrix m_matrix;

	internal override int Components => 3;

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

	internal PdfArray Gamma
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

	internal ColorspaceMatrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			m_matrix = value;
		}
	}

	internal override Color GetColor(string[] pars)
	{
		return Colorspace.GetRgbColor(ColorTransferFunction(pars));
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetRgbColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}

	private double[] ColorTransferFunction(string[] values)
	{
		double[,] array = new double[3, 3]
		{
			{ 3.240449, -1.537136, -0.498531 },
			{ -0.969265, 1.876011, 0.041556 },
			{ 0.055643, -0.204026, 1.057229 }
		};
		float.TryParse(values[0], out var result);
		float.TryParse(values[1], out var result2);
		float.TryParse(values[2], out var result3);
		double num = Math.Pow(result, (m_gamma[0] as PdfNumber).FloatValue);
		double num2 = Math.Pow(result2, (m_gamma[1] as PdfNumber).FloatValue);
		double num3 = Math.Pow(result3, (m_gamma[2] as PdfNumber).FloatValue);
		double[] array2 = new double[3];
		double num4 = Matrix.Xa * num + Matrix.Xb * num2 + Matrix.Xc * num3;
		double num5 = Matrix.Ya * num + Matrix.Yb * num2 + Matrix.Yc * num3;
		double num6 = Matrix.Za * num + Matrix.Zb * num2 + Matrix.Zc * num3;
		double num7 = array[0, 0] * num4 + array[0, 1] * num5 + array[0, 2] * num6;
		double num8 = array[1, 0] * num4 + array[1, 1] * num5 + array[1, 2] * num6;
		double num9 = array[2, 0] * num4 + array[2, 1] * num5 + array[2, 2] * num6;
		double num10 = 1.0 / (array[0, 0] * (double)(WhitePoint[0] as PdfNumber).FloatValue + array[0, 1] * (double)(WhitePoint[1] as PdfNumber).FloatValue + array[0, 2] * (double)(WhitePoint[2] as PdfNumber).FloatValue);
		double num11 = 1.0 / (array[1, 0] * (double)(WhitePoint[0] as PdfNumber).FloatValue + array[1, 1] * (double)(WhitePoint[1] as PdfNumber).FloatValue + array[1, 2] * (double)(WhitePoint[2] as PdfNumber).FloatValue);
		double num12 = 1.0 / (array[2, 0] * (double)(WhitePoint[0] as PdfNumber).FloatValue + array[2, 1] * (double)(WhitePoint[1] as PdfNumber).FloatValue + array[2, 2] * (double)(WhitePoint[2] as PdfNumber).FloatValue);
		double num13 = Math.Pow(ColorTransferFunction(num7 * num10), 0.5);
		double num14 = Math.Pow(ColorTransferFunction(num8 * num11), 0.5);
		double num15 = Math.Pow(ColorTransferFunction(num9 * num12), 0.5);
		array2[0] = num13;
		array2[1] = num14;
		array2[2] = num15;
		return array2;
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
					m_gamma = pdfDictionary["Gamma"] as PdfArray;
				}
				if (pdfDictionary.ContainsKey("Matrix"))
				{
					PdfArray array = pdfDictionary["Matrix"] as PdfArray;
					m_matrix = new ColorspaceMatrix(array);
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
				m_gamma = pdfDictionary2["Gamma"] as PdfArray;
			}
			if (pdfDictionary2.ContainsKey("Matrix"))
			{
				PdfArray array2 = pdfDictionary2["Matrix"] as PdfArray;
				m_matrix = new ColorspaceMatrix(array2);
			}
		}
	}

	private double[] FromXYZ(double x, double y, double z, double whitepointZ)
	{
		double[] array = new double[3];
		if (whitepointZ < 1.0)
		{
			double num = (array[0] = x * 3.1339 + y * -1.617 + z * -0.4906);
			double num2 = (array[1] = x * -0.9785 + y * 1.916 + z * 0.0333);
			double num3 = (array[2] = x * 0.072 + y * -0.229 + z * 1.4057);
			_ = num / (num + num2 + num3);
			_ = num2 / (num + num2 + num3);
		}
		else
		{
			double num4 = (array[0] = x * 3.2406 + y * -1.5372 + z * -0.4986);
			double num5 = (array[1] = x * -0.9689 + y * 1.8758 + z * 0.0415);
			double num6 = (array[2] = x * 0.0557 + y * -0.204 + z * 1.057);
			_ = num4 / (num4 + num5 + num6);
			_ = num5 / (num4 + num5 + num6);
		}
		return array;
	}

	private double ColorTransferFunction(double value)
	{
		if (value <= 0.0031308)
		{
			return value / 12.92;
		}
		return 1.055 * Math.Pow(value, 5.0 / 12.0) - 0.055;
	}

	private double ExtractColorValues(double colorValue)
	{
		return Math.Min(1.0, Math.Max(0.0, colorValue));
	}
}
