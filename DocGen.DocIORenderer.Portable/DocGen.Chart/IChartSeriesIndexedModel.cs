using System.ComponentModel;

namespace DocGen.Chart;

internal interface IChartSeriesIndexedModel
{
	int Count { get; }

	event ListChangedEventHandler Changed;

	double[] GetY(int xIndex);

	bool GetEmpty(int xIndex);
}
