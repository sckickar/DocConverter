namespace DocGen.Chart;

internal interface IChartSeriesSummary
{
	double MaxX { get; }

	double MaxY { get; }

	double MinX { get; }

	double MinY { get; }

	IChartSeriesModel ModelImpl { get; set; }

	IChartSeriesCategory CategoryModel { get; set; }

	void Refresh();

	double GetYPercentage(int pointIndex);

	double GetYPercentage(int pointIndex, int yIndex);

	ChartPoint FindValue(double value);

	ChartPoint FindValue(double value, string useValue);

	ChartPoint FindValue(double value, string useValue, ref int index);

	ChartPoint FindValue(double value, string useValue, ref int index, int endIndex);

	ChartPoint FindMinValue();

	ChartPoint FindMinValue(string useValue);

	ChartPoint FindMinValue(string useValue, ref int index);

	ChartPoint FindMinValue(string useValue, ref int index, int endIndex);

	ChartPoint FindMaxValue();

	ChartPoint FindMaxValue(string useValue);

	ChartPoint FindMaxValue(string useValue, ref int index);

	ChartPoint FindMaxValue(string useValue, ref int index, int endIndex);
}
