using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal static class Countable
{
	public static int Count<TSource>(IEnumerable<TSource> source)
	{
		if (source is ICollection<TSource> collection)
		{
			return collection.Count;
		}
		if (source is ICollection collection2)
		{
			return collection2.Count;
		}
		int num = 0;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
		}
		return num;
	}

	public static bool Any<TSource>(IEnumerable<TSource> source)
	{
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return true;
			}
		}
		return false;
	}

	public static bool All<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Any<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<TResult> OfType<TResult>(IEnumerable source)
	{
		return OfTypeIterator<TResult>(source);
	}

	private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source)
	{
		IEnumerator enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			if (current is TResult)
			{
				yield return (TResult)current;
			}
		}
	}

	public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source)
	{
		if (source is IList<TSource> list)
		{
			if (list.Count > 0)
			{
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return default(TSource);
	}

	public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source != null)
		{
			if (predicate != null)
			{
				IEnumerator<TSource> enumerator = source.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						TSource current = enumerator.Current;
						if (predicate(current))
						{
							return current;
						}
					}
					return default(TSource);
				}
			}
			throw new Exception("Reference Exception");
		}
		throw new Exception("Null Reference");
	}

	public static TSource First<TSource>(IEnumerable<TSource> source)
	{
		if (source is IList<TSource> list)
		{
			if (list.Count > 0)
			{
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		throw new Exception("Null Reference Exception");
	}

	public static TSource First<TSource>(IEnumerable<TSource> source, Func<TSource, bool> selector)
	{
		return First(Where(source, selector));
	}

	public static bool Contains<TSource>(IEnumerable<TSource> source, TSource value)
	{
		if (source is ICollection<TSource> collection)
		{
			return collection.Contains(value);
		}
		return Contains(source, value, null);
	}

	public static bool Contains<TSource>(IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		foreach (TSource item in source)
		{
			if (comparer.Equals(item, value))
			{
				return true;
			}
		}
		return false;
	}

	public static TSource[] ToArray<TSource>(IEnumerable<TSource> source)
	{
		return new Buffer<TSource>(source).ToArray();
	}

	public static List<TSource> ToList<TSource>(IEnumerable<TSource> source)
	{
		return new List<TSource>(source);
	}

	public static IEnumerable<TResult> Empty<TResult>()
	{
		return BlankEnumerable<TResult>.Instance;
	}

	public static byte Max(IEnumerable<byte> source)
	{
		byte b = 0;
		foreach (byte item in source)
		{
			if (b < item)
			{
				b = item;
			}
		}
		return b;
	}

	public static byte Max<TSource>(IEnumerable<TSource> source, Func<TSource, byte> selector)
	{
		return Max(Select(source, selector));
	}

	public static double Sum(IEnumerable<double> source)
	{
		double num = 0.0;
		foreach (double item in source)
		{
			num += item;
		}
		return num;
	}

	public static int Sum(IEnumerable<int> source)
	{
		if (source != null)
		{
			int num = 0;
			{
				foreach (int item in source)
				{
					num += item;
				}
				return num;
			}
		}
		throw new Exception("Null Reference");
	}

	public static int Sum<TSource>(IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		return Sum(Select(source, selector));
	}

	public static IEnumerable<TSource> Skip<TSource>(IEnumerable<TSource> source, int count)
	{
		IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (count-- > 0 && enumerator.MoveNext())
		{
		}
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		foreach (TSource item in source)
		{
			yield return selector(item);
		}
	}

	public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<TSource> Take<TSource>(IEnumerable<TSource> source, int count)
	{
		if (source != null)
		{
			return TakeIterator(source, count);
		}
		throw new Exception("Null Reference");
	}

	private static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count)
	{
		if (count <= 0)
		{
			yield break;
		}
		foreach (TSource item in source)
		{
			yield return item;
			int num = count - 1;
			count = num;
			if (num == 0)
			{
				break;
			}
		}
	}

	private static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		foreach (TSource item in first)
		{
			yield return item;
		}
		foreach (TSource item2 in second)
		{
			yield return item2;
		}
	}

	public static TSource Last<TSource>(IEnumerable<TSource> source)
	{
		if (!(source is IList<TSource> { Count: var count } list))
		{
			IEnumerator<TSource> enumerator = source.GetEnumerator();
			using (enumerator)
			{
				TSource current;
				do
				{
					current = enumerator.Current;
				}
				while (enumerator.MoveNext());
				return current;
			}
		}
		if (count > 0)
		{
			return list[count - 1];
		}
		throw new Exception("Null Reference");
	}

	public static IOrderedCountable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, descending: false);
	}

	public static IOrderedCountable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, descending: false);
	}

	public static TSource ElementAt<TSource>(IEnumerable<TSource> source, int index)
	{
		IList<TSource> list = source as IList<TSource>;
		if (list == null && index >= 0)
		{
			IEnumerator<TSource> enumerator = source.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					if (index == 0)
					{
						return enumerator.Current;
					}
					index--;
				}
			}
		}
		return list[index];
	}

	public static TSource LastOrDefault<TSource>(IEnumerable<TSource> source)
	{
		TSource val = default(TSource);
		if (!(source is IList<TSource> { Count: var count } list))
		{
			IEnumerator<TSource> enumerator = source.GetEnumerator();
			using (enumerator)
			{
				if (!enumerator.MoveNext())
				{
					return default(TSource);
				}
				TSource current;
				do
				{
					current = enumerator.Current;
				}
				while (enumerator.MoveNext());
				return current;
			}
		}
		if (count > 0)
		{
			return list[count - 1];
		}
		return default(TSource);
	}

	public static TSource LastOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		TSource result = default(TSource);
		if (predicate != null)
		{
			foreach (TSource item in source)
			{
				if (predicate(item))
				{
					result = item;
				}
			}
			return result;
		}
		throw new Exception("Null Reference");
	}

	public static TSource SingleOrDefault<TSource>(IEnumerable<TSource> source)
	{
		if (!(source is IList<TSource> { Count: var count } list))
		{
			IEnumerator<TSource> enumerator = source.GetEnumerator();
			TSource result;
			using (enumerator)
			{
				if (enumerator.MoveNext())
				{
					result = enumerator.Current;
					return result;
				}
				result = default(TSource);
			}
			return result;
		}
		return count switch
		{
			0 => default(TSource), 
			1 => list[0], 
			_ => throw new Exception("Null Reference"), 
		};
	}

	public static TSource SingleOrDefault<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		TSource result = default(TSource);
		if (predicate != null)
		{
			long num = 0L;
			foreach (TSource item in source)
			{
				if (predicate(item))
				{
					result = item;
					num++;
				}
			}
			long num2 = num;
			if (num2 <= 1 && num2 >= 0)
			{
				switch ((int)num2)
				{
				case 0:
					return default(TSource);
				case 1:
					return result;
				}
			}
			throw new Exception("Null Reference");
		}
		throw new Exception("Null Reference");
	}
}
