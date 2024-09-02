using System;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WCheckBox : WFormField, ILeafWidget, IWidget
{
	private int m_checkBoxSize;

	private byte m_bFlags;

	private CheckBoxSizeType m_sizeType;

	public override EntityType EntityType => EntityType.CheckBox;

	public int CheckBoxSize
	{
		get
		{
			return m_checkBoxSize;
		}
		set
		{
			if (value < 1 || value > 1584)
			{
				throw new ArgumentOutOfRangeException("The measurement must be between 1pt to 1584pt");
			}
			m_checkBoxSize = value;
		}
	}

	public bool DefaultCheckBoxValue
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public bool Checked
	{
		get
		{
			return base.Value switch
			{
				0 => false, 
				1 => true, 
				25 => DefaultCheckBoxValue, 
				_ => throw new ArgumentException("Unsupported checkbox field value found."), 
			};
		}
		set
		{
			base.Value = (value ? 1 : 0);
		}
	}

	public CheckBoxSizeType SizeType
	{
		get
		{
			if ((base.Params & 0x400) != 1024)
			{
				return CheckBoxSizeType.Auto;
			}
			return CheckBoxSizeType.Exactly;
		}
		set
		{
			m_sizeType = value;
			if (value == CheckBoxSizeType.Exactly)
			{
				base.Params = (short)BaseWordRecord.SetBitsByMask(base.Params, 1024, 10, 1);
			}
			else
			{
				base.Params = (short)BaseWordRecord.SetBitsByMask(base.Params, 1024, 10, 0);
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

	public WCheckBox(IWordDocument doc)
		: base(doc)
	{
		m_curFormFieldType = FormFieldType.CheckBox;
		m_paraItemType = ParagraphItemType.CheckBox;
		base.FieldType = FieldType.FieldFormCheckBox;
		base.Params = 229;
		m_checkBoxSize = 20;
	}

	protected override object CloneImpl()
	{
		return (WCheckBox)base.CloneImpl();
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("CheckBoxSize"))
		{
			m_checkBoxSize = reader.ReadShort("CheckBoxSize");
		}
		if (reader.HasAttribute("DefaultCheckBoxValue"))
		{
			DefaultCheckBoxValue = reader.ReadBoolean("DefaultCheckBoxValue");
		}
		if (reader.HasAttribute("CheckBoxSizeType"))
		{
			SizeType = (CheckBoxSizeType)(object)reader.ReadEnum("CheckBoxSizeType", typeof(CheckBoxSizeType));
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("CheckBoxSize", m_checkBoxSize);
		writer.WriteValue("CheckBoxSizeType", SizeType);
		writer.WriteValue("DefaultCheckBoxValue", DefaultCheckBoxValue);
	}

	internal void SetCheckBoxSizeValue(int checkBoxSize)
	{
		m_checkBoxSize = checkBoxSize;
	}

	protected override void CreateLayoutInfo()
	{
		base.CreateLayoutInfo();
		DocGen.Drawing.Font font = base.Document.FontSettings.GetFont(base.CharacterFormat.GetFontToRender(base.ScriptType).Name, base.CharacterFormat.GetFontToRender(base.ScriptType).Size, FontStyle.Regular, FontScriptType.English);
		if (m_sizeType != 0)
		{
			font = base.Document.FontSettings.GetFont(base.CharacterFormat.GetFontToRender(base.ScriptType).Name, m_checkBoxSize, FontStyle.Regular, FontScriptType.English);
		}
		m_layoutInfo.Font = new SyncFont(font);
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
		float checkBoxSize = GetCheckBoxSize(dc);
		return new SizeF(checkBoxSize, checkBoxSize);
	}

	internal float GetCheckBoxSize(DrawingContext dc)
	{
		WCharacterFormat characterFormat = base.CharacterFormat;
		float fontSize = ((SizeType != 0) ? ((float)m_checkBoxSize) : characterFormat.FontSize);
		if (m_layoutInfo == null)
		{
			CreateLayoutInfo();
		}
		DocGen.Drawing.Font font = m_layoutInfo.Font.GetFont(base.Document, FontScriptType.English);
		font = base.Document.FontSettings.GetFont(font.Name, fontSize, font.Style, FontScriptType.English);
		DocGen.Drawing.Font font2 = ((characterFormat != null && characterFormat.SubSuperScript != 0) ? base.Document.FontSettings.GetFont(font.Name, dc.GetSubSuperScriptFontSize(font), font.Style, FontScriptType.English) : font);
		return dc.MeasureString("0x25A1", font2, new StringFormat(dc.StringFormt), FontScriptType.English).Height;
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
		stringBuilder.Append(CheckBoxSize + ";");
		stringBuilder.Append(Checked ? "1" : "0;");
		stringBuilder.Append(base.Enabled ? "1" : "0;");
		stringBuilder.Append('\u0015');
		return stringBuilder;
	}
}
