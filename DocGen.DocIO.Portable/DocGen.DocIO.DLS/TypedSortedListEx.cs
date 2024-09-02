using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.DLS;

public class TypedSortedListEx<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection where TKey : IComparable
{
	private class TypedSortedListExEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable, IDocIOCloneable
	{
		private TypedSortedListEx<TKey, TValue> m_list;

		private TKey m_key;

		private TValue m_value;

		private int m_iIndex;

		private int m_iStartIndex;

		private int m_iEndIndex;

		private int m_iVersion;

		private bool m_bCurrent;

		public virtual TKey Key
		{
			get
			{
				if (m_iVersion != m_list.m_iVersion)
				{
					throw new InvalidOperationException();
				}
				if (!m_bCurrent)
				{
					throw new InvalidOperationException();
				}
				return m_key;
			}
		}

		public virtual KeyValuePair<TKey, TValue> Entry
		{
			get
			{
				if (m_iVersion != m_list.m_iVersion)
				{
					throw new InvalidOperationException();
				}
				if (!m_bCurrent)
				{
					throw new InvalidOperationException();
				}
				return new KeyValuePair<TKey, TValue>(m_key, m_value);
			}
		}

		public virtual KeyValuePair<TKey, TValue> Current
		{
			get
			{
				if (!m_bCurrent)
				{
					throw new InvalidOperationException();
				}
				return new KeyValuePair<TKey, TValue>(m_key, m_value);
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (!m_bCurrent)
				{
					throw new InvalidOperationException();
				}
				return new KeyValuePair<TKey, TValue>(m_key, m_value);
			}
		}

		public virtual object Value
		{
			get
			{
				if (m_iVersion != m_list.m_iVersion)
				{
					throw new InvalidOperationException();
				}
				if (!m_bCurrent)
				{
					throw new InvalidOperationException();
				}
				return m_value;
			}
		}

		internal TypedSortedListExEnumerator(TypedSortedListEx<TKey, TValue> list, int index, int count)
		{
			m_list = list;
			m_iIndex = index;
			m_iStartIndex = index;
			m_iEndIndex = index + count;
			m_iVersion = m_list.m_iVersion;
			m_bCurrent = false;
		}

		public void Dispose()
		{
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (m_iVersion != m_list.m_iVersion)
			{
				throw new InvalidOperationException();
			}
			if (m_iIndex < m_iEndIndex)
			{
				m_key = m_list.m_arrKeys[m_iIndex];
				m_value = m_list.m_dicValues[m_key];
				m_iIndex++;
				m_bCurrent = true;
				return true;
			}
			m_key = default(TKey);
			m_value = default(TValue);
			m_bCurrent = false;
			return false;
		}

		public virtual void Reset()
		{
			if (m_iVersion != m_list.m_iVersion)
			{
				throw new InvalidOperationException();
			}
			m_iIndex = m_iStartIndex;
			m_bCurrent = false;
			m_key = default(TKey);
			m_value = default(TValue);
		}
	}

	private class KeysEnumerator : IEnumerator<TKey>, IEnumerator, IDisposable
	{
		private TypedSortedListEx<TKey, TValue> m_list;

		private int m_iIndex = -1;

		private int m_iVersion;

		public TKey Current
		{
			get
			{
				if (m_iVersion != m_list.m_iVersion)
				{
					throw new InvalidOperationException("Parent collection was changed");
				}
				if (m_iIndex < 0 || m_iIndex >= m_list.m_iSize)
				{
					throw new InvalidOperationException();
				}
				return m_list.m_arrKeys[m_iIndex];
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (m_iIndex < 0 || m_iIndex >= m_list.m_iSize)
				{
					throw new InvalidOperationException();
				}
				if (m_iVersion != m_list.m_iVersion)
				{
					throw new InvalidOperationException("Parent collection was changed");
				}
				return m_list.m_arrKeys[m_iIndex];
			}
		}

		public KeysEnumerator(TypedSortedListEx<TKey, TValue> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			m_list = list;
			m_iVersion = m_list.m_iVersion;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (m_iVersion != m_list.m_iVersion)
			{
				throw new InvalidOperationException("Parent collection was changed");
			}
			if (m_iIndex < 0)
			{
				m_iIndex = 0;
			}
			else
			{
				m_iIndex++;
			}
			if (m_iIndex >= m_list.m_iSize)
			{
				m_iIndex = -1;
			}
			return m_iIndex >= 0;
		}

		public void Reset()
		{
			if (m_iVersion != m_list.m_iVersion)
			{
				throw new InvalidOperationException("Parent collection was changed");
			}
			m_iIndex = -1;
		}
	}

	private class KeyList : IList<TKey>, ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		private TypedSortedListEx<TKey, TValue> m_list;

		public virtual int Count => m_list.m_iSize;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => m_list.IsSynchronized;

		public virtual object SyncRoot => m_list.SyncRoot;

		public virtual TKey this[int index]
		{
			get
			{
				return m_list.GetKey(index);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal KeyList(TypedSortedListEx<TKey, TValue> list)
		{
			m_list = list;
		}

		public void Add(TKey key)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(TKey key)
		{
			if (m_list != null && key != null)
			{
				return m_list.ContainsKey(key);
			}
			return false;
		}

		public virtual void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentException("array");
			}
			Array.Copy(m_list.m_arrKeys, 0, array, arrayIndex, m_list.Count);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException("array");
			}
			Array.Copy(m_list.m_arrKeys, 0, array, arrayIndex, m_list.Count);
		}

		public virtual void Insert(int index, TKey value)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new KeysEnumerator(m_list);
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return new KeysEnumerator(m_list);
		}

		public virtual int IndexOf(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = Array.BinarySearch(m_list.m_arrKeys, 0, m_list.Count, key, m_list.m_comparer);
			if (num < 0)
			{
				return -1;
			}
			return num;
		}

		public bool Remove(TKey key)
		{
			throw new NotSupportedException();
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private class ValueList : IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		private TypedSortedListEx<TKey, TValue> m_list;

		private TValue[] vals;

		public virtual int Count => m_list.m_iSize;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => m_list.IsSynchronized;

		public virtual object SyncRoot => m_list.SyncRoot;

		public virtual TValue this[int index]
		{
			get
			{
				return m_list.GetByIndex(index);
			}
			set
			{
				m_list.SetByIndex(index, value);
			}
		}

		internal ValueList(TypedSortedListEx<TKey, TValue> list)
		{
			m_list = list;
			UpdateValues();
		}

		public virtual void UpdateValues()
		{
			int count = m_list.Count;
			vals = new TValue[count];
			m_list.m_dicValues.Values.CopyTo(vals, 0);
			TKey[] array = new TKey[count];
			m_list.m_dicValues.Keys.CopyTo(array, 0);
			SortDictionaryValues(array, vals);
		}

		private void SortDictionaryValues(TKey[] keys, TValue[] values)
		{
			for (int i = 0; i < keys.Length - 1; i++)
			{
				for (int j = i + 1; j < keys.Length; j++)
				{
					if (m_list.m_comparer.Compare(keys[i], keys[j]) > 0)
					{
						TKey val = keys[i];
						keys[i] = keys[j];
						keys[j] = val;
						TValue val2 = values[i];
						values[i] = values[j];
						values[j] = val2;
					}
				}
			}
		}

		public virtual void Add(TValue value)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(TValue value)
		{
			return m_list.ContainsValue(value);
		}

		public virtual void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException("arrray");
			}
			Array.Copy(vals, 0, array, arrayIndex, m_list.Count);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException();
			}
			if (m_list != null && array != null)
			{
				Array.Copy(vals, 0, array, arrayIndex, m_list.Count);
			}
		}

		public virtual void Insert(int index, TValue value)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerator<TValue> GetEnumerator()
		{
			return ((IEnumerable<TValue>)vals).GetEnumerator();
		}

		public virtual int IndexOf(TValue value)
		{
			return Array.IndexOf(vals, value, 0, m_list.Count);
		}

		public virtual bool Remove(TValue value)
		{
			throw new NotSupportedException();
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private const int DefaultCapacity = 16;

	private TKey[] m_arrKeys;

	private Dictionary<TKey, TValue> m_dicValues;

	private int m_iSize;

	private int m_iVersion;

	private IComparer<TKey> m_comparer;

	private KeyList m_listKeys;

	private ValueList m_lstValues;

	public virtual int Capacity
	{
		get
		{
			return m_arrKeys.Length;
		}
		set
		{
			if (value == m_arrKeys.Length)
			{
				return;
			}
			if (value < m_iSize)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value > 0)
			{
				TKey[] array = new TKey[value];
				if (m_iSize > 0)
				{
					Array.Copy(m_arrKeys, 0, array, 0, m_iSize);
				}
				m_arrKeys = array;
			}
			else
			{
				m_arrKeys = new TKey[16];
			}
		}
	}

	public virtual int Count => m_iSize;

	public virtual IList<TKey> Keys => GetKeyList();

	public virtual IList<TValue> Values => GetValueList();

	public virtual bool IsReadOnly => false;

	public virtual bool IsFixedSize => false;

	public virtual bool IsSynchronized => false;

	public virtual object SyncRoot => this;

	public virtual TValue this[TKey key]
	{
		get
		{
			m_dicValues.TryGetValue(key, out var value);
			return value;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (m_dicValues.ContainsKey(key))
			{
				m_dicValues[key] = value;
			}
			else
			{
				Add(key, value);
			}
			m_iVersion++;
		}
	}

	ICollection IDictionary.Keys => m_dicValues.Keys;

	ICollection IDictionary.Values => m_dicValues.Values;

	public object this[object key]
	{
		get
		{
			return (key is TKey) ? this[(TKey)key] : default(TValue);
		}
		set
		{
			this[(TKey)key] = (TValue)value;
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => m_dicValues.Values;

	public TypedSortedListEx()
	{
		m_arrKeys = new TKey[16];
		m_dicValues = new Dictionary<TKey, TValue>(16);
		m_comparer = Comparer<TKey>.Default;
	}

	public TypedSortedListEx(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity");
		}
		m_arrKeys = new TKey[initialCapacity];
		m_dicValues = new Dictionary<TKey, TValue>(initialCapacity);
		m_comparer = Comparer<TKey>.Default;
	}

	public TypedSortedListEx(IComparer<TKey> comparer)
		: this()
	{
		if (comparer != null)
		{
			m_comparer = comparer;
		}
	}

	public TypedSortedListEx(IComparer<TKey> comparer, int capacity)
		: this(comparer)
	{
		Capacity = capacity;
	}

	public TypedSortedListEx(IDictionary<TKey, TValue> d)
		: this(d, (IComparer<TKey>)null)
	{
	}

	public TypedSortedListEx(IDictionary<TKey, TValue> d, IComparer<TKey> comparer)
		: this(comparer, d?.Count ?? 0)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		d.Keys.CopyTo(m_arrKeys, 0);
		m_dicValues = new Dictionary<TKey, TValue>(d);
		Array.Sort(m_arrKeys, comparer);
		m_iSize = d.Count;
	}

	public static TypedSortedListEx<TKey, TValue> Synchronized(TypedSortedListEx<TKey, TValue> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		throw new NotImplementedException();
	}

	public virtual void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (m_dicValues.ContainsKey(key))
		{
			throw new ArgumentException("Duplicated");
		}
		int num = Array.BinarySearch(m_arrKeys, 0, m_iSize, key, m_comparer);
		Insert(~num, key, value);
	}

	public virtual void Clear()
	{
		m_iVersion++;
		m_iSize = 0;
		m_arrKeys = new TKey[16];
		m_dicValues = new Dictionary<TKey, TValue>(16);
	}

	public virtual object Clone()
	{
		TypedSortedListEx<TKey, TValue> typedSortedListEx = new TypedSortedListEx<TKey, TValue>(m_iSize);
		Array.Copy(m_arrKeys, 0, typedSortedListEx.m_arrKeys, 0, m_iSize);
		typedSortedListEx.m_dicValues = new Dictionary<TKey, TValue>(m_dicValues);
		typedSortedListEx.m_iSize = m_iSize;
		typedSortedListEx.m_iVersion = m_iVersion;
		typedSortedListEx.m_comparer = m_comparer;
		return typedSortedListEx;
	}

	public TypedSortedListEx<TKey, TValue> CloneAll()
	{
		int count = Count;
		TypedSortedListEx<TKey, TValue> typedSortedListEx = (TypedSortedListEx<TKey, TValue>)MemberwiseClone();
		typedSortedListEx.m_arrKeys = new TKey[count];
		typedSortedListEx.m_dicValues = new Dictionary<TKey, TValue>(count);
		typedSortedListEx.m_listKeys = null;
		typedSortedListEx.m_lstValues = null;
		typedSortedListEx.m_iSize = 0;
		for (int i = 0; i < count; i++)
		{
			TKey key = GetKey(i);
			TValue val = m_dicValues[key];
			if (val is IDocIOCloneable docIOCloneable)
			{
				val = (TValue)docIOCloneable.Clone();
			}
			typedSortedListEx.Add(key, val);
		}
		return typedSortedListEx;
	}

	public virtual bool Contains(TKey key)
	{
		return m_dicValues.ContainsKey(key);
	}

	public virtual bool ContainsKey(TKey key)
	{
		return m_dicValues.ContainsKey(key);
	}

	public virtual bool ContainsValue(TValue value)
	{
		return m_dicValues.ContainsValue(value);
	}

	public virtual void CopyTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException();
		}
		for (int i = 0; i < Count; i++)
		{
			KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(m_arrKeys[i], m_dicValues[m_arrKeys[i]]);
			array.SetValue(keyValuePair, i + arrayIndex);
		}
	}

	public virtual TValue GetByIndex(int index)
	{
		if (index < 0 || index >= m_iSize)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return m_dicValues[m_arrKeys[index]];
	}

	public virtual TKey GetKey(int index)
	{
		if (index < 0 || index >= m_iSize)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return m_arrKeys[index];
	}

	public virtual IList<TKey> GetKeyList()
	{
		if (m_listKeys == null)
		{
			m_listKeys = new KeyList(this);
		}
		return m_listKeys;
	}

	public virtual IList<TValue> GetValueList()
	{
		if (m_lstValues == null)
		{
			m_lstValues = new ValueList(this);
		}
		else
		{
			m_lstValues.UpdateValues();
		}
		return m_lstValues;
	}

	public virtual int IndexOfKey(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = Array.BinarySearch(m_arrKeys, 0, m_iSize, key, m_comparer);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	public virtual int IndexOfValue(TValue value)
	{
		object obj = null;
		IDictionaryEnumerator dictionaryEnumerator = m_dicValues.GetEnumerator();
		dictionaryEnumerator.Reset();
		while (dictionaryEnumerator.MoveNext())
		{
			if (dictionaryEnumerator.Value.Equals(value))
			{
				obj = dictionaryEnumerator.Key;
				break;
			}
		}
		if (obj == null)
		{
			return -1;
		}
		return Array.IndexOf(m_arrKeys, obj, 0, m_iSize);
	}

	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= m_iSize)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_iSize--;
		TKey key = m_arrKeys[index];
		if (index < m_iSize)
		{
			Array.Copy(m_arrKeys, index + 1, m_arrKeys, index, m_iSize - index);
		}
		m_arrKeys[m_iSize] = default(TKey);
		m_dicValues.Remove(key);
		m_iVersion++;
	}

	public virtual bool Remove(TKey key)
	{
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public virtual void SetByIndex(int index, TValue value)
	{
		if (index < 0 || index >= m_iSize)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_dicValues[m_arrKeys[index]] = value;
		m_iVersion++;
	}

	public virtual void TrimToSize()
	{
		Capacity = m_iSize;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return new TypedSortedListExEnumerator(this, 0, m_iSize);
	}

	private void Insert(int index, TKey key, TValue value)
	{
		if (m_iSize == m_arrKeys.Length)
		{
			EnsureCapacity(m_iSize + 1);
		}
		if (index < m_iSize)
		{
			Array.Copy(m_arrKeys, index, m_arrKeys, index + 1, m_iSize - index);
		}
		m_arrKeys[index] = key;
		m_dicValues[key] = value;
		m_iSize++;
		m_iVersion++;
	}

	private void EnsureCapacity(int min)
	{
		int num = ((m_arrKeys.Length == 0) ? 16 : (m_arrKeys.Length * 2));
		if (num < min)
		{
			num = min;
		}
		Capacity = num;
	}

	public void Add(object key, object value)
	{
		TKey key2 = (TKey)key;
		TValue value2 = (TValue)value;
		Add(key2, value2);
	}

	public bool Contains(object key)
	{
		if (key is TKey key2)
		{
			return ContainsKey(key2);
		}
		return false;
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return ((IDictionary)m_dicValues).GetEnumerator();
	}

	public void Remove(object key)
	{
		if (key is TKey)
		{
			Remove((TKey)key);
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return m_dicValues.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (TryGetValue(item.Key, out var value))
		{
			ref TValue reference = ref value;
			TValue val = default(TValue);
			if (val == null)
			{
				val = reference;
				//reference = ref val;
			}
			return reference.Equals(item.Value);
		}
		return false;
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)m_dicValues).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return Remove(item.Key);
	}
}
