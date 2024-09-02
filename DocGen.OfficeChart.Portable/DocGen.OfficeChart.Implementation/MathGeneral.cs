using System;

namespace DocGen.OfficeChart.Implementation;

internal class MathGeneral
{
	public static double Truncate(double value)
	{
		double num = Math.Round(value);
		if (num > value)
		{
			num -= 1.0;
		}
		return num;
	}
}
