using DocGen.DocIO.Rendering;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LeafEmtyWidget : ILeafWidget, IWidget
{
	private SizeF m_size;

	private LayoutInfo m_layoutInfo;

	public ILayoutInfo LayoutInfo => m_layoutInfo;

	public LeafEmtyWidget(SizeF size)
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		m_size = size;
	}

	public SizeF Measure(DrawingContext dc)
	{
		return m_size;
	}

	public void Draw(DrawingContext dc, LayoutedWidget layoutedWidget)
	{
	}

	public void InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	public void InitLayoutInfo(IWidget widget)
	{
	}
}
