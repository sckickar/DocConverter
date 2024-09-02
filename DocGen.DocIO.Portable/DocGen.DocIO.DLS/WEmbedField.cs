using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

internal class WEmbedField : WField
{
	protected internal int m_storagePicLocation;

	private byte m_bFlags;

	public override EntityType EntityType => EntityType.EmbededField;

	internal int StoragePicLocation
	{
		get
		{
			return m_storagePicLocation;
		}
		set
		{
			m_storagePicLocation = value;
		}
	}

	internal bool IsOle2
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

	internal WEmbedField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.EmbedField;
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("StoragePicLocation"))
		{
			m_storagePicLocation = reader.ReadInt("StoragePicLocation");
			IsOle2 = reader.ReadBoolean("Ole2Object");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (m_storagePicLocation > 0)
		{
			writer.WriteValue("StoragePicLocation", m_storagePicLocation);
			writer.WriteValue("Ole2Object", IsOle2);
		}
	}

	protected override object CloneImpl()
	{
		return (WEmbedField)base.CloneImpl();
	}
}
