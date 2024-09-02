using System;

namespace DocGen.Chart;

internal class ChartLegendItemClickEventArgs : EventArgs
{
	private ChartLegendItem m_item = new ChartLegendItem();

	private int m_index;

	private int m_LegendIndex;

	public ChartLegendItem Item
	{
		get
		{
			return m_item;
		}
		set
		{
			m_item = value;
		}
	}

	public int Index => m_index;

	public int LegendIndex => m_LegendIndex;

	public ChartLegendItemClickEventArgs(ChartLegendItem item, int index, int legendIndex)
	{
		m_item = item;
		m_index = index;
		m_LegendIndex = legendIndex;
	}
}
