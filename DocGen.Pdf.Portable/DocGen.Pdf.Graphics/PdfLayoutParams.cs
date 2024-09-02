using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfLayoutParams
{
	private PdfPage m_page;

	private RectangleF m_bounds;

	private PdfLayoutFormat m_format;

	internal PdfGraphics m_graphics;

	public PdfPage Page
	{
		get
		{
			return m_page;
		}
		set
		{
			m_page = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public PdfLayoutFormat Format
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}
}
