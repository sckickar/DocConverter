using System;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal interface ISharedFormula
{
	int FirstRow { get; set; }

	int LastRow { get; set; }

	int FirstColumn { get; set; }

	int LastColumn { get; set; }

	Ptg[] Formula { get; }
}
