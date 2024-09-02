using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfLightTableStyle
{
	private PdfCellStyle m_defaultStyle;

	private PdfCellStyle m_alternateStyle;

	private PdfHeaderSource m_headerSource;

	private int m_headerRowCount;

	private PdfCellStyle m_headerStyle;

	private bool m_bRepeateHeader;

	private bool m_bShowHeader;

	private float m_cellSpacing;

	private float m_cellPadding;

	private PdfBorderOverlapStyle m_overlappedBorders;

	private PdfPen m_borderPen;

	public PdfCellStyle DefaultStyle
	{
		get
		{
			return m_defaultStyle;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DefaultStyle");
			}
			m_defaultStyle = value;
		}
	}

	public PdfCellStyle AlternateStyle
	{
		get
		{
			return m_alternateStyle;
		}
		set
		{
			m_alternateStyle = value;
		}
	}

	public PdfHeaderSource HeaderSource
	{
		get
		{
			return m_headerSource;
		}
		set
		{
			m_headerSource = value;
		}
	}

	public int HeaderRowCount
	{
		get
		{
			return m_headerRowCount;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("HeaderRowsCount", "This parameter can't be less then zero");
			}
			m_headerRowCount = value;
		}
	}

	public PdfCellStyle HeaderStyle
	{
		get
		{
			return m_headerStyle;
		}
		set
		{
			m_headerStyle = value;
		}
	}

	public bool RepeatHeader
	{
		get
		{
			return m_bRepeateHeader;
		}
		set
		{
			m_bRepeateHeader = value;
		}
	}

	public bool ShowHeader
	{
		get
		{
			return m_bShowHeader;
		}
		set
		{
			m_bShowHeader = value;
		}
	}

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

	public float CellPadding
	{
		get
		{
			return m_cellPadding;
		}
		set
		{
			m_cellPadding = value;
		}
	}

	public PdfBorderOverlapStyle BorderOverlapStyle
	{
		get
		{
			return m_overlappedBorders;
		}
		set
		{
			m_overlappedBorders = value;
		}
	}

	public PdfPen BorderPen
	{
		get
		{
			return m_borderPen;
		}
		set
		{
			m_borderPen = value;
		}
	}

	public PdfLightTableStyle()
	{
		m_defaultStyle = new PdfCellStyle();
		m_bRepeateHeader = true;
		m_overlappedBorders = PdfBorderOverlapStyle.Overlap;
	}
}
