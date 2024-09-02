namespace DocGen.Chart;

internal interface IChartPointMinMax
{
	double X { get; }

	double Min { get; }

	double Max { get; }

	double GetY(int index, int yIndex);
}
