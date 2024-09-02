using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfLayoutResult
{
	private PdfPage m_page;

	private RectangleF m_bounds;

	private double m_totalPageSize;

	public PdfPage Page => m_page;

	public RectangleF Bounds => m_bounds;

	internal double TotalPageSize
	{
		get
		{
			return m_totalPageSize;
		}
		set
		{
			m_totalPageSize = value;
		}
	}

	public PdfLayoutResult(PdfPage page, RectangleF bounds)
	{
		m_page = page;
		m_bounds = bounds;
	}
}
