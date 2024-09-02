using System;

namespace DocGen.Office;

[Flags]
internal enum TTFFontStyle
{
	Regular = 0,
	Bold = 1,
	Italic = 2,
	Underline = 4,
	Strikeout = 8
}
