using DocGen.DocIO.ODF.Base.ODFImplementation;

namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class NumberedParagraph
{
	private bool m_isContinueNumbering;

	private int m_level;

	private int m_listId;

	private int m_startValue;

	private string m_styleName;

	private ODFParagraphProperties m_paragraphStyle;

	private Heading m_headingStyle;

	internal Heading HeadingStyle
	{
		get
		{
			return m_headingStyle;
		}
		set
		{
			m_headingStyle = value;
		}
	}

	internal ODFParagraphProperties ParagraphStyle
	{
		get
		{
			return m_paragraphStyle;
		}
		set
		{
			m_paragraphStyle = value;
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

	internal int StartValue
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

	internal int ListId
	{
		get
		{
			return m_listId;
		}
		set
		{
			m_listId = value;
		}
	}

	internal int Level
	{
		get
		{
			return m_level;
		}
		set
		{
			m_level = value;
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
}
