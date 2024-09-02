using DocGen.DocIO.Rendering;

namespace DocGen.Layouting;

internal interface IStringWidget : ISplitLeafWidget, ILeafWidget, IWidget, ITextMeasurable
{
	int OffsetToIndex(DrawingContext dc, double offset, string text, float clientWidth, float clientActiveAreaWidth, bool isSplitByCharacter);

	double GetTextAscent(DrawingContext dc, ref float exceededLineAscent);
}
