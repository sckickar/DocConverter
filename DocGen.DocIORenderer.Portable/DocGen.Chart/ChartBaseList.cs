using System;
using System.Collections;
using System.Threading;

namespace DocGen.Chart;

internal class ChartBaseList : IList, ICollection, IEnumerable
{
	internal class ChartBaseEnumerator : IEnumerator
	{
		private int m_currIndex = -1;

		private ChartBaseList m_list;

		public object Current => m_list.List[m_currIndex];

		public ChartBaseEnumerator(ChartBaseList list)
		{
			m_list = list;
		}

		public bool MoveNext()
		{
			return ++m_currIndex < m_list.Count;
		}

		public void Reset()
		{
			m_currIndex = -1;
		}
	}

	private const int DEF_ITEMS_SIZE = 4;

	private bool m_freezeEvent;

	private int m_count;

	private object[] m_items;

	private object m_syncRoot;

	protected bool FreezeEvent
	{
		get
		{
			return m_freezeEvent;
		}
		set
		{
			m_freezeEvent = value;
		}
	}

	protected IList List => this;

	public virtual bool IsFixedSize => false;

	public virtual bool IsReadOnly => false;

	public virtual bool IsSynchronized => false;

	public virtual object SyncRoot
	{
		get
		{
			if (m_syncRoot == null)
			{
				Interlocked.CompareExchange(ref m_syncRoot, new object(), null);
			}
			return m_syncRoot;
		}
	}

	public int Count => m_count;

	object IList.this[int index]
	{
		get
		{
			if (index < 0 || index >= m_count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_items[index];
		}
		set
		{
			if (Validate(value))
			{
				if (index < 0 || index >= m_count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (m_items[index] != value)
				{
					ChartListChangeArgs args = new ChartListChangeArgs(index, new object[1] { m_items[index] }, new object[1] { value });
					m_items[index] = value;
					OnChanged(args);
				}
			}
		}
	}

	public event ChartListChangeHandler Changed;

	public ChartBaseList()
	{
		m_items = new object[4];
	}

	public ChartBaseList(int capacity)
	{
		m_items = new object[capacity];
	}

	public void Clear()
	{
		object[] array = new object[m_count];
		Array.Copy(m_items, 0, array, 0, m_count);
		for (int i = 0; i < m_count; i++)
		{
			m_items[i] = null;
		}
		m_count = 0;
		OnChanged(new ChartListChangeArgs(0, array, null));
	}

	public void RemoveAt(int index)
	{
		if (index >= 0 && index < m_count)
		{
			ChartListChangeArgs args = new ChartListChangeArgs(index, new object[1] { m_items[index] }, null);
			NativeRemoveAt(index);
			OnChanged(args);
		}
	}

	public void CopyTo(Array array, int index)
	{
		Array.Copy(m_items, index, array, 0, m_count - index);
	}

	public void Sort(IComparer comparer)
	{
		Array.Sort(m_items, 0, m_count, comparer);
	}

	public object[] ToArray()
	{
		object[] array = new object[m_count];
		Array.Copy(m_items, 0, array, 0, m_count);
		return array;
	}

	public Array ToArray(Type type)
	{
		Array array = Array.CreateInstance(type, m_count);
		Array.Copy(m_items, 0, array, 0, m_count);
		return array;
	}

	public IEnumerator GetEnumerator()
	{
		return new ChartBaseEnumerator(this);
	}

	int IList.Add(object value)
	{
		int num = -1;
		if (Validate(value))
		{
			num = m_count;
			ChartListChangeArgs args = new ChartListChangeArgs(num, null, new object[1] { value });
			NativeInsert(num, value);
			OnChanged(args);
		}
		return num;
	}

	void IList.Insert(int index, object value)
	{
		if (Validate(value) && index <= m_count)
		{
			ChartListChangeArgs args = new ChartListChangeArgs(index, null, new object[1] { value });
			NativeInsert(index, value);
			OnChanged(args);
		}
	}

	void IList.Remove(object value)
	{
		int num = Array.IndexOf(m_items, value, 0, m_count);
		if (num >= 0 && num < m_count)
		{
			ChartListChangeArgs args = new ChartListChangeArgs(num, new object[1] { value }, null);
			NativeRemoveAt(num);
			OnChanged(args);
		}
	}

	int IList.IndexOf(object value)
	{
		return Array.IndexOf(m_items, value, 0, m_count);
	}

	bool IList.Contains(object value)
	{
		return Array.IndexOf(m_items, value, 0, m_count) != -1;
	}

	protected virtual bool Validate(object obj)
	{
		return true;
	}

	protected virtual void OnChanged(ChartListChangeArgs args)
	{
		if (!m_freezeEvent && this.Changed != null)
		{
			this.Changed(this, args);
		}
	}

	private void NativeInsert(int index, object obj)
	{
		m_count++;
		if (m_count > m_items.Length)
		{
			object[] items = m_items;
			m_items = new object[2 * m_items.Length];
			Array.Copy(items, 0, m_items, 0, items.Length);
		}
		if (index < m_count)
		{
			int num = m_count - 1;
			while (num > index)
			{
				m_items[num] = m_items[--num];
			}
		}
		m_items[index] = obj;
	}

	private void NativeRemoveAt(int index)
	{
		if (index != m_count)
		{
			int num = index;
			int num2 = m_count - 1;
			while (num < num2)
			{
				m_items[num] = m_items[++num];
			}
		}
		m_count--;
		m_items[m_count] = null;
	}
}
