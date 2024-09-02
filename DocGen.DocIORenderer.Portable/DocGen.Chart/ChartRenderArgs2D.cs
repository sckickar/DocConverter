using DocGen.Drawing;

namespace DocGen.Chart;

internal sealed class ChartRenderArgs2D : ChartRenderArgs
{
	private ChartGraph m_graph;

	private SizeF m_offset;

	private SizeF m_offsetDepth;

	private bool m_is3D;

	public ChartGraph Graph
	{
		get
		{
			return m_graph;
		}
		set
		{
			m_graph = value;
		}
	}

	public SizeF Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	public SizeF DepthOffset
	{
		get
		{
			return m_offsetDepth;
		}
		set
		{
			m_offsetDepth = value;
		}
	}

	public bool Is3D
	{
		get
		{
			return m_is3D;
		}
		set
		{
			m_is3D = value;
		}
	}

	public ChartRenderArgs2D(IChartAreaHost chart, ChartSeries series)
		: base(chart, series)
	{
		m_is3D = chart.Series3D;
	}

	public override PointF GetPoint(double x, double y)
	{
		PointF point = base.GetPoint(x, y);
		if (m_is3D)
		{
			point.X += m_offset.Width;
			point.Y += m_offset.Height;
		}
		RectangleF rectangleF = base.Chart.Bounds;
		point.X = ((point.X < rectangleF.X) ? rectangleF.X : point.X);
		point.Y = ((point.Y < rectangleF.Y) ? rectangleF.X : point.Y);
		point.X = ((point.X > rectangleF.Width) ? rectangleF.Width : point.X);
		point.Y = ((point.Y > rectangleF.Height) ? rectangleF.Height : point.Y);
		return point;
	}
}
