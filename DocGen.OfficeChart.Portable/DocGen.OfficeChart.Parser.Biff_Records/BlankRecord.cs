using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Blank)]
[CLSCompliant(false)]
internal class BlankRecord : CellPositionBase
{
	private const int DEF_RECORD_SIZE = 6;

	internal const int DEF_RECORD_SIZE_WITH_HEADER = 10;

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 6;

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 6;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}
}
