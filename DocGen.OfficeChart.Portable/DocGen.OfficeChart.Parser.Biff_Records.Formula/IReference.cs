using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[CLSCompliant(false)]
internal interface IReference
{
	ushort RefIndex { get; set; }
}
