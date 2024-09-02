using System;
using System.Collections;
using System.ComponentModel;
using SkiaSharp;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class LineRenderer : ChartSeriesRenderer
{
	private DoubleRange m_xRange = DoubleRange.Empty;

	private DoubleRange m_yRange = DoubleRange.Empty;

	protected override string RegionDescription => "Line Chart Renderer";

	protected override int RequireYValuesCount => 1;

	protected override bool ShouldSort => m_series.SortPoints;

	public LineRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		_ = base.Chart.Series3D;
		if (!IsInvertedAxes)
		{
			_ = base.XAxis.Inversed;
		}
		else
		{
			_ = base.YAxis.Inversed;
		}
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		int num = args.Series.PointFormats[ChartYValueUsage.YValue];
		_ = args.Series.ConfigItems.LineSegment;
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		_ = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		ChartStyledPoint[] styledPoints = base.StyledPoints;
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint chartStyledPoint = null;
		ChartStyledPoint chartStyledPoint2 = null;
		PointF pointF = PointF.Empty;
		PointF empty = PointF.Empty;
		new Pen(seriesStyle.GdipPen.Color, seriesStyle.GdipPen.Width);
		int num2 = (args.ActualXAxis.Inversed ? indexRange.To : indexRange.From);
		int num3 = (args.ActualXAxis.Inversed ? (indexRange.From - 1) : (indexRange.To + 1));
		int num4 = ((!args.ActualXAxis.Inversed) ? 1 : (-1));
		new ArrayList();
		for (int i = num2; i != num3; i += num4)
		{
			chartStyledPoint2 = styledPoints[i];
			if (chartStyledPoint2.IsVisible)
			{
				empty = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
				if ((dropSeriesPoints || !pointF.IsEmpty) && !((double)Math.Abs(pointF.X - empty.X) > args.Series.Resolution) && !((double)Math.Abs(empty.Y - pointF.Y) > args.Series.Resolution))
				{
					continue;
				}
				if (chartStyledPoint != null)
				{
					DoubleRange xRange = new DoubleRange(chartStyledPoint.X, chartStyledPoint2.X);
					DoubleRange yRange = new DoubleRange(chartStyledPoint.YValues[num], chartStyledPoint2.YValues[num]);
					if (args.IsVisible(xRange, yRange))
					{
						if (args.Is3D)
						{
							GraphicsPath graphicsPath = new GraphicsPath();
							graphicsPath.AddLine(pointF, ChartMath.AddPoint(pointF, seriesOffset));
							graphicsPath.AddLine(ChartMath.AddPoint(empty, seriesOffset), empty);
							graphicsPath.CloseFigure();
							using Pen pen = chartStyledPoint.Style.GdipPen.Clone() as Pen;
							if (m_series.ConfigItems.LineItem.DisableLineCap)
							{
								pen.StartCap = LineCap.Flat;
								pen.EndCap = LineCap.Flat;
								pen.LineJoin = LineJoin.Bevel;
							}
							args.Graph.DrawPath(GetBrush(chartStyledPoint.Index), pen, graphicsPath);
						}
						else
						{
							using Pen pen2 = chartStyledPoint2.Style.GdipPen.Clone() as Pen;
							if (m_series.ConfigItems.LineItem.DisableLineCap)
							{
								pen2.StartCap = LineCap.Flat;
								pen2.EndCap = LineCap.Flat;
							}
							if (chartStyledPoint.Style.DisplayShadow)
							{
								Size shadowOffset = chartStyledPoint.Style.ShadowOffset;
								PointF pointF2 = ChartMath.AddPoint(pointF, shadowOffset);
								PointF pointF3 = ChartMath.AddPoint(empty, shadowOffset);
								pen2.Color = chartStyledPoint.Style.ShadowInterior.BackColor;
								args.Graph.DrawLine(pen2, pointF2.X, pointF2.Y, pointF3.X, pointF3.Y);
							}
							pen2.SKPaint.PathEffect = m_series.GetDashStyle(pen2, seriesStyle.GdipPen.Width);
							SKStrokeCap capStyle = m_series.GetCapStyle(pen2);
							if (capStyle != 0)
							{
								pen2.SKPaint.StrokeCap = capStyle;
							}
							args.Graph.DrawLine(pen2, pointF.X, pointF.Y, empty.X, empty.Y);
						}
					}
				}
				pointF = empty;
				chartStyledPoint = chartStyledPoint2;
			}
			else if (base.Chart.AllowGapForEmptyPoints)
			{
				chartStyledPoint = null;
			}
		}
		if (!args.Is3D)
		{
			GraphicsState state = (args.Graph as ChartGDIGraph).Graphics.Save();
			DrawArrows((args.Graph as ChartGDIGraph).Graphics);
			(args.Graph as ChartGDIGraph).Graphics.Restore(state);
		}
	}

	public override void Render(ChartRenderArgs3D args)
	{
		_ = base.Chart.DropSeriesPoints;
		_ = IsInvertedAxes;
		_ = args.Series.ConfigItems.LineSegment;
		base.Chart.Series.IndexOf(m_series);
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num2 = placeDepth + seriesDepth;
		_ = base.SeriesStyle;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		ChartErrorBarsConfigItem errorBars = m_series.ConfigItems.ErrorBars;
		ChartStyledPoint chartStyledPoint = null;
		ChartStyledPoint chartStyledPoint2 = null;
		PointF pointF = PointF.Empty;
		PointF empty = PointF.Empty;
		args.Graph.AddPolygon(CreateBoundsPolygon(placeDepth));
		ArrayList arrayList = new ArrayList(array.Length);
		int i = indexRange.From;
		for (int num3 = indexRange.To + 1; i != num3; i++)
		{
			chartStyledPoint2 = array[i];
			if (!chartStyledPoint2.Point.IsEmpty)
			{
				empty = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
				if (chartStyledPoint != null)
				{
					double tan = (empty.Y - pointF.Y) / (empty.X - pointF.X);
					Polygon p = new Polygon(new Vector3D[4]
					{
						new Vector3D(empty.X, empty.Y, placeDepth),
						new Vector3D(pointF.X, pointF.Y, placeDepth),
						new Vector3D(pointF.X, pointF.Y, num2),
						new Vector3D(empty.X, empty.Y, num2)
					}, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
					arrayList.Add(new PolygonWithTangent(p, tan));
				}
				if (errorBars.Enabled && chartStyledPoint2.YValues.Length > 1)
				{
					BrushInfo brush = GetBrush(chartStyledPoint2.Index);
					new GraphicsPath();
					SizeF symbolSize = errorBars.SymbolSize;
					PointF point;
					PointF point2;
					if (errorBars.Orientation == ChartOrientation.Horizontal)
					{
						point = args.GetPoint(chartStyledPoint2.X + chartStyledPoint2.YValues[1], chartStyledPoint2.YValues[0]);
						point2 = args.GetPoint(chartStyledPoint2.X - chartStyledPoint2.YValues[1], chartStyledPoint2.YValues[0]);
					}
					else
					{
						point = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[0] + chartStyledPoint2.YValues[1]);
						point2 = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[0] - chartStyledPoint2.YValues[1]);
					}
					RectangleF value = new RectangleF(point, symbolSize);
					RectangleF value2 = new RectangleF(point2, symbolSize);
					value.X -= value.Width / 2f;
					value.Y -= value.Height / 2f;
					value2.X -= value2.Width / 2f;
					value2.Y -= value2.Height / 2f;
					GraphicsPath pathSymbol = ChartSymbolHelper.GetPathSymbol(errorBars.SymbolShape, Rectangle.Ceiling(value));
					GraphicsPath pathSymbol2 = ChartSymbolHelper.GetPathSymbol(errorBars.SymbolShape, Rectangle.Ceiling(value2));
					GraphicsPath graphicsPath = new GraphicsPath();
					graphicsPath.AddLine(point, point2);
					PathGroup3D pathGroup3D = new PathGroup3D(args.Z);
					pathGroup3D.AddPath(graphicsPath, null, null, chartStyledPoint2.Style.GdipPen);
					pathGroup3D.AddPath(pathSymbol, null, brush, chartStyledPoint2.Style.GdipPen);
					pathGroup3D.AddPath(pathSymbol2, null, brush, chartStyledPoint2.Style.GdipPen);
					args.Graph.AddPolygon(pathGroup3D);
				}
				pointF = empty;
				chartStyledPoint = chartStyledPoint2;
			}
			else if (base.Chart.AllowGapForEmptyPoints)
			{
				chartStyledPoint = null;
			}
		}
		arrayList.Sort(new PolygonWithTangentComparer());
		for (int j = 0; j < arrayList.Count; j++)
		{
			args.Graph.AddPolygon(((PolygonWithTangent)arrayList[j]).Polygon);
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

	internal override void DataUpdate(ListChangedEventArgs args)
	{
		m_xRange = DoubleRange.Empty;
		m_yRange = DoubleRange.Empty;
		base.DataUpdate(args);
	}

	internal override void Update(ChartUpdateFlags flags)
	{
		if ((flags & ChartUpdateFlags.Indexed) == ChartUpdateFlags.Indexed)
		{
			m_xRange = DoubleRange.Empty;
			m_yRange = DoubleRange.Empty;
		}
		base.Update(flags);
	}

	public override DoubleRange GetXDataMeasure()
	{
		if (m_xRange.IsEmpty)
		{
			m_xRange = base.GetXDataMeasure();
		}
		return m_xRange;
	}

	public override DoubleRange GetYDataMeasure()
	{
		if (m_yRange.IsEmpty)
		{
			m_yRange = base.GetYDataMeasure();
		}
		return m_yRange;
	}
}
