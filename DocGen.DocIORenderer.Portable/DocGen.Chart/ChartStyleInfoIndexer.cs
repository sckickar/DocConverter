namespace DocGen.Chart;

internal class ChartStyleInfoIndexer
{
	private IChartSeriesStylesModel m_chartSeries;

	public ChartStyleInfo this[int index] => m_chartSeries.ComposedStyles[index];

	public int Count => m_chartSeries.ComposedStyles.Count;

	internal ChartStyleInfoIndexer(IChartSeriesStylesModel chartSeries)
	{
		m_chartSeries = chartSeries;
	}
}
