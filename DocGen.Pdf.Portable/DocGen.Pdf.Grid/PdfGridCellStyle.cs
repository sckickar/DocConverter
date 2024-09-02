using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridCellStyle : PdfGridRowStyle
{
	private PdfBorders m_borders = PdfBorders.Default;

	private PdfPaddings m_cellPadding;

	private PdfEdges m_edges;

	private PdfStringFormat m_format;

	private PdfImage m_backgroundImage;

	public PdfStringFormat StringFormat
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

	public PdfBorders Borders
	{
		get
		{
			return m_borders;
		}
		set
		{
			m_borders = value;
		}
	}

	public PdfImage BackgroundImage
	{
		get
		{
			return m_backgroundImage;
		}
		set
		{
			m_backgroundImage = value;
		}
	}

	internal PdfEdges Edges
	{
		get
		{
			if (m_edges == null)
			{
				m_edges = new PdfEdges();
			}
			return m_edges;
		}
		set
		{
			m_edges = value;
		}
	}

	public PdfPaddings CellPadding
	{
		get
		{
			if (m_cellPadding == null)
			{
				m_cellPadding = base.GridCellPadding;
			}
			return m_cellPadding;
		}
		set
		{
			if (m_cellPadding == null)
			{
				m_cellPadding = new PdfPaddings();
			}
			m_cellPadding = value;
		}
	}
}
