using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class ContentControlListItems : IEnumerable
{
	private string m_lastValue;

	private List<ContentControlListItem> m_listItems = new List<ContentControlListItem>();

	internal string LastValue
	{
		get
		{
			return m_lastValue;
		}
		set
		{
			m_lastValue = value;
		}
	}

	public ContentControlListItem this[int index]
	{
		get
		{
			if (m_listItems == null)
			{
				m_listItems = new List<ContentControlListItem>();
			}
			return m_listItems[index];
		}
	}

	public int Count
	{
		get
		{
			if (m_listItems != null)
			{
				return m_listItems.Count;
			}
			return 0;
		}
	}

	public void Add(ContentControlListItem item)
	{
		if (m_listItems != null)
		{
			m_listItems.Add(item);
		}
	}

	public void Insert(int index, ContentControlListItem item)
	{
		if (m_listItems != null && index < m_listItems.Count - 1)
		{
			m_listItems.Insert(index, item);
		}
	}

	public void Remove(ContentControlListItem item)
	{
		if (m_listItems != null)
		{
			m_listItems.Remove(item);
		}
	}

	public void RemoveAt(int index)
	{
		if (m_listItems != null)
		{
			m_listItems.RemoveAt(index);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_listItems.GetEnumerator();
	}

	internal void Close()
	{
		if (m_listItems != null)
		{
			m_listItems.Clear();
			m_listItems = null;
		}
	}

	internal ContentControlListItems Clone()
	{
		ContentControlListItems contentControlListItems = (ContentControlListItems)MemberwiseClone();
		contentControlListItems.m_listItems = new List<ContentControlListItem>();
		foreach (ContentControlListItem listItem in m_listItems)
		{
			contentControlListItems.m_listItems.Add(listItem);
		}
		return contentControlListItems;
	}
}
