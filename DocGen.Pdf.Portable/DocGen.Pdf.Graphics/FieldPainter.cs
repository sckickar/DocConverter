using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

internal class FieldPainter
{
	internal class CloudStyleArc
	{
		internal PointF point;

		internal float endAngle;

		internal float startAngle;
	}

	private static PdfBrush s_whiteBrush = null;

	private static PdfBrush s_blackBrush = null;

	private static PdfBrush s_silverBrush = null;

	private static PdfBrush s_grayBrush = null;

	private static Dictionary<string, PdfPen> s_pens = new Dictionary<string, PdfPen>();

	internal static bool isAutoFontSize = false;

	private static PdfStringFormat s_checkFieldFormat = null;

	private static PdfBrush WhiteBrush
	{
		get
		{
			lock (s_pens)
			{
				if (s_whiteBrush == null)
				{
					s_whiteBrush = PdfBrushes.White;
				}
				return s_whiteBrush;
			}
		}
	}

	private static PdfBrush BlackBrush
	{
		get
		{
			lock (s_pens)
			{
				if (s_blackBrush == null)
				{
					s_blackBrush = PdfBrushes.Black;
				}
				return s_blackBrush;
			}
		}
	}

	private static PdfBrush GrayBrush
	{
		get
		{
			lock (s_pens)
			{
				if (s_grayBrush == null)
				{
					s_grayBrush = PdfBrushes.Gray;
				}
				return s_grayBrush;
			}
		}
	}

	private static PdfBrush SilverBrush
	{
		get
		{
			lock (s_pens)
			{
				if (s_silverBrush == null)
				{
					s_silverBrush = PdfBrushes.Silver;
				}
				return s_silverBrush;
			}
		}
	}

	private static PdfStringFormat CheckFieldFormat
	{
		get
		{
			lock (s_pens)
			{
				if (s_checkFieldFormat == null)
				{
					s_checkFieldFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
				}
				return s_checkFieldFormat;
			}
		}
	}

	public static void DrawButton(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, PdfStringFormat format)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		DrawRectangularControl(g, paintParams);
		RectangleF layoutRectangle = paintParams.Bounds;
		PdfDictionary pdfDictionary = null;
		if (g.Layer != null && g.Page != null && !g.Page.Dictionary.ContainsKey("Rotate"))
		{
			pdfDictionary = new PdfDictionary();
			if ((g.Page.Dictionary["Parent"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
			{
				pdfDictionary2.ContainsKey("Rotate");
			}
		}
		_ = paintParams.RotationAngle;
		_ = 0;
		if ((g.Layer != null && g.Page.Rotation != 0) || paintParams.RotationAngle > 0)
		{
			PdfGraphicsState state = g.Save();
			if (g.Layer != null && g.Page.Rotation != 0)
			{
				if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					g.TranslateTransform(g.Size.Height, 0f);
					g.RotateTransform(90f);
					float y = g.Page.Size.Height - (layoutRectangle.X + layoutRectangle.Width);
					float y2 = layoutRectangle.Y;
					layoutRectangle = new RectangleF(y2, y, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					SizeF size = g.Size;
					float x = size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y3 = size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x, y3, layoutRectangle.Width, layoutRectangle.Height);
				}
				else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					g.TranslateTransform(0f, g.Size.Width);
					g.RotateTransform(270f);
					float x2 = g.Size.Width - (layoutRectangle.Y + layoutRectangle.Height);
					float x3 = layoutRectangle.X;
					layoutRectangle = new RectangleF(x2, x3, layoutRectangle.Height, layoutRectangle.Width);
				}
			}
			if (paintParams.RotationAngle > 0)
			{
				if (paintParams.RotationAngle == 90)
				{
					g.TranslateTransform(0f, g.Size.Height);
					g.RotateTransform(-90f);
					float x4 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					float x5 = layoutRectangle.X;
					layoutRectangle = new RectangleF(x4, x5, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.RotationAngle == 270)
				{
					g.TranslateTransform(g.Size.Width, 0f);
					g.RotateTransform(-270f);
					float y4 = layoutRectangle.Y;
					float y5 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					layoutRectangle = new RectangleF(y4, y5, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.RotationAngle == 180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					float x6 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y6 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x6, y6, layoutRectangle.Width, layoutRectangle.Height);
				}
			}
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
			g.Restore(state);
		}
		else
		{
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
		}
	}

	public static void DrawPressedButton(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, PdfStringFormat format)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (paintParams.BorderStyle == PdfBorderStyle.Inset)
		{
			g.DrawRectangle(paintParams.ShadowBrush, paintParams.Bounds);
		}
		else
		{
			g.DrawRectangle(paintParams.BackBrush, paintParams.Bounds);
		}
		DrawBorder(g, paintParams.Bounds, paintParams.BorderPen, paintParams.BorderStyle, paintParams.BorderWidth);
		RectangleF layoutRectangle = new RectangleF(paintParams.BorderWidth, paintParams.BorderWidth, paintParams.Bounds.Size.Width - paintParams.BorderWidth, paintParams.Bounds.Size.Height - paintParams.BorderWidth);
		g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
		switch (paintParams.BorderStyle)
		{
		case PdfBorderStyle.Inset:
			DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, GrayBrush);
			DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, SilverBrush);
			break;
		case PdfBorderStyle.Beveled:
			DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, paintParams.ShadowBrush);
			DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, WhiteBrush);
			break;
		default:
			DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, paintParams.ShadowBrush);
			break;
		}
	}

	internal static void DrawTextBox(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, PdfStringFormat format, bool multiLine, bool Scroll, int maxLength)
	{
		if (paintParams.InsertSpace)
		{
			float num = 0f;
			char[] array = text.ToCharArray();
			if (maxLength > 0)
			{
				num = paintParams.Bounds.Width / (float)maxLength;
				g.DrawRectangle(paintParams.BorderPen, paintParams.BackBrush, paintParams.Bounds);
				for (int i = 0; i < maxLength; i++)
				{
					if (format.Alignment == PdfTextAlignment.Right)
					{
						text = ((maxLength - array.Length > i) ? "" : array[i - (maxLength - array.Length)].ToString());
					}
					else if (format.Alignment != PdfTextAlignment.Center || array.Length >= maxLength)
					{
						text = ((array.Length <= i) ? "" : array[i].ToString());
					}
					else
					{
						int num2 = maxLength / 2 - (int)Math.Ceiling((double)array.Length / 2.0);
						text = ((i < num2 || i >= num2 + array.Length) ? "" : array[i - num2].ToString());
					}
					paintParams.Bounds = new RectangleF(paintParams.Bounds.X, paintParams.Bounds.Y, num, paintParams.Bounds.Height);
					PdfStringFormat pdfStringFormat = (PdfStringFormat)format.Clone();
					pdfStringFormat.Alignment = PdfTextAlignment.Center;
					DrawTextBox(g, paintParams, text, font, pdfStringFormat, multiLine, Scroll);
					paintParams.Bounds = new RectangleF(paintParams.Bounds.X + num, paintParams.Bounds.Y, num, paintParams.Bounds.Height);
					if (paintParams.BorderWidth != 0f)
					{
						g.DrawLine(paintParams.BorderPen, paintParams.Bounds.Location, new PointF(paintParams.Bounds.X, paintParams.Bounds.Y + paintParams.Bounds.Height));
					}
				}
			}
			else
			{
				DrawTextBox(g, paintParams, text, font, format, multiLine, Scroll);
			}
		}
		else
		{
			DrawTextBox(g, paintParams, text, font, format, multiLine, Scroll);
		}
	}

	public static void DrawTextBox(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, PdfStringFormat format, bool multiLine, bool scroll)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.Save();
			g.InitializeCoordinates();
		}
		if (!paintParams.InsertSpace)
		{
			DrawRectangularControl(g, paintParams);
		}
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.Restore();
			g.Save();
			g.StreamWriter.BeginMarkupSequence("Tx");
			g.InitializeCoordinates();
		}
		RectangleF layoutRectangle = paintParams.Bounds;
		if (paintParams.BorderStyle == PdfBorderStyle.Beveled || paintParams.BorderStyle == PdfBorderStyle.Inset)
		{
			layoutRectangle.X += 4f * paintParams.BorderWidth;
			layoutRectangle.Width -= 8f * paintParams.BorderWidth;
		}
		else
		{
			layoutRectangle.X += 2f * paintParams.BorderWidth;
			layoutRectangle.Width -= 4f * paintParams.BorderWidth;
		}
		if (multiLine)
		{
			float num = ((format == null || format.LineSpacing == 0f) ? font.Height : format.LineSpacing);
			bool num2 = format != null && format.SubSuperScript == PdfSubSuperScript.SubScript;
			float ascent = font.Metrics.GetAscent(format);
			float descent = font.Metrics.GetDescent(format);
			float num3 = (num2 ? (num - (font.Height + descent)) : (num - ascent));
			if (text.Contains("\n"))
			{
				if (layoutRectangle.Location == PointF.Empty && paintParams.isFlatten)
				{
					layoutRectangle.Y = 0f - (layoutRectangle.Y - num3);
				}
			}
			else if (layoutRectangle.Location == PointF.Empty || ((double)layoutRectangle.X == Math.Round(num3) && layoutRectangle.Y == 0f))
			{
				layoutRectangle.Y = 0f - (layoutRectangle.Y - num3);
			}
			if ((isAutoFontSize || (multiLine && paintParams.BorderWidth >= 3f)) && paintParams.BorderWidth != 0f)
			{
				layoutRectangle.Y += 2.5f * paintParams.BorderWidth;
			}
		}
		PdfDictionary pdfDictionary = null;
		if (g.Layer != null && g.Page != null && !g.Page.Dictionary.ContainsKey("Rotate"))
		{
			pdfDictionary = new PdfDictionary();
			if ((g.Page.Dictionary["Parent"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
			{
				pdfDictionary2.ContainsKey("Rotate");
			}
		}
		if ((g.Layer != null && g.Page.Rotation != 0) || (paintParams.RotationAngle > 0 && paintParams.isFlatten))
		{
			PdfGraphicsState state = g.Save();
			if (paintParams.PageRotationAngle != 0)
			{
				if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle90)
				{
					g.TranslateTransform(g.Size.Height, 0f);
					g.RotateTransform(90f);
					float y = g.Size.Height - (layoutRectangle.X + layoutRectangle.Width);
					float y2 = layoutRectangle.Y;
					layoutRectangle = new RectangleF(y2, y, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					SizeF size = g.Size;
					float x = size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y3 = size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x, y3, layoutRectangle.Width, layoutRectangle.Height);
				}
				else if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle270)
				{
					g.TranslateTransform(0f, g.Size.Width);
					g.RotateTransform(270f);
					float x2 = g.Size.Width - (layoutRectangle.Y + layoutRectangle.Height);
					float x3 = layoutRectangle.X;
					layoutRectangle = new RectangleF(x2, x3, layoutRectangle.Height, layoutRectangle.Width);
				}
			}
			if (paintParams.RotationAngle > 0)
			{
				if (paintParams.RotationAngle == 90)
				{
					if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle90)
					{
						g.TranslateTransform(0f, g.Size.Height);
						g.RotateTransform(-90f);
						float x4 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
						float x5 = layoutRectangle.X;
						layoutRectangle = new RectangleF(x4, x5, layoutRectangle.Height, layoutRectangle.Width);
					}
					else if (layoutRectangle.Width > layoutRectangle.Height)
					{
						g.TranslateTransform(0f, g.Size.Height);
						g.RotateTransform(-90f);
						layoutRectangle = new RectangleF(paintParams.Bounds.X, paintParams.Bounds.Y, paintParams.Bounds.Width, paintParams.Bounds.Height);
					}
					else
					{
						float x6 = layoutRectangle.X;
						layoutRectangle.X = 0f - (layoutRectangle.Y + layoutRectangle.Height);
						layoutRectangle.Y = x6;
						float height = layoutRectangle.Height;
						layoutRectangle.Height = ((layoutRectangle.Width > font.Height) ? layoutRectangle.Width : font.Height);
						layoutRectangle.Width = height;
						g.RotateTransform(-90f);
						layoutRectangle = new RectangleF(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height);
					}
				}
				else if (paintParams.RotationAngle == 270)
				{
					g.TranslateTransform(g.Size.Width, 0f);
					g.RotateTransform(-270f);
					float y4 = layoutRectangle.Y;
					float y5 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					layoutRectangle = new RectangleF(y4, y5, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.RotationAngle == 180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					float x7 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y6 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x7, y6, layoutRectangle.Width, layoutRectangle.Height);
				}
			}
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
			g.Restore(state);
		}
		else
		{
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
		}
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.StreamWriter.EndMarkupSequence();
			g.Restore();
		}
	}

	public static void DrawListBox(PdfGraphics g, PaintParams paintParams, PdfListFieldItemCollection items, int[] selectedItem, PdfFont font, PdfStringFormat stringFormat)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.Save();
			g.InitializeCoordinates();
		}
		DrawRectangularControl(g, paintParams);
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.Restore();
			g.Save();
			g.StreamWriter.BeginMarkupSequence("Tx");
			g.InitializeCoordinates();
		}
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			PdfListFieldItem pdfListFieldItem = items[i];
			PointF empty = PointF.Empty;
			float borderWidth = paintParams.BorderWidth;
			float num = 2f * borderWidth;
			float num2 = 2f;
			bool flag = paintParams.BorderStyle == PdfBorderStyle.Inset || paintParams.BorderStyle == PdfBorderStyle.Beveled;
			if (flag)
			{
				empty.X = 2f * num;
				empty.Y = (float)(i + 2) * borderWidth + font.Height * (float)i;
			}
			else
			{
				empty.X = num + num2;
				empty.Y = (float)(i + 1) * borderWidth + font.Height * (float)i + (num2 - 1f);
			}
			PdfBrush brush = paintParams.ForeBrush;
			RectangleF bounds = paintParams.Bounds;
			float num3 = bounds.Width - num;
			RectangleF rectangle = bounds;
			if (flag)
			{
				rectangle.Height -= num;
			}
			else
			{
				rectangle.Height -= borderWidth;
			}
			g.SetClip(rectangle, PdfFillMode.Winding);
			bool flag2 = false;
			for (int j = 0; j < selectedItem.Length; j++)
			{
				if (selectedItem[j] == i)
				{
					flag2 = true;
				}
			}
			if (paintParams.RotationAngle == 0 && flag2)
			{
				float num4 = bounds.X + borderWidth;
				if (flag)
				{
					num4 += borderWidth;
					num3 -= num;
				}
				brush = new PdfSolidBrush(new PdfColor(byte.MaxValue, 153, 193, 218));
				g.DrawRectangle(brush, num4, empty.Y, num3, font.Height);
				brush = new PdfSolidBrush(new PdfColor(byte.MaxValue, 0, 0, 0));
			}
			string s = ((pdfListFieldItem.Text != null) ? pdfListFieldItem.Text : pdfListFieldItem.Value);
			RectangleF layoutRectangle = new RectangleF(empty.X, empty.Y, num3 - empty.X, font.Height);
			PdfDictionary pdfDictionary = null;
			if (g.Layer != null && g.Page != null && !g.Page.Dictionary.ContainsKey("Rotate"))
			{
				pdfDictionary = new PdfDictionary();
				if ((g.Page.Dictionary["Parent"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
				{
					pdfDictionary2.ContainsKey("Rotate");
				}
			}
			_ = paintParams.RotationAngle;
			_ = 0;
			if ((g.Layer != null && g.Page.Rotation != 0) || paintParams.RotationAngle > 0)
			{
				PdfGraphicsState state = g.Save();
				if (g.Layer != null && g.Page.Rotation != 0)
				{
					if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
					{
						g.TranslateTransform(g.Size.Height, 0f);
						g.RotateTransform(90f);
						float y = g.Page.Size.Height - (rectangle.X + rectangle.Width);
						float y2 = rectangle.Y;
						rectangle = new RectangleF(y2, y, rectangle.Height + rectangle.Width, rectangle.Width + rectangle.Height);
					}
					else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
					{
						g.TranslateTransform(g.Size.Width, g.Size.Height);
						g.RotateTransform(-180f);
						SizeF size = g.Page.Size;
						float x = size.Width - (rectangle.X + rectangle.Width);
						float y3 = size.Height - (rectangle.Y + rectangle.Height);
						rectangle = new RectangleF(x, y3, rectangle.Width, rectangle.Height);
					}
					else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
					{
						g.TranslateTransform(0f, g.Size.Width);
						g.RotateTransform(270f);
						float x2 = g.Page.Size.Width - (rectangle.Y + rectangle.Height);
						float x3 = rectangle.X;
						rectangle = new RectangleF(x2, x3, rectangle.Height + rectangle.Width, rectangle.Width + rectangle.Height);
					}
				}
				if (paintParams.RotationAngle > 0)
				{
					if (paintParams.RotationAngle == 90)
					{
						g.TranslateTransform(0f, g.Size.Height);
						g.RotateTransform(-90f);
						float x4 = g.Size.Height - (rectangle.Y + rectangle.Height);
						float x5 = rectangle.X;
						rectangle = new RectangleF(x4, x5, rectangle.Height + rectangle.Width, rectangle.Width);
					}
					else if (paintParams.RotationAngle == 270)
					{
						g.TranslateTransform(g.Size.Width, 0f);
						g.RotateTransform(-270f);
						float y4 = rectangle.Y;
						float y5 = g.Size.Width - (rectangle.X + rectangle.Width);
						rectangle = new RectangleF(y4, y5, rectangle.Height + rectangle.Width, rectangle.Width);
					}
					else if (paintParams.RotationAngle == 180)
					{
						g.TranslateTransform(g.Size.Width, g.Size.Height);
						g.RotateTransform(-180f);
						float x6 = g.Size.Width - (rectangle.X + rectangle.Width);
						float y6 = g.Size.Height - (rectangle.Y + rectangle.Height);
						rectangle = new RectangleF(x6, y6, rectangle.Width, rectangle.Height);
					}
				}
				if (flag2)
				{
					float num5 = bounds.X + borderWidth;
					if (flag)
					{
						num5 += borderWidth;
						num3 -= num;
					}
					brush = new PdfSolidBrush(new PdfColor(byte.MaxValue, 153, 193, 218));
					g.DrawRectangle(brush, num5, empty.Y, num3, font.Height);
					brush = new PdfSolidBrush(new PdfColor(byte.MaxValue, 0, 0, 0));
				}
				g.DrawString(s, font, brush, layoutRectangle, stringFormat);
				g.Restore(state);
			}
			else
			{
				g.DrawString(s, font, brush, layoutRectangle, stringFormat);
			}
		}
		if (g.IsTemplateGraphics && paintParams.IsRequired)
		{
			g.StreamWriter.EndMarkupSequence();
			g.Restore();
		}
	}

	public static void DrawCheckBox(PdfGraphics g, PaintParams paintParams, string checkSymbol, PdfCheckFieldState state)
	{
		DrawCheckBox(g, paintParams, checkSymbol, state, null);
	}

	public static void DrawCheckBox(PdfGraphics g, PaintParams paintParams, string checkSymbol, PdfCheckFieldState state, PdfFont font)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		if (checkSymbol == null)
		{
			throw new ArgumentNullException("checkSymbol");
		}
		switch (state)
		{
		case PdfCheckFieldState.Unchecked:
		case PdfCheckFieldState.Checked:
			if (paintParams.BorderPen != null && paintParams.BorderPen.Color.A != 0)
			{
				g.DrawRectangle(paintParams.BackBrush, paintParams.Bounds);
			}
			break;
		case PdfCheckFieldState.PressedUnchecked:
		case PdfCheckFieldState.PressedChecked:
			if (paintParams.BorderStyle == PdfBorderStyle.Beveled || paintParams.BorderStyle == PdfBorderStyle.Underline)
			{
				if (paintParams.BorderPen != null && paintParams.BorderPen.Color.A != 0)
				{
					g.DrawRectangle(paintParams.BackBrush, paintParams.Bounds);
				}
			}
			else if (paintParams.BorderPen != null && paintParams.BorderPen.Color.A != 0)
			{
				g.DrawRectangle(paintParams.ShadowBrush, paintParams.Bounds);
			}
			break;
		}
		RectangleF rectangleF = paintParams.Bounds;
		DrawBorder(g, paintParams.Bounds, paintParams.BorderPen, paintParams.BorderStyle, paintParams.BorderWidth);
		if (state == PdfCheckFieldState.PressedChecked || state == PdfCheckFieldState.PressedUnchecked)
		{
			switch (paintParams.BorderStyle)
			{
			case PdfBorderStyle.Inset:
				DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, BlackBrush);
				DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, WhiteBrush);
				break;
			case PdfBorderStyle.Beveled:
				DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, paintParams.ShadowBrush);
				DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, WhiteBrush);
				break;
			}
		}
		else
		{
			switch (paintParams.BorderStyle)
			{
			case PdfBorderStyle.Inset:
				DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, GrayBrush);
				DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, SilverBrush);
				break;
			case PdfBorderStyle.Beveled:
				DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, WhiteBrush);
				DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, paintParams.ShadowBrush);
				break;
			}
		}
		float num = 0f;
		float num2 = 0f;
		if (state != PdfCheckFieldState.Checked && state != PdfCheckFieldState.PressedChecked)
		{
			return;
		}
		if (font == null)
		{
			bool num3 = paintParams.BorderStyle == PdfBorderStyle.Beveled || paintParams.BorderStyle == PdfBorderStyle.Inset;
			float num4 = paintParams.BorderWidth;
			if (num3)
			{
				num4 *= 2f;
			}
			float val = (num3 ? (2f * paintParams.BorderWidth) : paintParams.BorderWidth);
			val = Math.Max(val, 1f);
			float num5 = Math.Min(num4, val);
			num2 = ((paintParams.Bounds.Width > paintParams.Bounds.Height) ? paintParams.Bounds.Height : paintParams.Bounds.Width);
			float size = num2 - 2f * num5;
			font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, size);
			if (paintParams.Bounds.Width > paintParams.Bounds.Height)
			{
				num = (paintParams.Bounds.Height - font.Height) / 2f;
			}
		}
		else
		{
			font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, font.Size);
		}
		if (num2 == 0f)
		{
			num2 = paintParams.Bounds.Height;
		}
		if (num2 < font.Size)
		{
			throw new Exception("Font size cannot be greater than CheckBox height");
		}
		PdfDictionary pdfDictionary = null;
		if (g.Layer != null && g.Page != null && !g.Page.Dictionary.ContainsKey("Rotate"))
		{
			pdfDictionary = new PdfDictionary();
			if ((g.Page.Dictionary["Parent"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
			{
				pdfDictionary2.ContainsKey("Rotate");
			}
		}
		if (paintParams.PageRotationAngle != 0 || paintParams.RotationAngle > 0)
		{
			PdfGraphicsState state2 = g.Save();
			if (paintParams.PageRotationAngle != 0)
			{
				if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle90)
				{
					g.TranslateTransform(g.Size.Height, 0f);
					g.RotateTransform(90f);
					float y = g.Size.Height - (rectangleF.X + rectangleF.Width);
					float y2 = rectangleF.Y;
					rectangleF = new RectangleF(y2, y, rectangleF.Height, rectangleF.Width);
				}
				else if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					SizeF size2 = g.Size;
					float x = size2.Width - (rectangleF.X + rectangleF.Width);
					float y3 = size2.Height - (rectangleF.Y + rectangleF.Height);
					rectangleF = new RectangleF(x, y3, rectangleF.Width, rectangleF.Height);
				}
				else if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle270)
				{
					g.TranslateTransform(0f, g.Size.Width);
					g.RotateTransform(270f);
					float x2 = g.Size.Width - (rectangleF.Y + rectangleF.Height);
					float x3 = rectangleF.X;
					rectangleF = new RectangleF(x2, x3, rectangleF.Height, rectangleF.Width);
				}
			}
			if (paintParams.RotationAngle > 0)
			{
				if (paintParams.RotationAngle == 90)
				{
					if (paintParams.PageRotationAngle == PdfPageRotateAngle.RotateAngle90)
					{
						g.TranslateTransform(0f, g.Size.Height);
						g.RotateTransform(-90f);
						float x4 = g.Size.Height - (rectangleF.Y + rectangleF.Height);
						float x5 = rectangleF.X;
						rectangleF = new RectangleF(x4, x5, rectangleF.Height, rectangleF.Width);
					}
					else if (rectangleF.Width > rectangleF.Height)
					{
						g.TranslateTransform(0f, g.Size.Height);
						g.RotateTransform(-90f);
						rectangleF = new RectangleF(paintParams.Bounds.X, paintParams.Bounds.Y, paintParams.Bounds.Width, paintParams.Bounds.Height);
					}
					else
					{
						float x6 = rectangleF.X;
						rectangleF.X = 0f - (rectangleF.Y + rectangleF.Height);
						rectangleF.Y = x6;
						float height = rectangleF.Height;
						rectangleF.Height = ((rectangleF.Width > font.Height) ? rectangleF.Width : font.Height);
						rectangleF.Width = height;
						g.RotateTransform(-90f);
						rectangleF = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
					}
				}
				else if (paintParams.RotationAngle == 270)
				{
					g.TranslateTransform(g.Size.Width, 0f);
					g.RotateTransform(-270f);
					float y4 = rectangleF.Y;
					float y5 = g.Size.Width - (rectangleF.X + rectangleF.Width);
					rectangleF = new RectangleF(y4, y5, rectangleF.Height, rectangleF.Width);
				}
				else if (paintParams.RotationAngle == 180)
				{
					g.TranslateTransform(g.Size.Width, g.Size.Height);
					g.RotateTransform(-180f);
					float x7 = g.Size.Width - (rectangleF.X + rectangleF.Width);
					float y6 = g.Size.Height - (rectangleF.Y + rectangleF.Height);
					rectangleF = new RectangleF(x7, y6, rectangleF.Width, rectangleF.Height);
				}
			}
			g.DrawString(checkSymbol, font, paintParams.ForeBrush, new RectangleF(rectangleF.X, rectangleF.Y - num, rectangleF.Width, rectangleF.Height), CheckFieldFormat);
			g.Restore(state2);
		}
		else
		{
			g.DrawString(checkSymbol, font, paintParams.ForeBrush, new RectangleF(rectangleF.X, rectangleF.Y - num, rectangleF.Width, rectangleF.Height), CheckFieldFormat);
		}
	}

	public static void DrawComboBox(PdfGraphics g, PaintParams paintParams)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		DrawRectangularControl(g, paintParams);
	}

	public static void DrawComboBox(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, PdfStringFormat format)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		DrawRectangularControl(g, paintParams);
		RectangleF layoutRectangle = paintParams.Bounds;
		PdfDictionary pdfDictionary = null;
		if (g.Layer != null && g.Page != null && !g.Page.Dictionary.ContainsKey("Rotate"))
		{
			pdfDictionary = new PdfDictionary();
			if ((g.Page.Dictionary["Parent"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2)
			{
				pdfDictionary2.ContainsKey("Rotate");
			}
		}
		_ = paintParams.RotationAngle;
		_ = 0;
		if ((g.Layer != null && g.Page.Rotation != 0) || paintParams.RotationAngle > 0)
		{
			PdfGraphicsState state = g.Save();
			if (g.Layer != null && g.Page.Rotation != 0)
			{
				if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					g.TranslateTransform(g.Size.Height, 0f);
					g.RotateTransform(90f);
					float y = g.Page.Size.Height - (layoutRectangle.X + layoutRectangle.Width);
					float y2 = layoutRectangle.Y;
					layoutRectangle = new RectangleF(y2, y, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					g.TranslateTransform(g.Page.Size.Width, g.Page.Size.Height);
					g.RotateTransform(-180f);
					SizeF size = g.Page.Size;
					float x = size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y3 = size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x, y3, layoutRectangle.Width, layoutRectangle.Height);
				}
				else if (g.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					g.TranslateTransform(0f, g.Size.Width);
					g.RotateTransform(270f);
					float x2 = g.Page.Size.Width - (layoutRectangle.Y + layoutRectangle.Height);
					float x3 = layoutRectangle.X;
					layoutRectangle = new RectangleF(x2, x3, layoutRectangle.Height, layoutRectangle.Width);
				}
			}
			if (paintParams.RotationAngle > 0)
			{
				if (paintParams.RotationAngle == 90)
				{
					g.TranslateTransform(0f, g.Size.Height);
					g.RotateTransform(-90f);
					float x4 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					float x5 = layoutRectangle.X;
					layoutRectangle = new RectangleF(x4, x5, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.RotationAngle == 270)
				{
					g.TranslateTransform(g.Size.Width, 0f);
					g.RotateTransform(-270f);
					float y4 = layoutRectangle.Y;
					float y5 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					layoutRectangle = new RectangleF(y4, y5, layoutRectangle.Height, layoutRectangle.Width);
				}
				else if (paintParams.RotationAngle == 180)
				{
					g.TranslateTransform(g.Page.Size.Width, g.Page.Size.Height);
					g.RotateTransform(-180f);
					float x6 = g.Size.Width - (layoutRectangle.X + layoutRectangle.Width);
					float y6 = g.Size.Height - (layoutRectangle.Y + layoutRectangle.Height);
					layoutRectangle = new RectangleF(x6, y6, layoutRectangle.Width, layoutRectangle.Height);
				}
			}
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
			g.Restore(state);
		}
		else
		{
			g.DrawString(text, font, paintParams.ForeBrush, layoutRectangle, format);
		}
	}

	public static void DrawRadioButton(PdfGraphics g, PaintParams paintParams, string checkSymbol, PdfCheckFieldState state)
	{
		if (checkSymbol != "l")
		{
			DrawCheckBox(g, paintParams, checkSymbol, state, null);
			return;
		}
		switch (state)
		{
		case PdfCheckFieldState.Unchecked:
		case PdfCheckFieldState.Checked:
			g.DrawEllipse(paintParams.BackBrush, paintParams.Bounds);
			break;
		case PdfCheckFieldState.PressedUnchecked:
		case PdfCheckFieldState.PressedChecked:
			if (paintParams.BorderStyle == PdfBorderStyle.Beveled || paintParams.BorderStyle == PdfBorderStyle.Underline)
			{
				g.DrawEllipse(paintParams.BackBrush, paintParams.Bounds);
			}
			else
			{
				g.DrawEllipse(paintParams.ShadowBrush, paintParams.Bounds);
			}
			break;
		}
		DrawRoundBorder(g, paintParams.Bounds, paintParams.BorderPen, paintParams.BorderWidth);
		DrawRoundShadow(g, paintParams, state);
		if (state == PdfCheckFieldState.Checked || state == PdfCheckFieldState.PressedChecked)
		{
			RectangleF rectangle = new RectangleF(paintParams.Bounds.X + paintParams.BorderWidth / 2f, paintParams.Bounds.Y + paintParams.BorderWidth / 2f, paintParams.Bounds.Width - paintParams.BorderWidth, paintParams.Bounds.Height - paintParams.BorderWidth);
			rectangle.X += rectangle.Width / 4f;
			rectangle.Y += rectangle.Width / 4f;
			rectangle.Height -= rectangle.Width / 2f;
			rectangle.Width -= rectangle.Width / 2f;
			g.DrawEllipse(paintParams.ForeBrush, rectangle);
		}
	}

	public static void DrawSignature(PdfGraphics g, PaintParams paintParams)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		if (paintParams == null)
		{
			throw new ArgumentNullException("paintParams");
		}
		DrawRectangularControl(g, paintParams);
		RectangleF bounds = paintParams.Bounds;
		if (paintParams.BorderStyle == PdfBorderStyle.Beveled || paintParams.BorderStyle == PdfBorderStyle.Inset)
		{
			bounds.X += 4f * paintParams.BorderWidth;
			bounds.Width -= 8f * paintParams.BorderWidth;
		}
		else
		{
			bounds.X += 2f * paintParams.BorderWidth;
			bounds.Width -= 4f * paintParams.BorderWidth;
		}
	}

	public static void DrawEllipseAnnotation(PdfGraphics g, PaintParams paintParams, float x, float y, float width, float height)
	{
		g.DrawEllipse(paintParams.BorderPen, paintParams.BackBrush, x, y, width, height);
	}

	public static void DrawRectangleAnnotation(PdfGraphics g, PaintParams paintParams, float x, float y, float width, float height)
	{
		g.DrawRectangle(paintParams.BorderPen, paintParams.BackBrush, x, y, width, height);
	}

	internal static void DrawPolygonCloud(PdfGraphics g, PdfPen borderPen, PdfBrush backBrush, float intensity, PointF[] points, float borderWidth)
	{
		float radius = intensity * 4f + 0.5f * borderWidth;
		DrawCloudStyle(g, backBrush, borderPen, radius, 0.833f, points, isAppearance: false);
	}

	internal static void DrawRectanglecloud(PdfGraphics g, PaintParams paintparams, RectangleF rectangle, float intensity, float borderWidth)
	{
		float radius = intensity * 4f + 0.5f * borderWidth;
		PdfPath pdfPath = new PdfPath();
		pdfPath.AddRectangle(rectangle);
		pdfPath.CloseFigure();
		DrawCloudStyle(g, paintparams.BackBrush, paintparams.BorderPen, radius, 0.833f, pdfPath.PathPoints, isAppearance: false);
	}

	private static void DrawCloudStyle(PdfGraphics g, PdfBrush brush, PdfPen pen, float radius, float overlap, PointF[] points, bool isAppearance)
	{
		if (IsClockWise(points))
		{
			PointF[] array = new PointF[points.Length];
			int num = points.Length - 1;
			int num2 = 0;
			while (num >= 0)
			{
				array[num2] = points[num];
				num--;
				num2++;
			}
			points = array;
		}
		List<CloudStyleArc> list = new List<CloudStyleArc>();
		float num3 = 2f * radius * overlap;
		PointF pointF = points[^1];
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF2 = points[i];
			float num4 = pointF2.X - pointF.X;
			float num5 = pointF2.Y - pointF.Y;
			float num6 = (float)Math.Sqrt(num4 * num4 + num5 * num5);
			num4 /= num6;
			num5 /= num6;
			float num7 = num3;
			for (float num8 = 0f; (double)num8 + 0.1 * (double)num7 < (double)num6; num8 += num7)
			{
				CloudStyleArc cloudStyleArc = new CloudStyleArc();
				cloudStyleArc.point = new PointF(pointF.X + num8 * num4, pointF.Y + num8 * num5);
				list.Add(cloudStyleArc);
			}
			pointF = pointF2;
		}
		new PdfPath().AddPolygon(points);
		CloudStyleArc cloudStyleArc2 = list[list.Count - 1];
		for (int j = 0; j < list.Count; j++)
		{
			CloudStyleArc cloudStyleArc3 = list[j];
			PointF intersectionDegrees = GetIntersectionDegrees(cloudStyleArc2.point, cloudStyleArc3.point, radius);
			cloudStyleArc2.endAngle = intersectionDegrees.X;
			cloudStyleArc3.startAngle = intersectionDegrees.Y;
			cloudStyleArc2 = cloudStyleArc3;
		}
		PdfPath pdfPath = new PdfPath();
		for (int k = 0; k < list.Count; k++)
		{
			CloudStyleArc cloudStyleArc4 = list[k];
			float num9 = cloudStyleArc4.startAngle % 360f;
			float num10 = cloudStyleArc4.endAngle % 360f;
			float num11 = 0f;
			if (num9 > 0f && num10 < 0f)
			{
				num11 = 180f - num9 + (180f - ((num10 < 0f) ? (0f - num10) : num10));
			}
			else if (num9 < 0f && num10 > 0f)
			{
				num11 = 0f - num9 + num10;
			}
			else if (num9 > 0f && num10 > 0f)
			{
				float num12 = 0f;
				if (num9 > num10)
				{
					num12 = num9 - num10;
					num11 = 360f - num12;
				}
				else
				{
					num11 = num10 - num9;
				}
			}
			else if (num9 < 0f && num10 < 0f)
			{
				float num13 = 0f;
				if (num9 > num10)
				{
					num13 = num9 - num10;
					num11 = 360f - num13;
				}
				else
				{
					num11 = 0f - (num9 + (0f - num10));
				}
			}
			if (num11 < 0f)
			{
				num11 = 0f - num11;
			}
			cloudStyleArc4.endAngle = num11;
			pdfPath.AddArc(new RectangleF(cloudStyleArc4.point.X - radius, cloudStyleArc4.point.Y - radius, 2f * radius, 2f * radius), num9, num11);
		}
		pdfPath.CloseFigure();
		PointF[] array2 = new PointF[pdfPath.PathPoints.Length];
		if (isAppearance)
		{
			for (int l = 0; l < pdfPath.PathPoints.Length; l++)
			{
				array2[l] = new PointF(pdfPath.PathPoints[l].X, 0f - pdfPath.PathPoints[l].Y);
			}
		}
		PdfPath pdfPath2 = null;
		pdfPath2 = ((!isAppearance) ? new PdfPath(pdfPath.PathPoints, pdfPath.PathTypes) : new PdfPath(array2, pdfPath.PathTypes));
		if (brush != null)
		{
			g.DrawPath(brush, pdfPath2);
		}
		float num14 = 60f / (float)Math.PI;
		pdfPath = new PdfPath();
		for (int m = 0; m < list.Count; m++)
		{
			CloudStyleArc cloudStyleArc5 = list[m];
			pdfPath.AddArc(new RectangleF(cloudStyleArc5.point.X - radius, cloudStyleArc5.point.Y - radius, 2f * radius, 2f * radius), cloudStyleArc5.startAngle, cloudStyleArc5.endAngle + num14);
		}
		pdfPath.CloseFigure();
		array2 = new PointF[pdfPath.PathPoints.Length];
		if (isAppearance)
		{
			for (int n = 0; n < pdfPath.PathPoints.Length; n++)
			{
				array2[n] = new PointF(pdfPath.PathPoints[n].X, 0f - pdfPath.PathPoints[n].Y);
			}
		}
		pdfPath2 = ((!isAppearance) ? new PdfPath(pdfPath.PathPoints, pdfPath.PathTypes) : new PdfPath(array2, pdfPath.PathTypes));
		g.DrawPath(pen, pdfPath2);
	}

	private static bool IsClockWise(PointF[] points)
	{
		double num = 0.0;
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF = points[i];
			PointF pointF2 = points[(i + 1) % points.Length];
			num += (double)((pointF2.X - pointF.X) * (pointF2.Y + pointF.Y));
		}
		return num > 0.0;
	}

	private static PointF GetIntersectionDegrees(PointF point1, PointF point2, float radius)
	{
		float num = point2.X - point1.X;
		float num2 = point2.Y - point1.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = (float)(0.5 * (double)num3 / (double)radius);
		if (num4 < -1f)
		{
			num4 = -1f;
		}
		if (num4 > 1f)
		{
			num4 = 1f;
		}
		float num5 = (float)Math.Atan2(num2, num);
		float num6 = (float)Math.Acos(num4);
		return new PointF((float)((double)(num5 - num6) * (180.0 / Math.PI)), (float)((Math.PI + (double)num5 + (double)num6) * (180.0 / Math.PI)));
	}

	public static void DrawFreeTextAnnotation(PdfGraphics g, PaintParams paintParams, string text, PdfFont font, RectangleF rect, bool isSkipDrawRectangle, PdfTextAlignment alignment, bool isRotation)
	{
		if (!isSkipDrawRectangle)
		{
			g.DrawRectangle(paintParams.BorderPen, paintParams.ForeBrush, rect);
			return;
		}
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.WordWrap = PdfWordWrapType.Word;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Top;
		pdfStringFormat.ComplexScript = paintParams.m_complexScript;
		pdfStringFormat.TextDirection = paintParams.m_textDirection;
		pdfStringFormat.Alignment = alignment;
		pdfStringFormat.LineSpacing = paintParams.m_lineSpacing;
		if (isRotation)
		{
			g.DrawString(text, font, paintParams.BackBrush, paintParams.Bounds.Location, pdfStringFormat);
		}
		else
		{
			g.DrawString(text, font, paintParams.BackBrush, rect, pdfStringFormat);
		}
	}

	private static void DrawBorder(PdfGraphics g, RectangleF bounds, PdfPen borderPen, PdfBorderStyle style, float borderWidth)
	{
		if (borderPen != null && borderWidth > 0f && !borderPen.Color.IsEmpty)
		{
			if (style == PdfBorderStyle.Underline)
			{
				g.DrawLine(borderPen, bounds.X, bounds.Y + bounds.Height - borderWidth / 2f, bounds.X + bounds.Width, bounds.Y + bounds.Height - borderWidth / 2f);
				return;
			}
			RectangleF rectangle = new RectangleF(bounds.X + borderWidth / 2f, bounds.Y + borderWidth / 2f, bounds.Width - borderWidth, bounds.Height - borderWidth);
			g.DrawRectangle(borderPen, rectangle);
		}
	}

	private static void DrawRoundBorder(PdfGraphics g, RectangleF bounds, PdfPen borderPen, float borderWidth)
	{
		if (borderPen != null && borderWidth > 0f && !borderPen.Color.IsEmpty)
		{
			RectangleF rectangleF = bounds;
			if (rectangleF != RectangleF.Empty)
			{
				rectangleF = new RectangleF(bounds.X + borderWidth / 2f, bounds.Y + borderWidth / 2f, bounds.Width - borderWidth, bounds.Height - borderWidth);
				g.DrawEllipse(borderPen, rectangleF);
			}
		}
	}

	private static void DrawRectangularControl(PdfGraphics g, PaintParams paintParams)
	{
		g.DrawRectangle(paintParams.BackBrush, paintParams.Bounds);
		DrawBorder(g, paintParams.Bounds, paintParams.BorderPen, paintParams.BorderStyle, paintParams.BorderWidth);
		switch (paintParams.BorderStyle)
		{
		case PdfBorderStyle.Inset:
			DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, GrayBrush);
			DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, SilverBrush);
			break;
		case PdfBorderStyle.Beveled:
			DrawLeftTopShadow(g, paintParams.Bounds, paintParams.BorderWidth, WhiteBrush);
			DrawRightBottomShadow(g, paintParams.Bounds, paintParams.BorderWidth, paintParams.ShadowBrush);
			break;
		}
	}

	private static void DrawLeftTopShadow(PdfGraphics g, RectangleF bounds, float width, PdfBrush brush)
	{
		PdfPath pdfPath = new PdfPath();
		pdfPath.AddPolygon(new PointF[6]
		{
			new PointF(bounds.X + width, bounds.Y + width),
			new PointF(bounds.X + width, bounds.Bottom - width),
			new PointF(bounds.X + 2f * width, bounds.Bottom - 2f * width),
			new PointF(bounds.X + 2f * width, bounds.Y + 2f * width),
			new PointF(bounds.Right - 2f * width, bounds.Y + 2f * width),
			new PointF(bounds.Right - width, bounds.Y + width)
		});
		g.DrawPath(brush, pdfPath);
	}

	private static void DrawRightBottomShadow(PdfGraphics g, RectangleF bounds, float width, PdfBrush brush)
	{
		PdfPath pdfPath = new PdfPath();
		pdfPath.AddPolygon(new PointF[6]
		{
			new PointF(bounds.X + width, bounds.Bottom - width),
			new PointF(bounds.X + 2f * width, bounds.Bottom - 2f * width),
			new PointF(bounds.Right - 2f * width, bounds.Bottom - 2f * width),
			new PointF(bounds.Right - 2f * width, bounds.Y + 2f * width),
			new PointF(bounds.X + bounds.Width - width, bounds.Y + width),
			new PointF(bounds.Right - width, bounds.Bottom - width)
		});
		g.DrawPath(brush, pdfPath);
	}

	private static void DrawRoundShadow(PdfGraphics g, PaintParams paintParams, PdfCheckFieldState state)
	{
		float borderWidth = paintParams.BorderWidth;
		RectangleF bounds = paintParams.Bounds;
		bounds.Inflate(-1.5f * borderWidth, -1.5f * borderWidth);
		PdfPen pdfPen = null;
		PdfPen pdfPen2 = null;
		PdfColor color = ((PdfSolidBrush)paintParams.ShadowBrush).Color;
		switch (paintParams.BorderStyle)
		{
		case PdfBorderStyle.Beveled:
			switch (state)
			{
			case PdfCheckFieldState.PressedUnchecked:
			case PdfCheckFieldState.PressedChecked:
				pdfPen = GetPen(color, borderWidth);
				pdfPen2 = GetPen(new PdfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue), borderWidth);
				break;
			case PdfCheckFieldState.Unchecked:
			case PdfCheckFieldState.Checked:
				pdfPen = GetPen(new PdfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue), borderWidth);
				pdfPen2 = GetPen(color, borderWidth);
				break;
			}
			break;
		case PdfBorderStyle.Inset:
			switch (state)
			{
			case PdfCheckFieldState.PressedUnchecked:
			case PdfCheckFieldState.PressedChecked:
				pdfPen = GetPen(new PdfColor(0, 0, 0), borderWidth);
				pdfPen2 = GetPen(new PdfColor(0, 0, 0), borderWidth);
				break;
			case PdfCheckFieldState.Unchecked:
			case PdfCheckFieldState.Checked:
				pdfPen = GetPen(new PdfColor(byte.MaxValue, 128, 128, 128), borderWidth);
				pdfPen2 = GetPen(new PdfColor(byte.MaxValue, 192, 192, 192), borderWidth);
				break;
			}
			break;
		}
		if (pdfPen != null && pdfPen2 != null)
		{
			g.DrawArc(pdfPen, bounds, 135f, 180f);
			g.DrawArc(pdfPen2, bounds, -45f, 180f);
		}
	}

	private static PdfPen GetPen(PdfColor color, float width)
	{
		lock (s_pens)
		{
			string key = $"{color}{width}";
			PdfPen pdfPen = (s_pens.ContainsKey(key) ? s_pens[key] : null);
			if (pdfPen == null)
			{
				pdfPen = new PdfPen(color, width);
				s_pens[key] = pdfPen;
			}
			return pdfPen;
		}
	}
}
