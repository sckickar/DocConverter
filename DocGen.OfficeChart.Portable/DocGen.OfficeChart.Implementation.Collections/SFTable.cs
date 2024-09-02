using System;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class SFTable : ICloneable
{
	private int m_iRowCount;

	private int m_iColumnCount;

	private SFArrayList<object> m_arrRows;

	private int m_iCellCount;

	public SFArrayList<object> Rows
	{
		get
		{
			if (m_arrRows == null)
			{
				m_arrRows = new SFArrayList<object>();
			}
			return m_arrRows;
		}
	}

	public int RowCount => m_iRowCount;

	public int ColCount => m_iColumnCount;

	public int CellCount => m_iCellCount;

	public object this[int rowIndex, int colIndex]
	{
		get
		{
			if (rowIndex >= m_iRowCount || rowIndex < 0 || colIndex >= m_iColumnCount || colIndex < 0)
			{
				return null;
			}
			if (Rows[rowIndex] is SFArrayList<object> sFArrayList)
			{
				return sFArrayList[colIndex];
			}
			return null;
		}
		set
		{
			if (rowIndex >= m_iRowCount || rowIndex < 0)
			{
				throw new ArgumentOutOfRangeException("rowIndex");
			}
			if (colIndex >= m_iColumnCount || colIndex < 0)
			{
				throw new ArgumentOutOfRangeException("colIndex");
			}
			SFArrayList<object> rows = Rows;
			SFArrayList<object> sFArrayList = rows[rowIndex] as SFArrayList<object>;
			if (sFArrayList == null)
			{
				if (value == null)
				{
					return;
				}
				sFArrayList = (SFArrayList<object>)(rows[rowIndex] = CreateCellCollection());
			}
			if (sFArrayList[colIndex] != null)
			{
				if (value == null)
				{
					m_iCellCount--;
				}
			}
			else if (value != null)
			{
				m_iCellCount++;
			}
			sFArrayList[colIndex] = value;
		}
	}

	public SFTable(int iRowCount, int iColumnCount)
	{
		m_iRowCount = iRowCount;
		m_iColumnCount = iColumnCount;
	}

	protected SFTable(SFTable data, bool clone)
	{
		m_iRowCount = data.m_iRowCount;
		m_iColumnCount = data.m_iColumnCount;
		if (data.m_arrRows != null && clone)
		{
			m_iCellCount = data.m_iCellCount;
			m_arrRows = (SFArrayList<object>)data.m_arrRows.Clone();
		}
	}

	protected SFTable(SFTable data, bool clone, object parent)
	{
		m_iRowCount = data.m_iRowCount;
		m_iColumnCount = data.m_iColumnCount;
		if (data.m_arrRows != null && clone)
		{
			m_iCellCount = data.m_iCellCount;
			m_arrRows = (SFArrayList<object>)data.m_arrRows.Clone(parent);
		}
	}

	public virtual object Clone()
	{
		return new SFTable(this, clone: true);
	}

	public virtual object Clone(object parent)
	{
		return new SFTable(this, clone: true, parent);
	}

	public void Clear()
	{
		m_arrRows = null;
	}

	public virtual SFArrayList<object> CreateCellCollection()
	{
		return new SFArrayList<object>();
	}

	public bool Contains(int rowIndex, int colIndex)
	{
		if (rowIndex < 0 || rowIndex >= m_iRowCount || colIndex < 0 || colIndex >= m_iColumnCount)
		{
			return false;
		}
		if (Rows[rowIndex] is SFArrayList<object> sFArrayList)
		{
			return sFArrayList[colIndex] != null;
		}
		return false;
	}
}
