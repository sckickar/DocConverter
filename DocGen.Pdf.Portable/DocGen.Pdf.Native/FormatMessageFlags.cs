using System;

namespace DocGen.Pdf.Native;

[Flags]
internal enum FormatMessageFlags
{
	AllocateBuffer = 0x100,
	IgnoreInserts = 0x200,
	FromString = 0x400,
	FromHmodule = 0x800,
	FromSystem = 0x1000,
	ArgumentArray = 0x2000
}
