using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedDelimiterWidget : LayoutedFuntionWidget
{
	private LayoutedStringWidget m_beginChar;

	private LayoutedStringWidget m_endChar;

	private LayoutedStringWidget m_seperator;

	private List<LayoutedOMathWidget> m_equation;

	internal LayoutedStringWidget BeginCharacter
	{
		get
		{
			return m_beginChar;
		}
		set
		{
			m_beginChar = value;
		}
	}

	internal LayoutedStringWidget EndCharacter
	{
		get
		{
			return m_endChar;
		}
		set
		{
			m_endChar = value;
		}
	}

	internal LayoutedStringWidget Seperator
	{
		get
		{
			return m_seperator;
		}
		set
		{
			m_seperator = value;
		}
	}

	internal List<LayoutedOMathWidget> Equation
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

	internal LayoutedDelimiterWidget(IOfficeMathFunctionBase widget)
		: base(widget)
	{
	}

	internal LayoutedDelimiterWidget(LayoutedDelimiterWidget srcWidget)
		: base(srcWidget)
	{
		if (srcWidget.BeginCharacter != null)
		{
			BeginCharacter = new LayoutedStringWidget(srcWidget.BeginCharacter);
		}
		if (srcWidget.EndCharacter != null)
		{
			EndCharacter = new LayoutedStringWidget(srcWidget.EndCharacter);
		}
		if (srcWidget.Seperator != null)
		{
			Seperator = new LayoutedStringWidget(srcWidget.Seperator);
		}
		List<LayoutedOMathWidget> list = new List<LayoutedOMathWidget>();
		foreach (LayoutedOMathWidget item in srcWidget.Equation)
		{
			list.Add(new LayoutedOMathWidget(item));
		}
		Equation = list;
	}

	public override void ShiftXYPosition(float xPosition, float yPosition)
	{
		base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		if (BeginCharacter != null)
		{
			BeginCharacter.ShiftXYPosition(xPosition, yPosition);
		}
		if (EndCharacter != null)
		{
			EndCharacter.ShiftXYPosition(xPosition, yPosition);
		}
		if (Seperator != null)
		{
			Seperator.ShiftXYPosition(xPosition, yPosition);
		}
		foreach (LayoutedOMathWidget item in Equation)
		{
			item.ShiftXYPosition(xPosition, yPosition);
		}
	}

	public override void Dispose()
	{
		if (BeginCharacter != null)
		{
			BeginCharacter.Dispose();
			BeginCharacter = null;
		}
		if (EndCharacter != null)
		{
			EndCharacter.Dispose();
			EndCharacter = null;
		}
		if (Seperator != null)
		{
			Seperator.Dispose();
			Seperator = null;
		}
		if (Equation != null)
		{
			for (int i = 0; i < Equation.Count; i++)
			{
				Equation[i].Dispose();
				Equation[i] = null;
			}
			Equation.Clear();
			Equation = null;
		}
		base.Dispose();
	}
}
