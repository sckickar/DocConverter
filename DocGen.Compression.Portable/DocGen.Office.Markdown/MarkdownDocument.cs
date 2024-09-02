using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MarkdownDocument
{
	private List<IMdBlock> m_blocks;

	internal List<IMdBlock> Blocks
	{
		get
		{
			if (m_blocks == null)
			{
				m_blocks = new List<IMdBlock>();
			}
			return m_blocks;
		}
		set
		{
			m_blocks = value;
		}
	}

	internal MdParagraph AddMdParagraph()
	{
		MdParagraph mdParagraph = new MdParagraph();
		Blocks.Add(mdParagraph);
		return mdParagraph;
	}

	internal MdThematicBreak AddMdThematicBreak()
	{
		MdThematicBreak mdThematicBreak = new MdThematicBreak();
		Blocks.Add(mdThematicBreak);
		return mdThematicBreak;
	}

	internal MdCodeBlock AddMdCodeBlock()
	{
		MdCodeBlock mdCodeBlock = new MdCodeBlock();
		Blocks.Add(mdCodeBlock);
		return mdCodeBlock;
	}

	internal MdTable AddMdTable()
	{
		MdTable mdTable = new MdTable();
		Blocks.Add(mdTable);
		return mdTable;
	}

	internal void Dispose()
	{
		foreach (IMdBlock block in Blocks)
		{
			block.Close();
		}
		if (m_blocks != null)
		{
			m_blocks.Clear();
			m_blocks = null;
		}
	}
}
