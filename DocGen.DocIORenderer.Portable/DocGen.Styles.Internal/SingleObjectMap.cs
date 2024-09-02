using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal sealed class SingleObjectMap : FrugalMapBase
{
	private Entry _loneEntry;

	public override int Count
	{
		get
		{
			if (int.MaxValue != _loneEntry.Key)
			{
				return 1;
			}
			return 0;
		}
	}

	public SingleObjectMap()
	{
		_loneEntry.Key = int.MaxValue;
		_loneEntry.Value = DependencyProperty.UnsetValue;
	}

	public override FrugalMapStoreState InsertEntry(int key, object value)
	{
		if (int.MaxValue == _loneEntry.Key || key == _loneEntry.Key)
		{
			_loneEntry.Key = key;
			_loneEntry.Value = value;
			return FrugalMapStoreState.Success;
		}
		return FrugalMapStoreState.ThreeObjectMap;
	}

	public override void RemoveEntry(int key)
	{
		if (key == _loneEntry.Key)
		{
			_loneEntry.Key = int.MaxValue;
			_loneEntry.Value = DependencyProperty.UnsetValue;
		}
	}

	public override object Search(int key)
	{
		if (key == _loneEntry.Key)
		{
			return _loneEntry.Value;
		}
		return DependencyProperty.UnsetValue;
	}

	public override void Sort()
	{
	}

	public override void GetKeyValuePair(int index, out int key, out object value)
	{
		if (index == 0)
		{
			value = _loneEntry.Value;
			key = _loneEntry.Key;
			return;
		}
		value = DependencyProperty.UnsetValue;
		key = int.MaxValue;
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		if (Count == 1)
		{
			callback(list, _loneEntry.Key, _loneEntry.Value);
		}
	}

	public override void Promote(FrugalMapBase newMap)
	{
		if (newMap.InsertEntry(_loneEntry.Key, _loneEntry.Value) != 0)
		{
			throw new ArgumentException("Promote", "newMap");
		}
	}
}
