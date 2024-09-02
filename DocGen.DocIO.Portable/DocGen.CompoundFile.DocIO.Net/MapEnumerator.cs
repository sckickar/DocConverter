using System;
using System.Collections;

namespace DocGen.CompoundFile.DocIO.Net;

internal class MapEnumerator : IEnumerator
{
	private RBTreeNode m_current;

	private RBTreeNode m_parent;

	object IEnumerator.Current => m_current;

	public RBTreeNode Current => m_current;

	public MapEnumerator(RBTreeNode parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parent = parent;
	}

	public void Reset()
	{
		m_current = null;
	}

	public bool MoveNext()
	{
		if (m_current == null)
		{
			m_current = m_parent;
		}
		else
		{
			m_current = MapCollection.Inc(m_current);
		}
		if (m_current != null)
		{
			return !m_current.IsNil;
		}
		return false;
	}
}
