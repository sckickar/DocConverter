namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation.Styles;

internal class List
{
	private string m_continueList;

	private bool m_isContinueNumbering;

	private ListHeader m_listHeader;

	private ListItem m_listItem;

	internal ListItem ListItem
	{
		get
		{
			return m_listItem;
		}
		set
		{
			m_listItem = value;
		}
	}

	internal ListHeader ListHeader
	{
		get
		{
			return m_listHeader;
		}
		set
		{
			m_listHeader = value;
		}
	}

	internal bool IsContinueNumbering
	{
		get
		{
			return m_isContinueNumbering;
		}
		set
		{
			m_isContinueNumbering = value;
		}
	}

	internal string ContinueList
	{
		get
		{
			return m_continueList;
		}
		set
		{
			m_continueList = value;
		}
	}
}
