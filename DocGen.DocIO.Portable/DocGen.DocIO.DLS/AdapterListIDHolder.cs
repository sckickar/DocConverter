using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class AdapterListIDHolder
{
	[ThreadStatic]
	private static AdapterListIDHolder m_instance;

	private Dictionary<int, string> m_listStyleIDtoName;

	private Dictionary<int, string> m_lfoStyleIDtoName;

	internal static AdapterListIDHolder Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new AdapterListIDHolder();
			}
			return m_instance;
		}
	}

	internal Dictionary<int, string> ListStyleIDtoName
	{
		get
		{
			if (m_listStyleIDtoName == null)
			{
				m_listStyleIDtoName = new Dictionary<int, string>();
			}
			return m_listStyleIDtoName;
		}
	}

	internal Dictionary<int, string> LfoStyleIDtoName
	{
		get
		{
			if (m_lfoStyleIDtoName == null)
			{
				m_lfoStyleIDtoName = new Dictionary<int, string>();
			}
			return m_lfoStyleIDtoName;
		}
	}

	private AdapterListIDHolder()
	{
	}

	internal bool ContainsListName(string name)
	{
		bool result = false;
		IDictionaryEnumerator dictionaryEnumerator = m_listStyleIDtoName.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			if (dictionaryEnumerator.Value.Equals(name))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal void Close()
	{
		if (m_listStyleIDtoName != null)
		{
			m_listStyleIDtoName.Clear();
			m_listStyleIDtoName = null;
		}
		if (m_lfoStyleIDtoName != null)
		{
			m_lfoStyleIDtoName.Clear();
			m_lfoStyleIDtoName = null;
		}
		m_instance = null;
	}
}
