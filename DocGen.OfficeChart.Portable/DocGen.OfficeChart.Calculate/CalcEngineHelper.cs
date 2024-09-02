using System;
using System.Globalization;

namespace DocGen.OfficeChart.Calculate;

internal static class CalcEngineHelper
{
	internal static double ToOADate(this DateTime inDateTime)
	{
		DateTime dateTime = DateTime.Parse("1899-12-30 12:0:0 AM");
		DateTime dateTime2 = DateTime.Parse("9999-12-31 12:0:0 AM");
		if (inDateTime < dateTime || inDateTime > dateTime2)
		{
			throw new ArgumentException("Not an Valid OLE Date.");
		}
		double num = (inDateTime - dateTime).Days;
		TimeSpan timeSpan = new TimeSpan(inDateTime.Hour, inDateTime.Minute, inDateTime.Second);
		double num2 = ((double)timeSpan.Hours + ((double)timeSpan.Minutes + (double)timeSpan.Seconds / 60.0) / 60.0) / 24.0;
		return num + num2;
	}

	internal static DateTime FromOADate(double doubleOLEValue)
	{
		if (doubleOLEValue < -657435.0 || doubleOLEValue > 2958465.99999999)
		{
			throw new ArgumentException("Not an valid OLE value.");
		}
		double num = 86400.0;
		string[] array = doubleOLEValue.ToString().Split(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
		int num2 = Convert.ToInt32(array[0]);
		int num3 = (int)(((array.Length > 1) ? Convert.ToDouble(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + array[1]) : 0.0) * num);
		return DateTime.Parse("1899-12-30 12:0:0 AM").AddDays(num2).AddSeconds(num3);
	}
}
