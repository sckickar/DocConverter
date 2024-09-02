using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class SplineAreaRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	public SplineAreaRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array = CalculateUnEmptyRanges(vrange);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].To - array[i].From > 0)
			{
				Render(g, array[i].From, array[i].To - array[i].From + 1);
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array = CalculateUnEmptyRanges(vrange);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].To - array[i].From > 0)
			{
				Render(g, array[i].From, array[i].To - array[i].From + 1);
			}
		}
	}

	public void Render(Graphics g, int from, int count)
	{
		bool series3D = base.Chart.Series3D;
		base.Chart.Series.IndexOf(m_series);
		BrushInfo brush = GetBrush();
		Pen gdipPen = base.SeriesStyle.GdipPen;
		ArrayList arrayList = new ArrayList(2);
		double origin = m_series.ActualYAxis.Origin;
		ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
		chartRenderArgs2D.Graph = new ChartGDIGraph(g);
		_ = g.ClipBounds;
		SizeF seriesOffset = GetSeriesOffset();
		SizeF thisOffset = GetThisOffset();
		_ = CustomOriginY;
		_ = thisOffset.Height;
		ChartPointWithIndex[] array = new ChartPointWithIndex[count];
		Array.Copy(PrepearePoints(), from, array, 0, count);
		int i = 0;
		int num = 0;
		for (; i < count - 1; i++)
		{
			if (array[i].Point.X == array[i + 1].Point.X)
			{
				if (i + 1 - num > 1)
				{
					ChartPointWithIndex[] array2 = new ChartPointWithIndex[i + 1 - num];
					Array.Copy(array, num, array2, 0, i + 1 - num);
					arrayList.Add(array2);
				}
				for (int j = i + 2; j < count; j++)
				{
					if (array[i + 1].Point.X != array[j].Point.X)
					{
						i = j - 2;
						num = j - 1;
						break;
					}
				}
			}
			else if (i == count - 2)
			{
				ChartPointWithIndex[] array3 = new ChartPointWithIndex[count - num];
				Array.Copy(array, num, array3, 0, count - num);
				arrayList.Add(array3);
			}
		}
		GraphicsPath graphicsPath = new GraphicsPath();
		for (int k = 0; k < arrayList.Count; k++)
		{
			ChartPointWithIndex[] visiblePoints = GetVisiblePoints((ChartPointWithIndex[])arrayList[k]);
			NaturalSpline(visiblePoints, out var ys);
			ChartPointWithIndex p;
			ChartPointWithIndex p2;
			ChartPointWithIndex p3;
			ChartPointWithIndex p4;
			for (int l = 0; l < visiblePoints.Length - 1; l++)
			{
				BezierPointsFromSpline(visiblePoints[l], visiblePoints[l + 1], ys[l], ys[l + 1], out p, out p2, out p3, out p4);
				PointF point = new PointF(GetXFromValue(p.Point, 0) + thisOffset.Width, GetYFromValue(p.Point, 0) + thisOffset.Height);
				PointF point2 = new PointF(GetXFromValue(p2.Point, 0) + thisOffset.Width, GetYFromValue(p2.Point, 0) + thisOffset.Height);
				PointF point3 = new PointF(GetXFromValue(p3.Point, 0) + thisOffset.Width, GetYFromValue(p3.Point, 0) + thisOffset.Height);
				PointF point4 = new PointF(GetXFromValue(p4.Point, 0) + thisOffset.Width, GetYFromValue(p4.Point, 0) + thisOffset.Height);
				graphicsPath.AddBezier(point, point2, point3, point4);
			}
			ChartPoint cp = new ChartPoint(visiblePoints[^1].Point.X, origin);
			PointF pointF = new PointF(GetXFromValue(cp, 0) + thisOffset.Width, GetYFromValue(cp, 0) + thisOffset.Height);
			ChartPoint cp2 = new ChartPoint(visiblePoints[0].Point.X, origin);
			PointF pointF2 = new PointF(GetXFromValue(cp2, 0) + thisOffset.Width, GetYFromValue(cp2, 0) + thisOffset.Height);
			PointF pointF3 = new PointF(GetXFromValue(visiblePoints[^1].Point, 0) + thisOffset.Width, GetYFromValue(visiblePoints[^1].Point, 0) + thisOffset.Height);
			PointF pointF4 = new PointF(GetXFromValue(visiblePoints[0].Point, 0) + thisOffset.Width, GetYFromValue(visiblePoints[0].Point, 0) + thisOffset.Height);
			graphicsPath.AddLine(pointF3, pointF);
			graphicsPath.AddLine(pointF, pointF2);
			graphicsPath.CloseFigure();
			new Region();
			new Region();
			if (series3D)
			{
				GraphicsPath graphicsPath2 = new GraphicsPath();
				for (int m = 0; m < visiblePoints.Length - 1; m++)
				{
					BezierPointsFromSpline(visiblePoints[m], visiblePoints[m + 1], ys[m], ys[m + 1], out p, out p2, out p3, out p4);
					PointF point = new PointF(GetXFromValue(p.Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(p.Point, 0) + thisOffset.Height + seriesOffset.Height);
					PointF point2 = new PointF(GetXFromValue(p2.Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(p2.Point, 0) + thisOffset.Height + seriesOffset.Height);
					PointF point3 = new PointF(GetXFromValue(p3.Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(p3.Point, 0) + thisOffset.Height + seriesOffset.Height);
					PointF point4 = new PointF(GetXFromValue(p4.Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(p4.Point, 0) + thisOffset.Height + seriesOffset.Height);
					graphicsPath2.AddBezier(point, point2, point3, point4);
					new PointF(GetXFromValue(p.Point, 0) + thisOffset.Width, GetYFromValue(p.Point, 0) + thisOffset.Height);
					new PointF(GetXFromValue(p2.Point, 0) + thisOffset.Width, GetYFromValue(p2.Point, 0) + thisOffset.Height);
					new PointF(GetXFromValue(p3.Point, 0) + thisOffset.Width, GetYFromValue(p3.Point, 0) + thisOffset.Height);
					new PointF(GetXFromValue(p4.Point, 0) + thisOffset.Width, GetYFromValue(p4.Point, 0) + thisOffset.Height);
				}
				new ChartPoint(visiblePoints[^1].Point.X, 0.0);
				PointF pointF5 = new PointF(GetXFromValue(cp, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(cp, 0) + thisOffset.Height + seriesOffset.Height);
				new ChartPoint(visiblePoints[0].Point.X, 0.0);
				PointF pointF6 = new PointF(GetXFromValue(cp2, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(cp2, 0) + thisOffset.Height + seriesOffset.Height);
				PointF pointF7 = new PointF(GetXFromValue(visiblePoints[^1].Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(visiblePoints[^1].Point, 0) + thisOffset.Height + seriesOffset.Height);
				PointF pointF8 = new PointF(GetXFromValue(visiblePoints[0].Point, 0) + thisOffset.Width + seriesOffset.Width, GetYFromValue(visiblePoints[0].Point, 0) + thisOffset.Height + seriesOffset.Height);
				graphicsPath2.AddLine(pointF5, pointF6);
				graphicsPath2.CloseFigure();
				if (!chartRenderArgs2D.Chart.Style3D)
				{
					BrushPaint.FillPath(g, graphicsPath2, brush);
					g.DrawPath(gdipPen, graphicsPath2);
				}
				else
				{
					Draw(chartRenderArgs2D.Graph, graphicsPath2, brush, gdipPen);
				}
				GraphicsPath graphicsPath3 = new GraphicsPath();
				graphicsPath3.AddPolygon(new PointF[4] { pointF4, pointF2, pointF6, pointF8 });
				GraphicsPath graphicsPath4 = new GraphicsPath();
				graphicsPath4.AddPolygon(new PointF[4] { pointF3, pointF, pointF5, pointF7 });
				GraphicsPath graphicsPath5 = new GraphicsPath();
				graphicsPath5.AddPolygon(new PointF[4] { pointF2, pointF6, pointF5, pointF });
				if (!chartRenderArgs2D.Chart.Style3D)
				{
					BrushPaint.FillPath(g, graphicsPath4, brush);
					g.DrawPath(gdipPen, graphicsPath4);
				}
				else
				{
					chartRenderArgs2D.Graph.isRight = true;
					Draw(chartRenderArgs2D.Graph, graphicsPath4, brush, gdipPen);
				}
				if (base.YAxis.Inversed)
				{
					Draw3DSpline(g, visiblePoints, ys, seriesOffset, brush, gdipPen);
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath3, brush);
						g.DrawPath(gdipPen, graphicsPath3);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath3, brush, gdipPen);
					}
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath4, brush);
						g.DrawPath(gdipPen, graphicsPath4);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath4, brush, gdipPen);
					}
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath5, brush);
						g.DrawPath(gdipPen, graphicsPath5);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath5, brush, gdipPen);
					}
				}
				else
				{
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath5, brush);
						g.DrawPath(gdipPen, graphicsPath5);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath5, brush, gdipPen);
					}
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath3, brush);
						g.DrawPath(gdipPen, graphicsPath3);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath3, brush, gdipPen);
					}
					Draw3DSpline(g, visiblePoints, ys, seriesOffset, brush, gdipPen);
					if (!chartRenderArgs2D.Chart.Style3D)
					{
						BrushPaint.FillPath(g, graphicsPath4, brush);
						g.DrawPath(gdipPen, graphicsPath4);
					}
					else
					{
						Draw(chartRenderArgs2D.Graph, graphicsPath4, brush, gdipPen);
					}
				}
			}
			BrushPaint.FillPath(g, graphicsPath, brush);
			g.DrawPath(gdipPen, graphicsPath);
		}
	}

	public void Render(Graphics3D g, int from, int count)
	{
		base.Chart.Series.IndexOf(m_series);
		float customOriginY = CustomOriginY;
		int sPLINE_DIGITIZATION = SPLINE_DIGITIZATION;
		PointF[] array = new PointF[count];
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num = placeDepth + seriesDepth;
		double origin = m_series.ActualYAxis.Origin;
		int n = from;
		int n2 = count;
		for (int i = 0; i < count; i++)
		{
			if (!IsVisiblePoint(m_series.Points[from + i]))
			{
				if (i == 0 || i == from)
				{
					array[i] = VisiblePoint(m_series.Points, i, out n, first: true);
				}
				else if (from + i == count - 1)
				{
					array[i] = VisiblePoint(m_series.Points, i, out n2, first: false);
				}
				else
				{
					array[i] = array[i - 1];
				}
			}
			else
			{
				array[i] = GetPointFromIndex(from + i);
			}
		}
		PointF[] array2 = RemoveDuplicates(array);
		int num2 = array2.Length;
		g.AddPolygon(CreateBoundsPolygon(placeDepth));
		ChartPoint cp = new ChartPoint(m_series.Points[from + n2 - 1].X, origin);
		PointF pointF = new PointF(GetXFromValue(cp, 0), GetYFromValue(cp, 0));
		ChartPoint cp2 = new ChartPoint(m_series.Points[n].X, origin);
		PointF pointF2 = new PointF(GetXFromValue(cp2, 0), GetYFromValue(cp2, 0));
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddCurve(array2);
		graphicsPath.AddLine(array[n2 - 1], new PointF(array[n2 - 1].X, customOriginY));
		graphicsPath.AddLine(graphicsPath.GetLastPoint(), new PointF(array[0].X, customOriginY));
		graphicsPath.CloseFigure();
		PointF[] points = graphicsPath.PathData.Points;
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(array[0].X, array[0].Y, placeDepth),
			new Vector3D(array[0].X, array[0].Y, num),
			new Vector3D(pointF2.X, pointF2.Y, num),
			new Vector3D(pointF2.X, pointF2.Y, placeDepth)
		}, GetBrush(), base.SeriesStyle.GdipPen);
		Polygon polygon2 = new Polygon(new Vector3D[4]
		{
			new Vector3D(array[n2 - 1].X, array[n2 - 1].Y, placeDepth),
			new Vector3D(array[n2 - 1].X, array[n2 - 1].Y, num),
			new Vector3D(pointF.X, pointF.Y, num),
			new Vector3D(pointF.X, pointF.Y, placeDepth)
		}, GetBrush(), base.SeriesStyle.GdipPen);
		Polygon polygon3 = new Polygon(new Vector3D[4]
		{
			new Vector3D(pointF2.X, pointF2.Y, placeDepth),
			new Vector3D(pointF2.X, pointF2.Y, num),
			new Vector3D(pointF.X, pointF.Y, num),
			new Vector3D(pointF.X, pointF.Y, placeDepth)
		}, GetBrush(), base.SeriesStyle.GdipPen);
		Vector3D[] array3 = new Vector3D[2 + sPLINE_DIGITIZATION * (num2 - 1)];
		Vector3D[] array4 = new Vector3D[2 + sPLINE_DIGITIZATION * (num2 - 1)];
		for (int j = 0; j < num2 - 1; j++)
		{
			PointF p = points[j * 3];
			PointF p2 = points[j * 3 + 1];
			PointF p3 = points[j * 3 + 2];
			PointF p4 = points[j * 3 + 3];
			PointF[] array5 = ChartMath.InterpolateBezier(p, p2, p3, p4, sPLINE_DIGITIZATION);
			for (int k = 0; k < array5.Length; k++)
			{
				array3[j * sPLINE_DIGITIZATION + k] = new Vector3D(array5[k].X, array5[k].Y, placeDepth);
				array4[j * sPLINE_DIGITIZATION + k] = new Vector3D(array5[k].X, array5[k].Y, num);
			}
		}
		Vector3D[] array6 = new Vector3D[2 + sPLINE_DIGITIZATION * (num2 - 1)];
		Vector3D[] array7 = new Vector3D[2 + sPLINE_DIGITIZATION * (num2 - 1)];
		_ = PointF.Empty;
		_ = PointF.Empty;
		for (int l = 0; l < num2 - 1; l++)
		{
			PointF p5 = points[l * 3];
			PointF p6 = points[l * 3 + 1];
			PointF p7 = points[l * 3 + 2];
			PointF p8 = points[l * 3 + 3];
			PointF[] array8 = ChartMath.InterpolateBezier(p5, p6, p7, p8, sPLINE_DIGITIZATION);
			_ = array8.Length / 2;
			_ = new Vector3D[array8.Length + 3];
			_ = new Vector3D[array8.Length + 3];
			_ = new Vector3D[array8.Length + 3];
			int num3 = array8.Length / 2;
			int num4 = array8.Length - array8.Length / 2;
			_ = new Vector3D[num3 + 2];
			_ = new Vector3D[num4 + 2];
			_ = new Vector3D[num3 + 2];
			_ = new Vector3D[num4 + 2];
			for (int m = 0; m < array8.Length; m++)
			{
				array6[l * sPLINE_DIGITIZATION + m] = new Vector3D(array8[m].X, array8[m].Y, placeDepth);
				array7[l * sPLINE_DIGITIZATION + m] = new Vector3D(array8[m].X, array8[m].Y, num);
			}
		}
		array3[^2] = new Vector3D(pointF.X, pointF.Y, array3[^3].Z);
		array3[^1] = new Vector3D(pointF2.X, pointF2.Y, array3[0].Z);
		array4[^2] = new Vector3D(pointF.X, pointF.Y, array4[^3].Z);
		array4[^1] = new Vector3D(pointF2.X, pointF2.Y, array4[0].Z);
		Polygon polygon4 = new Polygon(array3, GetBrush(), base.SeriesStyle.GdipPen);
		Polygon polygon5 = new Polygon(array4, GetBrush(), base.SeriesStyle.GdipPen);
		g.AddPolygon(polygon4);
		g.AddPolygon(polygon5);
		g.AddPolygon(polygon);
		g.AddPolygon(polygon2);
		g.AddPolygon(polygon3);
		for (int num5 = 0; num5 < num2 - 1; num5++)
		{
			PointF p9 = points[num5 * 3];
			PointF p10 = points[num5 * 3 + 1];
			PointF p11 = points[num5 * 3 + 2];
			PointF p12 = points[num5 * 3 + 3];
			PointF[] array9 = ChartMath.InterpolateBezier(p9, p10, p11, p12, sPLINE_DIGITIZATION);
			_ = array9.Length / 2;
			Pen pen = new Pen(GetBrush(num5).BackColor);
			for (int num6 = 1; num6 < array9.Length; num6++)
			{
				Vector3D vector3D = new Vector3D(array9[num6 - 1].X, array9[num6 - 1].Y, placeDepth);
				Vector3D vector3D2 = new Vector3D(array9[num6].X, array9[num6].Y, placeDepth);
				Vector3D vector3D3 = new Vector3D(array9[num6].X, array9[num6].Y, num);
				Vector3D vector3D4 = new Vector3D(array9[num6 - 1].X, array9[num6 - 1].Y, num);
				Polygon polygon6 = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, GetBrush(), pen);
				g.AddPolygon(polygon6);
			}
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		int x = bounds.X + bounds.Width / 3;
		int x2 = bounds.X + 2 * bounds.Width / 3;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddCurve(new Point[5]
		{
			new Point(bounds.X, bounds.Bottom),
			new Point(x, bounds.Top),
			new Point(x2, bounds.Top + bounds.Height / 2),
			new Point(bounds.Right, bounds.Top),
			new Point(bounds.Right, bounds.Bottom)
		});
		graphicsPath.CloseFigure();
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillPath(brush, graphicsPath);
				return;
			}
		}
		BrushPaint.FillPath(g, graphicsPath, base.SeriesStyle.Interior);
		g.DrawPath(base.SeriesStyle.GdipPen, graphicsPath);
	}
}
