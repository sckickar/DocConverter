using System;

namespace DocGen.Pdf.Graphics.Images.Metafiles;

[Flags]
internal enum PenFlags
{
	Default = 0,
	Transform = 1,
	StartCap = 2,
	EndCap = 4,
	LineJoin = 8,
	MiterLimit = 0x10,
	DashStyle = 0x20,
	DashCap = 0x40,
	DashOffset = 0x80,
	DashPattern = 0x100,
	Alignment = 0x200,
	CompoundArray = 0x400,
	CustomStartCap = 0x800,
	CustomEndCap = 0x1000
}
