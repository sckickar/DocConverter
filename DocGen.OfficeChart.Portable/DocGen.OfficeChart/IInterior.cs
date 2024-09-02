using DocGen.Drawing;

namespace DocGen.OfficeChart;

public interface IInterior
{
	OfficeKnownColors PatternColorIndex { get; set; }

	Color PatternColor { get; set; }

	OfficeKnownColors ColorIndex { get; set; }

	Color Color { get; set; }

	IGradient Gradient { get; }

	OfficePattern FillPattern { get; set; }
}
