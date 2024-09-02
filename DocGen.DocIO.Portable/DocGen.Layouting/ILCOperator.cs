using DocGen.DocIO.Rendering;

namespace DocGen.Layouting;

internal interface ILCOperator
{
	DrawingContext DrawingContext { get; }

	void SendLeafLayoutAfter(LayoutedWidget ltWidget, bool isFromTOCLinkStyle);
}
