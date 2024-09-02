using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedPhantomWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_equation;

	private bool m_show;

	internal bool Show
	{
		get
		{
			return m_show;
		}
		set
		{
			m_show = value;
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

	internal LayoutedPhantomWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedPhantomWidget(LayoutedPhantomWidget srcWidget)
		: base(srcWidget)
	{
		Show = srcWidget.Show;
		Equation = new LayoutedOMathWidget(srcWidget.Equation);
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
		base.Dispose();
	}
}
