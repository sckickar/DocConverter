using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSeriesText)]
[CLSCompliant(false)]
internal class ChartSeriesTextRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 3;

	[BiffRecordPos(0, 2)]
	private ushort m_usTextId;

	[BiffRecordPos(2, 1, TFieldType.String)]
	private string m_strText = string.Empty;

	public ushort TextId
	{
		get
		{
			return m_usTextId;
		}
		set
		{
			m_usTextId = value;
		}
	}

	public string Text
	{
		get
		{
			return m_strText;
		}
		set
		{
			m_strText = value;
		}
	}

	public override int MinimumRecordSize => 3;

	public ChartSeriesTextRecord()
	{
	}

	public ChartSeriesTextRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSeriesTextRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usTextId = provider.ReadUInt16(iOffset);
		m_strText = provider.ReadString8Bit(iOffset + 2, out var _);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usTextId);
		int num = iOffset;
		iOffset += 2;
		provider.WriteString8BitUpdateOffset(ref iOffset, m_strText);
		m_iLength = iOffset - num;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4 + m_strText.Length * 2;
	}
}
