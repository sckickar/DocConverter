using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedRadicalWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_degree;

	private LayoutedOMathWidget m_equation;

	private LayoutedLineWidget[] m_radicalLines;

	internal LayoutedOMathWidget Degree
	{
		get
		{
			return m_degree;
		}
		set
		{
			m_degree = value;
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

	internal LayoutedLineWidget[] RadicalLines
	{
		get
		{
			return m_radicalLines;
		}
		set
		{
			m_radicalLines = value;
		}
	}

	internal LayoutedRadicalWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedRadicalWidget(LayoutedRadicalWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Degree != null)
		{
			Degree = new LayoutedOMathWidget(srcWidget.Degree);
		}
		Equation = new LayoutedOMathWidget(srcWidget.Equation);
		LayoutedLineWidget[] array = new LayoutedLineWidget[srcWidget.RadicalLines.Length];
		for (int i = 0; i < srcWidget.RadicalLines.Length; i++)
		{
			array[i] = new LayoutedLineWidget(srcWidget.RadicalLines[i]);
		}
		RadicalLines = array;
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
		if (Degree != null)
		{
			Degree.ShiftXYPosition(xPosition, yPosition);
		}
		LayoutedLineWidget[] radicalLines = RadicalLines;
		for (int i = 0; i < radicalLines.Length; i++)
		{
			radicalLines[i].ShiftXYPosition(xPosition, yPosition);
		}
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (Degree != null)
		{
			Degree.Dispose();
			Degree = null;
		}
		if (RadicalLines != null)
		{
			for (int i = 0; i < RadicalLines.Length; i++)
			{
				RadicalLines[i].Dispose();
				RadicalLines[i] = null;
			}
			RadicalLines = null;
		}
		base.Dispose();
	}
}
