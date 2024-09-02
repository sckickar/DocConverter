using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class MathLayoutContext : LayoutContext
{
	private WordDocument m_document;

	private SizeF m_containerSize;

	private List<char> stretchableCharacters;

	private Stack<MathFunctionType> m_mathLayoutingStack;

	private float operatorXPosition;

	private float lineMaxHeight;

	private float mathXPosition;

	internal WMath MathWidget => base.Widget as WMath;

	internal WordDocument Document => m_document;

	internal SizeF ContainerSize
	{
		get
		{
			return m_containerSize;
		}
		set
		{
			m_containerSize = value;
		}
	}

	private Stack<MathFunctionType> MathLayoutingStack => m_mathLayoutingStack;

	internal MathLayoutContext(ILeafWidget widget, ILCOperator lcOperator, bool isForceFitLayout)
		: base(widget, lcOperator, isForceFitLayout)
	{
		m_document = (widget as Entity).Document;
		m_mathLayoutingStack = new Stack<MathFunctionType>();
	}

	public override LayoutedWidget Layout(RectangleF rect)
	{
		bool num = CreateLayoutArea(ref rect);
		LayoutedMathWidget layoutedMathWidget = CreateMathLayoutedWidget(rect.Location);
		ContainerSize = rect.Size;
		LayoutOfficeMathCollection(rect, layoutedMathWidget, MathWidget.MathParagraph.Maths);
		m_ltState = LayoutState.Fitted;
		if ((!num && Document.Settings.MathProperties != null) || MathWidget.IsInline)
		{
			return layoutedMathWidget;
		}
		return DoMathAlignment(layoutedMathWidget, rect);
	}

	private bool CreateLayoutArea(ref RectangleF rect)
	{
		if (Document.Settings.MathProperties != null && Document.Settings.MathProperties.DisplayMathDefaults && !MathWidget.IsInline)
		{
			rect.X += Document.Settings.MathProperties.LeftMargin;
			rect.Width -= Document.Settings.MathProperties.RightMargin;
			return true;
		}
		return false;
	}

	private LayoutedMathWidget DoMathAlignment(LayoutedMathWidget ltMathWidget, RectangleF clientArea)
	{
		MathJustification mathJustification = MathJustification.CenterGroup;
		if (Document.Settings.MathProperties != null)
		{
			mathJustification = Document.Settings.MathProperties.DefaultJustification;
		}
		if ((MathWidget.MathParagraph as OfficeMathParagraph).HasValue(77))
		{
			mathJustification = MathWidget.MathParagraph.Justification;
		}
		float num = clientArea.Right - ltMathWidget.Bounds.Right;
		switch (mathJustification)
		{
		case MathJustification.Right:
			ltMathWidget.ShiftXYPosition(num, 0f);
			break;
		case MathJustification.CenterGroup:
		case MathJustification.Center:
			ltMathWidget.ShiftXYPosition(num / 2f, 0f);
			break;
		}
		return ltMathWidget;
	}

	private void LayoutOfficeMathFunctions(RectangleF clientArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathBaseCollection officeMathFunctions)
	{
		RectangleF rectangleF = new RectangleF(clientArea.Location, clientArea.Size);
		for (int i = 0; i < officeMathFunctions.Count; i++)
		{
			ILayoutedFuntionWidget layoutedFuntionWidget = null;
			IOfficeMathFunctionBase officeMathFunctionBase = officeMathFunctions[i];
			MathLayoutingStack.Push(officeMathFunctionBase.Type);
			switch (officeMathFunctionBase.Type)
			{
			case MathFunctionType.Fraction:
				layoutedFuntionWidget = LayoutFractionSwitch(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Delimiter:
				layoutedFuntionWidget = LayoutDelimiterSwitch(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Function:
				layoutedFuntionWidget = LayoutMathFunctionWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Phantom:
				layoutedFuntionWidget = LayoutPhantomSwitch(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.BorderBox:
				layoutedFuntionWidget = LayoutBoderBoxWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.EquationArray:
				layoutedFuntionWidget = LayoutEquationArraySwitch(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Bar:
				layoutedFuntionWidget = LayoutBarWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Matrix:
				layoutedFuntionWidget = LayoutMatrixWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Radical:
				layoutedFuntionWidget = LayoutRadicalSwitch(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.RunElement:
				layoutedFuntionWidget = LayoutRunElement(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase, i);
				break;
			case MathFunctionType.Limit:
				layoutedFuntionWidget = LayoutLimitWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Box:
				layoutedFuntionWidget = LayoutBoxWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.LeftSubSuperscript:
			case MathFunctionType.SubSuperscript:
			case MathFunctionType.RightSubSuperscript:
				layoutedFuntionWidget = LayoutScriptWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.NArray:
				layoutedFuntionWidget = LayoutNArrayWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.Accent:
				layoutedFuntionWidget = LayoutAccentWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			case MathFunctionType.GroupCharacter:
				layoutedFuntionWidget = LayoutGroupCharacterWidget(rectangleF, officeMathLayoutedWidget, officeMathFunctionBase);
				break;
			}
			if (i + 1 != officeMathFunctions.Count && (officeMathFunctions[i].Type != MathFunctionType.RunElement || officeMathFunctions[i + 1].Type != MathFunctionType.RunElement))
			{
				WCharacterFormat controlCharacterProperty = GetControlCharacterProperty(officeMathFunctionBase);
				if (controlCharacterProperty != null)
				{
					float width = base.DrawingContext.MeasureString(" ", controlCharacterProperty.GetFontToRender(FontScriptType.English), null, FontScriptType.English).Width;
					layoutedFuntionWidget.Bounds = new RectangleF(layoutedFuntionWidget.Bounds.X, layoutedFuntionWidget.Bounds.Y, layoutedFuntionWidget.Bounds.Width + width, layoutedFuntionWidget.Bounds.Height);
				}
			}
			if (layoutedFuntionWidget != null)
			{
				rectangleF.X += layoutedFuntionWidget.Bounds.Width;
				rectangleF.Width -= layoutedFuntionWidget.Bounds.Width;
				layoutedFuntionWidget.Owner = officeMathLayoutedWidget;
				officeMathLayoutedWidget.Bounds = UpdateBounds(officeMathLayoutedWidget.Bounds, layoutedFuntionWidget.Bounds);
				officeMathLayoutedWidget.ChildWidgets.Add(layoutedFuntionWidget);
				if (officeMathLayoutedWidget.Bounds.Height > lineMaxHeight)
				{
					lineMaxHeight = officeMathLayoutedWidget.Bounds.Height;
				}
				if (layoutedFuntionWidget.Widget is IOfficeMathBox && layoutedFuntionWidget.Bounds.Y > rectangleF.Y)
				{
					rectangleF.X = layoutedFuntionWidget.Bounds.X + layoutedFuntionWidget.Bounds.Width;
					rectangleF.Y = layoutedFuntionWidget.Bounds.Y;
				}
			}
			MathLayoutingStack.Pop();
		}
		AlignOfficeMathWidgetVertically(officeMathLayoutedWidget);
	}

	private ILayoutedFuntionWidget LayoutGroupCharacterWidget(RectangleF clientActiveArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedGroupCharacterWidget layoutedGroupCharacterWidget = new LayoutedGroupCharacterWidget(mathFunction);
		IOfficeMathGroupCharacter officeMathGroupCharacter = mathFunction as IOfficeMathGroupCharacter;
		WCharacterFormat wCharacterFormat = officeMathGroupCharacter.ControlProperties as WCharacterFormat;
		float shiftY = 0f;
		layoutedGroupCharacterWidget.GroupCharacter = GetStringltWidget(wCharacterFormat, officeMathGroupCharacter.GroupCharacter, clientActiveArea);
		if (officeMathGroupCharacter.HasCharacterTop)
		{
			if (officeMathGroupCharacter.HasAlignTop)
			{
				float fontSizeRatio = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathGroupCharacter.Equation, fontSizeRatio);
				layoutedGroupCharacterWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathGroupCharacter.Equation);
				layoutedGroupCharacterWidget.Equation.ShiftXYPosition(0f, base.DrawingContext.GetAscent(wCharacterFormat.Font, FontScriptType.English));
			}
			else
			{
				layoutedGroupCharacterWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathGroupCharacter.Equation);
				string equationText = GetEquationText(officeMathGroupCharacter.Equation);
				string baseChar = "a";
				if (layoutedGroupCharacterWidget.GroupCharacter.Bounds.Height >= layoutedGroupCharacterWidget.Equation.Bounds.Height)
				{
					FindTextDifference(equationText, baseChar, wCharacterFormat, layoutedGroupCharacterWidget.Equation.Bounds, ref shiftY);
				}
				else
				{
					shiftY = layoutedGroupCharacterWidget.Equation.Bounds.Height / 8f;
				}
			}
		}
		else if (!officeMathGroupCharacter.HasAlignTop)
		{
			float fontSizeRatio2 = 0.71428573f;
			ReduceFontSizeOfOfficeMath(officeMathGroupCharacter.Equation, fontSizeRatio2);
			layoutedGroupCharacterWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathGroupCharacter.Equation);
			layoutedGroupCharacterWidget.Equation.ShiftXYPosition(0f, 0f - base.DrawingContext.GetAscent(wCharacterFormat.Font, FontScriptType.English) / 2f);
		}
		else
		{
			layoutedGroupCharacterWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathGroupCharacter.Equation);
			if (layoutedGroupCharacterWidget.Equation.Bounds.Height > layoutedGroupCharacterWidget.GroupCharacter.Bounds.Height)
			{
				shiftY = 0f - layoutedGroupCharacterWidget.Equation.Bounds.Height / 1.5f;
			}
		}
		if (layoutedGroupCharacterWidget.Equation == null)
		{
			layoutedGroupCharacterWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathGroupCharacter.Equation);
		}
		if (layoutedGroupCharacterWidget.Equation.Bounds.Width > layoutedGroupCharacterWidget.GroupCharacter.Bounds.Width)
		{
			layoutedGroupCharacterWidget.ScalingFactor = layoutedGroupCharacterWidget.Equation.Bounds.Width / layoutedGroupCharacterWidget.GroupCharacter.Bounds.Width;
		}
		RectangleF bounds = layoutedGroupCharacterWidget.GroupCharacter.Bounds;
		layoutedGroupCharacterWidget.GroupCharacter.Bounds = new RectangleF(bounds.X, bounds.Y - shiftY, bounds.Width, bounds.Height);
		layoutedGroupCharacterWidget.Bounds = UpdateBounds(layoutedGroupCharacterWidget.GroupCharacter.Bounds, layoutedGroupCharacterWidget.Equation.Bounds);
		return layoutedGroupCharacterWidget;
	}

	private void FindTextDifference(string eqText, string baseChar, WCharacterFormat characterFormat, RectangleF bounds, ref float shiftY)
	{
		shiftY = bounds.Height / 3f;
	}

	private LayoutedLimitWidget LayoutLimitWidget(RectangleF clientActiveArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedLimitWidget layoutedLimitWidget = new LayoutedLimitWidget(mathFunction);
		IOfficeMathLimit officeMathLimit = mathFunction as IOfficeMathLimit;
		layoutedLimitWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathLimit.Equation);
		float fontSizeRatio = 0.71428573f;
		ReduceFontSizeOfOfficeMath(officeMathLimit.Limit, fontSizeRatio);
		layoutedLimitWidget.Limit = LayoutOfficeMath(clientActiveArea, officeMathLimit.Limit);
		RectangleF bounds = default(RectangleF);
		if (officeMathLimit.LimitType == MathLimitType.UpperLimit)
		{
			DocGen.Drawing.Font fontToRender = (officeMathLimit.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
			layoutedLimitWidget.Equation.ShiftXYPosition(0f, layoutedLimitWidget.Limit.Bounds.Height - base.DrawingContext.GetDescent(fontToRender, FontScriptType.English));
			bounds.Y = layoutedLimitWidget.Limit.Bounds.Y;
			bounds.Height = layoutedLimitWidget.Equation.Bounds.Bottom - clientActiveArea.Y;
		}
		else if (officeMathLimit.LimitType == MathLimitType.LowerLimit)
		{
			DocGen.Drawing.Font fontToRender = (officeMathLimit.ControlProperties as WCharacterFormat).Font;
			layoutedLimitWidget.Limit.ShiftXYPosition(0f, layoutedLimitWidget.Equation.Bounds.Height - base.DrawingContext.GetDescent(fontToRender, FontScriptType.English));
			bounds.Y = layoutedLimitWidget.Equation.Bounds.Y;
			bounds.Height = layoutedLimitWidget.Limit.Bounds.Bottom - clientActiveArea.Y;
		}
		if (layoutedLimitWidget.Equation.Bounds.Width > layoutedLimitWidget.Limit.Bounds.Width)
		{
			float xPosition = (layoutedLimitWidget.Equation.Bounds.Width - layoutedLimitWidget.Limit.Bounds.Width) / 2f;
			layoutedLimitWidget.Limit.ShiftXYPosition(xPosition, 0f);
			bounds.X = layoutedLimitWidget.Equation.Bounds.X;
			bounds.Width = layoutedLimitWidget.Equation.Bounds.Width;
		}
		else if (layoutedLimitWidget.Limit.Bounds.Width > layoutedLimitWidget.Equation.Bounds.Width)
		{
			float xPosition2 = (layoutedLimitWidget.Limit.Bounds.Width - layoutedLimitWidget.Equation.Bounds.Width) / 2f;
			layoutedLimitWidget.Equation.ShiftXYPosition(xPosition2, 0f);
			bounds.X = layoutedLimitWidget.Limit.Bounds.X;
			bounds.Width = layoutedLimitWidget.Limit.Bounds.Width;
		}
		else
		{
			bounds.X = layoutedLimitWidget.Equation.Bounds.X;
			bounds.Width = layoutedLimitWidget.Equation.Bounds.Width;
		}
		layoutedLimitWidget.Bounds = bounds;
		return layoutedLimitWidget;
	}

	private ILayoutedFuntionWidget LayoutBoxWidget(RectangleF clientActiveArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedBoxWidget layoutedBoxWidget = new LayoutedBoxWidget(mathFunction);
		IOfficeMathBox officeMathBox = mathFunction as IOfficeMathBox;
		layoutedBoxWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathBox.Equation);
		if (officeMathBox.OperatorEmulator && officeMathBox.Break != null)
		{
			float xPosition = mathXPosition - clientActiveArea.X;
			if (officeMathBox.Break.AlignAt == 1)
			{
				xPosition = operatorXPosition - clientActiveArea.X;
			}
			layoutedBoxWidget.Equation.ShiftXYPosition(xPosition, lineMaxHeight);
		}
		if (operatorXPosition == 0f)
		{
			operatorXPosition = layoutedBoxWidget.Equation.Bounds.X;
		}
		layoutedBoxWidget.Bounds = layoutedBoxWidget.Equation.Bounds;
		return layoutedBoxWidget;
	}

	private LayoutedScriptWidget LayoutScriptWidget(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedScriptWidget layoutedScriptWidget = new LayoutedScriptWidget(mathFunction);
		IOfficeMathScript officeMathScript = mathFunction as IOfficeMathScript;
		IOfficeMathLeftScript officeMathLeftScript = mathFunction as IOfficeMathLeftScript;
		IOfficeMathRightScript officeMathRightScript = mathFunction as IOfficeMathRightScript;
		float num = 1f;
		if (IsNested())
		{
			num = 0.58823526f;
			if (officeMathScript != null)
			{
				ReduceFontSizeOfOfficeMath(officeMathScript.Script, num);
			}
			else if (officeMathLeftScript != null)
			{
				ReduceFontSizeOfOfficeMath(officeMathLeftScript.Superscript, num);
				ReduceFontSizeOfOfficeMath(officeMathLeftScript.Subscript, num);
			}
			else if (officeMathRightScript != null)
			{
				ReduceFontSizeOfOfficeMath(officeMathRightScript.Superscript, num);
				ReduceFontSizeOfOfficeMath(officeMathRightScript.Subscript, num);
			}
		}
		float x = currentBounds.X;
		float y = currentBounds.Y;
		switch (mathFunction.Type)
		{
		case MathFunctionType.SubSuperscript:
		{
			if (!IsNested())
			{
				num = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathScript.Script, num);
			}
			DocGen.Drawing.Font font = (officeMathScript.ControlProperties as WCharacterFormat).Font;
			layoutedScriptWidget.Equation = LayoutOfficeMath(currentBounds, officeMathScript.Equation);
			if (officeMathScript.ScriptType == MathScriptType.Superscript)
			{
				layoutedScriptWidget.Superscript = LayoutOfficeMath(currentBounds, officeMathScript.Script);
				y = layoutedScriptWidget.Superscript.Bounds.Height / 2f - base.DrawingContext.GetDescent(font, FontScriptType.English);
				x = layoutedScriptWidget.Equation.Bounds.Width;
				layoutedScriptWidget.Equation.ShiftXYPosition(0f, y);
				layoutedScriptWidget.Superscript.ShiftXYPosition(x, 0f);
			}
			else
			{
				layoutedScriptWidget.Subscript = LayoutOfficeMath(currentBounds, officeMathScript.Script);
				x = layoutedScriptWidget.Equation.Bounds.Width;
				y = layoutedScriptWidget.Equation.Bounds.Height - base.DrawingContext.GetDescent(font, FontScriptType.English) - layoutedScriptWidget.Subscript.Bounds.Height / 2f;
				layoutedScriptWidget.Subscript.ShiftXYPosition(x, y);
			}
			break;
		}
		case MathFunctionType.LeftSubSuperscript:
		{
			DocGen.Drawing.Font font = (officeMathLeftScript.ControlProperties as WCharacterFormat).Font;
			if (!IsNested())
			{
				num = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathLeftScript.Superscript, num);
				ReduceFontSizeOfOfficeMath(officeMathLeftScript.Subscript, num);
			}
			layoutedScriptWidget.Superscript = LayoutOfficeMath(currentBounds, officeMathLeftScript.Superscript);
			layoutedScriptWidget.Subscript = LayoutOfficeMath(currentBounds, officeMathLeftScript.Subscript);
			layoutedScriptWidget.Equation = LayoutOfficeMath(currentBounds, officeMathLeftScript.Equation);
			float num2 = 0f;
			if (layoutedScriptWidget.Superscript.Bounds.Width > layoutedScriptWidget.Subscript.Bounds.Width)
			{
				x = layoutedScriptWidget.Superscript.Bounds.Width;
				num2 = x - layoutedScriptWidget.Subscript.Bounds.Width;
				layoutedScriptWidget.Subscript.ShiftXYPosition(num2, 0f);
			}
			else if (layoutedScriptWidget.Superscript.Bounds.Width < layoutedScriptWidget.Subscript.Bounds.Width)
			{
				x = layoutedScriptWidget.Subscript.Bounds.Width;
				num2 = x - layoutedScriptWidget.Superscript.Bounds.Width;
				layoutedScriptWidget.Superscript.ShiftXYPosition(num2, 0f);
			}
			else
			{
				x = layoutedScriptWidget.Superscript.Bounds.Width;
			}
			layoutedScriptWidget.Equation.ShiftXYPosition(x, 0f);
			y = layoutedScriptWidget.Superscript.Bounds.Height / 2f - base.DrawingContext.GetDescent(font, FontScriptType.English);
			layoutedScriptWidget.Equation.ShiftXYPosition(0f, y);
			y = layoutedScriptWidget.Equation.Bounds.Height - base.DrawingContext.GetDescent(font, FontScriptType.English) - layoutedScriptWidget.Subscript.Bounds.Height / 2f;
			layoutedScriptWidget.Subscript.ShiftXYPosition(0f, y);
			break;
		}
		case MathFunctionType.RightSubSuperscript:
		{
			DocGen.Drawing.Font font = (officeMathRightScript.ControlProperties as WCharacterFormat).Font;
			if (!IsNested())
			{
				num = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathRightScript.Superscript, num);
				ReduceFontSizeOfOfficeMath(officeMathRightScript.Subscript, num);
			}
			layoutedScriptWidget.Equation = LayoutOfficeMath(currentBounds, officeMathRightScript.Equation);
			layoutedScriptWidget.Superscript = LayoutOfficeMath(currentBounds, officeMathRightScript.Superscript);
			y = layoutedScriptWidget.Superscript.Bounds.Height / 2f - base.DrawingContext.GetDescent(font, FontScriptType.English);
			x = layoutedScriptWidget.Equation.Bounds.Width;
			layoutedScriptWidget.Equation.ShiftXYPosition(0f, y);
			layoutedScriptWidget.Superscript.ShiftXYPosition(x, 0f);
			layoutedScriptWidget.Subscript = LayoutOfficeMath(currentBounds, officeMathRightScript.Subscript);
			x = layoutedScriptWidget.Equation.Bounds.Width;
			y = layoutedScriptWidget.Equation.Bounds.Height - base.DrawingContext.GetDescent(font, FontScriptType.English) - layoutedScriptWidget.Subscript.Bounds.Height / 2f;
			layoutedScriptWidget.Subscript.ShiftXYPosition(x, y);
			break;
		}
		}
		layoutedScriptWidget.Bounds = GetUpdatedBounds(officeMathScript, officeMathLeftScript, officeMathRightScript, layoutedScriptWidget);
		return layoutedScriptWidget;
	}

	private LayoutedFuntionWidget LayoutNArrayWidget(RectangleF clientActiveArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedNArrayWidget layoutedNArrayWidget = new LayoutedNArrayWidget(mathFunction);
		IOfficeMathNArray officeMathNArray = mathFunction as IOfficeMathNArray;
		float num = 1f;
		layoutedNArrayWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathNArray.Equation);
		layoutedNArrayWidget.NArrayCharacter = GetStringltWidget(officeMathNArray.ControlProperties as WCharacterFormat, officeMathNArray.NArrayCharacter, clientActiveArea);
		RectangleF bounds = layoutedNArrayWidget.NArrayCharacter.Bounds;
		float size = layoutedNArrayWidget.NArrayCharacter.Font.Size;
		float right = bounds.Right;
		if (!officeMathNArray.HideUpperLimit)
		{
			if (!IsNested())
			{
				num = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathNArray.Superscript, num);
			}
			else
			{
				num = 0.58823526f;
				ReduceFontSizeOfOfficeMath(officeMathNArray.Superscript, num);
			}
			layoutedNArrayWidget.Superscript = LayoutOfficeMath(clientActiveArea, officeMathNArray.Superscript);
			if (officeMathNArray.SubSuperscriptLimit)
			{
				bounds.Y += layoutedNArrayWidget.Superscript.Bounds.Height / 4f;
				layoutedNArrayWidget.Equation.ShiftXYPosition(0f, bounds.Y - layoutedNArrayWidget.Equation.Bounds.Y);
				layoutedNArrayWidget.Superscript.ShiftXYPosition(bounds.Width, 0f);
			}
			else
			{
				bounds.Y = layoutedNArrayWidget.Superscript.Bounds.Bottom;
				layoutedNArrayWidget.Equation.ShiftXYPosition(0f, bounds.Y - layoutedNArrayWidget.Equation.Bounds.Y);
				layoutedNArrayWidget.Superscript.ShiftXYPosition(bounds.Width / 2.5f, 0f);
			}
			if (layoutedNArrayWidget.Superscript.Bounds.Width > bounds.Width && !IsNested())
			{
				bounds.X += layoutedNArrayWidget.Superscript.Bounds.Width / 2f;
			}
			if (layoutedNArrayWidget.Superscript.Bounds.Right > bounds.Right)
			{
				right = layoutedNArrayWidget.Superscript.Bounds.Right;
			}
		}
		if (!officeMathNArray.HideLowerLimit)
		{
			if (!IsNested())
			{
				num = 0.71428573f;
				ReduceFontSizeOfOfficeMath(officeMathNArray.Subscript, num);
			}
			else
			{
				num = 0.58823526f;
				ReduceFontSizeOfOfficeMath(officeMathNArray.Subscript, num);
			}
			layoutedNArrayWidget.Subscript = LayoutOfficeMath(clientActiveArea, officeMathNArray.Subscript);
			float num2 = bounds.Height - size;
			float x = clientActiveArea.X;
			float y = clientActiveArea.Y;
			if (officeMathNArray.SubSuperscriptLimit)
			{
				DocGen.Drawing.Font font = layoutedNArrayWidget.NArrayCharacter.Font;
				x = layoutedNArrayWidget.NArrayCharacter.Bounds.Width;
				y = bounds.Bottom - clientActiveArea.Y - num2 - base.DrawingContext.GetDescent(font, FontScriptType.English);
				layoutedNArrayWidget.Subscript.ShiftXYPosition(x, y);
			}
			else
			{
				x = layoutedNArrayWidget.NArrayCharacter.Bounds.Width / 2.5f;
				y = bounds.Bottom - clientActiveArea.Y - num2;
				if (bounds.Right >= layoutedNArrayWidget.Subscript.Bounds.Right)
				{
					x = (bounds.Right - layoutedNArrayWidget.Subscript.Bounds.Right) / 2f;
				}
				else
				{
					float num3 = layoutedNArrayWidget.Subscript.Bounds.Width - bounds.Width;
					num3 /= 2f;
					x = 0f;
					bounds.X += num3;
					layoutedNArrayWidget.Superscript.ShiftXYPosition(num3, 0f);
				}
				layoutedNArrayWidget.Subscript.ShiftXYPosition(x, y);
			}
			if (layoutedNArrayWidget.Subscript.Bounds.Right > bounds.Right && (officeMathNArray.HideUpperLimit || layoutedNArrayWidget.Subscript.Bounds.Right > layoutedNArrayWidget.Superscript.Bounds.Right))
			{
				right = layoutedNArrayWidget.Subscript.Bounds.Right;
			}
		}
		right -= clientActiveArea.X;
		float num4 = (bounds.Height - layoutedNArrayWidget.Equation.Bounds.Height) / 2f;
		if (!officeMathNArray.HideUpperLimit)
		{
			if (layoutedNArrayWidget.Superscript.Bounds.Y > layoutedNArrayWidget.Equation.Bounds.Y + num4)
			{
				float num5 = layoutedNArrayWidget.Superscript.Bounds.Y - (layoutedNArrayWidget.Equation.Bounds.Y + num4);
				layoutedNArrayWidget.Superscript.ShiftXYPosition(0f, num5);
				if (layoutedNArrayWidget.Subscript != null)
				{
					layoutedNArrayWidget.Subscript.ShiftXYPosition(0f, num5);
				}
				layoutedNArrayWidget.Equation.ShiftXYPosition(right, num4 + num5);
				bounds.Y += num5;
			}
			else
			{
				layoutedNArrayWidget.Equation.ShiftXYPosition(right, num4);
			}
		}
		else if (layoutedNArrayWidget.NArrayCharacter.Bounds.Y > layoutedNArrayWidget.Equation.Bounds.Y + num4)
		{
			float num6 = layoutedNArrayWidget.NArrayCharacter.Bounds.Y - (layoutedNArrayWidget.Equation.Bounds.Y + num4);
			layoutedNArrayWidget.Equation.ShiftXYPosition(right, num4 + num6);
			if (layoutedNArrayWidget.Subscript != null)
			{
				layoutedNArrayWidget.Subscript.ShiftXYPosition(0f, num6);
			}
			bounds.Y += num6;
		}
		else
		{
			layoutedNArrayWidget.Equation.ShiftXYPosition(right, num4);
		}
		layoutedNArrayWidget.NArrayCharacter.Bounds = bounds;
		layoutedNArrayWidget.Bounds = GetUpdatedBoundsForNArray(layoutedNArrayWidget, officeMathNArray, bounds);
		return layoutedNArrayWidget;
	}

	private LayoutedAccentWidget LayoutAccentWidget(RectangleF clientActiveArea, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedAccentWidget layoutedAccentWidget = new LayoutedAccentWidget(mathFunction);
		IOfficeMathAccent officeMathAccent = mathFunction as IOfficeMathAccent;
		WCharacterFormat characterFormat = officeMathAccent.ControlProperties as WCharacterFormat;
		List<int> list = new List<int>();
		list.Add(769);
		list.Add(780);
		list.Add(776);
		list.Add(775);
		list.Add(770);
		list.Add(768);
		list.Add(774);
		list.Add(771);
		string baseChar = "a";
		layoutedAccentWidget.Equation = LayoutOfficeMath(clientActiveArea, officeMathAccent.Equation);
		layoutedAccentWidget.AccentCharacter = GetStringltWidget(characterFormat, officeMathAccent.AccentCharacter, clientActiveArea);
		if (layoutedAccentWidget.Equation.Bounds.Width > layoutedAccentWidget.AccentCharacter.Bounds.Width && !list.Contains(officeMathAccent.AccentCharacter[0]))
		{
			layoutedAccentWidget.ScalingFactor = layoutedAccentWidget.Equation.Bounds.Width / layoutedAccentWidget.AccentCharacter.Bounds.Width;
		}
		float num = 0f;
		float shiftY = 0f;
		string equationText = GetEquationText(officeMathAccent.Equation);
		if (layoutedAccentWidget.AccentCharacter.Bounds.Height >= layoutedAccentWidget.Equation.Bounds.Height)
		{
			FindTextDifference(equationText, baseChar, characterFormat, layoutedAccentWidget.Equation.Bounds, ref shiftY);
		}
		else
		{
			shiftY = layoutedAccentWidget.Equation.Bounds.Height / 10f;
		}
		if (layoutedAccentWidget.Equation.Bounds.Width > layoutedAccentWidget.AccentCharacter.Bounds.Width && layoutedAccentWidget.ScalingFactor == 1f)
		{
			num = layoutedAccentWidget.Equation.Bounds.Width / 2f + layoutedAccentWidget.AccentCharacter.Bounds.Width / 2f;
		}
		else if (officeMathAccent.AccentCharacter[0] == '\u0305' || officeMathAccent.AccentCharacter[0] == '\u033f')
		{
			num = layoutedAccentWidget.Equation.Bounds.Width;
		}
		layoutedAccentWidget.AccentCharacter.Bounds = new RectangleF(layoutedAccentWidget.AccentCharacter.Bounds.X + num, layoutedAccentWidget.AccentCharacter.Bounds.Y - shiftY, layoutedAccentWidget.AccentCharacter.Bounds.Width, layoutedAccentWidget.AccentCharacter.Bounds.Height);
		layoutedAccentWidget.Bounds = UpdateBounds(layoutedAccentWidget.AccentCharacter.Bounds, layoutedAccentWidget.Equation.Bounds);
		return layoutedAccentWidget;
	}

	private string GetEquationText(IOfficeMath equation)
	{
		string result = "";
		for (int i = 0; i < equation.Functions.Count; i++)
		{
			IOfficeMathFunctionBase officeMathFunctionBase = equation.Functions[i];
			if (officeMathFunctionBase.Type == MathFunctionType.RunElement)
			{
				IOfficeMathRunElement officeMathRunElement = officeMathFunctionBase as IOfficeMathRunElement;
				if (officeMathRunElement.Item is WTextRange)
				{
					result = (officeMathRunElement.Item as WTextRange).Text;
				}
			}
		}
		return result;
	}

	private LayoutedStringWidget GetStringltWidget(WCharacterFormat characterFormat, string character, RectangleF clientActiveArea)
	{
		LayoutedStringWidget layoutedStringWidget = new LayoutedStringWidget();
		layoutedStringWidget.Text = character;
		float fontSize = characterFormat.FontSize;
		if (MathLayoutingStack.Peek() == MathFunctionType.NArray)
		{
			fontSize = characterFormat.FontSize * 2f;
		}
		DocGen.Drawing.Font font = Document.FontSettings.GetFont((characterFormat.SymExFontName != null) ? characterFormat.SymExFontName : characterFormat.FontName, fontSize, FontStyle.Regular, FontScriptType.English);
		layoutedStringWidget.Font = font.Clone() as DocGen.Drawing.Font;
		SizeF sizeF = base.DrawingContext.MeasureString(layoutedStringWidget.Text, layoutedStringWidget.Font, base.DrawingContext.StringFormt, FontScriptType.English);
		if (sizeF.Width == 0f)
		{
			sizeF.Width = base.DrawingContext.MeasureString('\u20d7'.ToString(), layoutedStringWidget.Font, base.DrawingContext.StringFormt, FontScriptType.English).Width;
		}
		layoutedStringWidget.Bounds = new RectangleF(clientActiveArea.X, clientActiveArea.Y, sizeF.Width, sizeF.Height);
		return layoutedStringWidget;
	}

	private RectangleF GetUpdatedBoundsForNArray(LayoutedNArrayWidget nArrayWidget, IOfficeMathNArray nArray, RectangleF narrayBounds)
	{
		RectangleF result = default(RectangleF);
		result.X = nArrayWidget.NArrayCharacter.Bounds.X;
		if (nArrayWidget.Equation.Bounds.Y < narrayBounds.Y)
		{
			result.Y = nArrayWidget.Equation.Bounds.Y;
		}
		else
		{
			result.Y = narrayBounds.Y;
		}
		if (!nArray.HideUpperLimit && nArrayWidget.Equation.Bounds.Y > nArrayWidget.Superscript.Bounds.Y)
		{
			result.Y = nArrayWidget.Superscript.Bounds.Y;
		}
		result.Width = nArrayWidget.Equation.Bounds.Right - result.X;
		if (nArrayWidget.Equation.Bounds.Bottom > narrayBounds.Bottom)
		{
			result.Height = nArrayWidget.Equation.Bounds.Bottom - result.Y;
		}
		else
		{
			result.Height = narrayBounds.Bottom - result.Y;
		}
		if (!nArray.HideLowerLimit && nArrayWidget.Equation.Bounds.Bottom < nArrayWidget.Subscript.Bounds.Bottom)
		{
			result.Height = nArrayWidget.Subscript.Bounds.Bottom - result.Y;
		}
		return result;
	}

	private RectangleF GetUpdatedBounds(IOfficeMathScript script, IOfficeMathLeftScript leftScript, IOfficeMathRightScript rightScript, LayoutedScriptWidget scriptWidget)
	{
		RectangleF result = default(RectangleF);
		if (script != null)
		{
			result.X = scriptWidget.Equation.Bounds.X;
			if (script.ScriptType == MathScriptType.Superscript)
			{
				result.Y = scriptWidget.Superscript.Bounds.Y;
				result.Width = scriptWidget.Equation.Bounds.Width + scriptWidget.Superscript.Bounds.Width;
				result.Height = scriptWidget.Equation.Bounds.Bottom - scriptWidget.Superscript.Bounds.Y;
			}
			else
			{
				result.Y = scriptWidget.Equation.Bounds.Y;
				result.Width = scriptWidget.Equation.Bounds.Width + scriptWidget.Subscript.Bounds.Width;
				result.Height = scriptWidget.Subscript.Bounds.Bottom - scriptWidget.Equation.Bounds.Y;
			}
		}
		else if (leftScript != null)
		{
			result.X = scriptWidget.Superscript.Bounds.X;
			result.Y = scriptWidget.Superscript.Bounds.Y;
			result.Width = scriptWidget.Equation.Bounds.Width + ((scriptWidget.Subscript.Bounds.Width > scriptWidget.Superscript.Bounds.Width) ? scriptWidget.Subscript.Bounds.Width : scriptWidget.Superscript.Bounds.Width);
			result.Height = scriptWidget.Subscript.Bounds.Bottom - scriptWidget.Superscript.Bounds.Y;
		}
		else if (rightScript != null)
		{
			result.X = scriptWidget.Equation.Bounds.X;
			result.Y = scriptWidget.Superscript.Bounds.Y;
			result.Width = scriptWidget.Equation.Bounds.Width + ((scriptWidget.Subscript.Bounds.Width > scriptWidget.Superscript.Bounds.Width) ? scriptWidget.Subscript.Bounds.Width : scriptWidget.Superscript.Bounds.Width);
			result.Height = scriptWidget.Subscript.Bounds.Bottom - scriptWidget.Superscript.Bounds.Y;
		}
		return result;
	}

	private WCharacterFormat GetControlCharacterProperty(IOfficeMathFunctionBase mathFunction)
	{
		WCharacterFormat result = null;
		switch (mathFunction.Type)
		{
		case MathFunctionType.Fraction:
			result = (mathFunction as IOfficeMathFraction).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Delimiter:
			result = (mathFunction as IOfficeMathDelimiter).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Phantom:
			result = (mathFunction as IOfficeMathPhantom).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Function:
			result = (mathFunction as IOfficeMathFunction).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.BorderBox:
			result = (mathFunction as IOfficeMathBorderBox).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.EquationArray:
			result = (mathFunction as IOfficeMathEquationArray).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Bar:
			result = (mathFunction as IOfficeMathBar).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Radical:
			result = (mathFunction as IOfficeMathRadical).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Matrix:
			result = (mathFunction as IOfficeMathMatrix).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.RunElement:
			if ((mathFunction as IOfficeMathRunElement).Item is WTextRange)
			{
				result = ((mathFunction as IOfficeMathRunElement).Item as WTextRange).CharacterFormat;
			}
			break;
		case MathFunctionType.SubSuperscript:
			result = (mathFunction as IOfficeMathScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.LeftSubSuperscript:
			result = (mathFunction as IOfficeMathLeftScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.RightSubSuperscript:
			result = (mathFunction as IOfficeMathRightScript).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.NArray:
			result = (mathFunction as IOfficeMathNArray).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Limit:
			result = (mathFunction as IOfficeMathLimit).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Box:
			result = (mathFunction as IOfficeMathBox).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.Accent:
			result = (mathFunction as IOfficeMathAccent).ControlProperties as WCharacterFormat;
			break;
		case MathFunctionType.GroupCharacter:
			result = (mathFunction as IOfficeMathGroupCharacter).ControlProperties as WCharacterFormat;
			break;
		}
		return result;
	}

	private RectangleF UpdateBounds(RectangleF currentBounds, RectangleF modifiedBounds)
	{
		RectangleF result = currentBounds;
		if (modifiedBounds.X < result.X)
		{
			result.X = modifiedBounds.X;
		}
		if (modifiedBounds.Y < result.Y)
		{
			result.Y = modifiedBounds.Y;
		}
		result.Width += modifiedBounds.Width;
		if (modifiedBounds.Height > result.Height)
		{
			result.Height = modifiedBounds.Height;
		}
		return result;
	}

	private void GetNextCharacters(WTextRange textRange, ref char firstSpace, ref char secondSpace, ref char finalChar)
	{
		if (textRange.Text.Length > 0)
		{
			if (firstSpace == '\uffff')
			{
				firstSpace = textRange.Text[0];
			}
			else if (secondSpace == '\uffff')
			{
				secondSpace = textRange.Text[0];
			}
			else if (finalChar == '\uffff')
			{
				finalChar = textRange.Text[0];
			}
		}
		if (textRange.Text.Length > 1)
		{
			if (secondSpace == '\uffff')
			{
				secondSpace = textRange.Text[1];
			}
			else if (finalChar == '\uffff')
			{
				finalChar = textRange.Text[1];
			}
		}
		if (textRange.Text.Length > 2 && finalChar == '\uffff')
		{
			finalChar = textRange.Text[2];
		}
	}

	internal void UpdateTextByMathStyles(WTextRange textRange, int currentIndex)
	{
		string text = textRange.Text;
		MathStyleType mathStyleType = textRange.OwnerMathRunElement.MathFormat.Style;
		if (textRange.OwnerMathRunElement.MathFormat.HasNormalText)
		{
			mathStyleType = MathStyleType.Regular;
		}
		string text2 = "";
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			int num = 0;
			int num2 = Convert.ToInt32(c);
			int num3 = 52;
			char nextCharacter = '\uffff';
			char previousCharacter = '\uffff';
			bool flag = false;
			bool flag2 = false;
			bool flag3 = IsOperatorSymbol(c);
			if (mathStyleType != MathStyleType.Regular && c == ',')
			{
				char firstSpace = '\uffff';
				char secondSpace = '\uffff';
				char finalChar = '\uffff';
				if (i + 1 < text.Length)
				{
					firstSpace = text[i + 1];
				}
				if (i + 2 < text.Length)
				{
					secondSpace = text[i + 2];
				}
				if (i + 3 < text.Length)
				{
					finalChar = text[i + 3];
				}
				for (int j = 1; j < 3; j++)
				{
					if ((finalChar == '\uffff' || secondSpace == '\uffff' || firstSpace == '\uffff') && textRange.OwnerMathRunElement.OwnerMathEntity is OfficeMath && currentIndex + j < (textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions.Count)
					{
						if (!((textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex + j] is OfficeMathRunElement))
						{
							finalChar = ' ';
							break;
						}
						OfficeMathRunElement officeMathRunElement = (textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex + j] as OfficeMathRunElement;
						if (officeMathRunElement.Item is WTextRange)
						{
							WTextRange textRange2 = officeMathRunElement.Item as WTextRange;
							GetNextCharacters(textRange2, ref firstSpace, ref secondSpace, ref finalChar);
						}
					}
				}
				if ((finalChar == '\uffff' || secondSpace == '\uffff' || firstSpace == '\uffff') && textRange.OwnerMathRunElement.OwnerMathEntity != null && textRange.OwnerMathRunElement.OwnerMathEntity.OwnerMathEntity is OfficeMathParagraph)
				{
					OfficeMathParagraph officeMathParagraph = textRange.OwnerMathRunElement.OwnerMathEntity.OwnerMathEntity as OfficeMathParagraph;
					int num4 = 0;
					for (int k = 0; k < officeMathParagraph.Maths.Count; k++)
					{
						if (officeMathParagraph.Maths[k] == textRange.OwnerMathRunElement.OwnerMathEntity)
						{
							num4 = k;
							break;
						}
					}
					for (int l = 1; l < officeMathParagraph.Maths.Count; l++)
					{
						if (finalChar != '\uffff' && secondSpace != '\uffff' && firstSpace != '\uffff')
						{
							break;
						}
						if (num4 + 1 >= officeMathParagraph.Maths.Count || !(officeMathParagraph.Maths[num4 + l] is OfficeMath))
						{
							continue;
						}
						OfficeMath officeMath = officeMathParagraph.Maths[num4 + l] as OfficeMath;
						for (int m = 0; m < officeMath.Functions.Count; m++)
						{
							if (officeMath.Functions[m] is OfficeMathRunElement)
							{
								OfficeMathRunElement officeMathRunElement2 = officeMath.Functions[m] as OfficeMathRunElement;
								if (officeMathRunElement2.Item is WTextRange)
								{
									WTextRange textRange3 = officeMathRunElement2.Item as WTextRange;
									GetNextCharacters(textRange3, ref firstSpace, ref secondSpace, ref finalChar);
								}
							}
						}
					}
				}
				if (firstSpace == ' ' && secondSpace == ' ' && finalChar != '\uffff')
				{
					text2 = text2 + char.ConvertFromUtf32(c) + char.ConvertFromUtf32(8195) + char.ConvertFromUtf32(8195);
					continue;
				}
			}
			GetPreviousCharacter(ref previousCharacter, i, text, currentIndex, textRange);
			GetNextCharacter(ref nextCharacter, i, text, currentIndex, textRange);
			if (mathStyleType != MathStyleType.Regular && flag3)
			{
				if (previousCharacter != '\uffff')
				{
					flag2 = IsOperatorSymbol(previousCharacter);
				}
				if (nextCharacter != '\uffff')
				{
					flag = IsOperatorSymbol(nextCharacter);
				}
				if (!flag2 && previousCharacter != '\uffff' && c != ',' && c != '.')
				{
					text2 += " ";
				}
				text2 = ((c != '-') ? (text2 + char.ConvertFromUtf32(c)) : (text2 + char.ConvertFromUtf32(8722)));
				if ((!flag || (c >= '∫' && c <= '∴')) && nextCharacter != '\uffff' && !ShouldSkipSpacingAfterOperator(previousCharacter, c, ref nextCharacter, i + 1, text, currentIndex, textRange))
				{
					text2 += " ";
				}
				continue;
			}
			MathFontType font = textRange.OwnerMathRunElement.MathFormat.Font;
			if (font == MathFontType.Fraktur)
			{
				num3 = 104;
			}
			if (IsGreakCharacter(c))
			{
				if (char.IsUpper(c))
				{
					num = num2 - 913;
					switch (font)
					{
					case MathFontType.Roman:
						num += 120488;
						break;
					case MathFontType.SansSerif:
						num += 120662;
						break;
					}
				}
				else if (char.IsLower(c))
				{
					num = num2 - 945;
					num = AdjustBaseValue(num2, num);
					switch (font)
					{
					case MathFontType.Roman:
						num += 120514;
						break;
					case MathFontType.SansSerif:
						num += 120688;
						break;
					}
				}
				num3 = 58;
			}
			else
			{
				if (char.IsNumber(c))
				{
					num = num2 - 48;
					switch (font)
					{
					case MathFontType.Roman:
						if (mathStyleType == MathStyleType.Bold)
						{
							num += 120782;
							break;
						}
						text2 += c;
						continue;
					case MathFontType.DoubleStruck:
						num += 120792;
						break;
					case MathFontType.SansSerif:
						num = ((mathStyleType != MathStyleType.Bold) ? (num + 120802) : (num + 120812));
						break;
					case MathFontType.Monospace:
						num += 120822;
						break;
					}
					text2 += char.ConvertFromUtf32(num);
					continue;
				}
				if (char.IsUpper(c))
				{
					num = num2 - 65;
					switch (font)
					{
					case MathFontType.Roman:
						num += 119808;
						break;
					case MathFontType.SansSerif:
						num += 120224;
						break;
					case MathFontType.Script:
						num += 119964;
						break;
					case MathFontType.Fraktur:
						num += 120068;
						break;
					case MathFontType.Monospace:
						num += 120432;
						break;
					case MathFontType.DoubleStruck:
						num += 120120;
						break;
					}
				}
				else if (char.IsLower(c))
				{
					num = num2 - 97;
					switch (font)
					{
					case MathFontType.Roman:
						num += 119834;
						break;
					case MathFontType.SansSerif:
						num += 120250;
						break;
					case MathFontType.Script:
						num += 119990;
						break;
					case MathFontType.Fraktur:
						num += 120094;
						break;
					case MathFontType.Monospace:
						num += 120458;
						break;
					case MathFontType.DoubleStruck:
						num += 120146;
						break;
					}
				}
				else if (char.IsPunctuation(c) || char.IsSymbol(c) || (char.IsWhiteSpace(c) && previousCharacter != ','))
				{
					text2 += c;
					continue;
				}
			}
			if (num == 0)
			{
				if (previousCharacter != ',' || !char.IsWhiteSpace(c))
				{
					text2 += c;
				}
				continue;
			}
			switch (font)
			{
			case MathFontType.DoubleStruck:
			case MathFontType.Monospace:
				if (font == MathFontType.DoubleStruck)
				{
					switch (c)
					{
					case 'C':
						num = 8450;
						break;
					case 'H':
						num = 8461;
						break;
					case 'N':
						num = 8469;
						break;
					case 'P':
						num = 8473;
						break;
					case 'Q':
						num = 8474;
						break;
					case 'R':
						num = 8477;
						break;
					case 'Z':
						num = 8484;
						break;
					}
				}
				text2 += char.ConvertFromUtf32(num);
				break;
			case MathFontType.Roman:
				text2 = mathStyleType switch
				{
					MathStyleType.Bold => text2 + char.ConvertFromUtf32(num), 
					MathStyleType.Italic => (c != 'h') ? (text2 + char.ConvertFromUtf32(num + num3)) : (text2 + char.ConvertFromUtf32(8462)), 
					MathStyleType.BoldItalic => text2 + char.ConvertFromUtf32(num + num3 * 2), 
					_ => text2 + c, 
				};
				break;
			case MathFontType.Fraktur:
			case MathFontType.Script:
				if (mathStyleType == MathStyleType.Bold)
				{
					text2 += char.ConvertFromUtf32(num + num3);
					break;
				}
				if (font == MathFontType.Script)
				{
					switch (c)
					{
					case 'B':
						num = 8492;
						break;
					case 'E':
						num = 8496;
						break;
					case 'F':
						num = 8497;
						break;
					case 'H':
						num = 8459;
						break;
					case 'I':
						num = 8464;
						break;
					case 'L':
						num = 8466;
						break;
					case 'M':
						num = 8499;
						break;
					case 'R':
						num = 8475;
						break;
					case 'e':
						num = 8495;
						break;
					case 'g':
						num = 8458;
						break;
					case 'o':
						num = 8500;
						break;
					}
				}
				else
				{
					switch (c)
					{
					case 'c':
						num = 8493;
						break;
					case 'H':
						num = 8460;
						break;
					case 'I':
						num = 8465;
						break;
					case 'R':
						num = 8476;
						break;
					case 'Z':
						num = 8488;
						break;
					}
				}
				text2 += char.ConvertFromUtf32(num);
				break;
			default:
				switch (mathStyleType)
				{
				case MathStyleType.Regular:
					text2 += char.ConvertFromUtf32(num);
					break;
				case MathStyleType.Bold:
					text2 += char.ConvertFromUtf32(num + num3);
					break;
				case MathStyleType.Italic:
					text2 = ((c != 'h') ? (text2 + char.ConvertFromUtf32(num + num3 * 2)) : (text2 + char.ConvertFromUtf32(8462)));
					break;
				case MathStyleType.BoldItalic:
					text2 += char.ConvertFromUtf32(num + num3 * 3);
					break;
				}
				break;
			}
		}
		textRange.Text = text2;
	}

	private bool ShouldSkipSpacingAfterOperator(char previousCharacter, char currentChar, ref char nextCharacter, int characterIndex, string text, int currentIndex, WTextRange textRange)
	{
		if ((currentChar.Equals('.') && char.IsNumber(nextCharacter) && (char.IsNumber(previousCharacter) || char.IsWhiteSpace(previousCharacter))) || (currentChar.Equals('.') && char.IsWhiteSpace(nextCharacter)) || (currentChar.Equals('.') && nextCharacter.Equals('.')) || (currentChar.Equals(',') && char.IsNumber(previousCharacter) && char.IsNumber(nextCharacter)) || (currentChar.Equals(',') && char.IsWhiteSpace(nextCharacter) && IsNeedToSkipSpaceCharacter(ref nextCharacter, characterIndex, text, currentIndex, textRange)))
		{
			return true;
		}
		return false;
	}

	private bool IsNeedToSkipSpaceCharacter(ref char nextCharacter, int characterIndex, string text, int currentIndex, WTextRange textRange)
	{
		nextCharacter = '\uffff';
		GetNextCharacter(ref nextCharacter, characterIndex, text, currentIndex, textRange);
		return nextCharacter == '\uffff';
	}

	private void GetNextCharacter(ref char nextCharacter, int characterIndex, string text, int currentIndex, WTextRange textRange)
	{
		if (characterIndex + 1 < text.Length)
		{
			nextCharacter = text[characterIndex + 1];
		}
		else
		{
			if (currentIndex + 1 >= (textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions.Count || !(textRange.OwnerMathRunElement.OwnerMathEntity is OfficeMath) || !((textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex + 1] is OfficeMathRunElement))
			{
				return;
			}
			OfficeMathRunElement officeMathRunElement = (textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex + 1] as OfficeMathRunElement;
			if (officeMathRunElement.Item is WTextRange)
			{
				WTextRange wTextRange = officeMathRunElement.Item as WTextRange;
				if (wTextRange.Text.Length > 0)
				{
					nextCharacter = wTextRange.Text[0];
				}
			}
		}
	}

	private void GetPreviousCharacter(ref char previousCharacter, int characterIndex, string text, int currentIndex, WTextRange textRange)
	{
		if (characterIndex > 0)
		{
			previousCharacter = text[characterIndex - 1];
		}
		else
		{
			if (currentIndex <= 0 || !(textRange.OwnerMathRunElement.OwnerMathEntity is OfficeMath) || !((textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex - 1] is OfficeMathRunElement))
			{
				return;
			}
			OfficeMathRunElement officeMathRunElement = (textRange.OwnerMathRunElement.OwnerMathEntity as OfficeMath).Functions[currentIndex - 1] as OfficeMathRunElement;
			if (officeMathRunElement.Item is WTextRange)
			{
				WTextRange wTextRange = officeMathRunElement.Item as WTextRange;
				if (wTextRange.Text.Length > 0)
				{
					previousCharacter = wTextRange.Text[wTextRange.Text.Length - 1];
				}
			}
		}
	}

	private int AdjustBaseValue(int charValue, int baseValue)
	{
		return charValue switch
		{
			977 => baseValue -= 5, 
			982 => baseValue -= 6, 
			981 => baseValue -= 7, 
			_ => baseValue, 
		};
	}

	internal bool IsOperatorSymbol(int charValue)
	{
		if ((charValue >= 8704 && charValue <= 8959) || (charValue >= 42 && charValue <= 46) || charValue == 247 || charValue == 215 || charValue == 177 || charValue == 183 || charValue == 8741 || charValue == 61 || charValue == 8729)
		{
			return true;
		}
		return false;
	}

	internal bool IsGreakCharacter(int charValue)
	{
		if (charValue >= 913 && charValue <= 989)
		{
			return true;
		}
		return false;
	}

	private ILayoutedFuntionWidget LayoutRunElement(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction, int currentIndex)
	{
		IOfficeMathRunElement officeMathRunElement = mathFunction as IOfficeMathRunElement;
		if (officeMathRunElement.Item is ILeafWidget)
		{
			LayoutedWidget layoutedWidget = null;
			if (officeMathRunElement.Item is WTextRange)
			{
				WTextRange wTextRange = officeMathRunElement.Item as WTextRange;
				if (wTextRange.OrignalText == string.Empty)
				{
					wTextRange.OrignalText = wTextRange.Text;
					UpdateTextByMathStyles(wTextRange, currentIndex);
				}
				layoutedWidget = new LayoutedWidget(wTextRange);
				SizeF size = layoutedWidget.Widget.LayoutInfo.Size;
				layoutedWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, size.Width, size.Height);
			}
			else
			{
				layoutedWidget = LayoutContext.Create(officeMathRunElement.Item as ILeafWidget, m_lcOperator, base.IsForceFitLayout).Layout(currentBounds);
			}
			if (layoutedWidget != null)
			{
				LayoutedOfficeRunWidget layoutedOfficeRunWidget = new LayoutedOfficeRunWidget(officeMathRunElement);
				layoutedOfficeRunWidget.LayoutedWidget = layoutedWidget;
				layoutedWidget.Owner = layoutedOfficeRunWidget;
				layoutedOfficeRunWidget.Bounds = layoutedWidget.Bounds;
				return layoutedOfficeRunWidget;
			}
		}
		return null;
	}

	private bool IsNested()
	{
		return MathLayoutingStack.Count > 1;
	}

	private bool HasNestedFunction(MathFunctionType functionType)
	{
		int num = 0;
		MathFunctionType[] array = MathLayoutingStack.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == functionType)
			{
				num++;
			}
			if (num > 1)
			{
				return true;
			}
		}
		return false;
	}

	private LayoutedRadicalWidget LayoutRadicalSwitch(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedRadicalWidget layoutedRadicalWidget = new LayoutedRadicalWidget(mathFunction);
		IOfficeMathRadical officeMathRadical = mathFunction as IOfficeMathRadical;
		layoutedRadicalWidget.Equation = LayoutOfficeMath(currentBounds, officeMathRadical.Equation);
		float num = 0f;
		if (!officeMathRadical.HideDegree)
		{
			num = 0.603f;
			if (officeMathRadical.Degree.ArgumentSize == 1)
			{
				num = 0.803f;
			}
			else if (officeMathRadical.Degree.ArgumentSize == 2)
			{
				num = 1f;
			}
			ReduceFontSizeOfOfficeMath(officeMathRadical.Degree, num);
			layoutedRadicalWidget.Degree = LayoutOfficeMath(currentBounds, officeMathRadical.Degree);
		}
		layoutedRadicalWidget.RadicalLines = GenerateRadicalLines(officeMathRadical, layoutedRadicalWidget.Equation.Bounds, out var radicalSymbolWidth);
		DocGen.Drawing.Font fontToRender = (officeMathRadical.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
		float width = base.DrawingContext.MeasureString("√", fontToRender, null, FontScriptType.English).Width;
		radicalSymbolWidth += width / 100f * 10f;
		float descent = base.DrawingContext.GetDescent(fontToRender, FontScriptType.English);
		float num2 = radicalSymbolWidth;
		float num3 = descent;
		if (!officeMathRadical.HideDegree)
		{
			float width2 = layoutedRadicalWidget.Degree.Bounds.Width;
			float height = layoutedRadicalWidget.Degree.Bounds.Height;
			WCharacterFormat wCharacterFormat = officeMathRadical.ControlProperties as WCharacterFormat;
			DocGen.Drawing.Font font = Document.FontSettings.GetFont(wCharacterFormat.GetFontNameToRender(FontScriptType.English), wCharacterFormat.GetFontSizeToRender() * num, FontStyle.Regular, FontScriptType.English);
			width2 -= layoutedRadicalWidget.RadicalLines[1].Point2.X - (currentBounds.X + width / 100f * 10f);
			if (width2 < 0f)
			{
				width2 = 0f;
			}
			height -= base.DrawingContext.GetDescent(font, FontScriptType.English);
			float num4 = layoutedRadicalWidget.RadicalLines[1].Point1.Y - layoutedRadicalWidget.RadicalLines[0].Point2.Y;
			if (height > num4)
			{
				height -= num4;
			}
			else
			{
				float num5 = num4 / 100f * 50f - height;
				if (num5 <= 0f)
				{
					height = 0f - num5;
				}
				else
				{
					height = 0f;
					layoutedRadicalWidget.Degree.ShiftXYPosition(0f, num5);
				}
			}
			ShiftLayoutedLineWidgetXYPosition(layoutedRadicalWidget.RadicalLines, width2, height);
			num2 += width2;
			num3 += height;
		}
		layoutedRadicalWidget.Equation.ShiftXYPosition(num2, num3);
		float width3 = layoutedRadicalWidget.Equation.Bounds.Right - currentBounds.X;
		float height2 = layoutedRadicalWidget.Equation.Bounds.Bottom - currentBounds.Y;
		layoutedRadicalWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, width3, height2);
		return layoutedRadicalWidget;
	}

	private void ShiftLayoutedLineWidgetXYPosition(LayoutedLineWidget[] layoutedLines, float shiftX, float shiftY)
	{
		for (int i = 0; i < layoutedLines.Length; i++)
		{
			layoutedLines[i].ShiftXYPosition(shiftX, shiftY);
		}
	}

	private LayoutedLineWidget[] GenerateRadicalLines(IOfficeMathRadical radical, RectangleF equationBounds, out float radicalSymbolWidth)
	{
		DocGen.Drawing.Font fontToRender = (radical.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
		float ascent = base.DrawingContext.GetAscent(fontToRender, FontScriptType.English);
		float descent = base.DrawingContext.GetDescent(fontToRender, FontScriptType.English);
		float num = (ascent - descent) / 10f;
		Color textColor = base.DrawingContext.GetTextColor(radical.ControlProperties as WCharacterFormat);
		float height = 0f;
		List<PointF> list = GenerateRadicalUpwardLine(equationBounds, num, fontToRender);
		List<PointF> list2 = GenerateRadicalDownwardLine(equationBounds, num, list, ref height, fontToRender);
		List<PointF> list3 = GenerateRadicalTopHorizontalLine(equationBounds, num, list);
		List<PointF> list4 = GenerateRadicalHook(equationBounds, num, list2, ref height, fontToRender);
		List<List<PointF>> list5 = new List<List<PointF>>();
		list5.Add(list);
		list5.Add(list2);
		list5.Add(list3);
		list5.Add(list4);
		float num2 = list3[0].X - list4[0].X;
		foreach (List<PointF> item in list5)
		{
			for (int i = 0; i < item.Count; i++)
			{
				PointF value = item[i];
				value.X += num2;
				item[i] = value;
			}
		}
		radicalSymbolWidth = num2;
		LayoutedLineWidget[] array = new LayoutedLineWidget[4];
		for (int j = 0; j < list5.Count; j++)
		{
			List<PointF> list6 = list5[j];
			LayoutedLineWidget layoutedLineWidget = new LayoutedLineWidget();
			layoutedLineWidget.Point1 = list6[0];
			layoutedLineWidget.Point2 = list6[1];
			layoutedLineWidget.Width = num;
			layoutedLineWidget.Color = textColor;
			array[j] = layoutedLineWidget;
		}
		return array;
	}

	private List<PointF> GenerateRadicalHook(RectangleF argumentBounds, float lineThickness, List<PointF> downwardLinePoints, ref float height, DocGen.Drawing.Font controlFont)
	{
		float ascent = base.DrawingContext.GetAscent(controlFont, FontScriptType.English);
		height -= ascent * 0.425f;
		float widthFromAngle = GetWidthFromAngle(height, DegreeIntoRadians(32.2825f), DegreeIntoRadians(57.7174f));
		float x = downwardLinePoints[0].X + lineThickness / 4f - widthFromAngle;
		float y = argumentBounds.Bottom - ascent * 0.425f;
		PointF item = new PointF(x, y);
		x = downwardLinePoints[0].X;
		y = downwardLinePoints[0].Y;
		PointF item2 = new PointF(x, y);
		return new List<PointF> { item, item2 };
	}

	private static List<PointF> GenerateRadicalTopHorizontalLine(RectangleF argumentBounds, float lineThickness, List<PointF> upwardLinePoints)
	{
		float y = argumentBounds.Top + lineThickness / 4f;
		PointF item = new PointF(upwardLinePoints[1].X, y);
		float x = item.X + argumentBounds.Width;
		PointF item2 = new PointF(x, item.Y);
		return new List<PointF> { item, item2 };
	}

	private List<PointF> GenerateRadicalDownwardLine(RectangleF argumentBounds, float lineThickness, List<PointF> upwardLinePoints, ref float height, DocGen.Drawing.Font controlFont)
	{
		height = base.DrawingContext.GetAscent(controlFont, FontScriptType.English) * 0.558f;
		float widthFromAngle = GetWidthFromAngle(height, DegreeIntoRadians(64.3483f), DegreeIntoRadians(25.6516f));
		float num = upwardLinePoints[0].X - widthFromAngle;
		float num2 = argumentBounds.Bottom - height;
		PointF item = new PointF(num - lineThickness / 4f, num2 + lineThickness / 4f);
		PointF item2 = new PointF(upwardLinePoints[0].X, upwardLinePoints[0].Y);
		return new List<PointF> { item, item2 };
	}

	private List<PointF> GenerateRadicalUpwardLine(RectangleF argumentBounds, float lineThickness, DocGen.Drawing.Font controlFont)
	{
		float height = base.DrawingContext.MeasureString("√", controlFont, null, FontScriptType.English).Height;
		float widthFromAngle = GetWidthFromAngle(height, DegreeIntoRadians(80.3856f), DegreeIntoRadians(9.6143f));
		float x = argumentBounds.Left - widthFromAngle - lineThickness / 2f;
		float y = argumentBounds.Bottom + lineThickness / 2f;
		PointF item = new PointF(x, y);
		x = argumentBounds.Left - lineThickness / 2f;
		y = argumentBounds.Top + lineThickness / 4f;
		PointF item2 = new PointF(x, y);
		return new List<PointF> { item, item2 };
	}

	private float GetWidthFromAngle(float height, double angle1, double angle2)
	{
		return (float)((double)height / Math.Sin(angle1) * Math.Sin(angle2));
	}

	private double DegreeIntoRadians(float angle)
	{
		return Math.PI * (double)angle / 180.0;
	}

	private LayoutedPhantomWidget LayoutPhantomSwitch(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedPhantomWidget layoutedPhantomWidget = new LayoutedPhantomWidget(mathFunction);
		IOfficeMathPhantom officeMathPhantom = mathFunction as IOfficeMathPhantom;
		layoutedPhantomWidget.Equation = LayoutOfficeMath(currentBounds, officeMathPhantom.Equation);
		layoutedPhantomWidget.Show = officeMathPhantom.Show;
		float width = 0f;
		if (!officeMathPhantom.ZeroWidth)
		{
			width = layoutedPhantomWidget.Equation.Bounds.Width;
		}
		layoutedPhantomWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, width, layoutedPhantomWidget.Equation.Bounds.Height);
		return layoutedPhantomWidget;
	}

	private LayoutedFractionWidget LayoutFractionSwitch(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedFractionWidget layoutedFractionWidget = new LayoutedFractionWidget(mathFunction);
		IOfficeMathFraction officeMathFraction = mathFunction as IOfficeMathFraction;
		bool flag = HasNestedFunction(MathFunctionType.Fraction);
		if ((flag && Document.Settings.MathProperties != null && Document.Settings.MathProperties.SmallFraction) || MathWidget.IsInline || (flag && officeMathFraction.FractionType == MathFractionType.NoFractionBar))
		{
			float fontSizeRatio = 0.725f;
			ReduceFontSizeOfOfficeMath(officeMathFraction.Numerator, fontSizeRatio);
			ReduceFontSizeOfOfficeMath(officeMathFraction.Denominator, fontSizeRatio);
		}
		layoutedFractionWidget.Numerator = LayoutOfficeMath(currentBounds, officeMathFraction.Numerator);
		layoutedFractionWidget.Denominator = LayoutOfficeMath(currentBounds, officeMathFraction.Denominator);
		DocGen.Drawing.Font fontToRender = (officeMathFraction.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
		float ascent = base.DrawingContext.GetAscent(fontToRender, FontScriptType.English);
		float descent = base.DrawingContext.GetDescent(fontToRender, FontScriptType.English);
		float num = currentBounds.X;
		float y = currentBounds.Y;
		y += layoutedFractionWidget.Numerator.Bounds.Height;
		if (officeMathFraction.FractionType != 0 && officeMathFraction.FractionType != MathFractionType.NoFractionBar)
		{
			num += layoutedFractionWidget.Numerator.Bounds.Width;
		}
		switch (officeMathFraction.FractionType)
		{
		case MathFractionType.NormalFractionBar:
		case MathFractionType.NoFractionBar:
		{
			float width = (ascent - descent) / 10f;
			if (!IsNested() && !MathWidget.IsInline)
			{
				y += descent / 2f;
			}
			float num2 = layoutedFractionWidget.Numerator.Bounds.Width;
			if (num2 < layoutedFractionWidget.Denominator.Bounds.Width)
			{
				num2 = layoutedFractionWidget.Denominator.Bounds.Width;
			}
			layoutedFractionWidget.FractionLine = new LayoutedLineWidget();
			layoutedFractionWidget.FractionLine.Width = width;
			layoutedFractionWidget.FractionLine.Color = base.DrawingContext.GetTextColor(officeMathFraction.ControlProperties as WCharacterFormat);
			layoutedFractionWidget.FractionLine.Point1 = new PointF(num, y);
			layoutedFractionWidget.FractionLine.Point2 = new PointF(num + num2, y);
			if (officeMathFraction.FractionType == MathFractionType.NoFractionBar)
			{
				layoutedFractionWidget.FractionLine.Skip = true;
			}
			y = (IsNested() ? (y + width) : (y + (descent / 2f + width)));
			layoutedFractionWidget.Denominator.ShiftXYPosition(0f, y - currentBounds.Y);
			if (layoutedFractionWidget.Numerator.Bounds.Width < layoutedFractionWidget.Denominator.Bounds.Width)
			{
				num = (num2 - layoutedFractionWidget.Numerator.Bounds.Width) / 2f;
				layoutedFractionWidget.Numerator.ShiftXYPosition(num, 0f);
			}
			else
			{
				num = (num2 - layoutedFractionWidget.Denominator.Bounds.Width) / 2f;
				layoutedFractionWidget.Denominator.ShiftXYPosition(num, 0f);
			}
			float width2 = num2;
			float height = layoutedFractionWidget.Denominator.Bounds.Bottom - layoutedFractionWidget.Numerator.Bounds.Y;
			layoutedFractionWidget.Bounds = new RectangleF(layoutedFractionWidget.Numerator.Bounds.X, layoutedFractionWidget.Numerator.Bounds.Y, width2, height);
			break;
		}
		case MathFractionType.SkewedFractionBar:
		{
			float num2 = layoutedFractionWidget.Numerator.Bounds.Height / 2f + layoutedFractionWidget.Denominator.Bounds.Height / 2f;
			float num3 = Transform(new PointF(num, y + num2 / 2f), num2 / 2f, 290f).X - num;
			float width = (ascent - descent) / 10f;
			layoutedFractionWidget.FractionLine = new LayoutedLineWidget();
			layoutedFractionWidget.FractionLine.Width = width;
			layoutedFractionWidget.FractionLine.Color = base.DrawingContext.GetTextColor(officeMathFraction.ControlProperties as WCharacterFormat);
			layoutedFractionWidget.FractionLine.Point1 = new PointF(num + num3, y - num2 / 2f);
			layoutedFractionWidget.FractionLine.Point2 = new PointF(num - num3, y + num2 / 2f);
			num += num3;
			layoutedFractionWidget.Denominator.ShiftXYPosition(num - currentBounds.X, y - currentBounds.Y);
			float width2 = layoutedFractionWidget.Denominator.Bounds.Right - layoutedFractionWidget.Numerator.Bounds.X;
			float height = layoutedFractionWidget.Denominator.Bounds.Bottom - layoutedFractionWidget.Numerator.Bounds.Y;
			layoutedFractionWidget.Bounds = new RectangleF(layoutedFractionWidget.Numerator.Bounds.X, layoutedFractionWidget.Numerator.Bounds.Y, width2, height);
			break;
		}
		case MathFractionType.FractionInline:
		{
			float num2 = layoutedFractionWidget.Numerator.Bounds.Height / 2f + layoutedFractionWidget.Denominator.Bounds.Height / 2f;
			float num3 = Transform(new PointF(0f, num2 / 2f), num2 / 2f, 290f).X - 0f;
			float width = (ascent - descent) / 10f;
			layoutedFractionWidget.FractionLine = new LayoutedLineWidget();
			layoutedFractionWidget.FractionLine.Width = width;
			layoutedFractionWidget.FractionLine.Color = base.DrawingContext.GetTextColor(officeMathFraction.ControlProperties as WCharacterFormat);
			layoutedFractionWidget.FractionLine.Point1 = new PointF(num + num3 * 2f, layoutedFractionWidget.Numerator.Bounds.Y);
			layoutedFractionWidget.FractionLine.Point2 = new PointF(num, layoutedFractionWidget.Numerator.Bounds.Y + num2);
			float num4 = 0f;
			if (layoutedFractionWidget.Numerator.Bounds.Height < layoutedFractionWidget.Denominator.Bounds.Height)
			{
				num4 = (layoutedFractionWidget.Denominator.Bounds.Height - layoutedFractionWidget.Numerator.Bounds.Height) / 2f;
				layoutedFractionWidget.Numerator.ShiftXYPosition(0f, num4);
				num4 = (layoutedFractionWidget.Denominator.Bounds.Height - num2) / 2f;
			}
			else
			{
				num4 = (layoutedFractionWidget.Numerator.Bounds.Height - layoutedFractionWidget.Denominator.Bounds.Height) / 2f;
				layoutedFractionWidget.Denominator.ShiftXYPosition(0f, num4);
				num4 = (layoutedFractionWidget.Numerator.Bounds.Height - num2) / 2f;
			}
			layoutedFractionWidget.FractionLine.Point1 = new PointF(layoutedFractionWidget.FractionLine.Point1.X, layoutedFractionWidget.FractionLine.Point1.Y + num4);
			layoutedFractionWidget.FractionLine.Point2 = new PointF(layoutedFractionWidget.FractionLine.Point2.X, layoutedFractionWidget.FractionLine.Point2.Y + num4);
			num += num3 * 2f;
			layoutedFractionWidget.Denominator.ShiftXYPosition(num - layoutedFractionWidget.Denominator.Bounds.X, 0f);
			float width2 = layoutedFractionWidget.Denominator.Bounds.Right - layoutedFractionWidget.Numerator.Bounds.X;
			float height = ((!(layoutedFractionWidget.Numerator.Bounds.Height < layoutedFractionWidget.Denominator.Bounds.Height)) ? layoutedFractionWidget.Numerator.Bounds.Height : layoutedFractionWidget.Denominator.Bounds.Height);
			float y2 = layoutedFractionWidget.Numerator.Bounds.Y;
			if (layoutedFractionWidget.Numerator.Bounds.Y > layoutedFractionWidget.Denominator.Bounds.Y)
			{
				y2 = layoutedFractionWidget.Denominator.Bounds.Y;
			}
			layoutedFractionWidget.Bounds = new RectangleF(layoutedFractionWidget.Numerator.Bounds.X, y2, width2, height);
			break;
		}
		}
		return layoutedFractionWidget;
	}

	private LayoutedDelimiterWidget LayoutDelimiterSwitch(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedDelimiterWidget layoutedDelimiterWidget = new LayoutedDelimiterWidget(mathFunction);
		IOfficeMathDelimiter officeMathDelimiter = mathFunction as IOfficeMathDelimiter;
		List<LayoutedOMathWidget> list = new List<LayoutedOMathWidget>();
		RectangleF clientActiveArea = currentBounds;
		WCharacterFormat wCharacterFormat = officeMathDelimiter.ControlProperties as WCharacterFormat;
		DocGen.Drawing.Font font = Document.FontSettings.GetFont(wCharacterFormat.GetFontNameToRender(FontScriptType.English), wCharacterFormat.GetFontSizeToRender(), FontStyle.Regular, FontScriptType.English);
		if (!string.IsNullOrEmpty(officeMathDelimiter.BeginCharacter))
		{
			LayoutedStringWidget layoutedStringWidget = new LayoutedStringWidget();
			layoutedStringWidget.Text = officeMathDelimiter.BeginCharacter[0].ToString();
			layoutedStringWidget.Font = font;
			SizeF sizeF = base.DrawingContext.MeasureString(layoutedStringWidget.Text, layoutedStringWidget.Font, base.DrawingContext.StringFormt, FontScriptType.English);
			layoutedStringWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, sizeF.Width, sizeF.Height);
			clientActiveArea.X += sizeF.Width;
			clientActiveArea.Width -= sizeF.Width;
			layoutedDelimiterWidget.BeginCharacter = layoutedStringWidget;
		}
		if (!string.IsNullOrEmpty(officeMathDelimiter.Seperator))
		{
			LayoutedStringWidget layoutedStringWidget2 = new LayoutedStringWidget();
			layoutedStringWidget2.Text = officeMathDelimiter.Seperator[0].ToString();
			layoutedStringWidget2.Font = font;
			SizeF sizeF2 = base.DrawingContext.MeasureString(layoutedStringWidget2.Text, layoutedStringWidget2.Font, base.DrawingContext.StringFormt, FontScriptType.English);
			layoutedStringWidget2.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, sizeF2.Width, sizeF2.Height);
			layoutedDelimiterWidget.Seperator = layoutedStringWidget2;
		}
		float num = 0f;
		float y = currentBounds.Y;
		int num2 = 0;
		for (int i = 0; i < officeMathDelimiter.Equation.Count; i++)
		{
			LayoutedOMathWidget layoutedOMathWidget = LayoutOfficeMath(clientActiveArea, officeMathDelimiter.Equation[i]);
			clientActiveArea.X += layoutedOMathWidget.Bounds.Width;
			clientActiveArea.Width -= layoutedOMathWidget.Bounds.Width;
			if (layoutedDelimiterWidget.Seperator != null && i != officeMathDelimiter.Equation.Count - 1)
			{
				clientActiveArea.X += layoutedDelimiterWidget.Seperator.Bounds.Width;
				clientActiveArea.Width -= layoutedDelimiterWidget.Seperator.Bounds.Width;
			}
			if (num < layoutedOMathWidget.Bounds.Height)
			{
				num = layoutedOMathWidget.Bounds.Height;
				num2 = i;
			}
			list.Add(layoutedOMathWidget);
		}
		float num3 = GetVerticalCenterPoint(list[num2]);
		float num4 = 0f;
		if (officeMathDelimiter.IsGrow && officeMathDelimiter.DelimiterShape == MathDelimiterShapeType.Centered)
		{
			num4 = list[num2].Bounds.Height - num3 * 2f;
			if (num4 > 0f)
			{
				num3 += num4;
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j != num2)
			{
				LayoutedOMathWidget layoutedOMathWidget2 = list[j];
				float verticalCenterPoint = GetVerticalCenterPoint(layoutedOMathWidget2);
				float yPosition = num3 - verticalCenterPoint;
				layoutedOMathWidget2.ShiftXYPosition(0f, yPosition);
				if (y > layoutedOMathWidget2.Bounds.Y)
				{
					y = layoutedOMathWidget2.Bounds.Y;
				}
			}
			else if (num4 > 0f)
			{
				list[j].ShiftXYPosition(0f, num4);
			}
		}
		if (y < currentBounds.Y)
		{
			float yPosition2 = currentBounds.Y - y;
			for (int k = 0; k < list.Count; k++)
			{
				list[k].ShiftXYPosition(0f, yPosition2, isSkipOwnerContainer: true);
			}
		}
		float num5 = 0f;
		for (int l = 0; l < list.Count; l++)
		{
			LayoutedOMathWidget layoutedOMathWidget3 = list[l];
			if (num5 < layoutedOMathWidget3.Bounds.Bottom)
			{
				num5 = layoutedOMathWidget3.Bounds.Bottom;
			}
		}
		if (!string.IsNullOrEmpty(officeMathDelimiter.EndCharacter))
		{
			LayoutedStringWidget layoutedStringWidget3 = new LayoutedStringWidget();
			layoutedStringWidget3.Text = officeMathDelimiter.EndCharacter[0].ToString();
			layoutedStringWidget3.Font = font;
			SizeF sizeF3 = base.DrawingContext.MeasureString(layoutedStringWidget3.Text, layoutedStringWidget3.Font, base.DrawingContext.StringFormt, FontScriptType.English);
			layoutedStringWidget3.Bounds = new RectangleF(clientActiveArea.X, clientActiveArea.Y, sizeF3.Width, sizeF3.Height);
			clientActiveArea.X += sizeF3.Width;
			layoutedDelimiterWidget.EndCharacter = layoutedStringWidget3;
		}
		if (layoutedDelimiterWidget.BeginCharacter != null)
		{
			if (!officeMathDelimiter.IsGrow || (officeMathDelimiter.IsGrow && !IsStretchableCharacter(officeMathDelimiter.BeginCharacter[0])))
			{
				float yPosition3 = num3 - layoutedDelimiterWidget.BeginCharacter.Bounds.Height / 2f;
				layoutedDelimiterWidget.BeginCharacter.ShiftXYPosition(0f, yPosition3);
			}
			else
			{
				layoutedDelimiterWidget.BeginCharacter.IsStretchable = true;
			}
		}
		if (layoutedDelimiterWidget.Seperator != null)
		{
			if (!officeMathDelimiter.IsGrow || (officeMathDelimiter.IsGrow && !IsStretchableCharacter(officeMathDelimiter.Seperator[0])))
			{
				float yPosition4 = num3 - layoutedDelimiterWidget.Seperator.Bounds.Height / 2f;
				layoutedDelimiterWidget.Seperator.ShiftXYPosition(0f, yPosition4);
			}
			else
			{
				layoutedDelimiterWidget.Seperator.IsStretchable = true;
			}
		}
		if (layoutedDelimiterWidget.EndCharacter != null)
		{
			if (!officeMathDelimiter.IsGrow || (officeMathDelimiter.IsGrow && !IsStretchableCharacter(officeMathDelimiter.EndCharacter[0])))
			{
				float yPosition5 = num3 - layoutedDelimiterWidget.EndCharacter.Bounds.Height / 2f;
				layoutedDelimiterWidget.EndCharacter.ShiftXYPosition(0f, yPosition5);
			}
			else
			{
				layoutedDelimiterWidget.EndCharacter.IsStretchable = true;
			}
		}
		if (num4 < 0f)
		{
			num5 += 0f - num4;
		}
		float width = clientActiveArea.X - currentBounds.X;
		layoutedDelimiterWidget.Equation = list;
		layoutedDelimiterWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, width, num5 - currentBounds.Y);
		return layoutedDelimiterWidget;
	}

	private bool IsStretchableCharacter(char character)
	{
		if (stretchableCharacters == null)
		{
			stretchableCharacters = new List<char>();
			stretchableCharacters.Add('(');
			stretchableCharacters.Add(')');
			stretchableCharacters.Add('[');
			stretchableCharacters.Add(']');
			stretchableCharacters.Add('{');
			stretchableCharacters.Add('}');
			stretchableCharacters.Add('⌊');
			stretchableCharacters.Add('⌋');
			stretchableCharacters.Add('⌈');
			stretchableCharacters.Add('⌉');
			stretchableCharacters.Add('|');
			stretchableCharacters.Add('‖');
			stretchableCharacters.Add('⟦');
			stretchableCharacters.Add('⟧');
			stretchableCharacters.Add('⟨');
			stretchableCharacters.Add('⟩');
		}
		return stretchableCharacters.Contains(character);
	}

	private LayoutedMathFunctionWidget LayoutMathFunctionWidget(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedMathFunctionWidget layoutedMathFunctionWidget = new LayoutedMathFunctionWidget(mathFunction);
		IOfficeMathFunction officeMathFunction = mathFunction as IOfficeMathFunction;
		layoutedMathFunctionWidget.FunctionName = LayoutOfficeMath(currentBounds, officeMathFunction.FunctionName);
		layoutedMathFunctionWidget.Equation = LayoutOfficeMath(currentBounds, officeMathFunction.Equation);
		DocGen.Drawing.Font fontToRender = (officeMathFunction.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
		SizeF sizeF = base.DrawingContext.MeasureString(" ", fontToRender, null, FontScriptType.English);
		float xPosition = layoutedMathFunctionWidget.FunctionName.Bounds.Width + sizeF.Width;
		float num = 0f;
		float num2 = 0f;
		float yPosition = 0f;
		if (layoutedMathFunctionWidget.FunctionName.Bounds.Height < layoutedMathFunctionWidget.Equation.Bounds.Height)
		{
			num = GetVerticalCenterPoint(layoutedMathFunctionWidget.Equation);
			num2 = GetVerticalCenterPoint(layoutedMathFunctionWidget.FunctionName);
			layoutedMathFunctionWidget.FunctionName.ShiftXYPosition(0f, num - num2);
		}
		else
		{
			num = GetVerticalCenterPoint(layoutedMathFunctionWidget.FunctionName);
			num2 = GetVerticalCenterPoint(layoutedMathFunctionWidget.Equation);
			yPosition = num - num2;
		}
		layoutedMathFunctionWidget.Equation.ShiftXYPosition(xPosition, yPosition);
		float width = layoutedMathFunctionWidget.Equation.Bounds.Right - layoutedMathFunctionWidget.FunctionName.Bounds.X;
		float height = layoutedMathFunctionWidget.FunctionName.Bounds.Height;
		if (height < layoutedMathFunctionWidget.Equation.Bounds.Height)
		{
			height = layoutedMathFunctionWidget.Equation.Bounds.Height;
		}
		layoutedMathFunctionWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, width, height);
		return layoutedMathFunctionWidget;
	}

	private LayoutedBoderBoxWidget LayoutBoderBoxWidget(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedBoderBoxWidget layoutedBoderBoxWidget = new LayoutedBoderBoxWidget(mathFunction);
		IOfficeMathBorderBox officeMathBorderBox = mathFunction as IOfficeMathBorderBox;
		layoutedBoderBoxWidget.Equation = LayoutOfficeMath(currentBounds, officeMathBorderBox.Equation);
		WCharacterFormat wCharacterFormat = officeMathBorderBox.ControlProperties as WCharacterFormat;
		DocGen.Drawing.Font fontToRender = wCharacterFormat.GetFontToRender(FontScriptType.English);
		RectangleF innerBounds = layoutedBoderBoxWidget.Equation.Bounds;
		float ascent = base.DrawingContext.GetAscent(fontToRender, FontScriptType.English);
		float descent = base.DrawingContext.GetDescent(fontToRender, FontScriptType.English);
		float num = (ascent - descent) / 10f;
		layoutedBoderBoxWidget.BorderLines = GenerateBorderBox(officeMathBorderBox, layoutedBoderBoxWidget, ref innerBounds, fontToRender, wCharacterFormat, num);
		layoutedBoderBoxWidget.Bounds = new RectangleF(innerBounds.X - num / 2f, innerBounds.Y - num / 2f, innerBounds.Width + num, innerBounds.Height + num);
		return layoutedBoderBoxWidget;
	}

	private LayoutedBarWidget LayoutBarWidget(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedBarWidget layoutedBarWidget = new LayoutedBarWidget(mathFunction);
		IOfficeMathBar officeMathBar = mathFunction as IOfficeMathBar;
		layoutedBarWidget.Equation = LayoutOfficeMath(currentBounds, officeMathBar.Equation);
		WCharacterFormat wCharacterFormat = officeMathBar.ControlProperties as WCharacterFormat;
		DocGen.Drawing.Font fontToRender = wCharacterFormat.GetFontToRender(FontScriptType.English);
		RectangleF innerBounds = layoutedBarWidget.Equation.Bounds;
		float ascent = base.DrawingContext.GetAscent(fontToRender, FontScriptType.English);
		float descent = base.DrawingContext.GetDescent(fontToRender, FontScriptType.English);
		float barWidth = (ascent - descent) / 10f;
		layoutedBarWidget.BarLine = GenerateBarline(officeMathBar, layoutedBarWidget, ref innerBounds, fontToRender, wCharacterFormat, barWidth);
		layoutedBarWidget.Bounds = innerBounds;
		return layoutedBarWidget;
	}

	private LayoutedMatrixWidget LayoutMatrixWidget(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedMatrixWidget layoutedMatrixWidget = new LayoutedMatrixWidget(mathFunction);
		IOfficeMathMatrix officeMathMatrix = mathFunction as IOfficeMathMatrix;
		layoutedMatrixWidget.Rows = new List<List<LayoutedOMathWidget>>();
		DocGen.Drawing.Font fontToRender = (officeMathMatrix.ControlProperties as WCharacterFormat).GetFontToRender(FontScriptType.English);
		float rowSpacing = GetRowSpacing(officeMathMatrix.RowSpacing, fontToRender, officeMathMatrix.RowSpacingRule);
		float columnSpacing = GetColumnSpacing(officeMathMatrix.ColumnSpacing, fontToRender, officeMathMatrix.ColumnSpacingRule);
		float x = currentBounds.X;
		float y = currentBounds.Y;
		float num = 0f;
		float num2 = 0f;
		float previousLowerHeight = 0f;
		int num3 = 0;
		List<LayoutedOMathWidget> list = null;
		LayoutedOMathWidget layoutedOMathWidget = null;
		for (int i = 0; i < officeMathMatrix.Rows.Count; i++)
		{
			IOfficeMaths arguments = officeMathMatrix.Rows[i].Arguments;
			list = new List<LayoutedOMathWidget>();
			float num4 = 0f;
			for (int j = 0; j < officeMathMatrix.Columns.Count; j++)
			{
				MathHorizontalAlignment horizontalAlignment = officeMathMatrix.Columns[j].HorizontalAlignment;
				LayoutedOMathWidget layoutedOMathWidget2 = LayoutOfficeMath(currentBounds, arguments[j]);
				float num5 = columnSpacing + officeMathMatrix.ColumnWidth;
				switch (horizontalAlignment)
				{
				case MathHorizontalAlignment.Center:
					layoutedOMathWidget2.ShiftXYPosition(officeMathMatrix.ColumnWidth / 2f - layoutedOMathWidget2.Bounds.Width / 2f, 0f);
					break;
				case MathHorizontalAlignment.Right:
					layoutedOMathWidget2.ShiftXYPosition(officeMathMatrix.ColumnWidth - layoutedOMathWidget2.Bounds.Width, 0f);
					break;
				}
				currentBounds.X += num5;
				list.Add(layoutedOMathWidget2);
				if (num4 < layoutedOMathWidget2.Bounds.Height)
				{
					num4 = layoutedOMathWidget2.Bounds.Height;
					num3 = j;
				}
			}
			AlignCellsVertically(list, num3, rowSpacing, ref currentBounds, officeMathMatrix, num4, ref previousLowerHeight);
			layoutedMatrixWidget.Rows.Add(list);
			currentBounds.X = x;
		}
		RectangleF previousMaxCellBounds = RectangleF.Empty;
		for (int k = 0; k < officeMathMatrix.Columns.Count; k++)
		{
			float num6 = 0f;
			int num7 = 0;
			MathHorizontalAlignment horizontalAlignment2 = officeMathMatrix.Columns[k].HorizontalAlignment;
			LayoutedOMathWidget layoutedOMathWidget3 = null;
			for (int l = 0; l < officeMathMatrix.Rows.Count; l++)
			{
				layoutedOMathWidget3 = layoutedMatrixWidget.Rows[l][k];
				if (num6 < layoutedOMathWidget3.Bounds.Width)
				{
					num6 = layoutedOMathWidget3.Bounds.Width;
					num7 = l;
				}
			}
			layoutedOMathWidget = layoutedMatrixWidget.Rows[num7][k];
			AlignCellsHorizontally(ref previousMaxCellBounds, x, k, layoutedOMathWidget, layoutedMatrixWidget, horizontalAlignment2, columnSpacing, num7, officeMathMatrix);
		}
		num = layoutedOMathWidget.Bounds.Right - x;
		num2 = list[num3].Bounds.Bottom - y;
		layoutedMatrixWidget.Bounds = new RectangleF(x, y, num, num2);
		return layoutedMatrixWidget;
	}

	internal void AlignCellsVertically(List<LayoutedOMathWidget> layoutedCellCollection, int maxHeightCellIndex, float rowSpacing, ref RectangleF currentBounds, IOfficeMathMatrix matrix, float maxRowHeight, ref float previousLowerHeight)
	{
		float num = layoutedCellCollection[maxHeightCellIndex].Bounds.Y + GetVerticalCenterPoint(layoutedCellCollection[maxHeightCellIndex]);
		float num2 = num - layoutedCellCollection[maxHeightCellIndex].Bounds.Y;
		float num3 = layoutedCellCollection[maxHeightCellIndex].Bounds.Bottom - num;
		if (num2 > rowSpacing || num3 > rowSpacing)
		{
			for (int i = 0; i < layoutedCellCollection.Count; i++)
			{
				if (i != maxHeightCellIndex)
				{
					layoutedCellCollection[i].ShiftXYPosition(0f, num2 - layoutedCellCollection[i].Bounds.Height / 2f);
				}
			}
			currentBounds.Y = layoutedCellCollection[maxHeightCellIndex].Bounds.Bottom;
		}
		else if (matrix.RowSpacingRule == SpacingRule.Single)
		{
			currentBounds.Y += maxRowHeight;
		}
		else
		{
			currentBounds.Y += rowSpacing;
		}
		previousLowerHeight = num3;
	}

	internal void AlignCellsHorizontally(ref RectangleF previousMaxCellBounds, float xPosition, int columnIndex, LayoutedOMathWidget maxCellWidget, LayoutedMatrixWidget layoutedMatrixWidget, MathHorizontalAlignment columnAlignment, float columnSpacing, int maxCellWidthIndex, IOfficeMathMatrix matrix)
	{
		float xPosition2 = 0f;
		float num = 0f;
		if (previousMaxCellBounds != RectangleF.Empty || maxCellWidget.Bounds.X < xPosition)
		{
			if (maxCellWidget.Bounds.X < xPosition && previousMaxCellBounds == RectangleF.Empty)
			{
				xPosition2 = xPosition - maxCellWidget.Bounds.X;
			}
			else if (maxCellWidget.Bounds.X < previousMaxCellBounds.Right)
			{
				xPosition2 = previousMaxCellBounds.Right - maxCellWidget.Bounds.X + columnSpacing;
			}
			maxCellWidget.ShiftXYPosition(xPosition2, 0f);
			num = maxCellWidget.Bounds.Width / 2f + maxCellWidget.Bounds.X;
			for (int i = 0; i < matrix.Rows.Count; i++)
			{
				if (i != maxCellWidthIndex)
				{
					LayoutedOMathWidget layoutedOMathWidget = layoutedMatrixWidget.Rows[i][columnIndex];
					switch (columnAlignment)
					{
					case MathHorizontalAlignment.Left:
						xPosition2 = maxCellWidget.Bounds.X - layoutedOMathWidget.Bounds.X;
						break;
					case MathHorizontalAlignment.Center:
						xPosition2 = num - (layoutedOMathWidget.Bounds.Width / 2f + layoutedOMathWidget.Bounds.X);
						break;
					case MathHorizontalAlignment.Right:
						xPosition2 = maxCellWidget.Bounds.Right - layoutedOMathWidget.Bounds.Right;
						break;
					}
					layoutedMatrixWidget.Rows[i][columnIndex].ShiftXYPosition(xPosition2, 0f);
				}
			}
		}
		previousMaxCellBounds = layoutedMatrixWidget.Rows[maxCellWidthIndex][columnIndex].Bounds;
	}

	internal LayoutedLineWidget GenerateBarline(IOfficeMathBar bar, LayoutedBarWidget layoutedBarWidget, ref RectangleF innerBounds, DocGen.Drawing.Font controlFont, WCharacterFormat characterFormat, float barWidth)
	{
		LayoutedLineWidget layoutedLineWidget = new LayoutedLineWidget();
		SizeF sizeF = base.DrawingContext.MeasureString(" ", controlFont, null, FontScriptType.English);
		Color textColor = base.DrawingContext.GetTextColor(characterFormat);
		RectangleF rectangleF = innerBounds;
		if (bar.BarTop)
		{
			layoutedLineWidget.Point1 = new PointF(rectangleF.Left, rectangleF.Top + sizeF.Width / 2f);
			layoutedLineWidget.Point2 = new PointF(rectangleF.Right, rectangleF.Top + sizeF.Width / 2f);
		}
		else
		{
			layoutedLineWidget.Point1 = new PointF(rectangleF.Left, rectangleF.Bottom);
			layoutedLineWidget.Point2 = new PointF(rectangleF.Right, rectangleF.Bottom);
		}
		layoutedBarWidget.Equation.ShiftXYPosition(0f, sizeF.Width);
		innerBounds.Height += sizeF.Width;
		layoutedLineWidget.Color = textColor;
		layoutedLineWidget.Width = barWidth;
		return layoutedLineWidget;
	}

	internal List<LayoutedLineWidget> GenerateBorderBox(IOfficeMathBorderBox borderBox, LayoutedBoderBoxWidget borderBoxWidget, ref RectangleF innerBounds, DocGen.Drawing.Font controlFont, WCharacterFormat characterFormat, float borderWidth)
	{
		List<LayoutedLineWidget> list = new List<LayoutedLineWidget>();
		SizeF sizeF = base.DrawingContext.MeasureString(" ", controlFont, null, FontScriptType.English);
		RectangleF rectangleF = innerBounds;
		Color textColor = base.DrawingContext.GetTextColor(characterFormat);
		if (!borderBox.HideRight)
		{
			rectangleF.Width += sizeF.Width * 2f;
			innerBounds.Width += sizeF.Width * 2f;
		}
		if (!borderBox.HideBottom && !IsNested())
		{
			rectangleF.Height += sizeF.Width * 2f;
			innerBounds.Height += sizeF.Width * 2f;
		}
		if (!borderBox.HideLeft)
		{
			PointF point = new PointF(rectangleF.Left, rectangleF.Top - borderWidth / 2f);
			PointF point2 = new PointF(rectangleF.Left, rectangleF.Bottom);
			LayoutedLineWidget layoutedLineWidget = new LayoutedLineWidget();
			layoutedLineWidget.Point1 = point;
			layoutedLineWidget.Point2 = point2;
			layoutedLineWidget.Color = textColor;
			layoutedLineWidget.Width = borderWidth;
			list.Add(layoutedLineWidget);
			borderBoxWidget.Equation.ShiftXYPosition(sizeF.Width, 0f);
			innerBounds.X += sizeF.Width;
		}
		if (!borderBox.HideTop)
		{
			PointF point3 = new PointF(rectangleF.Left, rectangleF.Top);
			PointF point4 = new PointF(rectangleF.Right, rectangleF.Top);
			LayoutedLineWidget layoutedLineWidget2 = new LayoutedLineWidget();
			layoutedLineWidget2.Point1 = point3;
			layoutedLineWidget2.Point2 = point4;
			layoutedLineWidget2.Color = textColor;
			layoutedLineWidget2.Width = borderWidth;
			list.Add(layoutedLineWidget2);
			if (!IsNested())
			{
				borderBoxWidget.Equation.ShiftXYPosition(0f, sizeF.Width);
				innerBounds.Y += sizeF.Width;
			}
			else
			{
				borderBoxWidget.Equation.ShiftXYPosition(0f, sizeF.Width * 0.5f);
				innerBounds.Y += sizeF.Width * 0.5f;
				if (IsNested())
				{
					innerBounds.Height += sizeF.Width;
				}
			}
		}
		if (!borderBox.HideRight)
		{
			PointF point5 = new PointF(rectangleF.Right, rectangleF.Top - borderWidth / 2f);
			PointF point6 = new PointF(rectangleF.Right, rectangleF.Bottom);
			LayoutedLineWidget layoutedLineWidget3 = new LayoutedLineWidget();
			layoutedLineWidget3.Point1 = point5;
			layoutedLineWidget3.Point2 = point6;
			layoutedLineWidget3.Color = textColor;
			layoutedLineWidget3.Width = borderWidth;
			list.Add(layoutedLineWidget3);
		}
		if (!borderBox.HideBottom)
		{
			PointF point7 = new PointF(rectangleF.Left, rectangleF.Bottom - borderWidth / 2f);
			PointF point8 = new PointF(rectangleF.Right, rectangleF.Bottom - borderWidth / 2f);
			LayoutedLineWidget layoutedLineWidget4 = new LayoutedLineWidget();
			layoutedLineWidget4.Point1 = point7;
			layoutedLineWidget4.Point2 = point8;
			layoutedLineWidget4.Color = textColor;
			layoutedLineWidget4.Width = borderWidth;
			list.Add(layoutedLineWidget4);
		}
		if (borderBox.StrikeHorizontal)
		{
			PointF point9 = new PointF(rectangleF.Left, rectangleF.Top + rectangleF.Height / 2f);
			PointF point10 = new PointF(rectangleF.Right, rectangleF.Top + rectangleF.Height / 2f);
			LayoutedLineWidget layoutedLineWidget5 = new LayoutedLineWidget();
			layoutedLineWidget5.Point1 = point9;
			layoutedLineWidget5.Point2 = point10;
			layoutedLineWidget5.Color = textColor;
			layoutedLineWidget5.Width = borderWidth;
			list.Add(layoutedLineWidget5);
		}
		if (borderBox.StrikeVertical)
		{
			PointF point11 = new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top);
			PointF point12 = new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Bottom);
			LayoutedLineWidget layoutedLineWidget6 = new LayoutedLineWidget();
			layoutedLineWidget6.Point1 = point11;
			layoutedLineWidget6.Point2 = point12;
			layoutedLineWidget6.Color = textColor;
			layoutedLineWidget6.Width = borderWidth;
			list.Add(layoutedLineWidget6);
		}
		if (borderBox.StrikeDiagonalDown)
		{
			PointF point13 = new PointF(rectangleF.Left, rectangleF.Top);
			PointF point14 = new PointF(rectangleF.Right, rectangleF.Bottom);
			LayoutedLineWidget layoutedLineWidget7 = new LayoutedLineWidget();
			layoutedLineWidget7.Point1 = point13;
			layoutedLineWidget7.Point2 = point14;
			layoutedLineWidget7.Color = textColor;
			layoutedLineWidget7.Width = borderWidth;
			list.Add(layoutedLineWidget7);
		}
		if (borderBox.StrikeDiagonalUp)
		{
			PointF point15 = new PointF(rectangleF.Right, rectangleF.Top);
			PointF point16 = new PointF(rectangleF.Left, rectangleF.Bottom);
			LayoutedLineWidget layoutedLineWidget8 = new LayoutedLineWidget();
			layoutedLineWidget8.Point1 = point15;
			layoutedLineWidget8.Point2 = point16;
			layoutedLineWidget8.Color = textColor;
			layoutedLineWidget8.Width = borderWidth;
			list.Add(layoutedLineWidget8);
		}
		return list;
	}

	private LayoutedEquationArrayWidget LayoutEquationArraySwitch(RectangleF currentBounds, LayoutedOMathWidget officeMathLayoutedWidget, IOfficeMathFunctionBase mathFunction)
	{
		LayoutedEquationArrayWidget layoutedEquationArrayWidget = new LayoutedEquationArrayWidget(mathFunction);
		IOfficeMathEquationArray officeMathEquationArray = mathFunction as IOfficeMathEquationArray;
		List<List<IOfficeMath>> list = new List<List<IOfficeMath>>();
		for (int i = 0; i < officeMathEquationArray.Equation.Count; i++)
		{
			IOfficeMath item = officeMathEquationArray.Equation[i];
			List<IOfficeMath> list2 = new List<IOfficeMath>();
			list2.Add(item);
			list.Add(list2);
		}
		int count = list.Count;
		if (officeMathEquationArray.ExpandEquationContainer)
		{
			SplitEquationArray(list, officeMathEquationArray);
		}
		int count2 = list[0].Count;
		bool flag = true;
		float num = 0f;
		HasNestedFunction(MathFunctionType.Fraction);
		WCharacterFormat wCharacterFormat = officeMathEquationArray.ControlProperties as WCharacterFormat;
		if (wCharacterFormat.ReducedFontSize == 1f)
		{
			flag = false;
			wCharacterFormat.ReducedFontSize = 0f;
		}
		List<List<LayoutedOMathWidget>> list3 = new List<List<LayoutedOMathWidget>>();
		for (int j = 0; j < officeMathEquationArray.Equation.Count; j++)
		{
			List<LayoutedOMathWidget> item2 = new List<LayoutedOMathWidget>();
			list3.Add(item2);
		}
		DocGen.Drawing.Font fontToRender = wCharacterFormat.GetFontToRender(FontScriptType.English);
		if (flag)
		{
			num = GetRowSpacing(officeMathEquationArray.RowSpacing, fontToRender, officeMathEquationArray.RowSpacingRule);
		}
		float height = base.DrawingContext.MeasureString(" ", fontToRender, null, FontScriptType.English).Height;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = currentBounds.Y;
		float num6 = currentBounds.Height;
		for (int k = 0; k < count; k++)
		{
			float x = currentBounds.X;
			float num7 = currentBounds.Width;
			float num8 = 0f;
			int index = 0;
			float num9 = 0f;
			int count3 = list[k].Count;
			for (int l = 0; l < count3; l++)
			{
				RectangleF clientActiveArea = new RectangleF(x, num5, num7, num6);
				LayoutedOMathWidget layoutedOMathWidget = LayoutOfficeMath(clientActiveArea, list[k][l]);
				list3[k].Add(layoutedOMathWidget);
				if (num8 < layoutedOMathWidget.Bounds.Height)
				{
					num8 += layoutedOMathWidget.Bounds.Height;
					index = l;
				}
				num7 -= layoutedOMathWidget.Bounds.Width;
			}
			float num10 = num8;
			float num11 = num;
			float verticalCenterPoint = GetVerticalCenterPoint(list3[k][index]);
			if (k != 1)
			{
				num11 -= height / 2f;
			}
			if (k > 0)
			{
				if (num5 > num4)
				{
					num11 -= num5 - num4;
				}
				num9 = num11 - verticalCenterPoint;
				if (num9 > 0f)
				{
					num10 += num9;
				}
				num4 = num5 + num;
			}
			else
			{
				num4 = num5 + verticalCenterPoint;
			}
			for (int m = 0; m < count3; m++)
			{
				LayoutedOMathWidget layoutedOMathWidget2 = list3[k][m];
				float verticalCenterPoint2 = GetVerticalCenterPoint(layoutedOMathWidget2);
				float num12 = verticalCenterPoint - verticalCenterPoint2;
				if (num9 > 0f)
				{
					num12 += num9;
				}
				layoutedOMathWidget2.ShiftXYPosition(0f, num12);
			}
			num2 += num10;
			num5 += num10;
			num6 -= num10;
		}
		float num13 = 0f;
		if (officeMathEquationArray.ExpandEquationContainer)
		{
			num13 = ((officeMathEquationArray.ExpandEquationContent && count2 != 1) ? (ContainerSize.Width / (float)(count2 - 1)) : (ContainerSize.Width / (float)(count2 + 1)));
		}
		for (int n = 0; n < count2; n++)
		{
			float num14 = 0f;
			float num15 = 0f;
			for (int num16 = 0; num16 < count; num16++)
			{
				if (num14 < list3[num16][n].Bounds.Width)
				{
					num14 = list3[num16][n].Bounds.Width;
				}
			}
			if (officeMathEquationArray.ExpandEquationContainer && (n != 0 || !officeMathEquationArray.ExpandEquationContent || count2 == 1))
			{
				num15 = ((!officeMathEquationArray.ExpandEquationContent) ? (num13 - num14 / 2f) : (num13 - num14));
			}
			for (int num17 = 0; num17 < count; num17++)
			{
				LayoutedOMathWidget layoutedOMathWidget3 = list3[num17][n];
				float num18 = (num14 - layoutedOMathWidget3.Bounds.Width) / 2f;
				num18 += num15 + num3;
				layoutedOMathWidget3.ShiftXYPosition(num18, 0f);
			}
			num3 += num14 + num15;
		}
		layoutedEquationArrayWidget.Equation = list3;
		layoutedEquationArrayWidget.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, num3, num2);
		return layoutedEquationArrayWidget;
	}

	private void SplitEquationArray(List<List<IOfficeMath>> splittedEquationArray, IOfficeMathEquationArray equationArray)
	{
		int count = splittedEquationArray.Count;
		string[] array = new string[1] { "&&" };
		for (int i = 0; i < count; i++)
		{
			List<IOfficeMath> list = splittedEquationArray[i];
			int num = list.Count;
			for (int j = 0; j < num; j++)
			{
				IOfficeMath officeMath = list[j];
				int count2 = officeMath.Functions.Count;
				for (int k = 0; k < count2; k++)
				{
					IOfficeMathFunctionBase officeMathFunctionBase = officeMath.Functions[k];
					if (!(officeMathFunctionBase is IOfficeMathRunElement) || !((officeMathFunctionBase as IOfficeMathRunElement).Item is WTextRange) || !((officeMathFunctionBase as IOfficeMathRunElement).Item as WTextRange).Text.Contains(array[0]))
					{
						continue;
					}
					WTextRange wTextRange = (officeMathFunctionBase as IOfficeMathRunElement).Item as WTextRange;
					string[] array2 = wTextRange.Text.Split(array, StringSplitOptions.None);
					float num2 = array2.Length;
					for (int l = 0; (float)l < num2; l++)
					{
						if (l == 0)
						{
							wTextRange.Text = array2[l];
							continue;
						}
						if (l == array2.Length - 1)
						{
							IOfficeMath officeMath2 = new OfficeMath(equationArray);
							OfficeMathRunElement obj = officeMath2.Functions.Add(MathFunctionType.RunElement) as OfficeMathRunElement;
							WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
							wTextRange2.Text = array2[l];
							obj.Item = wTextRange2;
							if (k + 1 < count2)
							{
								(officeMath.Functions as OfficeMathBaseCollection).CloneItemsTo(officeMath2.Functions as OfficeMathBaseCollection, k + 1, count2 - 1);
								int index = k + 1;
								while (k + 1 < officeMath.Functions.Count)
								{
									(officeMath.Functions as OfficeMathBaseCollection).Remove(officeMath.Functions[index]);
								}
							}
							list.Add(officeMath2);
							num++;
							break;
						}
						IOfficeMath officeMath3 = new OfficeMath(equationArray);
						OfficeMathRunElement obj2 = officeMath3.Functions.Add(MathFunctionType.RunElement) as OfficeMathRunElement;
						WTextRange wTextRange3 = wTextRange.Clone() as WTextRange;
						wTextRange3.Text = array2[l];
						obj2.Item = wTextRange3;
						list.Insert(j + 1, officeMath3);
						j++;
						num++;
					}
					break;
				}
			}
			if (i != 0 && num > splittedEquationArray[i - 1].Count)
			{
				int num3 = num - splittedEquationArray[i - 1].Count;
				for (int m = 0; m < i; m++)
				{
					for (int n = 0; n < num3; n++)
					{
						IOfficeMath officeMath4 = new OfficeMath(equationArray);
						(officeMath4.Functions.Add(MathFunctionType.RunElement) as OfficeMathRunElement).Item = new WTextRange(Document);
						splittedEquationArray[m].Add(officeMath4);
					}
				}
			}
			else if (i != 0 && num < splittedEquationArray[i - 1].Count)
			{
				int num4 = splittedEquationArray[i - 1].Count - num;
				for (int num5 = 0; num5 < num4; num5++)
				{
					IOfficeMath officeMath5 = new OfficeMath(equationArray);
					(officeMath5.Functions.Add(MathFunctionType.RunElement) as OfficeMathRunElement).Item = new WTextRange(Document);
					splittedEquationArray[i].Add(officeMath5);
				}
			}
		}
	}

	private float GetRowSpacing(float inputRowSpacing, DocGen.Drawing.Font font, SpacingRule spacingRule)
	{
		SizeF sizeF = base.DrawingContext.MeasureString(" ", font, null, FontScriptType.English);
		switch (spacingRule)
		{
		case SpacingRule.OneAndHalf:
		case SpacingRule.Double:
		case SpacingRule.Multiple:
			switch (spacingRule)
			{
			case SpacingRule.Double:
				inputRowSpacing = 2f;
				break;
			case SpacingRule.OneAndHalf:
				inputRowSpacing = 1.5f;
				break;
			}
			return sizeF.Height * inputRowSpacing;
		case SpacingRule.Exactly:
			return inputRowSpacing;
		default:
			return 0f;
		}
	}

	private float GetColumnSpacing(float columnSpacing, DocGen.Drawing.Font font, SpacingRule spacingRule)
	{
		float result = 0f;
		switch (spacingRule)
		{
		case SpacingRule.Single:
		case SpacingRule.OneAndHalf:
		case SpacingRule.Double:
		case SpacingRule.Multiple:
			columnSpacing = spacingRule switch
			{
				SpacingRule.Single => 1f, 
				SpacingRule.OneAndHalf => 1.5f, 
				SpacingRule.Double => 2f, 
				_ => columnSpacing / 12f, 
			};
			result = font.SizeInPoints * columnSpacing;
			break;
		case SpacingRule.Exactly:
			result = columnSpacing;
			break;
		}
		return result;
	}

	private void ReduceFontSizeOfOfficeMath(IOfficeMath officeMath, float fontSizeRatio)
	{
		for (int i = 0; i < officeMath.Functions.Count; i++)
		{
			IOfficeMathFunctionBase officeMathFunctionBase = officeMath.Functions[i];
			switch (officeMathFunctionBase.Type)
			{
			case MathFunctionType.Fraction:
			{
				IOfficeMathFraction officeMathFraction = officeMathFunctionBase as IOfficeMathFraction;
				ReduceFontSizeOfOfficeMath(officeMathFraction.Numerator, fontSizeRatio);
				ReduceFontSizeOfOfficeMath(officeMathFraction.Denominator, fontSizeRatio);
				break;
			}
			case MathFunctionType.Phantom:
			{
				IOfficeMathPhantom officeMathPhantom = officeMathFunctionBase as IOfficeMathPhantom;
				ReduceFontSizeOfOfficeMath(officeMathPhantom.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.Delimiter:
			{
				IOfficeMathDelimiter officeMathDelimiter = officeMathFunctionBase as IOfficeMathDelimiter;
				for (int k = 0; k < officeMathDelimiter.Equation.Count; k++)
				{
					ReduceFontSizeOfOfficeMath(officeMathDelimiter.Equation[k], fontSizeRatio);
				}
				break;
			}
			case MathFunctionType.Function:
			{
				IOfficeMathFunction officeMathFunction = officeMathFunctionBase as IOfficeMathFunction;
				ReduceFontSizeOfOfficeMath(officeMathFunction.FunctionName, fontSizeRatio);
				ReduceFontSizeOfOfficeMath(officeMathFunction.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.BorderBox:
			{
				IOfficeMathBorderBox officeMathBorderBox = officeMathFunctionBase as IOfficeMathBorderBox;
				ReduceFontSizeOfOfficeMath(officeMathBorderBox.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.EquationArray:
			{
				IOfficeMathEquationArray officeMathEquationArray = officeMathFunctionBase as IOfficeMathEquationArray;
				(officeMathEquationArray.ControlProperties as WCharacterFormat).ReducedFontSize = 1f;
				for (int j = 0; j < officeMathEquationArray.Equation.Count; j++)
				{
					ReduceFontSizeOfOfficeMath(officeMathEquationArray.Equation[j], fontSizeRatio);
				}
				break;
			}
			case MathFunctionType.Radical:
			{
				IOfficeMathRadical officeMathRadical = officeMathFunctionBase as IOfficeMathRadical;
				ReduceFontSizeOfOfficeMath(officeMathRadical.Equation, fontSizeRatio);
				if (!officeMathRadical.HideDegree)
				{
					ReduceFontSizeOfOfficeMath(officeMathRadical.Degree, fontSizeRatio);
				}
				break;
			}
			case MathFunctionType.Bar:
			{
				IOfficeMathBar officeMathBar = officeMathFunctionBase as IOfficeMathBar;
				ReduceFontSizeOfOfficeMath(officeMathBar.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.Matrix:
			{
				IOfficeMathMatrix officeMathMatrix = officeMathFunctionBase as IOfficeMathMatrix;
				for (int l = 0; l < officeMathMatrix.Rows.Count; l++)
				{
					for (int m = 0; m < officeMathMatrix.Columns.Count; m++)
					{
						ReduceFontSizeOfOfficeMath(officeMathMatrix.Rows[l].Arguments[m], fontSizeRatio);
					}
				}
				break;
			}
			case MathFunctionType.RunElement:
			{
				IOfficeMathRunElement officeMathRunElement = officeMathFunctionBase as IOfficeMathRunElement;
				if (officeMathRunElement.Item is WTextRange)
				{
					WTextRange obj = officeMathRunElement.Item as WTextRange;
					WCharacterFormat characterFormat = obj.CharacterFormat;
					obj.CharacterFormat.ReducedFontSize = ((characterFormat.Bidi || characterFormat.ComplexScript) ? characterFormat.FontSizeBidi : characterFormat.FontSize) * fontSizeRatio;
				}
				break;
			}
			case MathFunctionType.Limit:
			{
				IOfficeMathLimit officeMathLimit = officeMathFunctionBase as IOfficeMathLimit;
				ReduceFontSizeOfOfficeMath(officeMathLimit.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.Box:
			{
				IOfficeMathBox officeMathBox = officeMathFunctionBase as IOfficeMathBox;
				ReduceFontSizeOfOfficeMath(officeMathBox.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.SubSuperscript:
			{
				IOfficeMathScript officeMathScript = officeMathFunctionBase as IOfficeMathScript;
				ReduceFontSizeOfOfficeMath(officeMathScript.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.LeftSubSuperscript:
			{
				IOfficeMathLeftScript officeMathLeftScript = officeMathFunctionBase as IOfficeMathLeftScript;
				ReduceFontSizeOfOfficeMath(officeMathLeftScript.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.RightSubSuperscript:
			{
				IOfficeMathRightScript officeMathRightScript = officeMathFunctionBase as IOfficeMathRightScript;
				ReduceFontSizeOfOfficeMath(officeMathRightScript.Equation, fontSizeRatio);
				break;
			}
			case MathFunctionType.NArray:
			{
				IOfficeMathNArray officeMathNArray = officeMathFunctionBase as IOfficeMathNArray;
				ReduceFontSizeOfOfficeMath(officeMathNArray.Equation, fontSizeRatio);
				WCharacterFormat wCharacterFormat = officeMathNArray.ControlProperties as WCharacterFormat;
				wCharacterFormat.ReducedFontSize = ((wCharacterFormat.Bidi || wCharacterFormat.ComplexScript) ? wCharacterFormat.FontSizeBidi : wCharacterFormat.FontSize) * fontSizeRatio;
				break;
			}
			case MathFunctionType.Accent:
			{
				IOfficeMathAccent officeMathAccent = officeMathFunctionBase as IOfficeMathAccent;
				ReduceFontSizeOfOfficeMath(officeMathAccent.Equation, fontSizeRatio);
				WCharacterFormat wCharacterFormat = officeMathAccent.ControlProperties as WCharacterFormat;
				wCharacterFormat.ReducedFontSize = ((wCharacterFormat.Bidi || wCharacterFormat.ComplexScript) ? wCharacterFormat.FontSizeBidi : wCharacterFormat.FontSize) * fontSizeRatio;
				break;
			}
			case MathFunctionType.GroupCharacter:
			{
				IOfficeMathGroupCharacter officeMathGroupCharacter = officeMathFunctionBase as IOfficeMathGroupCharacter;
				ReduceFontSizeOfOfficeMath(officeMathGroupCharacter.Equation, fontSizeRatio);
				WCharacterFormat wCharacterFormat = officeMathGroupCharacter.ControlProperties as WCharacterFormat;
				wCharacterFormat.ReducedFontSize = ((wCharacterFormat.Bidi || wCharacterFormat.ComplexScript) ? wCharacterFormat.FontSizeBidi : wCharacterFormat.FontSize) * fontSizeRatio;
				break;
			}
			}
		}
	}

	private void AlignMathWidgetVertically(LayoutedMathWidget layoutedMathWidget)
	{
		List<LayoutedOMathWidget> childWidgets = layoutedMathWidget.ChildWidgets;
		if (childWidgets.Count <= 1)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < childWidgets.Count; i++)
		{
			LayoutedOMathWidget layoutedOMathWidget = childWidgets[i];
			if (num < layoutedOMathWidget.Bounds.Height)
			{
				num = layoutedOMathWidget.Bounds.Height;
				num2 = i;
			}
		}
		LayoutedOMathWidget officeMathWidget = childWidgets[num2];
		float verticalCenterPoint = GetVerticalCenterPoint(officeMathWidget);
		float y = layoutedMathWidget.Bounds.Y;
		for (int j = 0; j < childWidgets.Count; j++)
		{
			if (j != num2)
			{
				LayoutedOMathWidget layoutedOMathWidget2 = childWidgets[j];
				float verticalCenterPoint2 = GetVerticalCenterPoint(layoutedOMathWidget2);
				float yPosition = verticalCenterPoint - verticalCenterPoint2;
				layoutedOMathWidget2.ShiftXYPosition(0f, yPosition);
				if (y > layoutedOMathWidget2.Bounds.Y)
				{
					y = layoutedOMathWidget2.Bounds.Y;
				}
			}
		}
		if (y < layoutedMathWidget.Bounds.Y)
		{
			float yPosition2 = layoutedMathWidget.Bounds.Y - y;
			layoutedMathWidget.ShiftXYPosition(0f, yPosition2, isSkipOwnerContainer: true);
		}
		float maxBottom = GetMaxBottom(layoutedMathWidget);
		layoutedMathWidget.Bounds = new RectangleF(layoutedMathWidget.Bounds.X, layoutedMathWidget.Bounds.Y, layoutedMathWidget.Bounds.Width, maxBottom - layoutedMathWidget.Bounds.Y);
	}

	private void AlignOfficeMathWidgetVertically(LayoutedOMathWidget officeMathWidget)
	{
		if (officeMathWidget.ChildWidgets.Count <= 1)
		{
			return;
		}
		float num = 0f;
		float y = officeMathWidget.Bounds.Y;
		num = officeMathWidget.GetVerticalCenterPoint(out var maxHeightWidgetIndex);
		for (int i = 0; i < officeMathWidget.ChildWidgets.Count; i++)
		{
			if (i != maxHeightWidgetIndex)
			{
				ILayoutedFuntionWidget layoutedFuntionWidget = officeMathWidget.ChildWidgets[i];
				float verticalCenterPoint = officeMathWidget.GetVerticalCenterPoint(layoutedFuntionWidget);
				float yPosition = num - verticalCenterPoint;
				layoutedFuntionWidget.ShiftXYPosition(0f, yPosition);
				if (y > layoutedFuntionWidget.Bounds.Y)
				{
					y = layoutedFuntionWidget.Bounds.Y;
				}
			}
		}
		if (y < officeMathWidget.Bounds.Y)
		{
			float yPosition2 = officeMathWidget.Bounds.Y - y;
			officeMathWidget.ShiftXYPosition(0f, yPosition2, isSkipOwnerContainer: true);
		}
		float maxBottom = GetMaxBottom(officeMathWidget);
		officeMathWidget.Bounds = new RectangleF(officeMathWidget.Bounds.X, officeMathWidget.Bounds.Y, officeMathWidget.Bounds.Width, maxBottom - officeMathWidget.Bounds.Y);
	}

	private float GetMaxBottom(LayoutedOMathWidget officeMathWidget)
	{
		float num = 0f;
		for (int i = 0; i < officeMathWidget.ChildWidgets.Count; i++)
		{
			ILayoutedFuntionWidget layoutedFuntionWidget = officeMathWidget.ChildWidgets[i];
			if (num < layoutedFuntionWidget.Bounds.Bottom)
			{
				num = layoutedFuntionWidget.Bounds.Bottom;
			}
		}
		return num;
	}

	private float GetMaxBottom(LayoutedMathWidget mathWidget)
	{
		float num = 0f;
		for (int i = 0; i < mathWidget.ChildWidgets.Count; i++)
		{
			LayoutedOMathWidget layoutedOMathWidget = mathWidget.ChildWidgets[i];
			if (num < layoutedOMathWidget.Bounds.Bottom)
			{
				num = layoutedOMathWidget.Bounds.Bottom;
			}
		}
		return num;
	}

	private float GetVerticalCenterPoint(LayoutedOMathWidget officeMathWidget)
	{
		int maxHeightWidgetIndex;
		return officeMathWidget.GetVerticalCenterPoint(out maxHeightWidgetIndex);
	}

	private void LayoutOfficeMathCollection(RectangleF clientArea, LayoutedMathWidget layoutedMathWidget, IOfficeMaths officeMathCollection)
	{
		RectangleF clientActiveArea = new RectangleF(clientArea.Location, clientArea.Size);
		mathXPosition = clientArea.X;
		for (int i = 0; i < officeMathCollection.Count; i++)
		{
			IOfficeMath officeMath = officeMathCollection[i];
			LayoutedOMathWidget layoutedOMathWidget = LayoutOfficeMath(clientActiveArea, officeMath);
			if (i + 1 != officeMathCollection.Count)
			{
				IOfficeMath officeMath2 = officeMathCollection[i + 1];
				if (officeMath2.Functions.Count > 0 && (officeMath.Functions[officeMath.Functions.Count - 1].Type != MathFunctionType.RunElement || officeMath2.Functions[0].Type != MathFunctionType.RunElement))
				{
					WCharacterFormat controlCharacterProperty = GetControlCharacterProperty(officeMath.Functions[officeMath.Functions.Count - 1]);
					if (controlCharacterProperty != null)
					{
						float width = base.DrawingContext.MeasureString(" ", controlCharacterProperty.GetFontToRender(FontScriptType.English), null, FontScriptType.English).Width;
						layoutedOMathWidget.Bounds = new RectangleF(layoutedOMathWidget.Bounds.X, layoutedOMathWidget.Bounds.Y, layoutedOMathWidget.Bounds.Width + width, layoutedOMathWidget.Bounds.Height);
					}
				}
			}
			clientActiveArea.X += layoutedOMathWidget.Bounds.Width;
			clientActiveArea.Width -= layoutedOMathWidget.Bounds.Width;
			layoutedOMathWidget.Owner = layoutedMathWidget;
			layoutedMathWidget.Bounds = UpdateBounds(layoutedMathWidget.Bounds, layoutedOMathWidget.Bounds);
			layoutedMathWidget.ChildWidgets.Add(layoutedOMathWidget);
		}
		AlignMathWidgetVertically(layoutedMathWidget);
	}

	private LayoutedOMathWidget LayoutOfficeMath(RectangleF clientActiveArea, IOfficeMath officeMath)
	{
		LayoutedOMathWidget layoutedOMathWidget = CreateOMathLayoutedWidget(clientActiveArea.Location, officeMath);
		LayoutOfficeMathFunctions(clientActiveArea, layoutedOMathWidget, officeMath.Functions);
		return layoutedOMathWidget;
	}

	private LayoutedOMathWidget CreateOMathLayoutedWidget(PointF location, IOfficeMath officeMath)
	{
		LayoutedOMathWidget layoutedOMathWidget = new LayoutedOMathWidget(officeMath);
		RectangleF bounds = layoutedOMathWidget.Bounds;
		bounds.Location = location;
		layoutedOMathWidget.Bounds = bounds;
		return layoutedOMathWidget;
	}

	private LayoutedMathWidget CreateMathLayoutedWidget(PointF location)
	{
		LayoutedMathWidget layoutedMathWidget = new LayoutedMathWidget(base.Widget);
		RectangleF bounds = layoutedMathWidget.Bounds;
		bounds.Location = location;
		layoutedMathWidget.Bounds = bounds;
		return layoutedMathWidget;
	}

	private PointF Transform(PointF inputPoint, float length, float angle)
	{
		return new PointF(inputPoint.X + length * (float)Math.Cos((double)angle * Math.PI / 180.0), inputPoint.Y + length * (float)Math.Sin((double)angle * Math.PI / 180.0));
	}
}
