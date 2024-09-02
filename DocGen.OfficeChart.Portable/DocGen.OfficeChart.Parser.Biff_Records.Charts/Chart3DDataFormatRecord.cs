using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.Chart3DDataFormat)]
[CLSCompliant(false)]
internal class Chart3DDataFormatRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 1)]
	private byte m_DataFormatBase;

	[BiffRecordPos(1, 1)]
	private byte m_DataFormatTop;

	public OfficeBaseFormat DataFormatBase
	{
		get
		{
			return (OfficeBaseFormat)m_DataFormatBase;
		}
		set
		{
			m_DataFormatBase = (byte)value;
		}
	}

	public OfficeTopFormat DataFormatTop
	{
		get
		{
			return (OfficeTopFormat)m_DataFormatTop;
		}
		set
		{
			m_DataFormatTop = (byte)value;
		}
	}

	public Chart3DDataFormatRecord()
	{
	}

	public Chart3DDataFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public Chart3DDataFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_DataFormatBase = provider.ReadByte(iOffset);
		m_DataFormatTop = provider.ReadByte(iOffset + 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteByte(iOffset, m_DataFormatBase);
		provider.WriteByte(iOffset + 1, m_DataFormatTop);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
