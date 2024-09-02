namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableColumnProperties
{
	private double m_columnWidth;

	private bool m_useOptimalColumnWidth;

	internal double ColumnWidth
	{
		get
		{
			return m_columnWidth;
		}
		set
		{
			m_columnWidth = value;
		}
	}

	internal bool UseOptimalColumnWidth
	{
		get
		{
			return m_useOptimalColumnWidth;
		}
		set
		{
			m_useOptimalColumnWidth = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OTableColumnProperties oTableColumnProperties))
		{
			return false;
		}
		return m_columnWidth == oTableColumnProperties.ColumnWidth;
	}
}
