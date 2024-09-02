namespace DocGen.OfficeChart;

public interface IOfficeChartLegend
{
	IOfficeChartFrameFormat FrameFormat { get; }

	IOfficeChartTextArea TextArea { get; }

	int X { get; set; }

	int Y { get; set; }

	OfficeLegendPosition Position { get; set; }

	bool IsVerticalLegend { get; set; }

	IChartLegendEntries LegendEntries { get; }

	bool IncludeInLayout { get; set; }

	IOfficeChartLayout Layout { get; set; }

	void Clear();

	void Delete();
}
