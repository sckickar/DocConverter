namespace DocGen.Pdf.Graphics;

public abstract class PdfFillElement : PdfDrawElement
{
	private PdfBrush m_brush;

	public PdfBrush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	protected PdfFillElement()
	{
	}

	protected PdfFillElement(PdfPen pen)
		: base(pen)
	{
	}

	protected PdfFillElement(PdfBrush brush)
		: this()
	{
		m_brush = brush;
	}

	protected PdfFillElement(PdfPen pen, PdfBrush brush)
		: this(pen)
	{
		m_brush = brush;
	}

	protected override PdfPen ObtainPen()
	{
		if (m_brush != null || base.Pen != null)
		{
			return base.Pen;
		}
		return PdfPens.Black;
	}
}
