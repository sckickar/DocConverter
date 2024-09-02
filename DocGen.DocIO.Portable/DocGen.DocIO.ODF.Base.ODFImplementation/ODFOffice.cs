namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFOffice
{
	private DocumentContent m_docContent;

	private DocumentStyles m_docStyles;

	internal DocumentContent DocContent
	{
		get
		{
			if (m_docContent == null)
			{
				m_docContent = new DocumentContent();
			}
			return m_docContent;
		}
		set
		{
			m_docContent = value;
		}
	}

	internal DocumentStyles DocStyles
	{
		get
		{
			if (m_docStyles == null)
			{
				m_docStyles = new DocumentStyles();
			}
			return m_docStyles;
		}
		set
		{
			m_docStyles = value;
		}
	}
}
