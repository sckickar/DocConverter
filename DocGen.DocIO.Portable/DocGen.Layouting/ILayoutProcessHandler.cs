using DocGen.Drawing;

namespace DocGen.Layouting;

internal interface ILayoutProcessHandler
{
	bool GetNextArea(out RectangleF rect, ref int columnIndex, ref bool isContinuousSection, bool isSplittedWidget, ref float topMargin, bool isFromDynmicLayout, ref IWidgetContainer curWidget);

	void PushLayoutedWidget(LayoutedWidget ltWidget, RectangleF layoutArea, bool isNeedToRestartFootnote, bool m_bisNeedToRestartEndnoteID, LayoutState state, bool isNeedToFindInterSectingPoint, bool isContinuousSection);

	bool HandleSplittedWidget(SplitWidgetContainer stWidgetContainer, LayoutState state, LayoutedWidget ltWidget, ref bool isLayoutedWidgetNeedToPushed);

	void HandleLayoutedWidget(LayoutedWidget ltWidget);
}
