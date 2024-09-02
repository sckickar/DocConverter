using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class SFArrayList<T> : List<T>, ICloneable where T : class
{
	public new T this[int index]
	{
		get
		{
			if (index < base.Count && index >= 0)
			{
				return base[index];
			}
			return null;
		}
		set
		{
			EnsureCount(index + 1);
			base[index] = value;
		}
	}

	public SFArrayList()
	{
	}

	public SFArrayList(ICollection<T> c)
		: base((IEnumerable<T>)c)
	{
	}

	public object Clone()
	{
		SFArrayList<T> sFArrayList = new SFArrayList<T>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			T item = ((current is ICloneable) ? ((T)((ICloneable)current).Clone()) : current);
			sFArrayList.Add(item);
		}
		return sFArrayList;
	}

	public object Clone(object parent)
	{
		SFArrayList<T> sFArrayList = new SFArrayList<T>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T val = enumerator.Current;
			if (val is ICloneParent cloneParent)
			{
				val = (T)cloneParent.Clone(parent);
			}
			sFArrayList.Add(val);
		}
		return sFArrayList;
	}

	public void EnsureCount(int value)
	{
		int count = base.Count;
		if (count < value)
		{
			AddRange(new T[value - count]);
		}
	}
}
