using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class StackingLineRenderer : ChartSeriesRenderer
{
	public override ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.OneForAll;

	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Stacking Area Chart Region";

	internal StackingLineRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		bool flag = (args.IsInvertedAxes ? base.YAxis.Inversed : base.XAxis.Inversed);
		bool flag2 = (args.IsInvertedAxes ? base.XAxis.Inversed : base.YAxis.Inversed);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		BrushInfo brush = GetBrush();
		Pen gdipPen = seriesStyle.GdipPen;
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		_ = PointF.Empty;
		_ = PointF.Empty;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		int i = 0;
		for (int num = array2.Length; i < num; i++)
		{
			IndexRange indexRange = array2[i];
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			bool flag3 = false;
			bool flag4 = false;
			IList visibleList = m_series.ChartModel.Series.VisibleList;
			for (int j = 0; j < visibleList.Count; j++)
			{
				ChartSeries chartSeries = visibleList[j] as ChartSeries;
				if (chartSeries.Type == m_series.Type)
				{
					flag4 = chartSeries == m_series;
					break;
				}
			}
			for (int num2 = visibleList.Count - 1; num2 > -1; num2--)
			{
				ChartSeries chartSeries2 = visibleList[num2] as ChartSeries;
				if (chartSeries2.Type == m_series.Type)
				{
					flag3 = chartSeries2 == m_series;
					break;
				}
			}
			Region region = null;
			PointF a = PointF.Empty;
			PointF c = PointF.Empty;
			ChartStyledPoint chartStyledPoint = null;
			int k = indexRange.From;
			for (int num3 = indexRange.To + 1; k < num3; k++)
			{
				ChartStyledPoint chartStyledPoint2 = array[k];
				if (!chartStyledPoint2.IsVisible)
				{
					continue;
				}
				double stackInfoValue = GetStackInfoValue(chartStyledPoint2.Index, isWithMe: false);
				double stackInfoValue2 = GetStackInfoValue(chartStyledPoint2.Index, isWithMe: true);
				PointF pointF = args.GetPoint(chartStyledPoint2.X, stackInfoValue);
				PointF pointF2 = args.GetPoint(chartStyledPoint2.X, stackInfoValue2);
				if (!dropSeriesPoints || chartStyledPoint == null || !(Math.Abs(args.IsInvertedAxes ? (pointF.Y - a.Y) : (pointF.X - a.X)) < 1f))
				{
					bool flag5 = (stackInfoValue > stackInfoValue2) ^ flag2;
					if (chartStyledPoint != null && flag5)
					{
						ChartMath.LineSegmentIntersectionPoint(a, pointF, c, pointF2);
					}
					if (flag5)
					{
						PointF pointF3 = pointF;
						pointF = pointF2;
						pointF2 = pointF3;
					}
					arrayList.Add(pointF2);
					arrayList2.Add(pointF);
					chartStyledPoint = chartStyledPoint2;
					a = pointF;
					c = pointF2;
				}
			}
			if (arrayList.Count <= 0)
			{
				continue;
			}
			PointF[] array3 = (PointF[])arrayList.ToArray(typeof(PointF));
			PointF[] array4 = (PointF[])arrayList2.ToArray(typeof(PointF));
			Array.Reverse(array4);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLines(array3);
			graphicsPath.CloseFigure();
			if (args.Is3D)
			{
				PointF pointF4 = array3[0];
				PointF pointF5 = array4[^1];
				PointF pointF6 = array3[^1];
				PointF pointF7 = array4[0];
				GraphicsPath graphicsPath2 = new GraphicsPath();
				GraphicsPath graphicsPath3 = new GraphicsPath();
				graphicsPath2.AddPolygon(new PointF[4]
				{
					pointF5,
					pointF4,
					ChartMath.AddPoint(pointF4, args.DepthOffset),
					ChartMath.AddPoint(pointF5, args.DepthOffset)
				});
				graphicsPath3.AddPolygon(new PointF[4]
				{
					pointF7,
					pointF6,
					ChartMath.AddPoint(pointF6, args.DepthOffset),
					ChartMath.AddPoint(pointF7, args.DepthOffset)
				});
				Math.Abs(args.DepthOffset.Width);
				Math.Abs(args.DepthOffset.Height);
				PointF[] array5 = (flag2 ? array4 : array3);
				PointF[] array6 = (flag2 ? array3 : array4);
				PointF pointF8 = PointF.Empty;
				PointF pointF9 = PointF.Empty;
				PointF pointF10 = PointF.Empty;
				PointF pointF11 = PointF.Empty;
				int num4 = (flag ? (array5.Length - 1) : 0);
				int num5 = (flag ? (-1) : array5.Length);
				int num6 = ((!flag) ? 1 : (-1));
				for (int l = num4; l != num5; l += num6)
				{
					PointF pointF12 = array5[l];
					PointF pointF13 = ChartMath.AddPoint(pointF12, args.DepthOffset);
					PointF pointF14 = array6[array6.Length - l - 1];
					PointF pointF15 = ChartMath.AddPoint(pointF14, args.DepthOffset);
					if (l != num4)
					{
						bool flag6 = false;
						GraphicsPath graphicsPath4 = new GraphicsPath();
						GraphicsPath graphicsPath5 = new GraphicsPath();
						graphicsPath4.AddPolygon(new PointF[4] { pointF12, pointF13, pointF9, pointF8 });
						graphicsPath5.AddPolygon(new PointF[4] { pointF14, pointF15, pointF11, pointF10 });
						flag6 = ((!args.IsInvertedAxes) ? (pointF14.Y <= pointF10.Y) : (pointF14.X >= pointF10.X));
						if (flag3 ^ args.ActualYAxis.Inversed)
						{
							args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
							if (!pointF8.IsEmpty && args.Chart.Style3D)
							{
								GraphicsPath graphicsPath6 = new GraphicsPath();
								graphicsPath6.AddPolygon(new PointF[4] { pointF12, pointF13, pointF9, pointF8 });
								args.Graph.DrawPath(brush, gdipPen, graphicsPath6);
								Draw(args.Graph, graphicsPath6, brush, gdipPen);
							}
						}
						if (!flag6 && (flag4 ^ args.ActualYAxis.Inversed))
						{
							args.Graph.DrawPath(brush, gdipPen, graphicsPath5);
						}
					}
					pointF8 = pointF12;
					pointF9 = pointF13;
					pointF10 = pointF14;
					pointF11 = pointF15;
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
					region.Intersect(clipBounds);
				}
			}
			else if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath7 = (GraphicsPath)graphicsPath.Clone();
				graphicsPath7.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
				args.Graph.DrawPath(brush, null, graphicsPath7);
			}
			gdipPen.Color = brush.BackColor;
			for (int m = 0; m < graphicsPath.PointCount - 1; m++)
			{
				gdipPen.Color = base.StyledPoints[m + 1].Style.Border.Color;
				args.Graph.DrawLine(gdipPen, new PointF(graphicsPath.PathPoints[m].X, graphicsPath.PathPoints[m].Y), new PointF(graphicsPath.PathPoints[m + 1].X, graphicsPath.PathPoints[m + 1].Y));
			}
			if (region != null)
			{
				region.Union(graphicsPath);
				region.Intersect(clipBounds);
			}
		}
	}

	protected override void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		RenderErrorBar(g, point);
		base.RenderAdornment(g, point);
	}

	public override void Render(Graphics3D g)
	{
		_ = OriginLocation;
		_ = base.SeriesStyle.GdipPen;
		BrushInfo brush = GetBrush();
		int num = base.Chart.Series.IndexOf(m_series);
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		_ = m_series.YAxis.Inversed;
		int num2 = base.Chart.Series.Count - 1;
		new ArrayList();
		new ArrayList();
		new ArrayList();
		new ArrayList();
		new ArrayList();
		g.AddPolygon(CreateBoundsPolygon(placeDepth));
		while (num2 > 0 && (base.Chart.Series[num2].Type != ChartSeriesType.StackingArea || !base.Chart.Series[num2].Visible))
		{
			num2--;
		}
		for (int i = 0; i <= num2 && (base.Chart.Series[i].Type != ChartSeriesType.StackingArea || !base.Chart.Series[i].Visible); i++)
		{
		}
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array = CalculateUnEmptyRanges(vrange);
		ChartPointWithIndex[] array2 = new ChartPointWithIndex[m_series.Points.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = new ChartPointWithIndex(m_series.Points[j], j);
		}
		for (int k = 0; k < array.Length; k++)
		{
			IndexRange indexRange = array[k];
			if (indexRange.To <= indexRange.From)
			{
				continue;
			}
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			ArrayList arrayList3 = new ArrayList();
			int l = indexRange.From;
			for (int num3 = indexRange.To + 1; l < num3; l++)
			{
				ChartPoint point = array2[l].Point;
				int index = array2[l].Index;
				if (IsVisiblePoint(point))
				{
					arrayList.Add(array2[l]);
					double stackInfoValue = GetStackInfoValue(index);
					ChartPoint cp = new ChartPoint(point.X, GetStackInfoValue(index, isWithMe: true));
					PointF pointF = new PointF(GetXFromValue(cp, 0), GetYFromValue(cp, 0));
					arrayList2.Add(pointF);
					cp = new ChartPoint(point.X, stackInfoValue);
					arrayList3.Add(new PointF(GetXFromValue(cp, 0), GetYFromValue(cp, 0)));
				}
			}
			int m = 0;
			int count = arrayList3.Count;
			for (int num4 = arrayList3.Count / 2; m < num4; m++)
			{
				PointF pointF2 = (PointF)arrayList3[m];
				int index2 = count - m - 1;
				arrayList3[m] = arrayList3[index2];
				arrayList3[index2] = pointF2;
			}
			PointF[] array3 = (PointF[])arrayList2.ToArray(typeof(PointF));
			PointF[] array4 = (PointF[])arrayList3.ToArray(typeof(PointF));
			Vector3D[] array5 = new Vector3D[array3.Length + array4.Length];
			Vector3D[] array6 = new Vector3D[array3.Length + array4.Length];
			_ = indexRange.To;
			_ = indexRange.From;
			int num5 = array3.Length;
			int num6 = array4.Length;
			for (int n = 0; n < num5; n++)
			{
				array5[n] = new Vector3D(array3[n].X, array3[n].Y, placeDepth);
				array6[n] = new Vector3D(array3[n].X, array3[n].Y, placeDepth + seriesDepth);
			}
			for (int num7 = 0; num7 < num6; num7++)
			{
				array5[num7 + num5] = new Vector3D(array4[num7].X, array4[num7].Y, placeDepth);
				array6[num7 + num5] = new Vector3D(array4[num7].X, array4[num7].Y, placeDepth + seriesDepth);
			}
			Polygon polygon = new Polygon(array5, brush);
			Polygon polygon2 = new Polygon(array6, brush);
			Polygon polygon3 = new Polygon(new Vector3D[4]
			{
				array5[0],
				array6[0],
				array6[^1],
				array5[^1]
			}, brush, base.SeriesStyle.GdipPen);
			Polygon polygon4 = new Polygon(new Vector3D[4]
			{
				array5[num5 - 1],
				array6[num5 - 1],
				array6[num5],
				array5[num5]
			}, brush, base.SeriesStyle.GdipPen);
			g.AddPolygon(polygon);
			g.AddPolygon(polygon2);
			g.AddPolygon(polygon3);
			g.AddPolygon(polygon4);
			for (int num8 = 0; num8 < num5 - 1; num8++)
			{
				Vector3D vector3D = new Vector3D(array3[num8].X, array3[num8].Y, placeDepth);
				Vector3D vector3D2 = new Vector3D(array3[num8].X, array3[num8].Y, placeDepth + seriesDepth);
				Vector3D vector3D3 = new Vector3D(array3[num8 + 1].X, array3[num8 + 1].Y, placeDepth + seriesDepth);
				Vector3D vector3D4 = new Vector3D(array3[num8 + 1].X, array3[num8 + 1].Y, placeDepth);
				Polygon polygon5 = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, GetBrush(), base.SeriesStyle.GdipPen);
				g.AddPolygon(polygon5);
			}
			for (int num9 = 0; num9 < num6 - 1; num9++)
			{
				Vector3D vector3D5 = new Vector3D(array4[num9].X, array4[num9].Y, placeDepth);
				Vector3D vector3D6 = new Vector3D(array4[num9].X, array4[num9].Y, placeDepth + seriesDepth);
				Vector3D vector3D7 = new Vector3D(array4[num9 + 1].X, array4[num9 + 1].Y, placeDepth + seriesDepth);
				Vector3D vector3D8 = new Vector3D(array4[num9 + 1].X, array4[num9 + 1].Y, placeDepth);
				Polygon polygon6 = new Polygon(new Vector3D[4] { vector3D5, vector3D6, vector3D7, vector3D8 }, brush, null);
				g.AddPolygon(polygon6);
			}
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		Pen pen = null;
		graphicsPath.AddLine(bounds.Left + bounds.Width / 8 - 2, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)), bounds.Right + 2, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)));
		using (pen = base.SeriesStyle.GdipPen.Clone() as Pen)
		{
			pen.Color = (isShadow ? shadowColor : base.SeriesStyle.Interior.BackColor);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
		if (m_series.Style.HasSymbol && m_series.Style.Symbol.Shape != 0)
		{
			BrushInfo brush = new BrushInfo(m_series.Style.Symbol.Color);
			pen = new Pen(m_series.Style.Symbol.Border.Color);
			graphicsPath.AddPath(ChartSymbolHelper.GetPathSymbol(base.SeriesStyle.Symbol.Shape, bounds), connect: false);
			graphicsPath.CloseFigure();
			BrushPaint.FillPath(g, graphicsPath, brush);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
	}

	public override DoubleRange GetYDataMeasure()
	{
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			if (m_series.Points[i].YValues.Length != 0)
			{
				double stackInfoValue = GetStackInfoValue(i, isWithMe: true);
				if (stackInfoValue > num)
				{
					num = stackInfoValue;
				}
				if (stackInfoValue < num2)
				{
					num2 = stackInfoValue;
				}
			}
		}
		DoubleRange doubleRange = new DoubleRange(num2, num);
		if (m_series.OriginDependent)
		{
			if (m_series.ActualYAxis.CustomOrigin)
			{
				return doubleRange + m_series.ActualYAxis.Origin;
			}
			return doubleRange + 0.0;
		}
		return doubleRange;
	}
}
