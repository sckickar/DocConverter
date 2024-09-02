using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ProtectionRev4)]
[CLSCompliant(false)]
internal class ProtectionRev4Record : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usProtection;

	public ushort IsProtected
	{
		get
		{
			return m_usProtection;
		}
		set
		{
			m_usProtection = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ProtectionRev4Record()
	{
	}

	public ProtectionRev4Record(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ProtectionRev4Record(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usProtection = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usProtection);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
