using System;
using System.Globalization;

namespace DocGen.Chart;

internal interface IChartDateTimeDefaults
{
	Calendar GetCalendar();

	int GetDaysInYear();

	int GetDaysInMonth();

	int GetMinDaysInMonth();

	int GetDaysInWeek();

	DayOfWeek GetFirstDayOfWeek();
}
