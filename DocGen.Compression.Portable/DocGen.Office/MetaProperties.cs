using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DocGen.Office;

public class MetaProperties
{
	private List<object> m_innerList;

	internal XDocument m_contentTypeSchemaProperties;

	public MetaProperty this[int index] => InnerList[index] as MetaProperty;

	public int Count => m_innerList.Count;

	internal IList InnerList => m_innerList;

	public MetaProperty FindByName(string name)
	{
		foreach (MetaProperty inner in m_innerList)
		{
			if (inner.DisplayName == name)
			{
				return inner;
			}
		}
		return null;
	}

	internal MetaProperties()
	{
		m_innerList = new List<object>();
	}

	internal void Add(MetaProperty metaProperty)
	{
		InnerList.Add(metaProperty);
		metaProperty.Parent = this;
	}

	internal void Remove(MetaProperty metaProperty)
	{
		InnerList.Remove(metaProperty);
	}

	internal void Close()
	{
		while (InnerList.Count > 0)
		{
			int index = InnerList.Count - 1;
			Remove(this[index]);
		}
	}
}
