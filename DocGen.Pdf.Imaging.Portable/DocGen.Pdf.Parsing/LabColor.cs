using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class LabColor : CIEBasedColorspace
{
	private PdfArray m_whitePoint;

	private PdfArray m_blackPoint;

	private PdfArray m_range;

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

	internal override Color GetColor(string[] pars)
	{
		double[] array = LabColorTransferFunction(pars);
		Color baseColor = Color.FromArgb(255, (byte)(array[0] * 255.0), (byte)(array[1] * 255.0), (byte)(array[2] * 255.0));
		if (baseColor.GetBrightness() == 0f)
		{
			return Color.FromArgb(255, baseColor);
		}
		return Color.FromArgb((byte)(255f * baseColor.GetBrightness()), baseColor);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetRgbColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}

	private double[] LabColorTransferFunction(string[] pars)
	{
		float.TryParse(pars[0], out var result);
		float.TryParse(pars[1], out var result2);
		float.TryParse(pars[2], out var result3);
		double num = CorrectRange(new double[2]
		{
			(m_range[0] as PdfNumber).FloatValue,
			(m_range[1] as PdfNumber).FloatValue
		}, result2);
		double num2 = CorrectRange(new double[2]
		{
			(m_range[2] as PdfNumber).FloatValue,
			(m_range[3] as PdfNumber).FloatValue
		}, result3);
		double x = ((double)result + 16.0) / 116.0 + num / 500.0;
		double x2 = ((double)result + 16.0) / 116.0;
		double x3 = ((double)result + 16.0) / 116.0 - num2 / 200.0;
		_ = new double[4];
		double x4 = (double)(WhitePoint[0] as PdfNumber).FloatValue * GammaFunction(x);
		double y = (double)(WhitePoint[1] as PdfNumber).FloatValue * GammaFunction(x2);
		double z = (double)(WhitePoint[2] as PdfNumber).FloatValue * GammaFunction(x3);
		return XYZtoRGB(x4, y, z, (WhitePoint[2] as PdfNumber).FloatValue);
	}

	private double GetCompare(double f, double min, double max)
	{
		if (!(f > min))
		{
			return min;
		}
		if (!(f < max))
		{
			return max;
		}
		return f;
	}

	private static double GammaFunction(double x)
	{
		if (x < 0.0)
		{
			return (x - 0.0) * 0.12841854934601665;
		}
		return x * x * x;
	}

	private static double CorrectRange(double[] range, double value)
	{
		double num = range[1] - range[0];
		return (value - range[0]) / num * 200.0 - 100.0;
	}

	private double[] XYZtoRGB(double x, double y, double z, double whitepointZ)
	{
		double[] array = new double[3];
		if (whitepointZ < 1.0)
		{
			array[0] = x * 3.1339 + y * -1.617 + z * -0.4906;
			array[1] = x * -0.9785 + y * 1.916 + z * 0.0333;
			array[2] = x * 0.072 + y * -0.229 + z * 1.4057;
		}
		else
		{
			array[0] = (x * 3.240449 + y * -1.537136 + z * -0.498531) * 0.830026;
			array[1] = (x * -0.969265 + y * 1.876011 + z * 0.041556) * 1.05452;
			array[2] = (x * 0.055643 + y * -0.204026 + z * 1.057229) * 1.1002999544143677;
		}
		return array;
	}

	private double ColorTransferFunction(double value)
	{
		if (value > 0.0031308)
		{
			return 1.055 * Math.Pow(value, 5.0 / 12.0) - 0.055;
		}
		return value * 12.92;
	}

	internal void SetValue(PdfArray labColorspaceArray)
	{
		if (labColorspaceArray[1] is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = labColorspaceArray[1] as PdfReferenceHolder;
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
				if (pdfDictionary.ContainsKey("Range"))
				{
					m_range = pdfDictionary["Range"] as PdfArray;
				}
			}
		}
		else if (labColorspaceArray[1] is PdfDictionary)
		{
			PdfDictionary pdfDictionary2 = labColorspaceArray[1] as PdfDictionary;
			if (pdfDictionary2.ContainsKey("WhitePoint"))
			{
				m_whitePoint = pdfDictionary2["WhitePoint"] as PdfArray;
			}
			if (pdfDictionary2.ContainsKey("BlackPoint"))
			{
				m_blackPoint = pdfDictionary2["BlackPoint"] as PdfArray;
			}
			if (pdfDictionary2.ContainsKey("Range"))
			{
				m_range = pdfDictionary2["Range"] as PdfArray;
			}
		}
	}
}
