using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public abstract class CollectionImpl : OwnerHolder
{
	private List<object> m_innerList;

	public int Count => m_innerList.Count;

	internal IList InnerList => m_innerList;

	protected CollectionImpl(WordDocument doc, OwnerHolder owner)
		: base(doc, owner)
	{
		m_innerList = new List<object>();
	}

	internal CollectionImpl(WordDocument doc, OwnerHolder owner, int capacity)
		: base(doc, owner)
	{
		m_innerList = new List<object>(capacity);
	}

	internal CollectionImpl()
	{
		m_innerList = new List<object>();
	}

	public IEnumerator GetEnumerator()
	{
		return m_innerList.GetEnumerator();
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
}
