using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class CandleRenderer : ColumnRenderer
{
	protected override int RequireYValuesCount => 4;

	protected override string RegionDescription => "Candle Chart Region";

	public CandleRenderer(ChartSeries series)
		: base(series)
	{
	}

	private PointF DrawPoint(ChartRenderArgs2D args, ChartStyledPoint stlPoint, DoubleRange sbsInfo, bool isInvertedAxes, bool isCylinder, bool dropPoints, ChartStyleInfo seriesSyle, SizeF cornerRadius, SizeF seriesOffset, SizeF depthOffset, SizeF halfOffset, SizeF lineOffset1, SizeF lineOffset2, Pen widenPen, PointF previousPoint, RectangleF clipPath)
	{
		ChartStyleInfo style = stlPoint.Style;
		ChartPoint point = stlPoint.Point;
		BrushInfo brush = GetBrush(style);
		Pen gdipPen = style.GdipPen;
		widenPen.Width = gdipPen.Width + 2f;
		if (stlPoint.IsVisible)
		{
			PointF pointFromValue = GetPointFromValue(point);
			if (!base.Chart.AllowGapForEmptyPoints)
			{
				pointFromValue = GetPointFromValue(stlPoint.X, stlPoint.YValues[0]);
			}
			if (dropPoints && !previousPoint.IsEmpty && ((!isInvertedAxes && Math.Abs(pointFromValue.X - previousPoint.X) < 1f) || (isInvertedAxes && Math.Abs(pointFromValue.Y - previousPoint.Y) < 1f)))
			{
				return PointF.Empty;
			}
			double num = point.X + sbsInfo.Start;
			double num2 = point.X + sbsInfo.End;
			double y = point.YValues[0];
			double y2 = point.YValues[1];
			double num3 = point.YValues[2];
			double num4 = point.YValues[3];
			double[] yValues = point.YValues;
			Array.Sort(yValues);
			PointF pointF = GetPointFromValue(new ChartPoint((num + num2) / 2.0, yValues[0]));
			PointF pointF2 = GetPointFromValue(new ChartPoint((num + num2) / 2.0, yValues[3]));
			RectangleF rect = GetRectangle(new ChartPoint(num, num3), new ChartPoint(num2, num4));
			if (base.Chart.Indexed || !base.Chart.AllowGapForEmptyPoints)
			{
				num = stlPoint.X + sbsInfo.Start;
				num2 = stlPoint.X + sbsInfo.End;
				pointF = args.GetPoint((num + num2) / 2.0, y);
				pointF2 = args.GetPoint((num + num2) / 2.0, y2);
				rect = args.GetRectangle(num, num3, num2, num4);
			}
			CheckColumnBounds(args.IsInvertedAxes, ref rect);
			GraphicsPath graphicsPath = null;
			GraphicsPath gp = null;
			GraphicsPath gp2 = null;
			if (args.Is3D)
			{
				pointF = ChartMath.AddPoint(pointF, seriesOffset);
				pointF2 = ChartMath.AddPoint(pointF2, seriesOffset);
				rect.Offset(seriesOffset.Width, seriesOffset.Height);
				if (!args.Chart.Style3D)
				{
					graphicsPath = CreateBox(rect, args.Is3D);
				}
				else
				{
					graphicsPath = CreateBox(rect, args.Is3D);
					gp = CreateBoxRight(rect, args.Is3D);
					gp2 = CreateBoxTop(rect, args.Is3D);
				}
				if (isCylinder)
				{
					if (isInvertedAxes)
					{
						if (!args.Chart.Style3D)
						{
							graphicsPath = CreateHorizintalCylinder3D(rect, depthOffset);
						}
						else
						{
							graphicsPath = CreateHorizintalCylinder3D(rect, depthOffset);
							gp2 = CreateHorizintalCylinder3DTop(rect, depthOffset);
							gp = null;
						}
					}
					else if (!args.Chart.Style3D)
					{
						graphicsPath = CreateVerticalCylinder3D(rect, depthOffset);
					}
					else
					{
						graphicsPath = CreateVerticalCylinder3D(rect, depthOffset);
						gp2 = CreateVerticalCylinder3DTop(rect, depthOffset);
						gp = null;
					}
				}
			}
			else
			{
				graphicsPath = RenderingHelper.CreateRoundRect(rect, cornerRadius);
				if (stlPoint.Style.DisplayShadow)
				{
					RectangleF rect2 = rect;
					rect2.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
					args.Graph.DrawRect(style.ShadowInterior, null, rect2);
				}
			}
			if (args.Is3D)
			{
				GraphicsPath graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddPolygon(new PointF[4]
				{
					ChartMath.AddPoint(pointF, lineOffset1),
					ChartMath.AddPoint(pointF2, lineOffset1),
					ChartMath.AddPoint(pointF2, lineOffset2),
					ChartMath.AddPoint(pointF, lineOffset2)
				});
				args.Graph.DrawPath(brush, gdipPen, graphicsPath2);
			}
			else
			{
				args.Graph.DrawLine(gdipPen, pointF, pointF2);
			}
			ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
			if (!args.Chart.Style3D)
			{
				args.Graph.DrawPath(brush, gdipPen, graphicsPath);
			}
			else
			{
				chartSeriesPath.AddPrimitive(graphicsPath, null, BrushInfo.Empty);
				ChartSeriesPath chartSeriesPath2 = new ChartSeriesPath();
				chartSeriesPath2.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(stlPoint.Index), "BoxCenter");
				chartSeriesPath2.Draw(args.Graph, "BoxCenter");
				ChartSeriesPath chartSeriesPath3 = new ChartSeriesPath();
				chartSeriesPath3.AddPrimitive(gp, style.GdipPen, GetBrush(stlPoint.Index), "BoxRight");
				chartSeriesPath3.Draw(args.Graph, "BoxRight");
				ChartSeriesPath chartSeriesPath4 = new ChartSeriesPath();
				chartSeriesPath4.AddPrimitive(gp2, style.GdipPen, GetBrush(stlPoint.Index));
				chartSeriesPath4.Draw(args.Graph, "BoxTop");
			}
			if (args.Is3D)
			{
				if (rect.Top < Math.Max(pointF.Y, pointF2.Y) && rect.Top > Math.Min(pointF.Y, pointF2.Y))
				{
					GraphicsPath graphicsPath3 = new GraphicsPath();
					PointF pt = new PointF(pointF.X, rect.Top);
					PointF pt2 = new PointF(pointF2.X, Math.Min(pointF2.Y, pointF.Y));
					graphicsPath3.AddPolygon(new PointF[4]
					{
						ChartMath.AddPoint(pt, lineOffset1),
						ChartMath.AddPoint(pt2, lineOffset1),
						ChartMath.AddPoint(pt2, lineOffset2),
						ChartMath.AddPoint(pt, lineOffset2)
					});
					args.Graph.DrawPath(brush, gdipPen, graphicsPath3);
				}
				else if (isInvertedAxes && rect.Right < Math.Max(pointF.X, pointF2.X) && rect.Right > Math.Min(pointF.X, pointF2.X))
				{
					GraphicsPath graphicsPath4 = new GraphicsPath();
					PointF pt3 = new PointF(rect.Right, pointF.Y);
					PointF pt4 = new PointF(Math.Max(pointF2.X, pointF.X), pointF2.Y);
					graphicsPath4.AddPolygon(new PointF[4]
					{
						ChartMath.AddPoint(pt3, lineOffset1),
						ChartMath.AddPoint(pt4, lineOffset1),
						ChartMath.AddPoint(pt4, lineOffset2),
						ChartMath.AddPoint(pt3, lineOffset2)
					});
					args.Graph.DrawPath(brush, gdipPen, graphicsPath4);
				}
			}
			return pointFromValue;
		}
		return PointF.Empty;
	}

	private void RenderSeries(ChartRenderArgs2D args, ChartStyledPoint styledPoint)
	{
		bool isInvertedAxes = IsInvertedAxes;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool isCylinder = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		SizeF cornerRadius = m_series.ConfigItems.ColumnItem.CornerRadius;
		SizeF thisOffset = GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		SizeF halfOffset = new SizeF(seriesOffset.Width / 2f, seriesOffset.Height / 2f);
		SizeF lineOffset = new SizeF(halfOffset.Width / 2f, halfOffset.Height / 2f);
		SizeF lineOffset2 = new SizeF(lineOffset.Width + halfOffset.Width, lineOffset.Height + halfOffset.Height);
		Pen pen = new Pen(Color.Black);
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		PointF previousPoint = PointF.Empty;
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				previousPoint = DrawPoint(args, array[i], sideBySideInfo, isInvertedAxes, isCylinder, dropSeriesPoints, seriesStyle, cornerRadius, thisOffset, seriesOffset, halfOffset, lineOffset, lineOffset2, pen, previousPoint, clipBounds);
			}
		}
		else
		{
			DrawPoint(args, styledPoint, sideBySideInfo, isInvertedAxes, isCylinder, dropSeriesPoints, seriesStyle, cornerRadius, thisOffset, seriesOffset, halfOffset, lineOffset, lineOffset2, pen, previousPoint, clipBounds);
		}
		pen.Dispose();
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
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
			chartRenderArgs2D.Graph = new ChartGDIGraph(g);
			RenderSeries(chartRenderArgs2D, styledPoint);
		}
	}

	public override void Render(ChartRenderArgs2D args)
	{
		RenderSeries(args, null);
	}

	public override void Render(ChartRenderArgs3D args)
	{
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		base.Chart.Series.IndexOf(m_series);
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool flag = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		bool isInvertedAxes = IsInvertedAxes;
		int num = m_series.PointFormats[ChartYValueUsage.LowValue];
		int num2 = m_series.PointFormats[ChartYValueUsage.HighValue];
		int num3 = m_series.PointFormats[ChartYValueUsage.OpenValue];
		int num4 = m_series.PointFormats[ChartYValueUsage.CloseValue];
		float placeDepth = GetPlaceDepth();
		float num5 = placeDepth + GetSeriesDepth();
		_ = m_series.ActualYAxis.Origin;
		args.Graph.AddPolygon(CreateBoundsPolygon(placeDepth));
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		PointF pointF = PointF.Empty;
		int i = indexRange.From;
		for (int num6 = indexRange.To + 1; i < num6; i++)
		{
			ChartStyledPoint chartStyledPoint = array[i];
			_ = chartStyledPoint.Point;
			ChartStyleInfo style = chartStyledPoint.Style;
			int index = chartStyledPoint.Index;
			BrushInfo brush = GetBrush(index);
			Pen gdipPen = style.GdipPen;
			if (!chartStyledPoint.IsVisible)
			{
				continue;
			}
			PointF point = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[num]);
			if (dropSeriesPoints && !pointF.IsEmpty && ((!isInvertedAxes && Math.Abs(point.X - pointF.X) < 1f) || (isInvertedAxes && Math.Abs(point.Y - pointF.Y) < 1f)))
			{
				continue;
			}
			double num7 = chartStyledPoint.X + sideBySideInfo.Start;
			double num8 = chartStyledPoint.X + sideBySideInfo.End;
			double y = chartStyledPoint.YValues[num];
			double y2 = chartStyledPoint.YValues[num2];
			double y3 = chartStyledPoint.YValues[num3];
			double y4 = chartStyledPoint.YValues[num4];
			PointF point2 = args.GetPoint((num7 + num8) / 2.0, y);
			PointF point3 = args.GetPoint((num7 + num8) / 2.0, y2);
			RectangleF rect = args.GetRectangle(num7, y3, num8, y4);
			CheckColumnBounds(args.IsInvertedAxes, ref rect);
			Vector3D v = new Vector3D(rect.Left, rect.Top, placeDepth);
			Vector3D v2 = new Vector3D(rect.Right, rect.Bottom, num5);
			if (m_series.Rotate)
			{
				if (flag)
				{
					args.Graph.CreateBox(v, v2, (Pen)null, (BrushInfo)null);
					args.Graph.CreateCylinderH(v, v2, POLYGON_SECTORS, gdipPen, brush);
				}
				else
				{
					args.Graph.CreateBox(v, v2, gdipPen, brush);
				}
			}
			else if (flag)
			{
				args.Graph.CreateBoxV(v, v2, null, null);
				args.Graph.CreateCylinderV(v, v2, POLYGON_SECTORS, gdipPen, brush);
			}
			else
			{
				args.Graph.CreateBoxV(v, v2, gdipPen, brush);
			}
			Polygon polygon = new Polygon(new Vector3D[4]
			{
				new Vector3D(point2.X, point2.Y, placeDepth + (num5 - placeDepth) / 3f),
				new Vector3D(point2.X, point2.Y, placeDepth + 2f * (num5 - placeDepth) / 3f),
				new Vector3D(point3.X, point3.Y, placeDepth + 2f * (num5 - placeDepth) / 3f),
				new Vector3D(point3.X, point3.Y, placeDepth + (num5 - placeDepth) / 3f)
			}, brush, gdipPen);
			args.Graph.AddPolygon(polygon);
			pointF = point;
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillRectangle(brush, bounds);
				return;
			}
		}
		if (m_series.Style.HasSymbol && m_series.Style.Symbol.Shape != 0)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			BrushInfo brush2 = new BrushInfo(m_series.Style.Symbol.Color);
			Pen pen = new Pen(m_series.Style.Symbol.Border.Color);
			graphicsPath.AddPath(ChartSymbolHelper.GetPathSymbol(base.SeriesStyle.Symbol.Shape, bounds), connect: false);
			graphicsPath.CloseFigure();
			BrushPaint.FillPath(g, graphicsPath, brush2);
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
