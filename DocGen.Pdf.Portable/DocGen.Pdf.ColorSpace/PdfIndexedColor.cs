namespace DocGen.Pdf.ColorSpace;

public class PdfIndexedColor : PdfExtendedColor
{
	private int m_colorIndex;

	public int SelectColorIndex
	{
		get
		{
			return m_colorIndex;
		}
		set
		{
			m_colorIndex = value;
		}
	}

	public PdfIndexedColor(PdfIndexedColorSpace colorspace)
		: base(colorspace)
	{
	}
}
