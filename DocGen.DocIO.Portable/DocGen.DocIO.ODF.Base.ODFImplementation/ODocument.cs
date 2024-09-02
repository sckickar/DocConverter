using System.Collections.Generic;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODocument
{
	private OBody m_body;

	private Dictionary<string, ImageRecord> m_documentImages;

	private List<OListStyle> m_listStyles;

	private List<ODFStyle> m_tocStyles;

	internal OBody Body
	{
		get
		{
			if (m_body == null)
			{
				m_body = new OBody();
			}
			return m_body;
		}
		set
		{
			m_body = value;
		}
	}

	internal Dictionary<string, ImageRecord> DocumentImages
	{
		get
		{
			if (m_documentImages == null)
			{
				m_documentImages = new Dictionary<string, ImageRecord>();
			}
			return m_documentImages;
		}
	}

	internal List<OListStyle> ListStyles
	{
		get
		{
			if (m_listStyles == null)
			{
				m_listStyles = new List<OListStyle>();
			}
			return m_listStyles;
		}
	}

	internal List<ODFStyle> TOCStyles
	{
		get
		{
			if (m_tocStyles == null)
			{
				m_tocStyles = new List<ODFStyle>();
			}
			return m_tocStyles;
		}
		set
		{
			m_tocStyles = value;
		}
	}

	internal void Close()
	{
		if (m_body != null)
		{
			m_body.Close();
			m_body = null;
		}
		if (m_documentImages != null)
		{
			m_documentImages.Clear();
			m_documentImages = null;
		}
		if (m_listStyles != null)
		{
			foreach (OListStyle listStyle in m_listStyles)
			{
				listStyle.Close();
			}
			m_listStyles.Clear();
			m_listStyles = null;
		}
		if (m_tocStyles == null)
		{
			return;
		}
		foreach (ODFStyle tocStyle in m_tocStyles)
		{
			tocStyle.Close();
		}
		m_tocStyles.Clear();
		m_tocStyles = null;
	}
}
