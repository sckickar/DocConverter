namespace DocGen.Chart;

internal interface IChartAxisLabelModel
{
	int Count { get; }

	ChartAxisLabel GetLabelAt(int index);
}
