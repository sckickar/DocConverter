using System;

namespace DocGen.Pdf.Native;

[Flags]
internal enum ETO
{
	OPAQUE = 2,
	CLIPPED = 4,
	GLYPH_INDEX = 0x10,
	NUMERICSLATIN = 0x800,
	NUMERICSLOCAL = 0x400,
	RTLREADING = 0x80,
	IGNORELANGUAGE = 0x1000,
	PDY = 0x2000
}
