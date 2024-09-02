using System;
using System.Collections.Generic;

namespace DocGen.Chart;

internal sealed class ChartPointFormatsRegistry
{
	private class ChartPointFormat : Dictionary<ChartYValueUsage, int>
	{
		public ChartPointFormat()
		{
		}

		public ChartPointFormat(params ChartYValueUsage[] usages)
		{
			for (int i = 0; i < usages.Length; i++)
			{
				if (usages[i] != 0)
				{
					if (ContainsKey(usages[i]))
					{
						throw new ArgumentException($"{usages[i]} usage is already presented.");
					}
					Add(usages[i], i);
				}
			}
		}
	}

	private static ChartPointFormat c_default;

	private Dictionary<ChartSeriesType, ChartPointFormat> m_pointFormats = new Dictionary<ChartSeriesType, ChartPointFormat>();

	private ChartSeries m_series;

	private int m_cacheYIndex = -1;

	public int this[ChartSeriesType type, ChartYValueUsage usage]
	{
		get
		{
			if (m_pointFormats.ContainsKey(type))
			{
				return m_pointFormats[type][usage];
			}
			return c_default[usage];
		}
	}

	public int this[ChartYValueUsage usage] => this[m_series.Type, usage];

	internal int YIndex => m_cacheYIndex;

	static ChartPointFormatsRegistry()
	{
		c_default = new ChartPointFormat
		{
			[ChartYValueUsage.YValue] = 0,
			[ChartYValueUsage.LowValue] = 0,
			[ChartYValueUsage.HighValue] = 1,
			[ChartYValueUsage.OpenValue] = 2,
			[ChartYValueUsage.CloseValue] = 3,
			[ChartYValueUsage.ErrorBarValue] = 1,
			[ChartYValueUsage.PointSizeValue] = 1
		};
	}

	internal ChartPointFormatsRegistry(ChartSeries series)
	{
		if (c_default == null)
		{
			c_default = new ChartPointFormat
			{
				[ChartYValueUsage.YValue] = 0,
				[ChartYValueUsage.LowValue] = 0,
				[ChartYValueUsage.HighValue] = 1,
				[ChartYValueUsage.OpenValue] = 2,
				[ChartYValueUsage.CloseValue] = 3,
				[ChartYValueUsage.ErrorBarValue] = 1,
				[ChartYValueUsage.PointSizeValue] = 1
			};
		}
		m_series = series;
	}

	internal void Dispose()
	{
		c_default = null;
		m_pointFormats = null;
		m_series = null;
	}

	public void Register(ChartSeriesType type, params ChartYValueUsage[] usages)
	{
		m_pointFormats[type] = new ChartPointFormat(usages);
		m_series.OnAppearanceChanged(EventArgs.Empty);
	}

	internal void OnSeriesTypeChanged(ChartSeriesType type)
	{
		m_cacheYIndex = this[type, ChartYValueUsage.YValue];
	}
}
