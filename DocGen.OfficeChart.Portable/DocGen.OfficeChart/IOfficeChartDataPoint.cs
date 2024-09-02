namespace DocGen.OfficeChart;

public interface IOfficeChartDataPoint : IParentApplication
{
	IOfficeChartDataLabels DataLabels { get; }

	int Index { get; }

	IOfficeChartSerieDataFormat DataFormat { get; }

	bool IsDefault { get; }

	bool IsDefaultmarkertype { get; set; }

	bool SetAsTotal { get; set; }
}
