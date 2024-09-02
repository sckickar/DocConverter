namespace DocGen.OfficeChart;

public interface IOfficeChartDataTable
{
	bool HasHorzBorder { get; set; }

	bool HasVertBorder { get; set; }

	bool HasBorders { get; set; }

	bool ShowSeriesKeys { get; set; }

	IOfficeChartTextArea TextArea { get; }
}
