using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DCON)]
[CLSCompliant(false)]
internal class DCONRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 2, true)]
	private short m_sFuncIndex;

	[BiffRecordPos(2, 2, true)]
	private short m_sLeftColumn;

	[BiffRecordPos(4, 2, true)]
	private short m_sTopRow;

	[BiffRecordPos(6, 2, true)]
	private short m_sLinkSource;

	public short FuncIndex
	{
		get
		{
			return m_sFuncIndex;
		}
		set
		{
			m_sFuncIndex = value;
		}
	}

	public bool IsLeftColumn
	{
		get
		{
			return m_sLeftColumn == 1;
		}
		set
		{
			m_sLeftColumn = (value ? ((short)1) : ((short)0));
		}
	}

	public bool IsTopRow
	{
		get
		{
			return m_sTopRow == 1;
		}
		set
		{
			m_sTopRow = (value ? ((short)1) : ((short)0));
		}
	}

	public bool IsLinkSource
	{
		get
		{
			return m_sLinkSource == 1;
		}
		set
		{
			m_sLinkSource = (value ? ((short)1) : ((short)0));
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public DCONRecord()
	{
	}

	public DCONRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DCONRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_sFuncIndex = provider.ReadInt16(iOffset);
		m_sLeftColumn = provider.ReadInt16(iOffset + 2);
		m_sTopRow = provider.ReadInt16(iOffset + 4);
		m_sLinkSource = provider.ReadInt16(iOffset + 6);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteInt16(iOffset, m_sFuncIndex);
		provider.WriteInt16(iOffset + 2, m_sLeftColumn);
		provider.WriteInt16(iOffset + 4, m_sTopRow);
		provider.WriteInt16(iOffset + 6, m_sLinkSource);
	}
}
