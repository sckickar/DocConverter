using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SaveRecalc)]
[CLSCompliant(false)]
internal class SaveRecalcRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usSaveRecalc = 1;

	public ushort RecalcOnSave
	{
		get
		{
			return m_usSaveRecalc;
		}
		set
		{
			m_usSaveRecalc = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public SaveRecalcRecord()
	{
	}

	public SaveRecalcRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SaveRecalcRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usSaveRecalc = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usSaveRecalc);
	}
}
