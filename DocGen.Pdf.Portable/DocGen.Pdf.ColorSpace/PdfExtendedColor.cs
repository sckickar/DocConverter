namespace DocGen.Pdf.ColorSpace;

public abstract class PdfExtendedColor
{
	protected PdfColorSpaces m_colorspace;

	public PdfColorSpaces ColorSpace => m_colorspace;

	public PdfExtendedColor(PdfColorSpaces colorspace)
	{
		m_colorspace = colorspace;
	}
}
