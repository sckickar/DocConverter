using DocGen.Styles;

namespace DocGen.Chart;

internal interface IChartSeriesStylesModel
{
	ChartStyleInfo Style { get; }

	IChartSeriesComposedStylesModel ComposedStyles { get; }

	event ChartStyleChangedEventHandler Changed;

	ChartStyleInfo GetStyleAt(int index);

	void ChangeStyleAt(ChartStyleInfo style, int index);

	void ChangeStyle(ChartStyleInfo style);

	ChartStyleInfo[] GetBaseStyles(IStyleInfo chartStyleInfo, int index);
}
