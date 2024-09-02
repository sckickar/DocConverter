using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal interface IFormulaRecord
{
	Ptg[] Formula { get; set; }
}
