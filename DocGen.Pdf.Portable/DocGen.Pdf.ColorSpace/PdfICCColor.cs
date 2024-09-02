using System;

namespace DocGen.Pdf.ColorSpace;

public class PdfICCColor : PdfExtendedColor
{
	private double[] m_components;

	private PdfICCColorSpace m_colorspaces;

	public double[] ColorComponents
	{
		get
		{
			return m_components;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ColorComponents", "ColorComponents array cannot be null.");
			}
			PdfICCColorSpace pdfICCColorSpace = base.ColorSpace as PdfICCColorSpace;
			if (value.Length != pdfICCColorSpace.ColorComponents)
			{
				throw new ArgumentOutOfRangeException("ColorComponents", "Array length must match the number of color components defined on the underlying ICC colorspace.");
			}
			m_components = value;
		}
	}

	internal PdfICCColorSpace ColorSpaces => m_colorspaces;

	public PdfICCColor(PdfColorSpaces colorspace)
		: base(colorspace)
	{
		m_colorspaces = colorspace as PdfICCColorSpace;
	}
}
