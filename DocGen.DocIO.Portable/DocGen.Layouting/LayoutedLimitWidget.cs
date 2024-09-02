using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedLimitWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_limit;

	private LayoutedOMathWidget m_equation;

	internal LayoutedOMathWidget Limit
	{
		get
		{
			return m_limit;
		}
		set
		{
			m_limit = value;
		}
	}

	internal LayoutedOMathWidget Equation
	{
		get
		{
			return m_equation;
		}
		set
		{
			m_equation = value;
		}
	}

	internal LayoutedLimitWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedLimitWidget(LayoutedLimitWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Limit != null)
		{
			Limit = new LayoutedOMathWidget(srcWidget.Limit);
		}
		if (srcWidget.Equation != null)
		{
			Equation = new LayoutedOMathWidget(srcWidget.Equation);
		}
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
		Limit.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (Limit != null)
		{
			Limit.Dispose();
			Limit = null;
		}
		base.Dispose();
	}
}
