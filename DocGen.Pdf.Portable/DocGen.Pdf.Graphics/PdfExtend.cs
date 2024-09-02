using System;

namespace DocGen.Pdf.Graphics;

[Flags]
public enum PdfExtend
{
	None = 0,
	Start = 1,
	End = 2,
	Both = 3
}
