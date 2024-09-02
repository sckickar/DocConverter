using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal struct TAddr
{
	private int m_iFirstRow;

	private int m_iLastRow;

	private int m_iFirstCol;

	private int m_iLastCol;

	public int FirstCol
	{
		get
		{
			return m_iFirstCol;
		}
		set
		{
			m_iFirstCol = value;
		}
	}

	public int FirstRow
	{
		get
		{
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	public int LastCol
	{
		get
		{
			return m_iLastCol;
		}
		set
		{
			m_iLastCol = value;
		}
	}

	public int LastRow
	{
		get
		{
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
		}
	}

	public TAddr(int iFirstRow, int iFirstCol, int iLastRow, int iLastCol)
	{
		m_iFirstRow = iFirstRow;
		m_iFirstCol = iFirstCol;
		m_iLastRow = iLastRow;
		m_iLastCol = iLastCol;
	}

	public TAddr(int iTopLeftIndex, int iBottomRightIndex)
	{
		m_iFirstRow = RangeImpl.GetRowFromCellIndex(iTopLeftIndex);
		m_iFirstCol = RangeImpl.GetColumnFromCellIndex(iTopLeftIndex);
		m_iLastRow = RangeImpl.GetRowFromCellIndex(iBottomRightIndex);
		m_iLastCol = RangeImpl.GetColumnFromCellIndex(iBottomRightIndex);
	}

	public TAddr(Rectangle rect)
	{
		m_iFirstCol = rect.X;
		m_iFirstRow = rect.Y;
		m_iLastCol = rect.Right;
		m_iLastRow = rect.Bottom;
	}

	public override string ToString()
	{
		return base.ToString() + " ( " + m_iFirstRow + ", " + m_iFirstCol + " ) - ( " + m_iLastRow + ", " + m_iLastCol + " )";
	}

	public Rectangle GetRectangle()
	{
		return Rectangle.FromLTRB(FirstCol, FirstRow, LastCol, LastRow);
	}
}
