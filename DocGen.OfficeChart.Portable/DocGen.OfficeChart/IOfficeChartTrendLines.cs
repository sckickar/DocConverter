namespace DocGen.OfficeChart;

public interface IOfficeChartTrendLines
{
	IOfficeChartTrendLine this[int iIndex] { get; }

	int Count { get; }

	IOfficeChartTrendLine Add();

	IOfficeChartTrendLine Add(OfficeTrendLineType type);

	void RemoveAt(int index);

	void Clear();
}
