using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Backup)]
[CLSCompliant(false)]
internal class BackupRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usBackup;

	public ushort Backup
	{
		get
		{
			return m_usBackup;
		}
		set
		{
			m_usBackup = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public BackupRecord()
	{
	}

	public BackupRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BackupRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usBackup = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usBackup);
		m_iLength = 2;
	}
}
