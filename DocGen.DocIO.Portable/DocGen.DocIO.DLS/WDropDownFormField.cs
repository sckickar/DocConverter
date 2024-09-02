using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WDropDownFormField : WFormField, ILeafWidget, IWidget
{
	private short m_defaultDropDownValue;

	private WDropDownCollection m_dropDownItems;

	public override EntityType EntityType => EntityType.DropDownFormField;

	public int DropDownSelectedIndex
	{
		get
		{
			if (base.Value != 25)
			{
				return base.Value;
			}
			return m_defaultDropDownValue;
		}
		set
		{
			base.Value = value;
		}
	}

	public WDropDownCollection DropDownItems => m_dropDownItems;

	internal int DefaultDropDownValue
	{
		get
		{
			return m_defaultDropDownValue;
		}
		set
		{
			m_defaultDropDownValue = (short)value;
		}
	}

	internal string DropDownValue
	{
		get
		{
			if (m_dropDownItems.Count > 0)
			{
				int num = ((DropDownSelectedIndex > 0) ? DropDownSelectedIndex : DefaultDropDownValue);
				if (num < 0 || num > m_dropDownItems.Count)
				{
					return m_dropDownItems[0].Text;
				}
				if (num < m_dropDownItems.Count)
				{
					return m_dropDownItems[DropDownSelectedIndex].Text;
				}
			}
			return "\u2002\u2002\u2002\u2002\u2002";
		}
		set
		{
			for (int i = 0; i < m_dropDownItems.Count; i++)
			{
				if (string.Compare(m_dropDownItems[i].Text, value) == 0)
				{
					DropDownSelectedIndex = i;
					break;
				}
			}
		}
	}

	ILayoutInfo IWidget.LayoutInfo
	{
		get
		{
			if (m_layoutInfo == null)
			{
				CreateLayoutInfo();
			}
			return m_layoutInfo;
		}
	}

	public WDropDownFormField(IWordDocument doc)
		: base(doc)
	{
		m_curFormFieldType = FormFieldType.DropDown;
		m_paraItemType = ParagraphItemType.DropDownFormField;
		base.FieldType = FieldType.FieldFormDropDown;
		base.Params = 32998;
		m_dropDownItems = new WDropDownCollection(base.Document);
	}

	protected override object CloneImpl()
	{
		WDropDownFormField wDropDownFormField = (WDropDownFormField)base.CloneImpl();
		wDropDownFormField.m_dropDownItems = new WDropDownCollection(base.Document);
		m_dropDownItems.CloneTo(wDropDownFormField.m_dropDownItems);
		return wDropDownFormField;
	}

	internal override void Close()
	{
		if (m_dropDownItems != null)
		{
			m_dropDownItems.Close();
			m_dropDownItems = null;
		}
		base.Close();
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("DefaultDrowDownValue"))
		{
			m_defaultDropDownValue = reader.ReadShort("DefaultDrowDownValue");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("DefaultDrowDownValue", m_defaultDropDownValue);
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("dropdown-items", m_dropDownItems);
	}

	protected override void CreateLayoutInfo()
	{
		base.CreateLayoutInfo();
		m_layoutInfo.Font = new SyncFont(DocumentLayouter.DrawingContext.GetFont(base.ScriptType, base.CharacterFormat, DropDownValue));
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return dc.MeasureString(DropDownValue, base.CharacterFormat.GetFontToRender(base.ScriptType), null, base.CharacterFormat, isMeasureFromTabList: false, base.ScriptType);
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal override StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0015');
		stringBuilder.Append(base.Enabled ? "1" : "0;");
		foreach (WDropDownItem dropDownItem in DropDownItems)
		{
			stringBuilder.Append(dropDownItem.Text + ";");
		}
		stringBuilder.Append('\u0015');
		return stringBuilder;
	}
}
