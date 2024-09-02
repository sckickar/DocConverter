using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public abstract class WidgetBase : Entity, IWidget
{
	internal ILayoutInfo m_layoutInfo;

	ILayoutInfo IWidget.LayoutInfo
	{
		get
		{
			if (m_layoutInfo == null)
			{
				CreateLayoutInfo();
			}
			return m_layoutInfo;
		}
	}

	public WidgetBase(WordDocument doc, Entity owner)
		: base(doc, owner)
	{
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	protected abstract void CreateLayoutInfo();
}
