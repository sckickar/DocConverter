using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IGradient
{
	ChartColor BackColorObject { get; }

	Color BackColor { get; set; }

	OfficeKnownColors BackColorIndex { get; set; }

	ChartColor ForeColorObject { get; }

	Color ForeColor { get; set; }

	OfficeKnownColors ForeColorIndex { get; set; }

	OfficeGradientStyle GradientStyle { get; set; }

	OfficeGradientVariants GradientVariant { get; set; }

	int CompareTo(IGradient gradient);

	void TwoColorGradient();

	void TwoColorGradient(OfficeGradientStyle style, OfficeGradientVariants variant);
}
