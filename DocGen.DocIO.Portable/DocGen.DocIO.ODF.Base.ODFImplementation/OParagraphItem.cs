namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OParagraphItem
{
	private TextProperties m_TextProperties;

	private ODFParagraphProperties m_ParagraphProperties;

	private string m_text;

	private bool m_span;

	private bool m_space;

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

	internal bool Space
	{
		get
		{
			return m_space;
		}
		set
		{
			m_space = value;
		}
	}

	internal bool Span
	{
		get
		{
			return m_span;
		}
		set
		{
			m_span = value;
		}
	}

	internal string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal ODFParagraphProperties ParagraphProperties
	{
		get
		{
			return m_ParagraphProperties;
		}
		set
		{
			m_ParagraphProperties = value;
		}
	}

	internal TextProperties TextProperties
	{
		get
		{
			return m_TextProperties;
		}
		set
		{
			m_TextProperties = value;
		}
	}

	internal void Dispose()
	{
		if (m_ParagraphProperties != null)
		{
			m_ParagraphProperties.Close();
			m_ParagraphProperties = null;
		}
		if (m_TextProperties != null)
		{
			m_TextProperties = null;
		}
	}
}
