using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class FBSE : BaseWordRecord
{
	private MSOBlipType m_btWin32;

	private MSOBlipType m_btMacOS;

	private byte[] m_rgbUid;

	private ushort m_tag;

	private uint m_size;

	private uint m_cRef;

	private uint m_foDelay;

	private MSOBlipUsage m_usage;

	private byte m_cbName;

	private byte m_unused2;

	private byte m_unused3;

	public MSOBlipType Win32
	{
		get
		{
			return m_btWin32;
		}
		set
		{
			m_btWin32 = value;
		}
	}

	public MSOBlipType MacOS
	{
		get
		{
			return m_btMacOS;
		}
		set
		{
			m_btMacOS = value;
		}
	}

	public byte[] Uid
	{
		get
		{
			return m_rgbUid;
		}
		set
		{
			m_rgbUid = value;
		}
	}

	public ushort Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public uint Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	public uint Ref
	{
		get
		{
			return m_cRef;
		}
		set
		{
			m_cRef = value;
		}
	}

	public uint Delay
	{
		get
		{
			return m_foDelay;
		}
		set
		{
			m_foDelay = value;
		}
	}

	public MSOBlipUsage Usage
	{
		get
		{
			return m_usage;
		}
		set
		{
			m_usage = value;
		}
	}

	public byte Name
	{
		get
		{
			return m_cbName;
		}
		set
		{
			m_cbName = value;
		}
	}

	public byte Unused2
	{
		set
		{
			m_unused2 = value;
		}
	}

	public byte Unused3
	{
		set
		{
			m_unused3 = value;
		}
	}

	public FBSE()
	{
		m_rgbUid = new byte[16];
	}

	public void Read(Stream stream)
	{
		m_btWin32 = (MSOBlipType)stream.ReadByte();
		m_btMacOS = (MSOBlipType)stream.ReadByte();
		m_rgbUid = ReadBytes(stream, 16);
		m_tag = BaseWordRecord.ReadUInt16(stream);
		m_size = BaseWordRecord.ReadUInt32(stream);
		m_cRef = BaseWordRecord.ReadUInt32(stream);
		m_foDelay = BaseWordRecord.ReadUInt32(stream);
		m_usage = (MSOBlipUsage)stream.ReadByte();
		m_cbName = (byte)stream.ReadByte();
		m_unused2 = (byte)stream.ReadByte();
		m_unused3 = (byte)stream.ReadByte();
	}

	public void Write(Stream stream)
	{
		stream.WriteByte((byte)m_btWin32);
		stream.WriteByte((byte)m_btMacOS);
		for (int i = 0; i < 16; i++)
		{
			stream.WriteByte(m_rgbUid[i]);
		}
		BaseWordRecord.WriteUInt16(stream, m_tag);
		BaseWordRecord.WriteUInt32(stream, m_size);
		BaseWordRecord.WriteUInt32(stream, m_cRef);
		BaseWordRecord.WriteUInt32(stream, m_foDelay);
		stream.WriteByte((byte)m_usage);
		stream.WriteByte(m_cbName);
		stream.WriteByte(m_unused2);
		stream.WriteByte(m_unused3);
	}
}
