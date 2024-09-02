namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFColumns
{
	private int m_columnCount;

	private int m_columnGap;

	public int ColumnGap
	{
		get
		{
			return m_columnGap;
		}
		set
		{
			m_columnGap = value;
		}
	}

	internal int ColumnCount
	{
		get
		{
			return m_columnCount;
		}
		set
		{
			m_columnCount = value;
		}
	}
}
