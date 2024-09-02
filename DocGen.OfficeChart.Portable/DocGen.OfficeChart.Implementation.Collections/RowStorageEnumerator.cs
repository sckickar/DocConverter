using System;
using System.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class RowStorageEnumerator : IEnumerator
{
	private RowStorage m_rowStorage;

	private int m_iOffset = -1;

	private RecordExtractor m_recordExtractor;

	public object Current => m_recordExtractor.GetRecord(m_rowStorage.Provider, m_iOffset, m_rowStorage.Version);

	public int RowIndex
	{
		get
		{
			if (m_iOffset == -1)
			{
				throw new InvalidOperationException("Enumerator pointer is not set to an object instance");
			}
			return m_rowStorage.GetRow(m_iOffset);
		}
	}

	public int ColumnIndex
	{
		get
		{
			if (m_iOffset == -1)
			{
				throw new InvalidOperationException("Enumerator pointer is not set to an object instance");
			}
			return m_rowStorage.GetColumn(m_iOffset);
		}
	}

	public int XFIndex
	{
		get
		{
			if (m_iOffset == -1)
			{
				throw new InvalidOperationException("Enumerator pointer is not set to an object instance");
			}
			return m_rowStorage.GetXFIndex(m_iOffset, bMulti: false);
		}
	}

	private RowStorageEnumerator()
	{
	}

	public RowStorageEnumerator(RowStorage row, RecordExtractor recordExtractor)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (recordExtractor == null)
		{
			throw new ArgumentNullException("recordExtractor");
		}
		m_rowStorage = row;
		m_recordExtractor = recordExtractor;
	}

	public void Reset()
	{
		m_iOffset = -1;
	}

	public bool MoveNext()
	{
		if (m_rowStorage.UsedSize == 0)
		{
			return false;
		}
		if (m_iOffset == -1)
		{
			m_iOffset = 0;
			return true;
		}
		int num = m_rowStorage.MoveNextCell(m_iOffset);
		if (num == m_rowStorage.UsedSize)
		{
			m_iOffset = -1;
			return false;
		}
		m_iOffset = num;
		return true;
	}

	[CLSCompliant(false)]
	public ArrayRecord GetArrayRecord()
	{
		if (m_iOffset == -1)
		{
			throw new InvalidOperationException("Enumerator pointer is not set to an object instance");
		}
		return m_rowStorage.GetArrayRecordByOffset(m_iOffset);
	}

	public string GetFormulaStringValue()
	{
		if (m_iOffset == -1)
		{
			throw new InvalidOperationException("Enumerator pointer is not set to an object instance");
		}
		return m_rowStorage.GetFormulaStringValueByOffset(m_iOffset);
	}
}
