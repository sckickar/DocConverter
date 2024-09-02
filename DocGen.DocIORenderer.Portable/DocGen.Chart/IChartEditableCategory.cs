namespace DocGen.Chart;

internal interface IChartEditableCategory : IChartSeriesCategory
{
	void SetCategory(int xIndex, string category);

	void Add(string category);
}
