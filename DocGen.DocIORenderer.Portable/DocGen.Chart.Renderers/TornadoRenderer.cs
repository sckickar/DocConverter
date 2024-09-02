using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class TornadoRenderer : BarRenderer
{
	public override ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.OneForAll;

	protected override int RequireYValuesCount => 2;

	protected override string RegionDescription => "Tornado Chart Region";

	public TornadoRenderer(ChartSeries series)
		: base(series)
	{
	}

	private void DrawPoint(ChartRenderArgs2D args, ChartStyledPoint styledPoint, DoubleRange sbsInfo, int lowIndex, int highIndex, RectangleF clip, ArrayList pathsList)
	{
		if (!styledPoint.IsVisible)
		{
			return;
		}
		double x = styledPoint.X + sbsInfo.Start;
		double x2 = styledPoint.X + sbsInfo.End;
		double y = styledPoint.YValues[lowIndex];
		double y2 = styledPoint.YValues[highIndex];
		PointF point = args.GetPoint(x, y);
		PointF point2 = args.GetPoint(x2, y2);
		RectangleF rectangleF = ChartMath.CorrectRect(point.X, point.Y, point2.X, point2.Y);
		ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
		chartSeriesPath.Bounds = rectangleF;
		if (args.Is3D)
		{
			chartSeriesPath.AddPrimitive(CreateBox(rectangleF, is3D: true), styledPoint.Style.GdipPen, GetBrush(styledPoint.Style));
		}
		else
		{
			if (styledPoint.Style.DisplayShadow)
			{
				RectangleF rect = rectangleF;
				rect.Offset(styledPoint.Style.ShadowOffset.Width, styledPoint.Style.ShadowOffset.Height);
				args.Graph.DrawRect(styledPoint.Style.ShadowInterior, null, rect);
			}
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(rectangleF);
			chartSeriesPath.AddPrimitive(graphicsPath, styledPoint.Style.GdipPen, GetBrush(styledPoint.Style));
		}
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
		int lowIndex = args.Series.PointFormats[ChartYValueUsage.LowValue];
		int highIndex = args.Series.PointFormats[ChartYValueUsage.HighValue];
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		ArrayList arrayList = new ArrayList(m_series.Points.Count);
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			for (int i = indexRange.From; i <= indexRange.To; i++)
			{
				DrawPoint(args, array[i], sideBySideInfo, lowIndex, highIndex, clipBounds, arrayList);
			}
			if (args.Is3D)
			{
				ChartSegment[] segments = (ChartSeriesPath[])arrayList.ToArray(typeof(ChartSeriesPath));
				m_segments = segments;
			}
			else
			{
				m_segments = null;
			}
		}
		else
		{
			DrawPoint(args, styledPoint, sideBySideInfo, lowIndex, highIndex, clipBounds, arrayList);
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
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		int num = args.Series.PointFormats[ChartYValueUsage.LowValue];
		int num2 = args.Series.PointFormats[ChartYValueUsage.HighValue];
		double z = args.Z;
		double z2 = args.Z + args.Depth;
		ChartStyledPoint chartStyledPoint = null;
		for (int i = indexRange.From; i <= indexRange.To; i++)
		{
			chartStyledPoint = array[i];
			if (chartStyledPoint.IsVisible)
			{
				double x = chartStyledPoint.X + sideBySideInfo.Start;
				double x2 = chartStyledPoint.X + sideBySideInfo.End;
				double y = chartStyledPoint.YValues[num];
				double y2 = chartStyledPoint.YValues[num2];
				PointF point = args.GetPoint(x, y);
				PointF point2 = args.GetPoint(x2, y2);
				args.Graph.CreateBox(new Vector3D(Math.Min(point.X, point2.X), Math.Min(point.Y, point2.Y), z), new Vector3D(Math.Max(point.X, point2.X), Math.Max(point.Y, point2.Y), z2), chartStyledPoint.Style.GdipPen, GetBrush(chartStyledPoint.Index));
			}
		}
	}
}
