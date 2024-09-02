using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedEquationArrayWidget : LayoutedFuntionWidget
{
	private List<List<LayoutedOMathWidget>> m_equation;

	internal List<List<LayoutedOMathWidget>> Equation
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

	internal LayoutedEquationArrayWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedEquationArrayWidget(LayoutedEquationArrayWidget srcWidget)
		: base(srcWidget)
	{
		List<List<LayoutedOMathWidget>> equation = new List<List<LayoutedOMathWidget>>();
		for (int i = 0; i < srcWidget.Equation.Count; i++)
		{
			List<LayoutedOMathWidget> list = new List<LayoutedOMathWidget>();
			for (int j = 0; j < srcWidget.Equation[i].Count; j++)
			{
				list.Add(new LayoutedOMathWidget(srcWidget.Equation[i][j]));
			}
		}
		Equation = equation;
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		for (int i = 0; i < Equation.Count; i++)
		{
			for (int j = 0; j < Equation[i].Count; j++)
			{
				Equation[i][j].ShiftXYPosition(xPosition, yPosition);
			}
		}
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			for (int i = 0; i < Equation.Count; i++)
			{
				for (int j = 0; j < Equation[i].Count; j++)
				{
					Equation[i][j].Dispose();
					Equation[i][j] = null;
				}
				Equation[i].Clear();
			}
			Equation.Clear();
			Equation = null;
		}
		base.Dispose();
	}
}
