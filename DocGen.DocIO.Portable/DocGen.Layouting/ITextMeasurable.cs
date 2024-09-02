using DocGen.DocIO.Rendering;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal interface ITextMeasurable
{
	SizeF Measure(string text);

	SizeF Measure(DrawingContext dc, string text);
}
