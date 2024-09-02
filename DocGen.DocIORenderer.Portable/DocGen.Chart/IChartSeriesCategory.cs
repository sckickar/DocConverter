using System.ComponentModel;

namespace DocGen.Chart;

internal interface IChartSeriesCategory
{
	event ListChangedEventHandler Changed;

	double[] GetY(int xIndex);

	string GetCategory(int xIndex);
}
