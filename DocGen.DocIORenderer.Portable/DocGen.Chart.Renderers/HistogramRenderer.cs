using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class HistogramRenderer : ChartSeriesRenderer
{
	private int m_numberOfNormalDistributionPoints = 500;

	private double m_dotWidthDivideFactor = 80.0;

	protected override int RequireYValuesCount => 0;

	public HistogramRenderer(ChartSeries series)
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

	public override void Render(Graphics g)
	{
		ChartHistogramConfigItem histogramItem = m_series.ConfigItems.HistogramItem;
		double num = (double)m_series.ActualXAxis.RealLength * m_series.ActualYAxis.VisibleRange.Delta / (m_series.ActualXAxis.VisibleRange.Delta * (double)m_series.ActualYAxis.RealLength);
		double delta = m_series.ActualXAxis.VisibleRange.Delta;
		_ = base.Chart.Series.VisibleCount;
		_ = g.ClipBounds;
		GetSeriesOffset();
		SizeF thisOffset = GetThisOffset();
		base.Chart.Series.IndexOf(m_series);
		bool series3D = base.Chart.Series3D;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		ChartPointWithIndex[] array = new ChartPointWithIndex[m_series.Points.Count];
		ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(base.Chart, m_series);
		chartRenderArgs2D.Graph = new ChartGDIGraph(g);
		int i = 0;
		for (int count = m_series.Points.Count; i < count; i++)
		{
			array[i] = new ChartPointWithIndex(m_series.Points[i], i);
		}
		if (dropSeriesPoints)
		{
			Array.Sort(array, new ComparerPointWithIndexByX());
		}
		ArrayList arrayList = new ArrayList(m_series.Points.Count);
		_ = PointF.Empty;
		GetHistogramIntervalsValues(array, out var histogramIntervals, out var histogramValues);
		int num2 = histogramValues.Length;
		int num3 = 0;
		for (int j = 1; j <= num2; j++)
		{
			double num4 = histogramIntervals[j - 1];
			double num5 = histogramValues[j - 1];
			double num6 = histogramIntervals[j];
			double y = 0.0;
			ChartPoint firstPoint = new ChartPoint(num4, num5);
			ChartPoint secondPoint = new ChartPoint(num6, y);
			RectangleF rectangle = GetRectangle(firstPoint, secondPoint);
			ChartStyleInfo seriesStyle = base.SeriesStyle;
			if (seriesStyle.DisplayShadow && !series3D)
			{
				RectangleF rectangle2 = GetRectangle(firstPoint, secondPoint);
				rectangle2.Offset(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
				BrushPaint.FillRectangle(g, rectangle2, seriesStyle.ShadowInterior);
			}
			rectangle.X += thisOffset.Width;
			rectangle.Y += thisOffset.Height;
			GraphicsPath graphicsPath = null;
			GraphicsPath gp = null;
			GraphicsPath gp2 = null;
			graphicsPath = CreateBox(rectangle, series3D);
			if (chartRenderArgs2D.Chart.Series3D)
			{
				if (!chartRenderArgs2D.Chart.Style3D)
				{
					graphicsPath = CreateBox(rectangle, is3D: true);
				}
				else
				{
					graphicsPath = CreateBox(rectangle, is3D: true);
					gp = CreateBoxRight(rectangle, is3D: true);
					gp2 = CreateBoxTop(rectangle, is3D: true);
				}
			}
			ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
			if (!chartRenderArgs2D.Chart.Style3D)
			{
				chartSeriesPath.AddPrimitive(graphicsPath, seriesStyle.GdipPen, GetBrush());
			}
			else
			{
				chartSeriesPath.AddPrimitive(graphicsPath, null, BrushInfo.Empty);
				ChartSeriesPath chartSeriesPath2 = new ChartSeriesPath();
				chartSeriesPath2.AddPrimitive(graphicsPath, seriesStyle.GdipPen, GetBrush(), "BoxCenter");
				chartSeriesPath2.Draw(chartRenderArgs2D.Graph, "BoxCenter");
				ChartSeriesPath chartSeriesPath3 = new ChartSeriesPath();
				chartSeriesPath3.AddPrimitive(gp, seriesStyle.GdipPen, GetBrush(), "BoxRight");
				chartSeriesPath3.Draw(chartRenderArgs2D.Graph, "BoxRight");
				ChartSeriesPath chartSeriesPath4 = new ChartSeriesPath();
				chartSeriesPath4.AddPrimitive(gp2, seriesStyle.GdipPen, GetBrush());
				chartSeriesPath4.Draw(chartRenderArgs2D.Graph, "BoxTop");
			}
			chartSeriesPath.Bounds = rectangle;
			if (series3D)
			{
				arrayList.Add(chartSeriesPath);
			}
			else
			{
				chartSeriesPath.Draw(g);
				arrayList.Add(chartSeriesPath);
			}
			if (!histogramItem.ShowDataPoints)
			{
				continue;
			}
			double num7 = delta / m_dotWidthDivideFactor;
			double num8 = num7 * num;
			double num9 = double.NaN;
			double num10 = num8 / 2.0;
			int num11 = num3;
			while (num11 < array.Length)
			{
				int index = array[num11].Index;
				double x = array[num11].Point.X;
				ChartStyleInfo styleAt = GetStyleAt(index);
				if (x > num4 && x <= num6)
				{
					ChartSeriesPath chartSeriesPath5 = new ChartSeriesPath();
					double num12 = num7 / 2.0;
					double num13 = num8;
					num10 = ((x != num9) ? (num8 / 2.0) : (num10 + num8));
					ChartPoint firstPoint2 = new ChartPoint(x - num12, num5 + num10);
					ChartPoint secondPoint2 = new ChartPoint(x + num12, num5 + num13 + num10);
					RectangleF rectangle3 = GetRectangle(firstPoint2, secondPoint2);
					rectangle3.X += thisOffset.Width;
					rectangle3.Y += thisOffset.Height;
					GraphicsPath graphicsPath2 = new GraphicsPath();
					graphicsPath2.AddEllipse(rectangle3);
					chartSeriesPath5.AddPrimitive(graphicsPath2, styleAt.GdipPen, GetBrush(index));
					chartSeriesPath5.Bounds = rectangle3;
					if (series3D)
					{
						arrayList.Add(chartSeriesPath5);
					}
					else
					{
						chartSeriesPath5.Draw(g);
					}
				}
				else if (x > num6)
				{
					break;
				}
				num9 = x;
				num11++;
				num3 = num11;
			}
		}
		if (histogramItem.ShowNormalDistribution)
		{
			GetHistogramMeanAndDeviation(array, out var mean, out var standartDeviation);
			double min = m_series.ActualXAxis.Range.Min;
			double num14 = (m_series.ActualXAxis.Range.Max - min) / (double)(m_numberOfNormalDistributionPoints - 1);
			PointF[] array2 = new PointF[m_numberOfNormalDistributionPoints];
			for (int k = 0; k < m_numberOfNormalDistributionPoints; k++)
			{
				double x2 = min + (double)k * num14;
				double y2 = NormalDistribution(x2, mean, standartDeviation) * (double)array.Length * (histogramIntervals[1] - histogramIntervals[0]);
				ChartPoint cp = new ChartPoint(x2, y2);
				array2[k] = new PointF(GetXFromValue(cp, 0) + thisOffset.Width, GetYFromValue(cp, 0) + thisOffset.Height);
			}
			GraphicsPath graphicsPath3 = new GraphicsPath();
			graphicsPath3.AddLines(array2);
			g.DrawPath(base.SeriesStyle.GdipPen, graphicsPath3);
		}
		ChartSegment[] segments = (ChartSeriesPath[])arrayList.ToArray(typeof(ChartSeriesPath));
		m_segments = segments;
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		double num = (double)m_series.ActualXAxis.RealLength * m_series.ActualYAxis.VisibleRange.Delta / (m_series.ActualXAxis.VisibleRange.Delta * (double)m_series.ActualYAxis.RealLength);
		double delta = m_series.ActualXAxis.VisibleRange.Delta;
		bool dropSeriesPoints = base.Chart.DropSeriesPoints;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num2 = placeDepth;
		g.AddPolygon(CreateBoundsPolygon(num2));
		ChartPointWithIndex[] array = new ChartPointWithIndex[m_series.Points.Count];
		int i = 0;
		for (int count = m_series.Points.Count; i < count; i++)
		{
			array[i] = new ChartPointWithIndex(m_series.Points[i], i);
		}
		if (dropSeriesPoints)
		{
			Array.Sort(array, new ComparerPointWithIndexByX());
		}
		new ArrayList(m_series.Points.Count);
		_ = PointF.Empty;
		GetHistogramIntervalsValues(array, out var histogramIntervals, out var histogramValues);
		int num3 = histogramValues.Length;
		int num4 = 0;
		for (int j = 1; j <= num3; j++)
		{
			double num5 = histogramIntervals[j - 1];
			double num6 = histogramValues[j - 1];
			double num7 = histogramIntervals[j];
			double y = 0.0;
			ChartPoint firstPoint = new ChartPoint(num5, num6);
			ChartPoint secondPoint = new ChartPoint(num7, y);
			RectangleF rectangle = GetRectangle(firstPoint, secondPoint);
			ChartStyleInfo seriesStyle = base.SeriesStyle;
			if (!m_series.Rotate)
			{
				g.CreateBoxV(new Vector3D(rectangle.Left, rectangle.Top, num2), new Vector3D(rectangle.Right, rectangle.Bottom, num2 + seriesDepth), seriesStyle.GdipPen, GetBrush());
			}
			else
			{
				g.CreateBox(new Vector3D(rectangle.Left, rectangle.Top, num2), new Vector3D(rectangle.Right, rectangle.Bottom, num2 + seriesDepth), seriesStyle.GdipPen, GetBrush());
			}
			if (!m_series.ConfigItems.HistogramItem.ShowDataPoints)
			{
				continue;
			}
			double num8 = delta / m_dotWidthDivideFactor;
			double num9 = num8 * num;
			double num10 = double.NaN;
			double num11 = num9 / 2.0;
			int num12 = num4;
			while (num12 < array.Length)
			{
				int index = array[num12].Index;
				double x = array[num12].Point.X;
				ChartStyleInfo styleAt = GetStyleAt(index);
				if (x > num5 && x <= num7)
				{
					double num13 = num8 / 2.0;
					double num14 = num9;
					num11 = ((x != num10) ? (num9 / 2.0) : (num11 + num9));
					ChartPoint firstPoint2 = new ChartPoint(x - num13, num6 + num11);
					ChartPoint secondPoint2 = new ChartPoint(x + num13, num6 + num14 + num11);
					RectangleF rectangle2 = GetRectangle(firstPoint2, secondPoint2);
					Vector3D v = new Vector3D(rectangle2.X, rectangle2.Y, num2);
					g.CreateEllipse(v, rectangle2.Size, 10, styleAt.GdipPen, GetBrush(index));
				}
				else if (x > num7)
				{
					break;
				}
				num10 = x;
				num12++;
				num4 = num12;
			}
		}
		if (m_series.ConfigItems.HistogramItem.ShowNormalDistribution)
		{
			GetHistogramMeanAndDeviation(array, out var mean, out var standartDeviation);
			double min = m_series.ActualXAxis.Range.Min;
			double num15 = (m_series.ActualXAxis.Range.Max - min) / (double)(m_numberOfNormalDistributionPoints - 1);
			Vector3D[] array2 = new Vector3D[m_numberOfNormalDistributionPoints * 2];
			for (int k = 0; k < m_numberOfNormalDistributionPoints; k++)
			{
				double x2 = min + (double)k * num15;
				double y2 = NormalDistribution(x2, mean, standartDeviation) * (double)array.Length * (histogramIntervals[1] - histogramIntervals[0]);
				ChartPoint cp = new ChartPoint(x2, y2);
				array2[k] = new Vector3D(GetXFromValue(cp, 0), GetYFromValue(cp, 0), num2);
			}
			for (int l = 1; l <= m_numberOfNormalDistributionPoints; l++)
			{
				double x3 = min + (double)(m_numberOfNormalDistributionPoints - l) * num15;
				double y3 = NormalDistribution(x3, mean, standartDeviation) * (double)array.Length * (histogramIntervals[1] - histogramIntervals[0]);
				ChartPoint cp2 = new ChartPoint(x3, y3);
				array2[m_numberOfNormalDistributionPoints + l - 1] = new Vector3D(GetXFromValue(cp2, 0), GetYFromValue(cp2, 0), num2);
			}
			Polygon polygon = new Polygon(array2, base.SeriesStyle.GdipPen);
			g.AddPolygon(polygon);
		}
	}

	private double NormalDistribution(double x, double m, double sigma)
	{
		return Math.Exp((0.0 - (x - m)) * (x - m) / (2.0 * sigma * sigma)) / (sigma * Math.Sqrt(Math.PI * 2.0));
	}

	protected override BrushInfo GetBrush()
	{
		return GradientBrush(base.GetBrush());
	}

	protected override BrushInfo GetBrush(int index)
	{
		return GradientBrush(base.GetBrush());
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

	public override DoubleRange GetYDataMeasure()
	{
		return new DoubleRange(0.0, GetHistogramMax());
	}

	public void GetHistogramIntervalsValues(ChartPointWithIndex[] cpwiA, out double[] histogramIntervals, out double[] histogramValues)
	{
		int num = cpwiA.Length;
		int numberOfIntervals = m_series.ConfigItems.HistogramItem.NumberOfIntervals;
		histogramIntervals = new double[numberOfIntervals + 1];
		histogramValues = new double[numberOfIntervals];
		double num2 = m_series.ActualXAxis.Range.Delta / (double)numberOfIntervals;
		double min = m_series.ActualXAxis.Range.Min;
		for (int i = 0; i <= numberOfIntervals; i++)
		{
			histogramIntervals[i] = min + num2 * (double)i;
		}
		for (int j = 0; j < numberOfIntervals; j++)
		{
			histogramValues[j] = 0.0;
		}
		double num3 = histogramIntervals[0];
		int num4 = 0;
		for (int k = 1; k <= numberOfIntervals; k++)
		{
			double num5 = histogramIntervals[k];
			int num6 = num4;
			while (num6 < num)
			{
				double x = cpwiA[num6].Point.X;
				if (x > num3 && x <= num5)
				{
					histogramValues[k - 1] += 1.0;
				}
				else if (x > num5)
				{
					break;
				}
				num6++;
				num4 = num6;
			}
			num3 = num5;
		}
	}

	public double GetHistogramMax()
	{
		PrepearePoints();
		ChartPointWithIndex[] array = new ChartPointWithIndex[m_series.Points.Count];
		int i = 0;
		for (int count = m_series.Points.Count; i < count; i++)
		{
			array[i] = new ChartPointWithIndex(m_series.Points[i], i);
		}
		Array.Sort(array, new ComparerPointWithIndexByX());
		new ArrayList(m_series.Points.Count);
		_ = PointF.Empty;
		GetHistogramIntervalsValues(array, out var _, out var histogramValues);
		int num = histogramValues.Length;
		double num2 = 0.0;
		for (int j = 0; j < num; j++)
		{
			if (num2 < histogramValues[j])
			{
				num2 = histogramValues[j];
			}
		}
		return num2;
	}

	public void GetHistogramMeanAndDeviation(ChartPointWithIndex[] cpwiA, out double mean, out double standartDeviation)
	{
		int num = cpwiA.Length;
		_ = m_series.ConfigItems.HistogramItem.NumberOfIntervals;
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			num2 += cpwiA[i].Point.X;
		}
		mean = num2 / (double)num;
		num2 = 0.0;
		for (int j = 0; j < num; j++)
		{
			double num3 = cpwiA[j].Point.X - mean;
			num2 += num3 * num3;
		}
		standartDeviation = Math.Sqrt(num2 / (double)num);
	}
}
