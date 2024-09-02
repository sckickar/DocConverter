using System;

namespace DocGen.Pdf.ColorSpace;

public class PdfCalRGBColor : PdfExtendedColor
{
	private double m_red;

	private double m_green;

	private double m_blue;

	public double Blue
	{
		get
		{
			return m_blue;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Blue", "Blue level must be between 0 and 1");
			}
			m_blue = value;
		}
	}

	public double Green
	{
		get
		{
			return m_green;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Green", "Green level must be between 0 and 1");
			}
			m_green = value;
		}
	}

	public double Red
	{
		get
		{
			return m_red;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Red", "Red level must be between 0 and 1");
			}
			m_red = value;
		}
	}

	public PdfCalRGBColor(PdfColorSpaces colorspace)
		: base(colorspace)
	{
	}
}
