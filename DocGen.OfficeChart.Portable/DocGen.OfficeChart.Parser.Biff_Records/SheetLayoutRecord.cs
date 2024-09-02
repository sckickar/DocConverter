using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SheetLayout)]
[CLSCompliant(false)]
internal class SheetLayoutRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 20;

	[BiffRecordPos(0, 2, true)]
	private short m_id;

	[BiffRecordPos(2, 4, true)]
	private int m_iReserved1;

	[BiffRecordPos(6, 4, true)]
	private int m_iReserved2;

	[BiffRecordPos(10, 2, true)]
	private short m_sReserved3;

	[BiffRecordPos(12, 4, true)]
	private int m_iUnknown = 20;

	[BiffRecordPos(16, 4, true)]
	private int m_iColorIndex;

	public short Id
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public int Reserved1
	{
		get
		{
			return m_iReserved1;
		}
		set
		{
			m_iReserved1 = value;
		}
	}

	public int Reserved2
	{
		get
		{
			return m_iReserved2;
		}
		set
		{
			m_iReserved2 = value;
		}
	}

	public short Reserved3
	{
		get
		{
			return m_sReserved3;
		}
		set
		{
			m_sReserved3 = value;
		}
	}

	public int Unknown
	{
		get
		{
			return m_iUnknown;
		}
		set
		{
			m_iUnknown = value;
		}
	}

	public int ColorIndex
	{
		get
		{
			return m_iColorIndex;
		}
		set
		{
			m_iColorIndex = value;
		}
	}

	public override int MinimumRecordSize => 20;

	public override int MaximumRecordSize => 20;

	public override int MaximumMemorySize => 20;

	public SheetLayoutRecord()
	{
		m_id = (short)base.TypeCode;
	}

	public SheetLayoutRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SheetLayoutRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_id = provider.ReadInt16(iOffset);
		m_iReserved1 = provider.ReadInt32(iOffset + 2);
		m_iReserved2 = provider.ReadInt32(iOffset + 6);
		m_sReserved3 = provider.ReadInt16(iOffset + 10);
		m_iUnknown = provider.ReadInt32(iOffset + 12);
		m_iColorIndex = provider.ReadInt32(iOffset + 16);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteInt16(iOffset, m_id);
		provider.WriteInt32(iOffset + 2, m_iReserved1);
		provider.WriteInt32(iOffset + 6, m_iReserved2);
		provider.WriteInt16(iOffset + 10, m_sReserved3);
		provider.WriteInt32(iOffset + 12, m_iUnknown);
		provider.WriteInt32(iOffset + 16, m_iColorIndex);
		m_iLength = 20;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 20;
	}
}
