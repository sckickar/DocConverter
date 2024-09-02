using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

[CLSCompliant(false)]
internal class Dogrid
{
	private ushort m_xaGrid = 1701;

	private ushort m_yaGrid = 1984;

	private ushort m_dxaGrid = 180;

	private ushort m_dyaGrid = 180;

	private byte m_flagsA = 1;

	private byte m_flagsB = 129;

	internal ushort XaGrid
	{
		get
		{
			return m_xaGrid;
		}
		set
		{
			m_xaGrid = value;
		}
	}

	internal ushort YaGrid
	{
		get
		{
			return m_yaGrid;
		}
		set
		{
			m_yaGrid = value;
		}
	}

	internal ushort DxaGrid
	{
		get
		{
			return m_dxaGrid;
		}
		set
		{
			m_dxaGrid = value;
		}
	}

	internal ushort DyaGrid
	{
		get
		{
			return m_dyaGrid;
		}
		set
		{
			m_dyaGrid = value;
		}
	}

	internal byte DyGridDisplay
	{
		get
		{
			return (byte)(m_flagsA & 0x7Fu);
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0x80u) | value);
		}
	}

	internal byte DxGridDisplay
	{
		get
		{
			return (byte)(m_flagsB & 0x7Fu);
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0x80u) | value);
		}
	}

	internal bool FollowMargins
	{
		get
		{
			return (m_flagsB & 0x80) >> 7 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal Dogrid()
	{
	}

	internal void Parse(Stream stream)
	{
		m_xaGrid = BaseWordRecord.ReadUInt16(stream);
		m_yaGrid = BaseWordRecord.ReadUInt16(stream);
		m_dxaGrid = BaseWordRecord.ReadUInt16(stream);
		m_dyaGrid = BaseWordRecord.ReadUInt16(stream);
		m_flagsA = (byte)stream.ReadByte();
		m_flagsB = (byte)stream.ReadByte();
	}

	internal void Write(Stream stream)
	{
		BaseWordRecord.WriteUInt16(stream, m_xaGrid);
		BaseWordRecord.WriteUInt16(stream, m_yaGrid);
		BaseWordRecord.WriteUInt16(stream, m_dxaGrid);
		BaseWordRecord.WriteUInt16(stream, m_dyaGrid);
		stream.WriteByte(m_flagsA);
		stream.WriteByte(m_flagsB);
	}
}
