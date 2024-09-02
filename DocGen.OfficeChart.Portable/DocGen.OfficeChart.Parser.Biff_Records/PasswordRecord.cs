using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Password)]
[CLSCompliant(false)]
internal class PasswordRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usPassword;

	public ushort IsPassword
	{
		get
		{
			return m_usPassword;
		}
		set
		{
			m_usPassword = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public PasswordRecord()
	{
	}

	public PasswordRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PasswordRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPassword = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usPassword);
		m_iLength = 2;
	}
}
