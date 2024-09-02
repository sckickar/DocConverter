using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IOfficeChartInterior
{
	Color ForegroundColor { get; set; }

	Color BackgroundColor { get; set; }

	OfficePattern Pattern { get; set; }

	OfficeKnownColors ForegroundColorIndex { get; set; }

	OfficeKnownColors BackgroundColorIndex { get; set; }

	bool UseAutomaticFormat { get; set; }

	bool SwapColorsOnNegative { get; set; }
}
