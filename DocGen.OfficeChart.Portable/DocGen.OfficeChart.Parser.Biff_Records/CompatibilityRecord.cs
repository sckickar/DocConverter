using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Compatibility)]
[CLSCompliant(false)]
internal class CompatibilityRecord : BiffRecordRaw
{
	private FutureHeader m_header;

	private uint m_bNoCompCheck;

	public uint NoComptabilityCheck
	{
		get
		{
			return m_bNoCompCheck;
		}
		set
		{
			m_bNoCompCheck = value;
		}
	}

	public CompatibilityRecord()
	{
		m_header = new FutureHeader();
		m_header.Type = 2188;
	}

	public override void ParseStructure(DataProvider arrData, int iOffset, int iLength, OfficeVersion version)
	{
		m_bNoCompCheck = arrData.ReadUInt32(iOffset + 12);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_header.Type);
		provider.WriteUInt16(iOffset + 2, m_header.Attributes);
		provider.WriteInt64(iOffset + 4, 0L);
		provider.WriteUInt32(iOffset + 12, m_bNoCompCheck);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
