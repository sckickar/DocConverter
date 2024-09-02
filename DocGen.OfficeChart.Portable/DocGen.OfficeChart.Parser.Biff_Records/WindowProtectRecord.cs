using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.WindowProtect)]
[CLSCompliant(false)]
internal class WindowProtectRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usProtection;

	public bool IsProtected
	{
		get
		{
			return m_usProtection == 1;
		}
		set
		{
			m_usProtection = (value ? ((ushort)1) : ((ushort)0));
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public WindowProtectRecord()
	{
	}

	public WindowProtectRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WindowProtectRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usProtection = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usProtection);
	}
}
