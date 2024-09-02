using System;
using System.Globalization;

namespace DocGen.Chart;

internal sealed class ChartDateTimeRange
{
	private DateTime start;

	private DateTime end;

	private Calendar calendar;

	private ChartIntervalCollection intervalCollection;

	public DateTime Start => start;

	public DateTime End => end;

	public ChartDateTimeInterval DefaultInterval => Intervals["default"];

	public ChartIntervalCollection Intervals => intervalCollection;

	public Calendar Calendar => calendar;

	public ChartDateTimeRange(DateTime start, DateTime end, double interval, ChartDateTimeIntervalType type)
		: this(start, end, interval, type, CultureInfo.CurrentCulture.Calendar)
	{
	}

	public ChartDateTimeRange(DateTime start, DateTime end, double interval, ChartDateTimeIntervalType type, Calendar calendar)
	{
		this.start = start;
		this.end = end;
		this.calendar = calendar;
		intervalCollection = new ChartIntervalCollection(this);
		ChartDateTimeInterval interval2 = new ChartDateTimeInterval(type, interval, ChartDateTimeIntervalType.Auto, 0.0);
		Intervals.Register("default", interval2);
	}

	public override string ToString()
	{
		return $"Start: {start.ToString()}\r\nEnd: {end.ToString()}\r\nIntervals\r\n{Intervals.ToString()}\r\n";
	}
}
