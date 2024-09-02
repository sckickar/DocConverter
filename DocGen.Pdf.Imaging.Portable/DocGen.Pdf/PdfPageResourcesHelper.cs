using System;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class PdfPageResourcesHelper
{
	private Dictionary<string, object> m_resources;

	internal Dictionary<string, FontStructureHelperBase> fontCollection = new Dictionary<string, FontStructureHelperBase>();

	public Dictionary<string, object> Resources => m_resources;

	public object this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (m_resources.ContainsKey(key))
			{
				return m_resources[key];
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			m_resources[key] = value;
		}
	}

	public bool isSameFont()
	{
		int num = 0;
		foreach (KeyValuePair<string, FontStructureHelperBase> item in fontCollection)
		{
			foreach (KeyValuePair<string, FontStructureHelperBase> item2 in fontCollection)
			{
				if (item.Value.FontName != item2.Value.FontName)
				{
					num = 1;
				}
			}
		}
		if (num == 0)
		{
			return true;
		}
		return false;
	}

	public PdfPageResourcesHelper()
	{
		m_resources = new Dictionary<string, object>();
	}

	public void Add(string resourceName, object resource)
	{
		if (!string.Equals(resourceName, "ProcSet") && !m_resources.ContainsKey(resourceName))
		{
			m_resources.Add(resourceName, resource);
			if (resource.GetType().Name == "FontStructure")
			{
				fontCollection.Add(resourceName, resource as FontStructureHelperNet);
			}
		}
	}

	public bool ContainsKey(string key)
	{
		return m_resources.ContainsKey(key);
	}

	internal void Dispose()
	{
		if (m_resources != null)
		{
			m_resources.Clear();
			m_resources = null;
		}
		if (fontCollection != null)
		{
			fontCollection.Clear();
			fontCollection = null;
		}
	}
}
