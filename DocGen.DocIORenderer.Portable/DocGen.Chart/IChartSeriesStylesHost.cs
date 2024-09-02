using DocGen.Drawing;

namespace DocGen.Chart;

internal interface IChartSeriesStylesHost
{
	Color BackColor { get; }

	ChartBaseStylesMap GetStylesMap();
}
