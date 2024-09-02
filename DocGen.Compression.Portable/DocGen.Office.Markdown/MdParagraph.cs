using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MdParagraph : IMdBlock
{
	private List<IMdInline> m_inLines;

	private MdParagraphStyle m_styleName;

	private MdListFormat m_listFormat;

	private bool isHorizontaRule;

	private const char blockQuoteChar = '>';

	private MdTaskProperties m_taskItemProperties;

	internal List<IMdInline> Inlines
	{
		get
		{
			if (m_inLines == null)
			{
				m_inLines = new List<IMdInline>();
			}
			return m_inLines;
		}
		set
		{
			m_inLines = value;
		}
	}

	internal MdParagraphStyle StyleName
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

	internal MdListFormat ListFormat
	{
		get
		{
			return m_listFormat;
		}
		set
		{
			m_listFormat = value;
		}
	}

	internal MdTaskProperties TaskItemProperties
	{
		get
		{
			return m_taskItemProperties;
		}
		set
		{
			m_taskItemProperties = value;
		}
	}

	internal bool IsHorizontalRule
	{
		get
		{
			return isHorizontaRule;
		}
		set
		{
			isHorizontaRule = value;
		}
	}

	internal MdTextRange AddMdTextRange()
	{
		MdTextRange mdTextRange = new MdTextRange();
		Inlines.Add(mdTextRange);
		return mdTextRange;
	}

	internal MdHyperlink AddMdHyperlink()
	{
		MdHyperlink mdHyperlink = new MdHyperlink();
		Inlines.Add(mdHyperlink);
		return mdHyperlink;
	}

	internal MdPicture AddPicture()
	{
		MdPicture mdPicture = new MdPicture();
		Inlines.Add(mdPicture);
		return mdPicture;
	}

	internal void ApplyParagraphStyle(string styleName, MdParagraph mdParagraph)
	{
		switch (styleName)
		{
		case "Heading 1":
			mdParagraph.StyleName = MdParagraphStyle.Heading1;
			break;
		case "Heading 2":
			mdParagraph.StyleName = MdParagraphStyle.Heading2;
			break;
		case "Heading 3":
			mdParagraph.StyleName = MdParagraphStyle.Heading3;
			break;
		case "Heading 4":
			mdParagraph.StyleName = MdParagraphStyle.Heading4;
			break;
		case "Heading 5":
			mdParagraph.StyleName = MdParagraphStyle.Heading5;
			break;
		case "Heading 6":
			mdParagraph.StyleName = MdParagraphStyle.Heading6;
			break;
		case "Quote":
			mdParagraph.StyleName = MdParagraphStyle.BlockQuote;
			break;
		default:
			mdParagraph.StyleName = MdParagraphStyle.None;
			break;
		}
	}

	internal string GetCharForParaStyle(MdParagraph paragraph, string parastyleChar)
	{
		if (paragraph.Inlines.Count > 0)
		{
			switch (paragraph.StyleName)
			{
			case MdParagraphStyle.Heading1:
				parastyleChar = "# ";
				break;
			case MdParagraphStyle.Heading2:
				parastyleChar = "## ";
				break;
			case MdParagraphStyle.Heading3:
				parastyleChar = "### ";
				break;
			case MdParagraphStyle.Heading4:
				parastyleChar = "#### ";
				break;
			case MdParagraphStyle.Heading5:
				parastyleChar = "##### ";
				break;
			case MdParagraphStyle.Heading6:
				parastyleChar = "###### ";
				break;
			case MdParagraphStyle.BlockQuote:
				parastyleChar = '>'.ToString();
				break;
			}
		}
		return parastyleChar;
	}

	public void Close()
	{
		foreach (IMdInline inline in Inlines)
		{
			if (inline is MdPicture)
			{
				(inline as MdPicture).Close();
			}
		}
		if (m_inLines != null)
		{
			m_inLines.Clear();
			m_inLines = null;
		}
		m_listFormat = null;
		m_taskItemProperties = null;
	}
}
