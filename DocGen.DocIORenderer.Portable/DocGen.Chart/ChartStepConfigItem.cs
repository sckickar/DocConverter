using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartStepConfigItem : ChartConfigItem
{
	private bool inverted;

	[DefaultValue(false)]
	public bool Inverted
	{
		get
		{
			return inverted;
		}
		set
		{
			if (inverted != value)
			{
				inverted = value;
				RaisePropertyChanged("Inverted");
			}
		}
	}
}
