using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfLine : PdfDrawElement
{
	private float m_x1;

	private float m_y1;

	private float m_x2;

	private float m_y2;

	public float X1
	{
		get
		{
			return m_x1;
		}
		set
		{
			m_x1 = value;
		}
	}

	public float Y1
	{
		get
		{
			return m_y1;
		}
		set
		{
			m_y1 = value;
		}
	}

	public float X2
	{
		get
		{
			return m_x2;
		}
		set
		{
			m_x2 = value;
		}
	}

	public float Y2
	{
		get
		{
			return m_y2;
		}
		set
		{
			m_y2 = value;
		}
	}

	public PdfLine(float x1, float y1, float x2, float y2)
	{
		m_x1 = x1;
		m_y1 = y1;
		m_x2 = x2;
		m_y2 = y2;
	}

	public PdfLine(PointF point1, PointF point2)
		: this(point1.X, point1.Y, point2.X, point2.Y)
	{
	}

	public PdfLine(PdfPen pen, float x1, float y1, float x2, float y2)
		: base(pen)
	{
		m_x1 = x1;
		m_y1 = y1;
		m_x2 = x2;
		m_y2 = y2;
	}

	public PdfLine(PdfPen pen, PointF point1, PointF point2)
		: base(pen)
	{
		m_x1 = point1.X;
		m_y1 = point1.Y;
		m_x2 = point2.X;
		m_y2 = point2.Y;
	}

	private PdfLine()
	{
	}

	protected override RectangleF GetBoundsInternal()
	{
		float num = Math.Min(X1, X2);
		float num2 = Math.Max(X1, X2);
		float num3 = Math.Min(Y1, Y2);
		float num4 = Math.Max(Y1, Y2);
		return new RectangleF(num, num3, num2 - num, num4 - num3);
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
		graphics.DrawLine(ObtainPen(), X1, Y1, X2, Y2);
	}
}
