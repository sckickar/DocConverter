using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class SortedObjectMap : FrugalMapBase
{
	private const int MINSIZE = 16;

	private const int MAXSIZE = 128;

	private const int GROWTH = 8;

	internal int _count;

	private int _lastKey = int.MaxValue;

	private Entry[] _entries;

	public override int Count => _count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		bool found;
		int num = FindInsertIndex(key, out found);
		if (found)
		{
			_entries[num].Value = value;
			return FrugalMapStoreState.Success;
		}
		if (128 > _count)
		{
			if (_entries != null)
			{
				if (_entries.Length <= _count)
				{
					Entry[] array = new Entry[_entries.Length + 8];
					Array.Copy(_entries, 0, array, 0, _entries.Length);
					_entries = array;
				}
			}
			else
			{
				_entries = new Entry[16];
			}
			if (num < _count)
			{
				Array.Copy(_entries, num, _entries, num + 1, _count - num);
			}
			else
			{
				_lastKey = key;
			}
			_entries[num].Key = key;
			_entries[num].Value = value;
			_count++;
			return FrugalMapStoreState.Success;
		}
		return FrugalMapStoreState.Hashtable;
	}

	public override void RemoveEntry(int key)
	{
		bool found;
		int num = FindInsertIndex(key, out found);
		if (found)
		{
			int num2 = _count - num - 1;
			if (num2 > 0)
			{
				Array.Copy(_entries, num + 1, _entries, num, num2);
			}
			else if (_count > 1)
			{
				_lastKey = _entries[_count - 2].Key;
			}
			else
			{
				_lastKey = int.MaxValue;
			}
			_entries[_count - 1].Key = int.MaxValue;
			_entries[_count - 1].Value = DependencyProperty.UnsetValue;
			_count--;
		}
	}

	public override object Search(int key)
	{
		bool found;
		int num = FindInsertIndex(key, out found);
		if (found)
		{
			return _entries[num].Value;
		}
		return DependencyProperty.UnsetValue;
	}

	public override void Sort()
	{
	}

	public override void GetKeyValuePair(int index, out int key, out object value)
	{
		if (index < _count)
		{
			value = _entries[index].Value;
			key = _entries[index].Key;
			return;
		}
		value = DependencyProperty.UnsetValue;
		key = int.MaxValue;
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		if (_count > 0)
		{
			for (int i = 0; i < _count; i++)
			{
				callback(list, _entries[i].Key, _entries[i].Value);
			}
		}
	}

	public override void Promote(FrugalMapBase newMap)
	{
		for (int i = 0; i < _entries.Length; i++)
		{
			if (newMap.InsertEntry(_entries[i].Key, _entries[i].Value) != 0)
			{
				throw new ArgumentException("Promote", "newMap");
			}
		}
	}

	private int FindInsertIndex(int key, out bool found)
	{
		int num = 0;
		if (_count > 0 && key <= _lastKey)
		{
			int num2 = _count - 1;
			do
			{
				int num3 = (num2 + num) / 2;
				if (key <= _entries[num3].Key)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
			while (num < num2);
			found = key == _entries[num].Key;
		}
		else
		{
			num = _count;
			found = false;
		}
		return num;
	}
}
