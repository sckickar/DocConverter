using System;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class OrderedEnumerable<TElement, TKey> : OrderedCountable<TElement>
{
	internal OrderedCountable<TElement> parent;

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
		}
	}

	internal override CountableSortingHelper<TElement> GetEnumerableSorter(CountableSortingHelper<TElement> next)
	{
		CountableSortingHelper<TElement> countableSortingHelper = new EnumerableSorter<TElement, TKey>(keySelector, comparer, descending, next);
		if (parent != null)
		{
			countableSortingHelper = parent.GetEnumerableSorter(countableSortingHelper);
		}
		return countableSortingHelper;
	}
}
