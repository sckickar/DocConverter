using DocGen.DocIO.DLS.XML;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WFieldMark : ParagraphItem
{
	private FieldMarkType m_fldMarkType;

	private WField m_parentField;

	public override EntityType EntityType => EntityType.FieldMark;

	public WCharacterFormat CharacterFormat => m_charFormat;

	public FieldMarkType Type
	{
		get
		{
			return m_fldMarkType;
		}
		set
		{
			m_fldMarkType = value;
		}
	}

	internal WField ParentField
	{
		get
		{
			return m_parentField;
		}
		set
		{
			m_parentField = value;
		}
	}

	internal WFieldMark(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(doc, this);
	}

	protected internal WFieldMark(WFieldMark fieldMark, IWordDocument doc)
		: this(doc)
	{
		Type = fieldMark.Type;
		m_charFormat.ImportContainer(fieldMark.CharacterFormat);
		m_charFormat.CopyProperties(fieldMark.CharacterFormat);
	}

	internal WFieldMark(IWordDocument doc, FieldMarkType type)
		: this(doc)
	{
		m_fldMarkType = type;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		if (reader.HasAttribute("FieldMarkType"))
		{
			m_fldMarkType = (FieldMarkType)(object)reader.ReadEnum("FieldMarkType", typeof(FieldMarkType));
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.FieldMark);
		writer.WriteValue("FieldMarkType", m_fldMarkType);
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("character-format", m_charFormat);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}
}
