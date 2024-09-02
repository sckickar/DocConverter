using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class HiLoRenderer : ChartSeriesRenderer
{
	protected override string RegionDescription => "HiLo Line";

	protected override int RequireYValuesCount => 2;

	public HiLoRenderer(ChartSeries series)
		: base(series)
	{
	}

	private BrushInfo GradientBrush(BrushInfo brushInfo)
	{
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		return brushInfo;
	}

	private void DrawPoint(Graphics g, ChartStyledPoint styledPoint, DoubleRange sbsInfo, bool is3d, SizeF seriesOffset, SizeF depthOffset, int seriesIndex, RectangleF clip)
	{
		if (IsVisiblePoint(styledPoint.Point))
		{
			ChartPoint chartPoint = new ChartPoint(styledPoint.Point.X, styledPoint.Point.YValues);
			new ChartRenderArgs2D(base.Chart, m_series).Graph = new ChartGDIGraph(g);
			chartPoint.X += sbsInfo.Median;
			PointF pointFromValue = GetPointFromValue(chartPoint, 0);
			PointF pointFromValue2 = GetPointFromValue(chartPoint, 1);
			if (is3d)
			{
				GraphicsPath graphicsPath = new GraphicsPath();
				pointFromValue = ChartMath.AddPoint(pointFromValue, seriesOffset);
				pointFromValue2 = ChartMath.AddPoint(pointFromValue2, seriesOffset);
				graphicsPath.AddLine(pointFromValue, ChartMath.AddPoint(pointFromValue, depthOffset));
				graphicsPath.AddLine(ChartMath.AddPoint(pointFromValue2, depthOffset), pointFromValue2);
				graphicsPath.CloseFigure();
				BrushPaint.FillPath(g, graphicsPath, GetBrush(styledPoint.Index));
				g.DrawPath(styledPoint.Style.GdipPen, graphicsPath);
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
				g.DrawLine(pen, pointFromValue, pointFromValue2);
			}
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
		SizeF thisOffset = GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		RectangleF clipBounds = g.ClipBounds;
		if (styledPoint == null)
		{
			IndexRange indexRange = CalculateVisibleRange();
			ChartStyledPoint[] array = PrepearePoints();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				DrawPoint(g, array[i], sideBySideInfo, series3D, thisOffset, seriesOffset, seriesIndex, clipBounds);
			}
		}
		else
		{
			DrawPoint(g, styledPoint, sideBySideInfo, series3D, thisOffset, seriesOffset, seriesIndex, clipBounds);
		}
	}

	public override PointF GetSymbolCoordinates(int index)
	{
		return GetHiLoSymbolCoordinates(index);
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
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = PrepearePoints();
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
			}
		}
	}

	protected override BrushInfo GetBrush()
	{
		BrushInfo brushInfo = base.GetBrush();
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		return brushInfo;
	}

	protected override BrushInfo GetBrush(int index)
	{
		return GradientBrush(base.GetBrush(index));
	}

	protected override BrushInfo GetBrush(ChartStyleInfo style)
	{
		return GradientBrush(base.GetBrush(style));
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if (m_series.Style.HasSymbol && m_series.Style.Symbol.Shape != 0)
		{
			BrushInfo brush = new BrushInfo(m_series.Style.Symbol.Color);
			Pen pen = new Pen(m_series.Style.Symbol.Border.Color);
			graphicsPath.AddPath(ChartSymbolHelper.GetPathSymbol(base.SeriesStyle.Symbol.Shape, bounds), connect: false);
			graphicsPath.CloseFigure();
			BrushPaint.FillPath(g, graphicsPath, brush);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
		else if (m_series.Index == 0 && m_series.SerieType == "Column_Clustered")
		{
			BrushInfo brushInfo = base.SeriesStyle.Interior;
			ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
			if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
			{
				brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
			}
			BrushPaint.FillRectangle(g, bounds, brushInfo);
			g.DrawRectangle(base.SeriesStyle.GdipPen, bounds);
		}
	}
}
