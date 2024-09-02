using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartDefaultText)]
[CLSCompliant(false)]
internal class ChartDefaultTextRecord : BiffRecordRaw
{
	public enum TextDefaults
	{
		ShowLabels,
		ValueAndPercents,
		All
	}

	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usObjectIdentifier;

	public TextDefaults TextCharacteristics
	{
		get
		{
			return (TextDefaults)m_usObjectIdentifier;
		}
		set
		{
			m_usObjectIdentifier = (ushort)value;
		}
	}

	public ChartDefaultTextRecord()
	{
	}

	public ChartDefaultTextRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartDefaultTextRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usObjectIdentifier = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usObjectIdentifier);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
