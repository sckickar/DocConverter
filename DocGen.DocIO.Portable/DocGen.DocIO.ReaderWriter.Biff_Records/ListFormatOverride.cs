using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListFormatOverride : BaseWordRecord
{
	private int m_lsid;

	private int m_unused1;

	private int m_unused2;

	internal int m_res1;

	internal int m_res2;

	private List<object> m_levels;

	private int m_clfolvl;

	internal List<object> Levels => m_levels;

	internal int ListID
	{
		get
		{
			return m_lsid;
		}
		set
		{
			m_lsid = value;
		}
	}

	internal ListFormatOverride()
	{
		m_levels = new ListLevels();
	}

	internal ListFormatOverride(Stream stream)
	{
		m_levels = new ListLevels();
		ReadLfo(stream);
	}

	internal override void Close()
	{
		base.Close();
		if (m_levels == null)
		{
			return;
		}
		foreach (object level in m_levels)
		{
			if (level is ListFormatOverrideLevel)
			{
				(level as ListFormatOverrideLevel).Close();
			}
		}
		m_levels.Clear();
		m_levels = null;
	}

	internal void WriteLfo(Stream stream)
	{
		BaseWordRecord.WriteInt32(stream, m_lsid);
		BaseWordRecord.WriteInt32(stream, m_unused1);
		BaseWordRecord.WriteInt32(stream, m_unused2);
		stream.WriteByte((byte)m_levels.Count);
		stream.WriteByte((byte)m_res1);
		BaseWordRecord.WriteInt16(stream, (short)m_res2);
	}

	internal void ReadLfo(Stream stream)
	{
		_ = stream.Position;
		m_lsid = BaseWordRecord.ReadInt32(stream);
		m_unused1 = BaseWordRecord.ReadInt32(stream);
		m_unused2 = BaseWordRecord.ReadInt32(stream);
		m_clfolvl = stream.ReadByte();
		m_res1 = stream.ReadByte();
		m_res2 = BaseWordRecord.ReadInt16(stream);
	}

	internal void WriteLfoLvls(Stream stream)
	{
		BaseWordRecord.WriteUInt32(stream, uint.MaxValue);
		int i = 0;
		for (int count = m_levels.Count; i < count; i++)
		{
			((ListFormatOverrideLevel)m_levels[i]).Write(stream);
		}
	}

	internal void ReadLfoLvls(Stream stream)
	{
		BaseWordRecord.ReadUInt32(stream);
		for (int i = 0; i < m_clfolvl; i++)
		{
			m_levels.Add(new ListFormatOverrideLevel(stream));
		}
	}
}
