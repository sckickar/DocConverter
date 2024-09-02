using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class BoxWhiskerRenderer : ColumnRenderer
{
	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Box and Whisker Chart Region";

	public BoxWhiskerRenderer(ChartSeries series)
		: base(series)
	{
	}

	private void DrawPoint(ChartRenderArgs2D args, ChartStyledPoint styledPoint, double axisFactor, DoubleRange sbsInfo, ArrayList pathsList, double dotWidth, RectangleF clip, double dotHeight)
	{
		if (styledPoint.IsVisible)
		{
			double[] array = styledPoint.YValues.Clone() as double[];
			int num = array.Length;
			Array.Sort(array);
			double statisticalMedian = GetStatisticalMedian(array);
			ChartPoint chartPoint;
			ChartPoint chartPoint2;
			if (num % 2 == 0)
			{
				int num2 = num / 2;
				double[] array2 = new double[num2];
				double[] array3 = new double[num2];
				Array.Copy(array, 0, array2, 0, num2);
				Array.Copy(array, num2, array3, 0, num2);
				chartPoint = new ChartPoint(styledPoint.X, GetStatisticalMedian(array2));
				chartPoint2 = new ChartPoint(styledPoint.X, GetStatisticalMedian(array3));
			}
			else
			{
				int num3 = num / 2 + 1;
				double[] array4 = new double[num3];
				double[] array5 = new double[num3];
				Array.Copy(array, 0, array4, 0, num3);
				Array.Copy(array, num3 - 1, array5, 0, num3);
				chartPoint = new ChartPoint(styledPoint.X, GetStatisticalMedian(array4));
				chartPoint2 = new ChartPoint(styledPoint.X, GetStatisticalMedian(array5));
			}
			double num4 = 1.5 * Math.Abs(chartPoint2.YValues[0] - chartPoint.YValues[0]);
			Dictionary<double, int> dictionary = new Dictionary<double, int>();
			Dictionary<double, int> dictionary2 = new Dictionary<double, int>();
			double num9;
			double num16;
			if (m_series.ConfigItems.BoxAndWhiskerItem.PercentileMode)
			{
				double num5 = array[0];
				double percentile = m_series.ConfigItems.BoxAndWhiskerItem.Percentile;
				double num6 = (double)(num - 1) * percentile;
				int num7 = (int)num6;
				double num8 = 0.0;
				num8 = ((num7 == 0) ? num6 : (num6 % (double)num7));
				num5 = ((m_series.ConfigItems.BoxAndWhiskerItem.Percentile != 0.0) ? ((1.0 - num8) * array[num7] + num8 * array[num7 + 1]) : array[0]);
				num9 = num5;
				for (int i = 0; i < num / 2; i++)
				{
					double num10 = array[i];
					if (num10 < num5)
					{
						if (dictionary.ContainsKey(num10))
						{
							dictionary[num10]++;
						}
						else
						{
							dictionary.Add(num10, 1);
						}
					}
				}
				double num11 = array[num - 1];
				double num12 = 1.0 - percentile;
				double num13 = (double)(num - 1) * num12;
				int num14 = (int)num13;
				double num15 = num13 % (double)num14;
				num15 = ((num14 == 0) ? num13 : (num13 % (double)num14));
				num11 = ((m_series.ConfigItems.BoxAndWhiskerItem.Percentile != 0.0) ? ((1.0 - num15) * array[num14] + num15 * array[num14 + 1]) : array[num - 1]);
				num16 = num11;
				for (int num17 = num - 1; num17 >= num / 2; num17--)
				{
					double num18 = array[num17];
					if (num18 > num11)
					{
						if (dictionary2.ContainsKey(num18))
						{
							dictionary2[num18]++;
						}
						else
						{
							dictionary2.Add(num18, 1);
						}
					}
				}
			}
			else
			{
				num9 = array[0];
				for (int j = 0; j < num; j++)
				{
					num9 = array[j];
					if (Math.Abs(num9 - chartPoint.YValues[0]) <= num4)
					{
						break;
					}
				}
				for (int k = 0; k < num / 2; k++)
				{
					double num19 = array[k];
					if (Math.Abs(num19 - chartPoint.YValues[0]) > num4)
					{
						if (dictionary.ContainsKey(num19))
						{
							dictionary[num19]++;
						}
						else
						{
							dictionary.Add(num19, 1);
						}
					}
				}
				num16 = array[num - 1];
				for (int num20 = num - 1; num20 >= 0; num20--)
				{
					num16 = array[num20];
					if (Math.Abs(num16 - chartPoint2.YValues[0]) <= num4)
					{
						break;
					}
				}
				for (int num21 = num - 1; num21 >= num / 2; num21--)
				{
					double num22 = array[num21];
					if (Math.Abs(num22 - chartPoint2.YValues[0]) > num4)
					{
						if (dictionary2.ContainsKey(num22))
						{
							dictionary2[num22]++;
						}
						else
						{
							dictionary2.Add(num22, 1);
						}
					}
				}
			}
			ChartStyleInfo style = styledPoint.Style;
			ChartPoint chartPoint3 = new ChartPoint(styledPoint.X + sbsInfo.Start, statisticalMedian);
			new ChartPoint(styledPoint.X + sbsInfo.Median, num9);
			new ChartPoint(styledPoint.X + sbsInfo.Median, num16);
			PointF point = args.GetPoint(chartPoint.X + sbsInfo.Start, chartPoint.YValues[0]);
			PointF point2 = args.GetPoint(chartPoint2.X + sbsInfo.Start, chartPoint2.YValues[0]);
			PointF point3 = args.GetPoint(chartPoint.X + sbsInfo.End, chartPoint.YValues[0]);
			PointF point4 = args.GetPoint(chartPoint2.X + sbsInfo.End, chartPoint2.YValues[0]);
			PointF point5 = args.GetPoint(chartPoint.X + sbsInfo.Median, chartPoint.YValues[0]);
			PointF point6 = args.GetPoint(chartPoint2.X + sbsInfo.Median, chartPoint2.YValues[0]);
			PointF point7 = args.GetPoint(styledPoint.X + sbsInfo.Start, num9);
			PointF point8 = args.GetPoint(styledPoint.X + sbsInfo.Start, num16);
			PointF point9 = args.GetPoint(styledPoint.X + sbsInfo.Median, num9);
			PointF point10 = args.GetPoint(styledPoint.X + sbsInfo.Median, num16);
			PointF point11 = args.GetPoint(styledPoint.X + sbsInfo.End, num9);
			PointF point12 = args.GetPoint(styledPoint.X + sbsInfo.End, num16);
			_ = m_series.XAxis.Size;
			_ = m_series.XAxis.Location;
			RectangleF rect = args.GetRectangle(chartPoint3.X, chartPoint3.YValues[0], chartPoint.X + sbsInfo.End, chartPoint.YValues[0]);
			RectangleF rect2 = args.GetRectangle(chartPoint3.X, chartPoint3.YValues[0], chartPoint2.X + sbsInfo.End, chartPoint2.YValues[0]);
			CheckColumnBounds(args.IsInvertedAxes, ref rect);
			CheckColumnBounds(args.IsInvertedAxes, ref rect2);
			if (IsFixedWidth)
			{
				float num23 = 0.5f * (float)base.Chart.ColumnFixedWidth;
				if (args.IsInvertedAxes)
				{
					float num25 = (point8.Y = point5.Y - num23);
					float num27 = (point7.Y = num25);
					float y = (point2.Y = num27);
					point.Y = y;
					num25 = (point12.Y = point5.Y + num23);
					num27 = (point11.Y = num25);
					y = (point4.Y = num27);
					point3.Y = y;
				}
				else
				{
					float num25 = (point8.X = point5.X - num23);
					float num27 = (point7.X = num25);
					float y = (point2.X = num27);
					point.X = y;
					num25 = (point12.X = point5.X + num23);
					num27 = (point11.X = num25);
					y = (point4.X = num27);
					point3.X = y;
				}
			}
			if (style.DisplayShadow && !args.Is3D)
			{
				RectangleF rect3 = rect;
				RectangleF rect4 = rect2;
				rect3.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
				rect4.Offset(style.ShadowOffset.Width, style.ShadowOffset.Height);
				args.Graph.DrawRect(style.ShadowInterior, null, rect3);
				args.Graph.DrawRect(style.ShadowInterior, null, rect4);
			}
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(rect);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point5, point9);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point7, point11);
			graphicsPath.CloseFigure();
			foreach (double key in dictionary.Keys)
			{
				int num38 = dictionary[key];
				double num39 = (double)num38 * dotWidth;
				double num40 = styledPoint.X - 0.5 * num39;
				double num41 = key - 0.5 * dotHeight;
				for (int l = 0; l < num38; l++)
				{
					RectangleF rectangle = args.GetRectangle(num40, num41, num40 + dotWidth, num41 + dotHeight);
					graphicsPath.AddEllipse(rectangle);
					num40 += dotWidth;
				}
				num41 += 1.5 * dotHeight;
			}
			graphicsPath.AddRectangle(rect2);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point6, point10);
			graphicsPath.CloseFigure();
			graphicsPath.AddLine(point8, point12);
			graphicsPath.CloseFigure();
			foreach (double key2 in dictionary2.Keys)
			{
				int num42 = dictionary2[key2];
				double num43 = (double)num42 * dotWidth;
				double num44 = styledPoint.X - 0.5 * num43;
				double num45 = key2 - 0.5 * dotHeight;
				for (int m = 0; m < num42; m++)
				{
					RectangleF rectangle2 = args.GetRectangle(num44, num45, num44 + dotWidth, num45 + dotHeight);
					graphicsPath.AddEllipse(rectangle2);
					num44 += dotWidth;
				}
				num45 += 1.5 * dotHeight;
			}
			if (args.Is3D)
			{
				graphicsPath.AddPolygon(new PointF[4]
				{
					new PointF(rect2.Right, rect2.Top),
					new PointF(rect2.Right + args.DepthOffset.Width, rect2.Top + args.DepthOffset.Height),
					new PointF(rect2.Right + args.DepthOffset.Width, rect2.Bottom + args.DepthOffset.Height),
					new PointF(rect2.Right, rect2.Bottom)
				});
				graphicsPath.AddPolygon(new PointF[4]
				{
					new PointF(rect2.Left, rect2.Top),
					new PointF(rect2.Left + args.DepthOffset.Width, rect2.Top + args.DepthOffset.Height),
					new PointF(rect2.Right + args.DepthOffset.Width, rect2.Top + args.DepthOffset.Height),
					new PointF(rect2.Right, rect2.Top)
				});
				if (m_series.Rotate)
				{
					graphicsPath.AddPolygon(new PointF[4]
					{
						new PointF(rect.Left, rect.Top),
						new PointF(rect.Left + args.DepthOffset.Width, rect.Top + args.DepthOffset.Height),
						new PointF(rect.Right + args.DepthOffset.Width, rect.Top + args.DepthOffset.Height),
						new PointF(rect.Right, rect.Top)
					});
				}
				else
				{
					graphicsPath.AddPolygon(new PointF[4]
					{
						new PointF(rect.Right, rect.Top),
						new PointF(rect.Right + args.DepthOffset.Width, rect.Top + args.DepthOffset.Height),
						new PointF(rect.Right + args.DepthOffset.Width, rect.Bottom + args.DepthOffset.Height),
						new PointF(rect.Right, rect.Bottom)
					});
				}
			}
			ChartSeriesPath chartSeriesPath = new ChartSeriesPath();
			chartSeriesPath.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style));
			chartSeriesPath.Bounds = rect;
			ChartSeriesPath chartSeriesPath2 = new ChartSeriesPath();
			chartSeriesPath2.AddPrimitive(graphicsPath, style.GdipPen, GetBrush(style));
			chartSeriesPath2.Bounds = rect2;
			if (args.Is3D)
			{
				pathsList.Add(chartSeriesPath);
				pathsList.Add(chartSeriesPath2);
			}
			else
			{
				chartSeriesPath.Draw(args.Graph);
				chartSeriesPath2.Draw(args.Graph);
			}
		}
		ChartSegment[] segments = (ChartSeriesPath[])pathsList.ToArray(typeof(ChartSeriesPath));
		m_segments = segments;
	}

	private void RenderSeries(ChartRenderArgs2D args, ChartStyledPoint styledPoint)
	{
		double num = (double)m_series.ActualXAxis.RealLength * m_series.ActualYAxis.VisibleRange.Delta / (m_series.ActualXAxis.VisibleRange.Delta * (double)m_series.ActualYAxis.RealLength);
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		ArrayList pathsList = new ArrayList();
		double num2 = 0.25 * sideBySideInfo.Delta;
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		double dotHeight = num2 * num;
		if (m_series.ConfigItems.BoxAndWhiskerItem.OutLierWidth != 0.0)
		{
			num2 = m_series.ConfigItems.BoxAndWhiskerItem.OutLierWidth / 100.0;
		}
		if (styledPoint == null)
		{
			ChartStyledPoint[] array = PrepearePoints();
			IndexRange indexRange = CalculateVisibleRange();
			int i = indexRange.From;
			for (int num3 = indexRange.To + 1; i < num3; i++)
			{
				DrawPoint(args, array[i], num, sideBySideInfo, pathsList, num2, clipBounds, dotHeight);
			}
		}
		else
		{
			DrawPoint(args, styledPoint, num, sideBySideInfo, pathsList, num2, clipBounds, dotHeight);
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
		double num = (double)m_series.ActualXAxis.RealLength * m_series.ActualYAxis.VisibleRange.Delta / (m_series.ActualXAxis.VisibleRange.Delta * (double)m_series.ActualYAxis.RealLength);
		DoubleRange sideBySideInfo = GetSideBySideInfo();
		_ = m_series.YAxis.Inversed;
		double num2 = 0.25 * sideBySideInfo.Delta;
		if (m_series.ConfigItems.BoxAndWhiskerItem.OutLierWidth != 0.0)
		{
			num2 = m_series.ConfigItems.BoxAndWhiskerItem.OutLierWidth / 100.0;
		}
		double num3 = num2 * num;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num4 = placeDepth;
		args.Graph.AddPolygon(CreateBoundsPolygon(num4));
		PrepearePoints();
		IndexRange indexRange = CalculateVisibleRange();
		new ArrayList();
		ChartStyledPoint chartStyledPoint = null;
		int i = indexRange.From;
		for (int num5 = indexRange.To + 1; i < num5; i++)
		{
			chartStyledPoint = base.StyledPoints[i];
			if (!chartStyledPoint.IsVisible)
			{
				continue;
			}
			double[] array = chartStyledPoint.YValues.Clone() as double[];
			int num6 = array.Length;
			Array.Sort(array);
			ChartPoint chartPoint = new ChartPoint(chartStyledPoint.X, GetStatisticalMedian(array));
			ChartPoint chartPoint2;
			ChartPoint chartPoint3;
			if (num6 % 2 == 0)
			{
				int num7 = num6 / 2;
				double[] array2 = new double[num7];
				double[] array3 = new double[num7];
				Array.Copy(array, 0, array2, 0, num7);
				Array.Copy(array, num7, array3, 0, num7);
				chartPoint2 = new ChartPoint(chartStyledPoint.X, GetStatisticalMedian(array2));
				chartPoint3 = new ChartPoint(chartStyledPoint.X, GetStatisticalMedian(array3));
			}
			else
			{
				int num8 = num6 / 2 + 1;
				double[] array4 = new double[num8];
				double[] array5 = new double[num8];
				Array.Copy(array, 0, array4, 0, num8);
				Array.Copy(array, num8 - 1, array5, 0, num8);
				chartPoint2 = new ChartPoint(chartStyledPoint.X, GetStatisticalMedian(array4));
				chartPoint3 = new ChartPoint(chartStyledPoint.X, GetStatisticalMedian(array5));
			}
			double num9 = Math.Abs(chartPoint3.YValues[0] - chartPoint2.YValues[0]);
			Dictionary<double, int> dictionary = new Dictionary<double, int>();
			Dictionary<double, int> dictionary2 = new Dictionary<double, int>();
			ChartPoint chartPoint4;
			ChartPoint chartPoint5;
			if (m_series.ConfigItems.BoxAndWhiskerItem.PercentileMode)
			{
				double num10 = array[0];
				double percentile = m_series.ConfigItems.BoxAndWhiskerItem.Percentile;
				double num11 = (double)(num6 - 1) * percentile;
				int num12 = (int)num11;
				double num13 = 0.0;
				num13 = ((num12 == 0) ? num11 : (num11 % (double)num12));
				num10 = ((m_series.ConfigItems.BoxAndWhiskerItem.Percentile != 0.0) ? ((1.0 - num13) * array[num12] + num13 * array[num12 + 1]) : array[0]);
				chartPoint4 = new ChartPoint(chartStyledPoint.X, num10);
				for (int j = 0; j < num6 / 2; j++)
				{
					double num14 = array[j];
					if (num14 < num10)
					{
						if (dictionary.ContainsKey(num14))
						{
							dictionary[num14]++;
						}
						else
						{
							dictionary.Add(num14, 1);
						}
					}
				}
				double num15 = array[num6 - 1];
				double num16 = 1.0 - percentile;
				double num17 = (double)(num6 - 1) * num16;
				int num18 = (int)num17;
				double num19 = num17 % (double)num18;
				num19 = ((num18 == 0) ? num17 : (num17 % (double)num18));
				num15 = ((m_series.ConfigItems.BoxAndWhiskerItem.Percentile != 0.0) ? ((1.0 - num19) * array[num18] + num19 * array[num18 + 1]) : array[num6 - 1]);
				chartPoint5 = new ChartPoint(chartStyledPoint.X, num15);
				for (int num20 = num6 - 1; num20 >= num6 / 2; num20--)
				{
					double num21 = array[num20];
					if (num21 > num15)
					{
						if (dictionary2.ContainsKey(num21))
						{
							dictionary2[num21]++;
						}
						else
						{
							dictionary2.Add(num21, 1);
						}
					}
				}
			}
			else
			{
				chartPoint4 = new ChartPoint(chartStyledPoint.X, array[0]);
				for (int k = 0; k < num6; k++)
				{
					double y = array[k];
					chartPoint4 = new ChartPoint(chartStyledPoint.X, y);
					if (Math.Abs(chartPoint4.YValues[0] - chartPoint2.YValues[0]) <= 1.5 * num9)
					{
						break;
					}
				}
				for (int l = 0; l < num6 / 2; l++)
				{
					double y2 = array[l];
					ChartPoint chartPoint6 = new ChartPoint(chartStyledPoint.X, y2);
					if (Math.Abs(chartPoint6.YValues[0] - chartPoint2.YValues[0]) > 1.5 * num9)
					{
						if (dictionary.ContainsKey(chartPoint6.YValues[0]))
						{
							int num22 = dictionary[chartPoint6.YValues[0]];
							num22++;
							dictionary[chartPoint6.YValues[0]] = num22;
						}
						else
						{
							dictionary.Add(chartPoint6.YValues[0], 1);
						}
					}
				}
				chartPoint5 = new ChartPoint(chartStyledPoint.X, array[num6 - 1]);
				for (int num23 = num6 - 1; num23 >= 0; num23--)
				{
					double y3 = array[num23];
					chartPoint5 = new ChartPoint(chartStyledPoint.X, y3);
					if (Math.Abs(chartPoint5.YValues[0] - chartPoint3.YValues[0]) <= 1.5 * num9)
					{
						break;
					}
				}
				for (int num24 = num6 - 1; num24 >= num6 / 2; num24--)
				{
					double y4 = array[num24];
					ChartPoint chartPoint7 = new ChartPoint(chartStyledPoint.X, y4);
					if (Math.Abs(chartPoint7.YValues[0] - chartPoint3.YValues[0]) > 1.5 * num9)
					{
						if (dictionary2.ContainsKey(chartPoint7.YValues[0]))
						{
							int num25 = dictionary2[chartPoint7.YValues[0]];
							num25++;
							dictionary2[chartPoint7.YValues[0]] = num25;
						}
						else
						{
							dictionary2.Add(chartPoint7.YValues[0], 1);
						}
					}
				}
			}
			ChartStyleInfo style = chartStyledPoint.Style;
			ChartPoint chartPoint8 = new ChartPoint(chartStyledPoint.X + sideBySideInfo.Start, chartPoint.YValues);
			new ChartPoint(chartStyledPoint.X + sideBySideInfo.End, chartPoint.YValues);
			new ChartPoint(chartPoint4.X + sideBySideInfo.Median, chartPoint4.YValues);
			new ChartPoint(chartPoint5.X + sideBySideInfo.Median, chartPoint5.YValues);
			PointF point = args.GetPoint(chartPoint2.X + sideBySideInfo.Start, chartPoint2.YValues[0]);
			PointF point2 = args.GetPoint(chartPoint3.X + sideBySideInfo.Start, chartPoint3.YValues[0]);
			PointF point3 = args.GetPoint(chartPoint2.X + sideBySideInfo.End, chartPoint2.YValues[0]);
			PointF point4 = args.GetPoint(chartPoint3.X + sideBySideInfo.End, chartPoint3.YValues[0]);
			PointF point5 = args.GetPoint(chartPoint2.X + sideBySideInfo.Median, chartPoint2.YValues[0]);
			PointF point6 = args.GetPoint(chartPoint3.X + sideBySideInfo.Median, chartPoint3.YValues[0]);
			PointF point7 = args.GetPoint(chartPoint4.X + sideBySideInfo.Start, chartPoint4.YValues[0]);
			PointF point8 = args.GetPoint(chartPoint5.X + sideBySideInfo.Start, chartPoint5.YValues[0]);
			PointF point9 = args.GetPoint(chartPoint4.X + sideBySideInfo.Median, chartPoint4.YValues[0]);
			PointF point10 = args.GetPoint(chartPoint5.X + sideBySideInfo.Median, chartPoint5.YValues[0]);
			PointF point11 = args.GetPoint(chartPoint4.X + sideBySideInfo.End, chartPoint4.YValues[0]);
			PointF point12 = args.GetPoint(chartPoint5.X + sideBySideInfo.End, chartPoint5.YValues[0]);
			if (IsFixedWidth)
			{
				float num26 = 0.5f * (float)base.Chart.ColumnFixedWidth;
				if (args.IsInvertedAxes)
				{
					float num28 = (point8.Y = point5.Y - num26);
					float num30 = (point7.Y = num28);
					float y5 = (point2.Y = num30);
					point.Y = y5;
					num28 = (point12.Y = point5.Y + num26);
					num30 = (point11.Y = num28);
					y5 = (point4.Y = num30);
					point3.Y = y5;
				}
				else
				{
					float num28 = (point8.X = point5.X - num26);
					float num30 = (point7.X = num28);
					float y5 = (point2.X = num30);
					point.X = y5;
					num28 = (point12.X = point5.X + num26);
					num30 = (point11.X = num28);
					y5 = (point4.X = num30);
					point3.X = y5;
				}
			}
			_ = m_series.XAxis.Size;
			_ = m_series.XAxis.Location;
			RectangleF rect = args.GetRectangle(chartPoint8.X, chartPoint8.YValues[0], chartPoint2.X + sideBySideInfo.End, chartPoint2.YValues[0]);
			RectangleF rect2 = args.GetRectangle(chartPoint8.X, chartPoint8.YValues[0], chartPoint3.X + sideBySideInfo.End, chartPoint3.YValues[0]);
			CheckColumnBounds(args.IsInvertedAxes, ref rect);
			CheckColumnBounds(args.IsInvertedAxes, ref rect2);
			BrushInfo brush = GetBrush(chartStyledPoint.Index);
			if (!args.IsInvertedAxes)
			{
				args.Graph.CreateBoxV(new Vector3D(rect.Left, rect.Top, num4), new Vector3D(rect.Right, rect.Bottom, num4 + seriesDepth), style.GdipPen, brush);
				args.Graph.CreateBoxV(new Vector3D(rect2.Left, rect2.Top, num4), new Vector3D(rect2.Right, rect2.Bottom, num4 + seriesDepth), style.GdipPen, brush);
			}
			else
			{
				args.Graph.CreateBox(new Vector3D(rect.Left, rect.Top, num4), new Vector3D(rect.Right, rect.Bottom, num4 + seriesDepth), style.GdipPen, brush);
				args.Graph.CreateBox(new Vector3D(rect2.Left, rect2.Top, num4), new Vector3D(rect2.Right, rect2.Bottom, num4 + seriesDepth), style.GdipPen, brush);
			}
			Polygon polygon = new Polygon(new Vector3D[4]
			{
				new Vector3D(point7.X, point7.Y, num4),
				new Vector3D(point11.X, point11.Y, num4),
				new Vector3D(point11.X, point11.Y, num4 + seriesDepth),
				new Vector3D(point7.X, point7.Y, num4 + seriesDepth)
			}, brush, style.GdipPen);
			args.Graph.AddPolygon(polygon);
			Polygon polygon2 = new Polygon(new Vector3D[4]
			{
				new Vector3D(point5.X, point5.Y, num4),
				new Vector3D(point9.X, point9.Y, num4),
				new Vector3D(point9.X, point9.Y, num4 + seriesDepth),
				new Vector3D(point5.X, point5.Y, num4 + seriesDepth)
			}, brush, style.GdipPen);
			args.Graph.AddPolygon(polygon2);
			_ = sideBySideInfo.Delta;
			foreach (double key in dictionary.Keys)
			{
				int num41 = dictionary[key];
				double num42 = (double)num41 * num2;
				double num43 = chartStyledPoint.X - 0.5 * num42;
				double num44 = key - 0.5 * num3;
				for (int m = 0; m < num41; m++)
				{
					RectangleF rectangle = args.GetRectangle(num43, num44, num43 + num2, num44 + num3);
					args.Graph.CreateEllipse(new Vector3D(rectangle.X, rectangle.Y, num4), new SizeF(rectangle.Width, rectangle.Height), 10, style.GdipPen, brush);
					num43 += num2;
				}
				num44 += 1.5 * num3;
			}
			Polygon polygon3 = new Polygon(new Vector3D[4]
			{
				new Vector3D(point8.X, point8.Y, num4),
				new Vector3D(point12.X, point12.Y, num4),
				new Vector3D(point12.X, point12.Y, num4 + seriesDepth),
				new Vector3D(point8.X, point8.Y, num4 + seriesDepth)
			}, brush, style.GdipPen);
			args.Graph.AddPolygon(polygon3);
			Polygon polygon4 = new Polygon(new Vector3D[4]
			{
				new Vector3D(point6.X, point6.Y, num4),
				new Vector3D(point10.X, point10.Y, num4),
				new Vector3D(point10.X, point10.Y, num4 + seriesDepth),
				new Vector3D(point6.X, point6.Y, num4 + seriesDepth)
			}, brush, style.GdipPen);
			args.Graph.AddPolygon(polygon4);
			foreach (double key2 in dictionary2.Keys)
			{
				int num45 = dictionary2[key2];
				double num46 = (double)num45 * num2;
				double num47 = chartStyledPoint.X - 0.5 * num46;
				double num48 = key2 - 0.5 * num3;
				for (int n = 0; n < num45; n++)
				{
					RectangleF rectangle2 = args.GetRectangle(num47, num48, num47 + num2, num48 + num3);
					args.Graph.CreateEllipse(new Vector3D(rectangle2.X, rectangle2.Y, num4), new SizeF(rectangle2.Width, rectangle2.Height), 10, style.GdipPen, brush);
					num47 += num2;
				}
				num48 += 1.5 * num3;
			}
		}
	}

	public override DoubleRange GetYDataMeasure()
	{
		if (m_series.Points.Count > 0)
		{
			double num = double.MinValue;
			double num2 = double.MaxValue;
			for (int i = 0; i < m_series.Points.Count; i++)
			{
				double[] yValues = m_series.Points[i].YValues;
				int j = 0;
				for (int num3 = yValues.Length; j < num3; j++)
				{
					if (yValues[j] > num)
					{
						num = yValues[j];
					}
					if (yValues[j] < num2)
					{
						num2 = yValues[j];
					}
				}
			}
			return new DoubleRange(num2, num);
		}
		return DoubleRange.Empty;
	}

	private double GetStatisticalMedian(double[] values)
	{
		int num = values.Length;
		if (num % 2 == 1)
		{
			return values[(num - 1) / 2];
		}
		return 0.5 * (values[num / 2] + values[num / 2 - 1]);
	}
}
