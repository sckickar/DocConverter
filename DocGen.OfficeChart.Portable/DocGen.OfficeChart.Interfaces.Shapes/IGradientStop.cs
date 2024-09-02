using DocGen.Drawing;

namespace DocGen.OfficeChart.Interfaces.Shapes;

internal interface IGradientStop
{
	Color Color { get; set; }

	int Position { get; set; }

	int Transparency { get; set; }
}
