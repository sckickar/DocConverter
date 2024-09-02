using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Style)]
[CLSCompliant(false)]
internal class StyleRecord : BiffRecordRaw, INamedObject
{
	private const ushort DEF_XF_INDEX_BIT_MASK = 4095;

	[BiffRecordPos(0, 2)]
	private ushort m_usExtFormatIndex;

	[BiffRecordPos(1, 7, TFieldType.Bit)]
	private bool m_bIsBuildIn = true;

	[BiffRecordPos(2, 1)]
	private byte m_BuildInOrNameLen;

	private byte m_OutlineStyleLevel = byte.MaxValue;

	private string m_strName;

	private string m_strNameCache;

	private ushort m_DefXFIndex;

	private bool m_isBuiltInCustomized;

	private bool m_isAsciiConverted;

	public bool IsBuildInStyle
	{
		get
		{
			return m_bIsBuildIn;
		}
		set
		{
			m_bIsBuildIn = value;
		}
	}

	internal bool IsAsciiConverted
	{
		get
		{
			return m_isAsciiConverted;
		}
		set
		{
			m_isAsciiConverted = value;
		}
	}

	public ushort ExtendedFormatIndex
	{
		get
		{
			if (DefXFIndex > 0)
			{
				return (ushort)(m_usExtFormatIndex & DefXFIndex);
			}
			return (ushort)(m_usExtFormatIndex & 0xFFFu);
		}
		set
		{
			m_usExtFormatIndex = value;
		}
	}

	public byte BuildInOrNameLen
	{
		get
		{
			return m_BuildInOrNameLen;
		}
		set
		{
			m_BuildInOrNameLen = value;
		}
	}

	public byte OutlineStyleLevel
	{
		get
		{
			return m_OutlineStyleLevel;
		}
		set
		{
			m_OutlineStyleLevel = value;
		}
	}

	public string StyleName
	{
		get
		{
			return m_strName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("StyleName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("StyleName - string cannot be empty.");
			}
			if (value.Length > 255)
			{
				throw new ArgumentOutOfRangeException("StyleName", "Style name cannot be larger 255 symbols.");
			}
			m_strName = value;
			m_bIsBuildIn = false;
			m_BuildInOrNameLen = checked((byte)m_strName.Length);
		}
	}

	internal string StyleNameCache
	{
		get
		{
			return m_strNameCache;
		}
		set
		{
			m_strNameCache = value;
		}
	}

	public bool IsBuiltIncustomized
	{
		get
		{
			return m_isBuiltInCustomized;
		}
		set
		{
			m_isBuiltInCustomized = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public string Name => m_strName;

	internal ushort DefXFIndex
	{
		get
		{
			return m_DefXFIndex;
		}
		set
		{
			m_DefXFIndex = value;
		}
	}

	public StyleRecord()
	{
	}

	public StyleRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public StyleRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iLength = iLength;
		m_usExtFormatIndex = provider.ReadUInt16(iOffset);
		m_bIsBuildIn = provider.ReadBit(iOffset + 1, 7);
		m_BuildInOrNameLen = provider.ReadByte(iOffset + 2);
		m_usExtFormatIndex &= 4095;
		if (m_bIsBuildIn)
		{
			if (iLength > 4)
			{
				throw new LargeBiffRecordDataException();
			}
			m_OutlineStyleLevel = provider.ReadByte(iOffset + 3);
		}
		else
		{
			provider.ReadByte(iOffset + 4);
			int buildInOrNameLen = m_BuildInOrNameLen;
			m_strName = provider.ReadString(iOffset + 4, buildInOrNameLen, out var _, isByteCounted: false);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (m_usExtFormatIndex > 4095)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usExtFormatIndex);
		provider.WriteBit(iOffset + 1, m_bIsBuildIn, 7);
		provider.WriteByte(iOffset + 2, m_BuildInOrNameLen);
		if (m_bIsBuildIn)
		{
			provider.WriteByte(iOffset + 3, m_OutlineStyleLevel);
			return;
		}
		provider.WriteByte(iOffset + 2, (byte)m_strName.Length);
		provider.WriteByte(iOffset + 3, 0);
		byte[] bytes = Encoding.Unicode.GetBytes(m_strName);
		provider.WriteByte(iOffset + 4, 1);
		provider.WriteBytes(iOffset + 5, bytes, 0, bytes.Length);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_bIsBuildIn)
		{
			return 4;
		}
		int byteCount = Encoding.Unicode.GetByteCount(m_strName);
		return 5 + byteCount;
	}

	public override void CopyTo(BiffRecordRaw raw)
	{
		StyleRecord obj = (raw as StyleRecord) ?? throw new ArgumentNullException("Wrong BiffRecord type");
		obj.m_usExtFormatIndex = m_usExtFormatIndex;
		obj.m_bIsBuildIn = m_bIsBuildIn;
		obj.m_BuildInOrNameLen = m_BuildInOrNameLen;
		obj.m_OutlineStyleLevel = m_OutlineStyleLevel;
		obj.m_strName = m_strName;
	}
}
