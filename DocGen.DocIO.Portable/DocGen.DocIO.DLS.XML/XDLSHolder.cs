using System.Collections.Generic;
using System.Xml;

namespace DocGen.DocIO.DLS.XML;

public class XDLSHolder
{
	private int m_id = -1;

	private Dictionary<string, object> m_hashElements;

	private Dictionary<string, object> m_hashRefElements;

	private byte m_bFlags = 1;

	public int ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public bool Cleared
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			if (value != ((m_bFlags & 1) != 0))
			{
				if (value)
				{
					Clear();
				}
				else
				{
					m_bFlags = (byte)((m_bFlags & 0xFEu) | 0u);
				}
			}
		}
	}

	public bool EnableID
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public bool SkipMe
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public void AddElement(string tagName, object value)
	{
		if (m_hashElements == null)
		{
			m_hashElements = new Dictionary<string, object>();
		}
		m_hashElements[tagName] = value;
	}

	public void AddRefElement(string tagName, object value)
	{
		if (m_hashRefElements == null)
		{
			m_hashRefElements = new Dictionary<string, object>();
		}
		m_hashRefElements[tagName] = value;
	}

	public void WriteHolder(IXDLSContentWriter writer)
	{
		if (m_hashElements != null)
		{
			foreach (string key in m_hashElements.Keys)
			{
				writer.WriteChildElement(key, m_hashElements[key]);
			}
		}
		if (m_hashRefElements == null)
		{
			return;
		}
		foreach (string key2 in m_hashRefElements.Keys)
		{
			if (m_hashRefElements[key2] is IXDLSSerializable iXDLSSerializable)
			{
				writer.WriteChildRefElement(key2, iXDLSSerializable.XDLSHolder.ID);
			}
		}
	}

	public bool ReadHolder(IXDLSContentReader reader)
	{
		if (reader.NodeType == XmlNodeType.Element)
		{
			string tagName = reader.TagName;
			if (m_hashElements != null && m_hashElements.ContainsKey(tagName))
			{
				object obj = m_hashElements[tagName];
				if (obj != null)
				{
					if (obj is IXDLSFactory iXDLSFactory)
					{
						obj = iXDLSFactory.Create(reader);
						m_hashElements[tagName] = obj;
					}
					return reader.ReadChildElement(obj);
				}
			}
			if (m_hashRefElements != null && m_hashRefElements.ContainsKey(tagName))
			{
				string attributeValue = reader.GetAttributeValue("ref");
				if (attributeValue == null)
				{
					m_hashRefElements[reader.TagName] = -1;
				}
				else
				{
					m_hashRefElements[reader.TagName] = XmlConvert.ToInt32(attributeValue);
				}
				return false;
			}
		}
		return false;
	}

	public void AfterDeserialization(IXDLSSerializable owner)
	{
		if (m_hashElements != null)
		{
			foreach (string key in m_hashElements.Keys)
			{
				if (m_hashElements[key] is IXDLSSerializable iXDLSSerializable)
				{
					iXDLSSerializable.XDLSHolder.AfterDeserialization(iXDLSSerializable);
				}
				else
				{
					if (!(m_hashElements[key] is IXDLSSerializableCollection iXDLSSerializableCollection))
					{
						continue;
					}
					foreach (IXDLSSerializable item in iXDLSSerializableCollection)
					{
						item?.XDLSHolder.AfterDeserialization(item);
					}
				}
			}
		}
		if (m_hashRefElements != null)
		{
			foreach (string key2 in m_hashRefElements.Keys)
			{
				int value = -1;
				if (m_hashRefElements[key2] != null)
				{
					value = (int)m_hashRefElements[key2];
				}
				owner.RestoreReference(key2, value);
			}
		}
		Clear();
	}

	public void BeforeSerialization()
	{
		if (m_hashElements == null)
		{
			return;
		}
		foreach (string key in m_hashElements.Keys)
		{
			if (m_hashElements[key] is IXDLSSerializable iXDLSSerializable)
			{
				iXDLSSerializable.XDLSHolder.Cleared = true;
				iXDLSSerializable.XDLSHolder.BeforeSerialization();
			}
			else
			{
				if (!(m_hashElements[key] is IXDLSSerializableCollection iXDLSSerializableCollection))
				{
					continue;
				}
				int num = 0;
				foreach (IXDLSSerializable item in iXDLSSerializableCollection)
				{
					if (item != null)
					{
						item.XDLSHolder.Cleared = true;
						item.XDLSHolder.ID = num;
						item.XDLSHolder.BeforeSerialization();
						num++;
					}
				}
			}
		}
	}

	private void Clear()
	{
		if (m_hashElements != null)
		{
			m_hashElements.Clear();
		}
		if (m_hashRefElements != null)
		{
			m_hashRefElements.Clear();
		}
		m_bFlags = (byte)((m_bFlags & 0xFEu) | 1u);
	}

	internal void Close()
	{
		if (m_hashElements != null)
		{
			m_hashElements.Clear();
			m_hashElements = null;
		}
		if (m_hashRefElements != null)
		{
			m_hashRefElements.Clear();
			m_hashRefElements = null;
		}
	}
}
