using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Layouting;
using DocGen.Office;
using DocGen.Pdf.Graphics;

namespace DocGen.DocToPdfConverter.Rendering;

internal class MathRenderer
{
	private PDFDrawingContext m_drawingContext;

	internal PDFDrawingContext DrawingContext => m_drawingContext;

	internal MathRenderer(PDFDrawingContext drawingContext)
	{
		m_drawingContext = drawingContext;
	}

	internal void Draw(WMath math, LayoutedWidget ltWidget)
	{
		LayoutedMathWidget layoutedMathWidget = ltWidget as LayoutedMathWidget;
		for (int i = 0; i < layoutedMathWidget.ChildWidgets.Count; i++)
		{
			LayoutedOMathWidget layoutedOMathWidget = layoutedMathWidget.ChildWidgets[i];
			Draw(layoutedOMathWidget);
		}
	}

	internal void Draw(LayoutedOMathWidget layoutedOMathWidget)
	{
		for (int i = 0; i < layoutedOMathWidget.ChildWidgets.Count; i++)
		{
			ILayoutedFuntionWidget layoutedFuntionWidget = layoutedOMathWidget.ChildWidgets[i];
			switch (layoutedFuntionWidget.Widget.Type)
			{
			case MathFunctionType.Fraction:
			{
				LayoutedFractionWidget layoutedFractionWidget = layoutedFuntionWidget as LayoutedFractionWidget;
				Draw(layoutedFractionWidget.Numerator);
				Draw(layoutedFractionWidget.FractionLine);
				Draw(layoutedFractionWidget.Denominator);
				break;
			}
			case MathFunctionType.Delimiter:
			{
				LayoutedDelimiterWidget layoutedDelimiterWidget = layoutedFuntionWidget as LayoutedDelimiterWidget;
				IOfficeMathDelimiter officeMathDelimiter = layoutedDelimiterWidget.Widget as IOfficeMathDelimiter;
				if (layoutedDelimiterWidget.BeginCharacter != null)
				{
					DrawDelimiterCharacter(layoutedDelimiterWidget.BeginCharacter, layoutedDelimiterWidget.Bounds.Height, officeMathDelimiter.ControlProperties as WCharacterFormat);
				}
				for (int l = 0; l < layoutedDelimiterWidget.Equation.Count; l++)
				{
					LayoutedOMathWidget layoutedOMathWidget2 = layoutedDelimiterWidget.Equation[l];
					Draw(layoutedOMathWidget2);
					if (layoutedDelimiterWidget.Seperator != null && l != layoutedDelimiterWidget.Equation.Count - 1)
					{
						layoutedDelimiterWidget.Seperator.Bounds = new RectangleF(layoutedOMathWidget2.Bounds.Right, layoutedDelimiterWidget.Seperator.Bounds.Y, layoutedDelimiterWidget.Seperator.Bounds.Width, layoutedDelimiterWidget.Seperator.Bounds.Height);
						DrawDelimiterCharacter(layoutedDelimiterWidget.Seperator, layoutedDelimiterWidget.Bounds.Height, officeMathDelimiter.ControlProperties as WCharacterFormat);
					}
				}
				if (layoutedDelimiterWidget.EndCharacter != null)
				{
					DrawDelimiterCharacter(layoutedDelimiterWidget.EndCharacter, layoutedDelimiterWidget.Bounds.Height, officeMathDelimiter.ControlProperties as WCharacterFormat);
				}
				break;
			}
			case MathFunctionType.Function:
			{
				LayoutedMathFunctionWidget layoutedMathFunctionWidget = layoutedFuntionWidget as LayoutedMathFunctionWidget;
				Draw(layoutedMathFunctionWidget.FunctionName);
				Draw(layoutedMathFunctionWidget.Equation);
				break;
			}
			case MathFunctionType.BorderBox:
			{
				LayoutedBoderBoxWidget layoutedBoderBoxWidget = layoutedFuntionWidget as LayoutedBoderBoxWidget;
				Draw(layoutedBoderBoxWidget.Equation);
				foreach (LayoutedLineWidget borderLine in layoutedBoderBoxWidget.BorderLines)
				{
					Draw(borderLine);
				}
				break;
			}
			case MathFunctionType.EquationArray:
			{
				LayoutedEquationArrayWidget layoutedEquationArrayWidget = layoutedFuntionWidget as LayoutedEquationArrayWidget;
				for (int j = 0; j < layoutedEquationArrayWidget.Equation.Count; j++)
				{
					List<LayoutedOMathWidget> list = layoutedEquationArrayWidget.Equation[j];
					for (int k = 0; k < list.Count; k++)
					{
						Draw(list[k]);
					}
				}
				break;
			}
			case MathFunctionType.Radical:
			{
				LayoutedRadicalWidget layoutedRadicalWidget = layoutedFuntionWidget as LayoutedRadicalWidget;
				Draw(layoutedRadicalWidget.Equation);
				if (layoutedRadicalWidget.Degree != null)
				{
					Draw(layoutedRadicalWidget.Degree);
				}
				LayoutedLineWidget[] radicalLines = layoutedRadicalWidget.RadicalLines;
				foreach (LayoutedLineWidget lineWidget in radicalLines)
				{
					Draw(lineWidget);
				}
				break;
			}
			case MathFunctionType.Bar:
			{
				LayoutedBarWidget layoutedBarWidget = layoutedFuntionWidget as LayoutedBarWidget;
				Draw(layoutedBarWidget.Equation);
				Draw(layoutedBarWidget.BarLine);
				break;
			}
			case MathFunctionType.Matrix:
			{
				LayoutedMatrixWidget layoutedMatrixWidget = layoutedFuntionWidget as LayoutedMatrixWidget;
				for (int n = 0; n < layoutedMatrixWidget.Rows.Count; n++)
				{
					List<LayoutedOMathWidget> list2 = layoutedMatrixWidget.Rows[n];
					for (int num = 0; num < list2.Count; num++)
					{
						Draw(list2[num]);
					}
				}
				break;
			}
			case MathFunctionType.RunElement:
			{
				LayoutedOfficeRunWidget layoutedOfficeRunWidget = layoutedFuntionWidget as LayoutedOfficeRunWidget;
				DrawingContext.Draw(layoutedOfficeRunWidget.LayoutedWidget, isHaveToInitLayoutInfo: true);
				break;
			}
			case MathFunctionType.LeftSubSuperscript:
			case MathFunctionType.SubSuperscript:
			case MathFunctionType.RightSubSuperscript:
			{
				LayoutedScriptWidget layoutedScriptWidget = layoutedFuntionWidget as LayoutedScriptWidget;
				if (layoutedFuntionWidget.Widget is IOfficeMathScript officeMathScript)
				{
					if (officeMathScript.ScriptType == MathScriptType.Superscript)
					{
						Draw(layoutedScriptWidget.Superscript);
					}
					else
					{
						Draw(layoutedScriptWidget.Subscript);
					}
				}
				else
				{
					Draw(layoutedScriptWidget.Superscript);
					Draw(layoutedScriptWidget.Subscript);
				}
				Draw(layoutedScriptWidget.Equation);
				break;
			}
			case MathFunctionType.Limit:
			{
				LayoutedLimitWidget layoutedLimitWidget = layoutedFuntionWidget as LayoutedLimitWidget;
				Draw(layoutedLimitWidget.Equation);
				Draw(layoutedLimitWidget.Limit);
				break;
			}
			case MathFunctionType.Phantom:
			{
				LayoutedPhantomWidget layoutedPhantomWidget = layoutedFuntionWidget as LayoutedPhantomWidget;
				if (layoutedPhantomWidget.Show)
				{
					Draw(layoutedPhantomWidget.Equation);
				}
				break;
			}
			case MathFunctionType.Box:
			{
				LayoutedBoxWidget layoutedBoxWidget = layoutedFuntionWidget as LayoutedBoxWidget;
				Draw(layoutedBoxWidget.Equation);
				break;
			}
			case MathFunctionType.NArray:
			{
				LayoutedNArrayWidget layoutedNArrayWidget = layoutedFuntionWidget as LayoutedNArrayWidget;
				IOfficeMathNArray officeMathNArray = layoutedNArrayWidget.Widget as IOfficeMathNArray;
				Draw(layoutedNArrayWidget.Equation);
				Draw(layoutedNArrayWidget.NArrayCharacter, officeMathNArray.ControlProperties as WCharacterFormat, 1f);
				if (!officeMathNArray.HideUpperLimit)
				{
					Draw(layoutedNArrayWidget.Superscript);
				}
				if (!officeMathNArray.HideLowerLimit)
				{
					Draw(layoutedNArrayWidget.Subscript);
				}
				break;
			}
			case MathFunctionType.Accent:
			{
				LayoutedAccentWidget layoutedAccentWidget = layoutedFuntionWidget as LayoutedAccentWidget;
				IOfficeMathAccent officeMathAccent = layoutedAccentWidget.Widget as IOfficeMathAccent;
				Draw(layoutedAccentWidget.Equation);
				RectangleF bounds2 = layoutedAccentWidget.AccentCharacter.Bounds;
				float scalingFactor2 = layoutedAccentWidget.ScalingFactor;
				if (scalingFactor2 != 1f)
				{
					float x2 = bounds2.X / scalingFactor2;
					layoutedAccentWidget.AccentCharacter.Bounds = new RectangleF(x2, bounds2.Y, bounds2.Width, bounds2.Height);
				}
				Draw(layoutedAccentWidget.AccentCharacter, officeMathAccent.ControlProperties as WCharacterFormat, scalingFactor2);
				break;
			}
			case MathFunctionType.GroupCharacter:
			{
				LayoutedGroupCharacterWidget layoutedGroupCharacterWidget = layoutedFuntionWidget as LayoutedGroupCharacterWidget;
				IOfficeMathGroupCharacter officeMathGroupCharacter = layoutedGroupCharacterWidget.Widget as IOfficeMathGroupCharacter;
				Draw(layoutedGroupCharacterWidget.Equation);
				RectangleF bounds = layoutedGroupCharacterWidget.GroupCharacter.Bounds;
				float scalingFactor = layoutedGroupCharacterWidget.ScalingFactor;
				if (scalingFactor != 1f)
				{
					float x = bounds.X / scalingFactor;
					layoutedGroupCharacterWidget.GroupCharacter.Bounds = new RectangleF(x, bounds.Y, bounds.Width, bounds.Height);
				}
				Draw(layoutedGroupCharacterWidget.GroupCharacter, officeMathGroupCharacter.ControlProperties as WCharacterFormat, scalingFactor);
				break;
			}
			}
		}
	}

	private void DrawDelimiterCharacter(LayoutedStringWidget delimiterCharacterWidget, float stretchableHeight, WCharacterFormat format)
	{
		if (delimiterCharacterWidget.IsStretchable)
		{
			RectangleF bounds = delimiterCharacterWidget.Bounds;
			RectangleF exactStringBounds = DrawingContext.GetExactStringBounds(delimiterCharacterWidget.Text, delimiterCharacterWidget.Font);
			float num = stretchableHeight / exactStringBounds.Height;
			if (num > 0f)
			{
				DrawingContext.SetScaleTransform(1f, num);
				float num2 = exactStringBounds.Y * num;
				float y = (bounds.Y - num2) / num;
				delimiterCharacterWidget.Bounds = new RectangleF(bounds.X, y, bounds.Width, bounds.Height);
			}
		}
		Draw(delimiterCharacterWidget, format, 1f);
		if (delimiterCharacterWidget.IsStretchable)
		{
			DrawingContext.ResetTransform();
		}
	}

	internal void Draw(LayoutedLineWidget lineWidget)
	{
		if (!lineWidget.Skip)
		{
			PdfPen pen = new PdfPen(new PdfColor(lineWidget.Color), lineWidget.Width);
			DrawingContext.PDFGraphics.DrawLine(pen, lineWidget.Point1, lineWidget.Point2);
		}
	}

	private void Draw(LayoutedStringWidget stringWidget, WCharacterFormat characterFormat, float scalingFactor)
	{
		if (scalingFactor != 1f)
		{
			Matrix matrix = new Matrix();
			matrix.Scale(scalingFactor, 1f);
			DrawingContext.PDFGraphics.Transform = matrix;
		}
		PdfStringFormat format = DrawingContext.PDFGraphics.ConvertFormat(DrawingContext.StringFormt);
		PdfBrush brush = new PdfSolidBrush(DrawingContext.GetTextColor(characterFormat));
		string text = DrawingContext.GetEmbedFontStyle(characterFormat);
		if (!string.IsNullOrEmpty(text))
		{
			text = characterFormat.GetFontNameToRender(FontScriptType.English) + "_" + text;
		}
		PdfFont pdfFont = ((DrawingContext.FontStreams == null || ((string.IsNullOrEmpty(text) || !DrawingContext.FontStreams.ContainsKey(text)) && (string.IsNullOrEmpty(characterFormat.GetFontNameToRender(FontScriptType.English)) || !DrawingContext.FontStreams.ContainsKey(characterFormat.GetFontNameToRender(FontScriptType.English))))) ? DrawingContext.CreatePdfFont(stringWidget.Text, WordDocument.RenderHelper.GetFontStream(stringWidget.Font, FontScriptType.English), stringWidget.Font.Size, DrawingContext.GetFontStyle(stringWidget.Font.Style)) : DrawingContext.CreatePdfFont(text, characterFormat.GetFontNameToRender(FontScriptType.English), stringWidget.Font.Size, DrawingContext.GetFontStyle(stringWidget.Font.Style)));
		pdfFont.Ascent = DrawingContext.GetAscent(stringWidget.Font, FontScriptType.English);
		DrawingContext.PDFGraphics.DrawString(stringWidget.Text, pdfFont, brush, stringWidget.Bounds, format, directConversion: true);
		if (scalingFactor != 1f)
		{
			DrawingContext.ResetTransform();
		}
	}

	internal void Dispose()
	{
		m_drawingContext = null;
	}
}
