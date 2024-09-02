namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class SectionProperties
{
	private string m_backgroundColor;

	private int m_marginLeft;

	private int m_marginRight;

	private ODFColumns m_columns;

	internal ODFColumns Columns
	{
		get
		{
			return m_columns;
		}
		set
		{
			m_columns = value;
		}
	}

	internal string BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_backgroundColor = value;
		}
	}

	internal int MarginLeft
	{
		get
		{
			return m_marginLeft;
		}
		set
		{
			m_marginLeft = value;
		}
	}

	internal int MarginRight
	{
		get
		{
			return m_marginRight;
		}
		set
		{
			m_marginRight = value;
		}
	}

	internal void Close()
	{
		if (m_columns != null)
		{
			m_columns = null;
		}
	}
}
