using System;

namespace DocGen.DocIO.DLS;

public class SortedDictionary<TKey, TValue> : TypedSortedListEx<TKey, TValue> where TKey : IComparable
{
}
