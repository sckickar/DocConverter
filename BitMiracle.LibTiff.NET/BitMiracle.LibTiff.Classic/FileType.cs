using System;

namespace BitMiracle.LibTiff.Classic;

[Flags]
public enum FileType
{
	REDUCEDIMAGE = 1,
	PAGE = 2,
	MASK = 4
}
