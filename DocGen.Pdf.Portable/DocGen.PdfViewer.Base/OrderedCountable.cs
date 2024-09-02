using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal abstract class OrderedCountable<TElement> : IOrderedCountable<TElement>, IEnumerable<TElement>, IEnumerable
{
	internal IEnumerable<TElement> source;

	internal abstract CountableSortingHelper<TElement> GetEnumerableSorter(CountableSortingHelper<TElement> next);

	public IEnumerator<TElement> GetEnumerator()
	{
		Buffer<TElement> buffer = new Buffer<TElement>(source);
		if (buffer.count > 0)
		{
			CountableSortingHelper<TElement> enumerableSorter = GetEnumerableSorter(null);
			int[] array = enumerableSorter.Sort(buffer.items, buffer.count);
			for (int i = 0; i < buffer.count; i++)
			{
				yield return buffer.items[array[i]];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IOrderedCountable<TElement> IOrderedCountable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
	{
		return new OrderedEnumerable<TElement, TKey>(source, keySelector, comparer, descending)
		{
			parent = this
		};
	}
}
