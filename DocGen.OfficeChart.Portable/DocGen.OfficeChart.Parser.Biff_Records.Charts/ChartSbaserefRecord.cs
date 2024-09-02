using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSbaseref)]
[CLSCompliant(false)]
internal class ChartSbaserefRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usFirstRow;

	[BiffRecordPos(2, 2)]
	private ushort m_usLastRow;

	[BiffRecordPos(4, 2)]
	private ushort m_usFirstColumn;

	[BiffRecordPos(6, 2)]
	private ushort m_usLastColumn;

	public ushort FirstRow
	{
		get
		{
			return m_usFirstRow;
		}
		set
		{
			m_usFirstRow = value;
		}
	}

	public ushort LastRow
	{
		get
		{
			return m_usLastRow;
		}
		set
		{
			m_usLastRow = value;
		}
	}

	public ushort FirstColumn
	{
		get
		{
			return m_usFirstColumn;
		}
		set
		{
			m_usFirstColumn = value;
		}
	}

	public ushort LastColumn
	{
		get
		{
			return m_usLastColumn;
		}
		set
		{
			m_usLastColumn = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public ChartSbaserefRecord()
	{
	}

	public ChartSbaserefRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSbaserefRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFirstRow = provider.ReadUInt16(iOffset);
		m_usLastRow = provider.ReadUInt16(iOffset + 2);
		m_usFirstColumn = provider.ReadUInt16(iOffset + 4);
		m_usLastColumn = provider.ReadUInt16(iOffset + 6);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFirstRow);
		provider.WriteUInt16(iOffset + 2, m_usLastRow);
		provider.WriteUInt16(iOffset + 4, m_usFirstColumn);
		provider.WriteUInt16(iOffset + 6, m_usLastColumn);
		m_iLength = 8;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
