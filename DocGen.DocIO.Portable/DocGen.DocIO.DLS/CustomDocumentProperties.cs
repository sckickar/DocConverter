using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class CustomDocumentProperties : XDLSSerializableBase
{
	internal const string TagName = "property";

	internal const string NameAttribute = "name";

	internal const string PIDAttribute = "pid";

	internal const string FMTIDAttribute = "fmtid";

	protected Dictionary<string, DocumentProperty> m_customList;

	internal Dictionary<string, DocumentProperty> CustomHash => m_customList;

	public DocumentProperty this[string name]
	{
		get
		{
			if (m_customList.ContainsKey(name))
			{
				return m_customList[name];
			}
			return null;
		}
	}

	public DocumentProperty this[int index]
	{
		get
		{
			int num = 0;
			foreach (string key in m_customList.Keys)
			{
				if (num == index)
				{
					return m_customList[key];
				}
				num++;
			}
			return null;
		}
	}

	public int Count => m_customList.Count;

	internal CustomDocumentProperties()
		: this(0)
	{
	}

	internal CustomDocumentProperties(int count)
		: base(null, null)
	{
		m_customList = new Dictionary<string, DocumentProperty>(count);
	}

	public DocumentProperty Add(string name, object value)
	{
		DocumentProperty documentProperty = new DocumentProperty(name, value, DocumentProperty.DetectPropertyType(value));
		m_customList.Add(name, documentProperty);
		return documentProperty;
	}

	public void Remove(string name)
	{
		CustomHash.Remove(name);
	}

	public CustomDocumentProperties Clone()
	{
		CustomDocumentProperties customDocumentProperties = new CustomDocumentProperties(m_customList.Count);
		foreach (string key in m_customList.Keys)
		{
			DocumentProperty documentProperty = m_customList[key];
			customDocumentProperties.m_customList.Add(key, documentProperty.Clone());
		}
		return customDocumentProperties;
	}

	internal override void Close()
	{
		base.Close();
		if (m_customList == null)
		{
			return;
		}
		foreach (DocumentProperty value in m_customList.Values)
		{
			value.Close();
		}
		m_customList.Clear();
		m_customList = null;
	}
}
