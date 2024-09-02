using System;
using System.Collections.Generic;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class GraphicsPath : SKPath, IClone, IGraphicsPath
{
	private SKPoint m_lastPoint = StartFigurePoint;

	private PathData m_pathData = new PathData();

	private static SKPoint StartFigurePoint = new SKPoint(-1f, -1f);

	private bool m_isNeedToUpdate = true;

	public PointF[] PathPoints
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

	internal GraphicsPath(FillMode mode)
	{
		base.FillType = ((mode != FillMode.Winding) ? SKPathFillType.EvenOdd : SKPathFillType.Winding);
	}

	internal GraphicsPath()
	{
		base.FillType = SKPathFillType.EvenOdd;
	}

	internal GraphicsPath(PointF[] points, byte[] arr)
		: this()
	{
		AddPathByPoints(points, arr);
		if (arr[^1] == 5)
		{
			m_lastPoint = StartFigurePoint;
		}
	}

	internal void AddPathByPoints(PointF[] points, byte[] arr)
	{
		if (points == null || arr == null || arr.Length == 0 || points.Length == 0)
		{
			return;
		}
		m_pathData.Points = points;
		m_pathData.Types = arr;
		SKPoint[] arrayOfSKPoints = points.GetArrayOfSKPoints();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		while (num < arr.Length && num2 < points.Length)
		{
			switch ((SKPathVerb)arr[num])
			{
			case SKPathVerb.Move:
				if (num2 >= arrayOfSKPoints.Length)
				{
					flag = true;
				}
				else
				{
					MoveTo(arrayOfSKPoints[num2]);
				}
				break;
			case SKPathVerb.Line:
				if (num2 >= arrayOfSKPoints.Length)
				{
					flag = true;
				}
				else
				{
					LineTo(arrayOfSKPoints[num2]);
				}
				break;
			case SKPathVerb.Quad:
				if (num2 + 1 >= arrayOfSKPoints.Length)
				{
					flag = true;
					break;
				}
				QuadTo(arrayOfSKPoints[num2], arrayOfSKPoints[num2 + 1]);
				num2++;
				break;
			case SKPathVerb.Conic:
				if (num2 + 2 >= arrayOfSKPoints.Length)
				{
					flag = true;
					break;
				}
				ConicTo(arrayOfSKPoints[num2], arrayOfSKPoints[num2 + 1], arrayOfSKPoints[num2 + 2].Y);
				num2 += 2;
				flag2 = true;
				break;
			case SKPathVerb.Cubic:
				if (num2 + 2 >= arrayOfSKPoints.Length)
				{
					flag = true;
					break;
				}
				CubicTo(arrayOfSKPoints[num2], arrayOfSKPoints[num2 + 1], arrayOfSKPoints[num2 + 2]);
				num2 += 2;
				break;
			case SKPathVerb.Close:
				Close();
				break;
			}
			if (flag)
			{
				break;
			}
			m_lastPoint = arrayOfSKPoints[num2 - (flag2 ? 1 : 0)];
			num++;
			num2++;
		}
		if (arr[^1] == 5)
		{
			m_lastPoint = StartFigurePoint;
		}
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
					m_pathData.AddPoint(array[0].GetPointF());
					m_pathData.AddPointType(0);
					if (isClose && !flag2)
					{
						Close();
					}
					result = (m_lastPoint = array[0]);
					flag2 = false;
					break;
				case SKPathVerb.Line:
					m_pathData.AddPoint(array[1].GetPointF());
					m_pathData.AddPointType(1);
					result = (m_lastPoint = array[1]);
					break;
				case SKPathVerb.Quad:
					m_pathData.AddPoint(array[1].GetPointF(), array[2].GetPointF());
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
						result = (m_lastPoint = points[points.Count - 1].GetSKPoint());
					}
					else
					{
						m_pathData.AddPoint(array[1].GetPointF(), array[2].GetPointF(), new PointF(-1f, iterator.ConicWeight()));
						m_pathData.AddPointType(3);
						result = (m_lastPoint = array[2]);
					}
					break;
				}
				case SKPathVerb.Cubic:
					m_pathData.AddPoint(array[1].GetPointF(), array[2].GetPointF(), array[3].GetPointF());
					m_pathData.AddPointType(4);
					result = (m_lastPoint = array[3]);
					break;
				case SKPathVerb.Close:
					m_pathData.AddPoint(array[0].GetPointF());
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

	public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		AddArc(new RectangleF(x, y, width, height), startAngle, sweepAngle);
	}

	public void AddArc(RectangleF rect, float startAngle, float sweepAngle)
	{
		SKRect sKRect = RenderHelper.SKRect(rect);
		SKPoint pointOnAngle = GetPointOnAngle(sKRect, startAngle);
		if (!m_lastPoint.Equals(StartFigurePoint) && !pointOnAngle.Equals(m_lastPoint))
		{
			LineTo(pointOnAngle);
		}
		else if (m_lastPoint.X - pointOnAngle.X > 1f || m_lastPoint.X - pointOnAngle.X < -1f || m_lastPoint.Y - pointOnAngle.Y > 1f || m_lastPoint.Y - pointOnAngle.Y < -1f)
		{
			MoveTo(pointOnAngle);
		}
		ArcTo(sKRect, startAngle, sweepAngle, forceMoveTo: false);
		pointOnAngle = GetFinalPoint(sKRect.Location, sKRect.Width, sKRect.Height, startAngle, sweepAngle);
		NextPathPoint(pointOnAngle);
	}

	private SKPoint GetPointOnAngle(SKRect rect, float angle)
	{
		float num = rect.Left + rect.Width / 2f;
		float num2 = rect.Top + rect.Height / 2f;
		float num3 = rect.Width / 2f;
		float num4 = rect.Height / 2f;
		float x = num + num3 * (float)Math.Cos(Math.PI * (double)angle / 180.0);
		float y = num2 + num4 * (float)Math.Sin(Math.PI * (double)angle / 180.0);
		return new SKPoint(x, y);
	}

	private SKPoint GetFinalPoint(SKPoint startPoint, float width, float height, double startAngle, double sweepAngle)
	{
		SKPoint radius = new SKPoint(width / 2f, height / 2f);
		double angle = startAngle + sweepAngle;
		int num = ((!(sweepAngle < 0.0)) ? 1 : (-1));
		startAngle = UnstretchAngle(startAngle, radius);
		angle = UnstretchAngle(angle, radius);
		int val = (int)Math.Floor((double)(2 * num) * (angle - startAngle) / Math.PI) + 1;
		val = Math.Min(val, 4);
		double num2 = startAngle + (double)val * Math.PI / 2.0 * (double)num;
		num2 = (double)num * Math.Min((double)num * num2, (double)num * angle);
		SKPoint result = default(SKPoint);
		result.X = (float)(Math.Cos(num2) + 1.0) * radius.X + startPoint.X;
		result.Y = (float)(Math.Sin(num2) + 1.0) * radius.Y + startPoint.Y;
		return result;
	}

	private double UnstretchAngle(double angle, SKPoint radius)
	{
		double num = Math.PI * angle / 180.0;
		if (Math.Abs(Math.Cos(num)) < 1E-05 || Math.Abs(Math.Sin(num)) < 1E-05)
		{
			return num;
		}
		double num2 = Math.Atan2(Math.Sin(num) / (double)Math.Abs(radius.Y), Math.Cos(num) / (double)Math.Abs(radius.X));
		int num3 = (int)Math.Round(num / (Math.PI * 2.0), MidpointRounding.AwayFromZero) - (int)Math.Round(num2 / (Math.PI * 2.0), MidpointRounding.AwayFromZero);
		return num2 + (double)num3 * Math.PI * 2.0;
	}

	public void AddLine(PointF pointF1, PointF pointF2)
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

	public void AddLine(float pointX1, float pointY1, float pointX2, float pointY2)
	{
		AddLine(new PointF(pointX1, pointY1), new PointF(pointX2, pointY2));
	}

	public void AddLines(PointF[] linePoints)
	{
		AddArrayOfSKPointsInLines(linePoints.GetArrayOfSKPoints());
	}

	internal void AddLines(Point[] linePoints)
	{
		AddArrayOfSKPointsInLines(linePoints.GetArrayOfSKPoints());
	}

	private void AddArrayOfSKPointsInLines(SKPoint[] linePoints)
	{
		if (linePoints != null && linePoints.Length != 0)
		{
			if (m_lastPoint.Equals(StartFigurePoint) && (m_lastPoint.X - linePoints[0].X > 1f || m_lastPoint.X - linePoints[0].X < -1f || m_lastPoint.Y - linePoints[0].Y > 1f || m_lastPoint.Y - linePoints[0].Y < -1f))
			{
				MoveTo(linePoints[0]);
			}
			else
			{
				LineTo(linePoints[0]);
			}
			for (int i = 1; i < linePoints.Length; i++)
			{
				LineTo(linePoints[i]);
			}
			NextPathPoint(linePoints[^1]);
		}
	}

	public void AddCurve(PointF[] points)
	{
		AddArrayOfSKPointsInCurve(points.GetArrayOfSKPoints());
	}

	internal void AddCurve(Point[] points)
	{
		AddArrayOfSKPointsInCurve(points.GetArrayOfSKPoints());
	}

	private void AddArrayOfSKPointsInCurve(SKPoint[] points)
	{
		if (points.Length <= 1)
		{
			throw new ArgumentException("Parameter is not valid.");
		}
		if (!m_lastPoint.Equals(StartFigurePoint) && !points[0].Equals(m_lastPoint))
		{
			LineTo(points[0]);
		}
		else if (m_lastPoint.X - points[0].X > 1f || m_lastPoint.X - points[0].X < -1f || m_lastPoint.Y - points[0].Y > 1f || m_lastPoint.Y - points[0].Y < -1f)
		{
			MoveTo(points[0]);
		}
		GetCurveControlPoints(points, out var firstControlPoints, out var secondControlPoints);
		SKPoint[] array = new SKPoint[4];
		for (int i = 0; i < points.Length - 1; i++)
		{
			array[0] = points[i];
			array[1] = firstControlPoints[i];
			array[2] = secondControlPoints[i];
			array[3] = points[i + 1];
			AddArrayOfSKPointsInBezier(array, 0);
		}
		NextPathPoint(array[3]);
	}

	private void GetCurveControlPoints(SKPoint[] points, out SKPoint[] firstControlPoints, out SKPoint[] secondControlPoints)
	{
		if (points == null)
		{
			throw new ArgumentNullException("knots");
		}
		int num = points.Length - 1;
		if (num < 1)
		{
			throw new ArgumentException("At least two points required", "knots");
		}
		if (num == 1)
		{
			firstControlPoints = new SKPoint[1];
			firstControlPoints[0].X = (2f * points[0].X + points[1].X) / 3f;
			firstControlPoints[0].Y = (2f * points[0].Y + points[1].Y) / 3f;
			secondControlPoints = new SKPoint[1];
			secondControlPoints[0].X = 2f * firstControlPoints[0].X - points[0].X;
			secondControlPoints[0].Y = 2f * firstControlPoints[0].Y - points[0].Y;
			return;
		}
		double[] array = new double[num];
		for (int i = 1; i < num - 1; i++)
		{
			array[i] = 4f * points[i].X + 2f * points[i + 1].X;
		}
		array[0] = points[0].X + 2f * points[1].X;
		array[num - 1] = (double)(8f * points[num - 1].X + points[num].X) / 2.0;
		double[] firstControlPoints2 = GetFirstControlPoints(array);
		for (int j = 1; j < num - 1; j++)
		{
			array[j] = 4f * points[j].Y + 2f * points[j + 1].Y;
		}
		array[0] = points[0].Y + 2f * points[1].Y;
		array[num - 1] = (double)(8f * points[num - 1].Y + points[num].Y) / 2.0;
		double[] firstControlPoints3 = GetFirstControlPoints(array);
		firstControlPoints = new SKPoint[num];
		secondControlPoints = new SKPoint[num];
		for (int k = 0; k < num; k++)
		{
			firstControlPoints[k] = new SKPoint((float)firstControlPoints2[k], (float)firstControlPoints3[k]);
			if (k < num - 1)
			{
				secondControlPoints[k] = new SKPoint(2f * points[k + 1].X - (float)firstControlPoints2[k + 1], 2f * points[k + 1].Y - (float)firstControlPoints3[k + 1]);
			}
			else
			{
				secondControlPoints[k] = new SKPoint((points[num].X + (float)firstControlPoints2[num - 1]) / 2f, (points[num].Y + (float)firstControlPoints3[num - 1]) / 2f);
			}
		}
	}

	private double[] GetFirstControlPoints(double[] rhs)
	{
		int num = rhs.Length;
		double[] array = new double[num];
		double[] array2 = new double[num];
		double num2 = 2.0;
		array[0] = rhs[0] / num2;
		for (int i = 1; i < num; i++)
		{
			array2[i] = 1.0 / num2;
			num2 = ((i < num - 1) ? 4.0 : 3.5) - array2[i];
			array[i] = (rhs[i] - array[i - 1]) / num2;
		}
		for (int j = 1; j < num; j++)
		{
			array[num - j - 1] -= array2[num - j] * array[num - j];
		}
		return array;
	}

	public void AddBeziers(PointF[] points)
	{
		int num = points.Length;
		SKPoint[] array = new SKPoint[4];
		if (num == 4 || (num > 4 && (num - 1) % 3 == 0))
		{
			num = (num - 1) / 3;
			byte b = 0;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				b = 0;
				if (i == 0)
				{
					b = (byte)(b | 1u);
				}
				if (i == num - 1)
				{
					b = (byte)(b | 2u);
				}
				array[0] = points[num2].GetSKPoint();
				array[1] = points[num2 + 1].GetSKPoint();
				array[2] = points[num2 + 2].GetSKPoint();
				array[3] = points[num2 + 3].GetSKPoint();
				AddArrayOfSKPointsInBezier(array, b);
				num2 += 3;
			}
			return;
		}
		throw new ArgumentException("Parameter is not valid.");
	}

	public void AddBezier(PointF point1, PointF point2, PointF point3, PointF point4)
	{
		AddArrayOfSKPointsInBezier(new SKPoint[4]
		{
			point1.GetSKPoint(),
			point2.GetSKPoint(),
			point3.GetSKPoint(),
			point4.GetSKPoint()
		}, 3);
	}

	internal void AddBezier(float f0, float f1, float f2, float f3, float f4, float f5, float f6, float f7)
	{
		AddArrayOfSKPointsInBezier(new SKPoint[4]
		{
			new SKPoint(f0, f1),
			new SKPoint(f2, f3),
			new SKPoint(f4, f5),
			new SKPoint(f6, f7)
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

	public void AddEllipse(Rectangle rect)
	{
		AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public void AddEllipse(float x, float y, float width, float height)
	{
		AddOval(new SKRect(x, y, x + width, y + height));
		NextPathPoint();
	}

	public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		SKRect oval = new SKRect(x, y, x + width, y + height);
		double num = (double)x + (double)width / 2.0;
		double num2 = (double)y + (double)height / 2.0;
		_ = width / 2f;
		_ = height / 2f;
		if (sweepAngle != 0f)
		{
			SKPoint finalPoint = GetFinalPoint(new SKPoint(x, y), width, height, 0.0, startAngle);
			SKPoint finalPoint2 = GetFinalPoint(new SKPoint(x, y), width, height, startAngle, sweepAngle);
			AddArc(oval, startAngle, sweepAngle);
			LineTo(finalPoint2);
			LineTo((float)num, (float)num2);
			LineTo(finalPoint);
			Close();
			NextPathPoint();
		}
	}

	internal void AddPie(Rectangle rect, float startAngle, float sweepAngle)
	{
		AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
	}

	public void AddRectangle(Rectangle rectangle)
	{
		AddRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
		NextPathPoint();
	}

	public void AddRectangle(RectangleF rectangle)
	{
		AddRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
		NextPathPoint();
	}

	public void AddPolygon(PointF[] points)
	{
		AddPoly(points.GetArrayOfSKPoints());
		NextPathPoint();
	}

	internal void AddPolygon(Point[] points)
	{
		AddPoly(points.GetArrayOfSKPoints());
		NextPathPoint();
	}

	internal void AddString(string s, FontFamily family, int style, float emSize, RectangleF layoutRect, StringFormat format)
	{
		Font font = new Font(family.Name, emSize, style);
		List<Tuple<string, SKRect>> list = new Graphics().SplitText(s, font, layoutRect.Width, format);
		FontExtension fontExtension = new FontExtension(family.Name, emSize, font.Style, Graphics.CurrentGraphicsUnit);
		fontExtension.GetFontMetrics(out var metrics);
		float num = ((metrics.Top < 0f) ? (0f - metrics.Top) : metrics.Top);
		float num2 = num;
		foreach (Tuple<string, SKRect> item in list)
		{
			if (num2 - num > layoutRect.Height)
			{
				break;
			}
			SKPath textPath = fontExtension.GetTextPath(item.Item1, layoutRect.X + item.Item2.Location.X, layoutRect.Y + item.Item2.Location.Y + num2);
			AddPath(textPath);
			num2 += item.Item2.Height;
		}
		NextPathPoint();
	}

	internal void AddString(string s, FontFamily family, int style, float emSize, PointF origin, StringFormat format)
	{
		AddString(s, family.Name, style, emSize, origin, format);
	}

	public void AddString(string s, string familyName, int style, float emSize, PointF origin, StringFormat format)
	{
		SKPath textPath = new FontExtension(familyName, emSize, (FontStyle)style, Graphics.CurrentGraphicsUnit).GetTextPath(s, origin.X, origin.Y);
		AddPath(textPath);
		NextPathPoint();
	}

	internal void AddPath(GraphicsPath addingPath, bool connect)
	{
		if (addingPath.PathData == null || addingPath.Points.Length == 0)
		{
			return;
		}
		if (connect && PathData != null && base.Points.Length != 0 && !StartFigurePoint.Equals(m_lastPoint) && PathTypes[PathTypes.Length - 1] != 5)
		{
			if (!base.Points[base.Points.Length - 1].Equals(m_lastPoint))
			{
				MoveTo(m_lastPoint);
			}
		}
		else
		{
			connect = false;
		}
		AddPath(addingPath, connect ? SKPathAddMode.Extend : SKPathAddMode.Append);
		IterateAndFillThroughPathElements(isClose: false, update: true);
		m_isNeedToUpdate = true;
	}

	public void CloseFigure()
	{
		Close();
		NextPathPoint();
	}

	public void CloseAllFigures()
	{
		IterateAndFillThroughPathElements(isClose: true, update: true);
		SKPathFillType fillType = base.FillType;
		Reset();
		base.FillType = fillType;
		AddPathByPoints(PathData.Points, PathData.Types);
		m_isNeedToUpdate = true;
		m_lastPoint = StartFigurePoint;
	}

	public void StartFigure()
	{
		m_lastPoint = StartFigurePoint;
	}

	internal PointF GetLastPoint()
	{
		if (StartFigurePoint.Equals(m_lastPoint))
		{
			if (base.PointCount == 0)
			{
				return PointF.Empty;
			}
			SKPoint sKPoint = IterateAndFillThroughPathElements(isClose: false, update: true);
			m_isNeedToUpdate = true;
			return new PointF(sKPoint.X, sKPoint.Y);
		}
		return new PointF(m_lastPoint.X, m_lastPoint.Y);
	}

	public object Clone()
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

	public RectangleF GetBounds()
	{
		SKRect rect = default(SKRect);
		if (GetBounds(out rect))
		{
			return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
		}
		return RectangleF.Empty;
	}

	public void Transform(Matrix matrix)
	{
		SKPoint lastPoint = m_lastPoint;
		Transform(matrix.GetSKMatrix());
		IterateAndFillThroughPathElements(isClose: false, update: true);
		m_isNeedToUpdate = true;
		if (StartFigurePoint.Equals(lastPoint))
		{
			m_lastPoint = lastPoint;
		}
	}

    /*
	 int IGraphicsPath.get_PointCount()
	{
		return base.PointCount;
	}
	 */

    void IGraphicsPath.Reset()
	{
		Reset();
	}
}
