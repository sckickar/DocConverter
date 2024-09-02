using System.Collections.Generic;

namespace DocGen.Pdf;

public class TextLineCollection
{
	private List<TextLine> m_textLine = new List<TextLine>();

	public List<TextLine> TextLine
	{
		get
		{
			return m_textLine;
		}
		internal set
		{
			m_textLine = value;
		}
	}
}
