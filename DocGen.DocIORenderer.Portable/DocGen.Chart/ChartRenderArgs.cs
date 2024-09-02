using System;
using DocGen.Drawing;

namespace DocGen.Chart;

internal abstract class ChartRenderArgs
{
	private ChartAxis m_xAxis;

	private ChartAxis m_yAxis;

	private DoubleRange m_xRange;

	private DoubleRange m_yRange;

	private bool m_isInvertedAxes;

	private ChartSeries m_series;

	private IChartAreaHost m_chart;

	private int m_seriesIndex;

	private int m_placement;

	private RectangleF m_bounds;

	private DoubleRange m_sideBySideInfo = DoubleRange.Empty;

	public ChartAxis ActualXAxis
	{
		get
		{
			return m_xAxis;
		}
		set
		{
			m_xAxis = value;
		}
	}

	public ChartAxis ActualYAxis
	{
		get
		{
			return m_yAxis;
		}
		set
		{
			m_yAxis = value;
		}
	}

	public DoubleRange XRange => m_xRange;

	public DoubleRange YRange => m_yRange;

	public ChartSeries Series => m_series;

	public IChartAreaHost Chart
	{
		get
		{
			return m_chart;
		}
		set
		{
			m_chart = value;
		}
	}

	public int SeriesIndex => m_seriesIndex;

	public bool IsInvertedAxes
	{
		get
		{
			return m_isInvertedAxes;
		}
		set
		{
			m_isInvertedAxes = value;
		}
	}

	public int Placement
	{
		get
		{
			return m_placement;
		}
		set
		{
			m_placement = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public DoubleRange SideBySideInfo
	{
		get
		{
			if (m_sideBySideInfo.IsEmpty)
			{
				m_sideBySideInfo = m_series.ChartModel.GetSideBySideInfo(m_chart.GetChartArea(), m_series);
			}
			return m_sideBySideInfo;
		}
	}

	public ChartRenderArgs(IChartAreaHost chart, ChartSeries series)
	{
		m_chart = chart;
		m_series = series;
		m_seriesIndex = m_chart.Series.IndexOf(series);
		m_isInvertedAxes = series.RequireInvertedAxes;
		if (m_isInvertedAxes)
		{
			m_yAxis = m_chart.GetChartArea().GetXAxis(series);
			m_xAxis = m_chart.GetChartArea().GetYAxis(series);
		}
		else
		{
			m_xAxis = m_chart.GetChartArea().GetXAxis(series);
			m_yAxis = m_chart.GetChartArea().GetYAxis(series);
		}
		if (m_xAxis.ValueType == ChartValueType.Logarithmic)
		{
			m_xRange = new DoubleRange(Math.Pow(m_xAxis.LogBase, m_xAxis.VisibleRange.min), Math.Pow(m_xAxis.LogBase, m_xAxis.VisibleRange.max));
		}
		else
		{
			m_xRange = new DoubleRange(m_xAxis.VisibleRange.min, m_xAxis.VisibleRange.max);
		}
		if (m_yAxis.ValueType == ChartValueType.Logarithmic)
		{
			m_yRange = new DoubleRange(Math.Pow(m_yAxis.LogBase, m_yAxis.VisibleRange.min), Math.Pow(m_yAxis.LogBase, m_yAxis.VisibleRange.max));
		}
		else
		{
			m_yRange = new DoubleRange(m_yAxis.VisibleRange.min, m_yAxis.VisibleRange.max);
		}
	}

	public bool IsVisible(double x, double y)
	{
		if (m_xRange.Inside(x))
		{
			return m_yRange.Inside(y);
		}
		return false;
	}

	public bool IsVisible(DoubleRange xRange, DoubleRange yRange)
	{
		if (m_xRange.IsIntersects(xRange))
		{
			return m_yRange.IsIntersects(yRange);
		}
		return false;
	}

	public virtual PointF GetPoint(double x, double y)
	{
		float coordinateFromValue = m_xAxis.GetCoordinateFromValue(x);
		float coordinateFromValue2 = m_yAxis.GetCoordinateFromValue(y);
		if (!m_isInvertedAxes)
		{
			return new PointF(coordinateFromValue, coordinateFromValue2);
		}
		return new PointF(coordinateFromValue2, coordinateFromValue);
	}

	public RectangleF GetRectangle(double x1, double y1, double x2, double y2)
	{
		PointF point = GetPoint(x1, y1);
		PointF point2 = GetPoint(x2, y2);
		return ChartMath.CorrectRect(point.X, point.Y, point2.X, point2.Y);
	}
}
