namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DefaultPageLayout
{
	private HeaderFooterStyle m_headerStyle;

	private HeaderFooterStyle m_footerStyle;

	private PageLayoutProperties m_pageLayoutProperties;

	internal HeaderFooterStyle HeaderStyle
	{
		get
		{
			if (m_headerStyle == null)
			{
				m_headerStyle = new HeaderFooterStyle();
			}
			return m_headerStyle;
		}
		set
		{
			m_headerStyle = value;
		}
	}

	internal HeaderFooterStyle FooterStyle
	{
		get
		{
			if (m_footerStyle == null)
			{
				m_footerStyle = new HeaderFooterStyle();
			}
			return m_footerStyle;
		}
		set
		{
			m_footerStyle = value;
		}
	}

	internal PageLayoutProperties PageLayoutProperties
	{
		get
		{
			if (m_pageLayoutProperties == null)
			{
				m_pageLayoutProperties = new PageLayoutProperties();
			}
			return m_pageLayoutProperties;
		}
		set
		{
			m_pageLayoutProperties = value;
		}
	}

	internal void Dispose()
	{
		if (m_headerStyle != null)
		{
			m_headerStyle.Dispose();
			m_headerStyle = null;
		}
		if (m_footerStyle != null)
		{
			m_footerStyle.Dispose();
			m_footerStyle = null;
		}
		if (m_pageLayoutProperties != null)
		{
			m_pageLayoutProperties.Dispose();
			m_pageLayoutProperties = null;
		}
	}
}
