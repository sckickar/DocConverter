using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfEllipse : PdfRectangleArea
{
	public float RadiusX => base.Width / 2f;

	public float RadiusY => base.Height / 2f;

	public PointF Center => new PointF(base.X + RadiusX, base.Y + RadiusY);

	public PdfEllipse(float width, float height)
		: this(0f, 0f, width, height)
	{
	}

	public PdfEllipse(PdfPen pen, float width, float height)
		: this(pen, 0f, 0f, width, height)
	{
	}

	public PdfEllipse(PdfBrush brush, float width, float height)
		: this(brush, 0f, 0f, width, height)
	{
	}

	public PdfEllipse(PdfPen pen, PdfBrush brush, float width, float height)
		: this(pen, brush, 0f, 0f, width, height)
	{
	}

	public PdfEllipse(float x, float y, float width, float height)
		: base(x, y, width, height)
	{
	}

	public PdfEllipse(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfEllipse(PdfPen pen, float x, float y, float width, float height)
		: base(pen, null, x, y, width, height)
	{
	}

	public PdfEllipse(PdfPen pen, RectangleF rectangle)
		: base(pen, null, rectangle)
	{
	}

	public PdfEllipse(PdfBrush brush, float x, float y, float width, float height)
		: base(null, brush, x, y, width, height)
	{
	}

	public PdfEllipse(PdfBrush brush, RectangleF rectangle)
		: base(null, brush, rectangle)
	{
	}

	public PdfEllipse(PdfPen pen, PdfBrush brush, float x, float y, float width, float height)
		: base(pen, brush, x, y, width, height)
	{
	}

	public PdfEllipse(PdfPen pen, PdfBrush brush, RectangleF rectangle)
		: base(pen, brush, rectangle)
	{
	}

	protected PdfEllipse()
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
		graphics.DrawEllipse(ObtainPen(), base.Brush, base.Bounds);
	}
}
