using System;
using System.Collections;

namespace DocGen.Chart;

internal sealed class ChartPointSortedList : ICollection, IEnumerable
{
	internal class Enumerator : IEnumerator
	{
		private ChartPointSortedList m_collection;

		private int m_index;

		public ChartPointWithIndex Current => m_collection[m_index];

		object IEnumerator.Current => Current;

		internal Enumerator(ChartPointSortedList tc)
		{
			m_collection = tc;
			m_index = -1;
		}

		public bool MoveNext()
		{
			m_index++;
			return m_index < m_collection.Count;
		}

		public void Reset()
		{
			m_index = -1;
		}
	}

	private const int c_nDEFAULT_ARRAY_SIZE = 32;

	private int m_nCapacity;

	private ChartPointWithIndex[] m_array;

	private int m_count;

	public int Capacity
	{
		get
		{
			return m_array.Length;
		}
		set
		{
			if (value < m_count)
			{
				value = m_count;
			}
			if (value < m_nCapacity)
			{
				value = m_nCapacity;
			}
			if (m_array.Length != value)
			{
				ChartPointWithIndex[] array = new ChartPointWithIndex[value];
				Array.Copy(m_array, 0, array, 0, m_count);
				m_array = array;
			}
		}
	}

	public ChartPointWithIndex this[int index]
	{
		get
		{
			return m_array[index];
		}
		set
		{
			m_array[index] = value;
		}
	}

	bool ICollection.IsSynchronized => m_array.IsSynchronized;

	object ICollection.SyncRoot => m_array.SyncRoot;

	public int Count => m_count;

	public ChartPointSortedList()
		: this(32)
	{
	}

	public ChartPointSortedList(int capacity)
	{
		m_nCapacity = capacity;
		m_array = new ChartPointWithIndex[m_nCapacity];
		m_count = 0;
	}

	public int Add(ChartPointWithIndex item)
	{
		ProvideSpaceFor(1);
		int insertIndex = GetInsertIndex(item, 0, Count);
		if (insertIndex < 0)
		{
			throw new IndexOutOfRangeException("Insert position");
		}
		Insert(insertIndex, item);
		return m_count++;
	}

	public void Clear()
	{
		m_array = new ChartPointWithIndex[m_nCapacity];
		m_count = 0;
	}

	public bool Contains(ChartPointWithIndex point)
	{
		return IndexOf(point) != -1;
	}

	public void RemoveAt(int index)
	{
		ValidateIndex(index);
		m_count--;
		Array.Copy(m_array, index + 1, m_array, index, m_count - index);
	}

	public ChartPointWithIndex[] ToArray()
	{
		ChartPointWithIndex[] array = new ChartPointWithIndex[m_count];
		Array.Copy(m_array, array, m_count);
		return array;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	void ICollection.CopyTo(Array array, int start)
	{
		if (m_count > array.GetUpperBound(0) + 1 - start)
		{
			throw new ArgumentException("Destination array was not long enough.");
		}
		Array.Copy(m_array, 0, array, start, m_count);
	}

	private int IndexOf(ChartPointWithIndex item)
	{
		for (int i = 0; i < m_count; i++)
		{
			if (m_array[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	private void Insert(int index, ChartPointWithIndex value)
	{
		ProvideSpaceFor(1);
		if (index < Count)
		{
			Array.Copy(m_array, index, m_array, index + 1, Count - index);
		}
		m_array[index] = value;
	}

	private int GetInsertIndex(ChartPointWithIndex point, int start, int end)
	{
		if (m_array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (start < 0 || end < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (Count - start < end)
		{
			throw new ArgumentException("Argument_InvalidOffLen");
		}
		int num = start;
		int num2 = start + Count - 1;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			if (m_array[num3].Point.X < point.Point.X)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return num;
	}

	private void ProvideSpaceFor(int nItems)
	{
		if (m_count + nItems >= Capacity)
		{
			if (m_count + nItems < 2 * Capacity)
			{
				Capacity *= 2;
			}
			else
			{
				Capacity = m_count + nItems;
			}
		}
	}

	private void ValidateIndex(int index)
	{
		if (index < 0 || index >= m_count)
		{
			throw new ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", index, "Specified argument was out of the range of valid values.");
		}
	}
}
