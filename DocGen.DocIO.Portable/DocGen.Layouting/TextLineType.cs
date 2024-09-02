using System;

namespace DocGen.Layouting;

[Flags]
internal enum TextLineType
{
	None = 0,
	NewLineBreak = 1,
	LayoutBreak = 2,
	FirstParagraphLine = 4,
	LastParagraphLine = 8
}
