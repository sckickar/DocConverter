namespace DocGen.OfficeChart.Parser.Biff_Records;

internal interface IDoubleValue
{
	double DoubleValue { get; }

	TBIFFRecord TypeCode { get; }
}
