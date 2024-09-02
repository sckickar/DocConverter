using System;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ArrayListEx
{
	private RowStorage[] m_items;

	private int m_iCount;

	public RowStorage this[int index]
	{
		get
		{
			if (index >= 0 && index < m_iCount)
			{
				return m_items[index];
			}
			return null;
		}
		set
		{
			if (m_iCount <= index)
			{
				UpdateSize(index + 1);
			}
			m_items[index] = value;
		}
	}

	public ArrayListEx()
	{
	}

	public ArrayListEx(int iCount)
	{
		if (iCount <= 0)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		m_iCount = iCount;
		m_items = new RowStorage[m_iCount];
	}

	public void UpdateSize(int iCount)
	{
		if (iCount > m_iCount)
		{
			int num = m_iCount * 2;
			m_iCount = ((iCount >= num) ? iCount : num);
			RowStorage[] array = new RowStorage[m_iCount];
			if (m_items != null)
			{
				m_items.CopyTo(array, 0);
			}
			m_items = array;
		}
	}

	public void ReduceSizeIfNecessary(int iCount)
	{
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		if (iCount < m_iCount)
		{
			RowStorage[] array = new RowStorage[iCount];
			Array.Copy(m_items, 0, array, 0, iCount);
			m_items = array;
			m_iCount = iCount;
		}
	}

	public void Insert(int index, int count, int length)
	{
		Array.Copy(m_items, index, m_items, index + count, length);
		int i = index;
		for (int num = index + count; i < num; i++)
		{
			m_items[i] = null;
		}
	}

	internal int GetCount()
	{
		return m_iCount;
	}

	internal bool GetRowIndex(int row, out int arrIndex)
	{
		if (m_iCount == 0)
		{
			arrIndex = 0;
			return false;
		}
		int num = 0;
		int num2 = 0;
		int num3 = m_iCount - 1;
		if (num3 == row)
		{
			arrIndex = row;
			return true;
		}
		if (num3 < row)
		{
			arrIndex = num3 + 1;
			return false;
		}
		while (num2 <= num3)
		{
			num = (num2 + num3) / 2;
			int num4 = row - num;
			if (num4 == 0)
			{
				arrIndex = num;
				return true;
			}
			if (num4 < 0)
			{
				num3 = num - 1;
			}
			else
			{
				num2 = num + 1;
			}
		}
		if (row > num)
		{
			arrIndex = num + 1;
		}
		else
		{
			arrIndex = num;
		}
		return false;
	}
}
