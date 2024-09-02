namespace DocGen.OfficeChart;

public interface IChartLegendEntries
{
	int Count { get; }

	IOfficeChartLegendEntry this[int iIndex] { get; }
}
