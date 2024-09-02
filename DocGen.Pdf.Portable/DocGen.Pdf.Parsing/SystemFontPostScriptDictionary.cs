using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontPostScriptDictionary : SystemFontPostScriptObject, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private readonly Dictionary<string, object> store;

	public ICollection<string> Keys => store.Keys;

	public ICollection<object> Values => store.Values;

	public object this[string key]
	{
		get
		{
			return store[key];
		}
		set
		{
			store[key] = value;
		}
	}

	public int Count => store.Count;

	public bool IsReadOnly => false;

	public SystemFontPostScriptDictionary()
	{
		store = new Dictionary<string, object>();
	}

	public SystemFontPostScriptDictionary(int capacity)
	{
		store = new Dictionary<string, object>(capacity);
	}

	public void Add(string key, object value)
	{
		store.Add(key, value);
	}

	public bool ContainsKey(string key)
	{
		return store.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		return store.Remove(key);
	}

	public bool TryGetValue(string key, out object value)
	{
		return store.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<string, object> item)
	{
		store[item.Key] = item.Value;
	}

	public void Clear()
	{
		store.Clear();
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
		return store.Remove(item.Key);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return store.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return store.GetEnumerator();
	}

	public T GetElementAs<T>(string key)
	{
		return (T)store[key];
	}
}
