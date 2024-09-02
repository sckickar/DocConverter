using System.Collections;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartSegmentComparer : IComparer
{
	private bool m_isInvesed;

	public ChartSegmentComparer(bool inversed)
	{
		m_isInvesed = inversed;
	}

	public int Compare(object x, object y)
	{
		ChartSegment chartSegment = x as ChartSegment;
		ChartSegment chartSegment2 = y as ChartSegment;
		RectangleF bounds = chartSegment.Bounds;
		RectangleF bounds2 = chartSegment2.Bounds;
		if (chartSegment.ZOrder > chartSegment2.ZOrder)
		{
			return 1;
		}
		if (chartSegment.ZOrder < chartSegment2.ZOrder)
		{
			return -1;
		}
		if (m_isInvesed)
		{
			if (bounds.Y < bounds2.Y)
			{
				return 1;
			}
			if (bounds.Y > bounds2.Y)
			{
				return -1;
			}
			if (bounds.X > bounds2.X)
			{
				return 1;
			}
			if (bounds.X < bounds2.X)
			{
				return -1;
			}
		}
		else
		{
			if (bounds.X > bounds2.X)
			{
				return 1;
			}
			if (bounds.X < bounds2.X)
			{
				return -1;
			}
			if (bounds.Y < bounds2.Y)
			{
				return 1;
			}
			if (bounds.Y > bounds2.Y)
			{
				return -1;
			}
		}
		return 0;
	}
}
