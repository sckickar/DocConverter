namespace DocGen.Chart.Renderers;

internal class BarRenderer : ColumnRenderer
{
	protected override string RegionDescription => "Bar Chart Region";

	public BarRenderer(ChartSeries series)
		: base(series)
	{
	}
}
