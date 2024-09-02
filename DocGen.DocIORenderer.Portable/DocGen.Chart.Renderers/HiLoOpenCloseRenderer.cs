using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class HiLoOpenCloseRenderer : HiLoRenderer
{
	protected override string RegionDescription => "HiLo Chart  Region";

	protected override int RequireYValuesCount => 4;

	public HiLoOpenCloseRenderer(ChartSeries series)
		: base(series)
	{
	}

	private void DrawPoint(Graphics g, ChartStyledPoint styledPoint, DoubleRange sbsInfo, bool is3d, int seriesIndex, bool rightToLeft, SizeF seriesOffset, SizeF depthOffset, RectangleF clip, ChartOpenCloseDrawMode drawMode, Color openWingColor, Color closeWingColor, bool drawOpenWing, bool drawCloseWing)
	{
		if (IsVisiblePoint(styledPoint.Point))
		{
			ChartPoint chartPoint = new ChartPoint(0.0, styledPoint.Point.YValues);
			GraphicsPath graphicsPath = new GraphicsPath();
			GraphicsPath graphicsPath2 = new GraphicsPath();
			BrushInfo interior = new BrushInfo(styledPoint.Style.Interior);
			new ChartRenderArgs2D(base.Chart, m_series).Graph = new ChartGDIGraph(g);
			if (drawOpenWing)
			{
				chartPoint.X = styledPoint.Point.X + sbsInfo.Median;
				PointF pointFromValue = GetPointFromValue(chartPoint, 2);
				chartPoint.X = styledPoint.Point.X + sbsInfo.Start;
				PointF pointFromValue2 = GetPointFromValue(chartPoint, 2);
				if (is3d)
				{
					GraphicsPath graphicsPath3 = new GraphicsPath();
					pointFromValue = ChartMath.AddPoint(pointFromValue, seriesOffset);
					pointFromValue2 = ChartMath.AddPoint(pointFromValue2, seriesOffset);
					graphicsPath3.AddLine(pointFromValue, ChartMath.AddPoint(pointFromValue, depthOffset));
					graphicsPath3.AddLine(ChartMath.AddPoint(pointFromValue2, depthOffset), pointFromValue2);
					graphicsPath3.CloseFigure();
					if (openWingColor != Color.Empty)
					{
						styledPoint.Style.Interior = new BrushInfo(openWingColor);
					}
					BrushPaint.FillPath(g, graphicsPath3, GetBrush(styledPoint.Index));
					styledPoint.Style.Interior = interior;
					graphicsPath = graphicsPath3;
				}
				else
				{
					using Pen pen = styledPoint.Style.GdipPen.Clone() as Pen;
					if (styledPoint.Style.DisplayShadow)
					{
						Size shadowOffset = styledPoint.Style.ShadowOffset;
						PointF pt = ChartMath.AddPoint(pointFromValue, shadowOffset);
						PointF pt2 = ChartMath.AddPoint(pointFromValue2, shadowOffset);
						pen.Color = styledPoint.Style.ShadowInterior.BackColor;
						g.DrawLine(pen, pt, pt2);
					}
					pen.Color = GetBrush(styledPoint.Index).BackColor;
					if (openWingColor != Color.Empty)
					{
						pen.Color = openWingColor;
					}
					g.DrawLine(pen, pointFromValue, pointFromValue2);
				}
			}
			if (drawCloseWing)
			{
				chartPoint.X = styledPoint.Point.X + sbsInfo.Median;
				PointF pointFromValue3 = GetPointFromValue(chartPoint, 3);
				chartPoint.X = styledPoint.Point.X + sbsInfo.End;
				PointF pointFromValue4 = GetPointFromValue(chartPoint, 3);
				if (is3d)
				{
					GraphicsPath graphicsPath4 = new GraphicsPath();
					pointFromValue3 = ChartMath.AddPoint(pointFromValue3, seriesOffset);
					pointFromValue4 = ChartMath.AddPoint(pointFromValue4, seriesOffset);
					graphicsPath4.AddLine(pointFromValue3, ChartMath.AddPoint(pointFromValue3, depthOffset));
					graphicsPath4.AddLine(ChartMath.AddPoint(pointFromValue4, depthOffset), pointFromValue4);
					graphicsPath4.CloseFigure();
					if (closeWingColor != Color.Empty)
					{
						styledPoint.Style.Interior = new BrushInfo(closeWingColor);
					}
					BrushPaint.FillPath(g, graphicsPath4, GetBrush(styledPoint.Index));
					styledPoint.Style.Interior = interior;
					graphicsPath2 = graphicsPath4;
				}
				else
				{
					using Pen pen2 = styledPoint.Style.GdipPen.Clone() as Pen;
					if (styledPoint.Style.DisplayShadow)
					{
						Size shadowOffset2 = styledPoint.Style.ShadowOffset;
						PointF pt3 = ChartMath.AddPoint(pointFromValue3, shadowOffset2);
						PointF pt4 = ChartMath.AddPoint(pointFromValue4, shadowOffset2);
						pen2.Color = styledPoint.Style.ShadowInterior.BackColor;
						g.DrawLine(pen2, pt3, pt4);
					}
					pen2.Color = GetBrush(styledPoint.Index).BackColor;
					if (closeWingColor != Color.Empty)
					{
						pen2.Color = closeWingColor;
					}
					g.DrawLine(pen2, pointFromValue3, pointFromValue4);
				}
			}
			GraphicsPath graphicsPath5 = (rightToLeft ? graphicsPath2 : graphicsPath);
			if (graphicsPath5 == graphicsPath)
			{
				styledPoint.Style.Interior = new BrushInfo(openWingColor);
			}
			else
			{
				styledPoint.Style.Interior = new BrushInfo(openWingColor);
			}
			g.DrawPath(styledPoint.Style.GdipPen, graphicsPath5);
			BrushPaint.FillPath(g, graphicsPath5, GetBrush(styledPoint.Index));
			styledPoint.Style.Interior = interior;
			chartPoint.X = styledPoint.Point.X + sbsInfo.Median;
			PointF pointFromValue5 = GetPointFromValue(chartPoint, 0);
			PointF pointFromValue6 = GetPointFromValue(chartPoint, 1);
			if (is3d)
			{
				GraphicsPath graphicsPath6 = new GraphicsPath();
				pointFromValue5 = ChartMath.AddPoint(pointFromValue5, seriesOffset);
				pointFromValue6 = ChartMath.AddPoint(pointFromValue6, seriesOffset);
				graphicsPath6.AddLine(pointFromValue5, ChartMath.AddPoint(pointFromValue5, depthOffset));
				graphicsPath6.AddLine(ChartMath.AddPoint(pointFromValue6, depthOffset), pointFromValue6);
				graphicsPath6.CloseFigure();
				BrushPaint.FillPath(g, graphicsPath6, GetBrush(styledPoint.Index));
				g.DrawPath(styledPoint.Style.GdipPen, graphicsPath6);
			}
			else
			{
				using Pen pen3 = styledPoint.Style.GdipPen.Clone() as Pen;
				if (styledPoint.Style.DisplayShadow)
				{
					Size shadowOffset3 = styledPoint.Style.ShadowOffset;
					PointF pt5 = ChartMath.AddPoint(pointFromValue5, shadowOffset3);
					PointF pt6 = ChartMath.AddPoint(pointFromValue6, shadowOffset3);
					pen3.Color = styledPoint.Style.ShadowInterior.BackColor;
					g.DrawLine(pen3, pt5, pt6);
				}
				pen3.Color = GetBrush(styledPoint.Index).BackColor;
				g.DrawLine(pen3, pointFromValue5, pointFromValue6);
			}
			GraphicsPath graphicsPath7 = (rightToLeft ? graphicsPath : graphicsPath2);
			if (graphicsPath7 == graphicsPath2)
			{
				styledPoint.Style.Interior = new BrushInfo(closeWingColor);
			}
			else
			{
				styledPoint.Style.Interior = new BrushInfo(openWingColor);
			}
			g.DrawPath(styledPoint.Style.GdipPen, graphicsPath7);
			BrushPaint.FillPath(g, graphicsPath7, GetBrush(styledPoint.Index));
			styledPoint.Style.Interior = interior;
		}
		foreach (Trendline trendline in m_series.Trendlines)
		{
			if (trendline.Visible)
			{
				ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
				chartRenderArgs2D.Graph = new ChartGDIGraph(g);
				trendline.TrendlineDraw(chartRenderArgs2D, m_series);
			}
		}
	}

	private void RenderSeries(Graphics g, ChartStyledPoint styledPoint)
	{
		bool series3D = base.Chart.Series3D;
		int seriesIndex = base.Chart.Series.IndexOf(m_series);
		bool inversed = m_series.ActualXAxis.Inversed;
		SizeF thisOffset = GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		RectangleF clipBounds = g.ClipBounds;
		ChartOpenCloseDrawMode drawMode = m_series.ConfigItems.HiLoOpenCloseItem.DrawMode;
		Color openTipColor = m_series.ConfigItems.HiLoOpenCloseItem.OpenTipColor;
		Color closeTipColor = m_series.ConfigItems.HiLoOpenCloseItem.CloseTipColor;
		bool drawOpenWing = drawMode == ChartOpenCloseDrawMode.Open || drawMode == ChartOpenCloseDrawMode.Both;
		bool drawCloseWing = drawMode == ChartOpenCloseDrawMode.Close || drawMode == ChartOpenCloseDrawMode.Both;
		if (styledPoint == null)
		{
			IndexRange indexRange = CalculateVisibleRange();
			ChartStyledPoint[] array = PrepearePoints();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				DrawPoint(g, array[i], sideBySideInfo, series3D, seriesIndex, inversed, thisOffset, seriesOffset, clipBounds, drawMode, openTipColor, closeTipColor, drawOpenWing, drawCloseWing);
			}
		}
		else
		{
			DrawPoint(g, styledPoint, sideBySideInfo, series3D, seriesIndex, inversed, thisOffset, seriesOffset, clipBounds, drawMode, openTipColor, closeTipColor, drawOpenWing, drawCloseWing);
		}
	}

	public override void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
		if (!base.Chart.Series3D || !base.Chart.RealMode3D)
		{
			ChartStyledPoint styledPoint = new ChartStyledPoint(point, info, pointIndex);
			RenderSeries(g, styledPoint);
		}
	}

	public override void Render(Graphics g)
	{
		RenderSeries(g, null);
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		ChartOpenCloseDrawMode drawMode = m_series.ConfigItems.HiLoOpenCloseItem.DrawMode;
		bool flag = drawMode == ChartOpenCloseDrawMode.Open || drawMode == ChartOpenCloseDrawMode.Both;
		bool flag2 = drawMode == ChartOpenCloseDrawMode.Close || drawMode == ChartOpenCloseDrawMode.Both;
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = PrepearePoints();
		Polygon[] array2 = new Polygon[indexRange.To - indexRange.From + 1];
		Polygon[] array3 = new Polygon[indexRange.To - indexRange.From + 1];
		float placeDepth = GetPlaceDepth();
		float num = placeDepth + GetSeriesDepth();
		g.AddPolygon(CreateBoundsPolygon(placeDepth));
		ChartStyledPoint chartStyledPoint = null;
		int i = indexRange.From;
		for (int num2 = indexRange.To + 1; i < num2; i++)
		{
			chartStyledPoint = array[i];
			if (IsVisiblePoint(chartStyledPoint.Point))
			{
				ChartPoint chartPoint = new ChartPoint(chartStyledPoint.Point.X, chartStyledPoint.Point.YValues);
				chartPoint.X += sideBySideInfo.Median;
				PointF pointFromValue = GetPointFromValue(chartPoint, 0);
				PointF pointFromValue2 = GetPointFromValue(chartPoint, 1);
				Polygon polygon = new Polygon(new Vector3D[4]
				{
					new Vector3D(pointFromValue.X, pointFromValue.Y, placeDepth),
					new Vector3D(pointFromValue2.X, pointFromValue2.Y, placeDepth),
					new Vector3D(pointFromValue2.X, pointFromValue2.Y, num),
					new Vector3D(pointFromValue.X, pointFromValue.Y, num)
				}, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
				g.AddPolygon(polygon);
				if (flag)
				{
					chartPoint.X = chartStyledPoint.Point.X + sideBySideInfo.Median;
					PointF pointFromValue3 = GetPointFromValue(chartPoint, 2);
					chartPoint.X = chartStyledPoint.Point.X + sideBySideInfo.Start;
					PointF pointFromValue4 = GetPointFromValue(chartPoint, 2);
					Polygon polygon2 = new Polygon(new Vector3D[4]
					{
						new Vector3D(pointFromValue3.X, pointFromValue3.Y, placeDepth),
						new Vector3D(pointFromValue4.X, pointFromValue4.Y, placeDepth),
						new Vector3D(pointFromValue4.X, pointFromValue4.Y, num),
						new Vector3D(pointFromValue3.X, pointFromValue3.Y, num)
					}, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
					array3[i] = polygon2;
				}
				if (flag2)
				{
					chartPoint.X = chartStyledPoint.Point.X + sideBySideInfo.Median;
					PointF pointFromValue5 = GetPointFromValue(chartPoint, 3);
					chartPoint.X = chartStyledPoint.Point.X + sideBySideInfo.End;
					PointF pointFromValue6 = GetPointFromValue(chartPoint, 3);
					Polygon polygon3 = new Polygon(new Vector3D[4]
					{
						new Vector3D(pointFromValue5.X, pointFromValue5.Y, placeDepth),
						new Vector3D(pointFromValue6.X, pointFromValue6.Y, placeDepth),
						new Vector3D(pointFromValue6.X, pointFromValue6.Y, num),
						new Vector3D(pointFromValue5.X, pointFromValue5.Y, num)
					}, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
					array2[i] = polygon3;
				}
			}
		}
		for (int j = 0; j < array2.Length; j++)
		{
			if (array3[j] != null)
			{
				g.AddPolygon(array3[j]);
			}
			if (array2[j] != null)
			{
				g.AddPolygon(array2[j]);
			}
		}
	}
}
