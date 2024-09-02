using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfRectangle : PdfRectangleArea
{
	public PdfRectangle(float width, float height)
		: this(0f, 0f, width, height)
	{
	}

	public PdfRectangle(PdfPen pen, float width, float height)
		: this(pen, 0f, 0f, width, height)
	{
	}

	public PdfRectangle(PdfBrush brush, float width, float height)
		: this(brush, 0f, 0f, width, height)
	{
	}

	public PdfRectangle(PdfPen pen, PdfBrush brush, float width, float height)
		: this(pen, brush, 0f, 0f, width, height)
	{
	}

	public PdfRectangle(float x, float y, float width, float height)
		: base(x, y, width, height)
	{
	}

	public PdfRectangle(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfRectangle(PdfPen pen, float x, float y, float width, float height)
		: base(pen, null, x, y, width, height)
	{
	}

	public PdfRectangle(PdfPen pen, RectangleF rectangle)
		: base(pen, null, rectangle)
	{
	}

	public PdfRectangle(PdfBrush brush, float x, float y, float width, float height)
		: base(null, brush, x, y, width, height)
	{
	}

	public PdfRectangle(PdfBrush brush, RectangleF rectangle)
		: base(null, brush, rectangle)
	{
	}

	public PdfRectangle(PdfPen pen, PdfBrush brush, float x, float y, float width, float height)
		: base(pen, brush, x, y, width, height)
	{
	}

	public PdfRectangle(PdfPen pen, PdfBrush brush, RectangleF rectangle)
		: base(pen, brush, rectangle)
	{
	}

	private PdfRectangle()
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
		graphics.DrawRectangle(ObtainPen(), base.Brush, base.Bounds);
	}
}
