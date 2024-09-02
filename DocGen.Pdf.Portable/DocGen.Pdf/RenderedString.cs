using System.Collections.Generic;

namespace DocGen.Pdf;

internal class RenderedString
{
	private string m_text;

	private Dictionary<int, int> m_indexAndWidths;

	internal string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal Dictionary<int, int> IndexAndWidths
	{
		get
		{
			return m_indexAndWidths;
		}
		set
		{
			m_indexAndWidths = value;
		}
	}
}
