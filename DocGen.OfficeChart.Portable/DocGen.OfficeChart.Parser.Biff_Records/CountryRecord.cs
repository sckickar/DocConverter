using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Country)]
[CLSCompliant(false)]
internal class CountryRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usDefault = 1;

	[BiffRecordPos(2, 2)]
	private ushort m_usCurrent = 1;

	public ushort DefaultCountry
	{
		get
		{
			return m_usDefault;
		}
		set
		{
			m_usDefault = value;
		}
	}

	public ushort CurrentCountry
	{
		get
		{
			return m_usCurrent;
		}
		set
		{
			m_usCurrent = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize => 4;

	public CountryRecord()
	{
	}

	public CountryRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CountryRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usDefault = provider.ReadUInt16(iOffset);
		m_usCurrent = provider.ReadUInt16(iOffset + 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usDefault);
		provider.WriteUInt16(iOffset + 2, m_usCurrent);
		m_iLength = 4;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
