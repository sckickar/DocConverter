using System;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class SplitWidgetContainer : IWidgetContainer, IWidget
{
	private IWidgetContainer m_container;

	internal IWidget m_currentChild;

	private int m_firstIndex;

	private IWidgetContainer m_realWidgetConatiner;

	public IWidgetContainer RealWidgetContainer => m_realWidgetConatiner;

	public ILayoutInfo LayoutInfo => m_container.LayoutInfo;

	public int Count => m_container.Count - m_firstIndex;

	public IWidget this[int index]
	{
		get
		{
			if (index == 0)
			{
				return m_currentChild;
			}
			return m_container[index + m_firstIndex];
		}
	}

	public EntityCollection WidgetInnerCollection => m_container.WidgetInnerCollection;

	public SplitWidgetContainer(IWidgetContainer container)
	{
		m_container = container;
		m_currentChild = null;
		m_firstIndex = m_container.Count;
		m_realWidgetConatiner = ((container is SplitWidgetContainer) ? (container as SplitWidgetContainer).RealWidgetContainer : container);
	}

	public SplitWidgetContainer(IWidgetContainer container, IWidget currentChild, int firstIndex)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (currentChild == null)
		{
			throw new ArgumentNullException("currentChild");
		}
		if (firstIndex < 0)
		{
			throw new ArgumentOutOfRangeException("firstIndex", firstIndex, "Value can not be less 0");
		}
		m_container = container;
		m_currentChild = currentChild;
		m_firstIndex = firstIndex;
		m_realWidgetConatiner = ((container is SplitWidgetContainer) ? (container as SplitWidgetContainer).RealWidgetContainer : container);
	}

	void IWidget.InitLayoutInfo()
	{
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
		int num = WidgetInnerCollection.InnerList.IndexOf(widget as Entity);
		if (num > 0)
		{
			m_currentChild = new SplitWidgetContainer(RealWidgetContainer, widget, num);
		}
		else
		{
			m_currentChild = widget;
		}
	}
}
