using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.OleSize)]
[CLSCompliant(false)]
internal class OleSizeRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usReserved;

	[BiffRecordPos(2, 2)]
	private ushort m_usFirstRow;

	[BiffRecordPos(4, 2)]
	private ushort m_usLastRow;

	[BiffRecordPos(6, 1)]
	private byte m_FirstColumn;

	[BiffRecordPos(7, 1)]
	private byte m_LastColumn;

	public ushort Reserved => m_usReserved;

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

	public byte FirstColumn
	{
		get
		{
			return m_FirstColumn;
		}
		set
		{
			m_FirstColumn = value;
		}
	}

	public byte LastColumn
	{
		get
		{
			return m_LastColumn;
		}
		set
		{
			m_LastColumn = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public OleSizeRecord()
	{
	}

	public OleSizeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public OleSizeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usReserved = provider.ReadUInt16(iOffset);
		m_usFirstRow = provider.ReadUInt16(iOffset + 2);
		m_usLastRow = provider.ReadUInt16(iOffset + 4);
		m_FirstColumn = provider.ReadByte(iOffset + 6);
		m_LastColumn = provider.ReadByte(iOffset + 7);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usReserved);
		provider.WriteUInt16(iOffset + 2, m_usFirstRow);
		provider.WriteUInt16(iOffset + 4, m_usLastRow);
		provider.WriteByte(iOffset + 6, m_FirstColumn);
		provider.WriteByte(iOffset + 7, m_LastColumn);
		m_iLength = 8;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
