using System.Collections;
using System.Collections.Generic;

namespace DocGen.Office;

internal abstract class CollectionImpl : OwnerHolder, ICollectionBase, IOfficeMathEntity
{
	internal List<object> m_innerList;

	public int Count => m_innerList.Count;

	internal IList InnerList => m_innerList;

	internal CollectionImpl(IOfficeMathEntity owner)
		: base(owner)
	{
		m_innerList = new List<object>();
	}

	internal override void Close()
	{
		if (m_innerList != null)
		{
			m_innerList.Clear();
			m_innerList = null;
		}
		base.Close();
	}

	internal void Add(object item)
	{
		m_innerList.Add(item);
	}

	public void Remove(IOfficeMathEntity item)
	{
		m_innerList.Remove(item);
	}

	public void Clear()
	{
		m_innerList.Clear();
	}
}
