using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DataStyle : NumberFormat
{
	private string m_name;

	private string m_displayName;

	private bool m_volatile;

	private List<string> m_text;

	private List<MapStyle> m_map;

	private TextProperties m_textProperties;

	private bool m_hasSections;

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

	internal string DisplayName
	{
		get
		{
			return m_displayName;
		}
		set
		{
			m_displayName = value;
		}
	}

	internal bool Volatile
	{
		get
		{
			return m_volatile;
		}
		set
		{
			m_volatile = value;
		}
	}

	internal List<string> Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal List<MapStyle> Map
	{
		get
		{
			if (m_map == null)
			{
				m_map = new List<MapStyle>();
			}
			return m_map;
		}
		set
		{
			m_map = value;
		}
	}

	internal TextProperties TextProperties
	{
		get
		{
			return m_textProperties;
		}
		set
		{
			m_textProperties = value;
		}
	}

	internal bool HasSections
	{
		get
		{
			return m_hasSections;
		}
		set
		{
			m_hasSections = value;
		}
	}

	internal void Dispose1()
	{
		if (m_text != null)
		{
			m_text.Clear();
			m_text = null;
		}
		if (m_map != null)
		{
			m_map.Clear();
			m_map = null;
		}
		if (m_textProperties != null)
		{
			m_textProperties = null;
		}
	}
}
