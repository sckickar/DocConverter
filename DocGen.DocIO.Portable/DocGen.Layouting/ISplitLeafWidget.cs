using DocGen.DocIO.Rendering;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal interface ISplitLeafWidget : ILeafWidget, IWidget
{
	ISplitLeafWidget[] SplitBySize(DrawingContext dc, SizeF size, float clientWidth, float clientActiveAreaWidth, ref bool isLastWordFit, bool isTabStopInterSectingfloattingItem, bool isSplitByCharacter, bool isFirstItemInLine, ref int countForConsecutivelimit, Layouter layouter, ref bool isHyphenated);
}
