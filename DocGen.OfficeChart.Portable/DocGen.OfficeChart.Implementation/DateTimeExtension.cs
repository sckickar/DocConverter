using System;
using System.Globalization;

namespace DocGen.OfficeChart.Implementation;

internal static class DateTimeExtension
{
	public static DateTime FromOADate(double doubleOLEValue)
	{
		if (doubleOLEValue < -657435.0 || doubleOLEValue > 2958465.99999999)
		{
			throw new ArgumentException("Not an valid OLE value.");
		}
		double num = 86400.0;
		string[] array = doubleOLEValue.ToString().Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
		int num2 = Convert.ToInt32(array[0]);
		double value = ((array.Length == 1) ? 0.0 : Convert.ToDouble(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + array[1]));
		return DateTime.Parse("1899-12-30 12:0:0 AM").AddDays(num2).AddDays(value);
	}

	public static double ToOADate(this DateTime inDateTime)
	{
		DateTime dateTime = DateTime.Parse("1899-12-30 12:0:0 AM");
		DateTime minSupportedDateTime = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MinSupportedDateTime;
		DateTime maxSupportedDateTime = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime;
		if (inDateTime < minSupportedDateTime || inDateTime > maxSupportedDateTime)
		{
			throw new ArgumentException("Not a valid OLE date.");
		}
		return (inDateTime - dateTime).TotalDays;
	}
}
