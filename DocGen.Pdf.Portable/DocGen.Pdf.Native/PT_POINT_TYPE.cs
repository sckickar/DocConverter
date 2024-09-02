using System;

namespace DocGen.Pdf.Native;

[Flags]
internal enum PT_POINT_TYPE : byte
{
	PT_CLOSEFIGURE = 1,
	PT_LINETO = 2,
	PT_BEZIERTO = 4,
	PT_MOVETO = 6
}
