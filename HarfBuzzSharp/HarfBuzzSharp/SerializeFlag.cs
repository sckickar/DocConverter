using System;

namespace HarfBuzzSharp;

[Flags]
public enum SerializeFlag
{
	Default = 0,
	NoClusters = 1,
	NoPositions = 2,
	NoGlyphNames = 4,
	GlyphExtents = 8,
	GlyphFlags = 0x10,
	NoAdvances = 0x20
}
