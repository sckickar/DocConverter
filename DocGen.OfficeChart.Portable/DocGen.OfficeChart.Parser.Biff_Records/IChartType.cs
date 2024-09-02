namespace DocGen.OfficeChart.Parser.Biff_Records;

internal interface IChartType
{
	bool StackValues { get; set; }

	bool ShowAsPercents { get; set; }
}
