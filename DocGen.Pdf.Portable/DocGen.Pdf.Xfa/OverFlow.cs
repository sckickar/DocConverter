using System.Xml;

namespace DocGen.Pdf.Xfa;

internal class OverFlow
{
	private string m_overFlowID = string.Empty;

	internal string OverFlowID
	{
		get
		{
			return m_overFlowID;
		}
		set
		{
			m_overFlowID = value;
		}
	}

	internal void Read(XmlNode node)
	{
		if (node.Attributes["target"] != null)
		{
			OverFlowID = node.Attributes["target"].Value;
		}
	}
}
