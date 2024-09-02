using System;

namespace DocGen.OfficeChart;

public interface IOfficeChartSeriesAxis : IOfficeChartAxis
{
	[Obsolete("Please, use TickLabelSpacing instead of it")]
	int LabelFrequency { get; set; }

	int TickLabelSpacing { get; set; }

	int TickMarkSpacing { get; set; }
}
