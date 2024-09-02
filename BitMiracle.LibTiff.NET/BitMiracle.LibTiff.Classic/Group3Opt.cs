using System;

namespace BitMiracle.LibTiff.Classic;

[Flags]
public enum Group3Opt
{
	UNKNOWN = -1,
	ENCODING2D = 1,
	UNCOMPRESSED = 2,
	FILLBITS = 4
}
