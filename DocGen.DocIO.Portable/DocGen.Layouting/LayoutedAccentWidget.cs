using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedAccentWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_equation;

	private LayoutedStringWidget m_accentCharacter;

	private float m_scalingFactor = 1f;

	internal LayoutedStringWidget AccentCharacter
	{
		get
		{
			return m_accentCharacter;
		}
		set
		{
			m_accentCharacter = value;
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

	internal float ScalingFactor
	{
		get
		{
			return m_scalingFactor;
		}
		set
		{
			m_scalingFactor = value;
		}
	}

	internal LayoutedAccentWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedAccentWidget(LayoutedAccentWidget srcWidget)
		: base(srcWidget)
	{
		AccentCharacter = new LayoutedStringWidget(srcWidget.AccentCharacter);
		Equation = new LayoutedOMathWidget(srcWidget.Equation);
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		AccentCharacter.Bounds = new RectangleF(AccentCharacter.Bounds.X + xPosition, AccentCharacter.Bounds.Y + yPosition, AccentCharacter.Bounds.Width, AccentCharacter.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (AccentCharacter != null)
		{
			AccentCharacter.Dispose();
			AccentCharacter = null;
		}
		base.Dispose();
	}
}
