using System.Collections.Generic;
using DocGen.DocIO.ODF.Base.ODFImplementation;

namespace DocGen.DocIO.ODTImplementation;

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
