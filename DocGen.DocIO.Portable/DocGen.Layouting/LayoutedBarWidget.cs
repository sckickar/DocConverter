using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedBarWidget : LayoutedFuntionWidget
{
	private LayoutedLineWidget m_barline;

	private LayoutedOMathWidget m_equation;

	internal LayoutedLineWidget BarLine
	{
		get
		{
			return m_barline;
		}
		set
		{
			m_barline = value;
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

	internal LayoutedBarWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedBarWidget(LayoutedBarWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Equation != null)
		{
			Equation = new LayoutedOMathWidget(srcWidget.Equation);
		}
		if (srcWidget.BarLine != null)
		{
			BarLine = new LayoutedLineWidget(srcWidget.BarLine);
		}
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
		BarLine.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (BarLine != null)
		{
			BarLine.Dispose();
			BarLine = null;
		}
		base.Dispose();
	}
}
