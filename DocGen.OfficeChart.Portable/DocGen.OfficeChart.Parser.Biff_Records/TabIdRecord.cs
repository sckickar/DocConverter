using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.TabId)]
[CLSCompliant(false)]
internal class TabIdRecord : BiffRecordRaw
{
	private ushort[] m_arrTabIds = new ushort[1] { 1 };

	public ushort[] TabIds
	{
		get
		{
			return m_arrTabIds;
		}
		set
		{
			m_arrTabIds = value;
		}
	}

	public TabIdRecord()
	{
	}

	public TabIdRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public TabIdRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		InternalDataIntegrityCheck();
		m_arrTabIds = new ushort[base.Length / 2];
		int num = 0;
		int num2 = m_iLength + iOffset;
		while (iOffset < num2)
		{
			m_arrTabIds[num] = provider.ReadUInt16(iOffset);
			num++;
			iOffset += 2;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(OfficeVersion.Excel97to2003);
		int i = 0;
		for (int num = m_arrTabIds.Length; i < num; i++)
		{
			provider.WriteUInt16(iOffset, m_arrTabIds[i]);
			iOffset += 2;
		}
	}

	private void InternalDataIntegrityCheck()
	{
		if (m_iLength % 2 != 0)
		{
			throw new WrongBiffRecordDataException("MergeCellsRecord");
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return m_arrTabIds.Length * 2;
	}
}
