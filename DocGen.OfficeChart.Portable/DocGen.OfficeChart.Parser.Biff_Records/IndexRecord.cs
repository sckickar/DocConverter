using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Index)]
[CLSCompliant(false)]
internal class IndexRecord : BiffRecordRaw
{
	private const int DEF_FIXED_SIZE = 16;

	private const int DEF_SUB_ITEM_SIZE = 4;

	[BiffRecordPos(0, 4, true)]
	private int m_iReserved0;

	[BiffRecordPos(4, 4, true)]
	private int m_iFirstRow;

	[BiffRecordPos(8, 4, true)]
	private int m_iLastRowAdd1;

	[BiffRecordPos(12, 4, true)]
	private int m_iReserved1;

	private int[] m_arrDbCells;

	private List<DBCellRecord> m_arrDBCellRecords;

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
			return m_iLastRowAdd1;
		}
		set
		{
			m_iLastRowAdd1 = value;
		}
	}

	public int[] DbCells
	{
		get
		{
			return m_arrDbCells;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.Length > 2048)
			{
				throw new ArgumentOutOfRangeException("Worksheet cannot contain more than 2048 DBCells.");
			}
			m_arrDbCells = value;
		}
	}

	public int Reserved0 => m_iReserved0;

	public int Reserved1 => m_iReserved1;

	public override int MinimumRecordSize => 16;

	internal List<DBCellRecord> DbCellRecords
	{
		get
		{
			return m_arrDBCellRecords;
		}
		set
		{
			m_arrDBCellRecords = value;
		}
	}

	public IndexRecord()
	{
	}

	public IndexRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public IndexRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iReserved0 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iFirstRow = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iLastRowAdd1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iReserved1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		InternalDataIntegrityCheck();
		m_arrDbCells = new int[(m_iLength - 16) / 4];
		int num = 0;
		while (iOffset < m_iLength)
		{
			m_arrDbCells[num] = provider.ReadInt32(iOffset);
			iOffset += 4;
			num++;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteInt32(iOffset, m_iReserved0);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iFirstRow);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iLastRowAdd1);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iReserved1);
		iOffset += 4;
		if (m_arrDbCells != null)
		{
			int num = m_arrDbCells.Length;
			for (int i = 0; i < num; i++)
			{
				provider.WriteInt32(iOffset, m_arrDbCells[i]);
				iOffset += 4;
			}
		}
	}

	private void InternalDataIntegrityCheck()
	{
		if (m_iLength % 4 != 0)
		{
			throw new WrongBiffRecordDataException("IndexRecord");
		}
	}

	public void UpdateOffsets()
	{
		if (m_arrDBCellRecords != null)
		{
			int i = 0;
			for (int count = m_arrDBCellRecords.Count; i < count; i++)
			{
				long streamPos = m_arrDBCellRecords[i].StreamPos;
				m_arrDbCells[i] = (int)streamPos;
			}
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = ((m_arrDbCells != null) ? m_arrDbCells.Length : 0);
		return 16 + num * 4;
	}
}
