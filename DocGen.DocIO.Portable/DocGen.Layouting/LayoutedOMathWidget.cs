using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedOMathWidget
{
	private List<ILayoutedFuntionWidget> m_layoutedFucntionWidgetList;

	private RectangleF m_bounds = RectangleF.Empty;

	private IOfficeMath m_widget;

	private LayoutedMathWidget m_owner;

	internal List<ILayoutedFuntionWidget> ChildWidgets => m_layoutedFucntionWidgetList;

	internal RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	internal IOfficeMath Widget => m_widget;

	internal LayoutedMathWidget Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
		}
	}

	internal LayoutedOMathWidget(IOfficeMath widget)
	{
		m_widget = widget;
		m_layoutedFucntionWidgetList = new List<ILayoutedFuntionWidget>();
	}

	internal LayoutedOMathWidget(LayoutedOMathWidget srcWidget)
	{
		Bounds = srcWidget.Bounds;
		m_widget = srcWidget.Widget;
		m_layoutedFucntionWidgetList = new List<ILayoutedFuntionWidget>();
		for (int i = 0; i < srcWidget.ChildWidgets.Count; i++)
		{
			IOfficeMathFunctionBase widget = srcWidget.ChildWidgets[i].Widget;
			ILayoutedFuntionWidget item = null;
			switch (widget.Type)
			{
			case MathFunctionType.Fraction:
				item = new LayoutedFractionWidget(srcWidget.ChildWidgets[i] as LayoutedFractionWidget);
				break;
			case MathFunctionType.Function:
				item = new LayoutedMathFunctionWidget(srcWidget.ChildWidgets[i] as LayoutedMathFunctionWidget);
				break;
			case MathFunctionType.BorderBox:
				item = new LayoutedBoderBoxWidget(srcWidget.ChildWidgets[i] as LayoutedBoderBoxWidget);
				break;
			case MathFunctionType.EquationArray:
				item = new LayoutedEquationArrayWidget(srcWidget.ChildWidgets[i] as LayoutedEquationArrayWidget);
				break;
			case MathFunctionType.Radical:
				item = new LayoutedRadicalWidget(srcWidget.ChildWidgets[i] as LayoutedRadicalWidget);
				break;
			case MathFunctionType.Bar:
				item = new LayoutedBarWidget(srcWidget.ChildWidgets[i] as LayoutedBarWidget);
				break;
			case MathFunctionType.Matrix:
				item = new LayoutedMatrixWidget(srcWidget.ChildWidgets[i] as LayoutedMatrixWidget);
				break;
			case MathFunctionType.RunElement:
				item = new LayoutedOfficeRunWidget(srcWidget.ChildWidgets[i]);
				break;
			case MathFunctionType.LeftSubSuperscript:
			case MathFunctionType.SubSuperscript:
			case MathFunctionType.RightSubSuperscript:
				item = new LayoutedScriptWidget(srcWidget.ChildWidgets[i] as LayoutedScriptWidget);
				break;
			case MathFunctionType.Limit:
				item = new LayoutedLimitWidget(srcWidget.ChildWidgets[i] as LayoutedLimitWidget);
				break;
			case MathFunctionType.Box:
				item = new LayoutedBoxWidget(srcWidget.ChildWidgets[i] as LayoutedBoxWidget);
				break;
			case MathFunctionType.NArray:
				item = new LayoutedNArrayWidget(srcWidget.ChildWidgets[i] as LayoutedNArrayWidget);
				break;
			case MathFunctionType.Delimiter:
				item = new LayoutedDelimiterWidget(srcWidget.ChildWidgets[i] as LayoutedDelimiterWidget);
				break;
			case MathFunctionType.Accent:
				item = new LayoutedAccentWidget(srcWidget.ChildWidgets[i] as LayoutedAccentWidget);
				break;
			case MathFunctionType.Phantom:
				item = new LayoutedPhantomWidget(srcWidget.ChildWidgets[i] as LayoutedPhantomWidget);
				break;
			case MathFunctionType.GroupCharacter:
				item = new LayoutedGroupCharacterWidget(srcWidget.ChildWidgets[i] as LayoutedGroupCharacterWidget);
				break;
			}
			ChildWidgets.Add(item);
		}
	}

	internal void Dispose()
	{
		if (m_layoutedFucntionWidgetList != null)
		{
			for (int i = 0; i < m_layoutedFucntionWidgetList.Count; i++)
			{
				m_layoutedFucntionWidgetList[i].Dispose();
				m_layoutedFucntionWidgetList[i] = null;
			}
			m_layoutedFucntionWidgetList.Clear();
			m_layoutedFucntionWidgetList = null;
		}
		m_widget = null;
		m_owner = null;
	}

	internal void ShiftXYPosition(float xPosition, float yPosition, bool isSkipOwnerContainer)
	{
		if (!isSkipOwnerContainer)
		{
			Bounds = new RectangleF(Bounds.X + xPosition, Bounds.Y + yPosition, Bounds.Width, Bounds.Height);
		}
		foreach (ILayoutedFuntionWidget childWidget in ChildWidgets)
		{
			childWidget.ShiftXYPosition(xPosition, yPosition);
		}
	}

	internal void ShiftXYPosition(float xPosition, float yPosition)
	{
		ShiftXYPosition(xPosition, yPosition, isSkipOwnerContainer: false);
	}

	internal float GetVerticalCenterPoint()
	{
		int maxHeightWidgetIndex;
		return GetVerticalCenterPoint(out maxHeightWidgetIndex);
	}

	internal float GetVerticalCenterPoint(out int maxHeightWidgetIndex)
	{
		float num = 0f;
		maxHeightWidgetIndex = 0;
		if (ChildWidgets.Count == 0)
		{
			return 0f;
		}
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			ILayoutedFuntionWidget layoutedFuntionWidget = ChildWidgets[i];
			if (num < layoutedFuntionWidget.Bounds.Height)
			{
				num = layoutedFuntionWidget.Bounds.Height;
				maxHeightWidgetIndex = i;
			}
		}
		return GetVerticalCenterPoint(ChildWidgets[maxHeightWidgetIndex]);
	}

	internal float GetVerticalCenterPoint(ILayoutedFuntionWidget layoutedFuntionWidget)
	{
		float num = 0f;
		float num2 = 0f;
		int index = 0;
		switch (layoutedFuntionWidget.Widget.Type)
		{
		case MathFunctionType.Fraction:
			switch ((layoutedFuntionWidget.Widget as IOfficeMathFraction).FractionType)
			{
			case MathFractionType.NormalFractionBar:
			case MathFractionType.NoFractionBar:
				num = (layoutedFuntionWidget as LayoutedFractionWidget).FractionLine.Point1.Y + (layoutedFuntionWidget as LayoutedFractionWidget).FractionLine.Width / 2f;
				break;
			case MathFractionType.SkewedFractionBar:
			case MathFractionType.FractionInline:
				num = (layoutedFuntionWidget as LayoutedFractionWidget).FractionLine.Point1.Y + ((layoutedFuntionWidget as LayoutedFractionWidget).FractionLine.Point2.Y - (layoutedFuntionWidget as LayoutedFractionWidget).FractionLine.Point1.Y) / 2f;
				break;
			}
			break;
		case MathFunctionType.EquationArray:
		{
			IOfficeMathEquationArray officeMathEquationArray = layoutedFuntionWidget.Widget as IOfficeMathEquationArray;
			switch (officeMathEquationArray.VerticalAlignment)
			{
			case MathVerticalAlignment.Center:
				num = layoutedFuntionWidget.Bounds.Y + layoutedFuntionWidget.Bounds.Height / 2f;
				break;
			case MathVerticalAlignment.Top:
			case MathVerticalAlignment.Bottom:
			{
				List<LayoutedOMathWidget> list = null;
				list = ((officeMathEquationArray.VerticalAlignment != MathVerticalAlignment.Top) ? (layoutedFuntionWidget as LayoutedEquationArrayWidget).Equation[officeMathEquationArray.Equation.Count - 1] : (layoutedFuntionWidget as LayoutedEquationArrayWidget).Equation[0]);
				float num3 = float.MaxValue;
				for (int j = 0; j < list.Count; j++)
				{
					if (num2 < list[j].Bounds.Height)
					{
						num2 = list[j].Bounds.Height;
						index = j;
					}
					if (list[j].Bounds.Y < num3)
					{
						num3 = list[j].Bounds.Y;
					}
				}
				num = list[index].GetVerticalCenterPoint();
				num = ((officeMathEquationArray.VerticalAlignment != MathVerticalAlignment.Top) ? (num + num3) : (num + layoutedFuntionWidget.Bounds.Y));
				break;
			}
			}
			break;
		}
		case MathFunctionType.Matrix:
		{
			IOfficeMathMatrix officeMathMatrix = layoutedFuntionWidget.Widget as IOfficeMathMatrix;
			switch (officeMathMatrix.VerticalAlignment)
			{
			case MathVerticalAlignment.Center:
				num = layoutedFuntionWidget.Bounds.Y + layoutedFuntionWidget.Bounds.Height / 2f;
				break;
			case MathVerticalAlignment.Top:
			case MathVerticalAlignment.Bottom:
			{
				List<LayoutedOMathWidget> list2 = null;
				list2 = ((officeMathMatrix.VerticalAlignment != MathVerticalAlignment.Top) ? (layoutedFuntionWidget as LayoutedMatrixWidget).Rows[(layoutedFuntionWidget as LayoutedMatrixWidget).Rows.Count - 1] : (layoutedFuntionWidget as LayoutedMatrixWidget).Rows[0]);
				num2 = 0f;
				float num4 = float.MaxValue;
				index = 0;
				for (int k = 0; k < list2.Count; k++)
				{
					if (num2 < list2[k].Bounds.Height)
					{
						num2 = list2[k].Bounds.Height;
						index = k;
					}
					if (list2[k].Bounds.Y < num4)
					{
						num4 = list2[k].Bounds.Y;
					}
				}
				num = list2[index].GetVerticalCenterPoint();
				num = ((officeMathMatrix.VerticalAlignment != MathVerticalAlignment.Top) ? (num + num4) : (num + layoutedFuntionWidget.Bounds.Y));
				break;
			}
			}
			break;
		}
		case MathFunctionType.Delimiter:
		{
			num2 = 0f;
			index = 0;
			LayoutedDelimiterWidget layoutedDelimiterWidget = layoutedFuntionWidget as LayoutedDelimiterWidget;
			OfficeMathDelimiter officeMathDelimiter = layoutedDelimiterWidget.Widget as OfficeMathDelimiter;
			if (officeMathDelimiter.IsGrow && officeMathDelimiter.DelimiterShape == MathDelimiterShapeType.Centered)
			{
				num = layoutedDelimiterWidget.Bounds.Height / 2f;
			}
			else
			{
				for (int i = 0; i < layoutedDelimiterWidget.Equation.Count; i++)
				{
					if (num2 < layoutedDelimiterWidget.Equation[i].Bounds.Height)
					{
						num2 = layoutedDelimiterWidget.Equation[i].Bounds.Height;
						index = i;
					}
				}
				num = layoutedDelimiterWidget.Equation[index].GetVerticalCenterPoint();
			}
			num += layoutedFuntionWidget.Bounds.Y;
			break;
		}
		case MathFunctionType.Radical:
		{
			LayoutedRadicalWidget layoutedRadicalWidget = layoutedFuntionWidget as LayoutedRadicalWidget;
			num = layoutedRadicalWidget.Equation.Bounds.Y + layoutedRadicalWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.Bar:
		case MathFunctionType.BorderBox:
		case MathFunctionType.Phantom:
		case MathFunctionType.RunElement:
			num = layoutedFuntionWidget.Bounds.Y + layoutedFuntionWidget.Bounds.Height / 2f;
			break;
		case MathFunctionType.Function:
		{
			LayoutedMathFunctionWidget layoutedMathFunctionWidget = layoutedFuntionWidget as LayoutedMathFunctionWidget;
			num = layoutedMathFunctionWidget.Equation.Bounds.Y + layoutedMathFunctionWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.Limit:
		{
			LayoutedLimitWidget layoutedLimitWidget = layoutedFuntionWidget as LayoutedLimitWidget;
			num = layoutedLimitWidget.Equation.Bounds.Y + layoutedLimitWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.Box:
			num = layoutedFuntionWidget.Bounds.Y + layoutedFuntionWidget.Bounds.Height / 2f;
			break;
		case MathFunctionType.LeftSubSuperscript:
		case MathFunctionType.SubSuperscript:
		case MathFunctionType.RightSubSuperscript:
		{
			LayoutedScriptWidget layoutedScriptWidget = layoutedFuntionWidget as LayoutedScriptWidget;
			num = layoutedScriptWidget.Equation.Bounds.Y + layoutedScriptWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.NArray:
		{
			LayoutedNArrayWidget layoutedNArrayWidget = layoutedFuntionWidget as LayoutedNArrayWidget;
			num = layoutedNArrayWidget.Equation.Bounds.Y + layoutedNArrayWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.Accent:
		{
			LayoutedAccentWidget layoutedAccentWidget = layoutedFuntionWidget as LayoutedAccentWidget;
			num = layoutedAccentWidget.Bounds.Y + layoutedAccentWidget.Equation.Bounds.Height / 2f;
			break;
		}
		case MathFunctionType.GroupCharacter:
		{
			LayoutedGroupCharacterWidget layoutedGroupCharacterWidget = layoutedFuntionWidget as LayoutedGroupCharacterWidget;
			num = layoutedGroupCharacterWidget.Bounds.Y + layoutedGroupCharacterWidget.Equation.Bounds.Height / 2f;
			break;
		}
		}
		return num - layoutedFuntionWidget.Bounds.Y;
	}
}
