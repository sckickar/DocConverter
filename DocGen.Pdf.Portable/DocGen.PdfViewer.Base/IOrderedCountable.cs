using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

public interface IOrderedCountable<TElement> : IEnumerable<TElement>, IEnumerable
{
	IOrderedCountable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
}
