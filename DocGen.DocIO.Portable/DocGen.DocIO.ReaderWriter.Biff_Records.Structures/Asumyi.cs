using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class Asumyi
{
	private byte m_flagsA;

	private ushort m_wDlgLevel = 25;

	private uint m_lHighestLevel;

	private uint m_lCurrentLevel;

	internal bool Valid
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool View
	{
		get
		{
			return (m_flagsA & 2) >> 1 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal byte ViewBy
	{
		get
		{
			return (byte)((m_flagsA & 0xC) >> 2);
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xF3u) | (uint)(value << 2));
		}
	}

	internal bool UpdateProps
	{
		get
		{
			return (m_flagsA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal ushort WDlgLevel
	{
		get
		{
			return m_wDlgLevel;
		}
		set
		{
			m_wDlgLevel = value;
		}
	}

	internal uint LHighestLevel
	{
		get
		{
			return m_lHighestLevel;
		}
		set
		{
			m_lHighestLevel = value;
		}
	}

	internal uint LCurrentLevel
	{
		get
		{
			return m_lCurrentLevel;
		}
		set
		{
			m_lCurrentLevel = value;
		}
	}

	internal Asumyi()
	{
	}

	internal void Parse(Stream stream)
	{
		m_flagsA = (byte)stream.ReadByte();
		stream.ReadByte();
		m_wDlgLevel = BaseWordRecord.ReadUInt16(stream);
		m_lHighestLevel = BaseWordRecord.ReadUInt32(stream);
		m_lCurrentLevel = BaseWordRecord.ReadUInt32(stream);
	}

	internal void Write(Stream stream)
	{
		stream.WriteByte(m_flagsA);
		stream.WriteByte(0);
		BaseWordRecord.WriteUInt16(stream, m_wDlgLevel);
		BaseWordRecord.WriteUInt32(stream, m_lHighestLevel);
		BaseWordRecord.WriteUInt32(stream, m_lCurrentLevel);
	}
}
