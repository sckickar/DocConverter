namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class MasterPage : INamedObject
{
	private string m_name;

	private string m_pageLayoutName;

	private HeaderFooterContent m_header;

	private HeaderFooterContent m_headerLeft;

	private HeaderFooterContent m_footer;

	private HeaderFooterContent m_footerLeft;

	private HeaderFooterContent m_firstPageHeader;

	private HeaderFooterContent m_firstPageFooter;

	public string Name
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

	internal string PageLayoutName
	{
		get
		{
			return m_pageLayoutName;
		}
		set
		{
			m_pageLayoutName = value;
		}
	}

	internal HeaderFooterContent Header
	{
		get
		{
			return m_header;
		}
		set
		{
			m_header = value;
		}
	}

	internal HeaderFooterContent HeaderLeft
	{
		get
		{
			return m_headerLeft;
		}
		set
		{
			m_headerLeft = value;
		}
	}

	internal HeaderFooterContent Footer
	{
		get
		{
			return m_footer;
		}
		set
		{
			m_footer = value;
		}
	}

	internal HeaderFooterContent FooterLeft
	{
		get
		{
			return m_footerLeft;
		}
		set
		{
			m_footerLeft = value;
		}
	}

	internal HeaderFooterContent FirstPageHeader
	{
		get
		{
			return m_firstPageHeader;
		}
		set
		{
			m_firstPageHeader = value;
		}
	}

	internal HeaderFooterContent FirstPageFooter
	{
		get
		{
			return m_firstPageFooter;
		}
		set
		{
			m_firstPageFooter = value;
		}
	}

	internal void Dispose()
	{
		if (m_header != null)
		{
			m_header.Dispose();
			m_header = null;
		}
		if (m_headerLeft != null)
		{
			m_headerLeft.Dispose();
			m_headerLeft = null;
		}
		if (m_footer != null)
		{
			m_footer.Dispose();
			m_footer = null;
		}
		if (m_footerLeft != null)
		{
			m_footerLeft.Dispose();
			m_footerLeft = null;
		}
	}
}
