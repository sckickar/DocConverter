using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.RefMode)]
[CLSCompliant(false)]
internal class RefModeRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usRefMode = 1;

	public ushort IsA1ReferenceMode
	{
		get
		{
			return m_usRefMode;
		}
		set
		{
			m_usRefMode = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public RefModeRecord()
	{
	}

	public RefModeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public RefModeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRefMode = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usRefMode);
	}
}
