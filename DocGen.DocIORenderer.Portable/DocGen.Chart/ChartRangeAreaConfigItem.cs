using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartRangeAreaConfigItem : ChartConfigItem
{
	private bool m_ReverseHighLowPoint = true;

	[Description("Gets or sets a value indicating whether High and Low point should be swapped when low point value is higher than high point value.")]
	[DefaultValue(true)]
	public bool SwapHighLowPoint
	{
		get
		{
			return m_ReverseHighLowPoint;
		}
		set
		{
			if (value != m_ReverseHighLowPoint)
			{
				m_ReverseHighLowPoint = value;
				RaisePropertyChanged("SwapHighLowPoint");
			}
		}
	}
}
