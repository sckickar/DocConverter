using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[ToolboxItem(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class Trendline
{
	private IList<double> _xValues;

	private double _xMin;

	private double _xMax;

	private ChartDateTimeInterval m_dateTimeInterval;

	private ChartDateTimeIntervalType m_dateTimeIntervalType;

	private IList<double> trendXValues;

	private IList<double> trendYValues;

	private List<double> trendXSegmentValues;

	private List<double> trendYSegmentValues;

	private DoubleRange m_visibleRange = DoubleRange.Empty;

	private bool m_visible = true;

	private Color m_TrendlineColor = Color.Black;

	private DashStyle m_TrendlineStyle;

	private float m_TrendlineWidth = 1f;

	private double m_ForwardForecast;

	private double m_BackwardForecast;

	private double m_Intercept;

	private double m_InterceptX;

	private bool m_IsIntercept;

	private double m_Slope;

	private double m_Period = 2.0;

	private string m_name = "Trendline";

	private double m_PolynomialOrder = 2.0;

	private double[] m_PolynomialSlope;

	private PointF[] m_points;

	private TrendlineType m_trendlinetype;

	private ChartDateTimeDefaults m_chartDateTimeDefaults = new ChartDateTimeDefaults();

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (m_name != value)
			{
				m_name = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public Color Color
	{
		get
		{
			return m_TrendlineColor;
		}
		set
		{
			if (m_TrendlineColor != value)
			{
				m_TrendlineColor = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public DashStyle Style
	{
		get
		{
			return m_TrendlineStyle;
		}
		set
		{
			if (m_TrendlineStyle != value)
			{
				m_TrendlineStyle = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public float Width
	{
		get
		{
			return m_TrendlineWidth;
		}
		set
		{
			if (m_TrendlineWidth != value)
			{
				m_TrendlineWidth = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double BackwardForecast
	{
		get
		{
			return m_BackwardForecast;
		}
		set
		{
			if (m_BackwardForecast != value)
			{
				m_BackwardForecast = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double ForwardForecast
	{
		get
		{
			return m_ForwardForecast;
		}
		set
		{
			if (m_ForwardForecast != value)
			{
				m_ForwardForecast = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	internal double Intercept
	{
		get
		{
			return m_InterceptX;
		}
		set
		{
			if (m_InterceptX != value)
			{
				m_InterceptX = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	internal bool IsIntercept
	{
		get
		{
			return m_IsIntercept;
		}
		set
		{
			m_IsIntercept = value;
		}
	}

	public double PolynomialOrder
	{
		get
		{
			return m_PolynomialOrder;
		}
		set
		{
			if (m_PolynomialOrder != value && m_trendlinetype == TrendlineType.Polynomial)
			{
				m_PolynomialOrder = ((value >= 2.0 && value <= 6.0) ? value : 2.0);
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double Period
	{
		get
		{
			return m_Period;
		}
		set
		{
			if (m_Period != value && m_trendlinetype == TrendlineType.MovingAverage)
			{
				m_Period = ((value > 2.0) ? value : 2.0);
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public TrendlineType Type
	{
		get
		{
			return m_trendlinetype;
		}
		set
		{
			if (m_trendlinetype != value)
			{
				m_trendlinetype = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	internal event EventHandler Changed;

	internal void UpdateElements(ChartSeries m_series)
	{
		_xValues = (from x in getXValues(m_series)
			where !double.IsNaN(x)
			select x).ToList();
		if (_xValues.Count <= 2)
		{
			return;
		}
		double num = _xValues[0];
		double num2 = _xValues[_xValues.Count - 1];
		if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime)
		{
			m_dateTimeInterval = m_series.ActualXAxis.DateTimeRange.DefaultInterval;
		}
		for (int i = 0; i < _xValues.Count - 1; i++)
		{
			if (_xValues[i] < num)
			{
				_xMin = _xValues[i];
			}
			else
			{
				_xMin = num;
			}
			num = _xMin;
			if (_xValues[i] > num2)
			{
				_xMax = _xValues[i];
			}
			else
			{
				_xMax = num2;
			}
			num2 = _xMax;
		}
		CheckTrendlineType(m_series);
	}

	private void CheckTrendlineType(ChartSeries m_series)
	{
		switch (m_trendlinetype)
		{
		case TrendlineType.Linear:
			UpdateTrendSource(m_series);
			CalculateLinearTrendline(m_series);
			break;
		case TrendlineType.Exponential:
			UpdateExponentialTrendSource(m_series);
			CalculateExponentialTrendline(m_series);
			break;
		case TrendlineType.Power:
			UpdatePowerTrendSource(m_series);
			CalculatePowerTrendline(m_series);
			break;
		case TrendlineType.Logarithmic:
			UpdateLogarithmicTrendSource(m_series);
			CalculateLogarithmicTrendline(m_series);
			break;
		case TrendlineType.Polynomial:
			if ((double)m_series.Points.Count > m_PolynomialOrder && m_PolynomialOrder > 1.0 && m_PolynomialOrder <= 6.0)
			{
				UpdateTrendSource(m_series);
				CalculatePolynomialTrendLine(m_series);
			}
			break;
		case TrendlineType.MovingAverage:
			UpdateMovingAverageTrendSource(m_series);
			CalculateMovingAverageTrendline(m_series);
			break;
		}
	}

	private void UpdateTrendSource(ChartSeries m_series)
	{
		trendXValues = new List<double>();
		trendYValues = new List<double>();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			if (!double.IsNaN(m_series.Points[i].X))
			{
				trendXValues.Add(m_series.Points[i].X);
				if ((m_series.Type == ChartSeriesType.Candle || m_series.Type == ChartSeriesType.HiLoOpenClose) && m_series.SeriesModel.GetY(i).Length == 4)
				{
					trendYValues.Add(m_series.SeriesModel.GetY(i)[3]);
				}
				else if ((m_series.Type == ChartSeriesType.HiLo || m_series.Type == ChartSeriesType.RangeArea || m_series.Type == ChartSeriesType.ColumnRange) && m_series.SeriesModel.GetY(i).Length == 2)
				{
					trendYValues.Add(m_series.SeriesModel.GetY(i)[1]);
				}
				else
				{
					trendYValues.Add(m_series.SeriesModel.GetY(i)[0]);
				}
			}
		}
		CalculateSumXAndYValue();
	}

	private void CalculateLinearTrendline(ChartSeries m_series)
	{
		m_points = new PointF[2];
		int count = trendXValues.Count;
		_ = PointF.Empty;
		_ = PointF.Empty;
		if (count <= 0)
		{
			return;
		}
		double linearYValue = GetLinearYValue(trendXValues[0]);
		double linearYValue2 = GetLinearYValue(trendXValues[count - 1]);
		double num2;
		double num4;
		double num;
		double num3;
		if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
		{
			num = trendXValues[0] - m_BackwardForecast;
			num2 = linearYValue;
			num3 = trendXValues[count - 1] + m_ForwardForecast;
			num4 = linearYValue2;
		}
		else
		{
			if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime && m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto)
			{
				DateTime value = DateTime.FromOADate(trendXValues[0]);
				m_dateTimeIntervalType = CalculateIntervalType(DateTime.FromOADate(trendXValues[count - 1]).Subtract(value));
				m_series.ActualXAxis.IntervalType = m_dateTimeIntervalType;
			}
			if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime)
			{
				m_dateTimeIntervalType = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto) ? m_dateTimeIntervalType : m_series.ActualXAxis.IntervalType);
				double num5 = CalculateDateTimeForecastValue(trendXValues[0], 0.0 - m_BackwardForecast, m_dateTimeIntervalType);
				double num6 = CalculateDateTimeForecastValue(trendXValues[count - 1], m_ForwardForecast, m_dateTimeIntervalType);
				num = num5;
				num2 = linearYValue;
				num3 = num6;
				num4 = linearYValue2;
			}
			else
			{
				num = trendXValues[0];
				num3 = trendXValues[count - 1];
				foreach (double trendXValue in trendXValues)
				{
					num = Math.Min(trendXValue, num);
					num3 = Math.Max(trendXValue, num3);
				}
				num -= m_BackwardForecast;
				num2 = ((!IsIntercept) ? GetLinearYValue(num) : Intercept);
				num3 += m_ForwardForecast;
				num4 = GetLinearYValue(num3);
			}
		}
		m_points[0] = new PointF((float)num, (float)num2);
		m_points[1] = new PointF((float)num3, (float)num4);
	}

	private void UpdateExponentialTrendSource(ChartSeries m_series)
	{
		trendXValues = new List<double>();
		trendYValues = new List<double>();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			trendXValues.Add(m_series.Points[i].X);
			if ((m_series.Type == ChartSeriesType.Candle || m_series.Type == ChartSeriesType.HiLoOpenClose) && m_series.SeriesModel.GetY(i).Length == 4)
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[3]));
			}
			else if ((m_series.Type == ChartSeriesType.HiLo || m_series.Type == ChartSeriesType.RangeArea || m_series.Type == ChartSeriesType.ColumnRange) && m_series.SeriesModel.GetY(i).Length == 2)
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[1]));
			}
			else
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[0]));
			}
		}
		CalculateSumXAndYValue();
	}

	private void CalculateExponentialTrendline(ChartSeries m_series)
	{
		m_points = new PointF[3];
		int count = trendXValues.Count;
		if (count > 0)
		{
			CalculateTrendXSegment(trendXValues, m_series);
			trendYSegmentValues.Add(GetExponentialYValue(trendXValues[0] - trendXValues[count - 1] * (m_BackwardForecast / (trendXValues[count - 1] - trendXValues[0]))));
			trendYSegmentValues.Add(GetExponentialYValue(trendXValues[(int)Math.Ceiling((double)count / 2.0) - 1]));
			trendYSegmentValues.Add(GetExponentialYValue(trendXValues[count - 1] + trendXValues[count - 1] * (m_ForwardForecast / (trendXValues[count - 1] - trendXValues[0]))));
			m_points[0] = new PointF((float)trendXSegmentValues[0], (float)trendYSegmentValues[0]);
			m_points[1] = new PointF((float)trendXSegmentValues[1], (float)trendYSegmentValues[1]);
			m_points[2] = new PointF((float)trendXSegmentValues[2], (float)trendYSegmentValues[2]);
		}
	}

	private void UpdatePowerTrendSource(ChartSeries m_series)
	{
		trendXValues = new List<double>();
		trendYValues = new List<double>();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			trendXValues.Add(Math.Log(m_series.Points[i].X));
			if ((m_series.Type == ChartSeriesType.Candle || m_series.Type == ChartSeriesType.HiLoOpenClose) && m_series.SeriesModel.GetY(i).Length == 4)
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[3]));
			}
			else if ((m_series.Type == ChartSeriesType.HiLo || m_series.Type == ChartSeriesType.RangeArea || m_series.Type == ChartSeriesType.ColumnRange) && m_series.SeriesModel.GetY(i).Length == 2)
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[1]));
			}
			else
			{
				trendYValues.Add(Math.Log(m_series.SeriesModel.GetY(i)[0]));
			}
		}
		CalculateSumXAndYValue();
	}

	private void CalculatePowerTrendline(ChartSeries m_series)
	{
		int count = trendXValues.Count;
		m_points = new PointF[3];
		if (count > 0)
		{
			CalculateTrendXSegment(trendXValues, m_series);
			trendYSegmentValues.Add(GetPowerYValue(trendXValues[0]) - (double)(count - 1) * (m_BackwardForecast / (trendXValues[count - 1] - trendXValues[0])));
			trendYSegmentValues.Add(GetPowerYValue(trendXValues[(int)Math.Ceiling((double)count / 2.0) - 1]));
			trendYSegmentValues.Add(GetPowerYValue(trendXValues[count - 1]) + (double)(count - 1) * (m_ForwardForecast / (trendXValues[count - 1] - trendXValues[0])));
			m_points[0] = new PointF((float)trendXSegmentValues[0], (float)trendYSegmentValues[0]);
			m_points[1] = new PointF((float)trendXSegmentValues[1], (float)trendYSegmentValues[1]);
			m_points[2] = new PointF((float)trendXSegmentValues[2], (float)trendYSegmentValues[2]);
		}
	}

	private void UpdateLogarithmicTrendSource(ChartSeries m_series)
	{
		trendXValues = new List<double>();
		trendYValues = new List<double>();
		m_points = new PointF[3];
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			double x = m_series.Points[i].X;
			trendXValues.Add((x == 0.0) ? 1.0 : Math.Log(x));
			if ((m_series.Type == ChartSeriesType.Candle || m_series.Type == ChartSeriesType.HiLoOpenClose) && m_series.SeriesModel.GetY(i).Length == 4)
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[3]);
			}
			else if ((m_series.Type == ChartSeriesType.HiLo || m_series.Type == ChartSeriesType.RangeArea || m_series.Type == ChartSeriesType.ColumnRange) && m_series.SeriesModel.GetY(i).Length == 2)
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[1]);
			}
			else
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[0]);
			}
		}
		CalculateSumXAndYValue();
	}

	private void CalculateLogarithmicTrendline(ChartSeries m_series)
	{
		int count = trendXValues.Count;
		if (count > 0)
		{
			CalculateTrendXSegment(trendXValues, m_series);
			trendYSegmentValues.Add(GetLogarithmicYValue(Math.Log(trendXValues[0])) - m_BackwardForecast);
			trendYSegmentValues.Add(GetLogarithmicYValue(Math.Log(trendXValues[(int)Math.Ceiling((double)count / 2.0) - 1])));
			trendYSegmentValues.Add(GetLogarithmicYValue(Math.Log(trendXValues[count - 1])) + m_ForwardForecast);
			m_points[0] = new PointF((float)trendXSegmentValues[0], (float)trendYSegmentValues[0]);
			m_points[1] = new PointF((float)trendXSegmentValues[1], (float)trendYSegmentValues[1]);
			m_points[2] = new PointF((float)trendXSegmentValues[2], (float)trendYSegmentValues[2]);
		}
	}

	private void CalculatePolynomialTrendLine(ChartSeries m_series)
	{
		double polynomialOrder = m_PolynomialOrder;
		m_PolynomialSlope = new double[(int)polynomialOrder + 1];
		int count = trendXValues.Count;
		for (int i = 0; i < count; i++)
		{
			double num = trendXValues[i];
			double num2 = trendYValues[i];
			if (!double.IsNaN(num) && !double.IsNaN(num2))
			{
				for (int j = 0; (double)j <= polynomialOrder; j++)
				{
					m_PolynomialSlope[j] += Math.Pow(num, j) * num2;
				}
			}
		}
		double[] array = new double[(int)(1.0 + 2.0 * polynomialOrder)];
		double[,] array2 = new double[(int)(polynomialOrder + 1.0), (int)(polynomialOrder + 1.0)];
		int num3 = 0;
		for (int k = 0; k < trendXValues.Count; k++)
		{
			double num4 = 1.0;
			double num5 = trendXValues[k];
			if (!double.IsNaN(num5) && !double.IsNaN(trendYValues[k]))
			{
				for (int l = 0; l < array.Length; l++)
				{
					array[l] += num4;
					num4 *= num5;
					num3++;
				}
			}
		}
		for (int m = 0; (double)m <= polynomialOrder; m++)
		{
			for (int n = 0; (double)n <= polynomialOrder; n++)
			{
				array2[m, n] = array[m + n];
			}
		}
		if (!GaussJordanEliminiation(array2, m_PolynomialSlope))
		{
			m_PolynomialSlope = null;
		}
		CreatePolynomialSegments(m_series);
	}

	private void CreatePolynomialSegments(ChartSeries m_series)
	{
		trendXSegmentValues = new List<double>();
		trendYSegmentValues = new List<double>();
		double num = trendXValues.Count;
		double num2 = 1.0;
		if (m_PolynomialSlope == null)
		{
			return;
		}
		for (int i = 1; i <= m_PolynomialSlope.Length; i++)
		{
			_ = m_series.ActualXAxis;
			if (i == 1)
			{
				if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
				{
					trendXSegmentValues.Add(trendXValues[i - 1] - m_BackwardForecast);
				}
				else
				{
					if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime && m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto)
					{
						DateTime value = DateTime.FromOADate(_xMin);
						m_dateTimeIntervalType = CalculateIntervalType(DateTime.FromOADate(_xMax).Subtract(value));
					}
					if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime)
					{
						m_dateTimeIntervalType = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto) ? m_dateTimeIntervalType : m_series.ActualXAxis.IntervalType);
						double item = CalculateDateTimeForecastValue(_xMin, 0.0 - m_BackwardForecast, m_dateTimeIntervalType);
						trendXSegmentValues.Add(item);
					}
					else
					{
						trendXSegmentValues.Add(trendXValues[0] - m_BackwardForecast);
					}
				}
				trendYSegmentValues.Add(GetPolynomialYValue(m_PolynomialSlope, trendXSegmentValues[trendXSegmentValues.Count - 1]));
				continue;
			}
			if (i == m_PolynomialSlope.Length)
			{
				if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
				{
					trendXSegmentValues.Add(trendXValues[trendXValues.Count - 1] + m_ForwardForecast);
				}
				else
				{
					if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime && m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto)
					{
						DateTime value2 = DateTime.FromOADate(_xMin);
						m_dateTimeIntervalType = CalculateIntervalType(DateTime.FromOADate(_xMax).Subtract(value2));
					}
					if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime)
					{
						m_dateTimeIntervalType = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto) ? m_dateTimeIntervalType : m_series.ActualXAxis.IntervalType);
						double item2 = CalculateDateTimeForecastValue(_xMax, ForwardForecast, m_dateTimeIntervalType);
						trendXSegmentValues.Add(item2);
					}
					else
					{
						trendXSegmentValues.Add(trendXValues[trendXValues.Count - 1] + m_ForwardForecast);
					}
				}
				trendYSegmentValues.Add(GetPolynomialYValue(m_PolynomialSlope, trendXSegmentValues[trendXSegmentValues.Count - 1]));
				continue;
			}
			num2 += (num + (num - 1.0) * (m_ForwardForecast / (_xMax - _xMin))) / (double)m_PolynomialSlope.Length;
			if (num == num2)
			{
				continue;
			}
			if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
			{
				trendXSegmentValues.Add(trendXValues[(int)(Math.Ceiling(num2) - 1.0)]);
			}
			else
			{
				int num3 = (int)Math.Ceiling(num2);
				if (num > (double)num3)
				{
					trendXSegmentValues.Add(trendXValues[num3 - 1]);
				}
				else
				{
					trendYSegmentValues.Add(trendXValues[trendXValues.Count - 1] + num2);
				}
			}
			trendYSegmentValues.Add(GetPolynomialYValue(m_PolynomialSlope, trendXSegmentValues[trendXSegmentValues.Count - 1]));
		}
		m_points = new PointF[trendXSegmentValues.Count];
		for (int j = 0; j < trendXSegmentValues.Count; j++)
		{
			m_points[j] = new PointF((float)trendXSegmentValues[j], (float)trendYSegmentValues[j]);
		}
	}

	private void UpdateMovingAverageTrendSource(ChartSeries m_series)
	{
		trendXValues = new List<double>();
		trendYValues = new List<double>();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
			{
				trendXValues.Add(i);
			}
			else
			{
				trendXValues.Add(m_series.SeriesModel.GetX(i));
			}
			if ((m_series.Type == ChartSeriesType.Candle || m_series.Type == ChartSeriesType.HiLoOpenClose) && m_series.SeriesModel.GetY(i).Length == 4)
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[3]);
			}
			else if ((m_series.Type == ChartSeriesType.HiLo || m_series.Type == ChartSeriesType.RangeArea || m_series.Type == ChartSeriesType.ColumnRange) && m_series.SeriesModel.GetY(i).Length == 2)
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[1]);
			}
			else
			{
				trendYValues.Add(m_series.SeriesModel.GetY(i)[0]);
			}
		}
		CalculateSumXAndYValue();
	}

	private void CalculateMovingAverageTrendline(ChartSeries m_series)
	{
		trendXSegmentValues = new List<double>();
		trendYSegmentValues = new List<double>();
		int count = m_series.Points.Count;
		double num = ((m_Period >= (double)count) ? ((double)(count - 1)) : m_Period);
		num = ((num < 2.0) ? 2.0 : num);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		double num6 = 0.0;
		for (num2 = 0; num2 < count - 1; num2++)
		{
			num6 = (num4 = (num5 = 0));
			num3 = num2;
			while ((double)num4 < num)
			{
				num4++;
				if (num3 > trendYValues.Count - 1)
				{
					num5++;
				}
				else
				{
					num6 += trendYValues[num3];
				}
				num3++;
			}
			if (num5 == 0)
			{
				num6 = ((num - (double)num5 <= 0.0) ? 0.0 : (num6 / (num - (double)num5)));
				trendYSegmentValues.Add(num6);
			}
		}
		m_points = new PointF[trendYSegmentValues.Count];
		for (num2 = 0; num2 < trendYSegmentValues.Count; num2++)
		{
			m_points[num2] = new PointF((float)trendXValues[(int)num - 1 + num2], (float)trendYSegmentValues[num2]);
		}
	}

	private static bool GaussJordanEliminiation(double[,] a, double[] b)
	{
		int length = a.GetLength(0);
		int[] array = new int[length];
		int[] array2 = new int[length];
		int[] array3 = new int[length];
		for (int i = 0; i < length; i++)
		{
			array3[i] = 0;
		}
		for (int j = 0; j < length; j++)
		{
			double num = 0.0;
			int num2 = 0;
			int num3 = 0;
			for (int k = 0; k < length; k++)
			{
				if (array3[k] == 1)
				{
					continue;
				}
				for (int l = 0; l < length; l++)
				{
					if (array3[l] == 0 && Math.Abs(a[k, l]) >= num)
					{
						num = Math.Abs(a[k, l]);
						num2 = k;
						num3 = l;
					}
				}
			}
			array3[num3]++;
			if (num2 != num3)
			{
				for (int m = 0; m < length; m++)
				{
					double num4 = a[num2, m];
					a[num2, m] = a[num3, m];
					a[num3, m] = num4;
				}
				double num5 = b[num2];
				b[num2] = b[num3];
				b[num3] = num5;
			}
			array2[j] = num2;
			array[j] = num3;
			if (a[num3, num3] == 0.0)
			{
				return false;
			}
			double num6 = 1.0 / a[num3, num3];
			a[num3, num3] = 1.0;
			for (int n = 0; n < length; n++)
			{
				a[num3, n] *= num6;
			}
			b[num3] *= num6;
			for (int num7 = 0; num7 < length; num7++)
			{
				if (num7 != num3)
				{
					double num8 = a[num7, num3];
					a[num7, num3] = 0.0;
					for (int num9 = 0; num9 < length; num9++)
					{
						a[num7, num9] -= a[num3, num9] * num8;
					}
					b[num7] -= b[num3] * num8;
				}
			}
		}
		for (int num10 = length - 1; num10 >= 0; num10--)
		{
			if (array2[num10] != array[num10])
			{
				for (int num11 = 0; num11 < length; num11++)
				{
					double num12 = a[num11, array2[num10]];
					a[num11, array2[num10]] = a[num11, array[num10]];
					a[num11, array[num10]] = num12;
				}
			}
		}
		return true;
	}

	private ChartDateTimeIntervalType CalculateIntervalType(TimeSpan diff)
	{
		if (diff.Days > m_chartDateTimeDefaults.GetDaysInYear())
		{
			return ChartDateTimeIntervalType.Years;
		}
		if (diff.Days > m_chartDateTimeDefaults.GetDaysInMonth())
		{
			return ChartDateTimeIntervalType.Months;
		}
		if (diff.Days > m_chartDateTimeDefaults.GetDaysInWeek() * 2)
		{
			return ChartDateTimeIntervalType.Weeks;
		}
		if (diff.Days > 1)
		{
			return ChartDateTimeIntervalType.Days;
		}
		if (diff.TotalHours > 1.0)
		{
			return ChartDateTimeIntervalType.Hours;
		}
		if (diff.TotalMinutes > 1.0)
		{
			return ChartDateTimeIntervalType.Minutes;
		}
		if (diff.TotalSeconds > 1.0)
		{
			return ChartDateTimeIntervalType.Seconds;
		}
		return ChartDateTimeIntervalType.MilliSeconds;
	}

	private double CalculateDateTimeForecastValue(double _xMin, double forecastValue, ChartDateTimeIntervalType type)
	{
		DateTime date = DateTime.FromOADate(_xMin);
		return Convert.ToDateTime(m_dateTimeInterval.IncreaseInterval(date, forecastValue, type)).ToOADate();
	}

	internal void TrendlineDraw(ChartRenderArgs2D args, ChartSeries m_series)
	{
		if (m_trendlinetype == TrendlineType.Linear || m_trendlinetype == TrendlineType.MovingAverage)
		{
			CreateLine(args, m_series);
		}
		else
		{
			CreateSpline(args, m_series);
		}
	}

	private void CreateLine(ChartRenderArgs2D args, ChartSeries m_series)
	{
		ChartStyleInfo style = m_series.Style;
		Pen pen = new Pen(style.GdipPen.Color, style.GdipPen.Width);
		PointF empty = PointF.Empty;
		PointF empty2 = PointF.Empty;
		for (int i = 0; i < m_points.Length - 1; i++)
		{
			empty = args.GetPoint(m_points[i].X, m_points[i].Y);
			empty2 = args.GetPoint(m_points[i + 1].X, m_points[i + 1].Y);
			using Pen pen2 = GetPen();
			pen.LineJoin = LineJoin.Round;
			pen.StartCap = LineCap.Round;
			pen.EndCap = LineCap.Round;
			args.Graph.SmoothingMode = SmoothingMode.AntiAlias;
			args.Graph.DrawLine(pen2, empty.X, empty.Y, empty2.X, empty2.Y);
		}
	}

	private Pen GetPen()
	{
		return new Pen(m_TrendlineColor, m_TrendlineWidth)
		{
			DashStyle = m_TrendlineStyle
		};
	}

	private void CreateSpline(ChartRenderArgs2D args, ChartSeries m_series)
	{
		int num = -1;
		double[] ys = null;
		NaturalSpline(trendXSegmentValues, trendYSegmentValues, out ys);
		for (int i = 0; i < trendXSegmentValues.Count; i++)
		{
			num = i + 1;
			PointF point = new PointF((float)trendXSegmentValues[i], (float)trendYSegmentValues[i]);
			if (num < trendXSegmentValues.Count)
			{
				PointF point2 = new PointF((float)trendXSegmentValues[num], (float)trendYSegmentValues[num]);
				GetBezierControlPoints(point, point2, ys[i], ys[num], out var controlPoint, out var controlPoint2);
				_ = m_series.Style;
				using Pen pen = GetPen();
				GraphicsPath graphicsPath = new GraphicsPath();
				PointF point3 = args.GetPoint(point.X, point.Y);
				PointF point4 = args.GetPoint(point2.X, point2.Y);
				PointF point5 = args.GetPoint(controlPoint.X, controlPoint.Y);
				PointF point6 = args.GetPoint(controlPoint2.X, controlPoint2.Y);
				graphicsPath.AddBezier(point3, point5, point6, point4);
				args.Graph.DrawPath(pen, graphicsPath);
			}
		}
	}

	private void NaturalSpline(List<double> xValues, List<double> yValues, out double[] ys2)
	{
		int count = xValues.Count;
		ys2 = new double[count];
		double num = 6.0;
		double[] array = new double[count];
		ys2[0] = (array[0] = 0.0);
		ys2[count - 1] = 0.0;
		for (int i = 1; i < count - 1; i++)
		{
			double num2 = xValues[i] - xValues[i - 1];
			double num3 = xValues[i + 1] - xValues[i - 1];
			double num4 = xValues[i + 1] - xValues[i];
			double num5 = yValues[i + 1] - yValues[i];
			double num6 = yValues[i] - yValues[i - 1];
			if (xValues[i] == xValues[i - 1] || xValues[i] == xValues[i + 1])
			{
				ys2[i] = 0.0;
				array[i] = 0.0;
			}
			else
			{
				double num7 = 1.0 / (num2 * ys2[i - 1] + 2.0 * num3);
				ys2[i] = (0.0 - num7) * num4;
				array[i] = num7 * (num * (num5 / num4 - num6 / num2) - num2 * array[i - 1]);
			}
		}
		for (int num8 = count - 2; num8 >= 0; num8--)
		{
			ys2[num8] = ys2[num8] * ys2[num8 + 1] + array[num8];
		}
	}

	private void GetBezierControlPoints(PointF point1, PointF point2, double ys1, double ys2, out PointF controlPoint1, out PointF controlPoint2)
	{
		double num = point2.X - point1.X;
		num *= num;
		double num2 = 2f * point1.X + point2.X;
		double num3 = point1.X + 2f * point2.X;
		double num4 = 2f * point1.Y + point2.Y;
		double num5 = point1.Y + 2f * point2.Y;
		double num6 = 1.0 / 3.0 * (num4 - 1.0 / 3.0 * num * (ys1 + 0.5 * ys2));
		double num7 = 1.0 / 3.0 * (num5 - 1.0 / 3.0 * num * (0.5 * ys1 + ys2));
		controlPoint1 = new PointF((float)(num2 * (1.0 / 3.0)), (float)num6);
		controlPoint2 = new PointF((float)(num3 * (1.0 / 3.0)), (float)num7);
	}

	private void CalculateTrendXSegment(IList<double> x, ChartSeries m_series)
	{
		int count = x.Count;
		trendXSegmentValues = new List<double>();
		trendYSegmentValues = new List<double>();
		if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
		{
			trendXSegmentValues.Add(trendXValues[0] - m_BackwardForecast);
			trendXSegmentValues.Add(trendXValues[(int)Math.Round((double)count / 2.0) - 1]);
			trendXSegmentValues.Add(trendXValues[count - 1] + m_ForwardForecast);
			return;
		}
		if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime && m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto)
		{
			DateTime value = DateTime.FromOADate(trendXValues[0]);
			m_dateTimeIntervalType = CalculateIntervalType(DateTime.FromOADate(trendXValues[count - 1]).Subtract(value));
		}
		if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime)
		{
			m_dateTimeIntervalType = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Auto) ? m_dateTimeIntervalType : m_series.ActualXAxis.IntervalType);
			double item = CalculateDateTimeForecastValue(trendXValues[0], 0.0 - m_BackwardForecast, m_dateTimeIntervalType);
			trendXSegmentValues.Add(item);
			trendXSegmentValues.Add(trendXValues[(int)Math.Round((double)count / 2.0)]);
			double item2 = CalculateDateTimeForecastValue(trendXValues[count - 1], m_ForwardForecast, m_dateTimeIntervalType);
			trendXSegmentValues.Add(item2);
		}
		else
		{
			trendXSegmentValues.Add(trendXValues[0] - m_BackwardForecast);
			trendXSegmentValues.Add(trendXValues[(int)Math.Round((double)count / 2.0) - 1]);
			trendXSegmentValues.Add(trendXValues[count - 1] + m_ForwardForecast);
		}
	}

	private void CalculateSumXAndYValue()
	{
		int count = trendXValues.Count;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = m_trendlinetype == TrendlineType.Logarithmic || m_trendlinetype == TrendlineType.Power;
		for (int i = 0; i < count; i++)
		{
			double num5 = trendXValues[i];
			num += num5;
			num2 += Math.Pow(num5, 2.0);
			num3 += ((!double.IsNaN(trendYValues[i])) ? trendYValues[i] : 0.0);
			if (!double.IsNaN(trendYValues[i]))
			{
				num4 += num5 * trendYValues[i];
			}
			if (flag)
			{
				trendXValues[i] = Math.Exp(trendXValues[i]);
			}
		}
		m_Slope = (num4 * (double)count - num * num3) / (num2 * (double)count - num * num);
		if (m_trendlinetype == TrendlineType.Exponential || m_trendlinetype == TrendlineType.Power)
		{
			m_Intercept = Math.Exp((num3 - m_Slope * num) / (double)count);
		}
		else
		{
			m_Intercept = (num3 - m_Slope * num) / (double)count;
		}
	}

	private double GetLinearYValue(double xValue)
	{
		return m_Intercept + m_Slope * xValue;
	}

	private double GetExponentialYValue(double xValue)
	{
		return m_Intercept * Math.Exp(m_Slope * xValue);
	}

	private double GetPolynomialYValue(double[] a, double x)
	{
		double num = 0.0;
		for (int i = 0; i < a.Length; i++)
		{
			num += a[i] * Math.Pow(x, i);
		}
		return num;
	}

	private double GetPowerYValue(double xValue)
	{
		return m_Intercept * Math.Pow(xValue, m_Slope);
	}

	private double GetLogarithmicYValue(double xValue)
	{
		return m_Intercept + m_Slope * xValue;
	}

	private List<double> getXValues(ChartSeries m_series)
	{
		List<double> list = new List<double>();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			if (m_series.ActualXAxis.ValueType == ChartValueType.Category || m_series.ActualXAxis.IsIndexed)
			{
				list.Add(i);
			}
			else
			{
				list.Add(m_series.SeriesModel.GetX(i));
			}
		}
		return list;
	}

	internal DoubleRange GetXDataMeasure(ChartSeries m_series)
	{
		if (((m_series.ActualXAxis.ExcludeInvisibleSeriesRange && m_series.Visible) || (!m_series.ActualXAxis.ExcludeInvisibleSeriesRange && m_points != null)) && m_points.Length != 0)
		{
			double num = double.MinValue;
			double num2 = double.MaxValue;
			for (int i = 0; i < m_points.Length; i++)
			{
				double num3 = m_points[i].X;
				if (num3 > num)
				{
					num = num3;
				}
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
			DoubleRange result = new DoubleRange(num2, num);
			if (double.IsInfinity(result.Delta))
			{
				return DoubleRange.Empty;
			}
			return result;
		}
		return DoubleRange.Empty;
	}

	internal DoubleRange GetYDataMeasure(ChartSeries m_series)
	{
		if (((m_series.ActualYAxis.ExcludeInvisibleSeriesRange && m_series.Visible) || (!m_series.ActualYAxis.ExcludeInvisibleSeriesRange && m_points != null)) && m_points.Length != 0)
		{
			double num = double.MinValue;
			double num2 = double.MaxValue;
			for (int i = 0; i < m_points.Length; i++)
			{
				double num3 = m_points[i].Y;
				if (num3 > num)
				{
					num = num3;
				}
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
			DoubleRange doubleRange = new DoubleRange(num2, num);
			if (double.IsInfinity(doubleRange.Delta))
			{
				return DoubleRange.Empty;
			}
			if (m_series.OriginDependent)
			{
				return doubleRange + m_series.ActualYAxis.CurrentOrigin;
			}
			return doubleRange;
		}
		return DoubleRange.Empty;
	}

	private void RaiseChanged(object sender, EventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(sender, args);
		}
	}
}
