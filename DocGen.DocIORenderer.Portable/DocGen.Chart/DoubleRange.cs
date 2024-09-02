using System;

namespace DocGen.Chart;

internal struct DoubleRange
{
	private double m_start;

	private double m_end;

	private static DoubleRange c_empty = new DoubleRange(double.NaN, double.NaN);

	public double Start
	{
		get
		{
			return m_start;
		}
		internal set
		{
			m_start = value;
		}
	}

	public double End
	{
		get
		{
			return m_end;
		}
		internal set
		{
			m_end = value;
		}
	}

	public double Delta => m_end - m_start;

	public double Median => (m_start + m_end) / 2.0;

	public bool IsEmpty
	{
		get
		{
			if (!double.IsNaN(m_start))
			{
				return double.IsNaN(m_end);
			}
			return true;
		}
	}

	public static DoubleRange Empty => c_empty;

	public DoubleRange(double start, double end)
	{
		if (start > end)
		{
			m_start = end;
			m_end = start;
		}
		else
		{
			m_start = start;
			m_end = end;
		}
	}

	public static DoubleRange operator +(DoubleRange leftRange, DoubleRange rightRange)
	{
		return Union(leftRange, rightRange);
	}

	public static DoubleRange operator +(DoubleRange range, double value)
	{
		return Union(range, value);
	}

	public static bool operator >(DoubleRange range, double value)
	{
		return range.m_start > value;
	}

	public static bool operator <(DoubleRange range, double value)
	{
		return range.m_end < value;
	}

	public static bool operator >=(DoubleRange range, double value)
	{
		return range.m_start >= value;
	}

	public static bool operator <=(DoubleRange range, double value)
	{
		return range.m_end <= value;
	}

	public static bool operator ==(DoubleRange leftRange, DoubleRange rightRange)
	{
		return leftRange.Equals(rightRange);
	}

	public static bool operator !=(DoubleRange leftRange, DoubleRange rightRange)
	{
		return !leftRange.Equals(rightRange);
	}

	public static DoubleRange FromMedian(double median, double size)
	{
		return new DoubleRange(median - 0.5 * size, median + 0.5 * size);
	}

	public static DoubleRange Union(double[] values)
	{
		double num = double.MaxValue;
		double num2 = double.MinValue;
		foreach (double num3 in values)
		{
			if (num > num3)
			{
				num = num3;
			}
			if (num2 < num3)
			{
				num2 = num3;
			}
		}
		return new DoubleRange(num, num2);
	}

	public static DoubleRange Union(DoubleRange leftRange, DoubleRange rightRange)
	{
		if (leftRange.IsEmpty)
		{
			return rightRange;
		}
		if (rightRange.IsEmpty)
		{
			return leftRange;
		}
		return new DoubleRange(Math.Min(leftRange.m_start, rightRange.m_start), Math.Max(leftRange.m_end, rightRange.m_end));
	}

	public static DoubleRange Union(DoubleRange range, double value)
	{
		if (range.IsEmpty)
		{
			return new DoubleRange(value, value);
		}
		return new DoubleRange(Math.Min(range.m_start, value), Math.Max(range.m_end, value));
	}

	public static DoubleRange Scale(DoubleRange range, double value)
	{
		if (range.IsEmpty)
		{
			return range;
		}
		double num = 0.5 * value * range.Delta;
		double median = range.Median;
		return new DoubleRange(median - num, median + num);
	}

	public static DoubleRange Multiply(DoubleRange range, double value)
	{
		if (range.IsEmpty)
		{
			return range;
		}
		return new DoubleRange(value * range.m_start, value * range.m_end);
	}

	public static DoubleRange Inflate(DoubleRange range, double value)
	{
		return new DoubleRange(range.m_start - value, range.m_end + value);
	}

	public static DoubleRange Offset(DoubleRange range, double value)
	{
		if (range.IsEmpty)
		{
			return range;
		}
		return new DoubleRange(range.m_start + value, range.m_end + value);
	}

	public static DoubleRange Intersect(DoubleRange leftRange, DoubleRange rightRange)
	{
		if (leftRange.IsIntersects(rightRange))
		{
			return new DoubleRange(Math.Max(leftRange.m_start, rightRange.m_start), Math.Min(leftRange.m_end, rightRange.m_end));
		}
		return c_empty;
	}

	public static bool Exclude(DoubleRange range, DoubleRange excluder, out DoubleRange leftRange, out DoubleRange rightRange)
	{
		leftRange = Empty;
		rightRange = Empty;
		if (!range.IsEmpty && !excluder.IsEmpty)
		{
			if (excluder.m_start < range.m_start)
			{
				if (excluder.m_end > range.m_start)
				{
					leftRange = new DoubleRange(excluder.m_start, range.m_start);
				}
				else
				{
					leftRange = excluder;
				}
			}
			if (excluder.m_end > range.m_end)
			{
				if (excluder.m_start < range.m_end)
				{
					rightRange = new DoubleRange(range.m_end, excluder.m_end);
				}
				else
				{
					rightRange = excluder;
				}
			}
		}
		if (leftRange.IsEmpty)
		{
			return !rightRange.IsEmpty;
		}
		return true;
	}

	public bool IsIntersects(DoubleRange range)
	{
		if (IsEmpty || IsEmpty)
		{
			return false;
		}
		if (range.m_start <= m_end)
		{
			return range.m_end >= m_start;
		}
		return false;
	}

	public bool IsIntersects(double start, double end)
	{
		return IsIntersects(new DoubleRange(start, end));
	}

	public bool Inside(double value)
	{
		if (IsEmpty)
		{
			return false;
		}
		if (value <= m_end)
		{
			return value >= m_start;
		}
		return false;
	}

	public bool Inside(double value, bool equal)
	{
		if (IsEmpty)
		{
			return false;
		}
		if (equal)
		{
			if (value <= m_end)
			{
				return value >= m_start;
			}
			return false;
		}
		if (value < m_end)
		{
			return value > m_start;
		}
		return false;
	}

	public bool Inside(DoubleRange range)
	{
		if (IsEmpty)
		{
			return false;
		}
		if (m_start <= range.m_start)
		{
			return m_end >= range.m_end;
		}
		return false;
	}

	public double Interpolate(double interpolator)
	{
		return m_start + interpolator * (m_end - m_start);
	}

	public double Extrapolate(double value)
	{
		return (value - m_start) / (m_end - m_start);
	}

	public override bool Equals(object obj)
	{
		if (obj is DoubleRange doubleRange)
		{
			if (m_start == doubleRange.m_start)
			{
				return m_end == doubleRange.m_end;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_start.GetHashCode() ^ m_end.GetHashCode();
	}
}
