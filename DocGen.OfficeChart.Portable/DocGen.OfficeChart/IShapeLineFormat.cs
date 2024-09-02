using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IShapeLineFormat
{
	double Weight { get; set; }

	Color ForeColor { get; set; }

	Color BackColor { get; set; }

	OfficeKnownColors ForeColorIndex { get; set; }

	OfficeKnownColors BackColorIndex { get; set; }

	OfficeShapeArrowStyle BeginArrowHeadStyle { get; set; }

	OfficeShapeArrowStyle EndArrowHeadStyle { get; set; }

	OfficeShapeArrowLength BeginArrowheadLength { get; set; }

	OfficeShapeArrowLength EndArrowheadLength { get; set; }

	OfficeShapeArrowWidth BeginArrowheadWidth { get; set; }

	OfficeShapeArrowWidth EndArrowheadWidth { get; set; }

	OfficeShapeDashLineStyle DashStyle { get; set; }

	OfficeShapeLineStyle Style { get; set; }

	double Transparency { get; set; }

	bool Visible { get; set; }

	OfficeGradientPattern Pattern { get; set; }

	bool HasPattern { get; set; }
}
