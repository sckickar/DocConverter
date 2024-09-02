using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.PasswordRev4)]
[CLSCompliant(false)]
internal class PasswordRev4Record : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

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

	public PasswordRev4Record()
	{
	}

	public PasswordRev4Record(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PasswordRev4Record(int iReserve)
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

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
