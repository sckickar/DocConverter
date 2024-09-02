namespace DocGen.OfficeChart;

public interface IOfficeChartValueAxis : IOfficeChartAxis
{
	double MinimumValue { get; set; }

	double MaximumValue { get; set; }

	double MajorUnit { get; set; }

	double MinorUnit { get; set; }

	double CrossesAt { get; set; }

	bool IsAutoMin { get; set; }

	bool IsAutoMax { get; set; }

	bool IsAutoMajor { get; set; }

	bool IsAutoMinor { get; set; }

	bool IsAutoCross { get; set; }

	bool IsLogScale { get; set; }

	double LogBase { get; set; }

	bool IsMaxCross { get; set; }

	double DisplayUnitCustom { get; set; }

	OfficeChartDisplayUnit DisplayUnit { get; set; }

	bool HasDisplayUnitLabel { get; set; }

	IOfficeChartTextArea DisplayUnitLabel { get; }
}
