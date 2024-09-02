namespace DocGen.Chart.Renderers;

internal class FullStackedColumnRenderer : StackingColumnRenderer
{
	protected override string RegionDescription => "Full Stacking Column Chart Region";

	public FullStackedColumnRenderer(ChartSeries series)
		: base(series)
	{
	}
}
