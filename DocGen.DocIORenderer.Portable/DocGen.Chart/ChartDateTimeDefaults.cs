using System;
using System.Globalization;
using System.Threading;

namespace DocGen.Chart;

internal class ChartDateTimeDefaults : IChartDateTimeDefaults
{
	public const int DaysInYear = 365;

	public const int DaysInMonth = 31;

	public const int MinDaysInMonth = 28;

	public const int DaysInWeek = 7;

	public const DayOfWeek FirstDayOfWeek = DayOfWeek.Sunday;

	private Calendar calendar = new GregorianCalendar();

	public virtual Calendar GetCalendar()
	{
		return Thread.CurrentThread.CurrentUICulture.Calendar;
	}

	public virtual int GetDaysInYear()
	{
		return 365;
	}

	public virtual int GetDaysInMonth()
	{
		return 31;
	}

	public virtual int GetMinDaysInMonth()
	{
		return 28;
	}

	public virtual int GetDaysInWeek()
	{
		return 7;
	}

	public virtual DayOfWeek GetFirstDayOfWeek()
	{
		return DayOfWeek.Sunday;
	}
}
