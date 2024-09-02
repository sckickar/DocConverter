using System;

namespace DocGen.CompoundFile.DocIO.Net;

[CLSCompliant(false)]
internal enum PRSPEC : uint
{
	PRSPEC_INVALID = uint.MaxValue,
	PRSPEC_LPWSTR = 0u,
	PRSPEC_PROPID = 1u
}
