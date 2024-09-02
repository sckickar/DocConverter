namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableRowProperties
{
	private double m_rowHeight;

	private bool m_useOptimalRowHeight;

	private bool m_isBreakAcrossPages;

	private bool m_isHeaderRow;

	internal double RowHeight
	{
		get
		{
			return m_rowHeight;
		}
		set
		{
			m_rowHeight = value;
		}
	}

	internal bool UseOptimalRowHeight
	{
		get
		{
			return m_useOptimalRowHeight;
		}
		set
		{
			m_useOptimalRowHeight = value;
		}
	}

	internal bool IsBreakAcrossPages
	{
		get
		{
			return m_isBreakAcrossPages;
		}
		set
		{
			m_isBreakAcrossPages = value;
		}
	}

	internal bool IsHeaderRow
	{
		get
		{
			return m_isHeaderRow;
		}
		set
		{
			m_isHeaderRow = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OTableRowProperties oTableRowProperties))
		{
			return false;
		}
		return m_rowHeight == oTableRowProperties.RowHeight;
	}
}
