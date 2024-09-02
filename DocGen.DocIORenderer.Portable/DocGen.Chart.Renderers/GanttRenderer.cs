using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class GanttRenderer : BarRenderer
{
	protected override int RequireYValuesCount => 2;

	protected override string RegionDescription => "Gantt Chart Region";

	public GanttRenderer(ChartSeries series)
		: base(series)
	{
	}

	private void DrawPoint(Graphics g, ChartStyledPoint styledPoint, DoubleRange sbsInfo, ChartGanttConfigItem configItem, RectangleF clip, bool isIndexed, int serIndex, bool series3D, SizeF offset, PointF offsetSeries, int i)
	{
		_ = styledPoint.Point;
		if (!styledPoint.IsVisible)
		{
			return;
		}
		ChartStyleInfo style = styledPoint.Style;
		double x = styledPoint.Point.X;
		double x2 = styledPoint.Point.X;
		double num = styledPoint.Point.YValues[0];
		double num2 = styledPoint.Point.YValues[1];
		if (configItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
		{
			DoubleRange doubleRange = DoubleRange.Scale(sbsInfo, style.PointWidth);
			x += doubleRange.Start;
			x2 += doubleRange.End;
		}
		else
		{
			x += sbsInfo.Start;
			x2 += sbsInfo.End;
		}
		RectangleF rectangle = GetRectangle(new ChartPoint(x, num), new ChartPoint(x2, num2));
		if (isIndexed)
		{
			double x3;
			double x4 = (x3 = styledPoint.X);
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
			if (configItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
			{
				DoubleRange doubleRange2 = DoubleRange.Scale(sbsInfo, style.PointWidth);
				x4 += doubleRange2.Start;
				x3 += doubleRange2.End;
			}
			else
			{
				CalculateSides(styledPoint, sbsInfo, out x4, out x3);
			}
			rectangle = chartRenderArgs2D.GetRectangle(x4, num, x3, num2);
		}
		if (series3D)
		{
			Draw3DRectangle(g, rectangle, offset, GetBrush(style), style.GdipPen);
		}
		else
		{
			if (style.DisplayShadow)
			{
				RectangleF r = rectangle;
				r.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
				BrushPaint.FillRectangle(g, r, style.ShadowInterior);
			}
			BrushPaint.FillRectangle(g, rectangle, GetBrush(style));
			g.DrawRectangle(style.GdipPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
		if (style.RelatedPoints.Points == null)
		{
			return;
		}
		for (int j = 0; j < style.RelatedPoints.Count; j++)
		{
			PointF pointFromIndex = GetPointFromIndex(style.RelatedPoints.Points[j]);
			PointF[] connectionLine = GetConnectionLine(GetPointFromIndex(i, 1), pointFromIndex, base.DividedIntervalSpace.Height / 2f);
			g.DrawLines(style.RelatedPoints.GdipPen, connectionLine);
			if (style.RelatedPoints.StartSymbol != null)
			{
				RenderingHelper.DrawRelatedPointSymbol(g, style.RelatedPoints.StartSymbol, style.RelatedPoints.Border, style.Images, connectionLine[0]);
			}
			if (style.RelatedPoints.EndSymbol != null)
			{
				RenderingHelper.DrawRelatedPointSymbol(g, style.RelatedPoints.EndSymbol, style.RelatedPoints.Border, style.Images, connectionLine[^1]);
			}
		}
	}

	private void RenderSeries(Graphics g, ChartStyledPoint styledPoint)
	{
		ChartGanttConfigItem ganttItem = m_series.ConfigItems.GanttItem;
		DoubleRange empty = DoubleRange.Empty;
		RectangleF clipBounds = g.ClipBounds;
		bool indexed = base.Chart.Indexed;
		if (ganttItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
		{
			empty = new DoubleRange(0.0, GetMinPointsDelta());
			empty = DoubleRange.Offset(empty, (0.0 - empty.Delta) / 2.0);
			empty = DoubleRange.Scale(empty, 0.01 * (double)(100f - base.Chart.Spacing));
		}
		else
		{
			empty = GetSideBySideRange();
		}
		int serIndex = base.Chart.Series.IndexOf(m_series);
		bool series3D = base.Chart.Series3D;
		SizeF seriesOffset = GetSeriesOffset();
		PointF offsetSeries = GetThisOffset().ToPointF();
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			int i = 0;
			int num = array.Length;
			int num2 = 1;
			if (base.YAxis.Inversed)
			{
				num2 = -1;
				i = num - 1;
				num = -1;
			}
			for (; i != num; i += num2)
			{
				DrawPoint(g, array[i], empty, ganttItem, clipBounds, indexed, serIndex, series3D, seriesOffset, offsetSeries, i);
			}
		}
		else
		{
			int i2 = 0;
			int num3 = 1;
			if (base.YAxis.Inversed)
			{
				i2 = num3 - 1;
			}
			DrawPoint(g, styledPoint, empty, ganttItem, clipBounds, indexed, serIndex, series3D, seriesOffset, offsetSeries, i2);
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
		ChartGanttConfigItem ganttItem = m_series.ConfigItems.GanttItem;
		DoubleRange empty = DoubleRange.Empty;
		if (ganttItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
		{
			empty = new DoubleRange(0.0, GetMinPointsDelta());
			empty = DoubleRange.Offset(empty, (0.0 - empty.Delta) / 2.0);
			empty = DoubleRange.Scale(empty, 0.01 * (double)(100f - base.Chart.Spacing));
		}
		else
		{
			empty = GetSideBySideRange();
		}
		base.Chart.Series.IndexOf(m_series);
		ChartStyledPoint[] array = PrepearePoints();
		int i = 0;
		int num = array.Length;
		int num2 = 1;
		float placeDepth = GetPlaceDepth();
		float num3 = GetSeriesDepth() + placeDepth;
		if (base.YAxis.Inversed)
		{
			num2 = -1;
			i = num - 1;
			num = -1;
		}
		ChartStyledPoint chartStyledPoint = null;
		for (; i != num; i += num2)
		{
			chartStyledPoint = array[i];
			ChartPoint cpt = m_series.Points[i];
			if (!IsVisiblePoint(cpt))
			{
				continue;
			}
			ChartStyleInfo style = chartStyledPoint.Style;
			double x = chartStyledPoint.Point.X;
			double x2 = chartStyledPoint.Point.X;
			double num4 = chartStyledPoint.Point.YValues[0];
			double num5 = chartStyledPoint.Point.YValues[1];
			if (ganttItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
			{
				DoubleRange doubleRange = DoubleRange.Scale(empty, -0.5 * (double)style.PointWidth);
				x += doubleRange.Start;
				x2 += doubleRange.End;
			}
			else
			{
				x += empty.Start;
				x2 += empty.End;
			}
			RectangleF rectangle = GetRectangle(new ChartPoint(x, num4), new ChartPoint(x2, num5));
			if (base.Chart.Indexed)
			{
				double x3;
				double x4 = (x3 = chartStyledPoint.X);
				ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
				if (ganttItem.DrawMode == ChartGanttDrawMode.CustomPointWidthMode)
				{
					DoubleRange doubleRange2 = DoubleRange.Scale(empty, style.PointWidth);
					x4 += doubleRange2.Start;
					x3 += doubleRange2.End;
				}
				else
				{
					CalculateSides(chartStyledPoint, empty, out x4, out x3);
				}
				rectangle = chartRenderArgs2D.GetRectangle(x4, num4, x3, num5);
			}
			g.CreateBox(new Vector3D(rectangle.Left, rectangle.Top, placeDepth), new Vector3D(rectangle.Right, rectangle.Bottom, num3), style.GdipPen, GetBrush(i));
		}
	}

	private PointF[] GetConnectionLine(PointF from, PointF to, float offset)
	{
		return new PointF[6]
		{
			from,
			new PointF(from.X + 10f, from.Y),
			new PointF(from.X + 10f, to.Y - ((to.Y > from.Y) ? offset : (0f - offset))),
			new PointF(to.X - 10f, to.Y - ((to.Y > from.Y) ? offset : (0f - offset))),
			new PointF(to.X - 10f, to.Y),
			to
		};
	}
}
