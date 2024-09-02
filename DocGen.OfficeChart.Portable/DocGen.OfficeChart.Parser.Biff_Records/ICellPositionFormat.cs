using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal interface ICellPositionFormat
{
	int Row { get; set; }

	int Column { get; set; }

	ushort ExtendedFormatIndex { get; set; }

	TBIFFRecord TypeCode { get; }
}
