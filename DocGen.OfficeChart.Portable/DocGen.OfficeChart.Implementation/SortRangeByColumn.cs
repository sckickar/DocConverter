using System.Collections;

namespace DocGen.OfficeChart.Implementation;

internal class SortRangeByColumn : IComparer
{
	public int Compare(object x, object y)
	{
		RangeImpl obj = x as RangeImpl;
		RangeImpl rangeImpl = y as RangeImpl;
		return obj.Column - rangeImpl.Column;
	}
}
