using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfPath : PdfFillElement
{
	private List<PointF> m_points;

	private List<byte> m_pathTypes;

	private bool m_bStartFigure = true;

	private PdfFillMode m_fillMode = PdfFillMode.Alternate;

	private bool isBeziers3;

	internal bool isXps;

	public PdfFillMode FillMode
	{
		get
		{
			return m_fillMode;
		}
		set
		{
			m_fillMode = value;
		}
	}

	public PointF[] PathPoints => Points.ToArray();

	public byte[] PathTypes => Types.ToArray();

	public int PointCount
	{
		get
		{
			int result = 0;
			if (m_points != null)
			{
				result = m_points.Count;
			}
			return result;
		}
	}

	public PointF LastPoint => GetLastPoint();

	internal List<PointF> Points
	{
		get
		{
			if (m_points == null)
			{
				m_points = new List<PointF>();
			}
			return m_points;
		}
	}

	internal List<byte> Types
	{
		get
		{
			if (m_pathTypes == null)
			{
				m_pathTypes = new List<byte>();
			}
			return m_pathTypes;
		}
	}

	public PdfPath()
	{
	}

	public PdfPath(PointF[] points, byte[] pathTypes)
	{
		AddPath(points, pathTypes);
	}

	public PdfPath(PdfPen pen)
		: base(pen)
	{
	}

	public PdfPath(PdfBrush brush)
		: base(brush)
	{
	}

	public PdfPath(PdfBrush brush, PdfFillMode fillMode)
		: this(brush)
	{
		FillMode = fillMode;
	}

	public PdfPath(PdfPen pen, PointF[] points, byte[] pathTypes)
		: base(pen)
	{
		AddPath(points, pathTypes);
	}

	public PdfPath(PdfBrush brush, PdfFillMode fillMode, PointF[] points, byte[] pathTypes)
		: base(brush)
	{
		AddPath(points, pathTypes);
		FillMode = fillMode;
	}

	public PdfPath(PdfPen pen, PdfBrush brush, PdfFillMode fillMode)
		: base(pen, brush)
	{
		FillMode = fillMode;
	}

	public void AddArc(RectangleF rectangle, float startAngle, float sweepAngle)
	{
		AddArc(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		List<float[]> bezierArcPoints = PdfGraphics.GetBezierArcPoints(x, y, x + width, y + height, startAngle, sweepAngle);
		List<float> list = new List<float>(8);
		for (int i = 0; i < bezierArcPoints.Count; i++)
		{
			float[] collection = bezierArcPoints[i];
			list.Clear();
			list.AddRange(collection);
			AddPoints(list, PathPointType.Bezier3);
		}
	}

	public void AddBezier(PointF startPoint, PointF firstControlPoint, PointF secondControlPoint, PointF endPoint)
	{
		AddBezier(startPoint.X, startPoint.Y, firstControlPoint.X, firstControlPoint.Y, secondControlPoint.X, secondControlPoint.Y, endPoint.X, endPoint.Y);
	}

	public void AddBezier(float startPointX, float startPointY, float firstControlPointX, float firstControlPointY, float secondControlPointX, float secondControlPointY, float endPointX, float endPointY)
	{
		List<float> list = new List<float>(8);
		list.Add(startPointX);
		list.Add(startPointY);
		list.Add(firstControlPointX);
		list.Add(firstControlPointY);
		list.Add(secondControlPointX);
		list.Add(secondControlPointY);
		list.Add(endPointX);
		list.Add(endPointY);
		AddPoints(list, PathPointType.Bezier3);
	}

	internal void AddBeziers(List<PointF> points)
	{
		List<float> list = new List<float>();
		int num = 0;
		for (int i = 0; i < points.Count; i++)
		{
			list.Add(points[i].X);
			list.Add(points[i].Y);
			if (num == 4)
			{
				isBeziers3 = true;
				AddPoints(list, PathPointType.Bezier3);
				list = new List<float>();
				num = 0;
				isBeziers3 = false;
			}
		}
		if (list.Count > 0)
		{
			isBeziers3 = true;
			AddPoints(list, PathPointType.Bezier3);
			isBeziers3 = false;
		}
	}

	public void AddEllipse(RectangleF rectangle)
	{
		AddEllipse(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void AddEllipse(float x, float y, float width, float height)
	{
		StartFigure();
		AddArc(x, y, width, height, 0f, 360f);
		CloseFigure();
	}

	public void AddLine(PointF point1, PointF point2)
	{
		AddLine(point1.X, point1.Y, point2.X, point2.Y);
	}

	public void AddLine(float x1, float y1, float x2, float y2)
	{
		List<float> list = new List<float>(4);
		list.Add(x1);
		list.Add(y1);
		list.Add(x2);
		list.Add(y2);
		AddPoints(list, PathPointType.Line);
	}

	public void AddPath(PdfPath path)
	{
		AddPath(path.PathPoints, path.PathTypes);
	}

	public void AddPath(PointF[] pathPoints, byte[] pathTypes)
	{
		if (pathPoints == null)
		{
			throw new ArgumentNullException("pathPoints");
		}
		if (pathTypes == null)
		{
			throw new ArgumentNullException("pathTypes");
		}
		if (pathPoints.Length != pathTypes.Length)
		{
			throw new ArgumentException("The argument arrays should be of equal length.");
		}
		Points.AddRange(pathPoints);
		Types.AddRange(pathTypes);
	}

	public void AddPie(RectangleF rectangle, float startAngle, float sweepAngle)
	{
		AddPie(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		StartFigure();
		AddArc(x, y, width, height, startAngle, sweepAngle);
		AddPoint(new PointF(x + width / 2f, y + height / 2f), PathPointType.Line);
		CloseFigure();
	}

	public void AddPolygon(PointF[] points)
	{
		List<float> list = new List<float>(points.Length * 2);
		StartFigure();
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF = points[i];
			list.Add(pointF.X);
			list.Add(pointF.Y);
		}
		AddPoints(list, PathPointType.Line);
		CloseFigure();
	}

	public void AddRectangle(RectangleF rectangle)
	{
		AddRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void AddRectangle(float x, float y, float width, float height)
	{
		List<float> list = new List<float>();
		StartFigure();
		list.Add(x);
		list.Add(y);
		list.Add(x + width);
		list.Add(y);
		list.Add(x + width);
		list.Add(y + height);
		list.Add(x);
		list.Add(y + height);
		AddPoints(list, PathPointType.Line);
		CloseFigure();
	}

	public void StartFigure()
	{
		m_bStartFigure = true;
	}

	public void CloseFigure()
	{
		if (PointCount > 0)
		{
			CloseFigure(PointCount - 1);
		}
		StartFigure();
	}

	public void CloseAllFigures()
	{
		PointF pointF = PathPoints[0];
		int i = 0;
		for (int count = m_pathTypes.Count; i < count; i++)
		{
			PathPointType pathPointType = (PathPointType)Types[i];
			bool flag = false;
			if (i != 0 && pathPointType == PathPointType.Start)
			{
				CloseFigure(i - 1);
				flag = true;
			}
			else if (i == m_pathTypes.Count - 1 && !flag && isXps && pointF.X == PathPoints[i].X)
			{
				CloseFigure(i);
			}
		}
	}

	public PointF GetLastPoint()
	{
		PointF result = PointF.Empty;
		int pointCount = PointCount;
		if (pointCount > 0 && m_points != null)
		{
			result = m_points[pointCount - 1];
		}
		return result;
	}

	internal PdfPath(PdfFillMode fillMode)
	{
		FillMode = fillMode;
	}

	internal void AddLines(PointF[] linePoints)
	{
		PointF point = linePoints[0];
		if (linePoints.Length == 1)
		{
			AddPoint(linePoints[0], PathPointType.Line);
			return;
		}
		for (int i = 1; i < linePoints.Length; i++)
		{
			PointF pointF = linePoints[i];
			AddLine(point, pointF);
			point = pointF;
		}
	}

	internal void Scale(float scaleX, float scaleY)
	{
		List<PointF> list = new List<PointF>(m_points.Count);
		for (int i = 0; i < m_points.Count; i++)
		{
			list.Add(new PointF(m_points[i].X * scaleX, m_points[i].Y * scaleY));
		}
		m_points = list;
	}

	internal void AddBeziers(PointF[] points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (points.Length < 4)
		{
			throw new ArgumentException("Incorrect size of array", "points");
		}
		int num = 3;
		int num2 = 0;
		PointF startPoint = points[num2];
		num2++;
		while (num2 + num <= points.Length)
		{
			PointF firstControlPoint = points[num2];
			num2++;
			PointF secondControlPoint = points[num2];
			num2++;
			PointF pointF = points[num2];
			num2++;
			AddBezier(startPoint, firstControlPoint, secondControlPoint, pointF);
			startPoint = pointF;
		}
	}

	protected override RectangleF GetBoundsInternal()
	{
		PointF[] pathPoints = PathPoints;
		RectangleF result = RectangleF.Empty;
		if (pathPoints.Length != 0)
		{
			float num = pathPoints[0].X;
			float num2 = pathPoints[0].X;
			float num3 = pathPoints[0].Y;
			float num4 = pathPoints[0].Y;
			int i = 1;
			for (int num5 = pathPoints.Length; i < num5; i++)
			{
				PointF pointF = pathPoints[i];
				num = Math.Min(pointF.X, num);
				num2 = Math.Max(pointF.X, num2);
				num3 = Math.Min(pointF.Y, num3);
				num4 = Math.Max(pointF.Y, num4);
			}
			result = new RectangleF(num, num3, num2 - num, num4 - num3);
		}
		return result;
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
			graphics.StructElementChanged += graphics.ApplyTag;
			graphics.CurrentTagType = "Figure";
		}
		graphics.DrawPath(ObtainPen(), base.Brush, this);
	}

	private void AddPoints(List<float> points, PathPointType pointType)
	{
		AddPoints(points, pointType, 0, points.Count);
	}

	private void AddPoints(List<float> points, PathPointType pointType, int startIndex, int endIndex)
	{
		int num;
		for (num = startIndex; num < endIndex; num++)
		{
			PointF pointF = new PointF(points[num], points[num + 1]);
			if (num == startIndex)
			{
				if (PointCount <= 0 || m_bStartFigure)
				{
					AddPoint(pointF, PathPointType.Start);
					m_bStartFigure = false;
				}
				else if (pointF != LastPoint && !isBeziers3)
				{
					AddPoint(pointF, PathPointType.Line);
				}
				else if (pointF != LastPoint)
				{
					AddPoint(pointF, PathPointType.Bezier3);
				}
			}
			else
			{
				AddPoint(pointF, pointType);
			}
			num++;
		}
	}

	private void AddPoint(PointF point, PathPointType pointType)
	{
		Points.Add(point);
		Types.Add((byte)pointType);
	}

	private void CloseFigure(int index)
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException();
		}
		PathPointType pathPointType = (PathPointType)Types[index];
		pathPointType |= PathPointType.CloseSubpath;
		Types[index] = (byte)pathPointType;
	}
}
