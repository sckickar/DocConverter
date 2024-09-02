using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class StepAreaRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "StepArea renderer region";

	public StepAreaRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		bool inverted = m_series.ConfigItems.StepItem.Inverted;
		bool flag = (args.IsInvertedAxes ? base.YAxis.Inversed : base.XAxis.Inversed);
		bool flag2 = (args.IsInvertedAxes ? base.XAxis.Inversed : base.YAxis.Inversed);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		double origin = m_series.ActualYAxis.Origin;
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		BrushInfo brush = GetBrush();
		Pen gdipPen = seriesStyle.GdipPen;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		for (int i = 0; i < array2.Length; i++)
		{
			IndexRange indexRange = array2[i];
			ArrayList arrayList = new ArrayList();
			ChartStyledPoint chartStyledPoint = null;
			ChartStyledPoint chartStyledPoint2 = null;
			_ = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			ArrayList arrayList2 = new ArrayList();
			Region region = null;
			ChartStyledPoint chartStyledPoint3 = null;
			PointF pointF = PointF.Empty;
			int j = indexRange.From;
			for (int num2 = indexRange.To + 1; j < num2; j++)
			{
				ChartStyledPoint chartStyledPoint4 = array[j];
				if (!chartStyledPoint4.IsVisible)
				{
					continue;
				}
				PointF point = args.GetPoint(chartStyledPoint4.X, chartStyledPoint4.YValues[num]);
				PointF point2 = args.GetPoint(chartStyledPoint4.X, origin);
				if (dropSeriesPoints && !pointF.IsEmpty && ((!args.IsInvertedAxes && Math.Abs(point.X - pointF.X) < 1f) || (args.IsInvertedAxes && Math.Abs(point.Y - pointF.Y) < 1f)))
				{
					continue;
				}
				if (chartStyledPoint == null)
				{
					chartStyledPoint = chartStyledPoint4;
					_ = chartStyledPoint4.Index;
				}
				if (chartStyledPoint3 != null)
				{
					if (inverted)
					{
						arrayList.Add(args.GetPoint(chartStyledPoint3.X, chartStyledPoint4.YValues[num]));
					}
					else
					{
						arrayList.Add(args.GetPoint(chartStyledPoint4.X, chartStyledPoint3.YValues[num]));
					}
				}
				chartStyledPoint2 = chartStyledPoint4;
				arrayList.Add(point);
				chartStyledPoint3 = chartStyledPoint4;
				pointF = point;
				_ = chartStyledPoint4.Index;
				arrayList2.Add(chartStyledPoint4.Index);
			}
			if (chartStyledPoint == null)
			{
				continue;
			}
			PointF point3 = args.GetPoint(chartStyledPoint.X, origin);
			PointF point4 = args.GetPoint(chartStyledPoint2.X, origin);
			PointF[] array3 = (PointF[])arrayList.ToArray(typeof(PointF));
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines(array3);
			graphicsPath.AddLine(point4, point3);
			graphicsPath.CloseFigure();
			if (args.Is3D)
			{
				PointF pointF2 = array3[0];
				PointF pointF3 = array3[^1];
				PointF pointF4 = point3;
				PointF pointF5 = point4;
				GraphicsPath graphicsPath2 = new GraphicsPath();
				GraphicsPath graphicsPath3 = new GraphicsPath();
				GraphicsPath graphicsPath4 = new GraphicsPath();
				graphicsPath2.AddPolygon(new PointF[4]
				{
					pointF4,
					pointF2,
					ChartMath.AddPoint(pointF2, args.DepthOffset),
					ChartMath.AddPoint(pointF4, args.DepthOffset)
				});
				graphicsPath3.AddPolygon(new PointF[4]
				{
					pointF5,
					pointF3,
					ChartMath.AddPoint(pointF3, args.DepthOffset),
					ChartMath.AddPoint(pointF5, args.DepthOffset)
				});
				graphicsPath4.AddPolygon(new PointF[4]
				{
					pointF4,
					pointF5,
					ChartMath.AddPoint(pointF5, args.DepthOffset),
					ChartMath.AddPoint(pointF4, args.DepthOffset)
				});
				PointF pointF6 = PointF.Empty;
				PointF pointF7 = PointF.Empty;
				args.Graph.DrawPath(brush, gdipPen, flag ? graphicsPath3 : graphicsPath2);
				if (args.Chart.Style3D)
				{
					Draw(args.Graph, flag ? graphicsPath3 : graphicsPath2, brush, gdipPen);
				}
				if (!flag2)
				{
					args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
					if (args.Chart.Style3D)
					{
						Draw(args.Graph, graphicsPath4, brush, gdipPen);
					}
				}
				int k = (flag ? (array3.Length - 1) : 0);
				int num3 = (flag ? (-1) : array3.Length);
				for (int num4 = ((!flag) ? 1 : (-1)); k != num3; k += num4)
				{
					PointF pointF8 = array3[k];
					PointF pointF9 = ChartMath.AddPoint(pointF8, args.DepthOffset);
					if (!pointF6.IsEmpty)
					{
						GraphicsPath graphicsPath5 = new GraphicsPath();
						graphicsPath5.AddPolygon(new PointF[4] { pointF8, pointF9, pointF7, pointF6 });
						args.Graph.DrawPath(brush, gdipPen, graphicsPath5);
						if (args.Chart.Style3D)
						{
							Draw(args.Graph, graphicsPath5, brush, gdipPen);
						}
					}
					pointF6 = pointF8;
					pointF7 = pointF9;
				}
				if (flag2)
				{
					args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
					if (args.Chart.Style3D)
					{
						Draw(args.Graph, graphicsPath4, brush, gdipPen);
					}
				}
				args.Graph.DrawPath(brush, gdipPen, flag ? graphicsPath2 : graphicsPath3);
				if (args.Chart.Style3D)
				{
					args.Graph.isRight = true;
					Draw(args.Graph, flag ? graphicsPath2 : graphicsPath3, brush, gdipPen);
				}
				if (region != null)
				{
					Region region2 = new Region(graphicsPath2);
					Region region3 = new Region(graphicsPath3);
					Region region4 = new Region(graphicsPath4);
					region2.Intersect(clipBounds);
					region3.Intersect(clipBounds);
					region4.Intersect(clipBounds);
				}
			}
			else if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath6 = (GraphicsPath)graphicsPath.Clone();
				graphicsPath6.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
				args.Graph.DrawPath(seriesStyle.ShadowInterior, null, graphicsPath6);
			}
			args.Graph.DrawPath(brush, gdipPen, graphicsPath);
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

	public void Render(Graphics3D g, int from, int count)
	{
		if (count <= 1)
		{
			return;
		}
		int j = m_series.PointFormats[ChartYValueUsage.YValue];
		_ = base.Chart.Series.VisibleCount;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		_ = base.Chart.Series3D;
		base.Chart.Series.IndexOf(m_series);
		_ = m_series.ActualYAxis.Inversed;
		_ = m_series.ActualXAxis.Inversed;
		double origin = m_series.ActualYAxis.Origin;
		new ArrayList();
		new ArrayList();
		new ArrayList();
		new ArrayList();
		ArrayList arrayList = new ArrayList();
		ChartPointWithIndex[] array = new ChartPointWithIndex[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new ChartPointWithIndex(m_series.Points[from + i], from + i);
		}
		Array.Sort(array, new ComparerPointWithIndexByX());
		ArrayList arrayList2 = new ArrayList(m_series.Points.Count);
		ArrayList arrayList3 = new ArrayList(m_series.Points.Count);
		int k = 0;
		for (int num = array.Length; k < num; k++)
		{
			ChartPoint point = array[k].Point;
			int index = array[k].Index;
			if (IsVisiblePoint(point))
			{
				arrayList2.Add(array[k]);
				arrayList3.Add(GetPointFromIndex(index));
			}
		}
		PointF[] array2 = (PointF[])arrayList3.ToArray(typeof(PointF));
		array = (ChartPointWithIndex[])arrayList2.ToArray(typeof(ChartPointWithIndex));
		PointF[] array3 = new PointF[array2.Length * 2 - 1];
		Vector3D[] array4 = new Vector3D[array3.Length + 2];
		Vector3D[] array5 = new Vector3D[array3.Length + 2];
		bool inverted = m_series.ConfigItems.StepItem.Inverted;
		for (int l = 0; l < array2.Length; l++)
		{
			array3[2 * l] = array2[l];
			if (l < array2.Length - 1)
			{
				if (inverted)
				{
					ChartPoint cp = new ChartPoint(((ChartPointWithIndex)arrayList2[l]).Point.X, ((ChartPointWithIndex)arrayList2[l + 1]).Point.YValues);
					array3[2 * l + 1] = new PointF(GetXFromValue(cp, j), GetYFromValue(cp, j));
				}
				else
				{
					ChartPoint cp2 = new ChartPoint(((ChartPointWithIndex)arrayList2[l + 1]).Point.X, ((ChartPointWithIndex)arrayList2[l]).Point.YValues);
					array3[2 * l + 1] = new PointF(GetXFromValue(cp2, j), GetYFromValue(cp2, j));
				}
			}
		}
		new GraphicsPath();
		ChartPoint cp3 = new ChartPoint(((ChartPointWithIndex)arrayList2[array2.Length - 1]).Point.X, origin);
		PointF pointF = new PointF(GetXFromValue(cp3, 0), GetYFromValue(cp3, 0));
		ChartPoint cp4 = new ChartPoint(((ChartPointWithIndex)arrayList2[0]).Point.X, origin);
		PointF pointF2 = new PointF(GetXFromValue(cp4, 0), GetYFromValue(cp4, 0));
		for (int m = 0; m < array3.Length; m++)
		{
			array4[m] = new Vector3D(array3[m].X, array3[m].Y, placeDepth);
			array5[m] = new Vector3D(array3[m].X, array3[m].Y, placeDepth + seriesDepth);
		}
		array4[^2] = new Vector3D(pointF.X, pointF.Y, array4[^3].Z);
		array4[^1] = new Vector3D(pointF2.X, pointF2.Y, array4[0].Z);
		array5[array4.Length - 2] = new Vector3D(pointF.X, pointF.Y, array5[^3].Z);
		array5[array4.Length - 1] = new Vector3D(pointF2.X, pointF2.Y, array5[0].Z);
		Polygon polygon = new Polygon(array4, GetBrush(), null);
		Polygon polygon2 = new Polygon(array5, GetBrush(), null);
		Polygon polygon3 = new Polygon(new Vector3D[4]
		{
			array4[0],
			array4[^1],
			array5[^1],
			array5[0]
		}, GetBrush(), base.SeriesStyle.GdipPen);
		Polygon polygon4 = new Polygon(new Vector3D[4]
		{
			array4[^2],
			array4[^3],
			array5[^3],
			array5[^2]
		}, GetBrush(), base.SeriesStyle.GdipPen);
		Polygon polygon5 = new Polygon(new Vector3D[4]
		{
			array4[^1],
			array4[^2],
			array5[^2],
			array5[^1]
		}, GetBrush(), base.SeriesStyle.GdipPen);
		g.AddPolygon(polygon);
		g.AddPolygon(polygon2);
		g.AddPolygon(polygon3);
		g.AddPolygon(polygon4);
		g.AddPolygon(polygon5);
		for (int n = 0; n < arrayList2.Count - 1; n++)
		{
			_ = ((ChartPointWithIndex)arrayList2[n + 1]).Index;
			Vector3D vector3D = new Vector3D(array3[2 * n + 1].X, array3[2 * n + 1].Y, placeDepth);
			Vector3D vector3D2 = new Vector3D(array3[2 * (n + 1)].X, array3[2 * (n + 1)].Y, placeDepth);
			Vector3D vector3D3 = new Vector3D(array3[2 * (n + 1)].X, array3[2 * (n + 1)].Y, placeDepth + seriesDepth);
			Vector3D vector3D4 = new Vector3D(array3[2 * n + 1].X, array3[2 * n + 1].Y, placeDepth + seriesDepth);
			Polygon polygon6 = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, GetBrush(), base.SeriesStyle.GdipPen);
			g.AddPolygon(polygon6);
		}
		for (int num2 = 0; num2 < arrayList2.Count - 1; num2++)
		{
			_ = ((ChartPointWithIndex)arrayList2[num2]).Index;
			Vector3D vector3D5 = new Vector3D(array3[2 * num2].X, array3[2 * num2].Y, placeDepth);
			Vector3D vector3D6 = new Vector3D(array3[2 * num2 + 1].X, array3[2 * num2 + 1].Y, placeDepth);
			Vector3D vector3D7 = new Vector3D(array3[2 * num2 + 1].X, array3[2 * num2 + 1].Y, placeDepth + seriesDepth);
			Vector3D vector3D8 = new Vector3D(array3[2 * num2].X, array3[2 * num2].Y, placeDepth + seriesDepth);
			Polygon polygon7 = new Polygon(new Vector3D[4] { vector3D5, vector3D6, vector3D7, vector3D8 }, GetBrush(), base.SeriesStyle.GdipPen);
			g.AddPolygon(polygon7);
		}
		arrayList.Add(((ChartPointWithIndex)arrayList2[arrayList2.Count - 1]).Index);
	}
}
