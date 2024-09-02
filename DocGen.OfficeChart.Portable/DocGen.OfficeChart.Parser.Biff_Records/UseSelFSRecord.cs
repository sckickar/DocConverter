using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.UseSelFS)]
[CLSCompliant(false)]
internal class UseSelFSRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usFlags;

	public bool Flags
	{
		get
		{
			return (m_usFlags & 1) == 1;
		}
		set
		{
			m_usFlags = (value ? ((ushort)1) : ((ushort)0));
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public UseSelFSRecord()
	{
	}

	public UseSelFSRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public UseSelFSRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFlags = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usFlags);
	}
}
