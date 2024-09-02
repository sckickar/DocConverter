using SkiaSharp;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartTitle : Control
{
	public Font Font { get; set; }

	public string Name { get; set; }

	public string Text { get; set; }

	public int Margin { get; set; }

	public bool ShowBorder { get; set; }

	internal Size TextSize { get; set; }

	public LineInfo Border { get; set; }

	public Color ForeColor { get; set; }

	public BrushInfo BackInterior { get; set; }

	internal RectangleF rectangleF { get; set; }

	internal bool IsManualLayout { get; set; }

	public ChartTitle()
	{
		base.Visible = true;
		Font = SystemFonts.DefaultFont;
		Behavior = ChartDockingFlags.Dockable;
		Position = ChartDock.Top;
		ForeColor = Color.Black;
		Alignment = ChartAlignment.Center;
	}

	public override SizeF Measure(Graphics g, SizeF size)
	{
		SizeF value = base.Size;
		if (base.Orientation == ChartOrientation.Vertical)
		{
			TextSize = Size.Ceiling(g.MeasureString(Text, Font, (int)size.Height));
			value = Size.Ceiling(new SizeF(TextSize.Height + 2 * Margin, TextSize.Width + 2 * Margin));
		}
		else
		{
			if (IsManualLayout)
			{
				SKRect sKRect = g.MeasureString(Text, Font, StringFormat.GenericDefault);
				TextSize = Size.Ceiling(new SizeF(sKRect.Width, sKRect.Height));
			}
			else
			{
				TextSize = Size.Ceiling(g.MeasureString(Text, Font, (int)size.Width));
			}
			value = new SizeF(TextSize.Width + 2 * Margin, TextSize.Height + 2 * Margin);
		}
		Size size3 = (base.Size = Size.Ceiling(value));
		return size3;
	}

	public override void Render(Graphics g)
	{
		if (IsManualLayout && rectangleF != new RectangleF(-1f, -1f, -1f, -1f))
		{
			Draw(g, rectangleF);
		}
		else
		{
			Draw(g, new Rectangle(base.Location, base.Size));
		}
	}

	private void Draw(Graphics g, RectangleF rect)
	{
		if (!base.Visible)
		{
			return;
		}
		string text = Text;
		RectangleF rectangleF = new RectangleF(rect.X + (float)Margin, rect.Y + (float)Margin, TextSize.Width, TextSize.Height);
		StringFormat stringFormat = new StringFormat();
		BrushPaint.FillRectangle(g, rectangleF, BackInterior);
		switch (Alignment)
		{
		case ChartAlignment.Near:
			stringFormat.Alignment = StringAlignment.Near;
			break;
		case ChartAlignment.Center:
			stringFormat.Alignment = StringAlignment.Center;
			break;
		case ChartAlignment.Far:
			stringFormat.Alignment = StringAlignment.Far;
			break;
		}
		if (text != null && text != string.Empty)
		{
			using SolidBrush brush = new SolidBrush(ForeColor);
			if (base.Orientation == ChartOrientation.Vertical)
			{
				Matrix transform = g.Transform;
				if (Position == ChartDock.Right)
				{
					PointF pointF = new PointF(rectangleF.X + rectangleF.Height, rectangleF.Y);
					PointF pointF2 = new PointF(rectangleF.X + rectangleF.Height, rectangleF.Y + rectangleF.Width);
					PointF pointF3 = new PointF(rectangleF.X, rectangleF.Y);
					g.MultiplyTransform(new Matrix(rectangleF, new PointF[3] { pointF, pointF2, pointF3 }));
					g.DrawString(text, Font, brush, rectangleF, stringFormat);
				}
				else
				{
					PointF pointF4 = new PointF(rectangleF.X, rectangleF.Y + rectangleF.Width);
					PointF pointF5 = new PointF(rectangleF.X, rectangleF.Y);
					PointF pointF6 = new PointF(rectangleF.X + rectangleF.Height, rectangleF.Y + rectangleF.Width);
					g.MultiplyTransform(new Matrix(rectangleF, new PointF[3] { pointF4, pointF5, pointF6 }));
					g.DrawString(text, Font, brush, rectangleF, stringFormat);
				}
				g.Transform = transform;
			}
			else
			{
				g.DrawString(text, Font, brush, rectangleF, stringFormat);
			}
		}
		if (ShowBorder)
		{
			g.DrawRectangle(Border.Pen, rectangleF.X, rectangleF.Y, rectangleF.Width + 1f, rectangleF.Height + 1f);
		}
	}
}
