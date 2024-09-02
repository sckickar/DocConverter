namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class HeaderSection
{
	private OParagraph m_paragraph;

	internal OParagraph Paragraph
	{
		get
		{
			if (m_paragraph == null)
			{
				m_paragraph = new OParagraph();
			}
			return m_paragraph;
		}
		set
		{
			m_paragraph = value;
		}
	}

	internal void Dispose()
	{
		if (m_paragraph != null)
		{
			m_paragraph.Dispose();
			m_paragraph = null;
		}
	}
}
