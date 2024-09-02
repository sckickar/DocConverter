using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedFractionWidget : LayoutedFuntionWidget
{
	private LayoutedOMathWidget m_numerator;

	private LayoutedOMathWidget m_denominator;

	private LayoutedLineWidget m_fractionLine;

	internal LayoutedOMathWidget Numerator
	{
		get
		{
			return m_numerator;
		}
		set
		{
			m_numerator = value;
		}
	}

	internal LayoutedOMathWidget Denominator
	{
		get
		{
			return m_denominator;
		}
		set
		{
			m_denominator = value;
		}
	}

	internal LayoutedLineWidget FractionLine
	{
		get
		{
			return m_fractionLine;
		}
		set
		{
			m_fractionLine = value;
		}
	}

	internal LayoutedFractionWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedFractionWidget(LayoutedFractionWidget srcWidget)
		: base(srcWidget)
	{
		Numerator = new LayoutedOMathWidget(srcWidget.Numerator);
		Denominator = new LayoutedOMathWidget(srcWidget.Denominator);
		FractionLine = new LayoutedLineWidget(srcWidget.FractionLine);
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		Numerator.ShiftXYPosition(xPosition, yPosition);
		Denominator.ShiftXYPosition(xPosition, yPosition);
		FractionLine.ShiftXYPosition(xPosition, yPosition);
	}

	public override void Dispose()
	{
		if (Numerator != null)
		{
			Numerator.Dispose();
			Numerator = null;
		}
		if (Denominator != null)
		{
			Denominator.Dispose();
			Denominator = null;
		}
		if (FractionLine != null)
		{
			FractionLine.Dispose();
			FractionLine = null;
		}
		base.Dispose();
	}
}
