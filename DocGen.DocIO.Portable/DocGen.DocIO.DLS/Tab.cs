using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class Tab : XDLSSerializableBase
{
	private TabJustification m_jc;

	private TabLeader m_tlc;

	private float m_tabPosition;

	private float m_tabDeletePosition;

	internal Dictionary<int, object> m_propertiesHash;

	internal const byte DeletePositionKey = 1;

	public TabJustification Justification
	{
		get
		{
			return m_jc;
		}
		set
		{
			if (value != m_jc)
			{
				m_jc = value;
			}
			OnChange();
		}
	}

	public TabLeader TabLeader
	{
		get
		{
			return m_tlc;
		}
		set
		{
			if (value != m_tlc)
			{
				m_tlc = value;
			}
			OnChange();
		}
	}

	public float Position
	{
		get
		{
			return m_tabPosition;
		}
		set
		{
			if (value != m_tabPosition)
			{
				m_tabPosition = value;
			}
			OnChange();
		}
	}

	public float DeletePosition
	{
		get
		{
			if (HasKey(1))
			{
				return (float)PropertiesHash[1];
			}
			return m_tabDeletePosition;
		}
		set
		{
			m_tabDeletePosition = value;
			OnChange();
			SetKeyValue(1, value);
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal Tab(IWordDocument doc)
		: base((WordDocument)doc, null)
	{
	}

	internal Tab(IWordDocument doc, float position, TabJustification justification, TabLeader leader)
		: this(doc, position, 0f, justification, leader)
	{
	}

	internal Tab(IWordDocument doc, float position, float deletePosition, TabJustification justification, TabLeader leader)
		: this(doc)
	{
		m_tabPosition = position;
		m_jc = justification;
		m_tlc = leader;
		m_tabDeletePosition = deletePosition;
	}

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		writer.WriteValue("Position", Position);
		writer.WriteValue("Justification", Justification);
		writer.WriteValue("Leader", TabLeader);
		writer.WriteValue("Delete", DeletePosition);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		if (reader.HasAttribute("Position"))
		{
			m_tabPosition = reader.ReadFloat("Position");
		}
		if (reader.HasAttribute("Justification"))
		{
			m_jc = (TabJustification)(object)reader.ReadEnum("Justification", typeof(TabJustification));
		}
		if (reader.HasAttribute("Leader"))
		{
			m_tlc = (TabLeader)(object)reader.ReadEnum("Leader", typeof(TabLeader));
		}
		if (reader.HasAttribute("Delete"))
		{
			m_tabDeletePosition = reader.ReadFloat("Delete");
		}
	}

	internal Tab Clone()
	{
		return (Tab)CloneImpl();
	}

	private void OnChange()
	{
		if (base.OwnerBase != null)
		{
			(base.OwnerBase as TabCollection).OnChange();
		}
	}

	internal bool Compare(Tab tab)
	{
		if (DeletePosition != tab.DeletePosition)
		{
			return false;
		}
		if (Justification != tab.Justification)
		{
			return false;
		}
		if (Position != tab.Position)
		{
			return false;
		}
		if (TabLeader != tab.TabLeader)
		{
			return false;
		}
		return true;
	}
}
