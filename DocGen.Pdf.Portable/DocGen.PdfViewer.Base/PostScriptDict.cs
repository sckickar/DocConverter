using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class PostScriptDict : PostScriptObj, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private readonly Dictionary<string, object> collection;

	public ICollection<string> Keys => collection.Keys;

	public ICollection<object> Values => collection.Values;

	public object this[string key]
	{
		get
		{
			return collection[key];
		}
		set
		{
			collection[key] = value;
		}
	}

	public int Count => collection.Count;

	public bool IsReadOnly => false;

	public PostScriptDict()
	{
		collection = new Dictionary<string, object>();
	}

	public PostScriptDict(int capacity)
	{
		collection = new Dictionary<string, object>(capacity);
	}

	public void Add(string key, object value)
	{
		collection.Add(key, value);
	}

	public bool ContainsKey(string key)
	{
		return collection.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		return collection.Remove(key);
	}

	public bool TryGetValue(string key, out object value)
	{
		return collection.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<string, object> item)
	{
		collection[item.Key] = item.Value;
	}

	public void Clear()
	{
		collection.Clear();
	}

	public bool Contains(KeyValuePair<string, object> item)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		return collection.Remove(item.Key);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return collection.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return collection.GetEnumerator();
	}

	public T GetElementAs<T>(string key)
	{
		if (collection.ContainsKey(key))
		{
			return (T)collection[key];
		}
		if (key == "middot" && collection.ContainsKey("periodcentered"))
		{
			return (T)collection["periodcentered"];
		}
		return (T)(object)null;
	}
}
