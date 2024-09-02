using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class KagiRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	public KagiRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(Graphics g)
	{
		base.Chart.Series.IndexOf(m_series);
		bool series3D = base.Chart.Series3D;
		_ = m_series.ConfigItems.StepItem.Inverted;
		Pen pen = base.SeriesStyle.GdipPen.Clone() as Pen;
		BrushInfo interior = base.SeriesStyle.Interior;
		Pen gdipPen = m_series.GetOfflineStyle().GdipPen;
		_ = g.ClipBounds;
		Color priceUpColor = m_series.ConfigItems.FinancialItem.PriceUpColor;
		Color priceDownColor = m_series.ConfigItems.FinancialItem.PriceDownColor;
		double num = m_series.ReversalAmount;
		bool reversalIsPercent = m_series.ReversalIsPercent;
		if (reversalIsPercent)
		{
			num /= 100.0;
		}
		double num2 = (float)m_series.Points[0].YValues[0];
		double num3 = (float)m_series.Points[0].YValues[0];
		double num4 = (float)m_series.Points[0].YValues[0];
		double num5 = (float)m_series.Points[0].YValues[0];
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		ArrayList arrayList3 = new ArrayList();
		_ = m_series.Points[0].YValues[0];
		bool flag = m_series.Points[0].YValues[0] <= m_series.Points[1].YValues[0];
		double x = m_series.Points[0].X;
		Color color = (flag ? priceUpColor : priceDownColor);
		arrayList2.Add(GetPointFromIndex(0));
		arrayList3.Add(color);
		for (int i = 1; i < m_series.Points.Count; i++)
		{
			double num6 = m_series.Points[i].YValues[0];
			if (reversalIsPercent)
			{
				if ((num6 - num4) / Math.Abs(num4) <= 0.0 - num && flag)
				{
					ChartPoint chartPoint = new ChartPoint(x, num4);
					arrayList.Add(chartPoint);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint, 0), GetYFromValue(chartPoint, 0)));
					arrayList3.Add(color);
					double x2 = m_series.Points[i].X;
					chartPoint = new ChartPoint(x2, num4);
					arrayList.Add(chartPoint);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint, 0), GetYFromValue(chartPoint, 0)));
					arrayList3.Add(color);
					flag = false;
					x = x2;
					num2 = num4;
					num3 = num5;
					num4 = num2;
					num5 = num6;
				}
				if ((num6 - num5) / Math.Abs(num5) >= num && !flag)
				{
					ChartPoint chartPoint2 = new ChartPoint(x, num5);
					arrayList.Add(chartPoint2);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint2, 0), GetYFromValue(chartPoint2, 0)));
					arrayList3.Add(color);
					double x3 = m_series.Points[i].X;
					chartPoint2 = new ChartPoint(x3, num5);
					arrayList.Add(chartPoint2);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint2, 0), GetYFromValue(chartPoint2, 0)));
					arrayList3.Add(color);
					flag = true;
					x = x3;
					num2 = num4;
					num3 = num5;
					num4 = num6;
					num5 = num3;
				}
			}
			else
			{
				if (num6 - num4 <= 0.0 - num && flag)
				{
					ChartPoint chartPoint3 = new ChartPoint(x, num4);
					arrayList.Add(chartPoint3);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint3, 0), GetYFromValue(chartPoint3, 0)));
					arrayList3.Add(color);
					double x4 = m_series.Points[i].X;
					chartPoint3 = new ChartPoint(x4, num4);
					arrayList.Add(chartPoint3);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint3, 0), GetYFromValue(chartPoint3, 0)));
					arrayList3.Add(color);
					flag = false;
					x = x4;
					num2 = num4;
					num3 = num5;
					num4 = num2;
					num5 = num6;
				}
				if (num6 - num5 >= num && !flag)
				{
					ChartPoint chartPoint4 = new ChartPoint(x, num5);
					arrayList.Add(chartPoint4);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint4, 0), GetYFromValue(chartPoint4, 0)));
					arrayList3.Add(color);
					double x5 = m_series.Points[i].X;
					chartPoint4 = new ChartPoint(x5, num5);
					arrayList.Add(chartPoint4);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint4, 0), GetYFromValue(chartPoint4, 0)));
					arrayList3.Add(color);
					flag = true;
					x = x5;
					num2 = num4;
					num3 = num5;
					num4 = num6;
					num5 = num3;
				}
			}
			if (flag)
			{
				if (num6 > num2 && num5 < num2 && color != priceUpColor)
				{
					ChartPoint chartPoint5 = new ChartPoint(x, num2);
					arrayList.Add(chartPoint5);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint5, 0), GetYFromValue(chartPoint5, 0)));
					color = priceUpColor;
					arrayList3.Add(color);
				}
			}
			else if (num6 < num3 && num4 > num3 && color != priceDownColor)
			{
				ChartPoint chartPoint6 = new ChartPoint(x, num3);
				arrayList.Add(chartPoint6);
				arrayList2.Add(new PointF(GetXFromValue(chartPoint6, 0), GetYFromValue(chartPoint6, 0)));
				color = priceDownColor;
				arrayList3.Add(color);
			}
			if (num6 < num5)
			{
				num5 = num6;
			}
			if (num6 > num4)
			{
				num4 = num6;
			}
		}
		ChartPoint chartPoint7 = m_series.Points[m_series.Points.Count - 1];
		ChartPoint chartPoint8 = new ChartPoint(x, chartPoint7.YValues);
		arrayList.Add(chartPoint8);
		arrayList2.Add(new PointF(GetXFromValue(chartPoint8, 0), GetYFromValue(chartPoint8, 0)));
		arrayList3.Add(color);
		PointF[] stepPoints = new PointF[arrayList2.Count];
		Color[] array = new Color[arrayList3.Count];
		for (int j = 0; j < arrayList2.Count; j++)
		{
			stepPoints[j] = (PointF)arrayList2[j];
			array[j] = (Color)arrayList3[j];
		}
		if (m_series.GetOfflineStyle().DisplayShadow && !series3D)
		{
			PointF[] array2 = new PointF[stepPoints.Length];
			for (int k = 0; k < stepPoints.Length; k++)
			{
				ChartStyleInfo offlineStyle = m_series.GetOfflineStyle();
				array2[k] = new PointF(stepPoints[k].X + (float)offlineStyle.ShadowOffset.Width, stepPoints[k].Y + (float)offlineStyle.ShadowOffset.Height);
			}
			pen.Color = m_series.GetOfflineStyle().ShadowInterior.ForeColor;
			g.DrawLines(pen, array2);
		}
		if (series3D)
		{
			SizeF seriesOffset = GetSeriesOffset();
			CalculateStepPointsForSeries3D(ref stepPoints);
			Draw3DLines(g, stepPoints, seriesOffset, interior, gdipPen, array);
			return;
		}
		for (int l = 0; l < stepPoints.Length - 1; l++)
		{
			pen.Color = array[l];
			g.DrawLine(pen, stepPoints[l], stepPoints[l + 1]);
		}
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		_ = base.Chart.Series3D;
		_ = m_series.ConfigItems.StepItem.Inverted;
		_ = m_series.GetOfflineStyle().GdipPen;
		_ = m_series.GetOfflineStyle().Interior;
		_ = m_series.GetOfflineStyle().GdipPen;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num = placeDepth;
		g.AddPolygon(CreateBoundsPolygon(num));
		Color priceUpColor = m_series.ConfigItems.FinancialItem.PriceUpColor;
		Color priceDownColor = m_series.ConfigItems.FinancialItem.PriceDownColor;
		double num2 = m_series.ReversalAmount;
		bool reversalIsPercent = m_series.ReversalIsPercent;
		if (reversalIsPercent)
		{
			num2 /= 100.0;
		}
		double num3 = (float)m_series.Points[0].YValues[0];
		double num4 = (float)m_series.Points[0].YValues[0];
		double num5 = (float)m_series.Points[0].YValues[0];
		double num6 = (float)m_series.Points[0].YValues[0];
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		ArrayList arrayList3 = new ArrayList();
		_ = m_series.Points[0].YValues[0];
		bool flag = m_series.Points[0].YValues[0] <= m_series.Points[1].YValues[0];
		double x = m_series.Points[0].X;
		Color color = (flag ? priceUpColor : priceDownColor);
		arrayList2.Add(GetPointFromIndex(0));
		arrayList3.Add(color);
		arrayList.Add(m_series.Points[0]);
		for (int i = 1; i < m_series.Points.Count; i++)
		{
			double num7 = m_series.Points[i].YValues[0];
			if (reversalIsPercent)
			{
				if ((num7 - num5) / Math.Abs(num5) <= 0.0 - num2 && flag)
				{
					ChartPoint chartPoint = new ChartPoint(x, num5);
					arrayList.Add(chartPoint);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint, 0), GetYFromValue(chartPoint, 0)));
					arrayList3.Add(color);
					double x2 = m_series.Points[i].X;
					chartPoint = new ChartPoint(x2, num5);
					arrayList.Add(chartPoint);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint, 0), GetYFromValue(chartPoint, 0)));
					arrayList3.Add(color);
					flag = false;
					x = x2;
					num3 = num5;
					num4 = num6;
					num5 = num3;
					num6 = num7;
				}
				if ((num7 - num6) / Math.Abs(num6) >= num2 && !flag)
				{
					ChartPoint chartPoint2 = new ChartPoint(x, num6);
					arrayList.Add(chartPoint2);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint2, 0), GetYFromValue(chartPoint2, 0)));
					arrayList3.Add(color);
					double x3 = m_series.Points[i].X;
					chartPoint2 = new ChartPoint(x3, num6);
					arrayList.Add(chartPoint2);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint2, 0), GetYFromValue(chartPoint2, 0)));
					arrayList3.Add(color);
					flag = true;
					x = x3;
					num3 = num5;
					num4 = num6;
					num5 = num7;
					num6 = num4;
				}
			}
			else
			{
				if (num7 - num5 <= 0.0 - num2 && flag)
				{
					ChartPoint chartPoint3 = new ChartPoint(x, num5);
					arrayList.Add(chartPoint3);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint3, 0), GetYFromValue(chartPoint3, 0)));
					arrayList3.Add(color);
					double x4 = m_series.Points[i].X;
					chartPoint3 = new ChartPoint(x4, num5);
					arrayList.Add(chartPoint3);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint3, 0), GetYFromValue(chartPoint3, 0)));
					arrayList3.Add(color);
					flag = false;
					x = x4;
					num3 = num5;
					num4 = num6;
					num5 = num3;
					num6 = num7;
				}
				if (num7 - num6 >= num2 && !flag)
				{
					ChartPoint chartPoint4 = new ChartPoint(x, num6);
					arrayList.Add(chartPoint4);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint4, 0), GetYFromValue(chartPoint4, 0)));
					arrayList3.Add(color);
					double x5 = m_series.Points[i].X;
					chartPoint4 = new ChartPoint(x5, num6);
					arrayList.Add(chartPoint4);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint4, 0), GetYFromValue(chartPoint4, 0)));
					arrayList3.Add(color);
					flag = true;
					x = x5;
					num3 = num5;
					num4 = num6;
					num5 = num7;
					num6 = num4;
				}
			}
			if (flag)
			{
				if (num7 > num3 && num6 < num3 && color != priceUpColor)
				{
					ChartPoint chartPoint5 = new ChartPoint(x, num3);
					arrayList.Add(chartPoint5);
					arrayList2.Add(new PointF(GetXFromValue(chartPoint5, 0), GetYFromValue(chartPoint5, 0)));
					color = priceUpColor;
					arrayList3.Add(color);
				}
			}
			else if (num7 < num4 && num5 > num4 && color != priceDownColor)
			{
				ChartPoint chartPoint6 = new ChartPoint(x, num4);
				arrayList.Add(chartPoint6);
				arrayList2.Add(new PointF(GetXFromValue(chartPoint6, 0), GetYFromValue(chartPoint6, 0)));
				color = priceDownColor;
				arrayList3.Add(color);
			}
			if (num7 < num6)
			{
				num6 = num7;
			}
			if (num7 > num5)
			{
				num5 = num7;
			}
		}
		ChartPoint chartPoint7 = m_series.Points[m_series.Points.Count - 1];
		ChartPoint chartPoint8 = new ChartPoint(x, chartPoint7.YValues);
		arrayList.Add(chartPoint8);
		arrayList2.Add(new PointF(GetXFromValue(chartPoint8, 0), GetYFromValue(chartPoint8, 0)));
		arrayList3.Add(color);
		PointF[] array = new PointF[arrayList2.Count];
		Color[] array2 = new Color[arrayList3.Count];
		ChartPoint[] array3 = new ChartPoint[arrayList.Count];
		for (int j = 0; j < arrayList2.Count; j++)
		{
			array[j] = (PointF)arrayList2[j];
			array2[j] = (Color)arrayList3[j];
			array3[j] = (ChartPoint)arrayList[j];
		}
		int count = arrayList3.Count;
		float num8 = float.MinValue;
		float num9 = float.MinValue;
		float num10 = float.MaxValue;
		float num11 = float.MaxValue;
		for (int k = 0; k < count; k++)
		{
			PointF pointF = array[k];
			if (pointF.X > num9)
			{
				num9 = pointF.X;
			}
			if (pointF.Y > num8)
			{
				num8 = pointF.Y;
			}
			if (pointF.X < num11)
			{
				num11 = pointF.X;
			}
			if (pointF.Y > num10)
			{
				num10 = pointF.Y;
			}
		}
		float num12 = num;
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(num11, num10, num12),
			new Vector3D(num9, num10, num12),
			new Vector3D(num9, num8, num12),
			new Vector3D(num11, num8, num12)
		}, (BrushInfo)null, (Pen)null);
		g.AddPolygon(polygon);
		for (int l = 0; l < count - 1; l++)
		{
			PointF pointF2 = array[l];
			PointF pointF3 = array[l + 1];
			ChartPoint chartPoint9 = array3[l];
			ChartPoint chartPoint10 = array3[l + 1];
			if (chartPoint9.X == chartPoint10.X)
			{
				Vector3D vector3D = new Vector3D(pointF2.X, pointF2.Y, num);
				Vector3D vector3D2 = new Vector3D(pointF3.X, pointF3.Y, num);
				Vector3D vector3D3 = new Vector3D(pointF3.X, pointF3.Y, num + seriesDepth);
				Vector3D vector3D4 = new Vector3D(pointF2.X, pointF2.Y, num + seriesDepth);
				Polygon polygon2 = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, new BrushInfo(array2[l]), base.SeriesStyle.GdipPen);
				g.AddPolygon(polygon2);
			}
		}
		for (int m = 0; m < count - 1; m++)
		{
			PointF pointF4 = array[m];
			PointF pointF5 = array[m + 1];
			ChartPoint chartPoint11 = array3[m];
			ChartPoint chartPoint12 = array3[m + 1];
			if (chartPoint11.X != chartPoint12.X)
			{
				Vector3D vector3D5 = new Vector3D(pointF4.X, pointF4.Y, num);
				Vector3D vector3D6 = new Vector3D(pointF5.X, pointF5.Y, num);
				Vector3D vector3D7 = new Vector3D(pointF5.X, pointF5.Y, num + seriesDepth);
				Vector3D vector3D8 = new Vector3D(pointF4.X, pointF4.Y, num + seriesDepth);
				Polygon polygon3 = new Polygon(new Vector3D[4] { vector3D5, vector3D6, vector3D7, vector3D8 }, new BrushInfo(array2[m]), base.SeriesStyle.GdipPen);
				g.AddPolygon(polygon3);
			}
		}
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		base.DrawIcon(g, bounds, isShadow, shadowColor);
		if (!isShadow)
		{
			int num = bounds.Width / 4;
			int num2 = bounds.Height / 4;
			Pen pen = new Pen(m_series.ConfigItems.FinancialItem.PriceUpColor);
			Pen pen2 = new Pen(m_series.ConfigItems.FinancialItem.PriceDownColor);
			g.DrawLines(pen2, new Point[4]
			{
				new Point(bounds.Left + num, bounds.Top),
				new Point(bounds.Left + num, bounds.Top + 3 * num2),
				new Point(bounds.Left + 2 * num, bounds.Top + 3 * num2),
				new Point(bounds.Left + 2 * num, bounds.Top + 2 * num2)
			});
			g.DrawLines(pen, new Point[4]
			{
				new Point(bounds.Left + 2 * num, bounds.Top + 2 * num2),
				new Point(bounds.Left + 2 * num, bounds.Top + num2),
				new Point(bounds.Left + 3 * num, bounds.Top + num2),
				new Point(bounds.Left + 3 * num, bounds.Top)
			});
			pen.Dispose();
			pen2.Dispose();
		}
	}
}
