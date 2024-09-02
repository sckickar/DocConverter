using System;
using System.Collections.Generic;
using SkiaSharp;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class GraphicsPath : SKPath
{
	private SKPoint m_lastPoint = StartFigurePoint;

	private PathData m_pathData = new PathData();

	private static SKPoint StartFigurePoint = new SKPoint(-1f, -1f);

	private bool m_isNeedToUpdate = true;

	internal byte[] PathTypes
	{
		get
		{
			if (base.PointCount > 0 && (m_pathData.Points.Length == 0 || m_isNeedToUpdate))
			{
				IterateAndFillThroughPathElements(isClose: false, update: true);
				if (m_pathData.Types[m_pathData.Types.Length - 1] == 5)
				{
					m_lastPoint = StartFigurePoint;
				}
			}
			return m_pathData.Types;
		}
	}

	internal PathData PathData
	{
		get
		{
			if (base.PointCount > 0 && (m_pathData.Points.Length == 0 || m_isNeedToUpdate))
			{
				IterateAndFillThroughPathElements(isClose: false, update: true);
				if (m_pathData.Points.Length != 0 && m_pathData.Types[m_pathData.Types.Length - 1] == 5)
				{
					m_lastPoint = StartFigurePoint;
				}
			}
			return m_pathData;
		}
	}

	internal FillMode FillMode
	{
		get
		{
			if (base.FillType != 0)
			{
				return FillMode.Alternate;
			}
			return FillMode.Winding;
		}
		set
		{
			base.FillType = ((value != FillMode.Winding) ? SKPathFillType.EvenOdd : SKPathFillType.Winding);
		}
	}

	internal PointF[] PathPoints
	{
		get
		{
			if (base.PointCount > 0 && (m_pathData.Points.Length == 0 || m_isNeedToUpdate))
			{
				IterateAndFillThroughPathElements(isClose: false, update: true);
				if (m_pathData.Types[m_pathData.Types.Length - 1] == 5)
				{
					m_lastPoint = StartFigurePoint;
				}
			}
			return m_pathData.Points;
		}
	}

	internal GraphicsPath()
	{
		base.FillType = SKPathFillType.EvenOdd;
	}

	private void NextPathPoint()
	{
		IterateAndFillThroughPathElements(isClose: false, update: false);
		m_lastPoint = StartFigurePoint;
	}

	private void NextPathPoint(SKPoint currentPoint)
	{
		IterateAndFillThroughPathElements(isClose: false, update: false);
		m_lastPoint = currentPoint;
	}

	private SKPoint IterateAndFillThroughPathElements(bool isClose, bool update)
	{
		SKPoint result = SKPoint.Empty;
		if (!update)
		{
			m_isNeedToUpdate = true;
		}
		else
		{
			Iterator iterator = CreateIterator(isClose);
			SKPathVerb sKPathVerb = SKPathVerb.Move;
			SKPoint[] array = new SKPoint[4];
			bool flag = true;
			bool flag2 = true;
			m_pathData = new PathData();
			while (sKPathVerb != SKPathVerb.Done)
			{
				sKPathVerb = iterator.Next(array);
				switch (sKPathVerb)
				{
				case SKPathVerb.Move:
					m_pathData.AddPoint(GetPointF(array[0]));
					m_pathData.AddPointType(0);
					if (isClose && !flag2)
					{
						Close();
					}
					result = (m_lastPoint = array[0]);
					flag2 = false;
					break;
				case SKPathVerb.Line:
					m_pathData.AddPoint(GetPointF(array[1]));
					m_pathData.AddPointType(1);
					result = (m_lastPoint = array[1]);
					break;
				case SKPathVerb.Quad:
					m_pathData.AddPoint(GetPointF(array[1]), GetPointF(array[2]));
					m_pathData.AddPointType(2);
					result = (m_lastPoint = array[2]);
					break;
				case SKPathVerb.Conic:
				{
					List<PointF> points;
					int cubicPointsFromConic = GetCubicPointsFromConic(array[0], array[1], array[2], iterator.ConicWeight(), out points);
					if (points != null)
					{
						for (int i = 0; i < cubicPointsFromConic; i++)
						{
							m_pathData.AddPointType(4);
						}
						m_pathData.AddPoint(points.ToArray());
						result = (m_lastPoint = GetSKPoint(points[points.Count - 1]));
					}
					else
					{
						m_pathData.AddPoint(GetPointF(array[1]), GetPointF(array[2]), new PointF(-1f, iterator.ConicWeight()));
						m_pathData.AddPointType(3);
						result = (m_lastPoint = array[2]);
					}
					break;
				}
				case SKPathVerb.Cubic:
					m_pathData.AddPoint(GetPointF(array[1]), GetPointF(array[2]), GetPointF(array[3]));
					m_pathData.AddPointType(4);
					result = (m_lastPoint = array[3]);
					break;
				case SKPathVerb.Close:
					m_pathData.AddPoint(GetPointF(array[0]));
					m_pathData.AddPointType(5);
					m_lastPoint = StartFigurePoint;
					result = array[0];
					if (isClose)
					{
						Close();
					}
					break;
				default:
					if (isClose && flag)
					{
						Close();
					}
					break;
				}
				flag = ((isClose && sKPathVerb != SKPathVerb.Close && sKPathVerb != SKPathVerb.Done && !iterator.IsCloseLine() && !iterator.IsCloseContour()) ? true : false);
			}
		}
		return result;
	}

	private int GetCubicPointsFromConic(SKPoint point1, SKPoint point2, SKPoint point3, float weight, out List<PointF> points)
	{
		SKPoint[] pts;
		int num = SKPath.ConvertConicToQuads(point1, point2, point3, weight, out pts, 2);
		points = ((num == 0) ? null : new List<PointF>(num * 3));
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 2;
			points.Add(new PointF(pts[num2].X, pts[num2].Y));
			points.Add(new PointF(pts[num2 + 1].X, pts[num2 + 1].Y));
			points.Add(new PointF(pts[num2 + 2].X, pts[num2 + 2].Y));
		}
		return num;
	}

	internal PointF GetPointF(SKPoint point)
	{
		return new PointF(point.X, point.Y);
	}

	internal SKPoint GetSKPoint(PointF point)
	{
		return new SKPoint(point.X, point.Y);
	}

	internal RectangleF GetBounds()
	{
		SKRect rect = default(SKRect);
		if (GetBounds(out rect))
		{
			return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
		}
		return RectangleF.Empty;
	}

	public void AddRectangle(RectangleF rectangle)
	{
		AddRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
		NextPathPoint();
	}

	internal void AddLine(PointF pointF1, PointF pointF2)
	{
		SKPoint point = new SKPoint(pointF1.X, pointF1.Y);
		SKPoint sKPoint = new SKPoint(pointF2.X, pointF2.Y);
		if (!m_lastPoint.Equals(StartFigurePoint) && !point.Equals(m_lastPoint))
		{
			LineTo(point);
		}
		else if (m_lastPoint.X - point.X > 1f || m_lastPoint.X - point.X < -1f || m_lastPoint.Y - point.Y > 1f || m_lastPoint.Y - point.Y < -1f)
		{
			MoveTo(point);
		}
		LineTo(sKPoint);
		NextPathPoint(sKPoint);
	}

	internal void AddBezier(PointF point1, PointF point2, PointF point3, PointF point4)
	{
		AddArrayOfSKPointsInBezier(new SKPoint[4]
		{
			GetSKPoint(point1),
			GetSKPoint(point2),
			GetSKPoint(point3),
			GetSKPoint(point4)
		}, 3);
	}

	private void AddArrayOfSKPointsInBezier(SKPoint[] points, byte pos)
	{
		if (points.Length != 4)
		{
			throw new ArgumentException("Parameter is not valid.");
		}
		if (((uint)pos & (true ? 1u : 0u)) != 0)
		{
			if (!m_lastPoint.Equals(StartFigurePoint) && !points[0].Equals(m_lastPoint))
			{
				LineTo(points[0]);
			}
			else if (m_lastPoint.X - points[0].X > 1f || m_lastPoint.X - points[0].X < -1f || m_lastPoint.Y - points[0].Y > 1f || m_lastPoint.Y - points[0].Y < -1f)
			{
				MoveTo(points[0]);
			}
		}
		CubicTo(points[1], points[2], points[3]);
		if ((pos & 2u) != 0)
		{
			NextPathPoint(points[3]);
		}
	}

	internal void CloseFigure()
	{
		Close();
		NextPathPoint();
	}

	internal void StartFigure()
	{
		m_lastPoint = StartFigurePoint;
	}

	internal object Clone()
	{
		GraphicsPath graphicsPath = (GraphicsPath)MemberwiseClone();
		if (m_isNeedToUpdate)
		{
			IterateAndFillThroughPathElements(isClose: false, update: true);
		}
		graphicsPath.m_pathData = new PathData();
		m_pathData.Points.CopyTo(graphicsPath.m_pathData.Points, 0);
		m_pathData.Types.CopyTo(graphicsPath.m_pathData.Types, 0);
		return graphicsPath;
	}
}
