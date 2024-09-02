using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartRadarConfigItem : ChartConfigItem
{
	private ChartRadarDrawType type;

	[DefaultValue(ChartRadarDrawType.Area)]
	public ChartRadarDrawType Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				RaisePropertyChanged("Type");
			}
		}
	}
}
