using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class StringComparer : IComparer<string>, IComparer
{
	public int Compare(object x, object y)
	{
		string text = x as string;
		string text2 = y as string;
		if (text != null && text2 != null)
		{
			return string.CompareOrdinal(text, text2);
		}
		return 0;
	}

	public int Compare(string x, string y)
	{
		if (x != null && y != null)
		{
			return string.CompareOrdinal(x, y);
		}
		return 0;
	}
}
