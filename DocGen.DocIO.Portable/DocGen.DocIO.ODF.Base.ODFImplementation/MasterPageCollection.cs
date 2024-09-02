using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class MasterPageCollection : CollectionBase<MasterPage>
{
	private Dictionary<string, MasterPage> m_dictMasterPages;

	internal Dictionary<string, MasterPage> DictMasterPages
	{
		get
		{
			if (m_dictMasterPages == null)
			{
				m_dictMasterPages = new Dictionary<string, MasterPage>();
			}
			return m_dictMasterPages;
		}
		set
		{
			m_dictMasterPages = value;
		}
	}

	internal string Add(MasterPage page)
	{
		string text = page.Name;
		if (string.IsNullOrEmpty(page.Name))
		{
			text = CollectionBase<MasterPage>.GenerateDefaultName("mp", DictMasterPages.Values);
		}
		if (!DictMasterPages.ContainsKey(text))
		{
			string text2 = ContainsValue(page);
			if (text2 != null)
			{
				text = text2;
			}
			else
			{
				page.Name = text;
				DictMasterPages.Add(text, page);
			}
		}
		return text;
	}

	private string ContainsValue(MasterPage page)
	{
		string result = null;
		foreach (MasterPage value in DictMasterPages.Values)
		{
			if (value.Equals(page))
			{
				result = value.Name;
				break;
			}
		}
		return result;
	}

	internal void Remove(string key)
	{
		if (DictMasterPages.ContainsKey(key))
		{
			DictMasterPages.Remove(key);
		}
	}

	internal void Dispose()
	{
		if (m_dictMasterPages == null)
		{
			return;
		}
		foreach (MasterPage value in m_dictMasterPages.Values)
		{
			value.Dispose();
		}
		m_dictMasterPages.Clear();
		m_dictMasterPages = null;
	}
}
