namespace DocGen.Layouting;

internal interface IWidget
{
	ILayoutInfo LayoutInfo { get; }

	void InitLayoutInfo();

	void InitLayoutInfo(IWidget widget);
}
