using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OListStyle
{
	private List<ListLevelProperties> m_listLevels;

	private string m_name;

	private string m_currentStyleName;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string CurrentStyleName
	{
		get
		{
			return m_currentStyleName;
		}
		set
		{
			m_currentStyleName = value;
		}
	}

	internal List<ListLevelProperties> ListLevels
	{
		get
		{
			if (m_listLevels == null)
			{
				m_listLevels = new List<ListLevelProperties>();
			}
			return m_listLevels;
		}
	}

	internal void Close()
	{
		if (m_listLevels == null)
		{
			return;
		}
		foreach (ListLevelProperties listLevel in m_listLevels)
		{
			listLevel.Close();
		}
		m_listLevels.Clear();
		m_listLevels = null;
	}
}
