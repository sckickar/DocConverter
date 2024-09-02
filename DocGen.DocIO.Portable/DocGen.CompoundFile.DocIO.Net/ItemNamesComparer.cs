using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.CompoundFile.DocIO.Net;

internal class ItemNamesComparer : IComparer, IComparer<string>
{
	public int Compare(object x, object y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (y == null)
		{
			return 1;
		}
		if (x == null)
		{
			return -1;
		}
		string text = x.ToString();
		string text2 = y.ToString();
		int length = text.Length;
		int length2 = text2.Length;
		int num = length - length2;
		if (num == 0)
		{
			num = StringComparer.Ordinal.Compare(text, text2);
		}
		return num;
	}

	public int Compare(string x, string y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (y == null)
		{
			return 1;
		}
		if (x == null)
		{
			return -1;
		}
		int length = x.Length;
		int length2 = y.Length;
		int num = length - length2;
		if (num == 0)
		{
			num = StringComparer.Ordinal.Compare(x.ToUpper(), y.ToUpper());
		}
		return num;
	}
}
