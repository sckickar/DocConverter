using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal static class DrawingHelper
{
	private static StringFormat m_stringFormat;

	public static StringFormat NoClipFormat;

	private const int COLOR_COEF = 30;

	public static StringFormat CenteredFormat => m_stringFormat;

	static DrawingHelper()
	{
		m_stringFormat = new StringFormat();
		NoClipFormat = new StringFormat(StringFormatFlags.NoClip);
		m_stringFormat.Alignment = StringAlignment.Center;
		m_stringFormat.LineAlignment = StringAlignment.Center;
	}

	public static void Dispose()
	{
		if (NoClipFormat != null)
		{
			NoClipFormat.Dispose();
			NoClipFormat = null;
		}
		if (m_stringFormat != null)
		{
			m_stringFormat.Dispose();
			m_stringFormat = null;
		}
	}

	public static void InitializeStaticVariables()
	{
		m_stringFormat = new StringFormat();
		NoClipFormat = new StringFormat(StringFormatFlags.NoClip);
	}

	public static Color LeprColor(Color startColor, Color endColor, double interpolator)
	{
		int a = (int)((double)(int)startColor.A + interpolator * (double)(endColor.A - startColor.A));
		int r = (int)((double)(int)startColor.R + interpolator * (double)(endColor.R - startColor.R));
		int g = (int)((double)(int)startColor.G + interpolator * (double)(endColor.G - startColor.G));
		int b = (int)((double)(int)startColor.B + interpolator * (double)(endColor.B - startColor.B));
		return GetColorFromARGB(a, r, g, b);
	}

	public static BrushInfo ChangeBackColor(BrushInfo brInfo, Color color)
	{
		BrushInfo result = null;
		switch (brInfo.Style)
		{
		case BrushStyle.Solid:
			result = new BrushInfo(color);
			break;
		case BrushStyle.Gradient:
		{
			Color[] array2 = (Color[])brInfo.GradientColors.ToArray(typeof(Color));
			array2[0] = color;
			result = new BrushInfo(brInfo.GradientStyle, array2);
			break;
		}
		case BrushStyle.Pattern:
		{
			Color[] array = (Color[])brInfo.GradientColors.ToArray(typeof(Color));
			array[0] = color;
			result = new BrushInfo(brInfo.PatternStyle, array);
			break;
		}
		}
		return result;
	}

	public static BrushInfo AddColor(BrushInfo brInfo, Color color)
	{
		BrushInfo result = null;
		switch (brInfo.Style)
		{
		case BrushStyle.Solid:
			result = new BrushInfo(AddColor(brInfo.BackColor, color));
			break;
		case BrushStyle.Gradient:
		{
			Color[] array2 = new Color[brInfo.GradientColors.Count];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = AddColor(brInfo.GradientColors[j], color);
			}
			result = new BrushInfo(brInfo.GradientStyle, array2);
			break;
		}
		case BrushStyle.Pattern:
		{
			Color[] array = new Color[brInfo.GradientColors.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = AddColor(brInfo.GradientColors[i], color);
			}
			result = new BrushInfo(brInfo.PatternStyle, array);
			break;
		}
		}
		return result;
	}

	public static Color AddColor(Color color1, Color color2)
	{
		int alpha = Math.Min(color1.A + color2.A, 255);
		int red = Math.Min(color1.R + color2.R, 255);
		int green = Math.Min(color1.G + color2.G, 255);
		int blue = Math.Min(color1.B + color2.B, 255);
		return Color.FromArgb(alpha, red, green, blue);
	}

	public static BrushInfo AddColor(BrushInfo brInfo, int value)
	{
		BrushInfo result = null;
		switch (brInfo.Style)
		{
		case BrushStyle.None:
			result = brInfo;
			break;
		case BrushStyle.Solid:
			result = new BrushInfo(AddColor(brInfo.BackColor, value));
			break;
		case BrushStyle.Gradient:
		{
			Color[] array2 = new Color[brInfo.GradientColors.Count];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = AddColor(brInfo.GradientColors[j], value);
			}
			result = new BrushInfo(brInfo.GradientStyle, array2);
			break;
		}
		case BrushStyle.Pattern:
		{
			Color[] array = new Color[brInfo.GradientColors.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = AddColor(brInfo.GradientColors[i], value);
			}
			result = new BrushInfo(brInfo.PatternStyle, array);
			break;
		}
		}
		return result;
	}

	public static Color AddColor(Color color, int value)
	{
		return GetColorFromARGB(color.A, color.R + value, color.G + value, color.B + value);
	}

	public static BrushInfo MedialColor(BrushInfo brInfo, Color color)
	{
		BrushInfo result = null;
		switch (brInfo.Style)
		{
		case BrushStyle.Solid:
			result = new BrushInfo(MedialColor(brInfo.BackColor, color));
			break;
		case BrushStyle.Gradient:
		{
			Color[] array2 = new Color[brInfo.GradientColors.Count];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = MedialColor(brInfo.GradientColors[j], color);
			}
			result = new BrushInfo(brInfo.GradientStyle, array2);
			break;
		}
		case BrushStyle.Pattern:
		{
			Color[] array = new Color[brInfo.GradientColors.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MedialColor(brInfo.GradientColors[i], color);
			}
			result = new BrushInfo(brInfo.PatternStyle, array);
			break;
		}
		}
		return result;
	}

	public static Color MedialColor(Color color1, Color color2)
	{
		int alpha = (color1.A + color2.A) / 2;
		int red = (color1.R + color2.R) / 2;
		int green = (color1.G + color2.G) / 2;
		int blue = (color1.B + color2.B) / 2;
		return Color.FromArgb(alpha, red, green, blue);
	}

	public static Color GetColorFromARGB(int a, int r, int g, int b)
	{
		a = ChartMath.MinMax(a, 0, 255);
		r = ChartMath.MinMax(r, 0, 255);
		g = ChartMath.MinMax(g, 0, 255);
		b = ChartMath.MinMax(b, 0, 255);
		return Color.FromArgb(a, r, g, b);
	}

	public static void DrawBorder(Graphics g, Color color, int width, Rectangle rect, BorderStyle borderStyle)
	{
		int num = Math.Min(width, Math.Min(rect.Width, rect.Height) / 2);
		int num2 = num / 2;
		switch (borderStyle)
		{
		case BorderStyle.FixedSingle:
		{
			using Pen pen = new Pen(color, width);
			pen.Alignment = PenAlignment.Inset;
			g.DrawRectangle(pen, rect);
			break;
		}
		case BorderStyle.Fixed3D:
		{
			SolidBrush solidBrush = new SolidBrush(AddColor(color, -30));
			SolidBrush solidBrush2 = new SolidBrush(AddColor(color, 30));
			SolidBrush solidBrush3 = new SolidBrush(AddColor(color, -60));
			SolidBrush solidBrush4 = new SolidBrush(AddColor(color, 60));
			GraphicsPath graphicsPath = new GraphicsPath();
			GraphicsPath graphicsPath2 = new GraphicsPath();
			GraphicsPath graphicsPath3 = new GraphicsPath();
			GraphicsPath graphicsPath4 = new GraphicsPath();
			graphicsPath2.AddPolygon(new Point[6]
			{
				new Point(rect.Left, rect.Top),
				new Point(rect.Right, rect.Top),
				new Point(rect.Right - num, rect.Top + num),
				new Point(rect.Left + num, rect.Top + num),
				new Point(rect.Left + num, rect.Bottom - num),
				new Point(rect.Left, rect.Bottom)
			});
			graphicsPath4.AddPolygon(new Point[6]
			{
				new Point(rect.Left + num, rect.Top + num),
				new Point(rect.Right - num, rect.Top + num),
				new Point(rect.Right - num2, rect.Top + num2),
				new Point(rect.Left + num2, rect.Top + num2),
				new Point(rect.Left + num2, rect.Bottom - num2),
				new Point(rect.Left + num, rect.Bottom - num)
			});
			graphicsPath.AddPolygon(new Point[6]
			{
				new Point(rect.Right - num, rect.Bottom - num),
				new Point(rect.Right - num, rect.Top + num),
				new Point(rect.Right - num2, rect.Top + num2),
				new Point(rect.Right - num2, rect.Bottom - num2),
				new Point(rect.Left + num2, rect.Bottom - num2),
				new Point(rect.Left + num, rect.Bottom - num)
			});
			graphicsPath3.AddPolygon(new Point[6]
			{
				new Point(rect.Right, rect.Bottom),
				new Point(rect.Right, rect.Top),
				new Point(rect.Right - num, rect.Top + num),
				new Point(rect.Right - num, rect.Bottom - num),
				new Point(rect.Left + num, rect.Bottom - num),
				new Point(rect.Left, rect.Bottom)
			});
			g.FillPath(solidBrush, graphicsPath2);
			g.FillPath(solidBrush3, graphicsPath4);
			g.FillPath(solidBrush4, graphicsPath3);
			g.FillPath(solidBrush2, graphicsPath);
			solidBrush4.Dispose();
			solidBrush3.Dispose();
			solidBrush2.Dispose();
			solidBrush.Dispose();
			break;
		}
		}
	}

	public static GraphicsContainer BeginTransform(Graphics g)
	{
		SmoothingMode smoothingMode = g.SmoothingMode;
		TextRenderingHint textRenderingHint = g.TextRenderingHint;
		GraphicsContainer result = g.BeginContainer();
		g.SmoothingMode = smoothingMode;
		g.TextRenderingHint = textRenderingHint;
		return result;
	}

	public static void EndTransform(Graphics g, GraphicsContainer cont)
	{
		g.EndContainer(cont);
	}

	public static void DrawRectangleF(Graphics g, Pen p, RectangleF rect)
	{
		g.DrawRectangle(p, rect.X, rect.Y, rect.Width, rect.Height);
	}

	public static string EllipsesText(Graphics g, string text, Font font, float width)
	{
		if (text.Length >= 1)
		{
			int charactersFitted = 0;
			int linesFilled = 0;
			SizeF sizeF = g.MeasureString("...", font);
			if (width > sizeF.Width)
			{
				g.MeasureString(text, font, new SizeF(width - sizeF.Width, sizeF.Height), m_stringFormat, out charactersFitted, out linesFilled);
			}
			if (charactersFitted <= 0)
			{
				charactersFitted = 1;
			}
			if (charactersFitted != text.Length)
			{
				return text.Substring(0, charactersFitted) + "...";
			}
			return text;
		}
		return "";
	}
}
