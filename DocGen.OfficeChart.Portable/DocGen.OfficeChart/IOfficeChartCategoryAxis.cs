namespace DocGen.OfficeChart;

public interface IOfficeChartCategoryAxis : IOfficeChartValueAxis, IOfficeChartAxis
{
	int TickLabelSpacing { get; set; }

	new bool AutoTickLabelSpacing { get; set; }

	int TickMarkSpacing { get; set; }

	bool IsBetween { get; set; }

	IOfficeDataRange CategoryLabels { get; set; }

	object[] DirectCategoryLabels { get; set; }

	OfficeCategoryType CategoryType { get; set; }

	int Offset { get; set; }

	OfficeChartBaseUnit BaseUnit { get; set; }

	bool BaseUnitIsAuto { get; set; }

	OfficeChartBaseUnit MajorUnitScale { get; set; }

	OfficeChartBaseUnit MinorUnitScale { get; set; }

	bool NoMultiLevelLabel { get; set; }

	bool IsBinningByCategory { get; set; }

	bool HasAutomaticBins { get; set; }

	int NumberOfBins { get; set; }

	double BinWidth { get; set; }

	double UnderflowBinValue { get; set; }

	double OverflowBinValue { get; set; }
}
