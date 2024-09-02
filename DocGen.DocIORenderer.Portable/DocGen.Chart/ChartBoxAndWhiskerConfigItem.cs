using System.ComponentModel;

namespace DocGen.Chart;

internal sealed class ChartBoxAndWhiskerConfigItem : ChartConfigItem
{
	private bool m_percentileMode;

	private double m_percentile;

	private double m_outlierWidth;

	[Description("Gets or sets a value indicating whether chart render in percentile mode or in normal mode.")]
	[DefaultValue(false)]
	public bool PercentileMode
	{
		get
		{
			return m_percentileMode;
		}
		set
		{
			if (value != m_percentileMode)
			{
				m_percentileMode = value;
				RaisePropertyChanged("PercentileMode");
			}
		}
	}

	[Description("Gets or sets the percentile. It should be lie between 0.0 to 0.25")]
	[DefaultValue(0)]
	public double Percentile
	{
		get
		{
			return m_percentile;
		}
		set
		{
			if (value != m_percentile)
			{
				m_percentile = value;
				RaisePropertyChanged("Percentile");
			}
		}
	}

	[Description("Gets or sets the width of the outlier. Value should be greater than zero and it starts from 1.")]
	[DefaultValue(0)]
	public double OutLierWidth
	{
		get
		{
			return m_outlierWidth;
		}
		set
		{
			if (value != m_outlierWidth)
			{
				m_outlierWidth = value;
				RaisePropertyChanged("OutLierWidth");
			}
		}
	}
}
