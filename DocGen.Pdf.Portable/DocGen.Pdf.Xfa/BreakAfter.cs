using System.Xml;

namespace DocGen.Pdf.Xfa;

internal class BreakAfter
{
	private string m_afterTargetID = string.Empty;

	private bool m_isStartNew;

	private PdfXfaTargetType m_targetType = PdfXfaTargetType.PageArea;

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

	internal string AfterTargetID
	{
		get
		{
			return m_afterTargetID;
		}
		set
		{
			m_afterTargetID = value;
		}
	}

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

	internal void Read(XmlNode node)
	{
		if (node.Attributes["target"] != null)
		{
			AfterTargetID = node.Attributes["target"].Value;
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
