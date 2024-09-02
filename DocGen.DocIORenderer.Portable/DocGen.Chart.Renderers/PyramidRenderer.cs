using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class PyramidRenderer : FunnelRenderer
{
	public PyramidRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		OnRender(g, m_series.ConfigItems.PyramidItem.ShowSeriesTitle);
	}

	public override void Render(ChartRenderArgs3D args)
	{
		OnRender(args, m_series.ConfigItems.PyramidItem.ShowSeriesTitle);
	}

	protected override void CreateLayersAndLabels(ChartStyledPoint[] points, Graphics g, RectangleF drawingRect)
	{
		if (points.Length == 0)
		{
			return;
		}
		ChartPyramidMode pyramidMode = m_series.ConfigItems.PyramidItem.PyramidMode;
		bool series3D = base.Chart.Series3D;
		float num = (float)Math.Sin((double)base.ChartArea.Tilt * (Math.PI / 180.0));
		float rotationRation = (float)(((double)base.ChartArea.Rotation * Math.PI / 180.0 - Math.PI / 4.0) % Math.PI / 2.0);
		float num2 = ((series3D && !base.Chart.RealMode3D) ? (num * drawingRect.Width / 2f) : 0f);
		bool num3 = m_series.ConfigItems.PyramidItem.LabelPlacement != ChartAccumulationLabelPlacement.Left;
		float gapRatio = m_series.ConfigItems.PyramidItem.GapRatio;
		ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.PyramidItem.LabelStyle;
		ChartAccumulationLabelPlacement labelPlacement = m_series.ConfigItems.PyramidItem.LabelPlacement;
		ChartFigureBase figureBase = m_series.ConfigItems.PyramidItem.FigureBase;
		float num4 = (num3 ? 1 : (-1));
		double allValue = GetAllValue();
		if (allValue == 0.0)
		{
			return;
		}
		int num5 = points.Length;
		int num6 = m_series.PointFormats[ChartYValueUsage.YValue];
		Math.Abs(points[^1].Point.YValues[0]);
		m_layers = new ArrayList(num5);
		m_labels = new ArrayList(num5);
		double num7 = drawingRect.Width / (2f * (drawingRect.Height - num2));
		double x = drawingRect.Width / (4f * (drawingRect.Height - num2));
		double x2 = num7 / 24.0;
		float x3 = (num4 + 1f) / 2f * drawingRect.Left + (1f - num4) / 2f * drawingRect.Right + (float)((double)num4 * num7 * (double)(drawingRect.Height - num2));
		ChartMath.DoubleFunc doubleFunc = OptimizationFunc_Pyramid;
		if (pyramidMode == ChartPyramidMode.Linear)
		{
			float num8 = 0f;
			for (int i = 0; i < num5; i++)
			{
				float num9 = (float)((double)(drawingRect.Height - num2) * Math.Abs(points[i].YValues[num6] / allValue));
				PointF topCenterPoint = new PointF(x3, drawingRect.Top + num8);
				num8 += num9;
				AccumulationChartsLayer accumulationChartsLayer = new AccumulationChartsLayer(i, topCenterPoint, num9, series3D, num, ChartFunnelMode.YIsHeight);
				accumulationChartsLayer.Series = m_series;
				accumulationChartsLayer.GapRatio = gapRatio;
				accumulationChartsLayer.MinWidth = 0f;
				accumulationChartsLayer.FigureBase = figureBase;
				accumulationChartsLayer.RotationRation = rotationRation;
				accumulationChartsLayer.DepthPosition = drawingRect.Width / 2f;
				m_layers.Add(accumulationChartsLayer);
				ChartStyleInfo style = points[i].Style;
				if (style.Text.Length != 0 && style.DisplayText)
				{
					AccumulationChartsLabel accumulationChartsLabel = new AccumulationChartsLabel(i, points[i].Point, style, accumulationChartsLayer, AccumulationChartsLabelAttachMode.Center, m_series, points[i].Index);
					accumulationChartsLabel.LabelPlacement = labelPlacement;
					accumulationChartsLabel.LabelStyle = labelStyle;
					m_labels.Add(accumulationChartsLabel);
				}
			}
		}
		else
		{
			float num10 = 0f;
			for (int j = 0; j < num5; j++)
			{
				float num11 = (float)((double)(drawingRect.Height - num2) * Math.Abs(points[j].YValues[num6]) / allValue);
				PointF topCenterPoint2 = new PointF(x3, drawingRect.Top + num10);
				num10 += num11;
				AccumulationChartsLayer accumulationChartsLayer2 = new AccumulationChartsLayer(j, topCenterPoint2, num11, series3D, num, ChartFunnelMode.YIsHeight);
				accumulationChartsLayer2.Series = m_series;
				accumulationChartsLayer2.GapRatio = gapRatio;
				accumulationChartsLayer2.FigureBase = figureBase;
				accumulationChartsLayer2.RotationRation = rotationRation;
				accumulationChartsLayer2.MinWidth = 0f;
				accumulationChartsLayer2.DepthPosition = drawingRect.Width / 2f;
				m_layers.Add(accumulationChartsLayer2);
				ChartStyleInfo styleAt = GetStyleAt(points[j].Index);
				if (styleAt.Text.Length != 0 && styleAt.DisplayText)
				{
					AccumulationChartsLabel accumulationChartsLabel2 = new AccumulationChartsLabel(j, points[j].Point, styleAt, accumulationChartsLayer2, AccumulationChartsLabelAttachMode.Center, m_series, points[j].Index);
					accumulationChartsLabel2.LabelPlacement = labelPlacement;
					accumulationChartsLabel2.LabelStyle = labelStyle;
					m_labels.Add(accumulationChartsLabel2);
				}
			}
			doubleFunc = OptimizationFunc_SurfacePyramid;
		}
		double d = double.NaN;
		for (int k = 0; k < 4; k++)
		{
			d = ChartMath.SmartBisection(doubleFunc, x2, num7, num7 * 0.01, 20, 10);
			if (!double.IsNaN(d))
			{
				break;
			}
			for (int l = 0; l < m_labels.Count; l++)
			{
				((AccumulationChartsLabel)m_labels[l]).MaxTextWidth *= 0.75f;
			}
		}
		if (double.IsNaN(d))
		{
			doubleFunc(x);
		}
		m_layers.Reverse();
	}

	private double OptimizationFunc_Pyramid(double ctg)
	{
		float num = 0f;
		for (int i = 0; i < m_layers.Count; i++)
		{
			AccumulationChartsLayer accumulationChartsLayer = (AccumulationChartsLayer)m_layers[i];
			accumulationChartsLayer.UpWidth = (float)(2.0 * ctg * (double)num);
			num += accumulationChartsLayer.Height;
			accumulationChartsLayer.DownWidth = (float)(2.0 * ctg * (double)num);
		}
		RectangleF rectangleF = CalcLayersAndLabelsSizeLocAndGetTheirBoundingRect();
		return m_drawingRect.Width - rectangleF.Width;
	}

	private double OptimizationFunc_SurfacePyramid(double ctg)
	{
		bool series3D = base.Chart.Series3D;
		float num = (float)Math.Sin((double)base.ChartArea.Tilt * Math.PI / 360.0);
		float num2 = 2f * (float)ctg;
		float num3 = (series3D ? (num * num2) : 0f);
		float num4 = m_drawingRect.Height / (1f + num3);
		float num5 = num2 * num4 * num4 / 2f;
		double allValue = GetAllValue();
		float num6 = 0f;
		float num7 = 0f;
		for (int i = 0; i < m_layers.Count; i++)
		{
			float num8 = num5 * (float)(Math.Abs(m_points[i].Point.YValues[0]) / allValue);
			double d = num2 * num2 * num6 * num6 + 2f * num2 * num8;
			num7 = ((0f - num2) * num6 + (float)Math.Sqrt(d)) / num2;
			AccumulationChartsLayer accumulationChartsLayer = (AccumulationChartsLayer)m_layers[i];
			PointF topCenterPoint = new PointF(accumulationChartsLayer.TopCenterPoint.X, m_drawingRect.Top + num6);
			accumulationChartsLayer.TopCenterPoint = topCenterPoint;
			accumulationChartsLayer.UpWidth = (float)(2.0 * ctg * (double)num6);
			accumulationChartsLayer.Height = num7;
			num6 += num7;
			accumulationChartsLayer.DownWidth = (float)(2.0 * ctg * (double)num6);
		}
		RectangleF rectangleF = CalcLayersAndLabelsSizeLocAndGetTheirBoundingRect();
		return m_drawingRect.Width - rectangleF.Width;
	}
}
