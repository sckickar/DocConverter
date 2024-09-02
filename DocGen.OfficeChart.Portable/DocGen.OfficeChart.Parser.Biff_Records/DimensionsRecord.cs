using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Dimensions)]
[CLSCompliant(false)]
internal class DimensionsRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 14;

	[BiffRecordPos(0, 4, true)]
	private int m_iFirstRow;

	[BiffRecordPos(4, 4, true)]
	private int m_iLastRow;

	[BiffRecordPos(8, 2)]
	private ushort m_usFirstColumn;

	[BiffRecordPos(10, 2)]
	private ushort m_usLastColumn;

	[BiffRecordPos(12, 2)]
	private ushort m_usReserved;

	public ushort Reserved => m_usReserved;

	public int FirstRow
	{
		get
		{
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	public int LastRow
	{
		get
		{
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
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

	public override int MinimumRecordSize => 14;

	public override int MaximumRecordSize => 14;

	public DimensionsRecord()
	{
	}

	public DimensionsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DimensionsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iFirstRow = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iLastRow = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usFirstColumn = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLastColumn = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usReserved = provider.ReadUInt16(iOffset);
		if (m_usLastColumn <= m_usFirstColumn)
		{
			m_usLastColumn = (ushort)(m_usFirstColumn + 1);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 14;
		provider.WriteInt32(iOffset, m_iFirstRow);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iLastRow);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usFirstColumn);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLastColumn);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usReserved);
	}
}
