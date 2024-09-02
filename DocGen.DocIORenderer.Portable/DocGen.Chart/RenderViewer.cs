using System.Collections;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class RenderViewer
{
	private ArrayList m_paths = new ArrayList();

	private bool m_needRegionUpdate;

	private bool m_axisInverted;

	private IList m_regions;

	public bool NeedRegionUpdate
	{
		get
		{
			return m_needRegionUpdate;
		}
		set
		{
			m_needRegionUpdate = value;
		}
	}

	public bool AxisInverted
	{
		get
		{
			return m_axisInverted;
		}
		set
		{
			m_axisInverted = value;
		}
	}

	public IList Regions
	{
		get
		{
			return m_regions;
		}
		set
		{
			m_regions = value;
		}
	}

	public void AddSegment(ChartSegment segment)
	{
		m_paths.Add(segment);
	}

	public void Add(ChartSeriesRenderer render)
	{
		if (render.Segments != null)
		{
			m_paths.AddRange(render.Segments);
		}
	}

	public IEnumerable Sort()
	{
		if (m_paths.Count > 0)
		{
			m_paths.Sort(new ChartSegmentComparer(m_axisInverted));
		}
		return m_paths;
	}

	public void View(Graphics g)
	{
		Sort();
		foreach (ChartSegment path in m_paths)
		{
			path.Draw(g);
		}
	}

	public void Clear()
	{
		m_paths.Clear();
	}
}
