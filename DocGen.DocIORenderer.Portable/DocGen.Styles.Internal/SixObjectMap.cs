using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class SixObjectMap : FrugalMapBase
{
	private const int SIZE = 6;

	private ushort _count;

	private bool _sorted;

	private Entry _entry0;

	private Entry _entry1;

	private Entry _entry2;

	private Entry _entry3;

	private Entry _entry4;

	private Entry _entry5;

	public override int Count => _count;

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		if (_count > 0)
		{
			if (_entry0.Key == key)
			{
				_entry0.Value = value;
				return FrugalMapStoreState.Success;
			}
			if (_count > 1)
			{
				if (_entry1.Key == key)
				{
					_entry1.Value = value;
					return FrugalMapStoreState.Success;
				}
				if (_count > 2)
				{
					if (_entry2.Key == key)
					{
						_entry2.Value = value;
						return FrugalMapStoreState.Success;
					}
					if (_count > 3)
					{
						if (_entry3.Key == key)
						{
							_entry3.Value = value;
							return FrugalMapStoreState.Success;
						}
						if (_count > 4)
						{
							if (_entry4.Key == key)
							{
								_entry4.Value = value;
								return FrugalMapStoreState.Success;
							}
							if (_count > 5 && _entry5.Key == key)
							{
								_entry5.Value = value;
								return FrugalMapStoreState.Success;
							}
						}
					}
				}
			}
		}
		if (6 > _count)
		{
			_sorted = false;
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
				break;
			case 2:
				_entry2.Key = key;
				_entry2.Value = value;
				break;
			case 3:
				_entry3.Key = key;
				_entry3.Value = value;
				break;
			case 4:
				_entry4.Key = key;
				_entry4.Value = value;
				break;
			case 5:
				_entry5.Key = key;
				_entry5.Value = value;
				break;
			}
			_count++;
			return FrugalMapStoreState.Success;
		}
		return FrugalMapStoreState.Array;
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
		case 4:
			if (_entry0.Key == key)
			{
				_entry0 = _entry1;
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3.Key = int.MaxValue;
				_entry3.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry1.Key == key)
			{
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3.Key = int.MaxValue;
				_entry3.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry2.Key == key)
			{
				_entry2 = _entry3;
				_entry3.Key = int.MaxValue;
				_entry3.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry3.Key == key)
			{
				_entry3.Key = int.MaxValue;
				_entry3.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			break;
		case 5:
			if (_entry0.Key == key)
			{
				_entry0 = _entry1;
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4.Key = int.MaxValue;
				_entry4.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry1.Key == key)
			{
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4.Key = int.MaxValue;
				_entry4.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry2.Key == key)
			{
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4.Key = int.MaxValue;
				_entry4.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry3.Key == key)
			{
				_entry3 = _entry4;
				_entry4.Key = int.MaxValue;
				_entry4.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry4.Key == key)
			{
				_entry4.Key = int.MaxValue;
				_entry4.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			break;
		case 6:
			if (_entry0.Key == key)
			{
				_entry0 = _entry1;
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4 = _entry5;
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry1.Key == key)
			{
				_entry1 = _entry2;
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4 = _entry5;
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry2.Key == key)
			{
				_entry2 = _entry3;
				_entry3 = _entry4;
				_entry4 = _entry5;
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry3.Key == key)
			{
				_entry3 = _entry4;
				_entry4 = _entry5;
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry4.Key == key)
			{
				_entry4 = _entry5;
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
				_count--;
			}
			else if (_entry5.Key == key)
			{
				_entry5.Key = int.MaxValue;
				_entry5.Value = DependencyProperty.UnsetValue;
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
				if (_count > 2)
				{
					if (_entry2.Key == key)
					{
						return _entry2.Value;
					}
					if (_count > 3)
					{
						if (_entry3.Key == key)
						{
							return _entry3.Value;
						}
						if (_count > 4)
						{
							if (_entry4.Key == key)
							{
								return _entry4.Value;
							}
							if (_count > 5 && _entry5.Key == key)
							{
								return _entry5.Value;
							}
						}
					}
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
		bool flag;
		do
		{
			flag = false;
			if (_entry0.Key > _entry1.Key)
			{
				Entry entry = _entry0;
				_entry0 = _entry1;
				_entry1 = entry;
				flag = true;
			}
			if (_count <= 2)
			{
				continue;
			}
			if (_entry1.Key > _entry2.Key)
			{
				Entry entry = _entry1;
				_entry1 = _entry2;
				_entry2 = entry;
				flag = true;
			}
			if (_count <= 3)
			{
				continue;
			}
			if (_entry2.Key > _entry3.Key)
			{
				Entry entry = _entry2;
				_entry2 = _entry3;
				_entry3 = entry;
				flag = true;
			}
			if (_count > 4)
			{
				if (_entry3.Key > _entry4.Key)
				{
					Entry entry = _entry3;
					_entry3 = _entry4;
					_entry4 = entry;
					flag = true;
				}
				if (_count > 5 && _entry4.Key > _entry5.Key)
				{
					Entry entry = _entry4;
					_entry4 = _entry5;
					_entry5 = entry;
					flag = true;
				}
			}
		}
		while (flag);
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
			case 3:
				key = _entry3.Key;
				value = _entry3.Value;
				break;
			case 4:
				key = _entry4.Key;
				value = _entry4.Value;
				break;
			case 5:
				key = _entry5.Key;
				value = _entry5.Value;
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
			if (_count >= 3)
			{
				callback(list, _entry2.Key, _entry2.Value);
			}
			if (_count >= 4)
			{
				callback(list, _entry3.Key, _entry3.Value);
			}
			if (_count >= 5)
			{
				callback(list, _entry4.Key, _entry4.Value);
			}
			if (_count == 6)
			{
				callback(list, _entry5.Key, _entry5.Value);
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
		if (newMap.InsertEntry(_entry3.Key, _entry3.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
		if (newMap.InsertEntry(_entry4.Key, _entry4.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
		if (newMap.InsertEntry(_entry5.Key, _entry5.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
	}
}
