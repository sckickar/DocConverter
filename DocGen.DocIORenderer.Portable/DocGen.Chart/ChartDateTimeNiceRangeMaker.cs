#define TRACE
using System;
using System.Diagnostics;
using System.Globalization;

namespace DocGen.Chart;

internal sealed class ChartDateTimeNiceRangeMaker
{
	private enum SeekDirection
	{
		Forward,
		Reverse
	}

	private class ImplWeeks : NiceRangeMaker
	{
		protected override double MakeNiceNumber(double val)
		{
			if (val < 10.0)
			{
				return 10.0;
			}
			return 100.0;
		}
	}

	private INiceRangeMaker m_niceRangeMaker;

	private INiceRangeMaker m_niceWeeksRangeMaker;

	private Calendar m_calendar;

	private IChartDateTimeDefaults m_chartDateTimeDefaults;

	private ChartDateTimeIntervalType _desiredIntervalType;

	private bool m_isMax;

	public IChartDateTimeDefaults Defaults => m_chartDateTimeDefaults;

	public Calendar Calendar => m_calendar;

	public int DesiredIntervals
	{
		get
		{
			return m_niceRangeMaker.DesiredIntervals;
		}
		set
		{
			m_niceRangeMaker.DesiredIntervals = value;
			m_niceWeeksRangeMaker.DesiredIntervals = value;
		}
	}

	public ChartAxisRangePaddingType RangePaddingType
	{
		get
		{
			return m_niceRangeMaker.RangePaddingType;
		}
		set
		{
			m_niceRangeMaker.RangePaddingType = value;
		}
	}

	public bool ForceZero
	{
		get
		{
			return m_niceRangeMaker.ForceZero;
		}
		set
		{
			m_niceRangeMaker.ForceZero = value;
		}
	}

	public bool PreferZero
	{
		get
		{
			return m_niceRangeMaker.PreferZero;
		}
		set
		{
			m_niceRangeMaker.PreferZero = value;
		}
	}

	public ChartDateTimeIntervalType DesiredIntervalType
	{
		get
		{
			return _desiredIntervalType;
		}
		set
		{
			_desiredIntervalType = value;
		}
	}

	internal bool IsMax
	{
		get
		{
			return m_isMax;
		}
		set
		{
			m_isMax = value;
		}
	}

	public ChartDateTimeNiceRangeMaker(IChartDateTimeDefaults chartDateTimeDefaults, INiceRangeMaker niceRangeMaker)
	{
		m_chartDateTimeDefaults = chartDateTimeDefaults;
		m_niceRangeMaker = niceRangeMaker;
		m_niceWeeksRangeMaker = new ImplWeeks();
		m_calendar = chartDateTimeDefaults.GetCalendar();
	}

	public ChartDateTimeNiceRangeMaker(IChartDateTimeDefaults chartDateTimeDefaults)
		: this(chartDateTimeDefaults, new NiceRangeMaker())
	{
	}

	public ChartDateTimeNiceRangeMaker()
		: this(new ChartDateTimeDefaults(), new NiceRangeMaker())
	{
	}

	public ChartDateTimeRange MakeNiceRange(DateTime start, DateTime end)
	{
		ChartDateTimeIntervalType chartDateTimeIntervalType = _desiredIntervalType;
		if (chartDateTimeIntervalType == ChartDateTimeIntervalType.Auto)
		{
			chartDateTimeIntervalType = CalculateIntervalType(end.Subtract(start));
		}
		return MakeNiceRange(start, end, chartDateTimeIntervalType, m_niceRangeMaker.RangePaddingType);
	}

	public ChartDateTimeRange MakeNiceRange(DateTime start, DateTime end, ChartDateTimeIntervalType type)
	{
		return MakeNiceRange(start, end, type, m_niceRangeMaker.RangePaddingType);
	}

	public ChartDateTimeRange MakeNiceRange(DateTime start, DateTime end, ChartDateTimeIntervalType type, ChartAxisRangePaddingType rangePaddingType)
	{
		if (end <= start)
		{
			throw new ArgumentOutOfRangeException("End needs to be later than start.");
		}
		TimeSpan timeSpan = end - start;
		if (rangePaddingType == ChartAxisRangePaddingType.Calculate)
		{
			_ = timeSpan.TotalMilliseconds / (double)DesiredIntervals;
		}
		switch (type)
		{
		case ChartDateTimeIntervalType.Years:
			return MakeNiceYearsRange(start, end);
		case ChartDateTimeIntervalType.Months:
			return MakeNiceMonthsRange(start, end);
		case ChartDateTimeIntervalType.Weeks:
			return MakeNiceWeeksRange(start, end);
		case ChartDateTimeIntervalType.Days:
			return MakeNiceDaysRange(start, end);
		case ChartDateTimeIntervalType.Hours:
			return MakeNiceHoursRange(start, end);
		case ChartDateTimeIntervalType.Minutes:
			return MakeNiceMinutesRange(start, end);
		case ChartDateTimeIntervalType.Seconds:
			return MakeNiceSecondsRange(start, end);
		case ChartDateTimeIntervalType.MilliSeconds:
			return MakeNiceMilliSecondsRange(start, end);
		default:
			Trace.Fail("Invalid DateTimeIntervalType");
			throw new ArgumentOutOfRangeException("Invalid DateTimeIntervalType");
		}
	}

	private ChartDateTimeRange MakeNiceYearsRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		DateTime minValue = DateTime.MinValue;
		DateTime minValue2 = DateTime.MinValue;
		double num = 0.0;
		ChartDateTimeIntervalType type = ChartDateTimeIntervalType.Years;
		double min = 0.0;
		double max = (int)timeSpan.TotalDays / Defaults.GetDaysInYear();
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(min, max, m_niceRangeMaker.RangePaddingType);
		minValue = new DateTime(start.Year, 1, 1, 0, 0, 0, 0, m_calendar);
		minValue2 = m_calendar.AddYears(minValue, (int)minMaxInfo.Max);
		num = minMaxInfo.Interval;
		return new ChartDateTimeRange(minValue, minValue2, num, type, m_calendar);
	}

	private ChartDateTimeRange MakeNiceMonthsRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		DateTime minValue = DateTime.MinValue;
		DateTime minValue2 = DateTime.MinValue;
		double num = 0.0;
		ChartDateTimeIntervalType type = ChartDateTimeIntervalType.Months;
		m_niceRangeMaker.ForceZero = true;
		double min = 0.0;
		double max = (int)Math.Ceiling(timeSpan.TotalDays / (double)Defaults.GetDaysInMonth());
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(min, max, RangePaddingType);
		minValue = new DateTime(start.Year, start.Month, 1, 0, 0, 0, 0, m_calendar);
		minValue2 = m_calendar.AddMonths(minValue, (int)minMaxInfo.Max);
		num = minMaxInfo.Interval;
		return new ChartDateTimeRange(minValue, minValue2, num, type, m_calendar);
	}

	private ChartDateTimeRange MakeNiceWeeksRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		DateTime minValue = DateTime.MinValue;
		DateTime minValue2 = DateTime.MinValue;
		m_niceRangeMaker.ForceZero = true;
		double min = 0.0;
		double max = (int)Math.Ceiling(timeSpan.TotalDays / (double)Defaults.GetDaysInWeek());
		MinMaxInfo minMaxInfo = m_niceWeeksRangeMaker.MakeNiceRange(min, max, RangePaddingType);
		minValue = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, 0, m_calendar);
		minValue2 = m_calendar.AddDays(minValue, (int)Math.Ceiling((double)Defaults.GetDaysInWeek() * minMaxInfo.Max));
		minValue = AdjustToWeekStart(minValue, SeekDirection.Reverse);
		minValue2 = AdjustToWeekStart(minValue2, SeekDirection.Forward);
		return new ChartDateTimeRange(minValue, minValue2, minMaxInfo.Interval, ChartDateTimeIntervalType.Weeks, m_calendar);
	}

	private ChartDateTimeRange MakeNiceDaysRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		DateTime minValue = DateTime.MinValue;
		DateTime minValue2 = DateTime.MinValue;
		double num = 0.0;
		ChartDateTimeIntervalType type = ChartDateTimeIntervalType.Days;
		double min = 0.0;
		double max = (int)Math.Ceiling(timeSpan.TotalDays);
		if (IsMax)
		{
			RangePaddingType = ChartAxisRangePaddingType.None;
		}
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(min, max, RangePaddingType);
		minValue = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, 0, m_calendar);
		minValue2 = m_calendar.AddDays(minValue, (int)Math.Ceiling(minMaxInfo.Max));
		num = minMaxInfo.Interval;
		return new ChartDateTimeRange(minValue, minValue2, num, type, m_calendar);
	}

	private ChartDateTimeRange MakeNiceHoursRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(0.0, Math.Ceiling(timeSpan.TotalHours), m_niceRangeMaker.RangePaddingType);
		DateTime dateTime = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, 0, m_calendar);
		DateTime start2 = dateTime.AddHours(minMaxInfo.min);
		DateTime end2 = dateTime.AddHours(minMaxInfo.max);
		return new ChartDateTimeRange(start2, end2, minMaxInfo.Interval, ChartDateTimeIntervalType.Hours, m_calendar);
	}

	private ChartDateTimeRange MakeNiceMinutesRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(0.0, Math.Ceiling(timeSpan.TotalMinutes), m_niceRangeMaker.RangePaddingType);
		DateTime dateTime = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, 0, m_calendar);
		DateTime start2 = dateTime.AddMinutes(minMaxInfo.min);
		DateTime end2 = dateTime.AddMinutes(minMaxInfo.max);
		return new ChartDateTimeRange(start2, end2, minMaxInfo.Interval, ChartDateTimeIntervalType.Minutes, m_calendar);
	}

	private ChartDateTimeRange MakeNiceSecondsRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(0.0, Math.Ceiling(timeSpan.TotalSeconds), m_niceRangeMaker.RangePaddingType);
		DateTime dateTime = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second, 0, m_calendar);
		DateTime start2 = dateTime.AddSeconds(minMaxInfo.min);
		DateTime end2 = dateTime.AddSeconds(minMaxInfo.max);
		return new ChartDateTimeRange(start2, end2, minMaxInfo.Interval, ChartDateTimeIntervalType.Seconds, m_calendar);
	}

	private ChartDateTimeRange MakeNiceMilliSecondsRange(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		MinMaxInfo minMaxInfo = m_niceRangeMaker.MakeNiceRange(0.0, Math.Ceiling(timeSpan.TotalMilliseconds), m_niceRangeMaker.RangePaddingType);
		DateTime dateTime = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, start.Second, start.Millisecond, m_calendar);
		DateTime start2 = dateTime.AddSeconds(minMaxInfo.min);
		DateTime end2 = dateTime.AddSeconds(minMaxInfo.max);
		return new ChartDateTimeRange(start2, end2, minMaxInfo.Interval, ChartDateTimeIntervalType.MilliSeconds, m_calendar);
	}

	private ChartDateTimeIntervalType CalculateIntervalType(TimeSpan diff)
	{
		if (diff.Days > Defaults.GetDaysInYear())
		{
			return ChartDateTimeIntervalType.Years;
		}
		if (diff.Days > Defaults.GetDaysInMonth())
		{
			return ChartDateTimeIntervalType.Months;
		}
		if (diff.Days > Defaults.GetDaysInWeek() * 2)
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

	private DateTime AdjustToWeekStart(DateTime dt, SeekDirection direction)
	{
		int dayOfWeek = (int)dt.DayOfWeek;
		_ = dt.Day;
		if (dt.DayOfWeek != 0)
		{
			dt = ((direction != 0) ? m_calendar.AddDays(dt, -dayOfWeek) : m_calendar.AddDays(dt, 7 - dayOfWeek));
		}
		return dt;
	}
}
