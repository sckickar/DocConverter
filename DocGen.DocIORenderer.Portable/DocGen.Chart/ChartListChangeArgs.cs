using System;

namespace DocGen.Chart;

internal class ChartListChangeArgs : EventArgs
{
	private int m_index;

	private object[] m_oldItems;

	private object[] m_newItems;

	public int Index => m_index;

	public object[] OldItems => m_oldItems;

	public object[] NewItems => m_newItems;

	public ChartListChangeArgs(int index, object[] oldItems, object[] newItems)
	{
		m_index = index;
		m_oldItems = oldItems;
		m_newItems = newItems;
	}
}
