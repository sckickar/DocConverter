using System.Xml;

namespace DocGen.Pdf.Xfa;

internal class BreakBefore
{
	private string m_beforeTargetID = string.Empty;

	private PdfXfaTargetType m_targetType = PdfXfaTargetType.PageArea;

	private bool m_isStartNew;

	internal PdfXfaTargetType TargetType
	{
		get
		{
			return m_targetType;
		}
		set
		{
			m_targetType = value;
		}
	}

	internal string BeforeTargetID
	{
		get
		{
			return m_beforeTargetID;
		}
		set
		{
			m_beforeTargetID = value;
		}
	}

	internal bool IsStartNew
	{
		get
		{
			return m_isStartNew;
		}
		set
		{
			m_isStartNew = value;
		}
	}

	internal void Read(XmlNode node)
	{
		if (node.Attributes["target"] != null)
		{
			BeforeTargetID = node.Attributes["target"].Value;
		}
		if (node.Attributes["targetType"] != null)
		{
			string value = node.Attributes["targetType"].Value;
			if (!(value == "pageArea"))
			{
				if (value == "contentArea")
				{
					TargetType = PdfXfaTargetType.ContentArea;
				}
			}
			else
			{
				TargetType = PdfXfaTargetType.PageArea;
			}
		}
		if (node.Attributes["startNew"] != null)
		{
			string value2 = node.Attributes["startNew"].Value;
			if (value2 != null && value2 != string.Empty && int.Parse(value2) == 1)
			{
				IsStartNew = true;
			}
		}
	}
}
