using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public abstract class PdfEllipsePart : PdfRectangleArea
{
	private float m_startAngle;

	private float m_sweepAngle;

	public float StartAngle
	{
		get
		{
			return m_startAngle;
		}
		set
		{
			m_startAngle = value;
		}
	}

	public float SweepAngle
	{
		get
		{
			return m_sweepAngle;
		}
		set
		{
			m_sweepAngle = value;
		}
	}

	protected PdfEllipsePart()
	{
	}

	protected PdfEllipsePart(float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(x, y, width, height)
	{
		m_startAngle = startAngle;
		m_sweepAngle = sweepAngle;
	}

	protected PdfEllipsePart(RectangleF rectangle, float startAngle, float sweepAngle)
		: base(rectangle)
	{
		m_startAngle = startAngle;
		m_sweepAngle = sweepAngle;
	}

	protected PdfEllipsePart(PdfPen pen, PdfBrush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		: base(pen, brush, x, y, width, height)
	{
		m_startAngle = startAngle;
		m_sweepAngle = sweepAngle;
	}

	protected PdfEllipsePart(PdfPen pen, PdfBrush brush, RectangleF rectangle, float startAngle, float sweepAngle)
		: base(pen, brush, rectangle)
	{
		m_startAngle = startAngle;
		m_sweepAngle = sweepAngle;
	}
}
