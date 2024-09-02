namespace DocGen.Pdf.Graphics;

public abstract class PdfDrawElement : PdfShapeElement
{
	private PdfPen m_pen;

	public PdfPen Pen
	{
		get
		{
			return m_pen;
		}
		set
		{
			m_pen = value;
		}
	}

	protected PdfDrawElement()
	{
	}

	protected PdfDrawElement(PdfPen pen)
		: this()
	{
		m_pen = pen;
	}

	protected virtual PdfPen ObtainPen()
	{
		if (m_pen != null)
		{
			return m_pen;
		}
		return PdfPens.Black;
	}
}
