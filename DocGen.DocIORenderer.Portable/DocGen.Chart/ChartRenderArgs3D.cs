namespace DocGen.Chart;

internal sealed class ChartRenderArgs3D : ChartRenderArgs
{
	private Graphics3D m_graph;

	private double m_z;

	private double m_depth;

	public Graphics3D Graph
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

	public double Z
	{
		get
		{
			return m_z;
		}
		set
		{
			m_z = value;
		}
	}

	public double Depth
	{
		get
		{
			return m_depth;
		}
		set
		{
			m_depth = value;
		}
	}

	public ChartRenderArgs3D(IChartAreaHost chart, ChartSeries series)
		: base(chart, series)
	{
	}

	public Vector3D GetVector(double x, double y)
	{
		GetPoint(x, y);
		return new Vector3D(x, y, m_z);
	}
}
