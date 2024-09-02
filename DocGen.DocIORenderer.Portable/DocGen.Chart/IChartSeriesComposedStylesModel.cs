using DocGen.Styles;

namespace DocGen.Chart;

internal interface IChartSeriesComposedStylesModel
{
	ChartStyleInfo Style { get; }

	ChartStyleInfo this[int index] { get; }

	int Count { get; }

	ChartStyleInfo GetOfflineStyle();

	ChartStyleInfo GetOfflineStyle(int index);

	void ResetCache();

	ChartStyleInfo[] GetBaseStyles(IStyleInfo chartStyleInfo, int index);

	void ChangeStyle(ChartStyleInfo style, int index);
}
