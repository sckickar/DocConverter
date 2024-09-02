using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListFormatOverrideLevel : BaseWordRecord
{
	internal int m_startAt;

	internal int m_ilvl;

	internal bool m_bStartAt;

	internal bool m_bFormatting;

	internal int m_reserved1;

	internal int m_reserved2;

	internal int m_reserved3;

	internal ListLevel m_lvl;

	internal ListFormatOverrideLevel(bool overrideLvl)
	{
		if (overrideLvl)
		{
			m_lvl = new ListLevel();
		}
	}

	internal ListFormatOverrideLevel(Stream stream)
	{
		int startAt = (int)BaseWordRecord.ReadUInt32(stream);
		int num = stream.ReadByte();
		m_ilvl = num & 0xF;
		m_bStartAt = (num & 0x10) != 0;
		m_bFormatting = (num & 0x20) != 0;
		m_reserved1 = stream.ReadByte();
		m_reserved2 = stream.ReadByte();
		m_reserved3 = stream.ReadByte();
		if (m_bFormatting)
		{
			m_lvl = new ListLevel(stream);
		}
		else if (m_bStartAt)
		{
			m_startAt = startAt;
		}
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt32(stream, (uint)m_startAt);
		int num = 0;
		num |= m_ilvl;
		num |= (m_bStartAt ? 16 : 0);
		num |= (m_bFormatting ? 32 : 0);
		stream.WriteByte((byte)num);
		stream.WriteByte((byte)m_reserved1);
		stream.WriteByte((byte)m_reserved2);
		stream.WriteByte((byte)m_reserved3);
		if (m_bFormatting)
		{
			m_lvl.Write(stream);
		}
	}
}
