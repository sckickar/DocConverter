using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontOrderedEnumerable<TElement> : ISystemFontOrderedEnumerable<TElement>, IEnumerable<TElement>, IEnumerable
{
	internal IEnumerable<TElement> source;

	internal abstract SystemFontEnumerableSorter<TElement> GetEnumerableSorter(SystemFontEnumerableSorter<TElement> next);

	public IEnumerator<TElement> GetEnumerator()
	{
		SystemFontBuffer<TElement> buffer = new SystemFontBuffer<TElement>(source);
		if (buffer.count > 0)
		{
			SystemFontEnumerableSorter<TElement> enumerableSorter = GetEnumerableSorter(null);
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

	ISystemFontOrderedEnumerable<TElement> ISystemFontOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
	{
		return new OrderedEnumerable<TElement, TKey>(source, keySelector, comparer, descending)
		{
			parent = this
		};
	}
}
