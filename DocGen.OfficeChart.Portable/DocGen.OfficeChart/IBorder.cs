using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IBorder : IParentApplication
{
	OfficeKnownColors Color { get; set; }

	ChartColor ColorObject { get; }

	Color ColorRGB { get; set; }

	OfficeLineStyle LineStyle { get; set; }

	bool ShowDiagonalLine { get; set; }
}
