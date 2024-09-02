using System;

namespace BitMiracle.LibTiff.Classic;

[Flags]
public enum TiffPrintFlags
{
	NONE = 0,
	STRIPS = 1,
	CURVES = 2,
	COLORMAP = 4,
	JPEGQTABLES = 0x100,
	JPEGACTABLES = 0x200,
	JPEGDCTABLES = 0x200
}
