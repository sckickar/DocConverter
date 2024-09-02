using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ScenProtect)]
[CLSCompliant(false)]
internal class ScenProtectRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usProtect;

	public bool IsProtected
	{
		get
		{
			return m_usProtect != 0;
		}
		set
		{
			m_usProtect = (value ? ((ushort)1) : ((ushort)0));
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ScenProtectRecord()
	{
	}

	public ScenProtectRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ScenProtectRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usProtect = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usProtect);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
