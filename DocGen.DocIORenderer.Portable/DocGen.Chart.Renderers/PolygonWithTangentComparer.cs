using System;
using System.Collections;

namespace DocGen.Chart.Renderers;

internal class PolygonWithTangentComparer : IComparer
{
	int IComparer.Compare(object x, object y)
	{
		double num = Math.Abs(((PolygonWithTangent)x).Tangent);
		double num2 = Math.Abs(((PolygonWithTangent)y).Tangent);
		if (num > num2)
		{
			return -1;
		}
		if (num < num2)
		{
			return 1;
		}
		return 0;
	}
}
