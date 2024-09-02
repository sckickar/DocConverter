namespace DocGen.Chart;

internal interface IChartAxisGroupingLabelModel
{
	int Count { get; }

	ChartAxisGroupingLabel GetGroupingLabelAt(int index);
}
