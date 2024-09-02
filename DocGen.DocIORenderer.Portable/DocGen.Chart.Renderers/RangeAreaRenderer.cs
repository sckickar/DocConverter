using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class RangeAreaRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 2;

	protected override string RegionDescription => "RangeArea Chart Renderer";

	public RangeAreaRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		ChartRangeAreaConfigItem rangeAreaItem = args.Series.ConfigItems.RangeAreaItem;
		bool flag = (args.IsInvertedAxes ? base.YAxis.Inversed : base.XAxis.Inversed);
		bool flag2 = (args.IsInvertedAxes ? base.XAxis.Inversed : base.YAxis.Inversed);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		ArrayList arrayList = new ArrayList();
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		int num = args.Series.PointFormats[ChartYValueUsage.LowValue];
		int num2 = args.Series.PointFormats[ChartYValueUsage.HighValue];
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		BrushInfo brush = GetBrush();
		Pen gdipPen = seriesStyle.GdipPen;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		int i = 0;
		for (int num3 = array2.Length; i < num3; i++)
		{
			IndexRange indexRange = array2[i];
			ArrayList arrayList2 = new ArrayList();
			ArrayList arrayList3 = new ArrayList();
			Region region = null;
			PointF pointF = PointF.Empty;
			_ = PointF.Empty;
			ChartStyledPoint chartStyledPoint = null;
			int j = indexRange.From;
			for (int num4 = indexRange.To + 1; j < num4; j++)
			{
				ChartStyledPoint chartStyledPoint2 = array[j];
				if (!chartStyledPoint2.IsVisible)
				{
					continue;
				}
				PointF pointF2 = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
				PointF pointF3 = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num2]);
				if (!dropSeriesPoints || chartStyledPoint == null || !(Math.Abs(args.IsInvertedAxes ? (pointF2.Y - pointF.Y) : (pointF2.X - pointF.X)) < 1f))
				{
					if (args.ActualYAxis.Inversed ^ (rangeAreaItem.SwapHighLowPoint && chartStyledPoint2.YValues[num] > chartStyledPoint2.YValues[num2]) ^ flag2)
					{
						PointF pointF4 = pointF2;
						pointF2 = pointF3;
						pointF3 = pointF4;
					}
					arrayList2.Add(pointF3);
					arrayList3.Add(pointF2);
					chartStyledPoint = chartStyledPoint2;
					pointF = pointF2;
					arrayList.Add(chartStyledPoint2.Index);
				}
			}
			if (arrayList2.Count <= 0)
			{
				continue;
			}
			PointF[] array3 = (PointF[])arrayList2.ToArray(typeof(PointF));
			PointF[] array4 = (PointF[])arrayList3.ToArray(typeof(PointF));
			Array.Reverse(array4);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines(array3);
			graphicsPath.AddLines(array4);
			graphicsPath.CloseFigure();
			if (args.Is3D)
			{
				PointF pointF5 = array3[0];
				PointF pointF6 = array4[^1];
				PointF pointF7 = array3[^1];
				PointF pointF8 = array4[0];
				GraphicsPath graphicsPath2 = new GraphicsPath();
				GraphicsPath graphicsPath3 = new GraphicsPath();
				graphicsPath2.AddPolygon(new PointF[4]
				{
					pointF6,
					pointF5,
					ChartMath.AddPoint(pointF5, args.DepthOffset),
					ChartMath.AddPoint(pointF6, args.DepthOffset)
				});
				graphicsPath3.AddPolygon(new PointF[4]
				{
					pointF8,
					pointF7,
					ChartMath.AddPoint(pointF7, args.DepthOffset),
					ChartMath.AddPoint(pointF8, args.DepthOffset)
				});
				args.Graph.DrawPath(brush, gdipPen, flag ? graphicsPath3 : graphicsPath2);
				Math.Abs(args.DepthOffset.Width);
				Math.Abs(args.DepthOffset.Height);
				PointF[] array5 = (flag2 ? array4 : array3);
				PointF[] array6 = (flag2 ? array3 : array4);
				PointF pointF9 = PointF.Empty;
				PointF pointF10 = PointF.Empty;
				PointF pointF11 = PointF.Empty;
				PointF pointF12 = PointF.Empty;
				int num5 = (flag ? (array5.Length - 1) : 0);
				int num6 = (flag ? (-1) : array5.Length);
				int num7 = ((!flag) ? 1 : (-1));
				for (int k = num5; k != num6; k += num7)
				{
					PointF pointF13 = array5[k];
					PointF pointF14 = ChartMath.AddPoint(pointF13, args.DepthOffset);
					PointF pointF15 = array6[array6.Length - k - 1];
					PointF pointF16 = ChartMath.AddPoint(pointF15, args.DepthOffset);
					if (k != num5)
					{
						bool flag3 = false;
						GraphicsPath graphicsPath4 = new GraphicsPath();
						GraphicsPath graphicsPath5 = new GraphicsPath();
						graphicsPath4.AddPolygon(new PointF[4] { pointF13, pointF14, pointF10, pointF9 });
						graphicsPath5.AddPolygon(new PointF[4] { pointF15, pointF16, pointF12, pointF11 });
						if (args.IsInvertedAxes)
						{
							flag3 = pointF15.X > pointF11.X;
							flag3 &= Math.Abs(args.DepthOffset.Width / args.DepthOffset.Height) < Math.Abs((pointF15.X - pointF11.X) / (pointF15.Y - pointF11.Y));
						}
						else
						{
							flag3 = pointF15.Y < pointF11.Y;
							flag3 &= Math.Abs(args.DepthOffset.Width / args.DepthOffset.Height) > Math.Abs((pointF15.X - pointF11.X) / (pointF15.Y - pointF11.Y));
						}
						if (flag3)
						{
							args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
							args.Graph.DrawPath(brush, gdipPen, graphicsPath5);
						}
						else
						{
							args.Graph.DrawPath(brush, gdipPen, graphicsPath5);
							args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
						}
					}
					pointF9 = pointF13;
					pointF10 = pointF14;
					pointF11 = pointF15;
					pointF12 = pointF16;
				}
				args.Graph.DrawPath(brush, gdipPen, flag ? graphicsPath2 : graphicsPath3);
				if (region != null)
				{
					region.Union(graphicsPath2);
					region.Union(graphicsPath3);
					region.Intersect(clipBounds);
				}
			}
			else if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath6 = (GraphicsPath)graphicsPath.Clone();
				graphicsPath6.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
				args.Graph.DrawPath(brush, null, graphicsPath6);
			}
			args.Graph.DrawPath(brush, gdipPen, graphicsPath);
			if (region != null)
			{
				region.Union(graphicsPath);
				region.Intersect(clipBounds);
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		bool isInvertedAxes = IsInvertedAxes;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		base.Chart.Series.IndexOf(m_series);
		int j = m_series.PointFormats[ChartYValueUsage.LowValue];
		int j2 = m_series.PointFormats[ChartYValueUsage.HighValue];
		float placeDepth = GetPlaceDepth();
		float num = placeDepth + GetSeriesDepth();
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
			new ArrayList();
			ArrayList arrayList3 = new ArrayList();
			g.AddPolygon(CreateBoundsPolygon(placeDepth));
			PointF pointF = PointF.Empty;
			_ = PointF.Empty;
			int k = indexRange.From;
			for (int num3 = indexRange.To + 1; k < num3; k++)
			{
				ChartStyledPoint chartStyledPoint = array[k];
				if (chartStyledPoint.IsVisible)
				{
					PointF pointFromValue = GetPointFromValue(chartStyledPoint.Point, j);
					PointF pointFromValue2 = GetPointFromValue(chartStyledPoint.Point, j2);
					if (!dropSeriesPoints || pointF.IsEmpty || ((isInvertedAxes || !(Math.Abs(pointFromValue.X - pointF.X) < 1f)) && (!isInvertedAxes || !(Math.Abs(pointFromValue.Y - pointF.Y) < 1f))))
					{
						arrayList.Add(pointFromValue);
						arrayList2.Add(pointFromValue2);
						pointF = pointFromValue;
						arrayList3.Add(chartStyledPoint.Index);
					}
				}
			}
			PointF[] array3 = arrayList.ToArray(typeof(PointF)) as PointF[];
			PointF[] array4 = arrayList2.ToArray(typeof(PointF)) as PointF[];
			if (array4 != null)
			{
				Array.Reverse(array4);
			}
			PointF pointF2 = array3[0];
			PointF pointF3 = array3[^1];
			PointF pointF4 = array4[^1];
			PointF pointF5 = array4[0];
			Vector3D vector3D = new Vector3D(pointF2.X, pointF2.Y, placeDepth);
			Vector3D vector3D2 = new Vector3D(pointF2.X, pointF2.Y, num);
			Vector3D vector3D3 = new Vector3D(pointF3.X, pointF3.Y, placeDepth);
			Vector3D vector3D4 = new Vector3D(pointF3.X, pointF3.Y, num);
			Vector3D vector3D5 = new Vector3D(pointF4.X, pointF4.Y, placeDepth);
			Vector3D vector3D6 = new Vector3D(pointF4.X, pointF4.Y, num);
			Vector3D vector3D7 = new Vector3D(pointF5.X, pointF5.Y, placeDepth);
			Vector3D vector3D8 = new Vector3D(pointF5.X, pointF5.Y, num);
			Vector3D[] array5 = new Vector3D[arrayList.Count + arrayList2.Count];
			Vector3D[] array6 = new Vector3D[arrayList.Count + arrayList2.Count];
			Vector3D[] points = new Vector3D[4] { vector3D, vector3D2, vector3D6, vector3D5 };
			Vector3D[] points2 = new Vector3D[4] { vector3D3, vector3D4, vector3D8, vector3D7 };
			int l;
			for (l = 0; l < array3.Length; l++)
			{
				array5[l] = new Vector3D(array3[l].X, array3[l].Y, placeDepth);
				array6[l] = new Vector3D(array3[l].X, array3[l].Y, num);
			}
			for (int m = 0; m < array4.Length; m++)
			{
				array5[l + m] = new Vector3D(array4[m].X, array4[m].Y, placeDepth);
				array6[l + m] = new Vector3D(array4[m].X, array4[m].Y, num);
			}
			Polygon polygon = new Polygon(array5, brush);
			Polygon polygon2 = new Polygon(array6, brush);
			Polygon polygon3 = new Polygon(points, brush, gdipPen);
			Polygon polygon4 = new Polygon(points2, brush, gdipPen);
			g.AddPolygon(polygon);
			g.AddPolygon(polygon2);
			g.AddPolygon(polygon3);
			g.AddPolygon(polygon4);
			ArrayList arrayList4 = new ArrayList();
			int n = 1;
			int num4 = array5.Length;
			for (int num5 = num4 / 2; n < num5; n++)
			{
				Vector3D vector3D9 = array5[n - 1];
				Vector3D vector3D10 = array6[n - 1];
				Vector3D vector3D11 = array5[n];
				Vector3D vector3D12 = array6[n];
				Vector3D vector3D13 = array5[num4 - n - 1];
				Vector3D vector3D14 = array6[num4 - n - 1];
				Vector3D vector3D15 = array5[num4 - n];
				Vector3D vector3D16 = array6[num4 - n];
				Polygon value = new Polygon(new Vector3D[4] { vector3D9, vector3D10, vector3D12, vector3D11 }, brush, gdipPen);
				Polygon value2 = new Polygon(new Vector3D[4] { vector3D13, vector3D14, vector3D16, vector3D15 }, brush, gdipPen);
				arrayList4.Add(value);
				arrayList4.Add(value2);
				g.AddPolygon(new Polygon(new Vector3D[4] { vector3D11, vector3D12, vector3D14, vector3D13 }, brush));
			}
			foreach (Polygon item in arrayList4)
			{
				g.AddPolygon(item);
			}
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		int x = bounds.X + bounds.Width / 3;
		int x2 = bounds.X + 2 * bounds.Width / 3;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new Point[5]
		{
			new Point(bounds.X, bounds.Bottom),
			new Point(x, bounds.Top),
			new Point(x2, bounds.Top + bounds.Height / 2),
			new Point(bounds.Right, bounds.Top),
			new Point(bounds.Right, bounds.Bottom)
		});
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
