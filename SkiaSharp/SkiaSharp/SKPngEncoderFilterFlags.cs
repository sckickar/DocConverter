using System;

namespace SkiaSharp;

[Flags]
public enum SKPngEncoderFilterFlags
{
	NoFilters = 0,
	None = 8,
	Sub = 0x10,
	Up = 0x20,
	Avg = 0x40,
	Paeth = 0x80,
	AllFilters = 0xF8
}
