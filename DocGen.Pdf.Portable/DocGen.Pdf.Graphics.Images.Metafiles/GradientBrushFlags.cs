using System;

namespace DocGen.Pdf.Graphics.Images.Metafiles;

[Flags]
internal enum GradientBrushFlags
{
	Default = 0,
	Matrix = 2,
	ColorBlend = 4,
	Blend = 8,
	FocusScales = 0x40,
	GammaCorrection = 0x80
}
