using System;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartGDIGraph : ChartGraph
{
	private Graphics m_g;

	public Graphics Graphics => m_g;

	public override Matrix Transform
	{
		get
		{
			return m_g.Transform;
		}
		set
		{
			m_g.Transform = value;
		}
	}

	public override SmoothingMode SmoothingMode
	{
		get
		{
			return m_g.SmoothingMode;
		}
		set
		{
			m_g.SmoothingMode = value;
		}
	}

	public ChartGDIGraph(Graphics g)
	{
		m_g = g;
	}

	public override void DrawRect(Brush brush, Pen pen, float x, float y, float width, float height)
	{
		if (brush != null && height > 1f && width > 1f)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(new Rectangle((int)x, (int)y, (int)width, (int)height));
			m_g.FillPath(brush, graphicsPath);
		}
		if (pen != null)
		{
			m_g.DrawRectangle(pen, x, y, width, height);
		}
	}

	public override void DrawEllipse(Brush brush, Pen pen, float x, float y, float width, float height)
	{
		if (brush != null)
		{
			m_g.FillEllipse(brush, x, y, width, height);
		}
		if (pen != null)
		{
			m_g.DrawEllipse(pen, x, y, width, height);
		}
	}

	public override void DrawPath(Brush brush, Pen pen, GraphicsPath gp)
	{
		try
		{
			if (brush != null)
			{
				m_g.FillPath(brush, gp);
			}
			if (pen != null)
			{
				m_g.DrawPath(pen, gp);
			}
		}
		catch
		{
		}
	}

	public override void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
	{
		m_g.DrawLine(pen, x1, y1, x2, y2);
	}

	public override void DrawImage(DocGen.Drawing.Image image, float x, float y, float width, float height)
	{
		m_g.DrawImage(image, x, y, width, height);
	}

	public override void DrawPolyline(Pen pen, PointF[] points)
	{
		m_g.DrawLines(pen, points);
	}

	public override void DrawPolygon(Pen pen, PointF[] points)
	{
		m_g.DrawPolygon(pen, points);
	}

	public override void FillPolygon(SolidBrush brush, PointF[] points)
	{
		m_g.FillPolygon(brush, points);
	}

	public override SizeF MeasureString(string text, Font font)
	{
		return m_g.MeasureString(text, font);
	}

	public override SizeF MeasureString(string text, Font font, float maxWidth)
	{
		return m_g.MeasureString(text, font, (int)Math.Ceiling(maxWidth));
	}

	public override SizeF MeasureString(string text, Font font, float maxWidth, StringFormat stringFormat)
	{
		return m_g.MeasureString(text, font, (int)Math.Ceiling(maxWidth), stringFormat);
	}

	public override SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
	{
		return m_g.MeasureString(text, font, layoutArea, stringFormat);
	}

	public override void DrawString(string text, Font font, Brush brush, RectangleF rect)
	{
		m_g.DrawString(text, font, brush, rect);
	}

	public override void DrawString(string text, Font font, Brush brush, PointF location, StringFormat stringformat)
	{
		m_g.DrawString(text, font, brush, location, stringformat);
	}

	public override void DrawString(string text, Font font, Brush brush, RectangleF rect, StringFormat stringformat)
	{
		m_g.DrawString(text, font, brush, rect, stringformat);
	}
}
