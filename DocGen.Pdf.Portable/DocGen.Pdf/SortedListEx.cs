using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class SortedListEx : IDictionary, ICollection, IEnumerable, ICloneable
{
	private class SyncSortedListEx : SortedListEx
	{
		private SortedListEx _list;

		private object _root;

		public override int Capacity
		{
			get
			{
				lock (_root)
				{
					return _list.Capacity;
				}
			}
		}

		public override int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public override object SyncRoot => _root;

		public override bool IsReadOnly => _list.IsReadOnly;

		public override bool IsFixedSize => _list.IsFixedSize;

		public override bool IsSynchronized => true;

		public override object this[object key]
		{
			get
			{
				lock (_root)
				{
					return _list[key];
				}
			}
			set
			{
				lock (_root)
				{
					_list[key] = value;
				}
			}
		}

		internal SyncSortedListEx(SortedListEx list)
		{
			_list = list;
			_root = list.SyncRoot;
		}

		public override void Add(object key, object value)
		{
			lock (_root)
			{
				_list.Add(key, value);
			}
		}

		public override void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		public override object Clone()
		{
			lock (_root)
			{
				return _list.Clone();
			}
		}

		public override bool Contains(object key)
		{
			lock (_root)
			{
				return _list.Contains(key);
			}
		}

		public override bool ContainsKey(object key)
		{
			lock (_root)
			{
				return _list.ContainsKey(key);
			}
		}

		public override bool ContainsValue(object value)
		{
			lock (_root)
			{
				return _list.ContainsValue(value);
			}
		}

		public override void CopyTo(Array array, int index)
		{
			lock (_root)
			{
				_list.CopyTo(array, index);
			}
		}

		public override object GetByIndex(int index)
		{
			lock (_root)
			{
				return _list.GetByIndex(index);
			}
		}

		public override IDictionaryEnumerator GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		public override object GetKey(int index)
		{
			lock (_root)
			{
				return _list.GetKey(index);
			}
		}

		public override IList GetKeyList()
		{
			lock (_root)
			{
				return _list.GetKeyList();
			}
		}

		public override IList GetValueList()
		{
			lock (_root)
			{
				return _list.GetValueList();
			}
		}

		public override int IndexOfKey(object key)
		{
			lock (_root)
			{
				return _list.IndexOfKey(key);
			}
		}

		public override int IndexOfValue(object value)
		{
			lock (_root)
			{
				return _list.IndexOfValue(value);
			}
		}

		public override void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}

		public override void Remove(object key)
		{
			lock (_root)
			{
				_list.Remove(key);
			}
		}

		public override void SetByIndex(int index, object value)
		{
			lock (_root)
			{
				_list.SetByIndex(index, value);
			}
		}

		public override void TrimToSize()
		{
			lock (_root)
			{
				_list.TrimToSize();
			}
		}
	}

	private class SortedListExEnumerator : IDictionaryEnumerator, IEnumerator, ICloneable
	{
		internal const int Keys = 1;

		internal const int Values = 2;

		internal const int DictEntry = 3;

		private SortedListEx SortedListEx;

		private object key;

		private object value;

		private int index;

		private int startIndex;

		private int endIndex;

		private int version;

		private bool current;

		private int getObjectRetType;

		public virtual object Key
		{
			get
			{
				if (version != SortedListEx.version)
				{
					throw new InvalidOperationException();
				}
				if (!current)
				{
					throw new InvalidOperationException();
				}
				return key;
			}
		}

		public virtual DictionaryEntry Entry
		{
			get
			{
				if (version != SortedListEx.version)
				{
					throw new InvalidOperationException();
				}
				if (!current)
				{
					throw new InvalidOperationException();
				}
				return new DictionaryEntry(key, value);
			}
		}

		public virtual object Current
		{
			get
			{
				if (!current)
				{
					throw new InvalidOperationException();
				}
				if (getObjectRetType == 1)
				{
					return key;
				}
				if (getObjectRetType == 2)
				{
					return value;
				}
				return new DictionaryEntry(key, value);
			}
		}

		public virtual object Value
		{
			get
			{
				if (version != SortedListEx.version)
				{
					throw new InvalidOperationException();
				}
				if (!current)
				{
					throw new InvalidOperationException();
				}
				return value;
			}
		}

		internal SortedListExEnumerator(SortedListEx SortedListEx, int index, int count, int getObjRetType)
		{
			this.SortedListEx = SortedListEx;
			this.index = index;
			startIndex = index;
			endIndex = index + count;
			version = SortedListEx.version;
			getObjectRetType = getObjRetType;
			current = false;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (version != SortedListEx.version)
			{
				throw new InvalidOperationException();
			}
			if (index < endIndex)
			{
				key = SortedListEx.keys[index];
				value = SortedListEx.values[key];
				index++;
				current = true;
				return true;
			}
			key = null;
			value = null;
			current = false;
			return false;
		}

		public virtual void Reset()
		{
			if (version != SortedListEx.version)
			{
				throw new InvalidOperationException();
			}
			index = startIndex;
			current = false;
			key = null;
			value = null;
		}
	}

	private class KeyList : IList, ICollection, IEnumerable
	{
		private SortedListEx SortedListEx;

		public virtual int Count => SortedListEx._size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => SortedListEx.IsSynchronized;

		public virtual object SyncRoot => SortedListEx.SyncRoot;

		public virtual object this[int index]
		{
			get
			{
				return SortedListEx.GetKey(index);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal KeyList(SortedListEx SortedListEx)
		{
			this.SortedListEx = SortedListEx;
		}

		public virtual int Add(object key)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(object key)
		{
			return SortedListEx.Contains(key);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException();
			}
			Array.Copy(SortedListEx.keys, 0, array, arrayIndex, SortedListEx.Count);
		}

		public virtual void Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new SortedListExEnumerator(SortedListEx, 0, SortedListEx.Count, 1);
		}

		public virtual int IndexOf(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = Array.BinarySearch(SortedListEx.keys, 0, SortedListEx.Count, key, SortedListEx.comparer);
			if (num >= 0)
			{
				return num;
			}
			return -1;
		}

		public virtual void Remove(object key)
		{
			throw new NotSupportedException();
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private class ValueList : IList, ICollection, IEnumerable
	{
		private SortedListEx SortedListEx;

		private Array vals;

		public virtual int Count => SortedListEx._size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => SortedListEx.IsSynchronized;

		public virtual object SyncRoot => SortedListEx.SyncRoot;

		public virtual object this[int index]
		{
			get
			{
				return SortedListEx.GetByIndex(index);
			}
			set
			{
				SortedListEx.SetByIndex(index, value);
			}
		}

		internal ValueList(SortedListEx SortedListEx)
		{
			this.SortedListEx = SortedListEx;
			UpdateValues();
		}

		public virtual void UpdateValues()
		{
			int count = SortedListEx.Count;
			vals = new object[count];
			throw new NotImplementedException();
		}

		public virtual int Add(object key)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(object value)
		{
			return SortedListEx.ContainsValue(value);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException();
			}
			Array.Copy(vals, 0, array, arrayIndex, SortedListEx.Count);
		}

		public virtual void Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new SortedListExEnumerator(SortedListEx, 0, SortedListEx.Count, 2);
		}

		public virtual int IndexOf(object value)
		{
			return Array.IndexOf(vals, value, 0, SortedListEx.Count);
		}

		public virtual void Remove(object value)
		{
			throw new NotSupportedException();
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	private const int _defaultCapacity = 16;

	private object[] keys;

	private Dictionary<object, object> values;

	private int _size;

	private int version;

	private IComparer comparer;

	private KeyList keyList;

	private ValueList valueList;

	public virtual int Capacity
	{
		get
		{
			return keys.Length;
		}
		set
		{
			if (value == keys.Length)
			{
				return;
			}
			if (value < _size)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value > 0)
			{
				object[] destinationArray = new object[value];
				if (_size > 0)
				{
					Array.Copy(keys, 0, destinationArray, 0, _size);
				}
				keys = destinationArray;
			}
			else
			{
				keys = new object[16];
			}
		}
	}

	public virtual int Count => _size;

	public virtual ICollection Keys => GetKeyList();

	public virtual ICollection Values => GetValueList();

	public virtual bool IsReadOnly => false;

	public virtual bool IsFixedSize => false;

	public virtual bool IsSynchronized => false;

	public virtual object SyncRoot => this;

	public virtual object this[object key]
	{
		get
		{
			return values[key];
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (values.ContainsKey(key))
			{
				values[key] = value;
			}
			else
			{
				Add(key, value);
			}
			version++;
		}
	}

	public SortedListEx()
	{
		keys = new object[16];
		values = new Dictionary<object, object>(16);
	}

	public SortedListEx(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity");
		}
		keys = new object[initialCapacity];
		values = new Dictionary<object, object>(initialCapacity);
	}

	public SortedListEx(IComparer comparer)
		: this()
	{
		if (comparer != null)
		{
			this.comparer = comparer;
		}
	}

	public SortedListEx(IComparer comparer, int capacity)
		: this(comparer)
	{
		Capacity = capacity;
	}

	public SortedListEx(IDictionary d)
		: this(d, null)
	{
	}

	public SortedListEx(IDictionary d, IComparer comparer)
		: this(comparer, d?.Count ?? 0)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		throw new NotImplementedException();
	}

	public static SortedListEx Synchronized(SortedListEx list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new SyncSortedListEx(list);
	}

	public virtual void Add(object key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (values.ContainsKey(key))
		{
			throw new ArgumentException("Duplicated");
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		Insert(~num, key, value);
	}

	public virtual void Clear()
	{
		version++;
		_size = 0;
		keys = new object[16];
		values = new Dictionary<object, object>(16);
	}

	public virtual object Clone()
	{
		SortedListEx sortedListEx = new SortedListEx(_size);
		Array.Copy(keys, 0, sortedListEx.keys, 0, _size);
		sortedListEx.values = new Dictionary<object, object>(values);
		sortedListEx._size = _size;
		sortedListEx.version = version;
		sortedListEx.comparer = comparer;
		return sortedListEx;
	}

	public SortedListEx CloneAll()
	{
		int count = Count;
		SortedListEx sortedListEx = new SortedListEx(count + 1);
		for (int i = 0; i < count; i++)
		{
			object byIndex = GetByIndex(i);
			byIndex = ((ICloneable)byIndex).Clone();
			sortedListEx.Add(GetKey(i), byIndex);
		}
		return sortedListEx;
	}

	public virtual bool Contains(object key)
	{
		return values.ContainsKey(key);
	}

	public virtual bool ContainsKey(object key)
	{
		return values.ContainsKey(key);
	}

	public virtual bool ContainsValue(object value)
	{
		return values.ContainsValue(value);
	}

	public virtual void CopyTo(Array array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException();
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
			DictionaryEntry dictionaryEntry = new DictionaryEntry(keys[i], values[keys[i]]);
			array.SetValue(dictionaryEntry, i + arrayIndex);
		}
	}

	public virtual object GetByIndex(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return values[keys[index]];
	}

	public virtual object GetKey(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return keys[index];
	}

	public virtual IList GetKeyList()
	{
		if (keyList == null)
		{
			keyList = new KeyList(this);
		}
		return keyList;
	}

	public virtual IList GetValueList()
	{
		if (valueList == null)
		{
			valueList = new ValueList(this);
		}
		else
		{
			valueList.UpdateValues();
		}
		return valueList;
	}

	public virtual int IndexOfKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = Array.BinarySearch(keys, 0, _size, key, comparer);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	public virtual int IndexOfValue(object value)
	{
		object obj = null;
		IDictionaryEnumerator dictionaryEnumerator = values.GetEnumerator();
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
		return Array.IndexOf(keys, obj, 0, _size);
	}

	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		_size--;
		object key = keys[index];
		if (index < _size)
		{
			Array.Copy(keys, index + 1, keys, index, _size - index);
		}
		keys[_size] = null;
		values.Remove(key);
		version++;
	}

	public virtual void Remove(object key)
	{
		int num = IndexOfKey(key);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	public virtual void SetByIndex(int index, object value)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		values[keys[index]] = value;
		version++;
	}

	public virtual void TrimToSize()
	{
		Capacity = _size;
	}

	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return new SortedListExEnumerator(this, 0, _size, 3);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SortedListExEnumerator(this, 0, _size, 3);
	}

	private void Insert(int index, object key, object value)
	{
		if (_size == keys.Length)
		{
			EnsureCapacity(_size + 1);
		}
		if (index < _size)
		{
			Array.Copy(keys, index, keys, index + 1, _size - index);
		}
		keys[index] = key;
		values[key] = value;
		_size++;
		version++;
	}

	private void EnsureCapacity(int min)
	{
		int num = ((keys.Length == 0) ? 16 : (keys.Length * 2));
		if (num < min)
		{
			num = min;
		}
		Capacity = num;
	}
}
internal class SortedListEx<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICloneable
{
	private class SyncSortedListEx : SortedListEx<TKey, TValue>
	{
		private SortedListEx<TKey, TValue> m_list;

		private object m_root;

		public override int Capacity
		{
			get
			{
				lock (m_root)
				{
					return m_list.Capacity;
				}
			}
		}

		public override int Count
		{
			get
			{
				lock (m_root)
				{
					return m_list.Count;
				}
			}
		}

		public override object SyncRoot => m_root;

		public override bool IsReadOnly => m_list.IsReadOnly;

		public override bool IsFixedSize => m_list.IsFixedSize;

		public override bool IsSynchronized => true;

		public override TValue this[TKey key]
		{
			get
			{
				lock (m_root)
				{
					return m_list[key];
				}
			}
			set
			{
				lock (m_root)
				{
					m_list[key] = value;
				}
			}
		}

		internal SyncSortedListEx(SortedListEx<TKey, TValue> list)
		{
			m_list = list;
			m_root = list.SyncRoot;
		}

		public override void Add(TKey key, TValue value)
		{
			lock (m_root)
			{
				m_list.Add(key, value);
			}
		}

		public override void Clear()
		{
			lock (m_root)
			{
				m_list.Clear();
			}
		}

		public override object Clone()
		{
			lock (m_root)
			{
				return m_list.Clone();
			}
		}

		public override bool Contains(TKey key)
		{
			lock (m_root)
			{
				return m_list.Contains(key);
			}
		}

		public override bool ContainsKey(TKey key)
		{
			lock (m_root)
			{
				return m_list.ContainsKey(key);
			}
		}

		public override bool ContainsValue(TValue value)
		{
			lock (m_root)
			{
				return m_list.ContainsValue(value);
			}
		}

		public override void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			lock (m_root)
			{
				m_list.CopyTo(array, index);
			}
		}

		public override TValue GetByIndex(int index)
		{
			lock (m_root)
			{
				return m_list.GetByIndex(index);
			}
		}

		public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			lock (m_root)
			{
				return m_list.GetEnumerator();
			}
		}

		public override TKey GetKey(int index)
		{
			lock (m_root)
			{
				return m_list.GetKey(index);
			}
		}

		public override IList<TKey> GetKeyList()
		{
			lock (m_root)
			{
				return m_list.GetKeyList();
			}
		}

		public override IList<TValue> GetValueList()
		{
			lock (m_root)
			{
				return m_list.GetValueList();
			}
		}

		public override int IndexOfKey(TKey key)
		{
			lock (m_root)
			{
				return m_list.IndexOfKey(key);
			}
		}

		public override int IndexOfValue(TValue value)
		{
			lock (m_root)
			{
				return m_list.IndexOfValue(value);
			}
		}

		public override void RemoveAt(int index)
		{
			lock (m_root)
			{
				m_list.RemoveAt(index);
			}
		}

		public override bool Remove(TKey key)
		{
			bool flag = false;
			lock (m_root)
			{
				return m_list.Remove(key);
			}
		}

		public override void SetByIndex(int index, TValue value)
		{
			lock (m_root)
			{
				m_list.SetByIndex(index, value);
			}
		}

		public override void TrimToSize()
		{
			lock (m_root)
			{
				m_list.TrimToSize();
			}
		}
	}

	private class SortedListExEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable, ICloneable
	{
		private SortedListEx<TKey, TValue> m_sortedListEx;

		private TKey m_key;

		private TValue m_value;

		private int m_index;

		private int m_startIndex;

		private int m_endIndex;

		private int m_version;

		private bool m_current;

		private bool m_isDisposed;

		public virtual TKey Key
		{
			get
			{
				if (m_version != m_sortedListEx.m_version)
				{
					throw new InvalidOperationException();
				}
				if (!m_current)
				{
					throw new InvalidOperationException();
				}
				return m_key;
			}
		}

		public virtual KeyValuePair<TKey, TValue> Current
		{
			get
			{
				if (!m_current)
				{
					throw new InvalidOperationException();
				}
				return new KeyValuePair<TKey, TValue>(m_key, m_value);
			}
		}

		public virtual TValue Value
		{
			get
			{
				if (m_version != m_sortedListEx.m_version)
				{
					throw new InvalidOperationException();
				}
				if (!m_current)
				{
					throw new InvalidOperationException();
				}
				return m_value;
			}
		}

		object IEnumerator.Current => Current;

		internal SortedListExEnumerator(SortedListEx<TKey, TValue> sortedListEx, int index, int count)
		{
			m_sortedListEx = sortedListEx;
			m_index = index;
			m_startIndex = index;
			m_endIndex = index + count;
			m_version = sortedListEx.m_version;
			m_current = false;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public virtual bool MoveNext()
		{
			if (m_version != m_sortedListEx.m_version)
			{
				throw new InvalidOperationException();
			}
			if (m_index < m_endIndex)
			{
				m_key = m_sortedListEx.m_keys[m_index];
				m_value = m_sortedListEx.m_values[m_key];
				m_index++;
				m_current = true;
				return true;
			}
			m_key = default(TKey);
			m_value = default(TValue);
			m_current = false;
			return false;
		}

		public virtual void Reset()
		{
			if (m_version != m_sortedListEx.m_version)
			{
				throw new InvalidOperationException();
			}
			m_index = m_startIndex;
			m_current = false;
			m_key = default(TKey);
			m_value = default(TValue);
		}

		public void Dispose()
		{
			if (!m_isDisposed)
			{
				m_isDisposed = true;
				m_sortedListEx = null;
				m_current = false;
			}
		}
	}

	private class KeyList : IList<TKey>, ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		private SortedListEx<TKey, TValue> m_sortedListEx;

		public virtual int Count => m_sortedListEx.m_size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => m_sortedListEx.IsSynchronized;

		public virtual object SyncRoot => m_sortedListEx.SyncRoot;

		public virtual TKey this[int index]
		{
			get
			{
				return m_sortedListEx.GetKey(index);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal KeyList(SortedListEx<TKey, TValue> sortedListEx)
		{
			m_sortedListEx = sortedListEx;
		}

		public virtual void Add(TKey key)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(TKey key)
		{
			return m_sortedListEx.Contains(key);
		}

		public virtual void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException();
			}
			Array.Copy(m_sortedListEx.m_keys, 0, array, arrayIndex, m_sortedListEx.Count);
		}

		public virtual void Insert(int index, TKey value)
		{
			throw new NotSupportedException();
		}

		public virtual IEnumerator<TKey> GetEnumerator()
		{
			int index = 0;
			while (index < m_sortedListEx.m_size)
			{
				yield return m_sortedListEx.m_keys[index];
				int num = index + 1;
				index = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual int IndexOf(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			int num = Array.BinarySearch(m_sortedListEx.m_keys, 0, m_sortedListEx.Count, key, m_sortedListEx.m_comparer);
			if (num >= 0)
			{
				return num;
			}
			return -1;
		}

		public virtual bool Remove(TKey key)
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
		private SortedListEx<TKey, TValue> m_sortedListEx;

		private TValue[] m_values;

		public virtual int Count => m_sortedListEx.m_size;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => m_sortedListEx.IsSynchronized;

		public virtual object SyncRoot => m_sortedListEx.SyncRoot;

		public virtual TValue this[int index]
		{
			get
			{
				return m_sortedListEx.GetByIndex(index);
			}
			set
			{
				m_sortedListEx.SetByIndex(index, value);
			}
		}

		internal ValueList(SortedListEx<TKey, TValue> sortedListEx)
		{
			m_sortedListEx = sortedListEx;
			UpdateValues();
		}

		public virtual void UpdateValues()
		{
			int count = m_sortedListEx.Count;
			m_values = new TValue[count];
			m_sortedListEx.m_values.Values.CopyTo(m_values, 0);
			TKey[] array = new TKey[count];
			m_sortedListEx.m_values.Keys.CopyTo(array, 0);
			Array.Sort(array, m_sortedListEx.m_comparer);
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
			return m_sortedListEx.ContainsValue(value);
		}

		public virtual void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException();
			}
			Array.Copy(m_values, 0, array, arrayIndex, m_sortedListEx.Count);
		}

		public virtual void Insert(int index, TValue value)
		{
			throw new NotSupportedException();
		}

		public virtual IEnumerator<TValue> GetEnumerator()
		{
			int index = 0;
			TKey[] keys = m_sortedListEx.m_keys;
			while (index < m_sortedListEx.m_size)
			{
				yield return m_sortedListEx.m_values[keys[index]];
				int num = index + 1;
				index = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual int IndexOf(TValue value)
		{
			return Array.IndexOf(m_values, value, 0, m_sortedListEx.Count);
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

	private const int _defaultCapacity = 16;

	private TKey[] m_keys;

	private Dictionary<TKey, TValue> m_values;

	private int m_size;

	private int m_version;

	private IComparer<TKey> m_comparer;

	private KeyList m_keyList;

	private ValueList m_valueList;

	public virtual int Capacity
	{
		get
		{
			return m_keys.Length;
		}
		set
		{
			if (value == m_keys.Length)
			{
				return;
			}
			if (value < m_size)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value > 0)
			{
				TKey[] array = new TKey[value];
				if (m_size > 0)
				{
					Array.Copy(m_keys, 0, array, 0, m_size);
				}
				m_keys = array;
			}
			else
			{
				m_keys = new TKey[16];
			}
		}
	}

	public virtual int Count => m_size;

	public virtual IList<TKey> Keys => GetKeyList();

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => GetKeyList();

	public virtual IList<TValue> Values => GetValueList();

	ICollection<TValue> IDictionary<TKey, TValue>.Values => GetValueList();

	public virtual bool IsReadOnly => false;

	public virtual bool IsFixedSize => false;

	public virtual bool IsSynchronized => false;

	public virtual object SyncRoot => this;

	public virtual TValue this[TKey key]
	{
		get
		{
			return m_values[key];
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (m_values.ContainsKey(key))
			{
				m_values[key] = value;
			}
			else
			{
				Add(key, value);
			}
			m_version++;
		}
	}

	public SortedListEx()
	{
		m_keys = new TKey[16];
		m_values = new Dictionary<TKey, TValue>(16);
		m_comparer = Comparer<TKey>.Default;
	}

	public SortedListEx(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity");
		}
		m_keys = new TKey[initialCapacity];
		m_values = new Dictionary<TKey, TValue>(initialCapacity);
		m_comparer = Comparer<TKey>.Default;
	}

	public SortedListEx(IComparer<TKey> comparer)
		: this()
	{
		if (comparer != null)
		{
			m_comparer = comparer;
		}
	}

	public SortedListEx(IComparer<TKey> comparer, int capacity)
		: this(comparer)
	{
		Capacity = capacity;
	}

	public SortedListEx(IDictionary<TKey, TValue> d)
		: this(d, (IComparer<TKey>)null)
	{
	}

	public SortedListEx(IDictionary<TKey, TValue> d, IComparer<TKey> comparer)
		: this(comparer, d?.Count ?? 0)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		d.Keys.CopyTo(m_keys, 0);
		m_values = new Dictionary<TKey, TValue>(d);
		Array.Sort(m_keys, comparer);
		m_size = d.Count;
	}

	public static SortedListEx<TKey, TValue> Synchronized(SortedListEx<TKey, TValue> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new SyncSortedListEx(list);
	}

	public virtual void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (m_values.ContainsKey(key))
		{
			throw new ArgumentException("Duplicated");
		}
		int num = Array.BinarySearch(m_keys, 0, m_size, key, m_comparer);
		Insert(~num, key, value);
	}

	public virtual void Add(KeyValuePair<TKey, TValue> pair)
	{
		Add(pair.Key, pair.Value);
	}

	public virtual void Clear()
	{
		m_version++;
		m_size = 0;
		m_keys = new TKey[16];
		m_values = new Dictionary<TKey, TValue>(16);
	}

	public virtual object Clone()
	{
		SortedListEx<TKey, TValue> sortedListEx = new SortedListEx<TKey, TValue>(m_size);
		Array.Copy(m_keys, 0, sortedListEx.m_keys, 0, m_size);
		sortedListEx.m_values = new Dictionary<TKey, TValue>(m_values);
		sortedListEx.m_size = m_size;
		sortedListEx.m_version = m_version;
		sortedListEx.m_comparer = m_comparer;
		return sortedListEx;
	}

	public SortedListEx<TKey, TValue> CloneAll()
	{
		int count = Count;
		SortedListEx<TKey, TValue> sortedListEx = new SortedListEx<TKey, TValue>(count + 1);
		for (int i = 0; i < count; i++)
		{
			TValue byIndex = GetByIndex(i);
			byIndex = (TValue)((ICloneable)(object)byIndex).Clone();
			sortedListEx.Add(GetKey(i), byIndex);
		}
		return sortedListEx;
	}

	public virtual bool Contains(TKey key)
	{
		return m_values.ContainsKey(key);
	}

	public virtual bool ContainsKey(TKey key)
	{
		return m_values.ContainsKey(key);
	}

	public virtual bool ContainsValue(TValue value)
	{
		return m_values.ContainsValue(value);
	}

	public virtual bool Contains(KeyValuePair<TKey, TValue> pair)
	{
		bool result = false;
		if (ContainsKey(pair.Key))
		{
			result = pair.Value.Equals(this[pair.Key]);
		}
		return result;
	}

	public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException();
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
			TKey key = m_keys[i];
			KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, m_values[key]);
			array.SetValue(keyValuePair, i + arrayIndex);
		}
	}

	public virtual TValue GetByIndex(int index)
	{
		if (index < 0 || index >= m_size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return m_values[m_keys[index]];
	}

	public virtual TKey GetKey(int index)
	{
		if (index < 0 || index >= m_size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return m_keys[index];
	}

	public virtual IList<TKey> GetKeyList()
	{
		if (m_keyList == null)
		{
			m_keyList = new KeyList(this);
		}
		return m_keyList;
	}

	public virtual IList<TValue> GetValueList()
	{
		if (m_valueList == null)
		{
			m_valueList = new ValueList(this);
		}
		else
		{
			m_valueList.UpdateValues();
		}
		return m_valueList;
	}

	public virtual int IndexOfKey(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int num = Array.BinarySearch(m_keys, 0, m_size, key, m_comparer);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	public virtual int IndexOfValue(TValue value)
	{
		IEnumerator<KeyValuePair<TKey, TValue>> enumerator = m_values.GetEnumerator();
		enumerator.Reset();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.Equals(value))
			{
				TKey key = enumerator.Current.Key;
				return Array.IndexOf(m_keys, key, 0, m_size);
			}
		}
		return -1;
	}

	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= m_size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_size--;
		TKey key = m_keys[index];
		if (index < m_size)
		{
			Array.Copy(m_keys, index + 1, m_keys, index, m_size - index);
		}
		m_keys[m_size] = default(TKey);
		m_values.Remove(key);
		m_version++;
	}

	public virtual bool Remove(TKey key)
	{
		int num = IndexOfKey(key);
		bool result = false;
		if (num >= 0)
		{
			RemoveAt(num);
			result = true;
		}
		return result;
	}

	public virtual bool Remove(KeyValuePair<TKey, TValue> pair)
	{
		bool result = false;
		if (Contains(pair) && pair.Value.Equals(this[pair.Key]))
		{
			Remove(pair.Key);
			result = true;
		}
		return result;
	}

	public virtual void SetByIndex(int index, TValue value)
	{
		if (index < 0 || index >= m_size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_values[m_keys[index]] = value;
		m_version++;
	}

	public virtual void TrimToSize()
	{
		Capacity = m_size;
	}

	public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return new SortedListExEnumerator(this, 0, m_size);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual bool TryGetValue(TKey key, out TValue value)
	{
		bool result = false;
		if (ContainsKey(key))
		{
			value = this[key];
			result = true;
		}
		else
		{
			value = default(TValue);
		}
		return result;
	}

	private void Insert(int index, TKey key, TValue value)
	{
		if (m_size == m_keys.Length)
		{
			EnsureCapacity(m_size + 1);
		}
		if (index < m_size)
		{
			Array.Copy(m_keys, index, m_keys, index + 1, m_size - index);
		}
		m_keys[index] = key;
		m_values[key] = value;
		m_size++;
		m_version++;
	}

	private void EnsureCapacity(int min)
	{
		int num = ((m_keys.Length == 0) ? 16 : (m_keys.Length * 2));
		if (num < min)
		{
			num = min;
		}
		Capacity = num;
	}
}
