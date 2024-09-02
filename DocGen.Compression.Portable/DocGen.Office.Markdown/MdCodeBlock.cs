using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MdCodeBlock : IMdBlock
{
	private List<string> m_lines;

	private bool m_isFencedCode = true;

	internal List<string> Lines
	{
		get
		{
			if (m_lines == null)
			{
				m_lines = new List<string>();
			}
			return m_lines;
		}
		set
		{
			m_lines = value;
		}
	}

	internal bool IsFencedCode
	{
		get
		{
			return m_isFencedCode;
		}
		set
		{
			m_isFencedCode = value;
		}
	}

	public void Close()
	{
		if (m_lines != null)
		{
			m_lines.Clear();
			m_lines = null;
		}
	}
}
