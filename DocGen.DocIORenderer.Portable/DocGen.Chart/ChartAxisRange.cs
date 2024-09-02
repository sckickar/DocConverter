using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartAxisRange
{
	private ChartAxis m_axis;

	private DoubleRange m_visibleRange = DoubleRange.Empty;

	private ArrayList m_segments = new ArrayList();

	private double m_breakAmount = 0.2;

	private ArrayList m_ranges = new ArrayList();

	private double m_visibleSum;

	private bool m_isEmpty = true;

	private bool m_needUpdate;

	private ChartBreaksMode m_mode = ChartBreaksMode.Manual;

	public bool IsEmpty
	{
		get
		{
			if (m_needUpdate)
			{
				Recalculate();
				m_needUpdate = false;
			}
			return m_isEmpty;
		}
	}

	public ChartBreaksMode BreaksMode
	{
		get
		{
			return m_mode;
		}
		set
		{
			if (m_mode != value)
			{
				m_mode = value;
				m_needUpdate = true;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public double BreakAmount
	{
		get
		{
			return m_breakAmount;
		}
		set
		{
			value = ChartMath.MinMax(value, 0.0, 1.0);
			if (m_breakAmount != value)
			{
				m_breakAmount = value;
				m_needUpdate = true;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	internal IList Segments => m_segments;

	internal IList Breaks => m_ranges;

	public event EventHandler Changed;

	internal ChartAxisRange(ChartAxis axis)
	{
		m_axis = axis;
		m_axis.VisibleRangeChanged += OnVisibleRangeChanged;
		m_visibleRange = new DoubleRange(m_axis.VisibleRange.min, m_axis.VisibleRange.max);
	}

	public void Compute(ChartSeriesCollection series)
	{
		bool flag = true;
		ArrayList arrayList = new ArrayList();
		foreach (ChartSeries item in series)
		{
			if (!item.Compatible || m_axis != item.ActualYAxis)
			{
				continue;
			}
			if (item.BaseStackingType != ChartSeriesBaseStackingType.NotStacked)
			{
				flag = false;
			}
			foreach (ChartPoint point in item.Points)
			{
				arrayList.Add(point.YValues[0]);
			}
			if (item.OriginDependent)
			{
				arrayList.Add(m_axis.CustomOrigin ? m_axis.Origin : 0.0);
			}
		}
		arrayList.Sort();
		if (flag && arrayList.Count > 0)
		{
			m_segments.Clear();
			DoubleRange doubleRange = new DoubleRange(m_axis.Range.min, m_axis.Range.max);
			int num = 0;
			double num2 = 1.0 / (double)arrayList.Count;
			double start = (double)arrayList[0];
			double num3 = 0.0;
			double num4 = double.NaN;
			foreach (double item2 in arrayList)
			{
				if (!double.IsNaN(num4) && item2 - num4 > m_breakAmount * doubleRange.Delta)
				{
					ChartAxisSegment chartAxisSegment = new ChartAxisSegment();
					chartAxisSegment.Range = new DoubleRange(start, num4);
					if (num == 1)
					{
						chartAxisSegment.Range = DoubleRange.Inflate(chartAxisSegment.Range, m_axis.Range.interval / 2.0);
					}
					chartAxisSegment.Length = (double)num * num2;
					chartAxisSegment.Interval = chartAxisSegment.Range.Delta / ((double)m_axis.DesiredIntervals * chartAxisSegment.Length);
					num3 += chartAxisSegment.Length;
					m_segments.Add(chartAxisSegment);
					num = 0;
					start = item2;
				}
				num4 = item2;
				num++;
			}
			if (num3 < 1.0)
			{
				ChartAxisSegment chartAxisSegment2 = new ChartAxisSegment();
				chartAxisSegment2.Range = new DoubleRange(start, (double)arrayList[arrayList.Count - 1]);
				if (num == 1)
				{
					chartAxisSegment2.Range = DoubleRange.Inflate(chartAxisSegment2.Range, m_axis.Range.interval / 2.0);
				}
				chartAxisSegment2.Length = 1.0 - num3;
				chartAxisSegment2.Interval = chartAxisSegment2.Range.Delta / ((double)m_axis.DesiredIntervals * chartAxisSegment2.Length);
				m_segments.Add(chartAxisSegment2);
			}
			foreach (ChartAxisSegment segment in m_segments)
			{
				DoubleRange range = segment.Range;
				double interval = Math.Min(segment.Interval, m_axis.Range.interval);
				m_axis.CalculateNiceRange(ref range, ref interval, m_axis.RangePaddingType);
				segment.Interval = interval;
				segment.Range = range;
			}
		}
		m_needUpdate = true;
	}

	public void Union(DoubleRange range)
	{
		bool flag = true;
		int num = 0;
		while (num < m_ranges.Count)
		{
			DoubleRange doubleRange = (DoubleRange)m_ranges[num];
			if (range.Inside(doubleRange))
			{
				m_ranges.RemoveAt(num);
				continue;
			}
			if (doubleRange.Inside(range))
			{
				flag = false;
				break;
			}
			if (doubleRange.IsIntersects(range))
			{
				range += doubleRange;
				m_ranges.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		if (flag)
		{
			m_ranges.Add(range);
		}
		m_needUpdate = true;
		OnChanged(EventArgs.Empty);
	}

	public void Exclude(DoubleRange range)
	{
		for (int i = 0; i < m_ranges.Count; i++)
		{
			DoubleRange range2 = (DoubleRange)m_ranges[i];
			if (range.Inside(range2))
			{
				m_ranges.RemoveAt(i);
				i--;
			}
			else if (range2.Inside(range))
			{
				m_ranges[i] = new DoubleRange(range2.Start, range.Start);
				m_ranges.Insert(i + 1, new DoubleRange(range.End, range2.End));
			}
			else if (range2.IsIntersects(range))
			{
				if (range2.Start > range.Start && range2.End > range.Start)
				{
					m_ranges[i] = new DoubleRange(range.End, range2.End);
				}
				else
				{
					m_ranges[i] = new DoubleRange(range2.Start, range.Start);
				}
			}
		}
		m_needUpdate = true;
		OnChanged(EventArgs.Empty);
	}

	public void Clear()
	{
		m_ranges.Clear();
		m_needUpdate = true;
		OnChanged(EventArgs.Empty);
	}

	public bool IsVisible(double value)
	{
		if (!m_isEmpty)
		{
			if (m_mode == ChartBreaksMode.Auto)
			{
				foreach (ChartAxisSegment segment in m_segments)
				{
					if (segment.Range.Inside(value))
					{
						return true;
					}
				}
				return false;
			}
			if (m_mode == ChartBreaksMode.Manual)
			{
				foreach (DoubleRange range in m_ranges)
				{
					if (range.Start < value && range.End >= value)
					{
						return false;
					}
				}
				return true;
			}
		}
		return m_visibleRange.Inside(value);
	}

	public double ValueToCoeficient(double value)
	{
		double result = 0.0;
		if (!m_isEmpty)
		{
			switch (m_mode)
			{
			case ChartBreaksMode.None:
				result = m_visibleRange.Extrapolate(value);
				break;
			case ChartBreaksMode.Auto:
				result = AutoValueToCoefficient(value);
				break;
			case ChartBreaksMode.Manual:
				result = ManualValueToCoefficient(value);
				break;
			}
		}
		else
		{
			result = m_visibleRange.Extrapolate(value);
		}
		return result;
	}

	public double ValueToCoefficient(double value)
	{
		double result = 0.0;
		if (!m_isEmpty)
		{
			switch (m_mode)
			{
			case ChartBreaksMode.None:
				result = m_visibleRange.Extrapolate(value);
				break;
			case ChartBreaksMode.Auto:
				result = AutoValueToCoefficient(value);
				break;
			case ChartBreaksMode.Manual:
				result = ManualValueToCoefficient(value);
				break;
			}
		}
		else
		{
			result = m_visibleRange.Extrapolate(value);
		}
		return result;
	}

	public double CoefficientToValue(double coefficient)
	{
		double result = 0.0;
		if (!m_isEmpty)
		{
			switch (m_mode)
			{
			case ChartBreaksMode.None:
				result = m_visibleRange.Interpolate(coefficient);
				break;
			case ChartBreaksMode.Auto:
				result = AutoCoefficientToValue(coefficient);
				break;
			case ChartBreaksMode.Manual:
				result = ManualCoefficientToValue(coefficient);
				break;
			}
		}
		else
		{
			result = m_visibleRange.Interpolate(coefficient);
		}
		return result;
	}

	private double AutoValueToCoefficient(double value)
	{
		double num = 0.0;
		foreach (ChartAxisSegment segment in m_segments)
		{
			if (segment.Range > value)
			{
				break;
			}
			if (segment.Range.Inside(value))
			{
				num += segment.Length * segment.Range.Extrapolate(value);
				break;
			}
			num += segment.Length;
		}
		return num;
	}

	private double AutoCoefficientToValue(double coefficient)
	{
		double num = 0.0;
		foreach (ChartAxisSegment segment in m_segments)
		{
			if (coefficient < segment.Length)
			{
				num += segment.Range.Interpolate(coefficient / segment.Length);
				break;
			}
			coefficient -= segment.Length;
		}
		return num;
	}

	private double ManualValueToCoefficient(double value)
	{
		double num = value - m_visibleRange.Start;
		foreach (DoubleRange range in m_ranges)
		{
			DoubleRange doubleRange = DoubleRange.Intersect(range, m_visibleRange);
			if (!doubleRange.IsEmpty && doubleRange.Start < value)
			{
				num -= Math.Min(doubleRange.Delta, value - doubleRange.Start);
			}
		}
		return num / (m_visibleRange.Delta - m_visibleSum);
	}

	private double ManualCoefficientToValue(double value)
	{
		double num = (m_visibleRange.Delta - m_visibleSum) * value + m_visibleRange.Start;
		foreach (DoubleRange range in m_ranges)
		{
			DoubleRange doubleRange = DoubleRange.Intersect(range, m_visibleRange);
			if (!doubleRange.IsEmpty && doubleRange.Start < value)
			{
				num += doubleRange.Delta;
			}
		}
		return num;
	}

	private void OnChanged(EventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(this, args);
		}
	}

	private void OnVisibleRangeChanged(object sender, EventArgs e)
	{
		m_visibleRange = new DoubleRange(m_axis.VisibleRange.min, m_axis.VisibleRange.max);
		Recalculate();
	}

	internal void Recalculate()
	{
		if (m_mode == ChartBreaksMode.Manual)
		{
			m_visibleSum = 0.0;
			foreach (DoubleRange range in m_ranges)
			{
				DoubleRange doubleRange = DoubleRange.Intersect(range, m_visibleRange);
				if (!doubleRange.IsEmpty)
				{
					m_visibleSum += doubleRange.Delta;
				}
			}
			m_isEmpty = m_ranges.Count == 0;
		}
		else if (m_mode == ChartBreaksMode.Auto)
		{
			m_isEmpty = m_segments.Count == 0;
		}
		else
		{
			m_isEmpty = true;
		}
	}
}
