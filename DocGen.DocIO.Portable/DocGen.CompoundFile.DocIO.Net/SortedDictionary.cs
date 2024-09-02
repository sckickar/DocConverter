using System;

namespace DocGen.CompoundFile.DocIO.Net;

internal class SortedDictionary<TKey, TValue> : TypedSortedListEx<TKey, TValue> where TKey : IComparable
{
}
