using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IOfficeChartSerieDataFormat : IOfficeChartFillBorder
{
	IOfficeChartInterior AreaProperties { get; }

	OfficeBaseFormat BarShapeBase { get; set; }

	OfficeTopFormat BarShapeTop { get; set; }

	Color MarkerBackgroundColor { get; set; }

	Color MarkerForegroundColor { get; set; }

	OfficeChartMarkerType MarkerStyle { get; set; }

	OfficeKnownColors MarkerForegroundColorIndex { get; set; }

	OfficeKnownColors MarkerBackgroundColorIndex { get; set; }

	int MarkerSize { get; set; }

	bool IsAutoMarker { get; set; }

	int Percent { get; set; }

	bool Is3DBubbles { get; set; }

	IOfficeChartFormat CommonSerieOptions { get; }

	bool IsMarkerSupported { get; }

	TreeMapLabelOption TreeMapLabelOption { get; set; }

	bool ShowConnectorLines { get; set; }

	bool IsSmoothedLine { get; set; }

	bool ShowMeanLine { get; set; }

	bool ShowMeanMarkers { get; set; }

	bool ShowInnerPoints { get; set; }

	bool ShowOutlierPoints { get; set; }

	QuartileCalculation QuartileCalculationType { get; set; }
}
