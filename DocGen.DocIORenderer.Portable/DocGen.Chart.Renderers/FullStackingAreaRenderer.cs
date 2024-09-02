namespace DocGen.Chart.Renderers;

internal class FullStackingAreaRenderer : StackingAreaRenderer
{
	protected override string RegionDescription => "Full stacking area";

	public FullStackingAreaRenderer(ChartSeries series)
		: base(series)
	{
	}
}
