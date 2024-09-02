using System;

namespace DocGen.Pdf.Graphics;

[Flags]
public enum LineType
{
	None = 0,
	NewLineBreak = 1,
	LayoutBreak = 2,
	FirstParagraphLine = 4,
	LastParagraphLine = 8
}
