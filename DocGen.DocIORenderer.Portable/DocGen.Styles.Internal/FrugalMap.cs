using System;
using System.Collections;

namespace DocGen.Styles.Internal;

internal struct FrugalMap
{
	internal FrugalMapBase _mapStore;

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
					_mapStore = new SingleObjectMap();
				}
				FrugalMapStoreState frugalMapStoreState = _mapStore.InsertEntry(key, value);
				if (frugalMapStoreState == FrugalMapStoreState.Success)
				{
					return;
				}
				FrugalMapBase frugalMapBase;
				if (FrugalMapStoreState.ThreeObjectMap == frugalMapStoreState)
				{
					frugalMapBase = new ThreeObjectMap();
				}
				else if (FrugalMapStoreState.SixObjectMap == frugalMapStoreState)
				{
					frugalMapBase = new SixObjectMap();
				}
				else if (FrugalMapStoreState.Array == frugalMapStoreState)
				{
					frugalMapBase = new ArrayObjectMap();
				}
				else if (FrugalMapStoreState.SortedArray == frugalMapStoreState)
				{
					frugalMapBase = new SortedObjectMap();
				}
				else
				{
					if (FrugalMapStoreState.Hashtable != frugalMapStoreState)
					{
						throw new InvalidOperationException("Promote");
					}
					frugalMapBase = new HashObjectMap();
				}
				_mapStore.Promote(frugalMapBase);
				_mapStore = frugalMapBase;
				_mapStore.InsertEntry(key, value);
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
