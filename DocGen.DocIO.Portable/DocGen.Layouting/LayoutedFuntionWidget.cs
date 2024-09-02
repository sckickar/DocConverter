using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal abstract class LayoutedFuntionWidget : ILayoutedFuntionWidget
{
	private RectangleF m_bounds = RectangleF.Empty;

	private IOfficeMathFunctionBase m_widget;

	private LayoutedOMathWidget m_owner;

	public IOfficeMathFunctionBase Widget => m_widget;

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public LayoutedOMathWidget Owner
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

	internal LayoutedFuntionWidget(IOfficeMathFunctionBase widget)
	{
		m_widget = widget;
	}

	internal LayoutedFuntionWidget(LayoutedFuntionWidget srcWidget)
	{
		Bounds = srcWidget.Bounds;
		m_widget = srcWidget.Widget;
	}

	public abstract void ShiftXYPosition(float xPosition, float yPosition);

	public virtual void Dispose()
	{
		m_owner = null;
		m_owner = null;
	}
}
