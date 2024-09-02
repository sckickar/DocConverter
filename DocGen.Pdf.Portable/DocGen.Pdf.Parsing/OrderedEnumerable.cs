using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class OrderedEnumerable<TElement, TKey> : SystemFontOrderedEnumerable<TElement>
{
	internal SystemFontOrderedEnumerable<TElement> parent;

	internal Func<TElement, TKey> keySelector;

	internal IComparer<TKey> comparer;

	internal bool descending;

	internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
	{
		if (keySelector != null)
		{
			base.source = source;
			parent = null;
			this.keySelector = keySelector;
			this.comparer = ((comparer == null) ? Comparer<TKey>.Default : comparer);
			this.descending = descending;
			return;
		}
		throw new Exception("keySelector");
	}

	internal override SystemFontEnumerableSorter<TElement> GetEnumerableSorter(SystemFontEnumerableSorter<TElement> next)
	{
		SystemFontEnumerableSorter<TElement> systemFontEnumerableSorter = new EnumerableSorter<TElement, TKey>(keySelector, comparer, descending, next);
		if (parent != null)
		{
			systemFontEnumerableSorter = parent.GetEnumerableSorter(systemFontEnumerableSorter);
		}
		return systemFontEnumerableSorter;
	}
}
