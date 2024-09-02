using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal static class SystemFontExtensions
{
	private const int BUFFER_SIZE = 1024;

	public static T1 FindElement<T1, T2>(IEnumerable<T1> collection, T2 index, SystemFontCompareDelegate<T1, T2> comparer) where T1 : class
	{
		if (SystemFontEnumerable.Count(collection) == 0)
		{
			return null;
		}
		return FindElement(collection, 0, SystemFontEnumerable.Count(collection) - 1, index, comparer);
	}

	internal static byte[] ReadAllBytes(Stream reader)
	{
		if (!reader.CanRead)
		{
			return null;
		}
		if (reader.CanSeek)
		{
			reader.Seek(0L, SeekOrigin.Begin);
		}
		List<byte> list = new List<byte>();
		byte[] array = new byte[1024];
		int num;
		while ((num = reader.Read(array, 0, 1024)) > 0)
		{
			for (int i = 0; i < num; i++)
			{
				list.Add(array[i]);
			}
		}
		return list.ToArray();
	}

	private static T1 FindElement<T1, T2>(IEnumerable<T1> collection, int lo, int hi, T2 element, SystemFontCompareDelegate<T1, T2> comparer) where T1 : class
	{
		if (hi < lo)
		{
			return SystemFontEnumerable.ElementAt(collection, Math.Max(0, Math.Min(hi, SystemFontEnumerable.Count(collection))));
		}
		int num = lo + (hi - lo) / 2;
		T1 val = SystemFontEnumerable.ElementAt(collection, num);
		int num2 = comparer(val, element);
		if (num2 == 0)
		{
			return val;
		}
		if (num2 > 0)
		{
			return FindElement(collection, num + 1, hi, element, comparer);
		}
		return FindElement(collection, lo, num - 1, element, comparer);
	}
}
