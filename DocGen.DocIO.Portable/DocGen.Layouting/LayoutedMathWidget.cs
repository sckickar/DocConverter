using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedMathWidget : LayoutedWidget
{
	private List<LayoutedOMathWidget> m_layoutedOMathWidgetList;

	internal new List<LayoutedOMathWidget> ChildWidgets => m_layoutedOMathWidgetList;

	internal LayoutedMathWidget(IWidget widget)
		: base(widget)
	{
		m_layoutedOMathWidgetList = new List<LayoutedOMathWidget>();
	}

	internal LayoutedMathWidget(LayoutedWidget srcWidget)
		: base(srcWidget)
	{
		m_layoutedOMathWidgetList = new List<LayoutedOMathWidget>();
		LayoutedMathWidget layoutedMathWidget = srcWidget as LayoutedMathWidget;
		for (int i = 0; i < layoutedMathWidget.ChildWidgets.Count; i++)
		{
			ChildWidgets.Add(new LayoutedOMathWidget(layoutedMathWidget.ChildWidgets[i]));
		}
	}

	internal void Dispose()
	{
		if (m_layoutedOMathWidgetList != null)
		{
			for (int i = 0; i < m_layoutedOMathWidgetList.Count; i++)
			{
				m_layoutedOMathWidgetList[i].Dispose();
				m_layoutedOMathWidgetList[i] = null;
			}
			m_layoutedOMathWidgetList.Clear();
			m_layoutedOMathWidgetList = null;
		}
	}

	internal void ShiftXYPosition(float xPosition, float yPosition, bool isSkipOwnerContainer)
	{
		if (!isSkipOwnerContainer)
		{
			base.Bounds = new RectangleF(base.Bounds.X + xPosition, base.Bounds.Y + yPosition, base.Bounds.Width, base.Bounds.Height);
		}
		foreach (LayoutedOMathWidget childWidget in ChildWidgets)
		{
			childWidget.ShiftXYPosition(xPosition, yPosition, isSkipOwnerContainer);
		}
	}

	internal void ShiftXYPosition(float xPosition, float yPosition)
	{
		ShiftXYPosition(xPosition, yPosition, isSkipOwnerContainer: false);
	}

	internal DocGen.Drawing.Font GetFont()
	{
		int index = 0;
		float num = 0f;
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			if (num < ChildWidgets[i].Bounds.Height)
			{
				num = ChildWidgets[i].Bounds.Height;
				index = i;
			}
		}
		IOfficeMathFunctionBase widget = ChildWidgets[index].ChildWidgets[0].Widget;
		WCharacterFormat wCharacterFormat = null;
		switch (widget.Type)
		{
		case MathFunctionType.Fraction:
			wCharacterFormat = (widget as IOfficeMathFraction).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Delimiter:
			wCharacterFormat = (widget as IOfficeMathDelimiter).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Phantom:
			wCharacterFormat = (widget as IOfficeMathPhantom).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Function:
			wCharacterFormat = (widget as IOfficeMathFunction).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.BorderBox:
			wCharacterFormat = (widget as IOfficeMathBorderBox).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.EquationArray:
			wCharacterFormat = (widget as IOfficeMathEquationArray).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Bar:
			wCharacterFormat = (widget as IOfficeMathBar).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Radical:
			wCharacterFormat = (widget as IOfficeMathRadical).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Matrix:
			wCharacterFormat = (widget as IOfficeMathMatrix).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.RunElement:
			if ((widget as IOfficeMathRunElement).Item is WTextRange)
			{
				wCharacterFormat = ((widget as IOfficeMathRunElement).Item as WTextRange).CharacterFormat;
			}
			else if ((widget as IOfficeMathRunElement).Item is Break)
			{
				wCharacterFormat = ((widget as IOfficeMathRunElement).Item as Break).CharacterFormat;
			}
			break;
		case MathFunctionType.SubSuperscript:
			wCharacterFormat = (widget as IOfficeMathScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.LeftSubSuperscript:
			wCharacterFormat = (widget as IOfficeMathLeftScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.RightSubSuperscript:
			wCharacterFormat = (widget as IOfficeMathRightScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.NArray:
			wCharacterFormat = (widget as IOfficeMathNArray).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Limit:
			wCharacterFormat = (widget as IOfficeMathLimit).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Box:
			wCharacterFormat = (widget as IOfficeMathBox).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Accent:
			wCharacterFormat = (widget as IOfficeMathAccent).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.GroupCharacter:
			wCharacterFormat = (widget as IOfficeMathGroupCharacter).ControlProperties as WCharacterFormat;
			break;
		}
		return wCharacterFormat.Font;
	}
}
