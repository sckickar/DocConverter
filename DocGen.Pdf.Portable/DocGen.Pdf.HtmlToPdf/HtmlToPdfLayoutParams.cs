using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.HtmlToPdf;

public class HtmlToPdfLayoutParams : PdfLayoutParams
{
	private PdfPage m_page;

	private float[] m_verticalOffsets;

	private RectangleF m_bounds;

	private PdfLayoutFormat m_format;

	public new PdfPage Page
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

	public new RectangleF Bounds
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

	public float[] VerticalOffsets
	{
		get
		{
			return m_verticalOffsets;
		}
		set
		{
			m_verticalOffsets = value;
		}
	}

	public new PdfLayoutFormat Format
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
