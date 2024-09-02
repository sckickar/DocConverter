using DocGen.Drawing;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlToPdfParams
{
	private PdfPage m_page;

	private RectangleF m_bounds;

	private HtmlToPdfFormat m_format;

	private bool isSinglePageLayout;

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

	public HtmlToPdfFormat Format
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

	internal bool SinglePageLayout
	{
		get
		{
			return isSinglePageLayout;
		}
		set
		{
			isSinglePageLayout = value;
		}
	}
}
