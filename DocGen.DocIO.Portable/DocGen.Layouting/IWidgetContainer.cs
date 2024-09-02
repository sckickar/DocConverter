using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal interface IWidgetContainer : IWidget
{
	int Count { get; }

	IWidget this[int index] { get; }

	EntityCollection WidgetInnerCollection { get; }
}
