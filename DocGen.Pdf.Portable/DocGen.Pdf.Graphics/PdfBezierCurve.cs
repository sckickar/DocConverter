using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfBezierCurve : PdfDrawElement
{
	private PointF m_startPoint = PointF.Empty;

	private PointF m_firstControlPoint = PointF.Empty;

	private PointF m_secondControlPoint = PointF.Empty;

	private PointF m_endPoint = PointF.Empty;

	public PointF StartPoint
	{
		get
		{
			return m_startPoint;
		}
		set
		{
			m_startPoint = value;
		}
	}

	public PointF FirstControlPoint
	{
		get
		{
			return m_firstControlPoint;
		}
		set
		{
			m_firstControlPoint = value;
		}
	}

	public PointF SecondControlPoint
	{
		get
		{
			return m_secondControlPoint;
		}
		set
		{
			m_secondControlPoint = value;
		}
	}

	public PointF EndPoint
	{
		get
		{
			return m_endPoint;
		}
		set
		{
			m_endPoint = value;
		}
	}

	public PdfBezierCurve(PointF startPoint, PointF firstControlPoint, PointF secondControlPoint, PointF endPoint)
	{
		m_startPoint = startPoint;
		m_firstControlPoint = firstControlPoint;
		m_secondControlPoint = secondControlPoint;
		m_endPoint = endPoint;
	}

	public PdfBezierCurve(float startPointX, float startPointY, float firstControlPointX, float firstControlPointY, float secondControlPointX, float secondControlPointY, float endPointX, float endPointY)
	{
		m_startPoint.X = startPointX;
		m_startPoint.Y = startPointY;
		m_firstControlPoint.X = firstControlPointX;
		m_firstControlPoint.Y = firstControlPointY;
		m_secondControlPoint.X = secondControlPointX;
		m_secondControlPoint.Y = secondControlPointY;
		m_endPoint.X = endPointX;
		m_endPoint.Y = endPointY;
	}

	public PdfBezierCurve(PdfPen pen, PointF startPoint, PointF firstControlPoint, PointF secondControlPoint, PointF endPoint)
		: base(pen)
	{
		m_startPoint = startPoint;
		m_firstControlPoint = firstControlPoint;
		m_secondControlPoint = secondControlPoint;
		m_endPoint = endPoint;
	}

	public PdfBezierCurve(PdfPen pen, float startPointX, float startPointY, float firstControlPointX, float firstControlPointY, float secondControlPointX, float secondControlPointY, float endPointX, float endPointY)
		: base(pen)
	{
		m_startPoint.X = startPointX;
		m_startPoint.Y = startPointY;
		m_firstControlPoint.X = firstControlPointX;
		m_firstControlPoint.Y = firstControlPointY;
		m_secondControlPoint.X = secondControlPointX;
		m_secondControlPoint.Y = secondControlPointY;
		m_endPoint.X = endPointX;
		m_endPoint.Y = endPointY;
	}

	protected PdfBezierCurve()
	{
	}

	protected override RectangleF GetBoundsInternal()
	{
		throw new NotImplementedException();
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
		graphics.DrawBezier(ObtainPen(), StartPoint, FirstControlPoint, SecondControlPoint, EndPoint);
	}
}
