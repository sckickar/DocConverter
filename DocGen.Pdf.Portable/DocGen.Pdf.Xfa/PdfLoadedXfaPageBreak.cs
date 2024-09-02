using System.Xml;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaPageBreak
{
	private BreakBefore m_beforeBreak;

	private BreakAfter m_afterBreak;

	private OverFlow m_overflow;

	private string m_beforeTargetID = string.Empty;

	private string m_afterTargetID = string.Empty;

	private bool m_isStartNew;

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

	internal OverFlow Overflow
	{
		get
		{
			return m_overflow;
		}
		set
		{
			m_overflow = value;
		}
	}

	internal BreakBefore BeforeBreak
	{
		get
		{
			return m_beforeBreak;
		}
		set
		{
			m_beforeBreak = value;
		}
	}

	internal BreakAfter AfterBreak
	{
		get
		{
			return m_afterBreak;
		}
		set
		{
			m_afterBreak = value;
		}
	}

	internal void Read(XmlNode node)
	{
		if (node.Name == "break")
		{
			if (node.Attributes["beforeTarget"] != null)
			{
				BeforeTargetID = node.Attributes["beforeTarget"].Value.Replace("#", "");
			}
			else if (node.Attributes["afterTarget"] != null)
			{
				AfterTargetID = node.Attributes["afterTarget"].Value.Replace("#", "");
			}
			if (node.Attributes["startNew"] != null)
			{
				string value = node.Attributes["startNew"].Value;
				if (value != null && value != string.Empty && int.Parse(value) == 1)
				{
					IsStartNew = true;
				}
			}
		}
		else if (node.Name == "overflow")
		{
			Overflow = new OverFlow();
			Overflow.Read(node);
		}
		else if (node.Name == "breakBefore")
		{
			BeforeBreak = new BreakBefore();
			BeforeBreak.Read(node);
		}
		else if (node.Name == "breakAfter")
		{
			AfterBreak = new BreakAfter();
			AfterBreak.Read(node);
		}
	}
}
