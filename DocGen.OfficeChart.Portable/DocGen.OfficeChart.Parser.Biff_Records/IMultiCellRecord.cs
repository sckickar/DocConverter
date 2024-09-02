using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal interface IMultiCellRecord : ICellPositionFormat
{
	int FirstColumn { get; set; }

	int LastColumn { get; set; }

	int SubRecordSize { get; }

	TBIFFRecord SubRecordType { get; }

	int GetSeparateSubRecordSize(OfficeVersion version);

	void Insert(ICellPositionFormat cell);

	ICellPositionFormat[] Split(int iColumnIndex);

	BiffRecordRaw[] Split(bool bIgnoreStyles);
}
