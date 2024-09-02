using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class FunnelRenderer : ChartSeriesRenderer
{
	protected const float c_spacing = 0.03f;

	protected const float LABEL_RADIUS_OUTER_SPACE_RATIO = 0.95f;

	protected const float MAX_LABELS_INTERSECT_ITER_COUNT = 100f;

	private const float c_layerRectSpacing = 0.075f;

	protected ArrayList m_layers;

	protected ArrayList m_labels;

	protected RectangleF m_drawingRect;

	protected Graphics m_g;

	public override ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.All;

	protected override int RequireYValuesCount => 1;

	public FunnelRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		OnRender(g, m_series.ConfigItems.FunnelItem.ShowSeriesTitle);
	}

	public override void Render(ChartRenderArgs3D args)
	{
		OnRender(args, m_series.ConfigItems.FunnelItem.ShowSeriesTitle);
	}

	protected void OnRender(Graphics g, bool showTitle)
	{
		RectangleF bounds = base.Bounds;
		float num = 0.03f * Math.Min(bounds.Width, bounds.Height);
		base.Chart.Series.IndexOf(m_series);
		if (showTitle)
		{
			SizeF size = g.MeasureString(m_series.Text, base.SeriesStyle.GdipFont);
			RectangleF rectangle = LayoutHelper.AlignRectangle(bounds, size, ContentAlignment.BottomCenter);
			using (SolidBrush brush = new SolidBrush(base.SeriesStyle.TextColor))
			{
				g.DrawString(m_series.Text, base.SeriesStyle.GdipFont, brush, rectangle);
			}
			bounds.Height -= size.Height;
		}
		m_g = g;
		m_drawingRect = RectangleF.Inflate(bounds, 0f - num, 0f - num);
		ChartStyledPoint[] list = PrepearePoints().Clone() as ChartStyledPoint[];
		m_points = GetVisiblePoints(list);
		CreateLayersAndLabels(m_points, g, m_drawingRect);
		if (m_layers == null)
		{
			return;
		}
		foreach (AccumulationChartsLayer layer in m_layers)
		{
			ChartStyledPoint chartStyledPoint = m_points[layer.Index];
			layer.Draw(g, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
		}
		foreach (AccumulationChartsLabel label in m_labels)
		{
			label.Draw(g);
		}
	}

	protected void OnRender(ChartRenderArgs3D args, bool showTitle)
	{
		RectangleF bounds = base.Bounds;
		float num = 0.03f * Math.Min(bounds.Width, bounds.Height);
		if (showTitle)
		{
			GraphicsPath gp = new GraphicsPath();
			SizeF size = args.Graph.Graphics.MeasureString(m_series.Text, base.SeriesStyle.GdipFont);
			RectangleF rect = LayoutHelper.AlignRectangle(bounds, size, ContentAlignment.BottomCenter);
			RenderingHelper.AddTextPath(gp, args.Graph.Graphics, m_series.Text, base.SeriesStyle.GdipFont, rect);
			args.Graph.AddPolygon(Path3D.FromGraphicsPath(gp, -0.5 * args.Depth, new SolidBrush(base.SeriesStyle.TextColor)));
			bounds.Height -= size.Height;
		}
		m_g = args.Graph.Graphics;
		m_drawingRect = RectangleF.Inflate(bounds, 0f - num, 0f - num);
		ChartStyledPoint[] list = PrepearePoints().Clone() as ChartStyledPoint[];
		m_points = GetVisiblePoints(list);
		CreateLayersAndLabels(m_points, m_g, m_drawingRect);
		foreach (AccumulationChartsLayer layer in m_layers)
		{
			ChartStyledPoint chartStyledPoint = m_points[layer.Index];
			Polygon[] array = layer.Draw3D(GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
			for (int i = 0; i < array.Length; i++)
			{
				args.Graph.AddPolygon(array[i]);
			}
		}
		foreach (AccumulationChartsLabel label in m_labels)
		{
			args.Graph.AddPolygon(label.Draw3D());
		}
	}

	protected virtual void CreateLayersAndLabels(ChartStyledPoint[] points, Graphics g, RectangleF drawingRect)
	{
		if (points.Length == 0)
		{
			return;
		}
		ChartFunnelMode funnelMode = m_series.ConfigItems.FunnelItem.FunnelMode;
		if (funnelMode == ChartFunnelMode.YIsWidth)
		{
			Array.Sort(points, new ComparerPointWithIndexByY());
		}
		bool series3D = base.Chart.Series3D;
		float num = (float)Math.Sin((double)base.ChartArea.Tilt * Math.PI / 360.0);
		float rotationRation = (float)(((double)base.ChartArea.Rotation * (Math.PI / 180.0) - Math.PI / 4.0) % (Math.PI / 2.0));
		float num2 = (series3D ? (0.5f * num * drawingRect.Width) : 0f);
		bool num3 = m_series.ConfigItems.FunnelItem.LabelPlacement != ChartAccumulationLabelPlacement.Left;
		float gapRatio = m_series.ConfigItems.FunnelItem.GapRatio;
		ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.FunnelItem.LabelStyle;
		ChartAccumulationLabelPlacement labelPlacement = m_series.ConfigItems.FunnelItem.LabelPlacement;
		ChartFigureBase figureBase = m_series.ConfigItems.FunnelItem.FigureBase;
		float num4 = (num3 ? 1 : (-1));
		double allValue = GetAllValue();
		if (allValue == 0.0)
		{
			return;
		}
		int num5 = points.Length;
		double num6 = Math.Abs(points[^1].Point.YValues[0]);
		double num7 = (double)drawingRect.Width / num6;
		double num8 = num7 / 5.0;
		float x = (num4 + 1f) / 2f * drawingRect.Left + (1f - num4) / 2f * drawingRect.Right + (float)((double)num4 * num7 * num6 / 2.0);
		m_layers = new ArrayList(num5 - 1);
		m_labels = new ArrayList(num5);
		if (funnelMode == ChartFunnelMode.YIsWidth)
		{
			float num9 = (drawingRect.Height - num2) / (float)(num5 - 1);
			for (int i = 0; i < num5 - 1; i++)
			{
				PointF topCenterPoint = new PointF(x, drawingRect.Bottom - num9 * (float)(i + 1));
				AccumulationChartsLayer accumulationChartsLayer = new AccumulationChartsLayer(i, topCenterPoint, num9, series3D, num, funnelMode);
				accumulationChartsLayer.Series = m_series;
				accumulationChartsLayer.GapRatio = gapRatio;
				accumulationChartsLayer.FigureBase = figureBase;
				accumulationChartsLayer.RotationRation = rotationRation;
				accumulationChartsLayer.DepthPosition = drawingRect.Width / 2f;
				m_layers.Add(accumulationChartsLayer);
				ChartStyleInfo styleAt = GetStyleAt(points[i].Index);
				if (styleAt.Text.Length != 0 && styleAt.DisplayText)
				{
					AccumulationChartsLabel accumulationChartsLabel = new AccumulationChartsLabel(i, points[i].Point, styleAt, accumulationChartsLayer, AccumulationChartsLabelAttachMode.Bottom, m_series, points[i].Index);
					accumulationChartsLabel.LabelPlacement = labelPlacement;
					accumulationChartsLabel.LabelStyle = labelStyle;
					m_labels.Add(accumulationChartsLabel);
				}
			}
			ChartStyleInfo styleAt2 = GetStyleAt(points[num5 - 1].Index);
			((AccumulationChartsLayer)m_layers[num5 - 2]).TopLevel = true;
			AccumulationChartsLabel accumulationChartsLabel2 = new AccumulationChartsLabel(num5 - 1, points[num5 - 1].Point, styleAt2, (AccumulationChartsLayer)m_layers[num5 - 2], AccumulationChartsLabelAttachMode.Top, m_series, points[num5 - 1].Index);
			accumulationChartsLabel2.LabelPlacement = labelPlacement;
			accumulationChartsLabel2.LabelStyle = labelStyle;
			m_labels.Add(accumulationChartsLabel2);
			double d = double.NaN;
			for (int j = 0; j < 4; j++)
			{
				d = ChartMath.SmartBisection(OptimizationFunc_YIsWidth, num8, num7, num7 * 0.01, 20, 10);
				if (!double.IsNaN(d))
				{
					break;
				}
				for (int k = 0; k < m_labels.Count; k++)
				{
					((AccumulationChartsLabel)m_labels[k]).MaxTextWidth *= 0.75f;
				}
			}
			if (double.IsNaN(d))
			{
				OptimizationFunc_YIsWidth(num8);
			}
		}
		double num10 = drawingRect.Width / (drawingRect.Height - num2);
		double num11 = num10 / 12.0;
		x = (num4 + 1f) / 2f * drawingRect.Left + (1f - num4) / 2f * drawingRect.Right + (float)((double)num4 * num10 * (double)(drawingRect.Height - num2));
		if (funnelMode != ChartFunnelMode.YIsHeight)
		{
			return;
		}
		float num12 = 0f;
		for (int l = 0; l < num5; l++)
		{
			float num13 = Math.Abs((float)((double)(drawingRect.Height - num2) * points[l].Point.YValues[0] / allValue));
			num12 += num13;
			PointF topCenterPoint2 = new PointF(x, drawingRect.Bottom - num12);
			AccumulationChartsLayer accumulationChartsLayer2 = new AccumulationChartsLayer(l, topCenterPoint2, num13, series3D, num, funnelMode);
			accumulationChartsLayer2.Series = m_series;
			accumulationChartsLayer2.FigureBase = figureBase;
			accumulationChartsLayer2.GapRatio = gapRatio;
			accumulationChartsLayer2.RotationRation = rotationRation;
			accumulationChartsLayer2.DepthPosition = drawingRect.Width / 2f;
			m_layers.Add(accumulationChartsLayer2);
			ChartStyleInfo styleAt3 = GetStyleAt(points[l].Index);
			if (styleAt3.Text.Length != 0 && styleAt3.DisplayText)
			{
				AccumulationChartsLabel accumulationChartsLabel3 = new AccumulationChartsLabel(l, points[l].Point, styleAt3, accumulationChartsLayer2, AccumulationChartsLabelAttachMode.Center, m_series, points[l].Index);
				accumulationChartsLabel3.LabelPlacement = labelPlacement;
				accumulationChartsLabel3.LabelStyle = labelStyle;
				m_labels.Add(accumulationChartsLabel3);
			}
		}
		((AccumulationChartsLayer)m_layers[num5 - 1]).TopLevel = true;
		double d2 = double.NaN;
		for (int m = 0; m < 4; m++)
		{
			d2 = ChartMath.SmartBisection(OptimizationFunc_YIsHeight, num11, num10, num10 * 0.01, 20, 10);
			if (!double.IsNaN(d2))
			{
				break;
			}
			for (int n = 0; n < m_labels.Count; n++)
			{
				((AccumulationChartsLabel)m_labels[n]).MaxTextWidth *= 0.75f;
			}
		}
		if (double.IsNaN(d2))
		{
			OptimizationFunc_YIsHeight(num11);
		}
	}

	private double OptimizationFunc_YIsWidth(double k)
	{
		for (int i = 0; i < m_layers.Count; i++)
		{
			AccumulationChartsLayer obj = (AccumulationChartsLayer)m_layers[i];
			int num = obj.Index + 1;
			obj.UpWidth = Math.Abs((float)(k * m_points[num].Point.YValues[0]));
			int index = obj.Index;
			obj.DownWidth = Math.Abs((float)(k * m_points[index].Point.YValues[0]));
		}
		RectangleF rectangleF = CalcLayersAndLabelsSizeLocAndGetTheirBoundingRect();
		return m_drawingRect.Width - rectangleF.Width;
	}

	private double OptimizationFunc_YIsHeight(double ctg)
	{
		float num = 0f;
		for (int i = 0; i < m_layers.Count; i++)
		{
			AccumulationChartsLayer accumulationChartsLayer = (AccumulationChartsLayer)m_layers[i];
			accumulationChartsLayer.DownWidth = (float)(2.0 * ctg * (double)num);
			num += accumulationChartsLayer.Height;
			accumulationChartsLayer.UpWidth = (float)(2.0 * ctg * (double)num);
		}
		RectangleF rectangleF = CalcLayersAndLabelsSizeLocAndGetTheirBoundingRect();
		return m_drawingRect.Width - rectangleF.Width;
	}

	protected RectangleF CalcLayersAndLabelsSizeLocAndGetTheirBoundingRect()
	{
		RectangleF layersRect = GetLayersRect(m_layers);
		for (int i = 0; i < m_labels.Count; i++)
		{
			AccumulationChartsLabel obj = (AccumulationChartsLabel)m_labels[i];
			obj.CalcSize(m_g);
			obj.CalcLocation(layersRect);
		}
		FightWithLabelsIntersection();
		FightWithLabelsAndConnectionLinesIntersection();
		RectangleF labelsRect = GetLabelsRect(m_labels);
		RectangleF rectangleF = layersRect;
		if (!labelsRect.IsEmpty)
		{
			rectangleF = RectangleF.Union(layersRect, labelsRect);
		}
		PointF pointF = new PointF(m_drawingRect.Left - rectangleF.Left, m_drawingRect.Top + m_drawingRect.Height / 2f - layersRect.Top - layersRect.Height / 2f);
		for (int j = 0; j < m_layers.Count; j++)
		{
			AccumulationChartsLayer accumulationChartsLayer = (AccumulationChartsLayer)m_layers[j];
			accumulationChartsLayer.TopCenterPoint = new PointF(accumulationChartsLayer.TopCenterPoint.X + pointF.X, accumulationChartsLayer.TopCenterPoint.Y + pointF.Y);
		}
		RectangleF rectangleF2 = RectangleF.Union(labelsRect, m_drawingRect);
		for (int k = 0; k < m_labels.Count; k++)
		{
			AccumulationChartsLabel accumulationChartsLabel = (AccumulationChartsLabel)m_labels[k];
			PointF pointF2 = new PointF(m_drawingRect.Left - rectangleF.Left, accumulationChartsLabel.AllowYOffset ? (m_drawingRect.Top + m_drawingRect.Height / 2f - rectangleF2.Top - rectangleF2.Height / 2f) : 0f);
			accumulationChartsLabel.Rectangle = new RectangleF(accumulationChartsLabel.Rectangle.X + pointF2.X, accumulationChartsLabel.Rectangle.Y + pointF2.Y, accumulationChartsLabel.Rectangle.Width, accumulationChartsLabel.Rectangle.Height);
		}
		for (int l = 0; l < m_labels.Count; l++)
		{
			((AccumulationChartsLabel)m_labels[l]).CalcLocation(layersRect);
		}
		FightWithLabelsIntersection();
		FightWithLabelsAndConnectionLinesIntersection();
		labelsRect = GetLabelsRect(m_labels);
		rectangleF = GetLayersRect(m_layers);
		if (!labelsRect.IsEmpty)
		{
			rectangleF = RectangleF.Union(layersRect, labelsRect);
		}
		return rectangleF;
	}

	private void FightWithLabelsIntersection()
	{
		for (int i = 0; i < m_labels.Count; i++)
		{
			AccumulationChartsLabel accumulationChartsLabel = (AccumulationChartsLabel)m_labels[i];
			for (int j = 0; j <= i - 1; j++)
			{
				RectangleF rectangle = accumulationChartsLabel.Rectangle;
				RectangleF empty = RectangleF.Empty;
				empty = ((AccumulationChartsLabel)m_labels[j]).Rectangle;
				if (rectangle.IntersectsWith(empty))
				{
					accumulationChartsLabel.TryToAvoidRectangleIntersection(empty);
				}
			}
		}
	}

	private void FightWithLabelsAndConnectionLinesIntersection()
	{
		int num = int.MaxValue;
		for (int i = 0; (float)i < 100f; i++)
		{
			if (num < 0)
			{
				break;
			}
			num = 0;
			for (int j = 0; j < m_labels.Count; j++)
			{
				AccumulationChartsLabel accumulationChartsLabel = (AccumulationChartsLabel)m_labels[j];
				RectangleF rectangle = accumulationChartsLabel.Rectangle;
				AccumulationChartsLabel accumulationChartsLabel2 = null;
				if (j - 1 >= 0)
				{
					accumulationChartsLabel2 = (AccumulationChartsLabel)m_labels[j - 1];
				}
				PointF p = PointF.Empty;
				PointF p2 = PointF.Empty;
				accumulationChartsLabel2?.GetConnectioLinePoints(out p, out p2);
				AccumulationChartsLabel accumulationChartsLabel3 = null;
				if (j + 1 < m_labels.Count)
				{
					accumulationChartsLabel3 = (AccumulationChartsLabel)m_labels[j + 1];
				}
				PointF p3 = PointF.Empty;
				PointF p4 = PointF.Empty;
				accumulationChartsLabel3?.GetConnectioLinePoints(out p3, out p4);
				if (ChartMath.RectangleIntersectsWithLine(rectangle, p, p2))
				{
					num++;
					accumulationChartsLabel.TryToAvoidLineIntersection(p, p2);
				}
				if (ChartMath.RectangleIntersectsWithLine(rectangle, p3, p4))
				{
					num++;
					accumulationChartsLabel.TryToAvoidLineIntersection(p3, p4);
				}
			}
		}
	}

	private RectangleF GetLabelsRect(ArrayList labels)
	{
		RectangleF rectangleF = RectangleF.Empty;
		foreach (AccumulationChartsLabel label in labels)
		{
			rectangleF = ((!rectangleF.IsEmpty) ? RectangleF.Union(rectangleF, label.Rectangle) : label.Rectangle);
		}
		return rectangleF;
	}

	public RectangleF GetLayersRect(ArrayList layers)
	{
		RectangleF rectangleF = RectangleF.Empty;
		foreach (AccumulationChartsLayer layer in layers)
		{
			rectangleF = ((!rectangleF.IsEmpty) ? RectangleF.Union(rectangleF, layer.GetFullDrawingRect()) : layer.GetFullDrawingRect());
		}
		return RectangleF.Inflate(rectangleF, 0.075f * rectangleF.Width, 0f);
	}

	public override SizeF GetMinSize(Graphics g)
	{
		return SizeF.Empty;
	}

	protected double GetAllValue()
	{
		double num = 0.0;
		int num2 = m_series.PointFormats[ChartYValueUsage.YValue];
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			ChartPoint chartPoint = m_series.Points[i];
			if (IsVisiblePoint(chartPoint))
			{
				num += Math.Abs(chartPoint.YValues[num2]);
			}
		}
		return num;
	}

	private static bool AreRectanglesStacked(RectangleF r1, RectangleF r2)
	{
		if (r1.Left <= r2.Left && r1.Right > r2.Left)
		{
			return true;
		}
		if (r2.Left <= r1.Left && r2.Right > r1.Left)
		{
			return true;
		}
		return false;
	}

	internal override float GetTotalDepth()
	{
		return base.Bounds.Width;
	}
}
