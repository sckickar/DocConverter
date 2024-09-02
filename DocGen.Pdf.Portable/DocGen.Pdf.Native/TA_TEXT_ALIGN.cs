using System;

namespace DocGen.Pdf.Native;

[Flags]
internal enum TA_TEXT_ALIGN
{
	TA_NOUPDATECP = 0,
	TA_UPDATECP = 1,
	TA_LEFT = 0,
	TA_RIGHT = 2,
	TA_CENTER = 6,
	TA_TOP = 0,
	TA_BOTTOM = 8,
	TA_BASELINE = 0x18,
	TA_RTLREADING = 0x100
}
