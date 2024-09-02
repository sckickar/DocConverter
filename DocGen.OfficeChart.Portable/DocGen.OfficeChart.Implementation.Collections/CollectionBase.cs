using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class CollectionBase<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private List<T> m_arrItems;

	public int Capacity
	{
		get
		{
			return m_arrItems.Capacity;
		}
		set
		{
			m_arrItems.Capacity = value;
		}
	}

	public int Count => m_arrItems.Count;

	protected internal List<T> InnerList => m_arrItems;

	protected IList<T> List => m_arrItems;

	public T this[int i]
	{
		get
		{
			return m_arrItems[i];
		}
		set
		{
			T oldValue = m_arrItems[i];
			OnSet(i, oldValue, value);
			m_arrItems[i] = value;
			OnSetComplete(i, oldValue, value);
		}
	}

	public bool IsReadOnly => false;

	public CollectionBase()
	{
		m_arrItems = new List<T>();
	}

	public CollectionBase(int capacity)
	{
		m_arrItems = new List<T>(capacity);
	}

	public void Clear()
	{
		OnClear();
		m_arrItems.Clear();
		OnClearComplete();
	}

	public void Insert(int index, T item)
	{
		OnInsert(index, item);
		m_arrItems.Insert(index, item);
		OnInsertComplete(index, item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_arrItems.GetEnumerator();
	}

	protected virtual void OnClear()
	{
	}

	protected virtual void OnClearComplete()
	{
	}

	protected virtual void OnInsert(int index, T value)
	{
	}

	protected virtual void OnInsertComplete(int index, T value)
	{
	}

	protected virtual void OnRemove(int index, T value)
	{
	}

	protected virtual void OnRemoveComplete(int index, T value)
	{
	}

	protected virtual void OnSet(int index, T oldValue, T newValue)
	{
	}

	protected virtual void OnSetComplete(int index, T oldValue, T newValue)
	{
	}

	public void RemoveAt(int index)
	{
		T value = this[index];
		OnRemove(index, value);
		m_arrItems.RemoveAt(index);
		OnRemoveComplete(index, value);
	}

	public int IndexOf(T item)
	{
		return m_arrItems.IndexOf(item);
	}

	public virtual void Add(T item)
	{
		int count = Count;
		OnInsert(count, item);
		m_arrItems.Add(item);
		OnInsertComplete(count, item);
	}

	public bool Contains(T item)
	{
		return m_arrItems.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_arrItems.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		bool result = false;
		if (num >= 0)
		{
			RemoveAt(num);
			result = true;
		}
		return result;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_arrItems).GetEnumerator();
	}
}
