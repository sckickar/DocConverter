using System.Collections;

namespace DocGen.OfficeChart;

public interface IOfficeChart
{
	OfficeChartType ChartType { get; set; }

	IOfficeDataRange DataRange { get; set; }

	bool IsSeriesInRows { get; set; }

	string ChartTitle { get; set; }

	IOfficeChartTextArea ChartTitleArea { get; }

	double XPos { get; set; }

	double YPos { get; set; }

	double Width { get; set; }

	double Height { get; set; }

	IOfficeChartSeries Series { get; }

	IOfficeChartCategoryAxis PrimaryCategoryAxis { get; }

	IOfficeChartValueAxis PrimaryValueAxis { get; }

	IOfficeChartSeriesAxis PrimarySerieAxis { get; }

	IOfficeChartCategoryAxis SecondaryCategoryAxis { get; }

	IOfficeChartValueAxis SecondaryValueAxis { get; }

	IOfficeChartFrameFormat ChartArea { get; }

	IOfficeChartFrameFormat PlotArea { get; }

	IOfficeChartWallOrFloor Walls { get; }

	IOfficeChartWallOrFloor SideWall { get; }

	IOfficeChartWallOrFloor BackWall { get; }

	IOfficeChartWallOrFloor Floor { get; }

	IOfficeChartDataTable DataTable { get; }

	bool HasDataTable { get; set; }

	IOfficeChartLegend Legend { get; }

	bool HasLegend { get; set; }

	bool HasTitle { get; set; }

	int Rotation { get; set; }

	int Elevation { get; set; }

	int Perspective { get; set; }

	int HeightPercent { get; set; }

	int DepthPercent { get; set; }

	int GapDepth { get; set; }

	bool RightAngleAxes { get; set; }

	bool AutoScaling { get; set; }

	bool WallsAndGridlines2D { get; set; }

	bool HasPlotArea { get; set; }

	OfficeChartPlotEmpty DisplayBlanksAs { get; set; }

	bool PlotVisibleOnly { get; set; }

	bool SizeWithWindow { get; set; }

	IOfficeChartCategories Categories { get; }

	OfficeSeriesNameLevel SeriesNameLevel { get; set; }

	OfficeCategoriesLabelLevel CategoryLabelLevel { get; set; }

	int Style { get; set; }

	IOfficeChartData ChartData { get; }

	void Refresh();

	void SetChartData(object[][] data);

	void SetDataRange(object[][] data, int rowIndex, int columnIndex);

	void SetDataRange(IEnumerable enumerable, int rowIndex, int columnIndex);
}
