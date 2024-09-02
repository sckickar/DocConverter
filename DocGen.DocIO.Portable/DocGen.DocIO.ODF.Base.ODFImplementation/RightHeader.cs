namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class RightHeader
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
}
