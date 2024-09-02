namespace DocGen.Chart.Renderers;

internal class FullStackingLineRenderer : StackingLineRenderer
{
	protected override string RegionDescription => "Full stacking Line";

	internal FullStackingLineRenderer(ChartSeries series)
		: base(series)
	{
	}
}
