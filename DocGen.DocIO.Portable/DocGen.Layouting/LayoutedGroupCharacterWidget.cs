using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedGroupCharacterWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_equation;

	private LayoutedStringWidget m_groupCharacter;

	private float m_scalingFactor = 1f;

	internal LayoutedStringWidget GroupCharacter
	{
		get
		{
			return m_groupCharacter;
		}
		set
		{
			m_groupCharacter = value;
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

	internal LayoutedGroupCharacterWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedGroupCharacterWidget(LayoutedGroupCharacterWidget srcWidget)
		: base(srcWidget)
	{
		GroupCharacter = new LayoutedStringWidget(srcWidget.GroupCharacter);
		Equation = new LayoutedOMathWidget(srcWidget.Equation);
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		GroupCharacter.Bounds = new RectangleF(GroupCharacter.Bounds.X + xPosition, GroupCharacter.Bounds.Y + yPosition, GroupCharacter.Bounds.Width, GroupCharacter.Bounds.Height);
		Equation.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Equation != null)
		{
			Equation.Dispose();
			Equation = null;
		}
		if (GroupCharacter != null)
		{
			GroupCharacter.Dispose();
			GroupCharacter = null;
		}
		base.Dispose();
	}
}
