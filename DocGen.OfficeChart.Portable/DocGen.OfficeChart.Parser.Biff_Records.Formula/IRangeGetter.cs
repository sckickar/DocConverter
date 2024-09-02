namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

internal interface IRangeGetter
{
	IRange GetRange(IWorkbook book, IWorksheet sheet);
}
