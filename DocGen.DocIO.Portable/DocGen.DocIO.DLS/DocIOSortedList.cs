using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class DocIOSortedList<TKey, TValue> : TypedSortedListEx<TKey, TValue> where TKey : IComparable
{
	public DocIOSortedList()
	{
	}

	public DocIOSortedList(IComparer<TKey> comparer)
		: base(comparer)
	{
	}

	public DocIOSortedList(int count)
		: base(count)
	{
	}

	public DocIOSortedList(IDictionary<TKey, TValue> dictionary)
		: base(dictionary)
	{
	}
}
