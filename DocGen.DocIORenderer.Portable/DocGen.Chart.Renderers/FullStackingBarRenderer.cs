namespace DocGen.Chart.Renderers;

internal class FullStackingBarRenderer : FullStackedColumnRenderer
{
	protected override string RegionDescription => "Full Stacking Bar Chart Region";

	public FullStackingBarRenderer(ChartSeries series)
		: base(series)
	{
	}
}
