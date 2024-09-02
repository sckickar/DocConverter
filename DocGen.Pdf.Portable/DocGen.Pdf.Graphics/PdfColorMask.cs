namespace DocGen.Pdf.Graphics;

public class PdfColorMask : PdfMask
{
	private PdfColor m_startColor;

	private PdfColor m_endColor;

	public PdfColor StartColor
	{
		get
		{
			return m_startColor;
		}
		set
		{
			m_startColor = value;
		}
	}

	public PdfColor EndColor
	{
		get
		{
			return m_endColor;
		}
		set
		{
			m_endColor = value;
		}
	}

	public PdfColorMask(PdfColor startColor, PdfColor endColor)
	{
		m_endColor = endColor;
		m_startColor = startColor;
	}
}
