using System.IO;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal interface IChartAreaHost
{
	Rectangle Bounds { get; }

	ChartColumnDrawMode ColumnDrawMode { get; set; }

	ChartColumnWidthMode ColumnWidthMode { get; set; }

	int ColumnFixedWidth { get; set; }

	bool CompatibleSeries { get; set; }

	int ElementsSpacing { get; set; }

	bool Indexed { get; set; }

	bool AllowGapForEmptyPoints { get; set; }

	ChartIndexedValues IndexValues { get; }

	ChartDock LegendPosition { get; set; }

	ChartModel Model { get; set; }

	bool Radar { get; }

	bool Bar { get; }

	bool Polar { get; }

	ChartRadarAxisStyle RadarStyle { get; set; }

	bool RequireAxes { get; set; }

	bool RequireInvertedAxes { get; set; }

	ChartSeriesCollection Series { get; }

	bool Series3D { get; set; }

	bool Style3D { get; set; }

	SmoothingMode SmoothingMode { get; set; }

	TextRenderingHint TextRenderingHint { get; set; }

	float Spacing { get; set; }

	float SpacingBetweenSeries { get; set; }

	bool RealMode3D { get; set; }

	bool DropSeriesPoints { get; set; }

	bool ShowLegend { get; set; }

	BrushInfo ChartInterior { get; set; }

	BrushInfo BackInterior { get; set; }

	Font Font { get; set; }

	Color ForeColor { get; set; }

	void Draw(DocGen.Drawing.Image img);

	void Draw(Graphics g, Size sz);

	void Draw(DocGen.Drawing.Image img, Size sz);

	void SaveImage(Stream stream);

	void SeriesChanged(object sender, ChartSeriesCollectionChangedEventArgs e);

	IChartArea GetChartArea();
}
