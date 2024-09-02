namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class HeaderFooterStyle
{
	private HeaderFooterProperties m_headerFooterProperties;

	private bool m_isHeader;

	private double m_headerDistance;

	private double m_footerDistance;

	internal bool IsHeader
	{
		get
		{
			return m_isHeader;
		}
		set
		{
			m_isHeader = value;
		}
	}

	internal HeaderFooterProperties HeaderFooterproperties
	{
		get
		{
			if (m_headerFooterProperties == null)
			{
				m_headerFooterProperties = new HeaderFooterProperties();
			}
			return m_headerFooterProperties;
		}
		set
		{
			m_headerFooterProperties = value;
		}
	}

	internal double HeaderDistance
	{
		get
		{
			return m_headerDistance;
		}
		set
		{
			m_headerDistance = value;
		}
	}

	internal double FooterDistance
	{
		get
		{
			return m_footerDistance;
		}
		set
		{
			m_footerDistance = value;
		}
	}

	internal void Dispose()
	{
		if (m_headerFooterProperties != null)
		{
			m_headerFooterProperties.Dispose();
			m_headerFooterProperties = null;
		}
	}
}
