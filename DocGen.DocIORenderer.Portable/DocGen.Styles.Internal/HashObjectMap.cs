using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class HashObjectMap : FrugalMapBase
{
	internal const int MINSIZE = 163;

	private static object NullValue = new object();

	internal Hashtable _entries;

	public override int Count => _entries.Count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		if (_entries == null)
		{
			_entries = new Hashtable(163);
		}
		_entries[key] = ((value != NullValue && value != null) ? value : NullValue);
		return FrugalMapStoreState.Success;
	}

	public override void RemoveEntry(int key)
	{
		_entries.Remove(key);
	}

	public override object Search(int key)
	{
		object obj = _entries[key];
		if (obj == NullValue || obj == null)
		{
			return DependencyProperty.UnsetValue;
		}
		return obj;
	}

	public override void Sort()
	{
	}

	public override void GetKeyValuePair(int index, out int key, out object value)
	{
		if (index < _entries.Count)
		{
			IDictionaryEnumerator enumerator = _entries.GetEnumerator();
			enumerator.MoveNext();
			for (int i = 0; i < index; i++)
			{
				enumerator.MoveNext();
			}
			key = (int)enumerator.Key;
			if (enumerator.Value != NullValue && enumerator.Value != null)
			{
				value = enumerator.Value;
			}
			else
			{
				value = DependencyProperty.UnsetValue;
			}
			return;
		}
		value = DependencyProperty.UnsetValue;
		key = int.MaxValue;
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		IDictionaryEnumerator enumerator = _entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = (int)enumerator.Key;
			object value = ((enumerator.Value == NullValue || enumerator.Value == null) ? DependencyProperty.UnsetValue : enumerator.Value);
			callback(list, key, value);
		}
	}

	public override void Promote(FrugalMapBase newMap)
	{
		throw new InvalidOperationException("Promote");
	}
}
