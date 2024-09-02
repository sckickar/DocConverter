using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WDropDownItem : XDLSSerializableBase
{
	private string m_text = string.Empty;

	public string Text
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

	public WDropDownItem(IWordDocument doc)
		: base((WordDocument)doc, null)
	{
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("itemText", m_text);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("itemText"))
		{
			m_text = reader.ReadString("itemText");
		}
	}

	internal WDropDownItem Clone()
	{
		return (WDropDownItem)CloneImpl();
	}
}
