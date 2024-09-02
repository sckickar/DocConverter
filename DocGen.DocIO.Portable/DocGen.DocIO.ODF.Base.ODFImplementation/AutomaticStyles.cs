namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class AutomaticStyles : CommonStyles
{
	private PageLayoutCollection m_pageLayout;

	internal PageLayoutCollection PageLayoutCollection
	{
		get
		{
			if (m_pageLayout == null)
			{
				m_pageLayout = new PageLayoutCollection();
			}
			return m_pageLayout;
		}
		set
		{
			m_pageLayout = value;
		}
	}
}
