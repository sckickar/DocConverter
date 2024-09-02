using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class FdfObjectCollection
{
	private Dictionary<string, IPdfPrimitive> m_objects;

	internal Dictionary<string, IPdfPrimitive> Objects
	{
		get
		{
			return m_objects;
		}
		set
		{
			m_objects = value;
		}
	}

	internal FdfObjectCollection()
	{
		m_objects = new Dictionary<string, IPdfPrimitive>();
	}

	internal void Add(string key, IPdfPrimitive value)
	{
		if (m_objects.ContainsKey(key))
		{
			m_objects[key] = value;
		}
		else
		{
			m_objects.Add(key, value);
		}
	}

	internal void Dispose()
	{
		m_objects.Clear();
		m_objects = null;
	}
}
