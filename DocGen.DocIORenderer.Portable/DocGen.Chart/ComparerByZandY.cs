using System;
using System.Collections;

namespace DocGen.Chart;

internal class ComparerByZandY : IComparer
{
	int IComparer.Compare(object x, object y)
	{
		ChartSeries chartSeries = (ChartSeries)x;
		ChartSeries chartSeries2 = (ChartSeries)y;
		if (chartSeries.Points.Count == 0 && chartSeries2.Points.Count == 0)
		{
			return 0;
		}
		if (chartSeries.Points.Count == 0)
		{
			return -1;
		}
		if (chartSeries2.Points.Count == 0)
		{
			return 1;
		}
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < chartSeries.Points.Count; i++)
		{
			double[] yValues = chartSeries.Points[i].YValues;
			int j = 0;
			for (int num3 = yValues.Length; j < num3; j++)
			{
				num = Math.Max(num, Math.Abs(yValues[j]));
			}
		}
		for (int k = 0; k < chartSeries2.Points.Count; k++)
		{
			double[] yValues = chartSeries2.Points[k].YValues;
			int l = 0;
			for (int num4 = yValues.Length; l < num4; l++)
			{
				num2 = Math.Max(num2, Math.Abs(yValues[l]));
			}
		}
		if (num < num2)
		{
			return -1;
		}
		if (num > num2)
		{
			return 1;
		}
		return 0;
	}
}
