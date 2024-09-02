using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class SortPointByX : IComparer<PointF>
{
	public int Compare(PointF a, PointF b)
	{
		if (a.X > b.X)
		{
			return 1;
		}
		if (a.X < b.X)
		{
			return -1;
		}
		return 0;
	}
}
