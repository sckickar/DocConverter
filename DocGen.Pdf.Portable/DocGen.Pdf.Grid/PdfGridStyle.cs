namespace DocGen.Pdf.Grid;

public class PdfGridStyle : PdfGridStyleBase
{
	private float m_cellSpacing;

	private PdfPaddings m_cellPadding;

	private PdfBorderOverlapStyle m_borderOverlapStyle;

	private bool m_bAllowHorizontalOverflow;

	private PdfHorizontalOverflowType m_HorizontalOverflowType;

	public float CellSpacing
	{
		get
		{
			return m_cellSpacing;
		}
		set
		{
			m_cellSpacing = value;
		}
	}

	public PdfPaddings CellPadding
	{
		get
		{
			if (m_cellPadding == null)
			{
				m_cellPadding = new PdfPaddings();
			}
			base.GridCellPadding = m_cellPadding;
			return m_cellPadding;
		}
		set
		{
			m_cellPadding = value;
			base.GridCellPadding = m_cellPadding;
		}
	}

	public PdfBorderOverlapStyle BorderOverlapStyle
	{
		get
		{
			return m_borderOverlapStyle;
		}
		set
		{
			m_borderOverlapStyle = value;
		}
	}

	public bool AllowHorizontalOverflow
	{
		get
		{
			return m_bAllowHorizontalOverflow;
		}
		set
		{
			m_bAllowHorizontalOverflow = value;
		}
	}

	public PdfHorizontalOverflowType HorizontalOverflowType
	{
		get
		{
			return m_HorizontalOverflowType;
		}
		set
		{
			m_HorizontalOverflowType = value;
		}
	}

	public PdfGridStyle()
	{
		m_borderOverlapStyle = PdfBorderOverlapStyle.Overlap;
		m_bAllowHorizontalOverflow = false;
		m_HorizontalOverflowType = PdfHorizontalOverflowType.LastPage;
	}
}
