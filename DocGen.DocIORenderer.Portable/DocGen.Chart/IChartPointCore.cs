namespace DocGen.Chart;

internal interface IChartPointCore
{
	double X { get; set; }

	double[] Y { get; set; }

	bool IsEmpty { get; set; }
}
