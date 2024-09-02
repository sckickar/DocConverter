using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfPie : PdfEllipsePart
{
	public PdfPie(float width, float height, float startAngle, float sweepAngle)
		: this(0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfPen pen, float width, float height, float startAngle, float sweepAngle)
		: this(pen, 0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfBrush brush, float width, float height, float startAngle, float sweepAngle)
		: this(brush, 0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfPen pen, PdfBrush brush, float width, float height, float startAngle, float sweepAngle)
		: this(pen, brush, 0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(x, y, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(RectangleF rectangle, float startAngle, float sweepAngle)
		: base(rectangle, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(pen, null, x, y, width, height, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfPen pen, RectangleF rectangle, float startAngle, float sweepAngle)
		: base(pen, null, rectangle, startAngle, sweepAngle)
	{
	}

	public PdfPie(PdfBrush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		: this(x, y, width, height, startAngle, sweepAngle)
	{
		base.Brush = brush;
	}

	public PdfPie(PdfBrush brush, RectangleF rectangle, float startAngle, float sweepAngle)
		: this(rectangle, startAngle, sweepAngle)
	{
		base.Brush = brush;
	}

	public PdfPie(PdfPen pen, PdfBrush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		: this(x, y, width, height, startAngle, sweepAngle)
	{
		base.Pen = pen;
		base.Brush = brush;
	}

	public PdfPie(PdfPen pen, PdfBrush brush, RectangleF rectangle, float startAngle, float sweepAngle)
		: this(rectangle, startAngle, sweepAngle)
	{
		base.Pen = pen;
		base.Brush = brush;
	}

	protected PdfPie()
	{
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (base.PdfTag != null)
		{
			graphics.Tag = base.PdfTag;
		}
		graphics.DrawPie(ObtainPen(), base.Brush, base.Bounds, base.StartAngle, base.SweepAngle);
	}
}
