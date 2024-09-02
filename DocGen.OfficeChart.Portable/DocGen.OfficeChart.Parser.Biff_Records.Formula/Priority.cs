namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

internal enum Priority
{
	None,
	Equality,
	Concat,
	PlusMinus,
	MulDiv,
	Power,
	UnaryMinus,
	CellRange
}
