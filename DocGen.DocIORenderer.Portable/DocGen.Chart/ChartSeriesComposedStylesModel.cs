using System;
using System.Collections;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartSeriesComposedStylesModel : IChartSeriesComposedStylesModel
{
	private const int c_seriesIndex = -1;

	private IChartSeriesStylesModel m_chartSeries;

	private Hashtable m_cache = new Hashtable();

	public ChartStyleInfo Style => GetStyle(-1, offLine: false);

	public ChartStyleInfo this[int index] => GetStyle(index, offLine: false);

	public int Count => m_cache.Count;

	public ChartSeriesComposedStylesModel(IChartSeriesStylesModel chartSeries)
	{
		m_chartSeries = chartSeries;
	}

	public ChartStyleInfo GetOfflineStyle()
	{
		return GetStyle(-1, offLine: true);
	}

	public ChartStyleInfo GetOfflineStyle(int index)
	{
		return GetStyle(index, offLine: true);
	}

	public ChartStyleInfo[] GetBaseStyles(IStyleInfo chartStyleInfo, int index)
	{
		return m_chartSeries.GetBaseStyles(chartStyleInfo, index);
	}

	public void ChangeStyle(ChartStyleInfo style, int index)
	{
		if (index == -1)
		{
			m_chartSeries.ChangeStyle(style);
			m_cache.Clear();
		}
		else
		{
			m_chartSeries.ChangeStyleAt(style, index);
			m_cache.Remove(index);
		}
		style.Store.ResetChangedBits();
	}

	public void ChangeStyle(ChartStyleInfo style)
	{
		ChangeStyle(style, -1);
	}

	public void ResetCache()
	{
		m_cache.Clear();
	}

	private ChartStyleInfo GetStyle(int index, bool offLine)
	{
		if (m_cache.ContainsKey(index))
		{
			WeakReference weakReference = m_cache[index] as WeakReference;
			if (weakReference.Target != null)
			{
				return weakReference.Target as ChartStyleInfo;
			}
		}
		ChartStyleInfo chartStyleInfo = new ChartStyleInfo(new ChartStyleInfoIdentity(this, index, offLine));
		m_cache[index] = new WeakReference(chartStyleInfo);
		return chartStyleInfo;
	}
}
