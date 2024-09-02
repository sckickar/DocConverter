namespace DocGen.Chart;

internal enum ChartUpdateFlags
{
	None = 0,
	Data = 1,
	Styles = 2,
	Config = 4,
	Indexed = 8,
	Regions = 16,
	Ranges = 32,
	All = 63
}
