using System.Collections;

namespace DocGen.Chart;

internal sealed class ChartSeriesComparerByZOrder : IComparer
{
	public int Compare(object x, object y)
	{
		ChartSeries chartSeries = (ChartSeries)x;
		ChartSeries chartSeries2 = (ChartSeries)y;
		if (chartSeries.ZOrder > chartSeries2.ZOrder)
		{
			return 1;
		}
		if (chartSeries.ZOrder < chartSeries2.ZOrder)
		{
			return -1;
		}
		return 0;
	}
}
