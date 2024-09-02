using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfBorders
{
	private PdfPen m_left;

	private PdfPen m_right;

	private PdfPen m_top;

	private PdfPen m_bottom;

	public PdfPen Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	public PdfPen Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	public PdfPen Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	public PdfPen Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	public PdfPen All
	{
		set
		{
			m_left = (m_right = (m_top = (m_bottom = value)));
		}
	}

	internal bool IsAll
	{
		get
		{
			if (m_left == m_right && m_left == m_top)
			{
				return m_left == m_bottom;
			}
			return false;
		}
	}

	public static PdfBorders Default => new PdfBorders();

	public PdfBorders()
	{
		All = new PdfPen(new PdfColor(0, 0, 0))
		{
			DashStyle = PdfDashStyle.Solid
		};
	}
}
