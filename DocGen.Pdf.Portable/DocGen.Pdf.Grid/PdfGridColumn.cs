using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridColumn
{
	private PdfGrid m_grid;

	internal float m_width = float.MinValue;

	internal bool isCustomWidth;

	private PdfStringFormat m_format;

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			isCustomWidth = true;
			m_width = value;
		}
	}

	public PdfStringFormat Format
	{
		get
		{
			if (m_format == null)
			{
				m_format = new PdfStringFormat();
			}
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public PdfGrid Grid => m_grid;

	public PdfGridColumn(PdfGrid grid)
	{
		m_grid = grid;
	}
}
