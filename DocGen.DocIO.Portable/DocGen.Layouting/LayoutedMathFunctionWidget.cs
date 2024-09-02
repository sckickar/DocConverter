using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedMathFunctionWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_functionName;

	private LayoutedOMathWidget m_equation;

	internal LayoutedOMathWidget FunctionName
	{
		get
		{
			return m_functionName;
		}
		set
		{
			m_functionName = value;
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

	internal LayoutedMathFunctionWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedMathFunctionWidget(LayoutedMathFunctionWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.FunctionName != null)
		{
			FunctionName = new LayoutedOMathWidget(srcWidget.FunctionName);
		}
		if (srcWidget.Equation != null)
		{
			Equation = new LayoutedOMathWidget(srcWidget.Equation);
		}
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		FunctionName.ShiftXYPosition(xPosition, yPosition);
		Equation.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (FunctionName != null)
		{
			FunctionName.Dispose();
			FunctionName = null;
		}
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		base.Dispose();
	}
}
