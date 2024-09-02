using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal interface IChartArea
{
	Color BorderColor { get; set; }

	int Width { get; set; }

	int Height { get; set; }

	Rectangle Bounds { get; set; }

	Rectangle ClientRectangle { get; }

	float OffsetX { get; }

	float OffsetY { get; }

	Size Size { get; set; }

	Point Location { get; set; }

	bool Series3D { get; set; }

	bool RealSeries3D { get; set; }

	float Depth { get; set; }

	float Rotation { get; set; }

	float Tilt { get; set; }

	float Turn { get; set; }

	bool AutoScale { get; set; }

	BrushInfo BackInterior { get; set; }

	DocGen.Drawing.Image BackImage { get; set; }

	bool DivideArea { get; set; }

	bool MultiplePies { get; set; }

	bool RequireAxes { get; set; }

	bool LegacyAppearance { get; set; }

	bool RequireInvertedAxes { get; set; }

	ChartAxisCollection Axes { get; }

	SizeF AxisSpacing { get; set; }

	ChartAxis PrimaryXAxis { get; }

	ChartAxis PrimaryYAxis { get; }

	SizeF MinSize { get; set; }

	float Scale3DCoeficient { get; set; }

	ChartMargins ChartAreaMargins { get; set; }

	PointF Center { get; }

	float Radius { get; }

	Rectangle RenderBounds { get; }

	ChartMargins ChartPlotAreaMargins { get; set; }

	ChartSetMode AdjustPlotAreaMargins { get; set; }

	ChartAxesInfoBar AxesInfoBar { get; }

	ChartCustomPointCollection CustomPoints { get; }

	Transform3D Transform3D { get; }

	double FullStackMax { get; set; }

	IChartAreaHost Chart { get; }

	ChartAxesLayoutMode XAxesLayoutMode { get; set; }

	ChartAxesLayoutMode YAxesLayoutMode { get; set; }

	ChartSeriesParameters SeriesParameters { get; }

	void Draw(PaintEventArgs e);

	void Draw(PaintEventArgs e, ChartPaintFlags flags);

	void CalculateSizes(Rectangle rect, Graphics g);

	ChartPoint GetValueByPoint(Point pt);

	Point GetPointByValue(ChartPoint cpt);

	Point CorrectionFrom(Point pt);

	PointF CorrectionFrom(PointF pt);

	Point CorrectionTo(Point pt);

	PointF CorrectionTo(PointF pt);

	ChartAxis GetXAxis(ChartSeries ser);

	ChartAxis GetYAxis(ChartSeries ser);
}
