using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class HeaderFooterProperties : MarginBorderProperties
{
	private double m_minHeight;

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

	internal double MinHeight
	{
		get
		{
			return m_minHeight;
		}
		set
		{
			m_minHeight = value;
		}
	}

	internal new void Dispose()
	{
		base.Dispose();
	}
}
