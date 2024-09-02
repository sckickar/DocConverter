namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class PageNumber
{
	private PageNumberFormat m_pageNumber;

	private bool m_numberLetterSync;

	private bool m_pageFixed;

	private int m_pageAdjust;

	private SelectPage m_selectPage;

	private string m_content;

	internal SelectPage SelectPage
	{
		get
		{
			return m_selectPage;
		}
		set
		{
			m_selectPage = value;
		}
	}

	internal int PageAdjust
	{
		get
		{
			return m_pageAdjust;
		}
		set
		{
			m_pageAdjust = value;
		}
	}

	internal bool PageFixed
	{
		get
		{
			return m_pageFixed;
		}
		set
		{
			m_pageFixed = value;
		}
	}

	internal bool NumberLetterSync
	{
		get
		{
			return m_numberLetterSync;
		}
		set
		{
			m_numberLetterSync = value;
		}
	}

	internal PageNumberFormat PgNumber
	{
		get
		{
			return m_pageNumber;
		}
		set
		{
			m_pageNumber = value;
		}
	}

	internal string Content
	{
		get
		{
			return m_content;
		}
		set
		{
			m_content = value;
		}
	}
}
