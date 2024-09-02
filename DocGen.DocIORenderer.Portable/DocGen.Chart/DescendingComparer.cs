using System.Collections;

namespace DocGen.Chart;

internal class DescendingComparer : IComparer
{
	public int Compare(object x, object y)
	{
		if (x is string)
		{
			return string.Compare(y.ToString(), x.ToString());
		}
		double num = (double)x;
		double num2 = (double)y;
		if (!(num2 < num))
		{
			return (num2 > num) ? 1 : 0;
		}
		return -1;
	}
}
