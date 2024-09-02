namespace DocGen.OfficeChart;

public interface IOfficeChartFormat
{
	bool IsVaryColor { get; set; }

	int Overlap { get; set; }

	int GapWidth { get; set; }

	int FirstSliceAngle { get; set; }

	int DoughnutHoleSize { get; set; }

	int BubbleScale { get; set; }

	ChartBubbleSize SizeRepresents { get; set; }

	bool ShowNegativeBubbles { get; set; }

	bool HasRadarAxisLabels { get; set; }

	OfficeSplitType SplitType { get; set; }

	int SplitValue { get; set; }

	int PieSecondSize { get; set; }

	IOfficeChartDropBar FirstDropBar { get; }

	IOfficeChartDropBar SecondDropBar { get; }

	IOfficeChartBorder PieSeriesLine { get; }

	ExcelDropLineStyle DropLineStyle { get; set; }

	bool HasDropLines { get; set; }

	bool HasHighLowLines { get; set; }

	bool HasSeriesLines { get; set; }

	IOfficeChartBorder DropLines { get; }

	IOfficeChartBorder HighLowLines { get; }
}
