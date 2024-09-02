using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaSolidBrush : PdfXfaBrush
{
	private PdfColor m_color;

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public PdfXfaSolidBrush(PdfColor color)
	{
		m_color = color;
	}
}
