using System;

namespace DocGen.Chart;

internal sealed class ChartSeriesParameters
{
	private ChartArea m_area;

	private float m_seriesPointSpacing = 0.3f;

	private float m_seriesDepthSpacing = 0.1f;

	private float m_seriesSpacing;

	public float SeriesSpacing
	{
		get
		{
			return m_seriesSpacing;
		}
		set
		{
			if (m_seriesSpacing != value)
			{
				m_seriesSpacing = value;
			}
		}
	}

	public float PointSpacing
	{
		get
		{
			return m_seriesPointSpacing;
		}
		set
		{
			if (m_seriesPointSpacing != value)
			{
				m_seriesPointSpacing = value;
			}
		}
	}

	public float SeriesDepthSpacing
	{
		get
		{
			return m_seriesDepthSpacing;
		}
		set
		{
			if (m_seriesDepthSpacing != value)
			{
				m_seriesDepthSpacing = value;
			}
		}
	}

	internal ChartSeriesParameters(ChartArea area)
	{
		if (area == null)
		{
			throw new ArgumentNullException("area");
		}
		m_area = area;
	}
}
