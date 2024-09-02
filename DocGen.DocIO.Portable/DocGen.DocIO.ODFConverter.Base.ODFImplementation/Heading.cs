using DocGen.DocIO.ODF.Base.ODFImplementation;

namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class Heading : ODFParagraphProperties
{
	private string m_classNames;

	private string m_condStyleName;

	private int m_id;

	private bool m_isListHeader;

	private int m_outlineLevel;

	private bool m_restartNumbering;

	private uint m_startValue;

	private string m_styleName;

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

	internal uint StartValue
	{
		get
		{
			return m_startValue;
		}
		set
		{
			m_startValue = value;
		}
	}

	internal bool RestartNumbering
	{
		get
		{
			return m_restartNumbering;
		}
		set
		{
			m_restartNumbering = value;
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

	internal bool IsListHeader
	{
		get
		{
			return m_isListHeader;
		}
		set
		{
			m_isListHeader = value;
		}
	}

	internal int Id
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	internal string CondStyleName
	{
		get
		{
			return m_condStyleName;
		}
		set
		{
			m_condStyleName = value;
		}
	}

	internal string ClassNames
	{
		get
		{
			return m_classNames;
		}
		set
		{
			m_classNames = value;
		}
	}
}
