using System.Collections;

namespace DocGen.OfficeChart.Implementation;

internal class SortByRow : IComparer
{
	public int Compare(object x, object y)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex((int)x);
		int rowFromCellIndex2 = RangeImpl.GetRowFromCellIndex((int)y);
		return rowFromCellIndex - rowFromCellIndex2;
	}
}
