using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class BubbleRenderer : ChartSeriesRenderer
{
	private const float BLINK_RANGE = 0.3f;

	private const float BLINK_MIN = 0.4f;

	private const float LIGHT_ANGLE = (float)Math.PI / 4f;

	private const float c_minimalScaleFactor = 0.1f;

	private static readonly ColorBlend c_blinkColorBlend;

	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Bubble Chart Region";

	protected override bool IgnoreSeriesInversion => true;

	static BubbleRenderer()
	{
		c_blinkColorBlend = new ColorBlend
		{
			Positions = new float[3] { 0f, 0.7f, 1f },
			Colors = new Color[3]
			{
				Color.FromArgb(144, Color.Black),
				Color.Transparent,
				Color.FromArgb(128, Color.White)
			}
		};
	}

	public BubbleRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		RenderSeries(args, null);
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		_ = base.Chart.DropSeriesPoints;
		ChartBubbleType bubbleType = m_series.ConfigItems.BubbleItem.BubbleType;
		SizeF size = m_series.ConfigItems.BubbleItem.MinBounds.Size;
		SizeF size2 = m_series.ConfigItems.BubbleItem.MaxBounds.Size;
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = PrepearePoints();
		DoubleRange doubleRange = CalculateYRange(1);
		float seriesDepth = GetSeriesDepth();
		float num = GetPlaceDepth() + seriesDepth / 2f;
		ChartStyledPoint chartStyledPoint = null;
		int i = indexRange.From;
		for (int num2 = indexRange.To + 1; i < num2; i++)
		{
			chartStyledPoint = array[i];
			if (!IsVisiblePoint(chartStyledPoint.Point))
			{
				continue;
			}
			float num3 = 1f;
			if (chartStyledPoint.Point.YValues.Length > 1)
			{
				num3 = 0.1f;
				if (doubleRange.Delta != 0.0)
				{
					num3 += 0.9f * (float)((chartStyledPoint.Point.YValues[1] - doubleRange.Start) / doubleRange.Delta);
				}
			}
			PointF pointFromValue = GetPointFromValue(chartStyledPoint.Point);
			SizeF sz = new SizeF(size.Width + (size2.Width - size.Width) * num3, size.Height + (size2.Height - size.Height) * num3);
			RectangleF bounds = new RectangleF(pointFromValue.X - sz.Width / 2f, pointFromValue.Y - sz.Height / 2f, sz.Width, sz.Height);
			BrushInfo brushInfo = GetBrush(chartStyledPoint.Index);
			if (brushInfo.Style == BrushStyle.Solid && base.Chart.Model.ColorModel.AllowGradient)
			{
				brushInfo = new BrushInfo(GradientStyle.PathEllipse, ControlPaintExtension.Light(brushInfo.BackColor), brushInfo.BackColor);
			}
			brushInfo = (m_series.ConfigItems.BubbleItem.EnablePhongStyle ? brushInfo : new BrushInfo(Color.White));
			switch (bubbleType)
			{
			case ChartBubbleType.Circle:
				g.CreateEllipse(new Vector3D(bounds.X, bounds.Y, num), sz, 18, chartStyledPoint.Style.GdipPen, brushInfo);
				break;
			case ChartBubbleType.Square:
				g.CreateRectangle(new Vector3D(bounds.X, bounds.Y, num), sz, chartStyledPoint.Style.GdipPen, brushInfo);
				break;
			case ChartBubbleType.Image:
				if (chartStyledPoint.Style.ImageIndex >= 0 && chartStyledPoint.Style.ImageIndex < chartStyledPoint.Style.Images.Count)
				{
					Image3D image3D = Image3D.FromImage(chartStyledPoint.Style.Images[chartStyledPoint.Style.ImageIndex], bounds, num);
					g.AddPolygon(image3D);
					(new Polygon[1])[0] = image3D;
				}
				break;
			}
		}
	}

	public override void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
		if (!base.Chart.Series3D || !base.Chart.RealMode3D)
		{
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
			chartRenderArgs2D.Graph = new ChartGDIGraph(g);
			ChartStyledPoint chartStyledPoint = new ChartStyledPoint(point, info, pointIndex);
			if (chartStyledPoint.YValues.Length > 2)
			{
				chartStyledPoint.YValues[2] += 2.0;
			}
			RenderSeries(chartRenderArgs2D, chartStyledPoint);
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		ChartBubbleConfigItem bubbleItem = m_series.ConfigItems.BubbleItem;
		if (bubbleItem.BubbleType == ChartBubbleType.Circle)
		{
			if (isShadow)
			{
				using (SolidBrush brush = new SolidBrush(shadowColor))
				{
					g.FillEllipse(brush, bounds);
					return;
				}
			}
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(bounds);
			BrushPaint.FillPath(g, graphicsPath, m_series.Style.Interior);
			g.DrawEllipse(base.SeriesStyle.GdipPen, bounds);
		}
		else
		{
			base.DrawIcon(g, bounds, isShadow, shadowColor);
			if (!isShadow && bubbleItem.BubbleType == ChartBubbleType.Image && base.SeriesStyle.Images != null && base.SeriesStyle.ImageIndex > -1)
			{
				g.DrawImage(base.SeriesStyle.Images[base.SeriesStyle.ImageIndex], bounds);
			}
		}
	}

	private void DrawPoint(ChartRenderArgs2D args, ChartStyledPoint styledPoint, int sizeValueIndex, DoubleRange bubbleYRange, ChartBubbleType bubbleType, RectangleF clip, bool enablePhongStyle, bool is3D, SizeF maxSize, SizeF minSize, int serIndex, int yValueIndex, double maximumSize)
	{
		if (!styledPoint.IsVisible)
		{
			return;
		}
		float num = 1f;
		bool flag = false;
		float num2 = maxSize.Width * (float)(styledPoint.YValues[1] / maximumSize) * 2f;
		float width = (maxSize.Height = num2);
		maxSize.Width = width;
		PointF point = args.GetPoint(styledPoint.X, styledPoint.YValues[yValueIndex]);
		SizeF sizeF = new SizeF(minSize.Width + num * (maxSize.Width - minSize.Width), minSize.Height + num * (maxSize.Height - minSize.Height));
		RectangleF rectangleF = new RectangleF(point.X - sizeF.Width / 2f, point.Y - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
		GraphicsPath graphicsPath = new GraphicsPath();
		switch (bubbleType)
		{
		case ChartBubbleType.Circle:
			graphicsPath.AddEllipse(rectangleF);
			break;
		case ChartBubbleType.Square:
			graphicsPath.AddRectangle(rectangleF);
			break;
		case ChartBubbleType.Image:
			graphicsPath.AddRectangle(rectangleF);
			flag = true;
			break;
		}
		if (flag)
		{
			if (styledPoint.Style.ImageIndex >= 0 && styledPoint.Style.ImageIndex < styledPoint.Style.Images.Count)
			{
				args.Graph.DrawImage(styledPoint.Style.Images[styledPoint.Style.ImageIndex], rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
			}
			return;
		}
		if (!is3D && styledPoint.Style.DisplayShadow)
		{
			args.Graph.PushTransform();
			args.Graph.Translate(styledPoint.Style.ShadowOffset);
			args.Graph.DrawPath(styledPoint.Style.ShadowInterior, null, graphicsPath);
			args.Graph.PopTransform();
		}
		BrushInfo brushInfo = GetBrush(styledPoint.Index);
		if (base.Chart.Model.ColorModel.AllowGradient && enablePhongStyle)
		{
			switch (bubbleType)
			{
			case ChartBubbleType.Circle:
				args.Graph.DrawPath(brushInfo, null, graphicsPath);
				if (brushInfo.Style == BrushStyle.Solid)
				{
					using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
					{
						float num4 = (is3D ? ((float)(Math.Cos(0.7853981852531433) * Math.Sin(Math.PI / 180.0 * (double)base.ChartArea.Rotation))) : 0f);
						float num5 = (is3D ? ((float)(Math.Sin(0.7853981852531433) * Math.Sin(Math.PI / 180.0 * (double)base.ChartArea.Tilt))) : 0f);
						pathGradientBrush.InterpolationColors = c_blinkColorBlend;
						pathGradientBrush.CenterPoint = new PointF(rectangleF.X + (0.4f - 0.3f * num4) * sizeF.Width, rectangleF.Y + (0.4f - 0.3f * num5) * sizeF.Height);
						pathGradientBrush.SurroundColors = new Color[1] { Color.Transparent };
						pathGradientBrush.CenterColor = Color.Transparent;
						args.Graph.DrawPath(pathGradientBrush, styledPoint.Style.GdipPen, graphicsPath);
						break;
					}
				}
				break;
			case ChartBubbleType.Square:
				if (brushInfo.Style == BrushStyle.Solid)
				{
					ChartSeriesRenderer.PhongShadingColors(brushInfo.BackColor, brushInfo.BackColor, Color.White, -Math.PI / 4.0, 20.0, out var colors, out var _);
					brushInfo = new BrushInfo(GradientStyle.Horizontal, new BrushInfoColorArrayList(colors));
				}
				args.Graph.DrawPath(brushInfo, styledPoint.Style.GdipPen, graphicsPath);
				break;
			}
		}
		else if (!sizeF.IsEmpty)
		{
			if (enablePhongStyle)
			{
				args.Graph.DrawPath(brushInfo, styledPoint.Style.GdipPen, graphicsPath);
			}
			else
			{
				args.Graph.DrawPath(styledPoint.Style.GdipPen, graphicsPath);
			}
		}
	}

	private DoubleRange CalculateYRange(int index)
	{
		DoubleRange doubleRange = DoubleRange.Empty;
		foreach (ChartPoint point in m_series.Points)
		{
			if (!point.IsEmpty && point.YValues.Length > index)
			{
				doubleRange = DoubleRange.Union(doubleRange, point.YValues[index]);
			}
		}
		return doubleRange;
	}

	private void RenderSeries(ChartRenderArgs2D args, ChartStyledPoint styledPoint)
	{
		int seriesIndex = args.SeriesIndex;
		bool is3D = args.Is3D;
		int yValueIndex = args.Series.PointFormats[ChartYValueUsage.YValue];
		int num = args.Series.PointFormats[ChartYValueUsage.PointSizeValue];
		ChartBubbleType bubbleType = m_series.ConfigItems.BubbleItem.BubbleType;
		SizeF size = m_series.ConfigItems.BubbleItem.MinBounds.Size;
		SizeF size2 = m_series.ConfigItems.BubbleItem.MaxBounds.Size;
		bool enablePhongStyle = m_series.ConfigItems.BubbleItem.EnablePhongStyle;
		DoubleRange bubbleYRange = CalculateYRange(num);
		GetThisOffset();
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		if (styledPoint == null)
		{
			IndexRange indexRange = CalculateVisibleRange();
			ChartStyledPoint[] array = PrepearePoints();
			double num2 = 1.0;
			for (int i = 0; i < array.Length; i++)
			{
				double num3 = array[i].Point.YValues[1];
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			int j = indexRange.From;
			for (int num4 = indexRange.To + 1; j < num4; j++)
			{
				DrawPoint(args, array[j], num, bubbleYRange, bubbleType, clipBounds, enablePhongStyle, is3D, size2, size, seriesIndex, yValueIndex, num2);
			}
		}
		else
		{
			DrawPoint(args, styledPoint, num, bubbleYRange, bubbleType, clipBounds, enablePhongStyle, is3D, size2, size, seriesIndex, yValueIndex, styledPoint.Point.YValues[1]);
		}
	}
}
