using System.Collections.Generic;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OBody
{
	private Text m_text;

	private List<OTextBodyItem> m_textBodyItem;

	internal List<OTextBodyItem> TextBodyItems
	{
		get
		{
			if (m_textBodyItem == null)
			{
				m_textBodyItem = new List<OTextBodyItem>();
			}
			return m_textBodyItem;
		}
		set
		{
			m_textBodyItem = value;
		}
	}

	internal void Close()
	{
		if (m_textBodyItem != null)
		{
			m_textBodyItem.Clear();
			m_textBodyItem = null;
		}
	}
}
