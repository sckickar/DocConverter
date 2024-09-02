using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal interface IOutline
{
	ushort OutlineLevel { get; set; }

	bool IsCollapsed { get; set; }

	bool IsHidden { get; set; }

	ushort ExtendedFormatIndex { get; set; }

	ushort Index { get; set; }
}
