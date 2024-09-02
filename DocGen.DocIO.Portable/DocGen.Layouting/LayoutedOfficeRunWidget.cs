using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedOfficeRunWidget : LayoutedWidget, ILayoutedFuntionWidget
{
	private LayoutedWidget m_ltWidget;

	private IOfficeMathFunctionBase m_widget;

	private LayoutedOMathWidget m_owner;

	public new IOfficeMathFunctionBase Widget => m_widget;

	internal LayoutedWidget LayoutedWidget
	{
		get
		{
			return m_ltWidget;
		}
		set
		{
			m_ltWidget = value;
		}
	}

	public new LayoutedOMathWidget Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
		}
	}

	internal LayoutedOfficeRunWidget(IOfficeMathFunctionBase widget)
	{
		m_widget = widget;
	}

	internal LayoutedOfficeRunWidget(ILayoutedFuntionWidget srcWidget)
		: base(srcWidget as LayoutedOfficeRunWidget)
	{
		m_widget = srcWidget.Widget;
		LayoutedWidget = new LayoutedWidget((srcWidget as LayoutedOfficeRunWidget).LayoutedWidget);
	}

	public void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		LayoutedWidget.Bounds = new RectangleF(LayoutedWidget.Bounds.X + xPosition, LayoutedWidget.Bounds.Y + yPosition, LayoutedWidget.Bounds.Width, LayoutedWidget.Bounds.Height);
	}

	public void Dispose()
	{
		if (m_ltWidget != null)
		{
			m_ltWidget.InitLayoutInfoAll();
		}
		m_widget = null;
		m_owner = null;
	}
}
