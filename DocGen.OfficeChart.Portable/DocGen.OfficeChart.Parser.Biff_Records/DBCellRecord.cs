using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DBCell)]
[CLSCompliant(false)]
internal class DBCellRecord : BiffRecordRaw
{
	private const int DEF_FIXED_SIZE = 4;

	private const int DEF_SUB_ITEM_SIZE = 2;

	[BiffRecordPos(0, 4, false)]
	private int m_iRowOffset;

	private ushort[] m_arrCellOffset = new ushort[1];

	public int RowOffset
	{
		get
		{
			return m_iRowOffset;
		}
		set
		{
			m_iRowOffset = value;
		}
	}

	public ushort[] CellOffsets
	{
		get
		{
			return m_arrCellOffset;
		}
		set
		{
			m_arrCellOffset = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public DBCellRecord()
	{
	}

	public DBCellRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DBCellRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iRowOffset = provider.ReadInt32(iOffset);
		iOffset += 4;
		InternalDataIntegrityCheck();
		int num = (m_iLength - 4) / 2;
		m_arrCellOffset = new ushort[num];
		int num2 = 0;
		while (num2 < num)
		{
			m_arrCellOffset[num2] = provider.ReadUInt16(iOffset);
			num2++;
			iOffset += 2;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		int num = m_arrCellOffset.Length;
		m_iLength = GetStoreSize(version);
		provider.WriteInt32(iOffset, m_iRowOffset);
		iOffset += 4;
		for (int i = 0; i < num; i++)
		{
			provider.WriteUInt16(iOffset, m_arrCellOffset[i]);
			iOffset += 2;
		}
	}

	private void InternalDataIntegrityCheck()
	{
		if ((base.Length - 2) % 2 != 0)
		{
			throw new WrongBiffRecordDataException("DBCellRecord");
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4 + m_arrCellOffset.Length * 2;
	}
}
