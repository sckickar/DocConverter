using DocGen.Drawing;

namespace DocGen.Pdf.Xfa;

public class PdfXfaRadioButtonGroup : PdfXfaField
{
	internal PdfXfaRadioButtonListItem m_radioList = new PdfXfaRadioButtonListItem();

	internal int selectedItem;

	private PdfXfaFlowDirection m_layout;

	internal PdfXfaForm parent;

	internal SizeF Size = SizeF.Empty;

	private bool m_readOnly;

	public bool ReadOnly
	{
		get
		{
			return m_readOnly;
		}
		set
		{
			m_readOnly = value;
		}
	}

	public PdfXfaRadioButtonListItem Items
	{
		get
		{
			return m_radioList;
		}
		set
		{
			if (value != null)
			{
				m_radioList = value;
			}
		}
	}

	public PdfXfaFlowDirection FlowDirection
	{
		get
		{
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public PdfXfaRadioButtonGroup(string name)
	{
		base.Name = name;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		xfaWriter.Write.WriteStartElement("exclGroup");
		string value = "group1";
		if (base.Name != null && base.Name != "")
		{
			value = base.Name;
		}
		xfaWriter.Write.WriteAttributeString("name", value);
		if (FlowDirection == PdfXfaFlowDirection.Horizontal)
		{
			xfaWriter.Write.WriteAttributeString("layout", "lr-tb");
			xfaWriter.Write.WriteAttributeString("w", parent.Width + "pt");
		}
		else
		{
			xfaWriter.Write.WriteAttributeString("layout", "tb");
		}
		int num = 1;
		foreach (PdfXfaRadioButtonField radio in m_radioList)
		{
			if (radio.IsChecked)
			{
				selectedItem = num;
			}
			radio.Save(xfaWriter, num++);
		}
		xfaWriter.WriteMargins(base.Margins);
		xfaWriter.Write.WriteEndElement();
	}

	public object Clone()
	{
		return (PdfXfaRadioButtonGroup)MemberwiseClone();
	}
}
