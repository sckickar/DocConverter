using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartLineConfigItem : ChartConfigItem
{
	private bool m_disableLineRegion;

	private bool m_disableLineCap;

	[Description("Gets or sets a value indicating whether Line cap is enabled or disabled for drawing Line series.")]
	[DefaultValue(false)]
	public bool DisableLineCap
	{
		get
		{
			return m_disableLineCap;
		}
		set
		{
			if (value != m_disableLineCap)
			{
				m_disableLineCap = value;
				RaisePropertyChanged("DisableLineCap");
			}
		}
	}

	public bool DisableLineRegion
	{
		get
		{
			return m_disableLineRegion;
		}
		set
		{
			m_disableLineRegion = value;
			RaisePropertyChanged("DisableLineRegion");
		}
	}
}
