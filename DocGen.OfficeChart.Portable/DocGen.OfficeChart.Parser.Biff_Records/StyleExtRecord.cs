using System;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class StyleExtRecord : BiffRecordRaw
{
	private const int StartLength = 22;

	private FutureHeader m_header;

	[BiffRecordPos(12, 0, TFieldType.Bit)]
	private bool m_bIsBuildIn = true;

	[BiffRecordPos(12, 1, TFieldType.Bit)]
	private bool m_bIsHidden;

	[BiffRecordPos(12, 2, TFieldType.Bit)]
	private bool m_bIsCustom;

	[BiffRecordPos(12, 8)]
	private long m_reserved;

	private ushort m_category;

	private ushort m_builtInData;

	private string m_strName;

	private byte m_OutlineStyleLevel = byte.MaxValue;

	[BiffRecordPos(2, 1)]
	private ushort m_BuildInOrNameLen;

	private ushort m_xfPropCount;

	private byte[] m_preservedProperties;

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

	public bool IsHidden
	{
		get
		{
			return m_bIsHidden;
		}
		set
		{
			m_bIsHidden = value;
		}
	}

	public bool IsCustom
	{
		get
		{
			return m_bIsCustom;
		}
		set
		{
			m_bIsCustom = value;
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
		}
	}

	public ushort Category
	{
		get
		{
			return m_category;
		}
		set
		{
			m_category = value;
		}
	}

	public StyleExtRecord()
	{
		InitializeObjects();
	}

	private void InitializeObjects()
	{
		m_header = new FutureHeader();
		m_header.Type = 2194;
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_header.Type = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_header.Attributes = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_reserved = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_bIsBuildIn = provider.ReadBit(iOffset, 0);
		m_bIsHidden = provider.ReadBit(iOffset, 1);
		m_bIsCustom = provider.ReadBit(iOffset, 2);
		m_category = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_builtInData = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_BuildInOrNameLen = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_strName = provider.ReadString(iOffset, m_BuildInOrNameLen * 2, Encoding.Unicode, isUnicode: true);
		iOffset += m_BuildInOrNameLen * 2;
		provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_xfPropCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_preservedProperties = new byte[iLength - iOffset];
		provider.ReadArray(iOffset, m_preservedProperties, m_preservedProperties.Length);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_header.Type);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_header.Attributes);
		iOffset += 2;
		provider.WriteInt64(iOffset, 0L);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_category);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_builtInData);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_BuildInOrNameLen);
		iOffset += 2;
		byte[] bytes = Encoding.Unicode.GetBytes(m_strName);
		provider.WriteBytes(iOffset, bytes, 0, bytes.Length);
		iOffset += m_BuildInOrNameLen * 2;
		provider.WriteUInt32(iOffset, 0u);
		iOffset += 2;
		provider.WriteUInt32(iOffset, m_xfPropCount);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_preservedProperties);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 22 + Encoding.Unicode.GetByteCount(m_strName) + m_preservedProperties.Length;
	}
}
