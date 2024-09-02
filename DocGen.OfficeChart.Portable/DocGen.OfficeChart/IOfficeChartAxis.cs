namespace DocGen.OfficeChart;

public interface IOfficeChartAxis
{
	string NumberFormat { get; set; }

	OfficeAxisType AxisType { get; }

	string Title { get; set; }

	int TextRotationAngle { get; set; }

	IOfficeChartTextArea TitleArea { get; }

	IOfficeFont Font { get; }

	IOfficeChartGridLine MajorGridLines { get; }

	IOfficeChartGridLine MinorGridLines { get; }

	bool HasMinorGridLines { get; set; }

	bool HasMajorGridLines { get; set; }

	OfficeTickMark MinorTickMark { get; set; }

	OfficeTickMark MajorTickMark { get; set; }

	IOfficeChartBorder Border { get; }

	bool AutoTickLabelSpacing { get; set; }

	OfficeTickLabelPosition TickLabelPosition { get; set; }

	bool Visible { get; set; }

	bool ReversePlotOrder { get; set; }

	IShadow Shadow { get; }

	IThreeDFormat Chart3DOptions { get; }
}
