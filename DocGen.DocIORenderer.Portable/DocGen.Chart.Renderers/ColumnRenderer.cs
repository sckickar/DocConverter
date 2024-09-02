using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class ColumnRenderer : ChartSeriesRenderer
{
	public override ChartUsedSpaceType FillSpaceType
	{
		get
		{
			if (base.Chart.ColumnDrawMode != ChartColumnDrawMode.PlaneMode)
			{
				return ChartUsedSpaceType.OneForOne;
			}
			return ChartUsedSpaceType.OneForAll;
		}
	}

	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Column Chart Region";

	protected virtual bool IsFixedWidth => base.Chart.ColumnWidthMode == ChartColumnWidthMode.FixedWidthMode;

	public ColumnRenderer(ChartSeries series)
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

	private void DrawPoint(ChartRenderArgs2D args, ChartStyledPoint styledPoint, DoubleRange sbsInfo, SizeF cornerRadius, bool isCylinder, double origin, ChartErrorBarsConfigItem errorBarConfig, ArrayList pathsList, PointF previosPoint, RectangleF clip)
	{
		if (!styledPoint.IsVisible || styledPoint.YValues[0] == 0.0)
		{
			return;
		}
		double x = 0.0;
		double x2 = 0.0;
		double y = styledPoint.YValues[0];
		CalculateSides(styledPoint, sbsInfo, out x, out x2);
		RectangleF columnBounds = GetColumnBounds(args, styledPoint, x, y, x2, origin);
		if (!columnBounds.IntersectsWith(clip))
		{
			return;
		}
		GraphicsPath graphicsPath = null;
		GraphicsPath gp = null;
		GraphicsPath gp2 = null;
		if (args.Is3D)
		{
			if (isCylinder)
			{
				if (args.IsInvertedAxes)
				{
					if (!args.Chart.Style3D)
					{
						graphicsPath = CreateHorizintalCylinder3D(columnBounds, args.DepthOffset);
					}
					else
					{
						graphicsPath = CreateHorizintalCylinder3D(columnBounds, args.DepthOffset);
						gp2 = CreateHorizintalCylinder3DTop(columnBounds, args.DepthOffset);
					}
				}
				else if (!args.Chart.Style3D)
				{
					graphicsPath = CreateVerticalCylinder3D(columnBounds, args.DepthOffset);
				}
				else
				{
					graphicsPath = CreateVerticalCylinder3D(columnBounds, args.DepthOffset);
					gp2 = CreateVerticalCylinder3DTop(columnBounds, args.DepthOffset);
				}
			}
			else if (!args.Chart.Style3D)
			{
				graphicsPath = CreateBox(columnBounds, is3D: true);
			}
			else
			{
				graphicsPath = CreateBox(columnBounds, is3D: true);
				gp = CreateBoxRight(columnBounds, is3D: true);
				gp2 = CreateBoxTop(columnBounds, is3D: true);
			}
		}
		else
		{
			graphicsPath = RenderingHelper.CreateRoundRect(columnBounds, cornerRadius);
			if (styledPoint.Style.DisplayShadow)
			{
				RectangleF rectangleF = columnBounds;
				rectangleF.Offset(styledPoint.Style.ShadowOffset.Width, styledPoint.Style.ShadowOffset.Height);
				args.Graph.DrawRect(styledPoint.Style.ShadowInterior, null, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
			}
		}
		if (m_series.DrawColumnSeparatingLines)
		{
			PointF point = args.GetPoint(styledPoint.X - 0.5, args.ActualYAxis.Range.Min);
			PointF point2 = args.GetPoint(styledPoint.X - 0.5, args.ActualYAxis.Range.Min);
			PointF point3 = args.GetPoint(styledPoint.X - 0.5, args.ActualYAxis.Range.Max);
			PointF point4 = args.GetPoint(styledPoint.X - 0.5, args.ActualYAxis.Range.Max);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point, point3);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point2, point4);
			graphicsPath.CloseFigure();
		}
		ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
		if (!args.Chart.Style3D)
		{
			chartSeriesPath.AddPrimitive(graphicsPath, styledPoint.Style.GdipPen, GetBrush(styledPoint.Style));
		}
		else
		{
			chartSeriesPath.AddPrimitive(graphicsPath, styledPoint.Style.GdipPen, GetBrush(styledPoint.Style));
			chartSeriesPath.AddPrimitive(gp, styledPoint.Style.GdipPen, GetBrush(styledPoint.Style), "BoxRight");
			chartSeriesPath.AddPrimitive(gp2, styledPoint.Style.GdipPen, GetBrush(styledPoint.Style), "BoxTop");
		}
		chartSeriesPath.Bounds = columnBounds;
		if (args.Is3D)
		{
			pathsList.Add(chartSeriesPath);
		}
		else
		{
			chartSeriesPath.Draw(args.Graph);
		}
	}

	private void RenderSeries(ChartRenderArgs2D args, ChartStyledPoint styledPoint)
	{
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		SizeF cornerRadius = m_series.ConfigItems.ColumnItem.CornerRadius;
		bool isCylinder = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		double currentOrigin = args.ActualYAxis.CurrentOrigin;
		ChartErrorBarsConfigItem errorBars = m_series.ConfigItems.ErrorBars;
		ArrayList arrayList = new ArrayList(m_series.Points.Count);
		PointF empty = PointF.Empty;
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				DrawPoint(args, array[i], sideBySideInfo, cornerRadius, isCylinder, currentOrigin, errorBars, arrayList, empty, clipBounds);
			}
			ChartSegment[] segments = (ChartSeriesPath[])arrayList.ToArray(typeof(ChartSeriesPath));
			m_segments = segments;
		}
		else
		{
			DrawPoint(args, styledPoint, sideBySideInfo, cornerRadius, isCylinder, currentOrigin, errorBars, arrayList, empty, clipBounds);
		}
	}

	public override void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
		if (!base.Chart.Series3D || !base.Chart.RealMode3D)
		{
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
			ChartStyledPoint styledPoint = new ChartStyledPoint(point, info, pointIndex);
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
		_ = args.SeriesIndex;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool flag = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		bool isInvertedAxes = IsInvertedAxes;
		ChartErrorBarsConfigItem errorBars = m_series.ConfigItems.ErrorBars;
		double z = args.Z;
		double z2 = z + args.Depth;
		double origin = m_series.ActualYAxis.Origin;
		args.Graph.AddPolygon(CreateBoundsPolygon((float)z));
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		PointF pointF = PointF.Empty;
		ChartStyledPoint chartStyledPoint = null;
		int i = indexRange.From;
		for (int num = indexRange.To + 1; i < num; i++)
		{
			chartStyledPoint = array[i];
			if (!chartStyledPoint.IsVisible || chartStyledPoint.YValues[0] == 0.0)
			{
				continue;
			}
			ChartPoint point = chartStyledPoint.Point;
			ChartStyleInfo style = chartStyledPoint.Style;
			int index = chartStyledPoint.Index;
			BrushInfo brush = GetBrush(index);
			Pen gdipPen = style.GdipPen;
			PointF pointFromValue = GetPointFromValue(point);
			if (dropSeriesPoints && !pointF.IsEmpty && ((!isInvertedAxes && Math.Abs(pointFromValue.X - pointF.X) < 1f) || (isInvertedAxes && Math.Abs(pointFromValue.Y - pointF.Y) < 1f)))
			{
				continue;
			}
			double x = 0.0;
			double x2 = 0.0;
			double y = chartStyledPoint.YValues[0];
			double y2 = origin;
			CalculateSides(chartStyledPoint, sideBySideInfo, out x, out x2);
			RectangleF columnBounds = GetColumnBounds(args, chartStyledPoint, x, y, x2, y2);
			Vector3D v = new Vector3D(columnBounds.Left, columnBounds.Top, z);
			Vector3D v2 = new Vector3D(columnBounds.Right, columnBounds.Bottom, z2);
			if (args.IsInvertedAxes)
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
			if (m_series.DrawColumnSeparatingLines)
			{
				ChartPoint cpt = new ChartPoint(point.X - 0.5, m_series.ActualYAxis.Range.Min);
				ChartPoint cpt2 = new ChartPoint(point.X - 0.5, m_series.ActualYAxis.Range.Min);
				ChartPoint cpt3 = new ChartPoint(point.X - 0.5, m_series.ActualYAxis.Range.Max);
				ChartPoint cpt4 = new ChartPoint(point.X - 0.5, m_series.ActualYAxis.Range.Max);
				PointF pointFromValue2 = GetPointFromValue(cpt);
				PointF pointFromValue3 = GetPointFromValue(cpt2);
				PointF pointFromValue4 = GetPointFromValue(cpt3);
				PointF pointFromValue5 = GetPointFromValue(cpt4);
				Vector3D[] points = new Vector3D[4]
				{
					new Vector3D(pointFromValue2.X, pointFromValue2.Y, z),
					new Vector3D(pointFromValue4.X, pointFromValue4.Y, z),
					new Vector3D(pointFromValue4.X, pointFromValue4.Y, z2),
					new Vector3D(pointFromValue2.X, pointFromValue2.Y, z2)
				};
				Vector3D[] points2 = new Vector3D[4]
				{
					new Vector3D(pointFromValue3.X, pointFromValue3.Y, z),
					new Vector3D(pointFromValue5.X, pointFromValue5.Y, z),
					new Vector3D(pointFromValue5.X, pointFromValue5.Y, z2),
					new Vector3D(pointFromValue3.X, pointFromValue3.Y, z2)
				};
				args.Graph.AddPolygon(new Polygon(points, brush, gdipPen));
				args.Graph.AddPolygon(new Polygon(points2, brush, gdipPen));
			}
			if (errorBars.Enabled && point.YValues.Length > 1)
			{
				new GraphicsPath();
				SizeF symbolSize = errorBars.SymbolSize;
				PointF point2;
				PointF point3;
				if (errorBars.Orientation == ChartOrientation.Horizontal)
				{
					point2 = args.GetPoint(chartStyledPoint.X + chartStyledPoint.YValues[1], chartStyledPoint.YValues[0]);
					point3 = args.GetPoint(chartStyledPoint.X - chartStyledPoint.YValues[1], chartStyledPoint.YValues[0]);
				}
				else
				{
					point2 = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[0] + chartStyledPoint.YValues[1]);
					point3 = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[0] - chartStyledPoint.YValues[1]);
				}
				RectangleF value = new RectangleF(point2, symbolSize);
				RectangleF value2 = new RectangleF(point3, symbolSize);
				value.X -= value.Width / 2f;
				value.Y -= value.Height / 2f;
				value2.X -= value2.Width / 2f;
				value2.Y -= value2.Height / 2f;
				GraphicsPath pathSymbol = ChartSymbolHelper.GetPathSymbol(errorBars.SymbolShape, Rectangle.Ceiling(value));
				GraphicsPath pathSymbol2 = ChartSymbolHelper.GetPathSymbol(errorBars.SymbolShape, Rectangle.Ceiling(value2));
				GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddLine(point2, point3);
				PathGroup3D pathGroup3D = new PathGroup3D(args.Z);
				pathGroup3D.AddPath(graphicsPath, null, null, gdipPen);
				pathGroup3D.AddPath(pathSymbol, null, brush, gdipPen);
				pathGroup3D.AddPath(pathSymbol2, null, brush, gdipPen);
				args.Graph.AddPolygon(pathGroup3D);
			}
			pointF = pointFromValue;
		}
	}

	protected override void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		RenderErrorBar(g, point);
		base.RenderAdornment(g, point);
	}

	[Obsolete]
	public override PointF GetPointByValueForSeries(ChartPoint chpt)
	{
		CalculateSides(chpt, GetSideBySideInfo(), out var x, out var x2);
		return base.GetPointByValueForSeries(new ChartPoint(x + (x2 - x) / 2.0, chpt.YValues));
	}

	public override DoubleRange GetXDataMeasure()
	{
		DoubleRange xDataMeasure = base.GetXDataMeasure();
		DoubleRange doubleRange = new DoubleRange(0.0, 0.0);
		if (base.Chart.ColumnWidthMode == ChartColumnWidthMode.DefaultWidthMode)
		{
			doubleRange = GetSideBySideInfo();
		}
		return new DoubleRange(xDataMeasure.Start + doubleRange.Start, xDataMeasure.End + doubleRange.End);
	}

	protected void CalculateSides(ChartStyledPoint styledPoint, DoubleRange sbsInfo, out double x1, out double x2)
	{
		if (base.Chart.ColumnWidthMode != ChartColumnWidthMode.FixedWidthMode)
		{
			x1 = styledPoint.X + sbsInfo.Start;
			x2 = styledPoint.X + sbsInfo.End;
			if (base.Chart.ColumnWidthMode == ChartColumnWidthMode.RelativeWidthMode)
			{
				int num = m_series.PointFormats[ChartYValueUsage.PointSizeValue];
				if (styledPoint.YValues.Length > num)
				{
					x1 = styledPoint.X - styledPoint.YValues[num] / 2.0;
					x2 = x1 + styledPoint.YValues[num];
				}
			}
		}
		else
		{
			x1 = (x2 = styledPoint.X);
		}
	}

	protected RectangleF GetColumnBounds(ChartRenderArgs args, ChartStyledPoint stypedPoint, double x1, double y1, double x2, double y2)
	{
		RectangleF rectangle = args.GetRectangle(x1, y1, x2, y2);
		if (IsFixedWidth)
		{
			int num = m_series.PointFormats[ChartYValueUsage.PointSizeValue];
			float num2 = args.Chart.ColumnFixedWidth;
			if (num < stypedPoint.YValues.Length)
			{
				num2 = (float)stypedPoint.YValues[num];
				if (args.IsInvertedAxes)
				{
					rectangle.Inflate(0f, num2);
				}
				else
				{
					rectangle.Inflate(num2, 0f);
				}
			}
			else
			{
				DoubleRange sideBySideInfo = m_series.ChartModel.GetSideBySideInfo(base.ChartArea, m_series, num2);
				if (args.IsInvertedAxes)
				{
					rectangle.Y -= (float)sideBySideInfo.End;
					rectangle.Height = (float)sideBySideInfo.Delta;
				}
				else
				{
					rectangle.X += (float)sideBySideInfo.Start;
					rectangle.Width = (float)sideBySideInfo.Delta;
				}
			}
		}
		return rectangle;
	}

	protected void CheckColumnBounds(bool isInverted, ref RectangleF rect)
	{
		if (IsFixedWidth)
		{
			float num = base.Chart.ColumnFixedWidth;
			if (isInverted)
			{
				rect.Y += 0.5f * (rect.Height - num);
				rect.Height = num;
			}
			else
			{
				rect.X += 0.5f * (rect.Width - num);
				rect.Width = num;
			}
		}
	}

	protected void CheckColumnPoints(bool isInverted, ref RectangleF rect)
	{
		if (IsFixedWidth)
		{
			float num = base.Chart.ColumnFixedWidth;
			if (isInverted)
			{
				rect.Y += 0.5f * (rect.Height - num);
				rect.Height = num;
			}
			else
			{
				rect.X += 0.5f * (rect.Width - num);
				rect.Width = num;
			}
		}
	}

	protected void CalculateSides(ChartPoint cpt, DoubleRange sbsInfo, out double x1, out double x2)
	{
		x1 = cpt.X + sbsInfo.Start;
		x2 = cpt.X + sbsInfo.End;
		if (cpt.YValues.Length > 1 && base.Chart.ColumnWidthMode == ChartColumnWidthMode.RelativeWidthMode)
		{
			x1 = cpt.X;
			x2 = x1 + cpt.YValues[1];
		}
		else if (cpt.YValues.Length > 1 && base.Chart.ColumnWidthMode == ChartColumnWidthMode.FixedWidthMode)
		{
			x1 = cpt.X;
			x2 = x1 + cpt.YValues[1] / (double)base.DividedIntervalSpace.Width;
		}
	}

	protected override PointF GetSymbolCoordinates(ChartStyledPoint point)
	{
		double num = point.X;
		double num2 = point.YValues[0];
		if (m_series.BaseType == ChartSeriesBaseType.SideBySide && !IsFixedWidth)
		{
			num += GetSideBySideInfo().Median;
		}
		if (m_series.BaseStackingType == ChartSeriesBaseStackingType.Stacked)
		{
			num2 += GetStackInfoValue(point.Index);
		}
		else if (m_series.BaseStackingType == ChartSeriesBaseStackingType.FullStacked)
		{
			num2 = GetStackInfoValue(point.Index, isWithMe: true);
		}
		PointF result = ((m_series.Type == ChartSeriesType.Tornado && point.YValues.Length > 1) ? GetPointFromValue(num, point.YValues[1]) : ((m_series.Type != ChartSeriesType.ColumnRange || !m_series.Rotate) ? GetPointFromValue(num, num2) : GetPointFromValue(num, 1.0)));
		if (IsFixedWidth)
		{
			int num3 = m_series.PointFormats[ChartYValueUsage.PointSizeValue];
			float num4 = base.Chart.ColumnFixedWidth;
			if (num3 >= point.YValues.Length)
			{
				DoubleRange sideBySideInfo = m_series.ChartModel.GetSideBySideInfo(base.ChartArea, m_series, num4);
				if (IsInvertedAxes)
				{
					result.Y -= (float)sideBySideInfo.Median;
				}
				else
				{
					result.X += (float)sideBySideInfo.Median;
				}
			}
		}
		Size offset = point.Style.Symbol.Offset;
		result.X += offset.Width;
		result.Y += offset.Height;
		return result;
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
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillRectangle(brush, bounds);
				return;
			}
		}
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
