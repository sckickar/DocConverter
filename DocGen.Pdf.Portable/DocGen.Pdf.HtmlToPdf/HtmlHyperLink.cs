using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlHyperLink
{
	private RectangleF m_bounds;

	private string m_href;

	private string m_name;

	private string m_hash;

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

	internal string Hash
	{
		get
		{
			return m_hash;
		}
		set
		{
			m_hash = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public string Href
	{
		get
		{
			return m_href;
		}
		set
		{
			m_href = value;
		}
	}

	public HtmlHyperLink()
	{
	}

	public HtmlHyperLink(RectangleF Bounds, string Href)
	{
		m_bounds = Bounds;
		m_href = Href;
		ConvertBoundsToPoint();
	}

	internal void ConvertBoundsToPoint()
	{
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
		m_bounds = pdfUnitConvertor.ConvertFromPixels(m_bounds, PdfGraphicsUnit.Point);
	}
}
