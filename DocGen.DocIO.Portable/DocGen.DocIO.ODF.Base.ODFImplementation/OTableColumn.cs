namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableColumn
{
	private string m_defaultCellStyleName;

	private int m_repeatedRowColumns;

	private string m_styleName;

	private bool m_visibility;

	private int m_outlineLevel;

	private bool m_isCollapsed;

	internal string DefaultCellStyleName
	{
		get
		{
			return m_defaultCellStyleName;
		}
		set
		{
			m_defaultCellStyleName = value;
		}
	}

	internal int RepeatedRowColumns
	{
		get
		{
			return m_repeatedRowColumns;
		}
		set
		{
			m_repeatedRowColumns = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal bool Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
		}
	}

	internal int OutlineLevel
	{
		get
		{
			return m_outlineLevel;
		}
		set
		{
			m_outlineLevel = value;
		}
	}

	internal bool IsCollapsed
	{
		get
		{
			return m_isCollapsed;
		}
		set
		{
			m_isCollapsed = value;
		}
	}
}
