using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedScriptWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_superscript;

	private LayoutedOMathWidget m_subscript;

	private LayoutedOMathWidget m_equation;

	internal LayoutedOMathWidget Superscript
	{
		get
		{
			return m_superscript;
		}
		set
		{
			m_superscript = value;
		}
	}

	internal LayoutedOMathWidget Subscript
	{
		get
		{
			return m_subscript;
		}
		set
		{
			m_subscript = value;
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

	internal LayoutedScriptWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedScriptWidget(LayoutedScriptWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.Superscript != null)
		{
			Superscript = new LayoutedOMathWidget(srcWidget.Superscript);
		}
		if (srcWidget.Subscript != null)
		{
			Subscript = new LayoutedOMathWidget(srcWidget.Subscript);
		}
		Equation = new LayoutedOMathWidget(srcWidget.Equation);
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
		switch (base.Widget.Type)
		{
		case MathFunctionType.SubSuperscript:
			if ((base.Widget as IOfficeMathScript).ScriptType == MathScriptType.Superscript)
			{
				Superscript.ShiftXYPosition(xPosition, yPosition);
			}
			else
			{
				Subscript.ShiftXYPosition(xPosition, yPosition);
			}
			break;
		case MathFunctionType.LeftSubSuperscript:
		case MathFunctionType.RightSubSuperscript:
			Superscript.ShiftXYPosition(xPosition, yPosition);
			Subscript.ShiftXYPosition(xPosition, yPosition);
			break;
		}
	}

	public override void Dispose()
	{
		if (Superscript != null)
		{
			Superscript.Dispose();
			Superscript = null;
		}
		if (Subscript != null)
		{
			Subscript.Dispose();
			Subscript = null;
		}
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		base.Dispose();
	}
}
