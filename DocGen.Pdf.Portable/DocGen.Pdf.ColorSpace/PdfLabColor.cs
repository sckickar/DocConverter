using System;

namespace DocGen.Pdf.ColorSpace;

public class PdfLabColor : PdfExtendedColor
{
	private double m_a;

	private double m_b;

	private double m_l;

	public double A
	{
		get
		{
			return m_a;
		}
		set
		{
			PdfLabColorSpace pdfLabColorSpace = base.ColorSpace as PdfLabColorSpace;
			if (pdfLabColorSpace.Range != null && (value < pdfLabColorSpace.Range[0] || value > pdfLabColorSpace.Range[1]))
			{
				throw new ArgumentOutOfRangeException("A", "a* component must be in the range defined by the Lab colorspace.");
			}
			m_a = value;
		}
	}

	public double B
	{
		get
		{
			return m_b;
		}
		set
		{
			PdfLabColorSpace pdfLabColorSpace = base.ColorSpace as PdfLabColorSpace;
			if (pdfLabColorSpace.Range != null && (value < pdfLabColorSpace.Range[2] || value > pdfLabColorSpace.Range[3]))
			{
				throw new ArgumentOutOfRangeException("B", "b* component must be in the range defined by the Lab colorspace.");
			}
			m_b = value;
		}
	}

	public double L
	{
		get
		{
			return m_l;
		}
		set
		{
			if (value < 0.0 || value > 100.0)
			{
				throw new ArgumentOutOfRangeException("L", "L must be between 0 and 100");
			}
			m_l = value;
		}
	}

	public PdfLabColor(PdfColorSpaces colorspace)
		: base(colorspace)
	{
	}
}
