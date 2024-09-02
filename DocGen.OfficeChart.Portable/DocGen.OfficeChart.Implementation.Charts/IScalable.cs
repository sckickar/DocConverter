namespace DocGen.OfficeChart.Implementation.Charts;

internal interface IScalable
{
	bool IsLogScale { get; set; }

	bool ReversePlotOrder { get; set; }

	double MaximumValue { get; set; }

	double MinimumValue { get; set; }

	double LogBase { get; set; }
}
