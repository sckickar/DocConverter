using System.Collections;

namespace DocGen.OfficeChart.Implementation;

internal class SortRangeByRow : IComparer
{
	public int Compare(object x, object y)
	{
		RangeImpl obj = x as RangeImpl;
		RangeImpl rangeImpl = y as RangeImpl;
		return obj.Row - rangeImpl.Row;
	}
}
