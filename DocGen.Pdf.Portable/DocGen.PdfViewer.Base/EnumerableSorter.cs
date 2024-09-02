using System;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class EnumerableSorter<TElement, TKey> : CountableSortingHelper<TElement>
{
	internal Func<TElement, TKey> keySelector;

	internal IComparer<TKey> comparer;

	internal bool descending;

	internal CountableSortingHelper<TElement> next;

	internal TKey[] keys;

	internal EnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, CountableSortingHelper<TElement> next)
	{
		this.keySelector = keySelector;
		this.comparer = comparer;
		this.descending = descending;
		this.next = next;
	}

	internal override int CompareKeys(int index1, int index2)
	{
		int num = comparer.Compare(keys[index1], keys[index2]);
		if (num != 0)
		{
			if (descending)
			{
				return -num;
			}
			return num;
		}
		if (next != null)
		{
			return next.CompareKeys(index1, index2);
		}
		return index1 - index2;
	}

	internal override void ComputeKeys(TElement[] elements, int count)
	{
		keys = new TKey[count];
		for (int i = 0; i < count; i++)
		{
			keys[i] = keySelector(elements[i]);
		}
		if (next != null)
		{
			next.ComputeKeys(elements, count);
		}
	}
}
