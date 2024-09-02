using System.ComponentModel;

namespace DocGen.Chart;

internal interface IChartSeriesModel
{
	int Count { get; }

	event ListChangedEventHandler Changed;

	double GetX(int xIndex);

	double[] GetY(int xIndex);

	bool GetEmpty(int xIndex);
}
