using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal struct InsertionSortMap
{
	internal LargeSortedObjectMap _mapStore;

	public object this[int key]
	{
		get
		{
			if (_mapStore != null)
			{
				return _mapStore.Search(key);
			}
			return DependencyProperty.UnsetValue;
		}
		set
		{
			if (value != DependencyProperty.UnsetValue)
			{
				if (_mapStore == null)
				{
					_mapStore = new LargeSortedObjectMap();
				}
				FrugalMapStoreState frugalMapStoreState = _mapStore.InsertEntry(key, value);
				if (frugalMapStoreState != 0)
				{
					if (FrugalMapStoreState.SortedArray != frugalMapStoreState)
					{
						throw new InvalidOperationException("Promote");
					}
					LargeSortedObjectMap largeSortedObjectMap = new LargeSortedObjectMap();
					_mapStore.Promote(largeSortedObjectMap);
					_mapStore = largeSortedObjectMap;
					_mapStore.InsertEntry(key, value);
				}
			}
			else if (_mapStore != null)
			{
				_mapStore.RemoveEntry(key);
				if (_mapStore.Count == 0)
				{
					_mapStore = null;
				}
			}
		}
	}

	public int Count
	{
		get
		{
			if (_mapStore != null)
			{
				return _mapStore.Count;
			}
			return 0;
		}
	}

	public void Sort()
	{
		if (_mapStore != null)
		{
			_mapStore.Sort();
		}
	}

	public void GetKeyValuePair(int index, out int key, out object value)
	{
		if (_mapStore != null)
		{
			_mapStore.GetKeyValuePair(index, out key, out value);
			return;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		if (callback != null)
		{
			if (list != null)
			{
				if (_mapStore != null)
				{
					_mapStore.Iterate(list, callback);
				}
				return;
			}
			throw new ArgumentNullException("list");
		}
		throw new ArgumentNullException("callback");
	}
}
