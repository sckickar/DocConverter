namespace DocGen.OfficeChart;

internal interface IFillColor
{
	ChartColor ForeGroundColorObject { get; }

	ChartColor BackGroundColorObject { get; }

	OfficePattern Pattern { get; set; }

	bool IsAutomaticFormat { get; set; }

	IOfficeFill Fill { get; }

	bool Visible { get; set; }
}
