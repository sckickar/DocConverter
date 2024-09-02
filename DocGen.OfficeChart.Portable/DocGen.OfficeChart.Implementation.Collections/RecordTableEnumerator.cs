using System;
using System.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class RecordTableEnumerator : IDictionaryEnumerator, IEnumerator
{
	private CellRecordCollection m_table;

	private int m_iRow = -1;

	private RecordTable m_sfTable;

	private int m_iOffset;

	public object Current
	{
		get
		{
			if (m_iRow < 0)
			{
				return null;
			}
			RowStorage rowStorage = m_sfTable.Rows[m_iRow];
			if (rowStorage == null)
			{
				return new DictionaryEntry(null, null);
			}
			ICellPositionFormat cellPositionFormat = (ICellPositionFormat)rowStorage.GetRecordAtOffset(m_iOffset);
			return new DictionaryEntry(RangeImpl.GetCellIndex(cellPositionFormat.Column + 1, cellPositionFormat.Row + 1), cellPositionFormat);
		}
	}

	public object Key => Entry.Key;

	public object Value => Entry.Value;

	public DictionaryEntry Entry
	{
		get
		{
			if (m_iRow < 0)
			{
				throw new InvalidOperationException();
			}
			ICellPositionFormat cellPositionFormat = (ICellPositionFormat)m_sfTable.Rows[m_iRow].GetRecordAtOffset(m_iOffset);
			return new DictionaryEntry(RangeImpl.GetCellIndex(cellPositionFormat.Column + 1, cellPositionFormat.Row + 1), cellPositionFormat);
		}
	}

	private RecordTableEnumerator()
	{
	}

	public RecordTableEnumerator(CellRecordCollection table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		m_table = table;
		m_sfTable = table.Table;
	}

	public void Reset()
	{
		m_iRow = m_sfTable.FirstRow - 1;
		m_iOffset = 0;
		MoveNextRow();
	}

	public bool MoveNext()
	{
		int lastRow = m_sfTable.LastRow;
		if (m_iRow < 0)
		{
			m_iRow = m_sfTable.FirstRow;
			m_iOffset = 0;
			if (m_iRow < 0 || m_iRow > lastRow)
			{
				return false;
			}
			RowStorage rowStorage = m_sfTable.Rows[m_iRow];
			while ((rowStorage == null || rowStorage.UsedSize <= 0) && m_iRow <= lastRow)
			{
				m_iRow++;
				rowStorage = m_sfTable.Rows[m_iRow];
			}
			return true;
		}
		RowStorage rowStorage2 = m_sfTable.Rows[m_iRow];
		if (rowStorage2 != null)
		{
			m_iOffset = rowStorage2.MoveNextCell(m_iOffset);
			if (m_iOffset >= rowStorage2.UsedSize)
			{
				return MoveNextRow();
			}
			return true;
		}
		return false;
	}

	private bool MoveNextRow()
	{
		int lastRow = m_sfTable.LastRow;
		ArrayListEx rows = m_sfTable.Rows;
		m_iRow++;
		while (m_iRow <= lastRow)
		{
			RowStorage rowStorage = rows[m_iRow];
			if (rowStorage != null && rowStorage.UsedSize > 0)
			{
				m_iOffset = 0;
				return true;
			}
			m_iRow++;
		}
		return false;
	}
}
