using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class ArrayObjectMap : FrugalMapBase
{
	private const int MINSIZE = 9;

	private const int MAXSIZE = 15;

	private const int GROWTH = 3;

	private ushort _count;

	private bool _sorted;

	private Entry[] _entries;

	public override int Count => _count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_entries[i].Key == key)
			{
				_entries[i].Value = value;
				return FrugalMapStoreState.Success;
			}
		}
		if (15 > _count)
		{
			if (_entries != null)
			{
				_sorted = false;
				if (_entries.Length <= _count)
				{
					Entry[] array = new Entry[_entries.Length + 3];
					Array.Copy(_entries, 0, array, 0, _entries.Length);
					_entries = array;
				}
			}
			else
			{
				_entries = new Entry[9];
				_sorted = true;
			}
			_entries[_count].Key = key;
			_entries[_count].Value = value;
			_count++;
			return FrugalMapStoreState.Success;
		}
		return FrugalMapStoreState.SortedArray;
	}

	public override void RemoveEntry(int key)
	{
		for (int i = 0; i < _count; i++)
		{
			if (_entries[i].Key == key)
			{
				int num = _count - i - 1;
				if (num > 0)
				{
					Array.Copy(_entries, i + 1, _entries, i, num);
				}
				_entries[_count - 1].Key = int.MaxValue;
				_entries[_count - 1].Value = DependencyProperty.UnsetValue;
				_count--;
				break;
			}
		}
	}

	public override object Search(int key)
	{
		for (int i = 0; i < _count; i++)
		{
			if (key == _entries[i].Key)
			{
				return _entries[i].Value;
			}
		}
		return DependencyProperty.UnsetValue;
	}

	public override void Sort()
	{
		if (!_sorted && _count > 1)
		{
			QSort(0, _count - 1);
			_sorted = true;
		}
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

	private int Compare(int left, int right)
	{
		return _entries[left].Key - _entries[right].Key;
	}

	private int Partition(int left, int right)
	{
		int num = left - 1;
		int num2 = right;
		Entry entry;
		while (true)
		{
			if (Compare(++num, right) >= 0)
			{
				while (Compare(right, --num2) < 0 && num2 != left)
				{
				}
				if (num >= num2)
				{
					break;
				}
				entry = _entries[num2];
				_entries[num2] = _entries[num];
				_entries[num] = entry;
			}
		}
		entry = _entries[right];
		_entries[right] = _entries[num];
		_entries[num] = entry;
		return num;
	}

	private void QSort(int left, int right)
	{
		if (left < right)
		{
			int num = Partition(left, right);
			QSort(left, num - 1);
			QSort(num + 1, right);
		}
	}
}
