using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal interface ISystemFontOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
{
	ISystemFontOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
}
