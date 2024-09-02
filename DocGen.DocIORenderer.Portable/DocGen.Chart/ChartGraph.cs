using System;
using System.Collections.Generic;
using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal abstract class ChartGraph
{
	private Stack<Matrix> m_transformStack = new Stack<Matrix>();

	private bool m_right;

	public abstract Matrix Transform { get; set; }

	public abstract SmoothingMode SmoothingMode { get; set; }

	public bool isRight
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	[Obsolete("Use PushTransform()")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void PushTranfsorm()
	{
		m_transformStack.Push(Transform);
	}

	public void PushTransform()
	{
		m_transformStack.Push(Transform);
	}

	public void Translate(SizeF offset)
	{
		Transform.Translate(offset.Width, offset.Height);
	}

	public void MultiplyTransform(Matrix matrix)
	{
		Matrix matrix2 = Transform.Clone() as Matrix;
		matrix2.Multiply(matrix);
		Transform = matrix2;
	}

	public void PopTransform()
	{
		Transform = m_transformStack.Pop();
	}

	public void DrawLine(Pen pen, PointF pt1, PointF pt2)
	{
		DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
	}

	public void DrawRect(BrushInfo brushInfo, Pen pen, float x, float y, float width, float height)
	{
		using Brush brush = GetBrush(brushInfo, new RectangleF(x, y, width, height));
		DrawRect(brush, pen, x, y, width, height);
	}

	public void DrawRect(BrushInfo brushInfo, Pen pen, RectangleF rect)
	{
		using Brush brush = GetBrush(brushInfo, rect);
		DrawRect(brush, pen, rect.X, rect.Y, rect.Width, rect.Height);
	}

	public void DrawRect(Brush brush, Pen pen, RectangleF rect)
	{
		DrawRect(brush, pen, rect.X, rect.Y, rect.Width, rect.Height);
	}

	public void DrawRect(Pen pen, RectangleF rect)
	{
		DrawRect((Brush)null, pen, rect.X, rect.Y, rect.Width, rect.Height);
	}

	public void DrawEllipse(BrushInfo brushInfo, Pen pen, float x, float y, float width, float height)
	{
		using Brush brush = GetBrush(brushInfo, new RectangleF(x, y, width, height));
		DrawEllipse(brush, pen, x, y, width, height);
	}

	public void DrawPath(BrushInfo brushInfo, Pen pen, GraphicsPath path)
	{
		using Brush brush = GetBrush(brushInfo, path.GetBounds());
		DrawPath(brush, pen, path);
	}

	public void DrawPath(Pen pen, GraphicsPath path)
	{
		DrawPath((Brush)null, pen, path);
	}

	public void DrawImage(DocGen.Drawing.Image image, RectangleF rect)
	{
		DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
	}

	public abstract void DrawRect(Brush brush, Pen pen, float x, float y, float width, float height);

	public abstract void DrawEllipse(Brush brush, Pen pen, float x, float y, float width, float height);

	public abstract void DrawPath(Brush brush, Pen pen, GraphicsPath path);

	public abstract void DrawLine(Pen pen, float x1, float y1, float x2, float y2);

	public abstract void DrawImage(DocGen.Drawing.Image image, float x, float y, float width, float height);

	public abstract void DrawPolyline(Pen pen, PointF[] points);

	public abstract void DrawPolygon(Pen pen, PointF[] points);

	public abstract void FillPolygon(SolidBrush brush, PointF[] points);

	public abstract void DrawString(string text, Font font, Brush brush, RectangleF rect);

	public abstract void DrawString(string text, Font font, Brush brush, PointF location, StringFormat stringformat);

	public abstract void DrawString(string text, Font font, Brush brush, RectangleF rect, StringFormat stringformat);

	public abstract SizeF MeasureString(string text, Font font);

	public abstract SizeF MeasureString(string text, Font font, float maxWidth);

	public abstract SizeF MeasureString(string text, Font font, float maxWidth, StringFormat stringFormat);

	public abstract SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat);

	private Brush GetBrush(BrushInfo brushInfo, RectangleF bounds)
	{
		Brush result = null;
		if (!bounds.IsEmpty && brushInfo != null)
		{
			switch (brushInfo.Style)
			{
			case BrushStyle.Gradient:
				if (brushInfo.GradientStyle != 0)
				{
					switch (brushInfo.GradientStyle)
					{
					case GradientStyle.BackwardDiagonal:
					{
						LinearGradientBrush linearGradientBrush3;
						if (brushInfo.Angle != -1.0)
						{
							linearGradientBrush3 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true);
							linearGradientBrush3.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush3.InterpolationColors = brushInfo.Blend;
						}
						else
						{
							linearGradientBrush3 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.BackwardDiagonal);
							linearGradientBrush3.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush3.InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors);
						}
						result = linearGradientBrush3;
						break;
					}
					case GradientStyle.ForwardDiagonal:
					{
						LinearGradientBrush linearGradientBrush2;
						if (brushInfo.Angle != -1.0)
						{
							linearGradientBrush2 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true);
							linearGradientBrush2.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush2.InterpolationColors = brushInfo.Blend;
						}
						else
						{
							linearGradientBrush2 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.ForwardDiagonal);
							linearGradientBrush2.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush2.InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors);
						}
						result = linearGradientBrush2;
						break;
					}
					case GradientStyle.Horizontal:
					{
						LinearGradientBrush linearGradientBrush4;
						if (brushInfo.Angle != -1.0)
						{
							linearGradientBrush4 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true);
							linearGradientBrush4.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush4.InterpolationColors = brushInfo.Blend;
						}
						else
						{
							linearGradientBrush4 = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.Horizontal);
							linearGradientBrush4.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush4.InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors);
						}
						result = linearGradientBrush4;
						break;
					}
					case GradientStyle.PathEllipse:
					{
						GraphicsPath graphicsPath2 = new GraphicsPath();
						graphicsPath2.AddEllipse(RectangleF.Inflate(bounds, 0.25f * bounds.Width, 0.25f * bounds.Height));
						result = new PathGradientBrush(graphicsPath2)
						{
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					}
					case GradientStyle.PathRectangle:
					{
						_ = brushInfo.Rect;
						GraphicsPath graphicsPath = new GraphicsPath();
						graphicsPath.AddRectangle(bounds);
						RadialGradientBrush radialGradientBrush = new RadialGradientBrush(graphicsPath);
						radialGradientBrush.CenterPoint = radialGradientBrush.GetRadialCenterPoint(brushInfo.Rect, bounds);
						radialGradientBrush.InterpolationColors = brushInfo.Blend;
						result = radialGradientBrush;
						break;
					}
					case GradientStyle.Vertical:
					{
						LinearGradientBrush linearGradientBrush;
						if (brushInfo.Angle != -1.0)
						{
							linearGradientBrush = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, (float)brushInfo.Angle, isAngleScaleable: true, isXlsIO: true);
							linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush.InterpolationColors = brushInfo.Blend;
						}
						else
						{
							linearGradientBrush = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.Vertical);
							linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
							linearGradientBrush.InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors);
						}
						result = linearGradientBrush;
						break;
					}
					}
				}
				else
				{
					result = new SolidBrush(brushInfo.BackColor);
				}
				break;
			case BrushStyle.Pattern:
				result = ((brushInfo.PatternStyle == PatternStyle.None) ? ((Brush)new SolidBrush(brushInfo.BackColor)) : ((Brush)new HatchBrush((HatchStyle)(brushInfo.PatternStyle - 1), brushInfo.ForeColor, brushInfo.BackColor)));
				break;
			case BrushStyle.Solid:
				result = new SolidBrush(brushInfo.BackColor);
				break;
			}
		}
		return result;
	}

	public static Brush GetBrushItem(BrushInfo brushInfo, RectangleF bounds)
	{
		Brush result = null;
		if (!bounds.IsEmpty)
		{
			switch (brushInfo.Style)
			{
			case BrushStyle.Gradient:
				if (brushInfo.GradientStyle != 0)
				{
					switch (brushInfo.GradientStyle)
					{
					case GradientStyle.BackwardDiagonal:
						result = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.BackwardDiagonal)
						{
							WrapMode = WrapMode.TileFlipXY,
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					case GradientStyle.ForwardDiagonal:
						result = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.ForwardDiagonal)
						{
							WrapMode = WrapMode.TileFlipXY,
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					case GradientStyle.Horizontal:
						result = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.Horizontal)
						{
							WrapMode = WrapMode.TileFlipXY,
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					case GradientStyle.PathEllipse:
					{
						GraphicsPath graphicsPath2 = new GraphicsPath();
						graphicsPath2.AddEllipse(RectangleF.Inflate(bounds, 0.25f * bounds.Width, 0.25f * bounds.Height));
						result = new PathGradientBrush(graphicsPath2)
						{
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					}
					case GradientStyle.PathRectangle:
					{
						GraphicsPath graphicsPath = new GraphicsPath();
						graphicsPath.AddRectangle(bounds);
						result = new PathGradientBrush(graphicsPath)
						{
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					}
					case GradientStyle.Vertical:
						result = new LinearGradientBrush(bounds, Color.Empty, Color.Empty, LinearGradientMode.Vertical)
						{
							WrapMode = WrapMode.TileFlipXY,
							InterpolationColors = GetGenericColorBlend(brushInfo.GradientColors)
						};
						break;
					}
				}
				else
				{
					result = new SolidBrush(brushInfo.BackColor);
				}
				break;
			case BrushStyle.Pattern:
				result = ((brushInfo.PatternStyle == PatternStyle.None) ? ((Brush)new SolidBrush(brushInfo.BackColor)) : ((Brush)new HatchBrush((HatchStyle)(brushInfo.PatternStyle - 1), brushInfo.ForeColor, brushInfo.BackColor)));
				break;
			case BrushStyle.Solid:
				result = new SolidBrush(brushInfo.BackColor);
				break;
			}
		}
		return result;
	}

	private static ColorBlend GetGenericColorBlend(BrushInfoColorArrayList colors)
	{
		ColorBlend colorBlend = new ColorBlend(colors.Count);
		float num = 0f;
		_ = 1f / (float)(colors.Count - 1);
		int i = 0;
		for (int count = colors.Count; i < count; i++)
		{
			colorBlend.Positions[i] = num;
			colorBlend.Colors[i] = colors[count - i - 1];
			num += 1f / (float)count;
		}
		colorBlend.Positions[colorBlend.Positions.Length - 1] = 1f;
		return colorBlend;
	}
}
