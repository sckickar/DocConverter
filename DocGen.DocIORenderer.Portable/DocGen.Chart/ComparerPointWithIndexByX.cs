using System.Collections;

namespace DocGen.Chart;

internal class ComparerPointWithIndexByX : IComparer
{
	int IComparer.Compare(object x, object y)
	{
		ChartPoint point = (x as ChartPointWithIndex).Point;
		ChartPoint point2 = (y as ChartPointWithIndex).Point;
		if (point.X < point2.X)
		{
			return -1;
		}
		if (point.X > point2.X)
		{
			return 1;
		}
		return 0;
	}
}
