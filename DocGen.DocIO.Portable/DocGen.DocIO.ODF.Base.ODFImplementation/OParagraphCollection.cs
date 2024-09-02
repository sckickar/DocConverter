using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OParagraphCollection
{
	private List<OParagraph> m_Paragraph;

	internal List<OParagraph> Paragraph
	{
		get
		{
			if (m_Paragraph == null)
			{
				m_Paragraph = new List<OParagraph>();
			}
			return m_Paragraph;
		}
		set
		{
			m_Paragraph = value;
		}
	}
}
