using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal class OffsetArrayList : IEnumerable, IList, ICollection, IList<IBiffStorage>, ICollection<IBiffStorage>, IEnumerable<IBiffStorage>
{
	private List<IBiffStorage> m_list = new List<IBiffStorage>();

	public bool IsFixedSize => ((IList)m_list).IsFixedSize;

	public bool IsReadOnly => ((IList)m_list).IsReadOnly;

	public IBiffStorage this[int index]
	{
		get
		{
			return m_list[index];
		}
		set
		{
			m_list[index] = value;
		}
	}

	object IList.this[int index]
	{
		get
		{
			return m_list[index];
		}
		set
		{
			if (value is IBiffStorage value2)
			{
				m_list[index] = value2;
			}
		}
	}

	public bool IsSynchronized => ((ICollection)m_list).IsSynchronized;

	public int Count => m_list.Count;

	public object SyncRoot => ((ICollection)m_list).SyncRoot;

	public void RemoveAt(int index)
	{
		m_list.RemoveAt(index);
	}

	public void Insert(int index, IBiffStorage value)
	{
		m_list.Insert(index, value);
	}

	public bool Remove(IBiffStorage value)
	{
		int num = m_list.IndexOf(value);
		if (num >= 0)
		{
			m_list.RemoveAt(num);
		}
		return num >= 0;
	}

	public bool Contains(IBiffStorage value)
	{
		return m_list.Contains(value);
	}

	public void Clear()
	{
		m_list.Clear();
	}

	public int IndexOf(IBiffStorage value)
	{
		return m_list.IndexOf(value);
	}

	public int Add(IBiffStorage value)
	{
		m_list.Add(value);
		return m_list.Count - 1;
	}

	void ICollection<IBiffStorage>.Add(IBiffStorage value)
	{
		m_list.Add(value);
	}

	public void AddList(IList value)
	{
		int i = 0;
		for (int count = value.Count; i < count; i++)
		{
			IBiffStorage value2 = value[i] as IBiffStorage;
			Add(value2);
		}
	}

	public void AddRange(ICollection value)
	{
		foreach (IBiffStorage item in value)
		{
			Add(item);
		}
	}

	public void AddRange(ICollection<IBiffStorage> value)
	{
		m_list.AddRange(value);
	}

	public void Insert(int index, object value)
	{
		if (value is BiffRecordRaw value2)
		{
			Insert(index, value2);
		}
	}

	public void Remove(object value)
	{
		BiffRecordRaw value2 = value as BiffRecordRaw;
		Remove(value2);
	}

	public bool Contains(object value)
	{
		if (!(value is BiffRecordRaw value2))
		{
			return false;
		}
		return Contains(value2);
	}

	public int IndexOf(object value)
	{
		if (!(value is BiffRecordRaw value2))
		{
			return -1;
		}
		return IndexOf(value2);
	}

	public int Add(object value)
	{
		if (!(value is BiffRecordRaw value2))
		{
			return -1;
		}
		return Add(value2);
	}

	public void CopyTo(Array array, int index)
	{
		((ICollection)m_list).CopyTo(array, index);
	}

	public IEnumerator GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator<IBiffStorage> IEnumerable<IBiffStorage>.GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	public void UpdateBiffRecordsOffsets()
	{
		CalculateRecordsStreamPos();
	}

	protected void CalculateRecordsStreamPos()
	{
		int num = 0;
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			IBiffStorage biffStorage = m_list[i];
			biffStorage.StreamPos = num;
			num += 4 + biffStorage.GetStoreSize(OfficeVersion.Excel97to2003);
		}
	}

	public void CopyTo(IBiffStorage[] array, int arrayIndex)
	{
		m_list.CopyTo(array, arrayIndex);
	}
}
