using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class ColumnRangeRenderer : ColumnRenderer
{
	protected override int RequireYValuesCount => 2;

	protected override string RegionDescription => "Column Range Chart Region";

	protected override bool IsFixedWidth => false;

	public ColumnRangeRenderer(ChartSeries series)
		: base(series)
	{
	}

	private PointF DrawPoint(Graphics g, ChartRenderArgs2D args, ChartStyledPoint stlPoint, DoubleRange sbsInfo, PointF previosPoint, ChartStyleInfo seriesStyle, RectangleF clip, ArrayList pathsList, SizeF cornerRadius, SizeF seriesOffset, SizeF depthOffset, bool is3d, bool isInvertedAxes, bool dropPoints, bool isCylinder, int seriesIndex)
	{
		ChartStyleInfo style = stlPoint.Style;
		ChartPoint point = stlPoint.Point;
		if (IsVisiblePoint(point))
		{
			PointF pointFromValue = GetPointFromValue(point);
			if (base.Chart.Indexed || !base.Chart.AllowGapForEmptyPoints)
			{
				pointFromValue = GetPointFromValue(stlPoint.X, stlPoint.YValues[0]);
			}
			if (dropPoints && !previosPoint.IsEmpty && ((!isInvertedAxes && Math.Abs(pointFromValue.X - previosPoint.X) < 1f) || (isInvertedAxes && Math.Abs(pointFromValue.Y - previosPoint.Y) < 1f)))
			{
				return PointF.Empty;
			}
			double x = point.X + sbsInfo.Start;
			double x2 = point.X + sbsInfo.End;
			double num = point.YValues[0];
			double num2 = point.YValues[1];
			RectangleF rectangle = GetRectangle(new ChartPoint(x, num), new ChartPoint(x2, num2));
			if (base.Chart.Indexed || !base.Chart.AllowGapForEmptyPoints)
			{
				x = stlPoint.X + sbsInfo.Start;
				x2 = stlPoint.X + sbsInfo.End;
				rectangle = args.GetRectangle(x, num, x2, num2);
			}
			GraphicsPath graphicsPath = null;
			GraphicsPath gp = null;
			GraphicsPath gp2 = null;
			if (is3d)
			{
				rectangle.Offset(seriesOffset.Width, seriesOffset.Height);
				graphicsPath = CreateBox(rectangle, is3d);
				if (!args.Chart.Style3D)
				{
					graphicsPath = CreateBox(rectangle, is3d);
				}
				else
				{
					graphicsPath = CreateBox(rectangle, is3d);
					gp = CreateBoxRight(rectangle, is3d);
					gp2 = CreateBoxTop(rectangle, is3d);
				}
				if (isCylinder)
				{
					if (isInvertedAxes)
					{
						if (!args.Chart.Style3D)
						{
							graphicsPath = CreateHorizintalCylinder3D(rectangle, depthOffset);
						}
						else
						{
							graphicsPath = CreateHorizintalCylinder3D(rectangle, depthOffset);
							gp2 = CreateHorizintalCylinder3DTop(rectangle, depthOffset);
							gp = null;
						}
					}
					else if (!args.Chart.Style3D)
					{
						graphicsPath = CreateVerticalCylinder3D(rectangle, depthOffset);
					}
					else
					{
						graphicsPath = CreateVerticalCylinder3D(rectangle, depthOffset);
						gp2 = CreateVerticalCylinder3DTop(rectangle, depthOffset);
						gp = null;
					}
				}
			}
			else
			{
				graphicsPath = RenderingHelper.CreateRoundRect(rectangle, cornerRadius);
				if (stlPoint.Style.DisplayShadow)
				{
					RectangleF r = rectangle;
					r.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
					BrushPaint.FillRectangle(g, r, style.ShadowInterior);
				}
			}
			ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
			if (!args.Chart.Style3D)
			{
				chartSeriesPath.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style));
			}
			else
			{
				chartSeriesPath.AddPrimitive(graphicsPath, null, BrushInfo.Empty);
				ChartSeriesPath chartSeriesPath2 = new ChartSeriesPath();
				chartSeriesPath2.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style), "BoxCenter");
				chartSeriesPath2.Draw(args.Graph, "BoxCenter");
				ChartSeriesPath chartSeriesPath3 = new ChartSeriesPath();
				chartSeriesPath3.AddPrimitive(gp, style.GdipPen, GetBrush(style), "BoxRight");
				chartSeriesPath3.Draw(args.Graph, "BoxRight");
				ChartSeriesPath chartSeriesPath4 = new ChartSeriesPath();
				chartSeriesPath4.AddPrimitive(gp2, style.GdipPen, GetBrush(style), "BoxTop");
				chartSeriesPath4.Draw(args.Graph, "BoxTop");
			}
			chartSeriesPath.Bounds = rectangle;
			if (is3d && base.Chart.ColumnDrawMode == ChartColumnDrawMode.PlaneMode)
			{
				pathsList.Add(chartSeriesPath);
			}
			else
			{
				chartSeriesPath.Draw(g);
			}
			return pointFromValue;
		}
		return PointF.Empty;
	}

	private void RenderSeries(Graphics g, ChartStyledPoint styledPoint)
	{
		bool series3D = base.Chart.Series3D;
		bool isInvertedAxes = IsInvertedAxes;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool isCylinder = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		int seriesIndex = base.Chart.Series.IndexOf(m_series);
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		SizeF cornerRadius = m_series.ConfigItems.ColumnItem.CornerRadius;
		SizeF thisOffset = GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
		chartRenderArgs2D.Graph = new ChartGDIGraph(g);
		PointF previosPoint = PointF.Empty;
		RectangleF clipBounds = g.ClipBounds;
		ArrayList pathsList = new ArrayList(m_series.Points.Count);
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				_ = array[i];
				previosPoint = DrawPoint(g, chartRenderArgs2D, array[i], sideBySideInfo, previosPoint, seriesStyle, clipBounds, pathsList, cornerRadius, thisOffset, seriesOffset, series3D, isInvertedAxes, dropSeriesPoints, isCylinder, seriesIndex);
			}
			{
				foreach (Trendline trendline in m_series.Trendlines)
				{
					if (trendline.Visible)
					{
						trendline.TrendlineDraw(chartRenderArgs2D, m_series);
					}
				}
				return;
			}
		}
		DrawPoint(g, chartRenderArgs2D, styledPoint, sideBySideInfo, previosPoint, seriesStyle, clipBounds, pathsList, cornerRadius, thisOffset, seriesOffset, series3D, isInvertedAxes, dropSeriesPoints, isCylinder, seriesIndex);
	}

	public override void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
		ChartStyledPoint styledPoint = new ChartStyledPoint(point, info, pointIndex);
		if (!base.Chart.Series3D || !base.Chart.RealMode3D)
		{
			RenderSeries(g, styledPoint);
		}
	}

	public override void Render(Graphics g)
	{
		RenderSeries(g, null);
	}

	public override void Render(Graphics3D g)
	{
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		base.Chart.Series.IndexOf(m_series);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool flag = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		bool isInvertedAxes = IsInvertedAxes;
		float placeDepth = GetPlaceDepth();
		float num = placeDepth + GetSeriesDepth();
		_ = m_series.ActualYAxis.Origin;
		g.AddPolygon(CreateBoundsPolygon(placeDepth));
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		PointF pointF = PointF.Empty;
		ChartRenderArgs3D chartRenderArgs3D = new ChartRenderArgs3D(base.Chart, m_series);
		ChartStyledPoint chartStyledPoint = null;
		int i = indexRange.From;
		for (int num2 = indexRange.To + 1; i < num2; i++)
		{
			chartStyledPoint = array[i];
			ChartPoint point = chartStyledPoint.Point;
			ChartStyleInfo style = chartStyledPoint.Style;
			int index = chartStyledPoint.Index;
			BrushInfo brush = GetBrush(index);
			Pen gdipPen = style.GdipPen;
			if (!IsVisiblePoint(point))
			{
				continue;
			}
			PointF pointFromValue = GetPointFromValue(point);
			if (base.Chart.Indexed || !base.Chart.AllowGapForEmptyPoints)
			{
				pointFromValue = GetPointFromValue(chartStyledPoint.X, chartStyledPoint.YValues[0]);
			}
			if (dropSeriesPoints && !pointF.IsEmpty && ((!isInvertedAxes && Math.Abs(pointFromValue.X - pointF.X) < 1f) || (isInvertedAxes && Math.Abs(pointFromValue.Y - pointF.Y) < 1f)))
			{
				continue;
			}
			double x = point.X + sideBySideInfo.Start;
			double x2 = point.X + sideBySideInfo.End;
			double num3 = point.YValues[0];
			double num4 = point.YValues[1];
			RectangleF rectangle = GetRectangle(new ChartPoint(x, num3), new ChartPoint(x2, num4));
			if (base.Chart.Indexed || !base.Chart.AllowGapForEmptyPoints)
			{
				x = chartStyledPoint.X + sideBySideInfo.Start;
				x2 = chartStyledPoint.X + sideBySideInfo.End;
				rectangle = chartRenderArgs3D.GetRectangle(x, num3, x2, num4);
			}
			Vector3D v = new Vector3D(rectangle.Left, rectangle.Top, placeDepth);
			Vector3D v2 = new Vector3D(rectangle.Right, rectangle.Bottom, num);
			if (m_series.Rotate)
			{
				if (flag)
				{
					g.CreateBox(v, v2, (Pen)null, (BrushInfo)null);
					g.CreateCylinderH(v, v2, POLYGON_SECTORS, gdipPen, brush);
				}
				else
				{
					g.CreateBox(v, v2, gdipPen, brush);
				}
			}
			else if (flag)
			{
				g.CreateBoxV(v, v2, null, null);
				g.CreateCylinderV(v, v2, POLYGON_SECTORS, gdipPen, brush);
			}
			else
			{
				g.CreateBoxV(v, v2, gdipPen, brush);
			}
			pointF = pointFromValue;
		}
	}
}
