using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IOfficeChartBorder
{
	Color LineColor { get; set; }

	OfficeChartLinePattern LinePattern { get; set; }

	OfficeChartLineWeight LineWeight { get; set; }

	bool AutoFormat { get; set; }

	bool IsAutoLineColor { get; set; }

	OfficeKnownColors ColorIndex { get; set; }

	double Transparency { get; set; }

	OfficeArrowType BeginArrowType { get; set; }

	OfficeArrowType EndArrowType { get; set; }

	OfficeArrowSize BeginArrowSize { get; set; }

	OfficeArrowSize EndArrowSize { get; set; }
}
