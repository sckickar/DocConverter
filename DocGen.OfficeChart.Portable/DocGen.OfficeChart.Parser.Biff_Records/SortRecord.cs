using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Sort)]
[CLSCompliant(false)]
internal class SortRecord : BiffRecordRaw
{
	private const ushort TableIndexBitMask = 992;

	private const int TableIndexStartBit = 5;

	private const int DEF_FIXED_PART_SIZE = 5;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bSortColumns;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bFirstDesc;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bSecondDesc;

	[BiffRecordPos(0, 3, TFieldType.Bit)]
	private bool m_bThirdDesc;

	[BiffRecordPos(0, 4, TFieldType.Bit)]
	private bool m_bCaseSensitive;

	[BiffRecordPos(2, 1)]
	private byte m_FirstKeyLen;

	[BiffRecordPos(3, 1)]
	private byte m_SecondKeyLen;

	[BiffRecordPos(4, 1)]
	private byte m_ThirdKeyLen;

	private string m_strFirstKey = string.Empty;

	private string m_strSecondKey = string.Empty;

	private string m_strThirdKey = string.Empty;

	public bool IsSortColumns
	{
		get
		{
			return m_bSortColumns;
		}
		set
		{
			m_bSortColumns = value;
		}
	}

	public bool IsFirstDesc
	{
		get
		{
			return m_bFirstDesc;
		}
		set
		{
			m_bFirstDesc = value;
		}
	}

	public bool IsSecondDesc
	{
		get
		{
			return m_bSecondDesc;
		}
		set
		{
			m_bSecondDesc = value;
		}
	}

	public bool IsThirdDesc
	{
		get
		{
			return m_bThirdDesc;
		}
		set
		{
			m_bThirdDesc = value;
		}
	}

	public bool IsCaseSensitive
	{
		get
		{
			return m_bCaseSensitive;
		}
		set
		{
			m_bCaseSensitive = value;
		}
	}

	public ushort TableIndex
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 992) >> 5);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 992, (ushort)(value << 5));
		}
	}

	public byte FirstKeyLen => m_FirstKeyLen;

	public byte SecondKeyLen => m_SecondKeyLen;

	public byte ThirdKeyLen => m_ThirdKeyLen;

	public string FirstKey
	{
		get
		{
			return m_strFirstKey;
		}
		set
		{
			m_strFirstKey = value;
			m_FirstKeyLen = (byte)((value != null) ? ((byte)value.Length) : 0);
		}
	}

	public string SecondKey
	{
		get
		{
			return m_strSecondKey;
		}
		set
		{
			m_strSecondKey = value;
			m_SecondKeyLen = (byte)((value != null) ? ((byte)value.Length) : 0);
		}
	}

	public string ThirdKey
	{
		get
		{
			return m_strThirdKey;
		}
		set
		{
			m_strThirdKey = value;
			m_ThirdKeyLen = (byte)((value != null) ? ((byte)value.Length) : 0);
		}
	}

	public override int MinimumRecordSize => 5;

	public SortRecord()
	{
	}

	public SortRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SortRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bSortColumns = provider.ReadBit(iOffset, 0);
		m_bFirstDesc = provider.ReadBit(iOffset, 1);
		m_bSecondDesc = provider.ReadBit(iOffset, 2);
		m_bThirdDesc = provider.ReadBit(iOffset, 3);
		m_bCaseSensitive = provider.ReadBit(iOffset, 4);
		m_FirstKeyLen = provider.ReadByte(iOffset + 2);
		m_SecondKeyLen = provider.ReadByte(iOffset + 3);
		m_ThirdKeyLen = provider.ReadByte(iOffset + 4);
		iOffset += 5;
		m_strFirstKey = provider.ReadStringUpdateOffset(ref iOffset, m_FirstKeyLen);
		m_strSecondKey = provider.ReadStringUpdateOffset(ref iOffset, m_SecondKeyLen);
		m_strThirdKey = provider.ReadStringUpdateOffset(ref iOffset, m_ThirdKeyLen);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		int num = iOffset;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bSortColumns, 0);
		provider.WriteBit(iOffset, m_bFirstDesc, 1);
		provider.WriteBit(iOffset, m_bSecondDesc, 2);
		provider.WriteBit(iOffset, m_bThirdDesc, 3);
		provider.WriteBit(iOffset, m_bCaseSensitive, 4);
		provider.WriteByte(iOffset + 2, m_FirstKeyLen);
		provider.WriteByte(iOffset + 3, m_SecondKeyLen);
		provider.WriteByte(iOffset + 4, m_ThirdKeyLen);
		iOffset += 5;
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strFirstKey);
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strSecondKey);
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strThirdKey);
		provider.WriteByte(iOffset, 0);
		iOffset++;
		m_iLength = iOffset - num;
	}

	private int GetStringSize(string strValue)
	{
		if (strValue == null || strValue.Length == 0)
		{
			return 0;
		}
		return Encoding.Unicode.GetByteCount(strValue) + 1;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 5 + GetStringSize(m_strFirstKey) + GetStringSize(m_strSecondKey) + GetStringSize(m_strThirdKey) + 1;
	}
}
