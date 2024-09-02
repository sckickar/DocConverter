using System;

namespace DocGen.Pdf.ColorSpace;

public class PdfCalGrayColor : PdfExtendedColor
{
	private double m_gray;

	public double Gray
	{
		get
		{
			return m_gray;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Gray", "Gray level must be between 0 and 1");
			}
			m_gray = value;
		}
	}

	public PdfCalGrayColor(PdfColorSpaces colorspace)
		: base(colorspace)
	{
	}
}
