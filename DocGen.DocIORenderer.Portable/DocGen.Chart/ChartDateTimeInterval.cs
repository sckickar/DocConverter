#define TRACE
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace DocGen.Chart;

internal class ChartDateTimeInterval
{
	internal delegate bool IterationFilter(DateTime dt);

	internal delegate DateTime IterationModifier(DateTime dt);

	internal class Enumerator : IEnumerable, IEnumerator
	{
		protected IterationFilter iterationFilter;

		protected IterationModifier iterationModifier;

		protected ChartDateTimeInterval interval;

		protected DateTime start;

		protected DateTime end;

		protected int position = -1;

		protected DateTime lastDate;

		protected Calendar calendar;

		object IEnumerator.Current
		{
			get
			{
				if (position == -1)
				{
					return null;
				}
				return lastDate;
			}
		}

		internal Enumerator(ChartDateTimeInterval interval, DateTime start, DateTime end, Calendar calendar)
			: this(interval, start, end, calendar, null, null)
		{
		}

		internal Enumerator(ChartDateTimeInterval interval, DateTime start, DateTime end, Calendar calendar, IterationFilter iterationFilter, IterationModifier iterationModifier)
		{
			this.interval = interval;
			this.start = start;
			this.end = end;
			this.calendar = calendar;
			lastDate = start;
			this.iterationFilter = ((iterationFilter == null) ? new IterationFilter(DefaultIterationFilter) : iterationFilter);
			this.iterationModifier = ((iterationModifier == null) ? new IterationModifier(DefaultIterationModifier) : iterationModifier);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		bool IEnumerator.MoveNext()
		{
			DateTime nextDate = GetNextDate(lastDate, position == -1);
			if (!IsPastEnd(nextDate))
			{
				lastDate = nextDate;
				position++;
				return true;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			position = -1;
			lastDate = start;
		}

		protected virtual DateTime AdjustDate(DateTime dt)
		{
			return dt;
		}

		protected virtual bool IsPastEnd(DateTime dt)
		{
			return dt > end;
		}

		private DateTime GetNextDate(DateTime currentDate, bool first)
		{
			DateTime dt = interval.ApplyInterval(currentDate, calendar, first);
			dt = AdjustDate(dt);
			dt = DoIterationModifier(dt);
			if (!DoIterationFilter(dt) && !IsPastEnd(dt))
			{
				return GetNextDate(dt, first: false);
			}
			return dt;
		}

		private bool DoIterationFilter(DateTime dt)
		{
			return iterationFilter(dt);
		}

		private DateTime DoIterationModifier(DateTime dt)
		{
			return iterationModifier(dt);
		}

		private static bool DefaultIterationFilter(DateTime dt)
		{
			return true;
		}

		private static DateTime DefaultIterationModifier(DateTime dt)
		{
			return dt;
		}
	}

	internal class RangeEnumerator : Enumerator
	{
		protected DateTime rangeStart;

		protected DateTime rangeEnd;

		public RangeEnumerator(ChartDateTimeInterval interval, DateTime start, DateTime end, DateTime rangeStart, DateTime rangeEnd, Calendar calendar, IterationFilter iterationFilter, IterationModifier iterationModifier)
			: base(interval, rangeStart, rangeEnd, calendar, iterationFilter, iterationModifier)
		{
			this.rangeStart = rangeStart;
			this.rangeEnd = rangeEnd;
		}

		public RangeEnumerator(ChartDateTimeInterval interval, DateTime start, DateTime end, DateTime rangeStart, DateTime rangeEnd, Calendar calendar)
			: this(interval, start, end, rangeStart, rangeEnd, calendar, null, null)
		{
		}

		protected override DateTime AdjustDate(DateTime dt)
		{
			if (position == -1)
			{
				while (dt < rangeStart)
				{
					dt = interval.ApplyInterval(dt, calendar, first: false);
				}
			}
			return dt;
		}

		protected override bool IsPastEnd(DateTime dt)
		{
			return dt > rangeEnd;
		}
	}

	public const string DefaultIntervalName = "default";

	private ChartDateTimeIntervalType m_type;

	private double m_value;

	private ChartDateTimeIntervalType m_offsetType;

	private double m_offset;

	private ChartDateTimeRange m_parent;

	public ChartDateTimeIntervalType Type
	{
		get
		{
			if (m_value == 0.0)
			{
				return ChartDateTimeIntervalType.Auto;
			}
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public double Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public ChartDateTimeIntervalType OffsetType
	{
		get
		{
			return m_offsetType;
		}
		set
		{
			m_offsetType = value;
		}
	}

	public double Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	public ChartDateTimeRange Parent => m_parent;

	internal ChartDateTimeInterval(ChartDateTimeIntervalType type, double value, ChartDateTimeIntervalType offsetType, double offset)
	{
		m_type = type;
		m_value = value;
		m_offsetType = offsetType;
		m_offset = offset;
	}

	internal ChartDateTimeInterval(ChartDateTimeIntervalType type, double value)
		: this(type, value, ChartDateTimeIntervalType.Auto, 0.0)
	{
	}

	public IEnumerable Iterator()
	{
		return new Enumerator(this, m_parent.Start, m_parent.End, m_parent.Calendar);
	}

	public IEnumerable Iterator(IterationFilter filter)
	{
		return new Enumerator(this, m_parent.Start, m_parent.End, m_parent.Calendar, filter, null);
	}

	public IEnumerable Iterator(IterationModifier modifier)
	{
		return new Enumerator(this, m_parent.Start, m_parent.End, m_parent.Calendar, null, modifier);
	}

	public IEnumerable Iterator(IterationFilter filter, IterationModifier modifier)
	{
		return new Enumerator(this, m_parent.Start, m_parent.End, m_parent.Calendar, filter, modifier);
	}

	public IEnumerable Iterator(DateTime rangeStart, DateTime rangeEnd)
	{
		return new RangeEnumerator(this, m_parent.Start, m_parent.End, rangeStart, rangeEnd, m_parent.Calendar);
	}

	public IEnumerable Iterator(DateTime rangeStart, DateTime rangeEnd, IterationFilter filter)
	{
		return new RangeEnumerator(this, m_parent.Start, m_parent.End, rangeStart, rangeEnd, m_parent.Calendar, filter, null);
	}

	public IEnumerable Iterator(DateTime rangeStart, DateTime rangeEnd, IterationModifier modifier)
	{
		return new RangeEnumerator(this, m_parent.Start, m_parent.End, rangeStart, rangeEnd, m_parent.Calendar, null, modifier);
	}

	public IEnumerable Iterator(DateTime rangeStart, DateTime rangeEnd, IterationFilter filter, IterationModifier modifier)
	{
		return new RangeEnumerator(this, m_parent.Start, m_parent.End, rangeStart, rangeEnd, m_parent.Calendar, filter, modifier);
	}

	public static int GetIntervalCount(IEnumerable enumerable)
	{
		if (!(enumerable is Enumerator))
		{
			throw new ArgumentException("Invalid argument. Enumerator needs to be of type ChartDateTimeInterval.Enumerator.");
		}
		int num = 0;
		foreach (DateTime item in enumerable)
		{
			_ = item;
			num++;
		}
		if (num > 1)
		{
			return num - 1;
		}
		return 1;
	}

	public override string ToString()
	{
		return $"Type: {m_type}, Value: {m_value}, OffsetType: {m_offsetType}, Offset: {m_offset}";
	}

	protected virtual DateTime ApplyInterval(DateTime dateTime, Calendar calendar, ChartDateTimeIntervalType type, double interval)
	{
		DateTime result = dateTime;
		double num = 0.0;
		switch (type)
		{
		case ChartDateTimeIntervalType.Years:
		{
			int num2 = (int)Math.Floor(interval);
			result = dateTime.AddYears(num2);
			num = (interval - (double)num2) * (double)calendar.GetDaysInYear(result.Year);
			result = result.AddDays(num);
			break;
		}
		case ChartDateTimeIntervalType.Months:
		{
			int num3 = (int)Math.Floor(interval);
			result = dateTime.AddMonths(num3);
			num = (interval - (double)num3) * (double)calendar.GetDaysInMonth(result.Year, result.Month);
			result = result.AddDays(num);
			break;
		}
		case ChartDateTimeIntervalType.Weeks:
			result = dateTime.AddDays(7.0 * interval);
			break;
		case ChartDateTimeIntervalType.Days:
			result = dateTime.AddDays(interval);
			break;
		case ChartDateTimeIntervalType.Hours:
			result = dateTime.AddHours(interval);
			break;
		case ChartDateTimeIntervalType.Minutes:
			result = dateTime.AddMinutes(interval);
			break;
		case ChartDateTimeIntervalType.Seconds:
			result = dateTime.AddSeconds(interval);
			break;
		case ChartDateTimeIntervalType.MilliSeconds:
			result = dateTime.AddMilliseconds(interval);
			break;
		default:
			if (interval != 0.0)
			{
				Trace.Fail("Invalid DateTimeIntervalType");
				throw new ArgumentOutOfRangeException("Invalid DateTimeIntervalType");
			}
			break;
		}
		return result;
	}

	internal virtual DateTime IncreaseInterval(DateTime date, double interval, ChartDateTimeIntervalType intervalType)
	{
		TimeSpan value = new TimeSpan(0L);
		switch (intervalType)
		{
		case ChartDateTimeIntervalType.Days:
			value = TimeSpan.FromDays(interval);
			break;
		case ChartDateTimeIntervalType.Weeks:
			value = TimeSpan.FromDays(7.0 * interval);
			break;
		case ChartDateTimeIntervalType.Hours:
			value = TimeSpan.FromHours(interval);
			break;
		case ChartDateTimeIntervalType.MilliSeconds:
			value = TimeSpan.FromMilliseconds(interval);
			break;
		case ChartDateTimeIntervalType.Seconds:
			value = TimeSpan.FromSeconds(interval);
			break;
		case ChartDateTimeIntervalType.Minutes:
			value = TimeSpan.FromMinutes(interval);
			break;
		case ChartDateTimeIntervalType.Months:
			date = date.AddMonths((int)Math.Floor(interval));
			value = TimeSpan.FromDays(30.0 * (interval - Math.Floor(interval)));
			break;
		case ChartDateTimeIntervalType.Years:
			date = date.AddYears((int)Math.Floor(interval));
			value = TimeSpan.FromDays(365.0 * (interval - Math.Floor(interval)));
			break;
		}
		return date.Add(value);
	}

	internal void SetParent(ChartDateTimeRange parent)
	{
		m_parent = parent;
	}

	internal DateTime ApplyInterval(DateTime dateTime, Calendar calendar, bool first)
	{
		if (first)
		{
			return ApplyInterval(dateTime, calendar, OffsetType, Offset);
		}
		return ApplyInterval(dateTime, calendar, Type, Value);
	}
}
