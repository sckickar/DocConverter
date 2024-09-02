using System;

namespace DocGen.CompoundFile.DocIO.Net;

[Flags]
internal enum VARTYPE
{
	VT_EMPTY = 0,
	VT_I4 = 3,
	VT_DATE = 7,
	VT_BSTR = 8,
	VT_BOOL = 0xB,
	VT_VARIANT = 0xC,
	VT_INT = 0x16,
	VT_LPSTR = 0x1E,
	VT_LPWSTR = 0x1F,
	VT_FILETIME = 0x40,
	VT_VECTOR = 0x1000
}
