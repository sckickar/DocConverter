using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class AreaRenderer : ChartSeriesRenderer
{
	private const string c_areaRegionDescription = "Area Chart Renderer";

	protected override int RequireYValuesCount => 1;

	public AreaRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		bool flag = (args.IsInvertedAxes ? base.YAxis.Inversed : base.XAxis.Inversed);
		bool flag2 = (args.IsInvertedAxes ? base.XAxis.Inversed : base.YAxis.Inversed);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		int num = args.Series.PointFormats[ChartYValueUsage.YValue];
		double origin = m_series.ActualYAxis.Origin;
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		BrushInfo brush = GetBrush();
		Pen gdipPen = seriesStyle.GdipPen;
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		int i = 0;
		for (int num2 = array2.Length; i < num2; i++)
		{
			IndexRange indexRange = array2[i];
			ArrayList arrayList = new ArrayList();
			ChartStyledPoint chartStyledPoint = null;
			ChartStyledPoint chartStyledPoint2 = null;
			Region region = null;
			PointF pointF = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			_ = PointF.Empty;
			int j = indexRange.From;
			for (int num3 = indexRange.To + 1; j < num3; j++)
			{
				ChartStyledPoint chartStyledPoint3 = array[j];
				if (!chartStyledPoint3.IsVisible)
				{
					continue;
				}
				PointF point = args.GetPoint(chartStyledPoint3.X, chartStyledPoint3.YValues[num]);
				PointF point2 = args.GetPoint(chartStyledPoint3.X, origin);
				if (!dropSeriesPoints || pointF.IsEmpty || ((args.IsInvertedAxes || !(Math.Abs(point.X - pointF.X) < 1f)) && (!args.IsInvertedAxes || !(Math.Abs(point.Y - pointF.Y) < 1f))))
				{
					if (chartStyledPoint == null)
					{
						chartStyledPoint = chartStyledPoint3;
					}
					chartStyledPoint2 = chartStyledPoint3;
					arrayList.Add(point);
					pointF = point;
				}
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
				int num4 = (flag ? (-1) : array3.Length);
				for (int num5 = ((!flag) ? 1 : (-1)); k != num4; k += num5)
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
					region.Union(graphicsPath2);
					region.Union(graphicsPath3);
					region.Union(graphicsPath4);
				}
			}
			else if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath6 = (GraphicsPath)graphicsPath.Clone();
				graphicsPath6.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
				args.Graph.DrawPath(seriesStyle.ShadowInterior, null, graphicsPath6);
			}
			args.Graph.DrawPath(brush, gdipPen, graphicsPath);
			if (region != null)
			{
				region.Union(graphicsPath);
				region.Intersect(clipBounds);
			}
		}
	}

	public override void Render(ChartRenderArgs3D args)
	{
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		int num = args.Series.PointFormats[ChartYValueUsage.YValue];
		double origin = m_series.ActualYAxis.Origin;
		BrushInfo brush = GetBrush();
		Pen gdipPen = base.SeriesStyle.GdipPen;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		int i = 0;
		for (int num2 = array2.Length; i < num2; i++)
		{
			IndexRange indexRange = array2[i];
			if (indexRange.To <= indexRange.From)
			{
				continue;
			}
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			new ArrayList();
			new ArrayList();
			new ArrayList();
			new ArrayList();
			ArrayList arrayList3 = new ArrayList();
			ChartStyledPoint chartStyledPoint = null;
			ChartStyledPoint chartStyledPoint2 = null;
			_ = PointF.Empty;
			_ = PointF.Empty;
			args.Graph.AddPolygon(CreateBoundsPolygon((float)args.Z));
			PointF pointF = PointF.Empty;
			ChartStyledPoint chartStyledPoint3 = null;
			int j = indexRange.From;
			for (int num3 = indexRange.To + 1; j < num3; j++)
			{
				ChartStyledPoint chartStyledPoint4 = array[j];
				if (!chartStyledPoint4.IsVisible)
				{
					continue;
				}
				PointF point = args.GetPoint(chartStyledPoint4.X, chartStyledPoint4.YValues[num]);
				GetPointFromValue(chartStyledPoint4.X, origin);
				if (!dropSeriesPoints || pointF.IsEmpty || ((args.IsInvertedAxes || !(Math.Abs(point.X - pointF.X) < 1f)) && (!args.IsInvertedAxes || !(Math.Abs(point.Y - pointF.Y) < 1f))))
				{
					if (chartStyledPoint == null)
					{
						chartStyledPoint = chartStyledPoint4;
					}
					chartStyledPoint2 = chartStyledPoint4;
					if (chartStyledPoint3 != null)
					{
						Vector3D vector3D = new Vector3D(pointF.X, pointF.Y, args.Z);
						Vector3D vector3D2 = new Vector3D(pointF.X, pointF.Y, args.Z + args.Depth);
						Vector3D vector3D3 = new Vector3D(point.X, point.Y, args.Z);
						Vector3D vector3D4 = new Vector3D(point.X, point.Y, args.Z + args.Depth);
						Polygon p = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D4, vector3D3 }, brush, gdipPen);
						arrayList2.Add(new PolygonWithTangent(p, (vector3D.Y - vector3D3.Y) / (vector3D.X - vector3D3.X)));
					}
					arrayList.Add(point);
					pointF = point;
					chartStyledPoint3 = chartStyledPoint4;
					GetPointFromValue(chartStyledPoint4.X, origin);
					arrayList3.Add(chartStyledPoint4.Index);
				}
			}
			PointF point2 = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[num]);
			PointF pointFromValue = GetPointFromValue(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
			PointF pointFromValue2 = GetPointFromValue(chartStyledPoint.X, origin);
			PointF pointFromValue3 = GetPointFromValue(chartStyledPoint2.X, origin);
			Vector3D vector3D5 = new Vector3D(point2.X, point2.Y, args.Z);
			Vector3D vector3D6 = new Vector3D(point2.X, point2.Y, args.Z + args.Depth);
			Vector3D vector3D7 = new Vector3D(pointFromValue.X, pointFromValue.Y, args.Z);
			Vector3D vector3D8 = new Vector3D(pointFromValue.X, pointFromValue.Y, args.Z + args.Depth);
			Vector3D vector3D9 = new Vector3D(pointFromValue2.X, pointFromValue2.Y, args.Z);
			Vector3D vector3D10 = new Vector3D(pointFromValue2.X, pointFromValue2.Y, args.Z + args.Depth);
			Vector3D vector3D11 = new Vector3D(pointFromValue3.X, pointFromValue3.Y, args.Z);
			Vector3D vector3D12 = new Vector3D(pointFromValue3.X, pointFromValue3.Y, args.Z + args.Depth);
			arrayList.Add(pointFromValue3);
			arrayList.Add(pointFromValue2);
			Vector3D[] array3 = new Vector3D[arrayList.Count];
			Vector3D[] array4 = new Vector3D[arrayList.Count];
			Vector3D[] points = new Vector3D[4] { vector3D5, vector3D6, vector3D10, vector3D9 };
			Vector3D[] points2 = new Vector3D[4] { vector3D7, vector3D8, vector3D12, vector3D11 };
			Vector3D[] points3 = new Vector3D[4] { vector3D9, vector3D10, vector3D12, vector3D11 };
			for (int k = 0; k < arrayList.Count; k++)
			{
				PointF pointF2 = (PointF)arrayList[k];
				array3[k] = new Vector3D(pointF2.X, pointF2.Y, args.Z);
				array4[k] = new Vector3D(pointF2.X, pointF2.Y, args.Z + args.Depth);
			}
			Polygon polygon = new Polygon(array3, brush);
			Polygon polygon2 = new Polygon(array4, brush);
			Polygon polygon3 = new Polygon(points, brush, gdipPen);
			Polygon polygon4 = new Polygon(points2, brush, gdipPen);
			Polygon polygon5 = new Polygon(points3, brush, gdipPen);
			args.Graph.AddPolygon(polygon);
			args.Graph.AddPolygon(polygon2);
			args.Graph.AddPolygon(polygon3);
			args.Graph.AddPolygon(polygon4);
			args.Graph.AddPolygon(polygon5);
			arrayList2.Sort(new PolygonWithTangentComparer());
			for (int l = 0; l < arrayList2.Count; l++)
			{
				args.Graph.AddPolygon(((PolygonWithTangent)arrayList2[l]).Polygon);
			}
		}
	}

	protected override void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		RenderErrorBar(g, point);
		base.RenderAdornment(g, point);
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(bounds);
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
