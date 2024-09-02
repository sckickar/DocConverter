using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfArc : PdfEllipsePart
{
	public PdfArc(float width, float height, float startAngle, float sweepAngle)
		: this(0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfArc(PdfPen pen, float width, float height, float startAngle, float sweepAngle)
		: this(pen, 0f, 0f, width, height, startAngle, sweepAngle)
	{
	}

	public PdfArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(x, y, width, height, startAngle, sweepAngle)
	{
	}

	public PdfArc(RectangleF rectangle, float startAngle, float sweepAngle)
		: base(rectangle, startAngle, sweepAngle)
	{
	}

	public PdfArc(PdfPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(pen, null, x, y, width, height, startAngle, sweepAngle)
	{
	}

	public PdfArc(PdfPen pen, RectangleF rectangle, float startAngle, float sweepAngle)
		: base(pen, null, rectangle, startAngle, sweepAngle)
	{
	}

	protected PdfArc()
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
		graphics.DrawArc(ObtainPen(), base.Bounds, base.StartAngle, base.SweepAngle);
	}
}
