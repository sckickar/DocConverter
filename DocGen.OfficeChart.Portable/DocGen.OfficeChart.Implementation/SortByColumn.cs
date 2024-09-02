using System.Collections;

namespace DocGen.OfficeChart.Implementation;

internal class SortByColumn : IComparer
{
	public int Compare(object x, object y)
	{
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex((int)x);
		int columnFromCellIndex2 = RangeImpl.GetColumnFromCellIndex((int)y);
		return columnFromCellIndex - columnFromCellIndex2;
	}
}
