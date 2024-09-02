using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartCustomPointsRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	public ChartCustomPointsRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		base.RenderAdornments(g);
	}

	public override void Render(Graphics3D g)
	{
		base.RenderAdornments(g);
	}
}
