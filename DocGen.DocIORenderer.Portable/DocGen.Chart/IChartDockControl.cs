using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal interface IChartDockControl
{
	ChartDock Position { get; set; }

	ChartAlignment Alignment { get; set; }

	ChartOrientation Orientation { get; set; }

	ChartDockingFlags Behavior { get; set; }

	Point Location { get; set; }

	Size Size { get; set; }

	bool Visible { get; set; }

	bool Enabled { get; set; }

	event LocationEventHandler LocationChanging;

	event EventHandler ChartDockChanged;

	event EventHandler ChartAlignmentChanged;

	event EventHandler SizeChanged;

	event EventHandler LocationChanged;

	event EventHandler VisibleChanged;

	SizeF Measure(Graphics g, SizeF size);

	void Render(Graphics g);
}
