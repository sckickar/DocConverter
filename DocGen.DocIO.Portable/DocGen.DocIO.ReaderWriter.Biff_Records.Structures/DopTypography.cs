using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class DopTypography
{
	private ushort m_flagsA;

	private ushort m_cchFollowingPunct;

	private ushort m_cchLeadingPunct;

	private byte[] m_rgxchFPunct = new byte[202];

	private byte[] m_rgxchLPunct = new byte[102];

	internal bool KerningPunct
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal byte Justification
	{
		get
		{
			return (byte)((m_flagsA & 6) >> 1);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFF9u) | (uint)(value << 1));
		}
	}

	internal byte LevelOfKinsoku
	{
		get
		{
			return (byte)((m_flagsA & 0x18) >> 3);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFE7u) | (uint)(value << 3));
		}
	}

	internal bool Print2on1
	{
		get
		{
			return (m_flagsA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal byte CustomKsu
	{
		get
		{
			return (byte)((m_flagsA & 0x380) >> 7);
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFC7Fu) | (uint)(value << 7));
		}
	}

	internal bool JapaneseUseLevel2
	{
		get
		{
			return (m_flagsA & 0x400) >> 10 != 0;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal ushort CchFollowingPunct
	{
		get
		{
			return m_cchFollowingPunct;
		}
		set
		{
			m_cchFollowingPunct = value;
		}
	}

	internal ushort CchLeadingPunct
	{
		get
		{
			return m_cchLeadingPunct;
		}
		set
		{
			m_cchLeadingPunct = value;
		}
	}

	internal byte[] RgxchFPunct
	{
		get
		{
			return m_rgxchFPunct;
		}
		set
		{
			m_rgxchFPunct = value;
		}
	}

	internal byte[] RgxchLPunct
	{
		get
		{
			return m_rgxchLPunct;
		}
		set
		{
			m_rgxchLPunct = value;
		}
	}

	internal DopTypography()
	{
	}

	internal void Parse(Stream stream)
	{
		m_flagsA = BaseWordRecord.ReadUInt16(stream);
		m_cchFollowingPunct = BaseWordRecord.ReadUInt16(stream);
		m_cchLeadingPunct = BaseWordRecord.ReadUInt16(stream);
		stream.Read(m_rgxchFPunct, 0, m_rgxchFPunct.Length);
		stream.Read(m_rgxchLPunct, 0, m_rgxchLPunct.Length);
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_flagsA);
		BaseWordRecord.WriteUInt16(stream, m_cchFollowingPunct);
		BaseWordRecord.WriteUInt16(stream, m_cchLeadingPunct);
		stream.Write(m_rgxchFPunct, 0, m_rgxchFPunct.Length);
		stream.Write(m_rgxchLPunct, 0, m_rgxchLPunct.Length);
	}
}
