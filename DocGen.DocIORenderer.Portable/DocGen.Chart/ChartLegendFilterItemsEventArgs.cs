using System;

namespace DocGen.Chart;

internal class ChartLegendFilterItemsEventArgs : EventArgs
{
	private ChartLegendItemsCollection m_items;

	public ChartLegendItemsCollection Items
	{
		get
		{
			return m_items;
		}
		set
		{
			m_items = value;
		}
	}

	public ChartLegendFilterItemsEventArgs(ChartLegendItem[] items)
	{
		m_items = new ChartLegendItemsCollection();
		int i = 0;
		for (int num = items.Length; i < num; i++)
		{
			m_items.Add(items[i]);
		}
	}

	public ChartLegendFilterItemsEventArgs(ChartLegendItemsCollection items)
	{
		m_items = items;
	}
}
