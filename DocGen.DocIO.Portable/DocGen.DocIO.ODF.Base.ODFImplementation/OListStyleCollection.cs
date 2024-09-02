using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OListStyleCollection
{
	private List<OListStyle> m_listStyles;

	internal List<OListStyle> OListStyles
	{
		get
		{
			if (m_listStyles == null)
			{
				m_listStyles = new List<OListStyle>();
			}
			return m_listStyles;
		}
	}
}
