using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedBoderBoxWidget : LayoutedFuntionWidget
{
	private List<LayoutedLineWidget> m_borderLines;

	private LayoutedOMathWidget m_equation;

	internal List<LayoutedLineWidget> BorderLines
	{
		get
		{
			return m_borderLines;
		}
		set
		{
			m_borderLines = value;
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

	internal LayoutedBoderBoxWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedBoderBoxWidget(LayoutedBoderBoxWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Equation != null)
		{
			Equation = new LayoutedOMathWidget(srcWidget.Equation);
		}
		if (srcWidget.BorderLines != null)
		{
			BorderLines = new List<LayoutedLineWidget>(srcWidget.BorderLines);
		}
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
		foreach (LayoutedLineWidget borderLine in BorderLines)
		{
			borderLine.ShiftXYPosition(xPosition, yPosition);
		}
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (BorderLines != null)
		{
			for (int i = 0; i < BorderLines.Count; i++)
			{
				BorderLines[i].Dispose();
				BorderLines[i] = null;
			}
			BorderLines.Clear();
			BorderLines = null;
		}
		base.Dispose();
	}
}
