using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedNArrayWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_superscript;

	private LayoutedOMathWidget m_subscript;

	private LayoutedOMathWidget m_equation;

	private LayoutedStringWidget m_narrayCharacter;

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

	internal LayoutedStringWidget NArrayCharacter
	{
		get
		{
			return m_narrayCharacter;
		}
		set
		{
			m_narrayCharacter = value;
		}
	}

	internal LayoutedNArrayWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedNArrayWidget(LayoutedNArrayWidget srcWidget)
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
		NArrayCharacter = new LayoutedStringWidget(srcWidget.NArrayCharacter);
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		IOfficeMathNArray obj = base.Widget as IOfficeMathNArray;
		if (!obj.HideUpperLimit)
		{
			Superscript.ShiftXYPosition(xPosition, yPosition);
		}
		if (!obj.HideLowerLimit)
		{
			Subscript.ShiftXYPosition(xPosition, yPosition);
		}
		Equation.ShiftXYPosition(xPosition, yPosition);
		NArrayCharacter.ShiftXYPosition(xPosition, yPosition);
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
		if (NArrayCharacter != null)
		{
			NArrayCharacter.Dispose();
			NArrayCharacter = null;
		}
		base.Dispose();
	}
}
