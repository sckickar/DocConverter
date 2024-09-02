using System;

namespace DocGen.Pdf.Graphics;

[Flags]
public enum PdfFontStyle
{
	Regular = 0,
	Bold = 1,
	Italic = 2,
	Underline = 4,
	Strikeout = 8
}
