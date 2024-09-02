using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartDataLabels)]
[CLSCompliant(false)]
internal class ChartDataLabelsRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 12;

	[BiffRecordPos(12, 1)]
	private byte m_Options;

	[BiffRecordPos(12, 0, TFieldType.Bit)]
	private bool m_bSeriesName;

	[BiffRecordPos(12, 1, TFieldType.Bit)]
	private bool m_bCategoryName;

	[BiffRecordPos(12, 2, TFieldType.Bit)]
	private bool m_bValue;

	[BiffRecordPos(12, 3, TFieldType.Bit)]
	private bool m_bPercentage;

	[BiffRecordPos(12, 4, TFieldType.Bit)]
	private bool m_bBubbleSize;

	[BiffRecordPos(14, 2)]
	private ushort m_usDelimLen;

	private string m_strDelimiter;

	public byte Options => m_Options;

	public bool IsSeriesName
	{
		get
		{
			return m_bSeriesName;
		}
		set
		{
			m_bSeriesName = value;
		}
	}

	public bool IsCategoryName
	{
		get
		{
			return m_bCategoryName;
		}
		set
		{
			m_bCategoryName = value;
		}
	}

	public bool IsValue
	{
		get
		{
			return m_bValue;
		}
		set
		{
			m_bValue = value;
		}
	}

	public bool IsPercentage
	{
		get
		{
			return m_bPercentage;
		}
		set
		{
			m_bPercentage = value;
		}
	}

	public bool IsBubbleSize
	{
		get
		{
			return m_bBubbleSize;
		}
		set
		{
			m_bBubbleSize = value;
		}
	}

	public int DelimiterLength => m_usDelimLen;

	public string Delimiter
	{
		get
		{
			return m_strDelimiter;
		}
		set
		{
			m_strDelimiter = value;
			m_usDelimLen = (ushort)(value?.Length ?? 0);
		}
	}

	public ChartDataLabelsRecord()
	{
	}

	public ChartDataLabelsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartDataLabelsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_Options = provider.ReadByte(iOffset + 12);
		m_bSeriesName = provider.ReadBit(iOffset + 12, 0);
		m_bCategoryName = provider.ReadBit(iOffset + 12, 1);
		m_bValue = provider.ReadBit(iOffset + 12, 2);
		m_bPercentage = provider.ReadBit(iOffset + 12, 3);
		m_bBubbleSize = provider.ReadBit(iOffset + 12, 4);
		m_usDelimLen = provider.ReadUInt16(iOffset + 14);
		if (m_usDelimLen > 0)
		{
			m_strDelimiter = provider.ReadString(iOffset + 16, m_usDelimLen, out var _, isByteCounted: false);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		int num = iOffset;
		provider.WriteInt16(iOffset, 2155);
		int i = iOffset + 2;
		for (int num2 = iOffset + 12; i < num2; i++)
		{
			provider.WriteByte(i, 0);
		}
		provider.WriteByte(iOffset + 12, m_Options);
		provider.WriteBit(iOffset + 12, m_bSeriesName, 0);
		provider.WriteBit(iOffset + 12, m_bCategoryName, 1);
		provider.WriteBit(iOffset + 12, m_bValue, 2);
		provider.WriteBit(iOffset + 12, m_bPercentage, 3);
		provider.WriteBit(iOffset + 12, m_bBubbleSize, 4);
		provider.WriteByte(iOffset + 13, 0);
		provider.WriteUInt16(iOffset + 14, m_usDelimLen);
		if (m_usDelimLen > 0)
		{
			provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strDelimiter);
		}
		m_iLength = iOffset - num;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 16;
		if (m_usDelimLen > 0)
		{
			num += 1 + Encoding.Unicode.GetByteCount(m_strDelimiter);
		}
		return num;
	}

	public static bool operator ==(ChartDataLabelsRecord record1, ChartDataLabelsRecord record2)
	{
		bool flag = object.Equals(record1, null);
		bool flag2 = object.Equals(record2, null);
		if (flag && flag2)
		{
			return true;
		}
		if (flag || flag2)
		{
			return false;
		}
		if (record1.m_bSeriesName == record2.m_bSeriesName && record1.m_bCategoryName == record2.m_bCategoryName && record1.m_bValue == record2.m_bValue && record1.m_bPercentage == record2.m_bPercentage)
		{
			return record1.m_bBubbleSize == record2.m_bBubbleSize;
		}
		return false;
	}

	public static bool operator !=(ChartDataLabelsRecord record1, ChartDataLabelsRecord record2)
	{
		return !(record1 == record2);
	}
}
