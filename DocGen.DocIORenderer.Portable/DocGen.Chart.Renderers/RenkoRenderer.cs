using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class RenkoRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	public RenkoRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		base.Chart.Series.IndexOf(m_series);
		bool series3D = base.Chart.Series3D;
		_ = m_series.ConfigItems.StepItem.Inverted;
		_ = base.SeriesStyle.GdipPen;
		_ = base.SeriesStyle.Interior;
		Pen gdipPen = base.SeriesStyle.GdipPen;
		SizeF seriesOffset = GetSeriesOffset();
		SizeF thisOffset = GetThisOffset();
		bool flag = false;
		BrushInfo upPriceInterior = GetUpPriceInterior(base.SeriesStyle.Interior);
		BrushInfo downPriceInterior = GetDownPriceInterior(base.SeriesStyle.Interior);
		float width = thisOffset.Width;
		float height = thisOffset.Height;
		if (m_series.XAxis.Inversed)
		{
			flag = true;
		}
		double num = m_series.ReversalAmount;
		if (m_series.ReversalAmount <= 0.0)
		{
			num = 1.0;
		}
		ChartPointIndexer points = m_series.Points;
		int n = 0;
		ChartPoint chartPoint2;
		ChartPoint chartPoint;
		ChartPoint chartPoint3 = (chartPoint2 = (chartPoint = (IsVisiblePoint(points[0]) ? points[0] : VisibleChartPoint(points, 0, out n, first: true))));
		double num2 = chartPoint3.YValues[0];
		double num3 = chartPoint3.YValues[0];
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		_ = chartPoint3.YValues[0];
		GetPointFromIndex(0);
		ChartPoint chartPoint4 = chartPoint3;
		_ = GetPointFromIndex(0).Y;
		_ = GetPointFromIndex(1).Y;
		_ = GetPointFromIndex(0).X;
		for (int i = 1; i < points.Count; i++)
		{
			if (!IsVisiblePoint(points[i]))
			{
				ChartPoint chartPoint5 = (chartPoint2 = (chartPoint = VisibleChartPoint(points, i, out n, first: true)));
				_ = chartPoint5.YValues[0];
				num3 = chartPoint5.YValues[0];
				num2 = chartPoint5.YValues[0];
				continue;
			}
			chartPoint4 = points[i];
			double num4 = chartPoint4.YValues[0];
			GetPointFromIndex(i);
			double num5 = num4 - num3;
			if (num5 <= 0.0 - num)
			{
				int num6 = (int)Math.Floor(Math.Abs(num5 / num));
				double num7 = num;
				double num8 = (chartPoint4.X - chartPoint2.X) / (double)num6;
				for (int j = 0; j < num6; j++)
				{
					ChartPoint chartPoint6 = new ChartPoint(chartPoint2.X + (double)j * num8, chartPoint2.YValues[0] - (double)j * num7);
					ChartPoint secondPoint = new ChartPoint(chartPoint6.X + num8, chartPoint6.YValues[0] - num7);
					arrayList.Add(GetRectangle(chartPoint6, secondPoint));
					arrayList2.Add(downPriceInterior);
				}
				num3 -= (double)num6 * num;
				num2 = num3 + num;
				chartPoint2 = new ChartPoint(chartPoint2.X + (double)num6 * num8, chartPoint2.YValues[0] - (double)num6 * num7);
				chartPoint = new ChartPoint(chartPoint2.X, chartPoint2.YValues[0] + num7);
			}
			double num9 = num4 - num2;
			if (num9 >= num)
			{
				int num10 = (int)Math.Floor(Math.Abs(num9 / num));
				double num11 = num;
				double num12 = (chartPoint4.X - chartPoint2.X) / (double)num10;
				for (int k = 0; k < num10; k++)
				{
					ChartPoint chartPoint7 = new ChartPoint(chartPoint.X + (double)k * num12, chartPoint.YValues[0] + (double)k * num11);
					ChartPoint secondPoint2 = new ChartPoint(chartPoint7.X + num12, chartPoint7.YValues[0] + num11);
					arrayList.Add(GetRectangle(chartPoint7, secondPoint2));
					arrayList2.Add(upPriceInterior);
				}
				num2 += (double)num10 * num;
				num3 = num2 - num;
				chartPoint = new ChartPoint(chartPoint.X + (double)num10 * num12, chartPoint.YValues[0] + (double)num10 * num11);
				chartPoint2 = new ChartPoint(chartPoint.X, chartPoint.YValues[0] - num11);
			}
		}
		for (int l = 0; l < arrayList.Count; l++)
		{
			RectangleF rectangleF = (RectangleF)arrayList[l];
			rectangleF.X += width;
			rectangleF.Y += height;
			arrayList[l] = rectangleF;
		}
		if (base.SeriesStyle.DisplayShadow && !series3D)
		{
			ChartStyleInfo seriesStyle = base.SeriesStyle;
			int m = 0;
			int num13 = 1;
			int num14 = arrayList.Count;
			if (flag)
			{
				m = num14 - 1;
				num13 = -1;
				num14 = -1;
			}
			for (; m != num14; m += num13)
			{
				RectangleF r = new RectangleF(((RectangleF)arrayList[m]).Location, ((RectangleF)arrayList[m]).Size);
				r.Offset(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
				BrushPaint.FillRectangle(g, r, seriesStyle.ShadowInterior);
			}
		}
		if (series3D)
		{
			int num15 = 0;
			int num16 = 1;
			int num17 = arrayList.Count;
			if (flag)
			{
				num15 = num17 - 1;
				num16 = -1;
				num17 = -1;
			}
			for (; num15 != num17; num15 += num16)
			{
				BrushInfo brush = (BrushInfo)arrayList2[num15];
				Draw3DRectangle(g, (RectangleF)arrayList[num15], seriesOffset, brush, gdipPen);
			}
			return;
		}
		int num18 = 0;
		int num19 = 1;
		int num20 = arrayList.Count;
		if (flag)
		{
			num18 = num20 - 1;
			num19 = -1;
			num20 = -1;
		}
		for (; num18 != num20; num18 += num19)
		{
			BrushInfo brush2 = (BrushInfo)arrayList2[num18];
			BrushPaint.FillRectangle(g, (RectangleF)arrayList[num18], brush2);
			if (base.EnableStyles)
			{
				g.DrawRectangle(gdipPen, ((RectangleF)arrayList[num18]).X, ((RectangleF)arrayList[num18]).Y, ((RectangleF)arrayList[num18]).Width, ((RectangleF)arrayList[num18]).Height);
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		_ = base.Chart.Series3D;
		_ = m_series.ConfigItems.StepItem.Inverted;
		_ = base.SeriesStyle.GdipPen;
		_ = base.SeriesStyle.Interior;
		Pen gdipPen = base.SeriesStyle.GdipPen;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		bool flag = false;
		if (m_series.XAxis.Inversed)
		{
			flag = true;
		}
		BrushInfo upPriceInterior = GetUpPriceInterior(base.SeriesStyle.Interior);
		BrushInfo downPriceInterior = GetDownPriceInterior(base.SeriesStyle.Interior);
		double num = m_series.ReversalAmount;
		if (m_series.ReversalAmount <= 0.0)
		{
			num = 1.0;
		}
		ChartPointIndexer points = m_series.Points;
		int n = 0;
		ChartPoint chartPoint2;
		ChartPoint chartPoint;
		ChartPoint chartPoint3 = (chartPoint2 = (chartPoint = (IsVisiblePoint(points[0]) ? points[0] : VisibleChartPoint(points, 0, out n, first: true))));
		double num2 = chartPoint3.YValues[0];
		double num3 = chartPoint3.YValues[0];
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		_ = chartPoint3.YValues[0];
		GetPointFromIndex(0);
		ChartPoint chartPoint4 = chartPoint3;
		_ = GetPointFromIndex(0).Y;
		_ = GetPointFromIndex(1).Y;
		_ = GetPointFromIndex(0).X;
		for (int i = 1; i < points.Count; i++)
		{
			if (!IsVisiblePoint(points[i]))
			{
				ChartPoint chartPoint5 = (chartPoint2 = (chartPoint = VisibleChartPoint(points, i, out n, first: true)));
				_ = chartPoint5.YValues[0];
				num3 = chartPoint5.YValues[0];
				num2 = chartPoint5.YValues[0];
				continue;
			}
			chartPoint4 = points[i];
			double num4 = chartPoint4.YValues[0];
			GetPointFromIndex(i);
			double num5 = num4 - num3;
			if (num5 <= 0.0 - num)
			{
				int num6 = (int)Math.Floor(Math.Abs(num5 / num));
				double num7 = num;
				double num8 = (chartPoint4.X - chartPoint2.X) / (double)num6;
				for (int j = 0; j < num6; j++)
				{
					ChartPoint chartPoint6 = new ChartPoint(chartPoint2.X + (double)j * num8, chartPoint2.YValues[0] - (double)j * num7);
					ChartPoint secondPoint = new ChartPoint(chartPoint6.X + num8, chartPoint6.YValues[0] - num7);
					arrayList.Add(GetRectangle(chartPoint6, secondPoint));
					arrayList2.Add(downPriceInterior);
				}
				num3 -= (double)num6 * num;
				num2 = num3 + num;
				chartPoint2 = new ChartPoint(chartPoint2.X + (double)num6 * num8, chartPoint2.YValues[0] - (double)num6 * num7);
				chartPoint = new ChartPoint(chartPoint2.X, chartPoint2.YValues[0] + num7);
			}
			double num9 = num4 - num2;
			if (num9 >= num)
			{
				int num10 = (int)Math.Floor(Math.Abs(num9 / num));
				double num11 = num;
				double num12 = (chartPoint4.X - chartPoint2.X) / (double)num10;
				for (int k = 0; k < num10; k++)
				{
					ChartPoint chartPoint7 = new ChartPoint(chartPoint.X + (double)k * num12, chartPoint.YValues[0] + (double)k * num11);
					ChartPoint secondPoint2 = new ChartPoint(chartPoint7.X + num12, chartPoint7.YValues[0] + num11);
					arrayList.Add(GetRectangle(chartPoint7, secondPoint2));
					arrayList2.Add(upPriceInterior);
				}
				num2 += (double)num10 * num;
				num3 = num2 - num;
				chartPoint = new ChartPoint(chartPoint.X + (double)num10 * num12, chartPoint.YValues[0] + (double)num10 * num11);
				chartPoint2 = new ChartPoint(chartPoint.X, chartPoint.YValues[0] - num11);
			}
		}
		for (int l = 0; l < arrayList.Count; l++)
		{
			RectangleF rectangleF = (RectangleF)arrayList[l];
			arrayList[l] = rectangleF;
		}
		int m = 0;
		int num13 = 1;
		int num14 = arrayList.Count;
		if (flag)
		{
			m = num14 - 1;
			num13 = -1;
			num14 = -1;
		}
		for (; m != num14; m += num13)
		{
			BrushInfo b = (BrushInfo)arrayList2[m];
			RectangleF rectangleF2 = (RectangleF)arrayList[m];
			g.CreateBox(new Vector3D(rectangleF2.Left, rectangleF2.Top, placeDepth), new Vector3D(rectangleF2.Right, rectangleF2.Bottom, placeDepth + seriesDepth), gdipPen, b);
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		base.DrawIcon(g, bounds, isShadow, shadowColor);
		if (!isShadow)
		{
			int num = bounds.Width / 4;
			int num2 = bounds.Height / 4;
			SolidBrush solidBrush = new SolidBrush(m_series.ConfigItems.FinancialItem.PriceUpColor);
			SolidBrush solidBrush2 = new SolidBrush(m_series.ConfigItems.FinancialItem.PriceDownColor);
			g.FillRectangle(solidBrush2, bounds.Left, bounds.Top + num2, num, num2);
			g.FillRectangle(solidBrush2, bounds.Left + num, bounds.Top + 2 * num2, num, num2);
			g.FillRectangle(solidBrush, bounds.Left + 2 * num, bounds.Top + num2, num, num2);
			g.FillRectangle(solidBrush, bounds.Left + 3 * num, bounds.Top, num, num2);
			solidBrush.Dispose();
			solidBrush2.Dispose();
		}
	}
}
