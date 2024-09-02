using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation;

internal class ItemSizeHelper
{
	internal delegate int SizeGetter(int index);

	private List<int> m_arrSizeSum = new List<int>();

	private SizeGetter m_getter;

	public ItemSizeHelper(SizeGetter sizeGetter)
	{
		if (sizeGetter == null)
		{
			throw new ArgumentNullException("sizeGetter");
		}
		m_getter = sizeGetter;
		m_arrSizeSum.Add(0);
	}

	public int GetTotal(int rowIndex)
	{
		int count = m_arrSizeSum.Count;
		if (count <= rowIndex)
		{
			m_arrSizeSum.Capacity = Math.Max(m_arrSizeSum.Capacity, rowIndex);
			int num = 0;
			num = m_arrSizeSum[count - 1];
			for (int i = count; i <= rowIndex; i++)
			{
				num += m_getter(i);
				m_arrSizeSum.Add(num);
			}
		}
		return m_arrSizeSum[rowIndex];
	}

	public int GetTotal(int rowStart, int rowEnd)
	{
		if (rowStart > rowEnd)
		{
			return 0;
		}
		return GetTotal(rowEnd) - GetTotal(rowStart - 1);
	}

	public int GetSize(int itemIndex)
	{
		return GetTotal(itemIndex) - GetTotal(itemIndex - 1);
	}
}
