using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartGanttConfigItem : ChartConfigItem
{
	private ChartGanttDrawMode m_ganttDrawMode;

	[DefaultValue(ChartGanttDrawMode.CustomPointWidthMode)]
	public ChartGanttDrawMode DrawMode
	{
		get
		{
			return m_ganttDrawMode;
		}
		set
		{
			if (m_ganttDrawMode != value)
			{
				m_ganttDrawMode = value;
				RaisePropertyChanged("GanttDrawMode");
			}
		}
	}
}
