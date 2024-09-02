using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Interfaces;

internal interface IInternalFill : IOfficeFill
{
	ChartColor BackColorObject { get; }

	ChartColor ForeColorObject { get; }

	bool Tile { get; set; }

	GradientStops PreservedGradient { get; set; }

	bool IsGradientSupported { get; set; }

	new float TransparencyColor { get; set; }

	new float TextureVerticalScale { get; set; }

	new float TextureHorizontalScale { get; set; }

	new float TextureOffsetX { get; set; }

	new float TextureOffsetY { get; set; }

	string Alignment { get; set; }

	string TileFlipping { get; set; }
}
