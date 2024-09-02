using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocGen.Chart;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.ChartToImageConverter;

internal class ChartToImageconverter
{
	private delegate ChartSeries ChartConverterMethod(ChartSerieImpl serieImpl, ChartControl chart);

	private ChartSeriesFactory converter = new ChartSeriesFactory();

	private ChartImpl _chart;

	private bool Is3DConversionSupported()
	{
		switch (_chart.ChartType)
		{
		case OfficeChartType.Column_Clustered_3D:
		case OfficeChartType.Column_Stacked_3D:
		case OfficeChartType.Column_Stacked_100_3D:
		case OfficeChartType.Column_3D:
		case OfficeChartType.Bar_Clustered_3D:
		case OfficeChartType.Bar_Stacked_3D:
		case OfficeChartType.Bar_Stacked_100_3D:
		case OfficeChartType.Pie_3D:
		case OfficeChartType.Pie_Exploded_3D:
		case OfficeChartType.Area_3D:
		case OfficeChartType.Area_Stacked_3D:
		case OfficeChartType.Area_Stacked_100_3D:
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cylinder_Clustered_3D:
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Cone_Bar_Stacked_100:
		case OfficeChartType.Cone_Clustered_3D:
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Stacked:
		case OfficeChartType.Pyramid_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Stacked:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
		case OfficeChartType.Pyramid_Clustered_3D:
			return true;
		default:
			return false;
		}
	}

	private bool IsAutoChartTitle(ChartImpl chartImpl)
	{
		return false;
	}

	internal ChartControl GetChart(IOfficeChart excelChart)
	{
		bool isPie = false;
		bool isStock = false;
		bool isRadar = false;
		converter.IsChart3D = false;
		converter.parentWorkbook = _chart.ParentWorkbook;
		ChartControl chartControl = new ChartControl();
		converter.SfChart = chartControl;
		converter.IntializeFonts();
		chartControl.Font = new Font(_chart.Font.FontName, (float)_chart.Font.Size);
		bool flag = _chart.ChartType.ToString().Contains("Combination_Chart");
		converter.IsChartEx = ChartImpl.IsChartExSerieType(_chart.ChartType);
		bool flag2 = converter.IsBarChartAxis(_chart.PrimaryValueAxis as ChartAxisImpl);
		if (_chart.ChartType == OfficeChartType.Funnel)
		{
			flag2 = true;
		}
		ChartAxis chartAxis = (flag2 ? chartControl.PrimaryXAxis : chartControl.PrimaryYAxis);
		ChartAxis chartAxis2 = (flag2 ? chartControl.PrimaryYAxis : chartControl.PrimaryXAxis);
		if (converter.IsChartEx)
		{
			if (_chart.ChartType == OfficeChartType.Funnel)
			{
				chartControl.PrimaryYAxis.ValueType = ChartValueType.Category;
			}
			else
			{
				chartControl.PrimaryXAxis.ValueType = ChartValueType.Category;
			}
		}
		else
		{
			if (Array.IndexOf(ChartImpl.CHARTS_SCATTER, _chart.ChartType) != -1 || Array.IndexOf(ChartImpl.CHARTS_BUBBLE, _chart.ChartType) != -1)
			{
				chartAxis2.ValueType = ChartValueType.Double;
			}
			else if ((_chart.PrimaryCategoryAxis as ChartCategoryAxisImpl).IsChartBubbleOrScatter)
			{
				chartAxis2.ValueType = ChartValueType.Double;
			}
			else if (_chart.PrimaryCategoryAxis.CategoryType == OfficeCategoryType.Time && !_chart.IsStringRef)
			{
				chartAxis2.ValueType = ChartValueType.DateTime;
			}
			else
			{
				chartAxis2.ValueType = ChartValueType.Category;
			}
			if (_chart.PrimaryValueAxis.IsLogScale)
			{
				chartAxis.ValueType = ChartValueType.Logarithmic;
			}
			else
			{
				chartAxis.ValueType = ChartValueType.Double;
			}
		}
		List<IOfficeChartSerie> list = GetOfficeChartSeries(_chart.Series);
		int index = 0;
		int index2 = 0;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < list.Count; i++)
		{
			ChartSerieImpl chartSerieImpl = list[i] as ChartSerieImpl;
			if (chartSerieImpl.EnteredDirectlyCategoryLabels != null && chartSerieImpl.EnteredDirectlyValues != null && _chart.PrimaryCategoryAxis.AutoTickLabelSpacing)
			{
				int num = chartSerieImpl.EnteredDirectlyCategoryLabels.Length;
				if (num >= 50 && chartSerieImpl.EnteredDirectlyCategoryLabels.Length == chartSerieImpl.EnteredDirectlyValues.Length && !_chart.IsChartScatter)
				{
					int num2 = num / 50 + 1;
					int num3 = 0;
					List<object> list2 = new List<object>();
					List<object> list3 = new List<object>();
					list2.Add(chartSerieImpl.EnteredDirectlyCategoryLabels[num3]);
					list3.Add(chartSerieImpl.EnteredDirectlyValues[num3]);
					for (int j = 0; j < 50; j++)
					{
						num3 += num2;
						if (num3 < num)
						{
							list2.Add(chartSerieImpl.EnteredDirectlyCategoryLabels[num3]);
							list3.Add(chartSerieImpl.EnteredDirectlyValues[num3]);
						}
					}
					chartSerieImpl.EnteredDirectlyCategoryLabels = list2.ToArray();
					chartSerieImpl.EnteredDirectlyValues = list3.ToArray();
					chartSerieImpl.ValuesIRange = null;
					chartSerieImpl.CategoryLabelsIRange = null;
				}
			}
			if (chartSerieImpl.EnteredDirectlyCategoryLabels != null && chartSerieImpl.EnteredDirectlyValues != null && chartSerieImpl.EnteredDirectlyCategoryLabels.Length >= 50 && chartSerieImpl.EnteredDirectlyCategoryLabels.Length == chartSerieImpl.EnteredDirectlyValues.Length && chartSerieImpl.UsePrimaryAxis && !_chart.IsChartScatter)
			{
				(_chart.PrimaryCategoryAxis as ChartCategoryAxisImpl).ChangeDateTimeAxisValue = true;
			}
			if (chartSerieImpl.EnteredDirectlyCategoryLabels != null && chartSerieImpl.UsePrimaryAxis && !flag4)
			{
				index = i;
				flag4 = true;
			}
			else if (chartSerieImpl.EnteredDirectlyCategoryLabels != null && !chartSerieImpl.UsePrimaryAxis && !flag3)
			{
				index2 = i;
				flag3 = true;
			}
			else if (flag4 && flag3)
			{
				break;
			}
		}
		bool flag5 = false;
		if (flag)
		{
			list = _chart.Series.OrderByType();
			if (list.Count == 0 && _chart.Series.Count == _chart.Series.Count((IOfficeChartSerie x) => x.SerieType.ToString().Contains("Bubble")))
			{
				list = _chart.Series.ToList();
			}
		}
		else if (_chart.Series.Count > 1)
		{
			string text = _chart.ChartType.ToString();
			ChartFormatImpl chartFormatImpl = _chart.Series[0].SerieFormat.CommonSerieOptions as ChartFormatImpl;
			if ((chartFormatImpl.FormatRecordType == TBIFFRecord.ChartArea && !chartFormatImpl.IsStacked) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartScatter && !chartFormatImpl.IsBubbles) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartLine && !text.Contains("Stock") && !text.Contains("Stacked_100")) || chartFormatImpl.FormatRecordType == TBIFFRecord.ChartRadar || chartFormatImpl.FormatRecordType == TBIFFRecord.ChartRadarArea || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartBar && !chartFormatImpl.StackValuesBar && _chart.PrimaryCategoryAxis.ReversePlotOrder))
			{
				list.Reverse();
				flag5 = true;
				converter.IsSeriesReverseOrder = true;
			}
		}
		ChartAxis chartAxis3 = null;
		ChartAxis chartAxis4 = null;
		foreach (IOfficeChartSerie item in list)
		{
			if (item.IsFiltered && !converter.IsChartEx)
			{
				continue;
			}
			if (flag5)
			{
				(item as ChartSerieImpl).Reversed = true;
			}
			ChartSerieImpl chartSerieImpl2 = item as ChartSerieImpl;
			if (chartSerieImpl2.UsePrimaryAxis)
			{
				if (flag4 && chartSerieImpl2.EnteredDirectlyCategoryLabels == null && (list[index] as ChartSerieImpl).EnteredDirectlyCategoryLabels != null)
				{
					chartSerieImpl2.EnteredDirectlyCategoryLabels = (list[index] as ChartSerieImpl).EnteredDirectlyCategoryLabels;
				}
			}
			else if (!chartSerieImpl2.UsePrimaryAxis && flag3 && chartSerieImpl2.EnteredDirectlyCategoryLabels == null && (list[index2] as ChartSerieImpl).EnteredDirectlyCategoryLabels != null)
			{
				chartSerieImpl2.EnteredDirectlyCategoryLabels = (list[index2] as ChartSerieImpl).EnteredDirectlyCategoryLabels;
			}
			bool isNullSerie = chartSerieImpl2.ValuesIRange == null && chartSerieImpl2.EnteredDirectlyValues == null;
			if (isRadar && flag && (_chart.PrimaryFormats.Count != 1 || _chart.SecondaryFormats.Count != 0))
			{
				break;
			}
			GetChartSerie(flag ? item.SerieType : _chart.ChartType, chartControl, chartSerieImpl2, out isPie, out isStock, out isRadar, isNullSerie);
			if (chartControl.Series.Count > 0 && !converter.IsChart3D && !item.UsePrimaryAxis)
			{
				if (chartAxis3 != null || chartAxis4 != null)
				{
					ChartSeries chartSeries = chartControl.Series[chartControl.Series.Count - 1];
					chartSeries.XAxis = chartAxis3;
					chartSeries.YAxis = chartAxis4;
				}
				else
				{
					ChartSeries chartSeries2 = chartControl.Series[chartControl.Series.Count - 1];
					chartAxis3 = chartSeries2.XAxis;
					chartAxis4 = chartSeries2.YAxis;
				}
			}
			if (isPie || converter.IsChartEx)
			{
				break;
			}
		}
		if (isStock)
		{
			for (int k = 1; k < chartControl.Series.Count; k++)
			{
				chartControl.Series[k].Visible = false;
			}
		}
		foreach (ChartSeries item2 in chartControl.Series)
		{
			if (item2.SortBy == ChartSeriesSortingType.X && item2.XAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (item2.SortBy == ChartSeriesSortingType.Y && item2.YAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if ((item2.Type == ChartSeriesType.Bar || item2.Type == ChartSeriesType.StackingBar || item2.Type == ChartSeriesType.StackingBar100) && item2.SortBy == ChartSeriesSortingType.X && item2.YAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (item2.Type == ChartSeriesType.ColumnRange && item2.Rotate)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (((item2.SortBy == ChartSeriesSortingType.X && item2.XAxis.ValueType == ChartValueType.Double) || (item2.SortBy == ChartSeriesSortingType.Y && item2.YAxis.ValueType == ChartValueType.Double)) && item2.Type == ChartSeriesType.Scatter && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
		}
		RectangleF manualRect = new RectangleF(-1f, -1f, 0f, 0f);
		if (_chart.HasTitle || IsAutoChartTitle(_chart))
		{
			converter.SfChartTitle(_chart, chartControl, out manualRect);
		}
		else
		{
			chartControl.Title.Visible = false;
		}
		if (_chart.Series.Count > 0)
		{
			if ((!(isRadar && flag) || !converter.SecondayAxisAchived) && !isPie)
			{
				ChartCategoryAxisImpl chartCategoryAxisImpl = _chart.PrimaryCategoryAxis as ChartCategoryAxisImpl;
				ChartValueAxisImpl chartValueAxisImpl = _chart.PrimaryValueAxis as ChartValueAxisImpl;
				if (!converter.IsChartEx && chartValueAxisImpl.IsLogScale)
				{
					converter.SfLogerthmicAxis(chartAxis, chartValueAxisImpl, chartCategoryAxisImpl);
				}
				else if (_chart.ChartType == OfficeChartType.Funnel)
				{
					chartAxis.DrawGrid = false;
					chartAxis.DrawTickLabelGrid = false;
					chartAxis.IsVisible = false;
					chartAxis2.Inversed = true;
				}
				else
				{
					if (converter.IsChartEx)
					{
						chartAxis.ValueType = ChartValueType.Double;
					}
					converter.SfNumericalAxis(chartAxis, chartValueAxisImpl, chartCategoryAxisImpl, condition: true);
				}
				if (chartAxis2.ValueType == ChartValueType.Category)
				{
					converter.SfCategoryAxis(chartAxis, chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl, condition: true);
				}
				else if (chartAxis2.ValueType == ChartValueType.DateTime)
				{
					if (chartCategoryAxisImpl.ChangeDateTimeAxisValue)
					{
						chartAxis2.ChangeDateTimeAxisValue = true;
					}
					if (converter.FirstSeriesPoints != null && converter.FirstSeriesPoints.Count > 0 && converter.FirstSeriesPoints[0].X is DateTime)
					{
						converter.SfDateTimeAxis(chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl);
					}
					else
					{
						chartAxis2.ValueType = ChartValueType.Category;
						converter.SfCategoryAxis(chartAxis, chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl, condition: true);
					}
				}
				else
				{
					converter.SfNumericalAxis(chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl, condition: true);
				}
			}
			if (isRadar)
			{
				int num4 = 0;
				if (chartAxis2 != null && chartAxis2.AxisLabelConverter != null && chartAxis2.AxisLabelConverter.CustomRadarAxisLabels != null)
				{
					string value = chartAxis2.AxisLabelConverter.CustomRadarAxisLabels.OrderByDescending((KeyValuePair<double, string> s) => s.Value.Length).First().Value;
					SizeF sizeF = Graphics.FromImage(new Bitmap(chartControl.Size.Width, chartControl.Size.Height)).MeasureString(value, chartAxis.Font);
					float num5 = (float)converter.ChartWidth / 4.16667f;
					float num6 = (float)converter.ChartHeight / 6.16667f;
					if (sizeF.Width < num5)
					{
						num5 = sizeF.Width;
					}
					if (sizeF.Height < num6)
					{
						num6 = sizeF.Height;
					}
					chartControl.ChartAreaMargins.Left += (int)num5;
					chartControl.ChartAreaMargins.Right += (int)num5;
					chartControl.ChartAreaMargins.Top += (int)num6;
					chartControl.ChartAreaMargins.Bottom += (int)num6;
				}
				foreach (ChartSeries item3 in chartControl.Series)
				{
					if (num4 < item3.Points.Count)
					{
						num4 = item3.Points.Count;
					}
				}
				chartControl.PrimaryXAxis.Range = new MinMaxInfo(1.0, num4 + 1, 1.0);
				chartControl.PrimaryXAxis.ValueType = ChartValueType.Double;
				chartControl.PrimaryXAxis.Inversed = true;
				chartControl.IsRadar = true;
			}
		}
		else
		{
			chartAxis2.IsVisible = false;
			chartAxis.IsVisible = false;
			chartAxis2.DrawGrid = false;
			chartAxis.DrawGrid = false;
		}
		if (converter.SecondayAxisAchived)
		{
			if (chartAxis3 != null && chartAxis3.IsVisible)
			{
				_ = excelChart.SecondaryCategoryAxis;
				_ = excelChart.SecondaryValueAxis;
			}
			if (chartAxis4 != null && chartAxis4.IsVisible)
			{
				_ = excelChart.SecondaryCategoryAxis;
				_ = excelChart.SecondaryValueAxis;
			}
		}
		int[] sortedLegendOrders = null;
		ChartLegend emptyLegend = null;
		if (_chart.HasLegend)
		{
			converter.SfLegend(chartControl, _chart, chartControl.Legend, out sortedLegendOrders, !(manualRect.X < 0f) && !(manualRect.Y < 0f), out emptyLegend);
		}
		else
		{
			chartControl.Legend.Visible = false;
			chartControl.Legend.DockingFree = true;
		}
		if (_chart.HasPlotArea && _chart.Series.Count > 0)
		{
			converter.SfPloatArea(chartControl, _chart.PlotArea as ChartFrameFormatImpl, _chart);
		}
		else if (converter.IsChartEx && !_chart.HasPlotArea)
		{
			chartControl.ChartArea.BorderWidth = 0;
		}
		converter.SfChartArea(chartControl, _chart.ChartArea as ChartFrameFormatImpl, _chart);
		bool flag6 = false;
		IEnumerator enumerator2 = chartControl.Series.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext() && ((ChartSeries)enumerator2.Current).Type == ChartSeriesType.Bubble)
			{
				flag6 = true;
			}
		}
		finally
		{
			IDisposable disposable = enumerator2 as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		if (flag6)
		{
			chartControl.Size = new Size(converter.ChartWidth, converter.ChartHeight);
			int count = chartControl.Series.Count;
			for (int l = 0; l < count; l++)
			{
				chartControl.Series[l].ZOrder = count - (l + 1);
			}
			chartControl.PrepareAxesSeriesAndChart();
			chartControl.RecalculateSizes(Graphics.FromImage(new Bitmap(chartControl.Size.Width, chartControl.Size.Height)));
			SetBubbleSizeInCharts(chartControl, _chart, flag);
		}
		if (flag)
		{
			chartControl.InvertedSeriesIsCompatible = true;
		}
		return chartControl;
	}

	private void SetBubbleSizeInCharts(ChartControl sfChart, ChartImpl chartImpl, bool isCombinationChart)
	{
		double num = sfChart.ChartArea.RenderBounds.Size.Width;
		double num2 = sfChart.ChartArea.RenderBounds.Size.Height;
		double expectedSize = Math.Pow((num2 + num) / 2.0, 2.0) / (2.0 * Math.Max(num, num2));
		SetMaxMinRadiusForBubbles(sfChart, chartImpl, isPrimary: true, expectedSize);
		if (isCombinationChart)
		{
			SetMaxMinRadiusForBubbles(sfChart, chartImpl, isPrimary: false, expectedSize);
		}
	}

	private void SetMaxMinRadiusForBubbles(ChartControl sfChart, ChartImpl chartImpl, bool isPrimary, double expectedSize)
	{
		int num = 100;
		bool flag = false;
		int num2 = 0;
		for (num2 = 0; num2 < chartImpl.Series.Count; num2++)
		{
			if (chartImpl.Series[num2].UsePrimaryAxis == isPrimary)
			{
				num = chartImpl.Series[num2].SerieFormat.CommonSerieOptions.BubbleScale;
				if (num > 300)
				{
					num = 300;
				}
				flag = chartImpl.Series[num2].SerieFormat.CommonSerieOptions.SizeRepresents == ChartBubbleSize.Width;
				break;
			}
		}
		expectedSize = ((num <= 100) ? (expectedSize * 0.25 * ((double)num / 100.0)) : ((num > 200) ? (expectedSize * 0.5 * ((double)num / 300.0)) : (expectedSize * 0.375 * ((double)num / 200.0))));
		_ = sfChart.Series;
		List<Tuple<double, double>> list = new List<Tuple<double, double>>();
		double num3 = 0.0;
		double num4 = 0.0;
		foreach (ChartSeries item in sfChart.Series)
		{
			ChartSeriesModel chartSeriesModel = item.SeriesModel as ChartSeriesModel;
			num3 = (num4 = chartSeriesModel.GetY(0)[1]);
			for (num2 = 0; num2 < chartSeriesModel.Count; num2++)
			{
				double num5 = chartSeriesModel.GetY(num2)[1];
				if (num5 > num4)
				{
					num4 = num5;
				}
				if (num5 < num3)
				{
					num3 = num5;
				}
			}
			list.Add(Tuple.Create(num3, num4));
		}
		List<double> list2 = new List<double>(list.Count());
		List<double> list3 = new List<double>(list.Count());
		foreach (Tuple<double, double> item2 in list)
		{
			list3.Add(item2.Item1);
			list2.Add(item2.Item2);
		}
		list3.Sort();
		list2.Sort();
		num3 = Math.Abs(list3[0]);
		num4 = Math.Abs(list2[list2.Count - 1]);
		list3.Clear();
		list2.Clear();
		foreach (Tuple<double, double> item3 in list)
		{
			list3.Add(item3.Item1 / num3);
			list2.Add(item3.Item2 / num4);
		}
		num3 /= 8.0 * num4;
		num4 /= num4;
		double num6 = 0.0;
		if (!flag)
		{
			num6 = Math.PI * Math.Pow(expectedSize, 2.0);
			num3 = Math.Sqrt(num6 * num3 / Math.PI);
		}
		else
		{
			num3 = expectedSize * num3;
		}
		num2 = 0;
		foreach (ChartSeries item4 in sfChart.Series)
		{
			if (num2 >= list2.Count)
			{
				break;
			}
			if (flag)
			{
				item4.ConfigItems.BubbleItem.MaxBounds = new RectangleF(0f, 0f, (float)(expectedSize * list2[num2]), (float)(expectedSize * list2[num2]));
				item4.ConfigItems.BubbleItem.MinBounds = new RectangleF(0f, 0f, (float)num3, (float)num3);
			}
			else
			{
				item4.ConfigItems.BubbleItem.MaxBounds = new RectangleF(0f, 0f, (float)Math.Sqrt(num6 * list2[num2] / Math.PI), (float)Math.Sqrt(num6 * list2[num2] / Math.PI));
				item4.ConfigItems.BubbleItem.MinBounds = new RectangleF(0f, 0f, (float)num3, (float)num3);
			}
			num2++;
		}
	}

	internal ChartControl GetChart3D(IOfficeChart excelChart)
	{
		bool isPie = false;
		bool isStock = false;
		converter.IsChart3D = true;
		converter.parentWorkbook = _chart.ParentWorkbook;
		ChartControl chartControl = new ChartControl();
		chartControl.Font = new Font(_chart.Font.FontName, (float)_chart.Font.Size);
		chartControl.Series3D = true;
		chartControl.Style3D = true;
		bool num = converter.IsBarChartAxis(_chart.PrimaryValueAxis as ChartAxisImpl);
		ChartAxis chartAxis = (num ? chartControl.PrimaryXAxis : chartControl.PrimaryYAxis);
		ChartAxis chartAxis2 = (num ? chartControl.PrimaryYAxis : chartControl.PrimaryXAxis);
		if (!_chart.ChartType.ToString().Contains("Column_3D"))
		{
			chartControl.ColumnDrawMode = ChartColumnDrawMode.PlaneMode;
		}
		if (_chart.PrimaryCategoryAxis.CategoryType == OfficeCategoryType.Time)
		{
			chartAxis2.ValueType = ChartValueType.DateTime;
		}
		else
		{
			chartAxis2.ValueType = ChartValueType.Category;
		}
		if (_chart.PrimaryValueAxis.IsLogScale)
		{
			chartAxis.ValueType = ChartValueType.Logarithmic;
		}
		else
		{
			chartAxis.ValueType = ChartValueType.Double;
		}
		if (Array.IndexOf(ChartImpl.DEF_WALLS_OR_FLOOR_TYPES, _chart.ChartType) >= 0)
		{
			converter.SfWall(chartControl, _chart);
		}
		foreach (IOfficeChartSerie item in GetOfficeChartSeries(_chart.Series))
		{
			if (!item.IsFiltered)
			{
				ChartSerieImpl chartSerieImpl = item as ChartSerieImpl;
				bool isNullSerie = chartSerieImpl.ValuesIRange == null && chartSerieImpl.EnteredDirectlyValues == null;
				GetChartSerie3D(_chart.ChartType, chartControl, chartSerieImpl, out isPie, out isStock, isNullSerie);
				if (isPie || isStock)
				{
					break;
				}
			}
		}
		converter.SfRotation3D(_chart, chartControl);
		foreach (ChartSeries item2 in chartControl.Series)
		{
			if (item2.SortBy == ChartSeriesSortingType.X && item2.XAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (item2.SortBy == ChartSeriesSortingType.Y && item2.YAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (item2.Type == ChartSeriesType.ColumnRange && item2.Rotate)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if ((item2.Type == ChartSeriesType.Bar || item2.Type == ChartSeriesType.StackingBar || item2.Type == ChartSeriesType.StackingBar100) && item2.SortBy == ChartSeriesSortingType.X && item2.YAxis.ValueType == ChartValueType.Category && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
			else if (((item2.SortBy == ChartSeriesSortingType.X && item2.XAxis.ValueType == ChartValueType.Double) || (item2.SortBy == ChartSeriesSortingType.Y && item2.YAxis.ValueType == ChartValueType.Double)) && item2.Type == ChartSeriesType.Scatter && item2.SortPoints)
			{
				item2.SortPoints = false;
				converter.IsSeriesSorted = false;
			}
		}
		RectangleF manualRect = new RectangleF(-1f, -1f, 0f, 0f);
		if (_chart.HasTitle || IsAutoChartTitle(_chart))
		{
			converter.SfChartTitle(_chart, chartControl, out manualRect);
		}
		else
		{
			chartControl.Title.Visible = false;
		}
		if (_chart.Series.Count > 0 && !isPie)
		{
			ChartValueAxisImpl chartValueAxisImpl = _chart.PrimaryValueAxis as ChartValueAxisImpl;
			ChartCategoryAxisImpl chartCategoryAxisImpl = _chart.PrimaryCategoryAxis as ChartCategoryAxisImpl;
			if (_chart.PrimaryValueAxis.IsLogScale)
			{
				converter.SfLogerthmicAxis3D(chartAxis, chartValueAxisImpl, chartCategoryAxisImpl);
			}
			else
			{
				converter.SfNumericalAxis3D(chartAxis, chartValueAxisImpl, chartCategoryAxisImpl);
			}
			if (chartAxis2.ValueType == ChartValueType.Category)
			{
				converter.SfCategoryAxis3D(chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl);
			}
			else if (chartAxis2.ValueType == ChartValueType.DateTime)
			{
				if (converter.FirstSeriesPoints != null && converter.FirstSeriesPoints.Count > 0 && converter.FirstSeriesPoints[0].X is DateTime)
				{
					converter.SfDateTimeAxis3D(chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl);
				}
				else
				{
					chartAxis2.ValueType = ChartValueType.Category;
					converter.SfCategoryAxis3D(chartAxis2, chartCategoryAxisImpl, chartValueAxisImpl);
				}
			}
			else
			{
				converter.SfNumericalAxis3D(chartAxis2, chartCategoryAxisImpl, chartCategoryAxisImpl);
			}
		}
		else
		{
			chartAxis2.IsVisible = false;
			chartAxis.IsVisible = false;
			chartAxis2.DrawGrid = false;
			chartAxis.DrawGrid = false;
		}
		int[] sortedLegendOrders = null;
		if (_chart.HasLegend)
		{
			converter.SfLegend(chartControl, _chart, chartControl.Legend, out sortedLegendOrders, isPlotAreaManual: false, out var _);
		}
		else
		{
			chartControl.Legend.Visible = false;
			chartControl.Legend.DockingFree = true;
		}
		converter.SfChartArea3D(chartControl, _chart.ChartArea as ChartFrameFormatImpl, _chart);
		return chartControl;
	}

	private void GetChartSerie(OfficeChartType serieType, ChartControl chart, ChartSerieImpl serie, out bool isPie, out bool isStock, out bool isRadar, bool isNullSerie)
	{
		isPie = false;
		isStock = false;
		isRadar = false;
		ChartConverterMethod chartConverterMethod = null;
		if (isNullSerie)
		{
			return;
		}
		switch (serieType)
		{
		case OfficeChartType.Pareto:
		case OfficeChartType.Histogram:
		{
			ChartSeries paretoLine;
			ChartSeries series = converter.SfHistogramOrPareto(serie, serie.ParentChart, out paretoLine, chart);
			if (paretoLine != null)
			{
				chart.Series.Add(paretoLine);
				chart.Legend.Items[Array.FindIndex(chart.Legend.Items, (ChartLegendItem legendItem) => legendItem.Text == "Pareto")].Visible = false;
			}
			chart.Series.Add(series);
			return;
		}
		case OfficeChartType.Area:
		case OfficeChartType.Area_3D:
			chartConverterMethod = converter.SfAreaSeries;
			break;
		case OfficeChartType.Bar_Clustered:
		case OfficeChartType.Bar_Clustered_3D:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Clustered:
			chartConverterMethod = converter.SfBarseries;
			break;
		case OfficeChartType.Column_Clustered:
		case OfficeChartType.Column_Clustered_3D:
		case OfficeChartType.Column_3D:
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Clustered_3D:
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Clustered_3D:
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Clustered_3D:
			chartConverterMethod = converter.SfColumnSeries;
			break;
		case OfficeChartType.Pie:
		case OfficeChartType.Pie_3D:
		case OfficeChartType.PieOfPie:
		case OfficeChartType.Pie_Exploded:
		case OfficeChartType.Pie_Exploded_3D:
		case OfficeChartType.Pie_Bar:
			chartConverterMethod = converter.SfPieSeries;
			isPie = true;
			break;
		case OfficeChartType.Line:
		case OfficeChartType.Line_Stacked:
		case OfficeChartType.Line_Stacked_100:
		case OfficeChartType.Line_Markers:
		case OfficeChartType.Line_Markers_Stacked:
		case OfficeChartType.Line_Markers_Stacked_100:
		case OfficeChartType.Line_3D:
			chartConverterMethod = ((!(serie.SerieFormat as ChartSerieDataFormatImpl).IsSmoothed) ? new ChartConverterMethod(converter.SfLineSeries) : new ChartConverterMethod(converter.SfSplineSeries));
			break;
		case OfficeChartType.Area_Stacked:
		case OfficeChartType.Area_Stacked_3D:
			chartConverterMethod = converter.SfStackAreaSeries;
			break;
		case OfficeChartType.Bar_Stacked:
		case OfficeChartType.Bar_Stacked_3D:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Pyramid_Bar_Stacked:
			chartConverterMethod = converter.SfStackBarSeries;
			break;
		case OfficeChartType.Column_Stacked:
		case OfficeChartType.Column_Stacked_3D:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Pyramid_Stacked:
			chartConverterMethod = converter.SfStackedColumnSeries;
			break;
		case OfficeChartType.Area_Stacked_100:
		case OfficeChartType.Area_Stacked_100_3D:
			chartConverterMethod = converter.SfStackArea100Series;
			break;
		case OfficeChartType.Bar_Stacked_100:
		case OfficeChartType.Bar_Stacked_100_3D:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cone_Bar_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
			chartConverterMethod = converter.SfStackBar100Series;
			break;
		case OfficeChartType.Column_Stacked_100:
		case OfficeChartType.Column_Stacked_100_3D:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Pyramid_Stacked_100:
			chartConverterMethod = converter.SfStackColum100Series;
			break;
		case OfficeChartType.Radar:
		case OfficeChartType.Radar_Markers:
		case OfficeChartType.Radar_Filled:
			chartConverterMethod = converter.SfRadarSeries;
			isRadar = true;
			break;
		case OfficeChartType.Scatter_Markers:
			chartConverterMethod = converter.SfScatterrSeries;
			break;
		case OfficeChartType.Scatter_Line_Markers:
		case OfficeChartType.Scatter_Line:
			chartConverterMethod = converter.SfLineSeries;
			break;
		case OfficeChartType.Scatter_SmoothedLine_Markers:
		case OfficeChartType.Scatter_SmoothedLine:
			chartConverterMethod = converter.SfSplineSeries;
			break;
		case OfficeChartType.Stock_HighLowClose:
		case OfficeChartType.Stock_VolumeHighLowClose:
			chartConverterMethod = converter.SfStockHiLoSeries;
			isStock = true;
			break;
		case OfficeChartType.Stock_OpenHighLowClose:
		case OfficeChartType.Stock_VolumeOpenHighLowClose:
			chartConverterMethod = converter.SfCandleSeries;
			isStock = true;
			break;
		case OfficeChartType.Doughnut:
		case OfficeChartType.Doughnut_Exploded:
			chartConverterMethod = converter.SfDoughnutSeries;
			break;
		case OfficeChartType.Bubble:
		case OfficeChartType.Bubble_3D:
			chartConverterMethod = converter.SfBubbleSeries;
			break;
		case OfficeChartType.Funnel:
			chartConverterMethod = converter.SfFunnelSeries;
			break;
		}
		if (chartConverterMethod != null)
		{
			ChartSeries chartSeries = chartConverterMethod(serie, chart);
			chart.Series.Add(chartSeries);
			chartSeries.LegendItem.Text = chartSeries.Name;
		}
	}

	private void GetChartSerie3D(OfficeChartType serieType, ChartControl chart, ChartSerieImpl serie, out bool isPie, out bool isStock, bool isNullSerie)
	{
		isPie = false;
		isStock = false;
		ChartConverterMethod chartConverterMethod = null;
		if (!isNullSerie)
		{
			switch (serieType)
			{
			case OfficeChartType.Area_3D:
				chartConverterMethod = converter.SfAreaSeries3D;
				break;
			case OfficeChartType.Area_Stacked_100_3D:
				chartConverterMethod = converter.SfStackArea100Series3D;
				break;
			case OfficeChartType.Area_Stacked_3D:
				chartConverterMethod = converter.SfStackAreaSeries3D;
				break;
			case OfficeChartType.Bar_Clustered_3D:
			case OfficeChartType.Cylinder_Bar_Clustered:
			case OfficeChartType.Cone_Bar_Clustered:
			case OfficeChartType.Pyramid_Bar_Clustered:
				chartConverterMethod = converter.SfBarseries3D;
				break;
			case OfficeChartType.Bar_Stacked_3D:
			case OfficeChartType.Cylinder_Bar_Stacked:
			case OfficeChartType.Cone_Bar_Stacked:
			case OfficeChartType.Pyramid_Bar_Stacked:
				chartConverterMethod = converter.SfStackBarSeries3D;
				break;
			case OfficeChartType.Bar_Stacked_100_3D:
			case OfficeChartType.Cylinder_Bar_Stacked_100:
			case OfficeChartType.Cone_Bar_Stacked_100:
			case OfficeChartType.Pyramid_Bar_Stacked_100:
				chartConverterMethod = converter.SfStackBar100Series3D;
				break;
			case OfficeChartType.Column_Clustered_3D:
			case OfficeChartType.Column_3D:
			case OfficeChartType.Cylinder_Clustered:
			case OfficeChartType.Cylinder_Clustered_3D:
			case OfficeChartType.Cone_Clustered:
			case OfficeChartType.Cone_Clustered_3D:
			case OfficeChartType.Pyramid_Clustered:
			case OfficeChartType.Pyramid_Clustered_3D:
				chartConverterMethod = converter.SfColumnSeries3D;
				break;
			case OfficeChartType.Column_Stacked_3D:
			case OfficeChartType.Cylinder_Stacked:
			case OfficeChartType.Cone_Stacked:
			case OfficeChartType.Pyramid_Stacked:
				chartConverterMethod = converter.SfStackedColumnSeries3D;
				break;
			case OfficeChartType.Column_Stacked_100_3D:
			case OfficeChartType.Cylinder_Stacked_100:
			case OfficeChartType.Cone_Stacked_100:
			case OfficeChartType.Pyramid_Stacked_100:
				chartConverterMethod = converter.SfStackColum100Series3D;
				break;
			case OfficeChartType.Line_3D:
				chartConverterMethod = converter.SfLineSeries3D;
				break;
			case OfficeChartType.Pie_3D:
			case OfficeChartType.Pie_Exploded_3D:
				chartConverterMethod = converter.SfPieSeries3D;
				isPie = true;
				break;
			}
			ChartSeries chartSeries = chartConverterMethod(serie, chart);
			chart.Series.Add(chartSeries);
			chartSeries.LegendItem.Text = chartSeries.Name;
		}
	}

	public void SaveAsImage(IOfficeChart excelChart, Stream imageAsStream)
	{
		SaveAsImage(excelChart, imageAsStream, new ChartRenderingOptions
		{
			ScalingMode = ScalingMode.Best,
			ImageFormat = ExportImageFormat.Jpeg
		});
	}

	internal void SaveAsImage(IOfficeChart excelChart, Stream imageAsStream, ChartRenderingOptions imageOptions)
	{
		_chart = ((excelChart is ChartImpl) ? (excelChart as ChartImpl) : (excelChart as ChartShapeImpl).ChartObject);
		converter.SetChartSize(excelChart);
		converter.ListOfPoints = converter.GetListofPoints(_chart, out var _);
		ChartControl chartControl = null;
		chartControl = ((!Is3DConversionSupported()) ? GetChart(_chart) : GetChart3D(excelChart));
		chartControl.Width = converter.ChartWidth;
		chartControl.Height = converter.ChartHeight;
		chartControl.SaveImage(imageAsStream, imageOptions);
		converter.SecondayAxisAchived = false;
		chartControl = null;
	}

	private void ResetChartCommonData()
	{
		converter.SfChart = null;
		converter.ItemSource = null;
		converter.HasNewSeries = false;
		converter.NewseriesIndex = 0;
		if (converter.engine != null)
		{
			converter.engine.Dispose();
			converter.engine = null;
		}
	}

	private List<IOfficeChartSerie> GetOfficeChartSeries(IOfficeChartSeries officeChartSeries)
	{
		List<IOfficeChartSerie> result = new List<IOfficeChartSerie>(officeChartSeries);
		bool flag = true;
		bool flag2 = _chart.ChartType.ToString().Contains("Combination_Chart") || _chart.IsChartScatter;
		object[] array = null;
		foreach (ChartSerieImpl item in officeChartSeries)
		{
			if (item.SerieName != null)
			{
				item.Name = item.SerieName;
			}
			if (flag || flag2)
			{
				array = item.EnteredDirectlyCategoryLabels;
				flag = false;
			}
			object[] enteredDirectlyValues = item.EnteredDirectlyValues;
			object[] enteredDirectlyBubbles = item.EnteredDirectlyBubbles;
			if (array != null && item.CategoryLabelsIRange != null && item.CategoryLabelsIRange is RangeImpl rangeImpl)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (i < rangeImpl.Cells.Length)
					{
						string text = rangeImpl.Cells[i].NumberFormat;
						if (text == "General" && item.CategoriesFormatCode != null && item.CategoriesFormatCode != text)
						{
							text = item.CategoriesFormatCode;
						}
						rangeImpl.Cells[i].Value2 = array[i];
						rangeImpl.Cells[i].NumberFormat = text;
					}
				}
			}
			if (enteredDirectlyValues != null && item.ValuesIRange != null && item.ValuesIRange is RangeImpl rangeImpl2)
			{
				for (int j = 0; j < enteredDirectlyValues.Length; j++)
				{
					if (j < rangeImpl2.Cells.Length)
					{
						string text2 = rangeImpl2.Cells[j].NumberFormat;
						if (text2 == "General" && item.FormatCode != null && item.FormatCode != text2)
						{
							text2 = item.FormatCode;
						}
						rangeImpl2.Cells[j].Value2 = enteredDirectlyValues[j];
						rangeImpl2.Cells[j].NumberFormat = text2;
					}
				}
			}
			if (enteredDirectlyBubbles == null || item.BubblesIRange == null || !(item.BubblesIRange is RangeImpl rangeImpl3))
			{
				continue;
			}
			for (int k = 0; k < enteredDirectlyBubbles.Length; k++)
			{
				if (rangeImpl3 != null && k < rangeImpl3.Cells.Length)
				{
					rangeImpl3.Cells[k].Value2 = enteredDirectlyBubbles[k];
				}
			}
		}
		return result;
	}
}
