using System;
using System.Collections.Generic;

namespace DocGen.CompoundFile.DocIO.Net;

internal class SortedList<TKey, TValue> : TypedSortedListEx<TKey, TValue> where TKey : IComparable
{
	public SortedList()
	{
	}

	public SortedList(IComparer<TKey> comparer)
		: base(comparer)
	{
	}

	public SortedList(int count)
		: base(count)
	{
	}

	public SortedList(IDictionary<TKey, TValue> dictionary)
		: base(dictionary)
	{
	}
}
