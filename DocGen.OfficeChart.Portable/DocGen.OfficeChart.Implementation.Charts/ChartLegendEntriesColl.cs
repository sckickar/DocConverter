using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartLegendEntriesColl : CommonObject, IChartLegendEntries
{
	private Dictionary<int, ChartLegendEntryImpl> m_hashEntries = new Dictionary<int, ChartLegendEntryImpl>();

	private ChartImpl m_parentChart;

	internal Dictionary<int, ChartLegendEntryImpl> HashEntries => m_hashEntries;

	public int Count
	{
		get
		{
			IOfficeChartSeries series = m_parentChart.Series;
			string startSerieType = ChartFormatImpl.GetStartSerieType(m_parentChart.ChartType);
			bool num = Array.IndexOf(ChartImpl.DEF_LEGEND_NEED_DATA_POINT, startSerieType) == -1;
			int num2 = 0;
			if (num)
			{
				int num3 = series.Count;
				int i = 0;
				for (int count = series.Count; i < count; i++)
				{
					IOfficeChartSerie officeChartSerie = series[i];
					num3 += officeChartSerie.TrendLines.Count;
				}
				return num3;
			}
			return ((ChartSerieImpl)series[0]).PointNumber;
		}
	}

	public IOfficeChartLegendEntry this[int iIndex]
	{
		get
		{
			if (!m_parentChart.Loading && iIndex >= Count)
			{
				throw new ArgumentOutOfRangeException("iIndex");
			}
			if (m_hashEntries.ContainsKey(iIndex))
			{
				return m_hashEntries[iIndex];
			}
			return Add(iIndex);
		}
	}

	public ChartLegendEntriesColl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	private void SetParents()
	{
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_parentChart == null)
		{
			throw new ApplicationException("Can't find parent object.");
		}
	}

	public ChartLegendEntryImpl Add(int iIndex)
	{
		if (m_hashEntries.ContainsKey(iIndex))
		{
			return m_hashEntries[iIndex];
		}
		ChartLegendEntryImpl entry = new ChartLegendEntryImpl(base.Application, this, iIndex);
		return Add(iIndex, entry);
	}

	public ChartLegendEntryImpl Add(int iIndex, ChartLegendEntryImpl entry)
	{
		if (!m_parentChart.Loading && iIndex >= Count)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		entry.Index = iIndex;
		if (!m_parentChart.Loading)
		{
			string startSerieType = ChartFormatImpl.GetStartSerieType(m_parentChart.ChartType);
			if (Array.IndexOf(ChartImpl.DEF_LEGEND_NEED_DATA_POINT, startSerieType) != -1)
			{
				entry.LegendEntityIndex = iIndex;
			}
		}
		if (m_hashEntries.ContainsKey(iIndex))
		{
			m_hashEntries[iIndex] = entry;
		}
		else
		{
			m_hashEntries.Add(iIndex, entry);
		}
		IOfficeChartSeries series = m_parentChart.Series;
		List<IOfficeChartTrendLine> list = new List<IOfficeChartTrendLine>();
		int num = iIndex - series.Count;
		if (iIndex >= series.Count)
		{
			foreach (IOfficeChartSerie item in series)
			{
				if (item.TrendLines.Count > 0)
				{
					for (int i = 0; i < item.TrendLines.Count; i++)
					{
						list.Add(item.TrendLines[i]);
					}
				}
			}
		}
		ChartLegendEntryImpl chartLegendEntryImpl = Add(iIndex);
		if (iIndex < series.Count)
		{
			chartLegendEntryImpl.TextArea.Text = series[iIndex].Name;
			chartLegendEntryImpl.IsFormatted = false;
		}
		if (num >= 0 && num < list.Count)
		{
			(list[num] as ChartTrendLineImpl).LegendEntry = chartLegendEntryImpl;
		}
		return chartLegendEntryImpl;
	}

	public bool Contains(int iIndex)
	{
		return m_hashEntries.ContainsKey(iIndex);
	}

	public bool CanDelete(int iIndex)
	{
		if (m_hashEntries.Count != Count)
		{
			return true;
		}
		int i = 0;
		for (int count = m_hashEntries.Count; i < count; i++)
		{
			if (i != iIndex)
			{
				ChartLegendEntryImpl value = null;
				if (m_hashEntries.TryGetValue(i, out value) && !value.IsDeleted)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Remove(int iIndex)
	{
		int count = Count;
		string startSerieType = ChartFormatImpl.GetStartSerieType(m_parentChart.ChartType);
		if (Array.IndexOf(ChartImpl.DEF_LEGEND_NEED_DATA_POINT, startSerieType) != -1)
		{
			return;
		}
		if (iIndex < 0 || iIndex >= count)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		if (m_hashEntries.ContainsKey(iIndex))
		{
			m_hashEntries.Remove(iIndex);
		}
		for (int i = iIndex + 1; i < count; i++)
		{
			if (m_hashEntries.ContainsKey(i))
			{
				ChartLegendEntryImpl chartLegendEntryImpl = m_hashEntries[i];
				if (chartLegendEntryImpl.IsDeleted)
				{
					chartLegendEntryImpl.Index = i - 1;
					chartLegendEntryImpl.IsDeleted = true;
				}
				else
				{
					chartLegendEntryImpl.Index = i - 1;
				}
				m_hashEntries.Add(i - 1, chartLegendEntryImpl);
				m_hashEntries.Remove(i);
			}
		}
	}

	public ChartLegendEntriesColl Clone(object parent, Dictionary<int, int> dicIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartLegendEntriesColl chartLegendEntriesColl = (ChartLegendEntriesColl)MemberwiseClone();
		chartLegendEntriesColl.SetParent(parent);
		chartLegendEntriesColl.SetParents();
		int count = m_hashEntries.Count;
		chartLegendEntriesColl.m_hashEntries = new Dictionary<int, ChartLegendEntryImpl>(count);
		if (count == 0)
		{
			return chartLegendEntriesColl;
		}
		foreach (KeyValuePair<int, ChartLegendEntryImpl> hashEntry in m_hashEntries)
		{
			ChartLegendEntryImpl value = hashEntry.Value;
			value = value.Clone(chartLegendEntriesColl, dicIndexes, dicNewSheetNames);
			chartLegendEntriesColl.m_hashEntries.Add(hashEntry.Key, value);
		}
		return chartLegendEntriesColl;
	}

	public void Clear()
	{
		m_hashEntries.Clear();
	}

	public void UpdateEntries(int entryIndex, int value)
	{
		_ = Count;
		for (int num = Count - 1; num >= entryIndex; num--)
		{
			if (m_hashEntries.ContainsKey(num))
			{
				ChartLegendEntryImpl chartLegendEntryImpl = m_hashEntries[num];
				chartLegendEntryImpl.Index += value;
				m_hashEntries.Add(chartLegendEntryImpl.Index, chartLegendEntryImpl);
				m_hashEntries.Remove(num);
			}
		}
	}
}
