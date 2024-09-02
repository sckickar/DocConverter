using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.DLS;

public class XmlTableFormat
{
	private List<Stream> m_nodeArr;

	private string m_styleName;

	private WTable m_ownerTable;

	internal List<Stream> NodeArray
	{
		get
		{
			if (m_nodeArr == null)
			{
				m_nodeArr = new List<Stream>();
			}
			return m_nodeArr;
		}
		set
		{
			m_nodeArr = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal RowFormat Format => m_ownerTable.TableFormat;

	internal bool HasFormat
	{
		get
		{
			if (m_styleName != null || (m_nodeArr != null && m_nodeArr.Count > 0))
			{
				return true;
			}
			return false;
		}
	}

	internal WTable Owner => m_ownerTable;

	internal XmlTableFormat(WTable owner)
	{
		m_ownerTable = owner;
	}

	internal XmlTableFormat Clone(WTable ownerTable)
	{
		XmlTableFormat xmlTableFormat = new XmlTableFormat(ownerTable);
		xmlTableFormat.StyleName = m_styleName;
		xmlTableFormat.Owner.SetOwner(ownerTable.OwnerTextBody);
		xmlTableFormat.NodeArray = m_nodeArr;
		return xmlTableFormat;
	}

	internal void Close()
	{
		if (m_nodeArr != null)
		{
			m_nodeArr.Clear();
			m_nodeArr = null;
		}
	}
}
