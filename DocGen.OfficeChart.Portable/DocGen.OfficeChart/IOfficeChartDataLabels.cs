namespace DocGen.OfficeChart;

public interface IOfficeChartDataLabels : IOfficeChartTextArea, IParentApplication, IOfficeFont, IOptimizedUpdate
{
	bool IsSeriesName { get; set; }

	bool IsCategoryName { get; set; }

	bool IsValue { get; set; }

	bool IsPercentage { get; set; }

	string NumberFormat { get; set; }

	bool IsBubbleSize { get; set; }

	string Delimiter { get; set; }

	bool IsLegendKey { get; set; }

	bool ShowLeaderLines { get; set; }

	OfficeDataLabelPosition Position { get; set; }

	bool IsValueFromCells { get; set; }

	IOfficeDataRange ValueFromCellsRange { get; set; }
}
