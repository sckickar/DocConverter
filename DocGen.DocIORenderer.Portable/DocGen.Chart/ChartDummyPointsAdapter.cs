using System;
using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartDummyPointsAdapter : IChartSeriesModel
{
	private const int c_yMaxValue = 400;

	private const int c_yMinValue = 100;

	private const int c_pointsCount = 5;

	private static readonly Random c_random = new Random();

	private ChartSeries m_series;

	public int Count
	{
		get
		{
			if (m_series.SeriesModel.Count <= 0)
			{
				return RandomPoints.Length;
			}
			return m_series.SeriesModel.Count;
		}
	}

	private ChartPoint[] RandomPoints
	{
		get
		{
			if (m_series.ChartModel != null)
			{
				return ChartPredefinedValues.GetPoints(m_series.Type, m_series.ChartModel.Series.IndexOf(m_series));
			}
			return new ChartPoint[0];
		}
	}

	public event ListChangedEventHandler Changed
	{
		add
		{
		}
		remove
		{
		}
	}

	public ChartDummyPointsAdapter(ChartSeries series)
	{
		m_series = series;
	}

	public double GetX(int xIndex)
	{
		if (m_series.SeriesModel.Count > 0)
		{
			return m_series.SeriesModel.GetX(xIndex);
		}
		return RandomPoints[xIndex].X;
	}

	public double[] GetY(int xIndex)
	{
		if (m_series.SeriesModel.Count > 0)
		{
			return m_series.SeriesModel.GetY(xIndex);
		}
		return RandomPoints[xIndex].YValues;
	}

	public bool GetEmpty(int xIndex)
	{
		if (m_series.SeriesModel.Count > 0)
		{
			return m_series.SeriesModel.GetEmpty(xIndex);
		}
		if (xIndex < RandomPoints.Length)
		{
			return xIndex < 0;
		}
		return true;
	}
}
