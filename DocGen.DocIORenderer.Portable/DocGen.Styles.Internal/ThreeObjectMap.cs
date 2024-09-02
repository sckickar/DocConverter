using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class ThreeObjectMap : FrugalMapBase
{
	private const int SIZE = 3;

	private ushort _count;

	private bool _sorted;

	private Entry _entry0;

	private Entry _entry1;

	private Entry _entry2;

	public override int Count => _count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		switch (_count)
		{
		case 1:
			if (_entry0.Key == key)
			{
				_entry0.Value = value;
				return FrugalMapStoreState.Success;
			}
			break;
		case 2:
			if (_entry0.Key == key)
			{
				_entry0.Value = value;
				return FrugalMapStoreState.Success;
			}
			if (_entry1.Key == key)
			{
				_entry1.Value = value;
				return FrugalMapStoreState.Success;
			}
			break;
		case 3:
			if (_entry0.Key == key)
			{
				_entry0.Value = value;
				return FrugalMapStoreState.Success;
			}
			if (_entry1.Key == key)
			{
				_entry1.Value = value;
				return FrugalMapStoreState.Success;
			}
			if (_entry2.Key == key)
			{
				_entry2.Value = value;
				return FrugalMapStoreState.Success;
			}
			break;
		}
		if (3 > _count)
		{
			switch (_count)
			{
			case 0:
				_entry0.Key = key;
				_entry0.Value = value;
				_sorted = true;
				break;
			case 1:
				_entry1.Key = key;
				_entry1.Value = value;
				_sorted = false;
				break;
			case 2:
				_entry2.Key = key;
				_entry2.Value = value;
				_sorted = false;
				break;
			}
			_count++;
			return FrugalMapStoreState.Success;
		}
		return FrugalMapStoreState.SixObjectMap;
	}

	public override void RemoveEntry(int key)
	{
		switch (_count)
		{
		case 1:
			if (_entry0.Key == key)
			{
				_entry0.Key = int.MaxValue;
				_entry0.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			break;
		case 2:
			if (_entry0.Key == key)
			{
				_entry0 = _entry1;
				_entry1.Key = int.MaxValue;
				_entry1.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry1.Key == key)
			{
				_entry1.Key = int.MaxValue;
				_entry1.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			break;
		case 3:
			if (_entry0.Key == key)
			{
				_entry0 = _entry1;
				_entry1 = _entry2;
				_entry2.Key = int.MaxValue;
				_entry2.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry1.Key == key)
			{
				_entry1 = _entry2;
				_entry2.Key = int.MaxValue;
				_entry2.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry2.Key == key)
			{
				_entry2.Key = int.MaxValue;
				_entry2.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			break;
		}
	}

	public override object Search(int key)
	{
		if (_count > 0)
		{
			if (_entry0.Key == key)
			{
				return _entry0.Value;
			}
			if (_count > 1)
			{
				if (_entry1.Key == key)
				{
					return _entry1.Value;
				}
				if (_count > 2 && _entry2.Key == key)
				{
					return _entry2.Value;
				}
			}
		}
		return DependencyProperty.UnsetValue;
	}

	public override void Sort()
	{
		if (_sorted || _count <= 1)
		{
			return;
		}
		if (_entry0.Key > _entry1.Key)
		{
			Entry entry = _entry0;
			_entry0 = _entry1;
			_entry1 = entry;
		}
		if (_count > 2 && _entry1.Key > _entry2.Key)
		{
			Entry entry = _entry1;
			_entry1 = _entry2;
			_entry2 = entry;
			if (_entry0.Key > _entry1.Key)
			{
				entry = _entry0;
				_entry0 = _entry1;
				_entry1 = entry;
			}
		}
		_sorted = true;
	}

	public override void GetKeyValuePair(int index, out int key, out object value)
	{
		if (index < _count)
		{
			switch (index)
			{
			case 0:
				key = _entry0.Key;
				value = _entry0.Value;
				break;
			case 1:
				key = _entry1.Key;
				value = _entry1.Value;
				break;
			case 2:
				key = _entry2.Key;
				value = _entry2.Value;
				break;
			default:
				key = int.MaxValue;
				value = DependencyProperty.UnsetValue;
				break;
			}
			return;
		}
		key = int.MaxValue;
		value = DependencyProperty.UnsetValue;
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		if (_count > 0)
		{
			if (_count >= 1)
			{
				callback(list, _entry0.Key, _entry0.Value);
			}
			if (_count >= 2)
			{
				callback(list, _entry1.Key, _entry1.Value);
			}
			if (_count == 3)
			{
				callback(list, _entry2.Key, _entry2.Value);
			}
		}
	}

	public override void Promote(FrugalMapBase newMap)
	{
		if (newMap.InsertEntry(_entry0.Key, _entry0.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
		if (newMap.InsertEntry(_entry1.Key, _entry1.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
		if (newMap.InsertEntry(_entry2.Key, _entry2.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
	}
}
