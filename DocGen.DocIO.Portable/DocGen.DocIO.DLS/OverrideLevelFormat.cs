using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class OverrideLevelFormat : XDLSSerializableBase
{
	private int m_startAt;

	private byte m_bFlags;

	private WListLevel m_lfoLevel;

	internal int m_reserved1;

	internal int m_reserved2;

	internal int m_reserved3;

	internal bool OverrideStartAtValue
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

	internal bool OverrideFormatting
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal int StartAt
	{
		get
		{
			return m_startAt;
		}
		set
		{
			m_startAt = value;
		}
	}

	internal WListLevel OverrideListLevel
	{
		get
		{
			return m_lfoLevel;
		}
		set
		{
			m_lfoLevel = value;
		}
	}

	internal OverrideLevelFormat(WordDocument doc)
		: base(doc, null)
	{
		m_lfoLevel = new WListLevel(base.Document);
		m_lfoLevel.SetOwner(this);
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("level-override", m_lfoLevel);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (OverrideStartAtValue)
		{
			writer.WriteValue("ChangeStartAt", OverrideStartAtValue);
			writer.WriteValue("StartAt", m_startAt);
		}
		if (OverrideFormatting)
		{
			writer.WriteValue("ChangeFormat", OverrideFormatting);
		}
		if (m_reserved1 != 0)
		{
			writer.WriteValue("Reserved1", m_reserved1);
		}
		if (m_reserved2 != 0)
		{
			writer.WriteValue("Reserved2", m_reserved2);
		}
		if (m_reserved3 != 0)
		{
			writer.WriteValue("Reserved3", m_reserved3);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("ChangeFormat"))
		{
			OverrideFormatting = reader.ReadBoolean("ChangeFormat");
		}
		if (reader.HasAttribute("ChangeStartAt"))
		{
			OverrideStartAtValue = reader.ReadBoolean("ChangeStartAt");
		}
		if (reader.HasAttribute("StartAt"))
		{
			m_startAt = reader.ReadInt("StartAt");
		}
		if (reader.HasAttribute("Reserved1"))
		{
			m_reserved1 = reader.ReadInt("Reserved1");
		}
		if (reader.HasAttribute("Reserved2"))
		{
			m_reserved2 = reader.ReadInt("Reserved2");
		}
		if (reader.HasAttribute("Reserved3"))
		{
			m_reserved3 = reader.ReadInt("Reserved3");
		}
	}

	protected override object CloneImpl()
	{
		OverrideLevelFormat overrideLevelFormat = (OverrideLevelFormat)base.CloneImpl();
		overrideLevelFormat.OverrideListLevel = OverrideListLevel.Clone();
		overrideLevelFormat.OverrideListLevel.SetOwner(overrideLevelFormat);
		return overrideLevelFormat;
	}

	internal new void Close()
	{
		if (m_lfoLevel != null)
		{
			m_lfoLevel.Close();
			m_lfoLevel = null;
		}
		base.Close();
	}

	internal bool Compare(OverrideLevelFormat overrideLevelFormat)
	{
		if (OverrideFormatting != overrideLevelFormat.OverrideFormatting)
		{
			return false;
		}
		if (OverrideStartAtValue != overrideLevelFormat.OverrideStartAtValue)
		{
			return false;
		}
		if (StartAt != overrideLevelFormat.StartAt)
		{
			return false;
		}
		if (OverrideListLevel != null && overrideLevelFormat.OverrideListLevel != null)
		{
			if (OverrideListLevel.CharacterFormat != null && OverrideListLevel.CharacterFormat != null)
			{
				if (!OverrideListLevel.CharacterFormat.Compare(overrideLevelFormat.OverrideListLevel.CharacterFormat))
				{
					return false;
				}
			}
			else if ((OverrideListLevel.CharacterFormat == null && OverrideListLevel.CharacterFormat != null) || (OverrideListLevel.CharacterFormat != null && OverrideListLevel.CharacterFormat == null))
			{
				return false;
			}
			if (OverrideListLevel.ParagraphFormat != null && OverrideListLevel.ParagraphFormat != null)
			{
				if (!OverrideListLevel.ParagraphFormat.Compare(overrideLevelFormat.OverrideListLevel.ParagraphFormat))
				{
					return false;
				}
			}
			else if ((OverrideListLevel.ParagraphFormat == null && OverrideListLevel.ParagraphFormat != null) || (OverrideListLevel.ParagraphFormat != null && OverrideListLevel.ParagraphFormat == null))
			{
				return false;
			}
		}
		else if ((OverrideListLevel == null && overrideLevelFormat.OverrideListLevel != null) || (OverrideListLevel != null && overrideLevelFormat.OverrideListLevel == null))
		{
			return false;
		}
		return true;
	}
}
