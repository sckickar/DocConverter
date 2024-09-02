using System;

namespace DocGen.Pdf.Graphics;

[Flags]
internal enum TextRenderingMode
{
	Fill = 0,
	Stroke = 1,
	FillStroke = 2,
	None = 3,
	ClipFlag = 4,
	ClipFill = 4,
	ClipStroke = 5,
	ClipFillStroke = 6,
	Clip = 7
}
