namespace DocGen.Pdf.ColorSpace;

public class PdfSeparationColor : PdfExtendedColor
{
	private double m_tint = 1.0;

	public double Tint
	{
		get
		{
			return m_tint;
		}
		set
		{
			m_tint = value;
		}
	}

	public PdfSeparationColor(PdfColorSpaces colorspace)
		: base(colorspace)
	{
	}
}
