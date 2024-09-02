using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartModel
{
	private ChartBaseStylesMap m_baseStylesMap = new ChartBaseStylesMap();

	private ChartColorModel m_colorModel = new ChartColorModel();

	private ChartSeriesCollection m_series;

	private ChartIndexedValues m_intexed;

	private IChartAreaHost m_chart;

	private double m_minPointsDelta = double.NaN;

	private ChartSeries m_firstSeries;

	internal ArrayList labelStore = new ArrayList();

	public ChartColorModel ColorModel => m_colorModel;

	public ChartSeriesCollection Series => m_series;

	public ChartIndexedValues IndexedValues => m_intexed;

	internal IChartAreaHost Chart => m_chart;

	internal ChartSeries FirstSeries => m_firstSeries;

	public ChartModel()
	{
		m_series = new ChartSeriesCollection(this);
		m_intexed = new ChartIndexedValues(this);
		m_series.Changed += OnSeriesChanged;
	}

	public void SetChart(IChartAreaHost chartHost)
	{
		m_chart = chartHost;
	}

	public bool CheckSeriesCompatibility(ChartArea chartArea, bool invertedSeriesIsCompatible)
	{
		bool result = true;
		if (m_series.Count > 0)
		{
			bool flag = false;
			bool flag2 = false;
			m_firstSeries = null;
			foreach (ChartSeries item in m_series)
			{
				if (m_firstSeries == null)
				{
					if (item.Visible)
					{
						m_firstSeries = item;
						chartArea.RequireAxes = m_firstSeries.RequireAxes;
						chartArea.RequireInvertedAxes = m_firstSeries.RequireInvertedAxes;
						flag = m_firstSeries.Type == ChartSeriesType.Radar;
						flag2 = m_firstSeries.Type == ChartSeriesType.Polar;
						item.Compatible = true;
					}
				}
				else
				{
					bool flag3 = ((!invertedSeriesIsCompatible) ? (item.RequireAxes != chartArea.RequireAxes || item.RequireInvertedAxes != chartArea.RequireInvertedAxes) : ((!item.RequireAxes || !item.RequireInvertedAxes) && !item.Visible));
					if ((!flag || item.Type != ChartSeriesType.Radar) && (!flag2 || item.Type != ChartSeriesType.Polar) && !item.Type.ToString().Contains("Stacking") && flag3)
					{
						result = false;
						item.Compatible = false;
					}
					else
					{
						item.Compatible = true;
					}
				}
			}
		}
		return result;
	}

	public void Refresh(ChartArea area)
	{
		foreach (ChartSeries item in Series)
		{
			if (item.Type == ChartSeriesType.Line)
			{
				item.UpdateRenderer(ChartUpdateFlags.All);
			}
		}
		UpdateArea(area);
	}

	public void UpdateArea(ChartArea area)
	{
		foreach (ChartSeries item in Series)
		{
			foreach (Trendline trendline3 in item.Trendlines)
			{
				if (trendline3.Visible && item.BaseType != 0 && item.BaseType != ChartSeriesBaseType.Circular)
				{
					trendline3.UpdateElements(item);
				}
			}
		}
		if (area.AxesType == ChartAreaAxesType.None)
		{
			return;
		}
		foreach (ChartAxis axis in area.Axes)
		{
			if (axis.RangeType == ChartAxisRangeType.Auto || axis.IsIndexed)
			{
				DoubleRange baseRange = DoubleRange.Empty;
				ChartAxisRangePaddingType pattingType = axis.RangePaddingType;
				foreach (ChartSeries item2 in Series)
				{
					if (item2.ActualXAxis == axis)
					{
						baseRange += item2.Renderer.GetXDataMeasure();
					}
					else if (item2.ActualYAxis == axis)
					{
						if (item2.BaseStackingType == ChartSeriesBaseStackingType.FullStacked)
						{
							pattingType = ChartAxisRangePaddingType.None;
						}
						baseRange += item2.Renderer.GetYDataMeasure();
					}
				}
				foreach (ChartSeries item3 in Series)
				{
					foreach (Trendline trendline4 in item3.Trendlines)
					{
						if (!trendline4.Visible)
						{
							continue;
						}
						if (item3.ActualXAxis == axis)
						{
							baseRange += trendline4.GetXDataMeasure(item3);
						}
						else if (item3.ActualYAxis == axis)
						{
							if (item3.BaseStackingType == ChartSeriesBaseStackingType.FullStacked)
							{
								pattingType = ChartAxisRangePaddingType.None;
							}
							baseRange += trendline4.GetYDataMeasure(item3);
						}
					}
				}
				if (baseRange.IsEmpty)
				{
					baseRange = new DoubleRange(0.0, 1.0);
				}
				axis.SetNiceRange(baseRange, pattingType, area, axis);
			}
			if (axis.BreakRanges.BreaksMode == ChartBreaksMode.Auto)
			{
				axis.BreakRanges.Compute(Series);
			}
		}
	}

	public ChartBaseStylesMap GetStylesMap()
	{
		return m_baseStylesMap;
	}

	public void Dispose()
	{
		m_baseStylesMap = null;
		m_chart = null;
		if (m_firstSeries != null)
		{
			m_firstSeries.Dispose();
			m_firstSeries = null;
		}
		foreach (ChartSeries item in m_series)
		{
			item.Dispose();
		}
		m_series = null;
		m_intexed = null;
	}

	private void OnSeriesChanged(object sender, ChartSeriesCollectionChangedEventArgs e)
	{
		m_minPointsDelta = double.NaN;
		m_firstSeries = null;
	}

	internal double GetStackInfo(IChartArea chartArea, ChartSeries series, int pointIndex, bool isWithMe)
	{
		ChartPoint chartPoint = series.Points[pointIndex];
		double num = (isWithMe ? chartPoint.YValues[series.PointFormats.YIndex] : 0.0);
		double x = chartPoint.X;
		string text = "";
		if (series.ActualXAxis.ValueType == ChartValueType.Category)
		{
			text = series.Points[pointIndex].Category;
		}
		double num2 = series.ActualYAxis.CurrentOrigin;
		bool flag = series.BaseStackingType == ChartSeriesBaseStackingType.FullStacked;
		bool flag2 = false;
		if (series.BaseStackingType != ChartSeriesBaseStackingType.NotStacked)
		{
			int i = 0;
			for (int visibleCount = m_series.VisibleCount; i < visibleCount; i++)
			{
				if (!(!flag2 || flag))
				{
					break;
				}
				ChartSeries chartSeries = m_series.VisibleList[i] as ChartSeries;
				flag2 = ((!flag2) ? (chartSeries == series) : flag2);
				if (chartSeries.Type != series.Type || !(chartSeries.StackingGroup == series.StackingGroup) || chartSeries.BaseStackingType != series.BaseStackingType)
				{
					continue;
				}
				for (int j = 0; j < chartSeries.Points.Count; j++)
				{
					bool num3;
					if (chartSeries.Points[j].IsEmpty || series.ActualXAxis.ValueType == ChartValueType.Category)
					{
						if (!(chartSeries.Points[j].Category == text) || chartSeries.Points[j].X != x)
						{
							continue;
						}
						num3 = !double.IsNaN(chartSeries.Points[j].YValues[series.PointFormats.YIndex]);
					}
					else
					{
						num3 = chartSeries.Points[j].X == x;
					}
					if (!num3)
					{
						continue;
					}
					ChartSeriesType type = series.Type;
					if (!flag || type == ChartSeriesType.StackingArea100 || type == ChartSeriesType.StackingLine100)
					{
						if (!flag2)
						{
							if (flag || (type != ChartSeriesType.StackingBar && type != ChartSeriesType.StackingColumn))
							{
								num += chartSeries.Points[j].YValues[series.PointFormats.YIndex];
							}
							else if ((num <= 0.0 && chartSeries.Points[j].YValues[series.PointFormats.YIndex] <= 0.0 && series.Points[pointIndex].YValues[series.PointFormats.YIndex] < 0.0) || (num >= 0.0 && chartSeries.Points[j].YValues[series.PointFormats.YIndex] >= 0.0 && series.Points[pointIndex].YValues[series.PointFormats.YIndex] > 0.0))
							{
								num += chartSeries.Points[j].YValues[series.PointFormats.YIndex];
							}
						}
						num2 = ((!flag) ? (num2 + chartSeries.Points[j].YValues[series.PointFormats.YIndex]) : (num2 + Math.Abs(chartSeries.Points[j].YValues[series.PointFormats.YIndex])));
					}
					else
					{
						if (!flag2 && ((chartSeries.Points[j].YValues[series.PointFormats.YIndex] > 0.0 && num >= 0.0 && series.Points[pointIndex].YValues[series.PointFormats.YIndex] > 0.0) || (chartSeries.Points[j].YValues[series.PointFormats.YIndex] < 0.0 && num <= 0.0 && series.Points[pointIndex].YValues[series.PointFormats.YIndex] < 0.0)))
						{
							num += chartSeries.Points[j].YValues[series.PointFormats.YIndex];
						}
						num2 += Math.Abs(chartSeries.Points[j].YValues[series.PointFormats.YIndex]);
					}
					break;
				}
			}
			if (flag)
			{
				num = ((num2 == 0.0) ? 0.0 : (chartArea.FullStackMax * num / num2));
			}
		}
		return num;
	}

	internal DoubleRange GetSideBySideInfo(IChartArea chartArea, ChartSeries series)
	{
		int pos = 1;
		int all = 1;
		if (m_chart.ColumnDrawMode != ChartColumnDrawMode.ClusteredMode)
		{
			GetSideBySidePositions(series, out all, out pos);
		}
		double num = chartArea.SeriesParameters.SeriesSpacing;
		double num2 = (double)chartArea.SeriesParameters.PointSpacing - num;
		double value = GetMinPointsDelta(chartArea) * (1.0 - num2);
		double num3 = 1.0 / (double)all;
		double num4 = 0.5 - num3 * (double)(pos - 1);
		return DoubleRange.Scale(DoubleRange.Multiply(series.ActualXAxis.Inversed ? new DoubleRange(num4 - num3, num4) : new DoubleRange(0.0 - num4, 0.0 - num4 + num3), value), 1.0 - num);
	}

	internal DoubleRange GetSideBySideInfo(IChartArea chartArea, ChartSeries series, double seriesWidth)
	{
		int pos = 1;
		int all = 1;
		if (m_chart.ColumnDrawMode != ChartColumnDrawMode.ClusteredMode)
		{
			GetSideBySidePositions(series, out all, out pos);
		}
		double num = seriesWidth * (double)all;
		double num2 = num / (double)all;
		double num3 = 0.5 * num - num2 * (double)(pos - 1);
		return new DoubleRange(0.0 - num3, num2 - num3);
	}

	internal double GetMinPointsDelta(IChartArea chartArea)
	{
		if (double.IsNaN(m_minPointsDelta))
		{
			m_minPointsDelta = double.MaxValue;
			if (chartArea.Chart.Indexed)
			{
				m_minPointsDelta = 1.0;
			}
			else
			{
				foreach (ChartSeries item in m_series)
				{
					if (!item.Visible)
					{
						continue;
					}
					double[] array = new double[item.Points.Count];
					for (int i = 0; i < item.Points.Count; i++)
					{
						array[i] = item.Points[i].X;
					}
					Array.Sort(array);
					for (int j = 1; j < array.Length; j++)
					{
						double num = array[j] - array[j - 1];
						if (num != 0.0)
						{
							m_minPointsDelta = Math.Min(m_minPointsDelta, num);
						}
					}
					if (array.Length == 1 && item.XAxis.Range.Interval < 1.0 && item.XAxis.RangeType == ChartAxisRangeType.Set)
					{
						double interval = item.XAxis.Range.Interval;
						if (interval != 0.0)
						{
							m_minPointsDelta = Math.Min(m_minPointsDelta, interval);
						}
					}
				}
			}
			if (m_minPointsDelta == double.MaxValue)
			{
				m_minPointsDelta = 1.0;
			}
		}
		return m_minPointsDelta;
	}

	private void GetSideBySidePositions(ChartSeries current, out int all, out int pos)
	{
		pos = -1;
		all = 0;
		new Hashtable(5);
		ArrayList arrayList = new ArrayList();
		if (current.BaseType == ChartSeriesBaseType.SideBySide)
		{
			foreach (ChartSeries visible in m_series.VisibleList)
			{
				if (visible.BaseType != ChartSeriesBaseType.SideBySide)
				{
					continue;
				}
				if (visible.BaseStackingType == ChartSeriesBaseStackingType.NotStacked)
				{
					all++;
					if (visible == current)
					{
						pos = all;
					}
					continue;
				}
				bool flag = false;
				foreach (StackInfo item in arrayList)
				{
					if (item.Type == visible.Type && item.GroupName == visible.StackingGroup)
					{
						if (visible == current)
						{
							pos = item.All;
						}
						flag = true;
					}
				}
				if (!flag)
				{
					all++;
					StackInfo value = new StackInfo(visible.Type, all, visible.StackingGroup);
					arrayList.Add(value);
					if (visible == current)
					{
						pos = all;
					}
				}
			}
		}
		if (all < 1)
		{
			all = 1;
			pos = 1;
		}
	}
}
