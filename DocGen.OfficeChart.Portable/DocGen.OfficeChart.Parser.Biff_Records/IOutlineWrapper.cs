using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal interface IOutlineWrapper : IOutline
{
	int FirstIndex { get; set; }

	int LastIndex { get; set; }

	IOutline Outline { get; set; }

	IRange OutlineRange { get; set; }

	OfficeGroupBy GroupBy { get; set; }
}
