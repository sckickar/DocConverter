using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedBoxWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_equation;

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

	internal LayoutedBoxWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedBoxWidget(LayoutedBoxWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Equation != null)
		{
			Equation = new LayoutedOMathWidget(srcWidget.Equation);
		}
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
	}
}
