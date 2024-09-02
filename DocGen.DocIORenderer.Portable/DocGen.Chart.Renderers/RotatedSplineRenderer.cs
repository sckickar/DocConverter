namespace DocGen.Chart.Renderers;

internal class RotatedSplineRenderer : SplineRenderer
{
	protected override string RegionDescription => "RotatedSpline Chart\tRegion";

	public RotatedSplineRenderer(ChartSeries series)
		: base(series)
	{
	}
}
