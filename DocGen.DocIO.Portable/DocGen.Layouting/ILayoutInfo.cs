using DocGen.Drawing;

namespace DocGen.Layouting;

internal interface ILayoutInfo
{
	bool IsClipped { get; set; }

	bool IsSkip { get; set; }

	bool IsSkipBottomAlign { get; set; }

	bool IsLineContainer { get; }

	ChildrenLayoutDirection ChildrenLayoutDirection { get; }

	bool IsLineBreak { get; set; }

	bool TextWrap { get; set; }

	bool IsPageBreakItem { get; set; }

	bool IsVerticalText { get; set; }

	bool IsFirstItemInPage { get; set; }

	bool IsKeepWithNext { get; set; }

	bool IsHiddenRow { get; set; }

	SizeF Size { get; set; }

	SyncFont Font { get; set; }
}
