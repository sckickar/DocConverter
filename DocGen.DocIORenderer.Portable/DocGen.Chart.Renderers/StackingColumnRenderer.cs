using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class StackingColumnRenderer : ColumnRenderer
{
	public override ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.OneForAll;

	protected override string RegionDescription => "Stacking Column Chart Region";

	public StackingColumnRenderer(ChartSeries series)
		: base(series)
	{
	}

	private PointF DrawPoint(ChartRenderArgs2D args, ChartStyledPoint stlPoint, DoubleRange sbsInfo, bool dropPoints, bool isCylinder, RectangleF clip, SizeF cornerRadius, PointF previosPoint, ArrayList pathsList)
	{
		ChartStyleInfo style = stlPoint.Style;
		ChartPoint point = stlPoint.Point;
		if (stlPoint.IsVisible)
		{
			PointF point2 = args.GetPoint(point.X, point.YValues[0]);
			if (dropPoints && !previosPoint.IsEmpty && ((!args.IsInvertedAxes && Math.Abs(point2.X - previosPoint.X) < 1f) || (args.IsInvertedAxes && Math.Abs(point2.Y - previosPoint.Y) < 1f)))
			{
				return Point.Empty;
			}
			double x = 0.0;
			double x2 = 0.0;
			double stackInfoValue = GetStackInfoValue(stlPoint.Index, isWithMe: true);
			double stackInfoValue2 = GetStackInfoValue(stlPoint.Index, isWithMe: false);
			CalculateSides(stlPoint, sbsInfo, out x, out x2);
			RectangleF columnBounds = GetColumnBounds(args, stlPoint, x, stackInfoValue, x2, stackInfoValue2);
			if (columnBounds.IntersectsWith(clip))
			{
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
					if (stlPoint.Style.DisplayShadow)
					{
						RectangleF rect = columnBounds;
						rect.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
						args.Graph.DrawRect(style.ShadowInterior, null, rect);
					}
				}
				ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
				if (!args.Chart.Style3D)
				{
					chartSeriesPath.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style));
				}
				else
				{
					chartSeriesPath.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style));
					chartSeriesPath.AddPrimitive(gp, style.GdipPen, GetBrush(style), "BoxRight");
					chartSeriesPath.AddPrimitive(gp2, style.GdipPen, GetBrush(style), "BoxTop");
				}
				chartSeriesPath.Bounds = columnBounds;
				pathsList.Add(chartSeriesPath);
				previosPoint = point2;
			}
			return point2;
		}
		return Point.Empty;
	}

	private void RenderSeries(ChartRenderArgs2D args, ChartStyledPoint styledPoint)
	{
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		bool isCylinder = m_series.ConfigItems.ColumnItem.ColumnType == ChartColumnType.Cylinder;
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		SizeF cornerRadius = m_series.ConfigItems.ColumnItem.CornerRadius;
		PointF previosPoint = PointF.Empty;
		ArrayList arrayList = new ArrayList(m_series.Points.Count);
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			int i = indexRange.From;
			for (int num = indexRange.To + 1; i < num; i++)
			{
				previosPoint = DrawPoint(args, array[i], sideBySideInfo, dropSeriesPoints, isCylinder, clipBounds, cornerRadius, previosPoint, arrayList);
			}
			ChartSegment[] segments = (ChartSeriesPath[])arrayList.ToArray(typeof(ChartSeriesPath));
			m_segments = segments;
		}
		else
		{
			DrawPoint(args, styledPoint, sideBySideInfo, dropSeriesPoints, isCylinder, clipBounds, cornerRadius, previosPoint, arrayList);
			ChartSegment[] segments = (ChartSeriesPath[])arrayList.ToArray(typeof(ChartSeriesPath));
			m_segments = segments;
			segments = m_segments;
			for (int j = 0; j < segments.Length; j++)
			{
				((ChartSeriesPath)segments[j]).Draw(args.Graph);
			}
		}
	}

	public override void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
		if (!base.Chart.Series3D && !base.Chart.RealMode3D)
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
		ChartStyledPoint[] array = PrepearePoints();
		new ArrayList(m_series.Points.Count);
		IndexRange indexRange = CalculateVisibleRange();
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		ChartStyledPoint chartStyledPoint = null;
		for (int i = indexRange.From; i <= indexRange.To; i++)
		{
			chartStyledPoint = array[i];
			if (chartStyledPoint.IsVisible)
			{
				double x = 0.0;
				double x2 = 0.0;
				double stackInfoValue = GetStackInfoValue(chartStyledPoint.Index, isWithMe: true);
				double stackInfoValue2 = GetStackInfoValue(chartStyledPoint.Index, isWithMe: false);
				CalculateSides(chartStyledPoint, sideBySideInfo, out x, out x2);
				RectangleF columnBounds = GetColumnBounds(args, chartStyledPoint, x, stackInfoValue, x2, stackInfoValue2);
				if (args.IsInvertedAxes)
				{
					args.Graph.CreateBox(new Vector3D(columnBounds.Left, columnBounds.Top, args.Z), new Vector3D(columnBounds.Right, columnBounds.Bottom, args.Z + args.Depth), chartStyledPoint.Style.GdipPen, GetBrush(chartStyledPoint.Index));
				}
				else
				{
					args.Graph.CreateBoxV(new Vector3D(columnBounds.Left, columnBounds.Top, args.Z), new Vector3D(columnBounds.Right, columnBounds.Bottom, args.Z + args.Depth), chartStyledPoint.Style.GdipPen, GetBrush(chartStyledPoint.Index));
				}
			}
		}
	}

	[Obsolete]
	public override PointF GetPointByValueForSeries(ChartPoint chpt)
	{
		ChartPoint chartPoint = new ChartPoint(chpt.X, (double[])chpt.YValues.Clone());
		chartPoint.YValues[0] = GetStackInfoValue(m_series.Points.IndexOf(chpt), isWithMe: true);
		return base.GetPointByValueForSeries(chartPoint);
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
