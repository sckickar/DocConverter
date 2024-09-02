using DocGen.Drawing;

namespace DocGen.Chart;

internal interface IChartLegend
{
	string Name { get; set; }

	ChartDock Position { get; set; }

	ChartOrientation Orientation { get; set; }

	VerticalAlignment ItemsTextAligment { get; set; }

	StringAlignment ItemsAlignment { get; set; }

	ChartLegendRepresentationType RepresentationType { get; set; }

	ChartAlignment Alignment { get; set; }

	SizeF ItemsSize { get; set; }

	Size ItemsShadowOffset { get; set; }

	Color ItemsShadowColor { get; set; }

	int Spacing { get; set; }

	int RowsCount { get; set; }

	int ColumnsCount { get; set; }

	bool ShowSymbol { get; set; }

	bool OnlyColumnsForFloating { get; set; }

	bool FloatingAutoSize { get; set; }

	bool ShowItemsShadow { get; set; }

	bool SetDefSizeForCustom { get; set; }

	Font Font { get; set; }

	Color BackColor { get; set; }

	Color ForeColor { get; set; }

	event LegendFilterItemsEventHandler FilterItems;
}
