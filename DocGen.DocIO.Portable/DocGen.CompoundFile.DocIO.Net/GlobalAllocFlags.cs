using System;

namespace DocGen.CompoundFile.DocIO.Net;

[Flags]
internal enum GlobalAllocFlags
{
	GMEM_FIXED = 0,
	GMEM_MOVEABLE = 2,
	GMEM_ZEROINIT = 0x40,
	GMEM_NODISCARD = 0x20
}
