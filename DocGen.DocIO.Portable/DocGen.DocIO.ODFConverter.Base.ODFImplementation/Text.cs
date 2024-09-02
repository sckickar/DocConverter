using DocGen.DocIO.ODF.Base.ODFImplementation;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation.Styles;

namespace DocGen.DocIO.ODFConverter.Base.ODFImplementation;

internal class Text
{
	private OParagraph m_paragraph;

	private Heading m_heading;

	private List m_list;

	private NumberedParagraph m_numberedParagraph;

	private Section m_section;

	private OTableOfContent m_tableOfContent;

	private bool m_isSoftPageBreak;

	private OParagraphCollection m_paraItem;

	internal OParagraphCollection ParagraphItem
	{
		get
		{
			if (m_paraItem == null)
			{
				m_paraItem = new OParagraphCollection();
			}
			return m_paraItem;
		}
		set
		{
			m_paraItem = value;
		}
	}

	internal bool IsSoftPageBreak
	{
		get
		{
			return m_isSoftPageBreak;
		}
		set
		{
			m_isSoftPageBreak = value;
		}
	}

	internal OTableOfContent TableOfContent
	{
		get
		{
			return m_tableOfContent;
		}
		set
		{
			m_tableOfContent = value;
		}
	}

	internal Section Section
	{
		get
		{
			return m_section;
		}
		set
		{
			m_section = value;
		}
	}

	internal NumberedParagraph NumberedParagraph
	{
		get
		{
			return m_numberedParagraph;
		}
		set
		{
			m_numberedParagraph = value;
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
