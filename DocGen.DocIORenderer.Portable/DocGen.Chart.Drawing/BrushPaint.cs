using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Drawing;

internal sealed class BrushPaint
{
	public static void FillRectangle(Graphics g, Rectangle r, BrushInfo brush)
	{
		FillRectangle(g, (RectangleF)r, brush);
	}

	public static void FillRectangle(Graphics g, RectangleF r, BrushInfo brush)
	{
		if (r.Height == 0f || r.Width == 0f || !(brush != null))
		{
			return;
		}
		switch (brush.Style)
		{
		case BrushStyle.None:
			g.FillRectangle(SystemBrushes.Window, r);
			break;
		case BrushStyle.Solid:
			FillRectangle(g, r, brush.BackColor);
			break;
		case BrushStyle.Pattern:
			if (brush.PatternStyle != 0)
			{
				FillRectangle(g, r, brush.PatternStyle, brush.ForeColor, brush.BackColor);
				break;
			}
			goto case BrushStyle.Solid;
		case BrushStyle.Gradient:
			if (brush.GradientStyle != 0)
			{
				FillRectangle(g, r, brush, (Color[])brush.GradientColors.ToArray(typeof(Color)));
				break;
			}
			goto case BrushStyle.Solid;
		}
	}

	private static ColorBlend GetGenericColorBlend(Color[] colors)
	{
		ColorBlend colorBlend = new ColorBlend(colors.Length);
		Array.Reverse(colors);
		colorBlend.Colors = colors;
		float[] array = new float[colors.Length];
		array[0] = 0f;
		float num = 0f;
		for (int i = 1; i < colors.Length - 1; i++)
		{
			num = (array[i] = num + 1f / ((float)colors.Length - 1f));
		}
		array[^1] = 1f;
		colorBlend.Positions = array;
		return colorBlend;
	}

	public static void FillRectangle(Graphics g, RectangleF r, BrushInfo brush, Color[] colors)
	{
		if (r.Height == 0f || r.Width == 0f)
		{
			return;
		}
		Brush brush2 = null;
		PathGradientBrush pathGradientBrush = null;
		GraphicsPath graphicsPath = null;
		RectangleF clip = RectangleF.Empty;
		switch (brush.GradientStyle)
		{
		case GradientStyle.ForwardDiagonal:
			brush2 = ((brush.Angle == -1.0) ? new LinearGradientBrush(r, Color.Empty, Color.Empty, LinearGradientMode.ForwardDiagonal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(r, Color.Empty, Color.Empty, (float)brush.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brush.Blend
			});
			break;
		case GradientStyle.BackwardDiagonal:
			brush2 = ((brush.Angle == -1.0) ? new LinearGradientBrush(r, Color.Empty, Color.Empty, LinearGradientMode.BackwardDiagonal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(r, Color.Empty, Color.Empty, (float)brush.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brush.Blend
			});
			break;
		case GradientStyle.Horizontal:
			brush2 = ((brush.Angle == -1.0) ? new LinearGradientBrush(r, Color.Empty, Color.Empty, LinearGradientMode.Horizontal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(r, Color.Empty, Color.Empty, (float)brush.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brush.Blend
			});
			break;
		case GradientStyle.Vertical:
			brush2 = ((brush.Angle == -1.0) ? new LinearGradientBrush(r, Color.Empty, Color.Empty, LinearGradientMode.Vertical)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(r, Color.Empty, Color.Empty, (float)brush.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brush.Blend
			});
			break;
		case GradientStyle.PathRectangle:
		{
			_ = brush.Rect;
			graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(r);
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush(graphicsPath);
			radialGradientBrush.CenterPoint = radialGradientBrush.GetRadialCenterPoint(brush.Rect, r);
			radialGradientBrush.InterpolationColors = brush.Blend;
			brush2 = radialGradientBrush;
			graphicsPath = null;
			break;
		}
		case GradientStyle.PathEllipse:
			graphicsPath = new GraphicsPath();
			clip = g.ClipBounds;
			g.IntersectClip(r);
			r.Inflate(r.Width / 4f, r.Height / 4f);
			graphicsPath.AddEllipse(r);
			break;
		}
		if (graphicsPath != null && graphicsPath.PointCount > 0)
		{
			pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = colors[^1];
			Color[] array = ((graphicsPath.PointCount >= colors.Length - 1) ? new Color[colors.Length - 1] : new Color[graphicsPath.PointCount]);
			int num = 0;
			int num2 = colors.Length - 2;
			while (num2 >= 0 && num < graphicsPath.PointCount)
			{
				array[num++] = colors[num2];
				num2--;
			}
			pathGradientBrush.SurroundColors = array;
			brush2 = pathGradientBrush;
			pathGradientBrush = null;
		}
		if (brush2 != null)
		{
			graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(r);
			g.FillPath(brush2, graphicsPath);
			graphicsPath?.Dispose();
			brush2.Dispose();
			brush2 = null;
		}
		if (!clip.IsEmpty)
		{
			g.SetClip(clip);
		}
	}

	public static void FillRectangle(Graphics g, RectangleF r, PatternStyle hatchStyle, Color foreColor, Color backColor)
	{
		if (r.Height != 0f && r.Width != 0f && hatchStyle != 0)
		{
			Brush brush = new HatchBrush((HatchStyle)(hatchStyle - 1), foreColor, backColor);
			g.FillRectangle(brush, r);
			brush.Dispose();
		}
	}

	public static void FillRectangle(Graphics g, RectangleF r, Color color)
	{
		if (r.Height != 0f && r.Width != 0f)
		{
			Brush brush = new SolidBrush(color);
			g.FillRectangle(brush, r.X, r.Y, r.Width, r.Height);
			brush.Dispose();
		}
	}

	public static void FillPath(Graphics g, GraphicsPath p, BrushInfo brush)
	{
		if (!(brush != null))
		{
			return;
		}
		switch (brush.Style)
		{
		case BrushStyle.None:
			g.FillPath(SystemBrushes.Window, p);
			break;
		case BrushStyle.Solid:
		{
			Brush brush2 = new SolidBrush(brush.BackColor);
			g.FillPath(brush2, p);
			brush2.Dispose();
			break;
		}
		case BrushStyle.Pattern:
			if (brush.PatternStyle != 0)
			{
				Brush brush3 = new HatchBrush((HatchStyle)(brush.PatternStyle - 1), brush.ForeColor, brush.BackColor);
				g.FillPath(brush3, p);
				brush3.Dispose();
				break;
			}
			goto case BrushStyle.Solid;
		case BrushStyle.Gradient:
			if (brush.GradientStyle != 0)
			{
				FillPath(g, p, brush, (Color[])brush.GradientColors.ToArray(typeof(Color)));
				break;
			}
			goto case BrushStyle.Solid;
		}
	}

	public static void FillPath(Graphics g, GraphicsPath p, BrushInfo brushInfo, Color[] colors)
	{
		RectangleF bounds = p.GetBounds();
		if (bounds.Width == 0f || bounds.Height == 0f)
		{
			return;
		}
		Brush brush = null;
		PathGradientBrush pathGradientBrush = null;
		GraphicsPath graphicsPath = null;
		RectangleF empty = RectangleF.Empty;
		switch (brushInfo.GradientStyle)
		{
		case GradientStyle.ForwardDiagonal:
			brush = ((brushInfo.Angle == -1.0) ? new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, LinearGradientMode.ForwardDiagonal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brushInfo.Blend
			});
			break;
		case GradientStyle.BackwardDiagonal:
			brush = ((brushInfo.Angle == -1.0) ? new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, LinearGradientMode.BackwardDiagonal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brushInfo.Blend
			});
			break;
		case GradientStyle.Horizontal:
			brush = ((brushInfo.Angle == -1.0) ? new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, LinearGradientMode.Horizontal)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brushInfo.Blend
			});
			break;
		case GradientStyle.Vertical:
			brush = ((brushInfo.Angle == -1.0) ? new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, LinearGradientMode.Vertical)
			{
				InterpolationColors = GetGenericColorBlend(colors)
			} : new LinearGradientBrush(p.GetBounds(), Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true)
			{
				InterpolationColors = brushInfo.Blend
			});
			break;
		case GradientStyle.PathRectangle:
		{
			_ = brushInfo.Rect;
			RadialGradientBrush radialGradientBrush = new RadialGradientBrush(p);
			radialGradientBrush.CenterPoint = radialGradientBrush.GetRadialCenterPoint(brushInfo.Rect, bounds);
			radialGradientBrush.InterpolationColors = brushInfo.Blend;
			brush = radialGradientBrush;
			break;
		}
		case GradientStyle.PathEllipse:
		{
			graphicsPath = new GraphicsPath();
			Rectangle rect = Rectangle.Ceiling(p.GetBounds());
			rect.Inflate(rect.Width / 4, rect.Height / 4);
			graphicsPath.AddEllipse(rect);
			break;
		}
		}
		if (graphicsPath != null)
		{
			pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = colors[0];
			Color[] array = new Color[(graphicsPath.PointCount < colors.Length - 1) ? graphicsPath.PointCount : (colors.Length - 1)];
			int num = 0;
			int num2 = colors.Length - 2;
			while (num2 >= 0 && num < graphicsPath.PointCount)
			{
				array[num++] = colors[num2];
				num2--;
			}
			pathGradientBrush.SurroundColors = array;
			brush = pathGradientBrush;
			pathGradientBrush = null;
		}
		if (brush != null)
		{
			g.FillPath(brush, p);
			graphicsPath?.Dispose();
			brush.Dispose();
			brush = null;
		}
		if (!empty.IsEmpty)
		{
			g.SetClip(empty);
		}
	}
}
