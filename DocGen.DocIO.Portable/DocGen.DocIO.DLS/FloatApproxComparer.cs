using System.Collections;

namespace DocGen.DocIO.DLS;

internal class FloatApproxComparer : IComparer
{
	public int Compare(object x, object y)
	{
		if ((float)x - (float)y < 0.0001f)
		{
			return 0;
		}
		if ((float)x > (float)y)
		{
			return 1;
		}
		return -1;
	}
}
