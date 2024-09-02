using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ColumnInfo)]
[CLSCompliant(false)]
internal class ColumnInfoRecord : BiffRecordRaw, IOutline, IComparable
{
	private const ushort OutlevelBitMask = 1792;

	private const int DEF_MAX_SIZE = 12;

	[BiffRecordPos(0, 2)]
	private ushort m_usFirstCol;

	[BiffRecordPos(2, 2)]
	private ushort m_usLastCol;

	[BiffRecordPos(4, 2)]
	private ushort m_usColWidth = 2340;

	[BiffRecordPos(6, 2)]
	private ushort m_usExtFormatIndex = 15;

	[BiffRecordPos(8, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(8, 0, TFieldType.Bit)]
	private bool m_bHidden;

	[BiffRecordPos(8, 2, TFieldType.Bit)]
	private bool m_bBestFit;

	[BiffRecordPos(8, 1, TFieldType.Bit)]
	private bool m_bUserSet;

	[BiffRecordPos(8, 3, TFieldType.Bit)]
	private bool m_bPhonetic;

	[BiffRecordPos(9, 4, TFieldType.Bit)]
	private bool m_bCollapsed;

	[BiffRecordPos(10, 2)]
	private ushort m_usReserved = 4;

	public ushort Reserved => m_usReserved;

	public ushort FirstColumn
	{
		get
		{
			return m_usFirstCol;
		}
		set
		{
			m_usFirstCol = value;
		}
	}

	public ushort LastColumn
	{
		get
		{
			return m_usLastCol;
		}
		set
		{
			m_usLastCol = value;
		}
	}

	public ushort ColumnWidth
	{
		get
		{
			return m_usColWidth;
		}
		set
		{
			m_usColWidth = value;
		}
	}

	public ushort ExtendedFormatIndex
	{
		get
		{
			return m_usExtFormatIndex;
		}
		set
		{
			m_usExtFormatIndex = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return m_bHidden;
		}
		set
		{
			m_bHidden = value;
		}
	}

	internal bool IsBestFit
	{
		get
		{
			return m_bBestFit;
		}
		set
		{
			m_bBestFit = value;
		}
	}

	internal bool IsUserSet
	{
		get
		{
			return m_bUserSet;
		}
		set
		{
			m_bUserSet = value;
		}
	}

	internal bool IsPhenotic
	{
		get
		{
			return m_bPhonetic;
		}
		set
		{
			m_bPhonetic = value;
		}
	}

	public ushort OutlineLevel
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 1792) >> 8);
		}
		set
		{
			if (value > 7)
			{
				throw new ArgumentOutOfRangeException();
			}
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 1792, (ushort)(value << 8));
		}
	}

	public bool IsCollapsed
	{
		get
		{
			return m_bCollapsed;
		}
		set
		{
			m_bCollapsed = value;
		}
	}

	public override int MinimumRecordSize => 10;

	public override int MaximumRecordSize => 12;

	ushort IOutline.Index
	{
		get
		{
			return FirstColumn;
		}
		set
		{
			FirstColumn = value;
			LastColumn = value;
		}
	}

	public ColumnInfoRecord()
	{
	}

	public ColumnInfoRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ColumnInfoRecord(int iReserve)
		: base(iReserve)
	{
	}

	~ColumnInfoRecord()
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFirstCol = provider.ReadUInt16(iOffset);
		m_usLastCol = provider.ReadUInt16(iOffset + 2);
		m_usColWidth = provider.ReadUInt16(iOffset + 4);
		m_usExtFormatIndex = provider.ReadUInt16(iOffset + 6);
		m_usOptions = provider.ReadUInt16(iOffset + 8);
		m_bHidden = provider.ReadBit(iOffset + 8, 0);
		m_bUserSet = provider.ReadBit(iOffset + 8, 1);
		m_bBestFit = provider.ReadBit(iOffset + 8, 2);
		m_bPhonetic = provider.ReadBit(iOffset + 8, 3);
		m_bCollapsed = provider.ReadBit(iOffset + 9, 4);
		if (iLength > 12)
		{
			m_usReserved = provider.ReadUInt16(iOffset + 10);
		}
		m_iLength = MinimumRecordSize;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFirstCol);
		provider.WriteUInt16(iOffset + 2, m_usLastCol);
		provider.WriteUInt16(iOffset + 4, m_usColWidth);
		provider.WriteUInt16(iOffset + 6, m_usExtFormatIndex);
		provider.WriteUInt16(iOffset + 8, m_usOptions);
		provider.WriteBit(iOffset + 8, m_bHidden, 0);
		provider.WriteBit(iOffset + 8, m_bUserSet, 1);
		provider.WriteBit(iOffset + 8, m_bBestFit, 2);
		provider.WriteBit(iOffset + 8, m_bPhonetic, 3);
		provider.WriteBit(iOffset + 9, m_bCollapsed, 4);
		provider.WriteUInt16(iOffset + 10, m_usReserved);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}

	public int CompareTo(object obj)
	{
		int result = -1;
		if (obj is ColumnInfoRecord)
		{
			ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)obj;
			if ((result = OutlineLevel.CompareTo(columnInfoRecord.OutlineLevel)) == 0 && (result = m_usExtFormatIndex.CompareTo(columnInfoRecord.m_usExtFormatIndex)) == 0 && (result = m_usColWidth.CompareTo(columnInfoRecord.m_usColWidth)) == 0 && (result = m_bHidden.CompareTo(columnInfoRecord.m_bHidden)) == 0 && (result = m_bCollapsed.CompareTo(columnInfoRecord.m_bCollapsed)) == 0 && (result = m_usReserved.CompareTo(columnInfoRecord.m_usReserved)) == 0)
			{
				return 0;
			}
		}
		return result;
	}

	public void SetDefaultOptions()
	{
		m_usColWidth = 2340;
		m_usOptions = 0;
		m_bHidden = false;
		m_bCollapsed = false;
		m_usReserved = 4;
	}
}
