using DocGen.DocIO.Rendering;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal interface ILeafWidget : IWidget
{
	SizeF Measure(DrawingContext dc);
}
