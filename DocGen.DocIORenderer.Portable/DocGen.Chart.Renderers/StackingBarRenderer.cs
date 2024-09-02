namespace DocGen.Chart.Renderers;

internal class StackingBarRenderer : StackingColumnRenderer
{
	protected override string RegionDescription => "Stacking Bar Chart Region";

	public StackingBarRenderer(ChartSeries series)
		: base(series)
	{
	}
}
