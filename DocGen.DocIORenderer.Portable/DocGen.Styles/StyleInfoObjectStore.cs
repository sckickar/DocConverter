using System;
using System.Collections;
using DocGen.Styles.Internal;

namespace DocGen.Styles;

[Serializable]
internal class StyleInfoObjectStore : IDisposable
{
	private static int currentKey;

	private FrugalMap frugalMap;

	private object this[int key]
	{
		get
		{
			return frugalMap[key];
		}
		set
		{
			frugalMap[key] = value;
		}
	}

	public int Count => frugalMap.Count;

	static StyleInfoObjectStore()
	{
	}

	public StyleInfoObjectStore()
	{
		frugalMap = default(FrugalMap);
	}

	public void Dispose()
	{
		ArrayList list = new ArrayList();
		Iterate(list, DisposeCallback);
		GC.SuppressFinalize(this);
	}

	private void DisposeCallback(ArrayList list, int key, object value)
	{
		_Dispose(value);
	}

	private void _Dispose(object obj)
	{
		if (obj is StyleInfoSubObjectBase)
		{
			((StyleInfoSubObjectBase)obj).Dispose();
		}
	}

	public object GetObject(int key)
	{
		object obj = this[key];
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = null;
		}
		return obj;
	}

	public bool ContainsObject(int key)
	{
		return this[key] != DependencyProperty.UnsetValue;
	}

	public static int CreateKey()
	{
		return currentKey++;
	}

	public object GetObject(int key, out bool found)
	{
		object obj = this[key];
		found = true;
		if (obj == DependencyProperty.UnsetValue)
		{
			found = false;
			obj = null;
		}
		return obj;
	}

	public void SetObject(int key, object value)
	{
		this[key] = value;
	}

	public void RemoveObject(int key)
	{
		this[key] = DependencyProperty.UnsetValue;
	}

	public void GetKeyValuePair(int index, out int key, out object value)
	{
		frugalMap.GetKeyValuePair(index, out key, out value);
	}

	internal void Iterate(ArrayList list, FrugalMapIterationCallback callback)
	{
		frugalMap.Iterate(list, callback);
	}

	public void Sort()
	{
		frugalMap.Sort();
	}
}
