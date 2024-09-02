using System.Collections;

namespace DocGen.Chart;

internal class ComparerPointWithIndexByY : IComparer
{
	int IComparer.Compare(object x, object y)
	{
		ChartPointWithIndex obj = (ChartPointWithIndex)x;
		ChartPointWithIndex chartPointWithIndex = (ChartPointWithIndex)y;
		ChartPoint point = obj.Point;
		ChartPoint point2 = chartPointWithIndex.Point;
		if (point.YValues[0] > point2.YValues[0])
		{
			return 1;
		}
		if (point.YValues[0] < point2.YValues[0])
		{
			return -1;
		}
		return 0;
	}
}
