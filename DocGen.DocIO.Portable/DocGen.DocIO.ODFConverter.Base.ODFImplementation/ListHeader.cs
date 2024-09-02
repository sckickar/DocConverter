using DocGen.DocIO.ODF.Base.ODFImplementation;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation.Styles;

namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class ListHeader
{
	private Heading m_heading;

	private List m_list;

	private ODFParagraphProperties m_paragraph;

	internal ODFParagraphProperties Paragraph
	{
		get
		{
			return m_paragraph;
		}
		set
		{
			m_paragraph = value;
		}
	}

	internal List List
	{
		get
		{
			return m_list;
		}
		set
		{
			m_list = value;
		}
	}

	internal Heading Heading
	{
		get
		{
			return m_heading;
		}
		set
		{
			m_heading = value;
		}
	}
}
