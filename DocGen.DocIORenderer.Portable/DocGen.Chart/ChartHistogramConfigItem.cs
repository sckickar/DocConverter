using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartHistogramConfigItem : ChartConfigItem
{
	private bool m_showNormalDistribution;

	private bool m_showDataPoints = true;

	private int m_numberOfIntervals = 10;

	[DefaultValue(false)]
	public bool ShowNormalDistribution
	{
		get
		{
			return m_showNormalDistribution;
		}
		set
		{
			if (m_showNormalDistribution != value)
			{
				m_showNormalDistribution = value;
				RaisePropertyChanged("ShowNormalDistribution");
			}
		}
	}

	[DefaultValue(true)]
	public bool ShowDataPoints
	{
		get
		{
			return m_showDataPoints;
		}
		set
		{
			if (m_showDataPoints != value)
			{
				m_showDataPoints = value;
				RaisePropertyChanged("ShowDataPoints");
			}
		}
	}

	[DefaultValue(10)]
	public int NumberOfIntervals
	{
		get
		{
			return m_numberOfIntervals;
		}
		set
		{
			if (m_numberOfIntervals != value)
			{
				m_numberOfIntervals = value;
				RaisePropertyChanged("NumberOfIntervals");
			}
		}
	}
}
