using System;
using System.Collections.Generic;
using System.Linq;
using DocGen.Chart;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;

namespace DocGen.ChartToImageConverter;

internal class ChartSeriesFactory : ChartUtilities
{
	private const int ChartPointsCount = 2000;

	internal int PieAngle = 270;

	internal IList<ChartPointInternal> FirstSeriesPoints;

	internal bool isEntertedDirectCategoryValue;

	internal int[] SortedIndexes;

	internal ChartSeriesType[] RectangleSerieTypes = new ChartSeriesType[7]
	{
		ChartSeriesType.Column,
		ChartSeriesType.StackingColumn,
		ChartSeriesType.StackingColumn100,
		ChartSeriesType.ColumnRange,
		ChartSeriesType.Bar,
		ChartSeriesType.StackingBar,
		ChartSeriesType.StackingBar100
	};

	private IList<ChartPointInternal> GetChartPointsForHistogram(ChartSerieImpl serie, IList<ChartPointInternal> observableCollection)
	{
		IList<ChartPointInternal> list = new List<ChartPointInternal>();
		observableCollection = GetChartPointsValues(serie, showEmptyPoints: false, -1, isBubbles: false, observableCollection);
		double binwidth = 0.0;
		double overflowBin = 0.0;
		double underflowBin = 0.0;
		HistogramAxisFormat histogramAxisFormat = (((serie.SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty == null) ? (serie.ParentChart.PrimaryCategoryAxis as ChartCategoryAxisImpl).HistogramAxisFormatProperty : (serie.SerieFormat as ChartSerieDataFormatImpl).HistogramAxisFormatProperty);
		if (histogramAxisFormat != null && histogramAxisFormat.IsBinningByCategory)
		{
			int i;
			for (i = 0; i < observableCollection.Count; i++)
			{
				ChartPointInternal chartPointInternal = list.FirstOrDefault((ChartPointInternal x) => x.X == observableCollection[i].X);
				if (chartPointInternal == null)
				{
					list.Add(new ChartPointInternal
					{
						X = observableCollection[i].X,
						Value = observableCollection[i].Value
					});
				}
				else
				{
					chartPointInternal.Value += observableCollection[i].Value;
				}
			}
		}
		else
		{
			double num = observableCollection.Min((ChartPointInternal x) => x.Value);
			double num2 = observableCollection.Max((ChartPointInternal x) => x.Value);
			bool flag = false;
			if (histogramAxisFormat == null || histogramAxisFormat.HasAutomaticBins || (histogramAxisFormat.NumberOfBins <= 0 && histogramAxisFormat.BinWidth <= 0.0))
			{
				double standardDeviationValue = GetStandardDeviationValue(observableCollection);
				binwidth = GetAutomaticBinwidthValue(standardDeviationValue, num, num2, observableCollection.Count);
				double average = GetAverage(observableCollection);
				if (histogramAxisFormat != null)
				{
					if (!histogramAxisFormat.IsNotAutomaticOverFlowValue)
					{
						overflowBin = Math.Round(average + 3.0 * standardDeviationValue);
					}
					if (!histogramAxisFormat.IsNotAutomaticUnderFlowValue)
					{
						underflowBin = Math.Round(average - 3.0 * standardDeviationValue);
					}
				}
			}
			else if (histogramAxisFormat.NumberOfBins > 0)
			{
				binwidth = (num2 - num) / (double)histogramAxisFormat.NumberOfBins;
				flag = true;
			}
			else if (histogramAxisFormat.BinWidth > 0.0)
			{
				binwidth = histogramAxisFormat.BinWidth;
			}
			if (histogramAxisFormat != null && histogramAxisFormat.IsNotAutomaticOverFlowValue)
			{
				overflowBin = histogramAxisFormat.OverflowBinValue;
			}
			if (histogramAxisFormat != null && histogramAxisFormat.IsNotAutomaticUnderFlowValue)
			{
				underflowBin = histogramAxisFormat.UnderflowBinValue;
			}
			list = GetChartPointsByBinValues(observableCollection, binwidth, flag ? (histogramAxisFormat?.NumberOfBins ?? 0) : 0, num, num2, overflowBin, underflowBin, histogramAxisFormat);
		}
		if (serie.SerieType.ToString().Contains("Pareto"))
		{
			list = new List<ChartPointInternal>(list.OrderByDescending((ChartPointInternal x) => x.Value));
		}
		return list;
	}

	private IList<ChartPointInternal> GetChartPointsByBinValues(IList<ChartPointInternal> modifiedProducts, double binwidth, int numberOfBins, double min, double max, double overflowBin, double underflowBin, HistogramAxisFormat axisFormat)
	{
		ViewModel viewModel = null;
		bool flag = axisFormat?.IsNotAutomaticUnderFlowValue ?? false;
		bool flag2 = axisFormat?.IsNotAutomaticOverFlowValue ?? false;
		if (flag2 && flag && min < underflowBin && underflowBin <= max && min <= overflowBin && overflowBin < max && overflowBin >= underflowBin)
		{
			min = underflowBin;
			max = overflowBin;
			int num = ((numberOfBins != 0) ? numberOfBins : ((min == max) ? 3 : ((binwidth <= max - min) ? ((int)Math.Ceiling((max - min) / binwidth) + 2) : 3)));
			double binWidth = (max - min) / (double)(num - 2);
			viewModel = CalculateValuesFromFlowBins(modifiedProducts, min, max, 0, num, binWidth, axisFormat?.IsIntervalClosedinLeft ?? false);
		}
		else if (flag && min < underflowBin && underflowBin <= max)
		{
			min = underflowBin;
			int count = ((numberOfBins != 0) ? numberOfBins : ((int)Math.Ceiling((max - min) / binwidth) + 1));
			viewModel = CalculateValuesFromFlowBins(modifiedProducts, min, max, -1, count, binwidth, axisFormat?.IsIntervalClosedinLeft ?? false);
		}
		else if (flag2 && min <= overflowBin && overflowBin < max)
		{
			max = overflowBin;
			int count2 = ((numberOfBins != 0) ? numberOfBins : ((int)Math.Ceiling((max - min) / binwidth) + 1));
			viewModel = CalculateValuesFromFlowBins(modifiedProducts, min, max, 1, count2, binwidth, axisFormat?.IsIntervalClosedinLeft ?? false);
		}
		else
		{
			int num2 = ((binwidth == 0.0) ? 1 : ((numberOfBins != 0) ? numberOfBins : ((int)((max - min) / binwidth) + 1)));
			viewModel = new ViewModel(num2);
			double start = min;
			for (int i = 0; i < num2; i++)
			{
				if (axisFormat != null && axisFormat.IsIntervalClosedinLeft)
				{
					viewModel.Products[i].X = "[" + start + "," + (start + binwidth) + ((i == num2 - 1) ? "]" : ")");
				}
				else
				{
					viewModel.Products[i].X = ((i == 0) ? "[" : "(") + start + "," + (start + binwidth) + "]";
				}
				if (min == max && num2 > 1)
				{
					viewModel.Products[i].Value = ((i == 0) ? modifiedProducts.Count : 0);
				}
				else if (axisFormat == null || !axisFormat.IsIntervalClosedinLeft)
				{
					if (start == min)
					{
						viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= start && x.Value <= start + binwidth);
					}
					else if (start + binwidth != max)
					{
						viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value > start && x.Value <= start + binwidth);
					}
					else
					{
						viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value > start && x.Value <= max);
					}
				}
				else if (start == min)
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= start && x.Value < start + binwidth);
				}
				else if (start + binwidth != max)
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= start && x.Value < start + binwidth);
				}
				else
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= start && x.Value <= max);
				}
				start += binwidth;
			}
		}
		return viewModel.Products;
	}

	private ViewModel CalculateValuesFromFlowBins(IList<ChartPointInternal> modifiedProducts, double min, double max, int bintype, int count, double binWidth, bool isIntervalClosedInLeft)
	{
		string text = char.ConvertFromUtf32(8805).ToString();
		string text2 = char.ConvertFromUtf32(8804).ToString();
		ViewModel viewModel = new ViewModel(count);
		double current = min;
		double previous = min;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			if (i == 0 && bintype <= 0)
			{
				viewModel.Products[i].X = (isIntervalClosedInLeft ? "<" : text2) + min;
				viewModel.Products[i].Value = (isIntervalClosedInLeft ? modifiedProducts.Count((ChartPointInternal x) => x.Value < min) : modifiedProducts.Count((ChartPointInternal x) => x.Value <= min));
				continue;
			}
			if (i == count - 1 && bintype >= 0)
			{
				viewModel.Products[i].X = (isIntervalClosedInLeft ? text : ">") + max;
				viewModel.Products[i].Value = (isIntervalClosedInLeft ? modifiedProducts.Count((ChartPointInternal x) => x.Value >= max) : modifiedProducts.Count((ChartPointInternal x) => x.Value > max));
				continue;
			}
			current += binWidth;
			if (current >= max && bintype >= 0)
			{
				current = max;
			}
			viewModel.Products[i].X = ((isIntervalClosedInLeft || i == 0) ? "[" : "(") + previous + "," + current + ((!isIntervalClosedInLeft || i == count - 1) ? "]" : ")");
			if (flag)
			{
				viewModel.Products[i].Value = 0.0;
			}
			else if (!isIntervalClosedInLeft)
			{
				if (i == 0)
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= previous && x.Value <= current);
				}
				else if (i < count - 1)
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value > previous && x.Value <= current);
				}
				else
				{
					viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value > previous && x.Value <= max);
				}
			}
			else if (i == 0)
			{
				viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= previous && x.Value < current);
			}
			else if (i < count - 1)
			{
				viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= previous && x.Value < current);
			}
			else
			{
				viewModel.Products[i].Value = modifiedProducts.Count((ChartPointInternal x) => x.Value >= previous && x.Value <= max);
			}
			if (current == max)
			{
				flag = true;
			}
			previous += binWidth;
		}
		return viewModel;
	}

	private IList<ChartPointInternal> ChangeFunnelItemsSourceToRangeColumn(IList<ChartPointInternal> data)
	{
		double num = data.Max((ChartPointInternal point) => point.Value);
		if (num <= 0.0)
		{
			num = 1.0;
		}
		List<ChartPointInternal> list = new List<ChartPointInternal>();
		for (int i = 0; i < data.Count; i++)
		{
			double num2 = ((data[i].Value < 0.0) ? 0.0 : data[i].Value);
			double num3 = num2 / num;
			list.Add(new ChartPointInternal
			{
				X = data[i].X,
				Low = 0.5 - num3 / 2.0,
				High = 0.5 + num3 / 2.0,
				Value = num2
			});
		}
		return list;
	}

	private double GetStandardDeviationValue(IList<ChartPointInternal> array)
	{
		int count = array.Count;
		double num = 0.0;
		for (int i = 0; i < count; i++)
		{
			num += array[i].Value;
		}
		num /= (double)count;
		double num2 = 0.0;
		double num3 = 0.0;
		for (int j = 0; j < count; j++)
		{
			num3 = array[j].Value - num;
			num2 += num3 * num3;
		}
		if (count == 1)
		{
			return 0.0;
		}
		return Math.Sqrt(num2 / (double)(count - 1));
	}

	private double GetAutomaticBinwidthValue(double stdev, double min, double max, double length)
	{
		double num = 3.5 * stdev / Math.Pow(length, 0.333);
		if (num < 1.0)
		{
			double num2 = Math.Ceiling((max - min) / num);
			int length2 = Math.Truncate(10.0 / ((max - min) / num2)).ToString().Length;
			return Math.Round(num, length2);
		}
		if (num < 10.0 && Math.Truncate(num) >= 0.1)
		{
			return Math.Round(num, 1);
		}
		return Math.Round(num);
	}

	private double GetAverage(IList<ChartPointInternal> array)
	{
		int count = array.Count;
		double num = 0.0;
		for (int i = 0; i < count; i++)
		{
			num += array[i].Value;
		}
		return num / (double)count;
	}

	private ViewModel GetViewModel(ChartSerieImpl serie)
	{
		ViewModel result = null;
		if (serie.ValuesIRange != null && serie.FilteredValue == null)
		{
			int num = ((serie.ValuesIRange != null) ? serie.ValuesIRange.Count : 0);
			int num2 = ((serie.CategoryLabelsIRange != null) ? ((serie.CategoryLabelsIRange.LastColumn - serie.CategoryLabelsIRange.Column != 0 && serie.CategoryLabelsIRange.LastRow - serie.CategoryLabelsIRange.Row != 0) ? ((serie.ParentChart.IsSeriesInRows ? (serie.CategoryLabelsIRange.LastColumn - serie.CategoryLabelsIRange.Column) : (serie.CategoryLabelsIRange.LastRow - serie.CategoryLabelsIRange.Row)) + 1) : serie.CategoryLabelsIRange.Count) : 0);
			object[] enteredDirectlyValues = serie.EnteredDirectlyValues;
			object[] enteredDirectlyCategoryLabels = serie.EnteredDirectlyCategoryLabels;
			if ((enteredDirectlyValues == null || num < enteredDirectlyValues.Length) && (enteredDirectlyCategoryLabels == null || num < enteredDirectlyCategoryLabels.Length))
			{
				result = new ViewModel(num);
			}
			else if (enteredDirectlyValues != null && enteredDirectlyValues.Length < num && serie.ValuesIRange != null && serie.ValuesIRange is ExternalRange)
			{
				result = new ViewModel(enteredDirectlyValues.Length);
			}
			else if (num != 0 && num2 != 0 && num <= num2 && (enteredDirectlyCategoryLabels == null || serie.ParentChart.PrimaryCategoryAxis == null || serie.ParentChart.PrimaryCategoryAxis.CategoryType != OfficeCategoryType.Time))
			{
				result = new ViewModel(num);
			}
			else if (num != 0 && num2 != 0 && num <= num2)
			{
				result = new ViewModel(num);
			}
			else if (enteredDirectlyCategoryLabels != null)
			{
				result = new ViewModel(serie.EnteredDirectlyCategoryLabels.Length);
			}
			else if (num2 != 0 && num2 >= num)
			{
				result = new ViewModel(num2);
			}
			else if (enteredDirectlyValues != null)
			{
				result = new ViewModel(serie.EnteredDirectlyValues.Length);
			}
		}
		else if (serie.EnteredDirectlyValues != null)
		{
			result = new ViewModel(serie.EnteredDirectlyValues.Length);
		}
		return result;
	}

	private IList<ChartPointInternal> GetChartPointsValues(ChartSerieImpl serie, bool showEmptyPoints, int emptyPointValue, bool isBubbles, IList<ChartPointInternal> values)
	{
		IList<ChartPointInternal> list = values;
		ChartImpl parentChart = serie.ParentChart;
		string startSerieType = ChartFormatImpl.GetStartSerieType(serie.SerieType);
		bool flag = startSerieType == "Bubble" || startSerieType == "Scatter";
		bool isHistogramOrPareto = parentChart.IsHistogramOrPareto;
		bool flag2 = false;
		if (isBubbles)
		{
			flag2 = serie.SerieFormat.CommonSerieOptions.ShowNegativeBubbles;
		}
		List<int> list2 = null;
		List<int> list3 = null;
		object[] array = null;
		IRange valuesIRange = serie.ValuesIRange;
		IRange categoryLabelsIRange = serie.CategoryLabelsIRange;
		array = serie.EnteredDirectlyCategoryLabels;
		IRange range = null;
		if (isBubbles)
		{
			range = serie.BubblesIRange;
		}
		valuesIRange = (CheckIfValidValueRange(valuesIRange) ? valuesIRange : null);
		categoryLabelsIRange = (CheckIfValidValueRange(categoryLabelsIRange) ? categoryLabelsIRange : null);
		if (categoryLabelsIRange != null && categoryLabelsIRange.LastColumn > categoryLabelsIRange.Column && categoryLabelsIRange.LastRow != categoryLabelsIRange.Row && categoryLabelsIRange is RangeImpl && (categoryLabelsIRange as RangeImpl).IsMultiReference)
		{
			categoryLabelsIRange = categoryLabelsIRange[categoryLabelsIRange.Row, categoryLabelsIRange.LastColumn, categoryLabelsIRange.LastRow, categoryLabelsIRange.LastColumn];
		}
		if (isBubbles)
		{
			range = (CheckIfValidValueRange(range) ? range : null);
		}
		object[] enteredDirectlyValues = serie.EnteredDirectlyValues;
		object[] enteredDirectlyBubbles = serie.EnteredDirectlyBubbles;
		ChartCategoryAxisImpl chartCategoryAxisImpl = (serie.UsePrimaryAxis ? serie.ParentChart.PrimaryCategoryAxis : serie.ParentChart.SecondaryCategoryAxis) as ChartCategoryAxisImpl;
		ChartValueAxisImpl valueAxis = (serie.UsePrimaryAxis ? (parentChart.PrimaryValueAxis as ChartValueAxisImpl) : (parentChart.SecondaryValueAxis as ChartValueAxisImpl));
		WorksheetImpl worksheetImpl = null;
		WorksheetImpl worksheetImpl2 = null;
		WorksheetImpl sheet = null;
		if (valuesIRange != null)
		{
			worksheetImpl = valuesIRange.Worksheet as WorksheetImpl;
		}
		if (categoryLabelsIRange != null)
		{
			worksheetImpl2 = categoryLabelsIRange.Worksheet as WorksheetImpl;
		}
		if (isBubbles && range != null)
		{
			sheet = range.Worksheet as WorksheetImpl;
		}
		double categoryDisplayUnit = 1.0;
		double num = 1.0;
		num = GetDisplayUnitValue(valueAxis);
		if (flag && chartCategoryAxisImpl.IsChartBubbleOrScatter)
		{
			categoryDisplayUnit = GetDisplayUnitValue(chartCategoryAxisImpl);
		}
		string numberFormat = chartCategoryAxisImpl.NumberFormat;
		string text = ((categoryLabelsIRange == null) ? (string.IsNullOrEmpty(serie.CategoriesFormatCode) ? "General" : serie.CategoriesFormatCode) : ((categoryLabelsIRange.Cells.Length >= 1) ? categoryLabelsIRange.Cells[0].NumberFormat : "General"));
		ChartPointValueType chartPointValueType = ChartPointValueType.TextAxisValue;
		bool valueContainsAnyString = ValuesContainsAnyString(categoryLabelsIRange, array, list3, worksheetImpl2);
		if (!IsChartEx)
		{
			chartPointValueType = ((array != null || categoryLabelsIRange != null) ? GetChartPointValueType(flag, valueContainsAnyString, text, numberFormat, parentChart, chartCategoryAxisImpl) : (flag ? ChartPointValueType.ScatterDefaultIndexXValue : ChartPointValueType.DefaultIndexValueWithAxisNumFmt));
			if (serie.SerieType.ToString().Contains("Radar") && chartPointValueType == ChartPointValueType.DateTimeAxisValue)
			{
				chartPointValueType = (chartCategoryAxisImpl.IsSourceLinked ? ChartPointValueType.TextAxisValue : ChartPointValueType.TextAxisValueWithNumFmt);
			}
		}
		if (list3 != null && list3.Contains(0))
		{
			int i;
			for (i = 0; list3.Contains(i) && i < list.Count; i++)
			{
			}
			text = ((i < categoryLabelsIRange.Cells.Length) ? categoryLabelsIRange.Cells[i].NumberFormat : "General");
		}
		bool flag3 = false;
		string dataLabelNumberFormat = null;
		if (!IsChartEx && (serie.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels)
		{
			ChartDataLabelsImpl chartDataLabelsImpl = serie.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
			bool flag4 = (serie.DataPoints as ChartDataPointsCollection).CheckDPDataLabels();
			if ((chartDataLabelsImpl != null && !chartDataLabelsImpl.IsDelete && chartDataLabelsImpl.IsCategoryName) || flag4)
			{
				flag3 = true;
				dataLabelNumberFormat = chartDataLabelsImpl.NumberFormat;
			}
		}
		int j = 0;
		int k = 0;
		int num2 = 0;
		for (; j < list.Count; j++)
		{
			IRange range2 = null;
			object dataLabelResult = null;
			if (((chartPointValueType == ChartPointValueType.ScatterXAxisValue) ? (serie.ParentChart.DisplayBlanksAs == OfficeChartPlotEmpty.Interpolated) : (chartPointValueType == ChartPointValueType.DateTimeAxisValue)) && array != null && k < array.Length && array[k] == null)
			{
				if (list2 == null)
				{
					list2 = new List<int>(list.Count);
				}
				list2.Add(j);
				k++;
				num2++;
				continue;
			}
			if (valuesIRange != null)
			{
				WorksheetImpl.TRangeValueType cellType = WorksheetImpl.TRangeValueType.Number;
				IRange range3 = valuesIRange.Cells[j];
				if (list2 != null && IsRowOrColumnIsHidden(range3, worksheetImpl))
				{
					list2.Add(j);
				}
				else
				{
					double num3 = GetdoubleValueFromCell(range3, worksheetImpl, out cellType);
					string text2 = serie.SerieType.ToString();
					if (double.IsNaN(num3) && (serie.ParentChart.DisplayBlanksAs == OfficeChartPlotEmpty.Zero || text2.Contains("Area_Stacked_100") || (text2.Contains("Line") && text2.Contains("Stacked_100"))))
					{
						num3 = 0.0;
					}
					list[j].Value = num3;
				}
				if (cellType == WorksheetImpl.TRangeValueType.Blank && !IsChartEx && showEmptyPoints)
				{
					switch (emptyPointValue)
					{
					case 0:
						if (startSerieType.Contains("Pie") || startSerieType.Contains("Doughnut"))
						{
							list[j].Value = 0.0;
						}
						break;
					case 1:
						list[j].IsSummary = true;
						break;
					}
				}
			}
			else if (enteredDirectlyValues != null)
			{
				if (enteredDirectlyValues[j] == null)
				{
					list[j].Value = double.NaN;
				}
				else if (enteredDirectlyValues[j] is string)
				{
					if (enteredDirectlyValues[j].ToString().Equals("#N/A", StringComparison.OrdinalIgnoreCase))
					{
						list[j].Value = double.NaN;
					}
					else
					{
						list[j].Value = 0.0;
					}
				}
				else
				{
					list[j].Value = Convert.ToDouble(enteredDirectlyValues[j]);
				}
			}
			if (num > 1.0)
			{
				list[j].Value = list[j].Value / num;
			}
			object categoryValue = "";
			if (categoryLabelsIRange != null)
			{
				if (k < categoryLabelsIRange.Cells.Length)
				{
					range2 = categoryLabelsIRange.Cells[k];
				}
				else if (flag || chartPointValueType == ChartPointValueType.DateTimeAxisValue)
				{
					break;
				}
			}
			else if (array != null)
			{
				if (k < array.Length)
				{
					categoryValue = array[k];
				}
				else if (flag || chartPointValueType == ChartPointValueType.DateTimeAxisValue)
				{
					break;
				}
			}
			else if (chartCategoryAxisImpl.EnteredDirectlyCategoryLabels != null && serie.ParentChart.IsChart3D && serie.ParentChart.IsStacked)
			{
				if (k < chartCategoryAxisImpl.EnteredDirectlyCategoryLabels.Length)
				{
					categoryValue = chartCategoryAxisImpl.EnteredDirectlyCategoryLabels[k];
					isEntertedDirectCategoryValue = true;
				}
			}
			else if (!isHistogramOrPareto)
			{
				categoryValue = k + 1;
			}
			if (list3 != null)
			{
				if (k < categoryLabelsIRange.Cells.Length)
				{
					for (; list3.Contains(k) && k < list.Count; k++)
					{
					}
				}
				if (j != k)
				{
					if (k < categoryLabelsIRange.Cells.Length)
					{
						range2 = categoryLabelsIRange.Cells[k];
					}
					else
					{
						if (flag || chartPointValueType == ChartPointValueType.DateTimeAxisValue)
						{
							break;
						}
						range2 = null;
					}
				}
			}
			if (list2 == null || !list2.Contains(j))
			{
				if (list3 == null || range2 != null)
				{
					list[j].X = GetValueFromCell(range2, categoryValue, worksheetImpl2, num2, chartPointValueType, numberFormat, text, categoryDisplayUnit, flag3, out dataLabelResult, dataLabelNumberFormat);
					if (list[j].X.ToString() == double.NaN.ToString() && serie.ParentChart.DisplayBlanksAs == OfficeChartPlotEmpty.Zero)
					{
						list[j].X = 0;
					}
					if (flag3 && (chartPointValueType != ChartPointValueType.DefaultIndexValue || dataLabelResult != null))
					{
						list[j].Close = ((dataLabelResult != null) ? dataLabelResult : "");
					}
				}
				else
				{
					if (flag3 && chartPointValueType != ChartPointValueType.TextAxisValue)
					{
						list[j].Close = "";
					}
					if (chartPointValueType == ChartPointValueType.TextAxisValue || chartPointValueType == ChartPointValueType.TextAxisValueWithNumFmt)
					{
						list[j].X = "";
					}
					else
					{
						list[j].X = 0;
					}
				}
				k++;
				num2++;
			}
			if (!isBubbles)
			{
				continue;
			}
			list[j].Size = 0;
			if (range != null)
			{
				if (j < range.Cells.Length)
				{
					list[j].Size = GetdoubleValueFromCell(range.Cells[j], sheet, out var _);
				}
			}
			else if (enteredDirectlyBubbles != null)
			{
				if (j < enteredDirectlyBubbles.Length)
				{
					list[j].Size = ((enteredDirectlyBubbles[j] is string) ? 0.0 : Convert.ToDouble(enteredDirectlyBubbles[j]));
				}
			}
			else
			{
				list[j].Size = 1;
			}
			if (list[j].Size.ToString() == double.NaN.ToString())
			{
				list[j].Size = 0.0;
			}
			if (!flag2 && Convert.ToDouble(list[j].Size) < 0.0)
			{
				list[j].Size = 0.0;
			}
		}
		if (list2 != null)
		{
			IList<ChartPointInternal> list4 = new List<ChartPointInternal>();
			int num4 = 0;
			foreach (ChartPointInternal item in list)
			{
				if (!list2.Contains(num4))
				{
					list4.Add(item);
				}
				num4++;
			}
			list = list4;
		}
		if ((flag || chartPointValueType == ChartPointValueType.DateTimeAxisValue) && j + 1 != list.Count)
		{
			for (int num5 = list.Count - 1; num5 >= j; num5--)
			{
				list.RemoveAt(num5);
			}
		}
		SetDateTimeIntervalType(serie, list, isStock: false);
		return list;
	}

	private ChartPointValueType GetChartPointValueType(bool isBubbleOrScatter, bool valueContainsAnyString, string labelNumFmt, string axisNumFmt, ChartImpl chart, ChartCategoryAxisImpl categoryAxis)
	{
		ChartPointValueType chartPointValueType = ChartPointValueType.TextAxisValue;
		if (isBubbleOrScatter)
		{
			if (valueContainsAnyString)
			{
				return ChartPointValueType.ScatterDefaultIndexXValue;
			}
			return ChartPointValueType.ScatterXAxisValue;
		}
		if (chart.CategoryLabelLevel == OfficeCategoriesLabelLevel.CategoriesLabelLevelNone)
		{
			if (categoryAxis.IsSourceLinked)
			{
				if (valueContainsAnyString)
				{
					return ChartPointValueType.DefaultIndexValue;
				}
				if (labelNumFmt != "General" && labelNumFmt.ToLower() != "standard")
				{
					return ChartPointValueType.DefaultIndexValueWithNumFmt;
				}
				return ChartPointValueType.DefaultIndexValue;
			}
			if (axisNumFmt == "General")
			{
				return ChartPointValueType.DefaultIndexValue;
			}
			return ChartPointValueType.DefaultIndexValueWithAxisNumFmt;
		}
		if (!valueContainsAnyString)
		{
			if (categoryAxis.CategoryType == OfficeCategoryType.Time && !chart.IsStringRef)
			{
				return ChartPointValueType.DateTimeAxisValue;
			}
			if (!categoryAxis.IsSourceLinked)
			{
				return ChartPointValueType.TextAxisValueWithNumFmt;
			}
			return ChartPointValueType.TextAxisValue;
		}
		return ChartPointValueType.TextAxisValue;
	}

	private bool ValuesContainsAnyString(IRange categoryRanges, object[] directCategories, List<int> categoryIndexesArray, WorksheetImpl worksheet)
	{
		bool result = false;
		if (categoryRanges != null)
		{
			int num = 0;
			foreach (IRange categoryRange in categoryRanges)
			{
				if (categoryIndexesArray != null && IsRowOrColumnIsHidden(categoryRange, worksheet))
				{
					categoryIndexesArray.Add(num);
				}
				else
				{
					WorksheetImpl.TRangeValueType cellType = worksheet.GetCellType(categoryRange.Row, categoryRange.Column, bNeedFormulaSubType: true);
					if (cellType == WorksheetImpl.TRangeValueType.String || cellType == (WorksheetImpl.TRangeValueType.Formula | WorksheetImpl.TRangeValueType.String))
					{
						if (categoryIndexesArray == null)
						{
							return true;
						}
						result = true;
					}
				}
				num++;
			}
		}
		else if (directCategories != null && directCategories.Any((object x) => x != null && (x.GetType() == typeof(string) || !(x is IConvertible))))
		{
			return true;
		}
		return result;
	}

	private object GetValueFromCell(IRange cell, object categoryValue, WorksheetImpl sheet, int index, ChartPointValueType expectedType, string axisNumberFormat, string labelNumberFormat, double categoryDisplayUnit, bool isCategoryLabelsNeeded, out object dataLabelResult, string dataLabelNumberFormat)
	{
		object obj = "";
		if (categoryValue == null)
		{
			categoryValue = "";
		}
		dataLabelResult = null;
		switch (expectedType)
		{
		case ChartPointValueType.DefaultIndexValue:
			obj = (index + 1).ToString();
			break;
		case ChartPointValueType.ScatterDefaultIndexXValue:
		case ChartPointValueType.DefaultIndexValueWithAxisNumFmt:
		case ChartPointValueType.DefaultIndexValueWithNumFmt:
			if (isEntertedDirectCategoryValue)
			{
				obj = categoryValue;
				break;
			}
			obj = index + 1;
			if (categoryDisplayUnit > 1.0 && expectedType == ChartPointValueType.ScatterDefaultIndexXValue)
			{
				obj = (double)(index + 1) / categoryDisplayUnit;
			}
			break;
		case ChartPointValueType.ScatterXAxisValue:
		case ChartPointValueType.DateTimeAxisValue:
		case ChartPointValueType.TextAxisValueWithNumFmt:
		{
			WorksheetImpl.TRangeValueType cellType = WorksheetImpl.TRangeValueType.Number;
			double num = 0.0;
			if (cell != null)
			{
				num = GetdoubleValueFromCell(cell, sheet, out cellType);
			}
			else if (categoryValue != "")
			{
				num = Convert.ToDouble(categoryValue);
			}
			if (num.ToString() == double.NaN.ToString() && (expectedType == ChartPointValueType.DateTimeAxisValue || (cellType != 0 && (cellType & WorksheetImpl.TRangeValueType.Error) != WorksheetImpl.TRangeValueType.Error)))
			{
				num = 0.0;
			}
			if (!(num.ToString() != double.NaN.ToString()))
			{
				obj = ((expectedType != 0) ? "" : ((object)num));
			}
			else
			{
				if (categoryDisplayUnit > 1.0 && expectedType == ChartPointValueType.ScatterXAxisValue)
				{
					num /= categoryDisplayUnit;
				}
				obj = num;
			}
			if (expectedType == ChartPointValueType.DateTimeAxisValue)
			{
				obj = DateTimeExtension.FromOADate((double)obj);
			}
			break;
		}
		case ChartPointValueType.TextAxisValue:
			obj = ((cell == null) ? ((categoryValue == "" || !(labelNumberFormat.ToLower() != "standard") || !(labelNumberFormat != "General")) ? categoryValue.ToString() : ApplyNumberFormat(categoryValue, labelNumberFormat)) : cell.DisplayText);
			break;
		}
		if (expectedType == ChartPointValueType.TextAxisValue)
		{
			dataLabelResult = obj.ToString();
			return obj;
		}
		bool flag = expectedType == ChartPointValueType.DefaultIndexValueWithNumFmt;
		if (isCategoryLabelsNeeded)
		{
			switch (expectedType)
			{
			case ChartPointValueType.DefaultIndexValue:
			case ChartPointValueType.DefaultIndexValueWithAxisNumFmt:
			case ChartPointValueType.DefaultIndexValueWithNumFmt:
				if (dataLabelNumberFormat != null && dataLabelNumberFormat.ToLower() != "standard" && dataLabelNumberFormat != "General")
				{
					dataLabelResult = ApplyNumberFormat(obj, dataLabelNumberFormat);
				}
				else if (expectedType != ChartPointValueType.DefaultIndexValue)
				{
					dataLabelResult = obj.ToString();
				}
				if (expectedType == ChartPointValueType.DefaultIndexValue)
				{
					return obj;
				}
				break;
			case ChartPointValueType.ScatterDefaultIndexXValue:
			case ChartPointValueType.DateTimeAxisValue:
			case ChartPointValueType.TextAxisValueWithNumFmt:
				if (cell != null)
				{
					dataLabelResult = cell.DisplayText;
				}
				else if (categoryValue != "" && labelNumberFormat.ToLower() != "standard" && labelNumberFormat != "General")
				{
					dataLabelResult = ApplyNumberFormat(categoryValue, labelNumberFormat);
				}
				else
				{
					dataLabelResult = categoryValue.ToString();
				}
				break;
			case ChartPointValueType.ScatterXAxisValue:
				if (categoryDisplayUnit > 0.0)
				{
					if (obj == "")
					{
						dataLabelResult = "";
						break;
					}
					if (cell == null)
					{
						dataLabelResult = (double)obj * categoryDisplayUnit;
					}
					else
					{
						dataLabelResult = obj;
					}
					dataLabelResult = ApplyNumberFormat(dataLabelResult, (cell != null) ? cell.NumberFormat : labelNumberFormat);
				}
				else if (cell != null)
				{
					dataLabelResult = cell.DisplayText;
				}
				else if (categoryValue != "" && labelNumberFormat.ToLower() != "standard" && labelNumberFormat != "General")
				{
					dataLabelResult = ApplyNumberFormat(categoryValue, labelNumberFormat);
				}
				else
				{
					dataLabelResult = categoryValue.ToString();
				}
				break;
			}
		}
		if (expectedType == ChartPointValueType.TextAxisValueWithNumFmt || expectedType == ChartPointValueType.DefaultIndexValueWithNumFmt || expectedType == ChartPointValueType.DefaultIndexValueWithAxisNumFmt)
		{
			if (obj == "")
			{
				obj = ((cell != null) ? cell.DisplayText : "");
			}
			else
			{
				string text = (flag ? labelNumberFormat : axisNumberFormat);
				if (text == "standard" || text == "General")
				{
					obj = obj.ToString();
				}
				else if (cell != null)
				{
					string numberFormat = cell.NumberFormat;
					cell.NumberFormat = axisNumberFormat;
					obj = ApplyNumberFormat(obj, text);
					cell.NumberFormat = numberFormat;
				}
			}
		}
		return obj;
	}

	private double GetdoubleValueFromCell(IRange cell, WorksheetImpl sheet, out WorksheetImpl.TRangeValueType cellType)
	{
		cellType = sheet.GetCellType(cell.Row, cell.Column, bNeedFormulaSubType: true);
		double num = 0.0;
		num = (cell.Number.ToString().Contains("E") ? cell.Number : ((!double.TryParse(cell.Value, out var result)) ? cell.Number : result));
		if (num.ToString() == double.NaN.ToString())
		{
			if (cell.HasFormulaNumberValue || cell.HasFormulaDateTime)
			{
				num = cell.FormulaNumberValue;
			}
			else if ((cellType & WorksheetImpl.TRangeValueType.Formula) != WorksheetImpl.TRangeValueType.Formula || (cellType & WorksheetImpl.TRangeValueType.Error) != WorksheetImpl.TRangeValueType.Error)
			{
				num = ((cellType != 0 && ((cellType & WorksheetImpl.TRangeValueType.Error) != WorksheetImpl.TRangeValueType.Error || (!string.IsNullOrEmpty(cell.Error) && !cell.Error.Equals("#N/A", StringComparison.OrdinalIgnoreCase)))) ? 0.0 : double.NaN);
			}
			else if (cell.FormulaErrorValue == "#DIV/0!")
			{
				num = 0.0;
			}
		}
		return num;
	}

	private void SetDateTimeIntervalType(ChartSerieImpl serie, IList<ChartPointInternal> chartPoints, bool isStock)
	{
		if (serie.ParentChart.PrimaryCategoryAxis.CategoryType != OfficeCategoryType.Time || chartPoints.Count <= 1 || !(chartPoints[0].X is DateTime))
		{
			return;
		}
		if (serie.ParentSeries[0].Equals(serie) || serie.ParentSeries.OrderByType()[0].Equals(serie) || (serie.Reversed && serie.ParentSeries[serie.ParentSeries.Count - 1].Equals(serie)))
		{
			List<DateTime> list = new List<DateTime>(chartPoints.Count);
			foreach (ChartPointInternal chartPoint in chartPoints)
			{
				double result = 0.0;
				if (double.TryParse(chartPoint.X.ToString(), out result))
				{
					chartPoint.X = DateTime.FromOADate(result);
					list.Add((DateTime)chartPoint.X);
				}
				else
				{
					list.Add((DateTime)chartPoint.X);
				}
			}
			List<KeyValuePair<DateTime, int>> source = (from x in list.Select((DateTime x, int i) => new KeyValuePair<DateTime, int>(x, i))
				orderby x.Key
				select x).ToList();
			SortedIndexes = source.Select((KeyValuePair<DateTime, int> x) => x.Value).ToList().ToArray();
			ChartPointInternal[] array = new ChartPointInternal[chartPoints.Count];
			chartPoints.CopyTo(array, 0);
			chartPoints.Clear();
			int[] sortedIndexes = SortedIndexes;
			foreach (int num in sortedIndexes)
			{
				chartPoints.Add(array[num]);
			}
			FirstSeriesPoints = chartPoints;
			DateTime dateTime = (DateTime)chartPoints[0].X;
			if (chartPoints.Where((ChartPointInternal x) => ((DateTime)x.X).Day == dateTime.Day && ((DateTime)x.X).Year == dateTime.Year).Count() == chartPoints.Count)
			{
				DateTimeIntervalType = ChartDateTimeIntervalType.Months;
			}
			else if (chartPoints.Where((ChartPointInternal x) => ((DateTime)x.X).Month == dateTime.Month && ((DateTime)x.X).Year == dateTime.Year).Count() == chartPoints.Count)
			{
				DateTimeIntervalType = ChartDateTimeIntervalType.Days;
			}
			else if (chartPoints.Where((ChartPointInternal x) => ((DateTime)x.X).Day == dateTime.Day && ((DateTime)x.X).Month == dateTime.Month).Count() == chartPoints.Count)
			{
				DateTimeIntervalType = ChartDateTimeIntervalType.Years;
			}
			else
			{
				DateTimeIntervalType = ChartDateTimeIntervalType.Auto;
			}
			return;
		}
		ChartPointInternal[] array2 = new ChartPointInternal[chartPoints.Count];
		chartPoints.CopyTo(array2, 0);
		chartPoints.Clear();
		if (FirstSeriesPoints == null)
		{
			return;
		}
		for (int k = 0; k < FirstSeriesPoints.Count; k++)
		{
			ChartPointInternal chartPointInternal = new ChartPointInternal();
			chartPointInternal.X = FirstSeriesPoints[k].X;
			if (isStock)
			{
				if (FirstSeriesPoints[k].Size != null)
				{
					chartPointInternal.Size = FirstSeriesPoints[k].Size;
				}
				else if (array2[SortedIndexes[k]].Size != null)
				{
					chartPointInternal.Size = array2[SortedIndexes[k]].Size;
				}
			}
			else if (FirstSeriesPoints[k].Close != null)
			{
				chartPointInternal.Close = FirstSeriesPoints[k].Close;
			}
			else if (SortedIndexes[k] < array2.Length && array2[SortedIndexes[k]].Close != null)
			{
				chartPointInternal.Close = array2[SortedIndexes[k]].Close;
			}
			if (SortedIndexes[k] < array2.Length)
			{
				chartPointInternal.Value = array2[SortedIndexes[k]].Value;
				chartPoints.Add(chartPointInternal);
			}
		}
	}

	private IList<ChartPointInternal> GetChartPointsValuesForStockChart(bool showEmptyPoints, ChartImpl chart, ChartSerieImpl serie, IList<ChartPointInternal> values, bool isCandleChart)
	{
		IList<ChartPointInternal> list = values;
		ChartSerieImpl chartSerieImpl = null;
		List<int> list2 = null;
		ChartImpl parentChart = serie.ParentChart;
		ChartSerieImpl chartSerieImpl2;
		ChartSerieImpl chartSerieImpl3;
		ChartSerieImpl chartSerieImpl4;
		if (isCandleChart)
		{
			int[] array = (serie.ParentChart.ChartType.ToString().ToLower().Contains("volume") ? null : new int[4] { 0, 1, 2, 3 });
			if (array == null)
			{
				array = new int[4];
				int i = 0;
				int num = 0;
				for (; i < 5; i++)
				{
					if (!chart.Series[i].SerieType.ToString().ToLower().Contains("column"))
					{
						array[num] = i;
						num++;
					}
				}
				if (array.Count((int x) => x == 0) > 1)
				{
					array = new int[4] { 0, 1, 2, 3 };
				}
			}
			chartSerieImpl = chart.Series[array[0]] as ChartSerieImpl;
			chartSerieImpl2 = chart.Series[array[1]] as ChartSerieImpl;
			chartSerieImpl3 = chart.Series[array[2]] as ChartSerieImpl;
			chartSerieImpl4 = chart.Series[array[3]] as ChartSerieImpl;
		}
		else
		{
			chartSerieImpl2 = chart.Series[0] as ChartSerieImpl;
			chartSerieImpl3 = chart.Series[1] as ChartSerieImpl;
			chartSerieImpl4 = chart.Series[2] as ChartSerieImpl;
		}
		IRange range = null;
		IRange range2 = null;
		IRange range3 = null;
		IRange range4 = null;
		object[] array2 = null;
		if (isCandleChart)
		{
			array2 = chartSerieImpl.EnteredDirectlyValues;
		}
		object[] enteredDirectlyValues = chartSerieImpl2.EnteredDirectlyValues;
		object[] enteredDirectlyValues2 = chartSerieImpl3.EnteredDirectlyValues;
		object[] enteredDirectlyValues3 = chartSerieImpl4.EnteredDirectlyValues;
		IRange range5 = null;
		object[] enteredDirectlyCategoryLabels = serie.EnteredDirectlyCategoryLabels;
		List<int> list3 = null;
		if (isCandleChart)
		{
			range = ((chartSerieImpl.FilteredValue == null) ? chartSerieImpl.ValuesIRange : null);
		}
		range2 = ((chartSerieImpl2.FilteredValue == null) ? chartSerieImpl2.ValuesIRange : null);
		range3 = ((chartSerieImpl3.FilteredValue == null) ? chartSerieImpl3.ValuesIRange : null);
		range4 = ((chartSerieImpl4.FilteredValue == null) ? chartSerieImpl4.ValuesIRange : null);
		range5 = serie.CategoryLabelsIRange;
		range = (CheckIfValidValueRange(range) ? range : null);
		range2 = (CheckIfValidValueRange(range2) ? range2 : null);
		range3 = (CheckIfValidValueRange(range3) ? range3 : null);
		range4 = (CheckIfValidValueRange(range4) ? range4 : null);
		range5 = (CheckIfValidValueRange(range5) ? range5 : null);
		WorksheetImpl worksheet = null;
		WorksheetImpl worksheet2 = null;
		WorksheetImpl worksheet3 = null;
		WorksheetImpl worksheet4 = null;
		WorksheetImpl worksheetImpl = null;
		if (range != null)
		{
			worksheet = range.Worksheet as WorksheetImpl;
		}
		if (range2 != null)
		{
			worksheet2 = range2.Worksheet as WorksheetImpl;
		}
		if (range3 != null)
		{
			worksheet3 = range3.Worksheet as WorksheetImpl;
		}
		if (range4 != null)
		{
			worksheet4 = range4.Worksheet as WorksheetImpl;
		}
		if (range5 != null)
		{
			worksheetImpl = range5.Worksheet as WorksheetImpl;
		}
		ChartCategoryAxisImpl chartCategoryAxisImpl = (serie.UsePrimaryAxis ? serie.ParentChart.PrimaryCategoryAxis : serie.ParentChart.SecondaryCategoryAxis) as ChartCategoryAxisImpl;
		ChartValueAxisImpl valueAxis = (serie.UsePrimaryAxis ? serie.ParentChart.PrimaryValueAxis : serie.ParentChart.SecondaryValueAxis) as ChartValueAxisImpl;
		double num2 = 1.0;
		num2 = GetDisplayUnitValue(valueAxis);
		string numberFormat = chartCategoryAxisImpl.NumberFormat;
		string text = ((range5 == null) ? (string.IsNullOrEmpty(serie.CategoriesFormatCode) ? "General" : serie.CategoriesFormatCode) : ((range5.Cells.Length >= 1) ? range5.Cells[0].NumberFormat : "General"));
		ChartPointValueType chartPointValueType = ChartPointValueType.TextAxisValue;
		bool valueContainsAnyString = ValuesContainsAnyString(range5, enteredDirectlyCategoryLabels, list2, worksheetImpl);
		if (list2 != null && list2.Contains(0))
		{
			int j;
			for (j = 0; list2.Contains(j) && j < list.Count; j++)
			{
			}
			text = ((j < range5.Cells.Length) ? range5.Cells[j].NumberFormat : "General");
		}
		chartPointValueType = ((enteredDirectlyCategoryLabels != null || range5 != null) ? GetChartPointValueType(isBubbleOrScatter: false, valueContainsAnyString, text, numberFormat, parentChart, chartCategoryAxisImpl) : ChartPointValueType.DefaultIndexValueWithAxisNumFmt);
		bool flag = false;
		string dataLabelNumberFormat = null;
		if ((serie.DataPoints.DefaultDataPoint as ChartDataPointImpl).HasDataLabels)
		{
			ChartDataLabelsImpl chartDataLabelsImpl = serie.DataPoints.DefaultDataPoint.DataLabels as ChartDataLabelsImpl;
			bool flag2 = (serie.DataPoints as ChartDataPointsCollection).CheckDPDataLabels();
			if ((chartDataLabelsImpl != null && !chartDataLabelsImpl.IsDelete && chartDataLabelsImpl.IsCategoryName) || flag2)
			{
				flag = true;
				dataLabelNumberFormat = chartDataLabelsImpl.NumberFormat;
			}
		}
		int num3 = ((!isCandleChart) ? ((range2 != null) ? range2.Cells.Length : enteredDirectlyValues.Length) : ((range != null) ? range.Cells.Length : array2.Length));
		bool flag3 = chart.ChartType.ToString().Contains("Stock");
		int num4 = 0;
		int k = 0;
		while (num4 < num3)
		{
			if (((chartPointValueType == ChartPointValueType.ScatterXAxisValue) ? showEmptyPoints : (chartPointValueType == ChartPointValueType.DateTimeAxisValue)) && enteredDirectlyCategoryLabels != null && k < enteredDirectlyCategoryLabels.Length && enteredDirectlyCategoryLabels[k] == null)
			{
				if (list3 == null)
				{
					list3 = new List<int>(list.Count);
				}
				list3.Add(num4);
				k++;
			}
			else
			{
				IRange range6 = null;
				if (isCandleChart)
				{
					values[num4].Open = GetValueForStockCharts(range, array2, worksheet, num4, num2);
					values[num4].High = GetValueForStockCharts(range2, enteredDirectlyValues, worksheet2, num4, num2);
					values[num4].Low = GetValueForStockCharts(range3, enteredDirectlyValues2, worksheet3, num4, num2);
					values[num4].Close = GetValueForStockCharts(range4, enteredDirectlyValues3, worksheet4, num4, num2);
				}
				else
				{
					List<double> list4 = new List<double>(3);
					list4.Add(GetValueForStockCharts(range2, enteredDirectlyValues, worksheet2, num4, num2));
					list4.Add(GetValueForStockCharts(range3, enteredDirectlyValues2, worksheet3, num4, num2));
					list4.Add(GetValueForStockCharts(range4, enteredDirectlyValues3, worksheet4, num4, num2));
					list4.Sort();
					values[num4].Low = list4[0];
					values[num4].High = list4[2];
					list4.Clear();
					list4 = null;
				}
				object categoryValue = "";
				object dataLabelResult = null;
				if (range5 != null && num4 < range5.Cells.Length)
				{
					range6 = range5.Cells[num4];
				}
				else if (enteredDirectlyCategoryLabels != null && num4 < enteredDirectlyCategoryLabels.Length)
				{
					categoryValue = enteredDirectlyCategoryLabels[num4];
				}
				if (list2 != null)
				{
					if (k < range5.Cells.Length)
					{
						for (; list2.Contains(k) && k < list.Count; k++)
						{
						}
					}
					if (num4 != k)
					{
						range6 = ((k >= range5.Cells.Length) ? null : range5.Cells[k]);
					}
				}
				if (list2 == null || range6 != null)
				{
					list[num4].X = GetValueFromCell(range6, categoryValue, worksheetImpl, num4, chartPointValueType, numberFormat, text, 1.0, flag, out dataLabelResult, dataLabelNumberFormat);
					if (flag && chartPointValueType != ChartPointValueType.TextAxisValue && chartPointValueType != ChartPointValueType.DefaultIndexValue)
					{
						list[num4].Size = ((dataLabelResult != null) ? dataLabelResult : "");
					}
					if (chartPointValueType == ChartPointValueType.DateTimeAxisValue && range6 is RangeImpl && (range6 as RangeImpl).CellType == RangeImpl.TCellType.Blank && flag3)
					{
						list[num4].IsSummary = true;
					}
				}
				else
				{
					if (flag && chartPointValueType != ChartPointValueType.TextAxisValue)
					{
						list[num4].Size = "";
					}
					if (chartPointValueType == ChartPointValueType.TextAxisValue || chartPointValueType == ChartPointValueType.TextAxisValueWithNumFmt)
					{
						list[num4].X = "";
					}
					else
					{
						list[num4].X = 0;
					}
				}
			}
			num4++;
			k++;
		}
		if (list3 != null)
		{
			IList<ChartPointInternal> list5 = new List<ChartPointInternal>();
			int num5 = 0;
			foreach (ChartPointInternal item in list)
			{
				if (!list3.Contains(num5))
				{
					list5.Add(item);
				}
				num5++;
			}
			list = list5;
		}
		SetDateTimeIntervalType(serie, list, isStock: true);
		return list;
	}

	private double GetValueForStockCharts(IRange range, object[] directValues, WorksheetImpl worksheet, int index, double displayUnit)
	{
		double num = 0.0;
		if (range != null && index < range.Cells.Length)
		{
			IRange cell = range.Cells[index];
			num = GetdoubleValueFromCell(cell, worksheet, out var _);
		}
		else if (directValues != null && index < directValues.Length)
		{
			num = ((directValues[index] != null && !(directValues[index] is string)) ? Convert.ToDouble(directValues[index]) : 0.0);
		}
		if (num.ToString() == double.NaN.ToString())
		{
			num = 0.0;
		}
		if (displayUnit > 1.0)
		{
			num /= displayUnit;
		}
		return num;
	}

	internal Dictionary<int, IList<ChartPointInternal>> GetListofPoints(ChartImpl chart, out Dictionary<int, string> names)
	{
		Dictionary<int, IList<ChartPointInternal>> dictionary = new Dictionary<int, IList<ChartPointInternal>>(chart.Series.Count);
		names = new Dictionary<int, string>(chart.Series.Count);
		string text = chart.ChartType.ToString();
		bool flag = text.Contains("Combination_Chart");
		bool flag2 = false;
		bool flag3 = false;
		bool showEmptyPoints = false;
		int emptyPointValue = -1;
		switch ((int)chart.DisplayBlanksAs)
		{
		case 0:
			showEmptyPoints = false;
			break;
		case 1:
			showEmptyPoints = true;
			emptyPointValue = 0;
			break;
		case 2:
			showEmptyPoints = true;
			break;
		}
		foreach (IOfficeChartSerie item in chart.Series)
		{
			if (item.IsFiltered && !IsChartEx)
			{
				continue;
			}
			ChartSerieImpl chartSerieImpl = item as ChartSerieImpl;
			bool flag4 = chartSerieImpl.ValuesIRange == null && chartSerieImpl.EnteredDirectlyValues == null;
			if (flag3 && flag && (chart.PrimaryFormats.Count != 1 || chart.SecondaryFormats.Count != 0))
			{
				break;
			}
			string text2 = chartSerieImpl.SerieType.ToString();
			flag3 = text2.Contains("Radar");
			if (flag4)
			{
				dictionary.Add(chartSerieImpl.Index, null);
			}
			else
			{
				ViewModel viewModel = GetViewModel(chartSerieImpl);
				if (text2.Contains("Line") || text2.Contains("Radar"))
				{
					emptyPointValue = 1;
				}
				if (text.Contains("Stock"))
				{
					dictionary.Add(chartSerieImpl.Index, GetChartPointsValuesForStockChart(showEmptyPoints, chart, chartSerieImpl, viewModel.Products, text.Contains("Open")));
					break;
				}
				dictionary.Add(chartSerieImpl.Index, GetChartPointsValues(chartSerieImpl, showEmptyPoints, emptyPointValue, text2.Contains("Bubble"), viewModel.Products));
			}
			if (flag2 || IsChartEx)
			{
				break;
			}
		}
		foreach (IOfficeChartSerie item2 in chart.Series)
		{
			ChartSerieImpl chartSerieImpl2 = item2 as ChartSerieImpl;
			names.Add(chartSerieImpl2.Index, GetSerieName(chartSerieImpl2));
		}
		return dictionary;
	}

	internal ChartSeries SfBarseries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> products = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Bar;
		ChartSeriesCommon(serie, chartSeries, products, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfBarseries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Bar;
		ChartSeriesCommon(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackBar100Series(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingBar100;
		ChartSeriesCommon(serie, chartSeries, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackBar100_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfAreaSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Area;
		ChartSeriesCommon(serie, chartSeries);
		SetErrorBarConfiguration(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfAreaSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Area;
		ChartSeriesCommon(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfColumnSeries(ChartSerieImpl serie, ChartControl control)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Column;
		ChartSeriesCommon(serie, chartSeries, viewModel.Products, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfColumnSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Column;
		ChartSeriesCommon(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfLineSeries(ChartSerieImpl serie, ChartControl control)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeries chartSeries = new ChartSeries(control);
		string text = serie.SerieType.ToString();
		if (text.Contains("Scatter"))
		{
			chartSeries.Type = ChartSeriesType.Scatter;
			chartSeries.Style.Interior = null;
			chartSeries.ScatterConnectType = ScatterConnectType.Line;
		}
		else if (text.Contains("Stacked_100"))
		{
			chartSeries.Type = ChartSeriesType.StackingLine100;
			chartSeries.StackingGroup = "SfStackLine100_" + (serie.UsePrimaryAxis ? "1" : "2");
		}
		else
		{
			chartSeries.Type = ChartSeriesType.Line;
		}
		ChartSeriesCommon(serie, chartSeries, viewModel.Products, applyFill: false, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		DashStyle dashStyle = DashStyle.Solid;
		if (GetStrokeDashArrayValues(serie.SerieFormat.LineProperties.LinePattern, out dashStyle))
		{
			chartSeries.Style.Border.DashStyle = dashStyle;
		}
		if (serie.PointNumber != 0)
		{
			SetArrows(serie, chartSeries, viewModel.Products.Count);
		}
		DashCap cap = DashCap.Flat;
		GetDashCapStyle((DocGen.Drawing.LineCap)(serie.SerieFormat.LineProperties as ChartBorderImpl).CapStyle, out cap);
		chartSeries.Style.Border.DashCap = cap;
		GetBorderOnLineSeries(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfLineSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Line;
		ChartSeriesCommon(serie, chartSeries, applyFill: true);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfSplineSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		string text = serie.SerieType.ToString();
		if (text.Contains("Scatter"))
		{
			chartSeries.Type = ChartSeriesType.Scatter;
			chartSeries.Style.Interior = null;
			chartSeries.ScatterConnectType = ScatterConnectType.Spline;
		}
		else if (text.Contains("Stacked_100"))
		{
			chartSeries.Type = ChartSeriesType.StackingLine100;
			chartSeries.StackingGroup = "SfStackLine100_" + (serie.UsePrimaryAxis ? "1" : "2");
		}
		else
		{
			chartSeries.Type = ChartSeriesType.Spline;
		}
		ChartSeriesCommon(serie, chartSeries, applyFill: false);
		SetErrorBarConfiguration(serie, chartSeries);
		DashStyle dashStyle = DashStyle.Solid;
		if (GetStrokeDashArrayValues(serie.SerieFormat.LineProperties.LinePattern, out dashStyle))
		{
			chartSeries.Style.Border.DashStyle = dashStyle;
		}
		DashCap cap = DashCap.Flat;
		GetDashCapStyle((DocGen.Drawing.LineCap)(serie.SerieFormat.LineProperties as ChartBorderImpl).CapStyle, out cap);
		chartSeries.Style.Border.DashCap = cap;
		SfChartDataLabel(serie, chartSeries);
		if (serie.PointNumber != 0)
		{
			SetArrows(serie, chartSeries, GetViewModel(serie).Products.Count);
		}
		GetBorderOnLineSeries(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		return chartSeries;
	}

	private bool GetDashCapStyle(DocGen.Drawing.LineCap lineStyle, out DashCap cap)
	{
		switch (lineStyle)
		{
		case DocGen.Drawing.LineCap.Triangle:
			cap = DashCap.Round;
			return true;
		case DocGen.Drawing.LineCap.Square:
			cap = DashCap.Square;
			return true;
		case DocGen.Drawing.LineCap.Round:
			cap = DashCap.Round;
			return true;
		case DocGen.Drawing.LineCap.Flat:
			cap = DashCap.Flat;
			return true;
		default:
			cap = DashCap.Round;
			return false;
		}
	}

	internal ChartSeries SfPieSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Pie;
		ChartSeriesCommon(serie, chartSeries, applyFill: false);
		ApplyPieConfiguration(chartSeries, serie);
		BrushInfo borderBrush = null;
		double borderThickness = 0.0;
		GetFillOnPieDougnutSeries(serie, chartSeries, isPie: true, out borderBrush, out borderThickness);
		if (borderBrush != null && borderThickness != 0.0)
		{
			chartSeries.Style.Border.Color = borderBrush.BackColor;
			chartSeries.Style.Border.Width = (float)borderThickness;
		}
		else
		{
			GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		}
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	private void ApplyPieConfiguration(ChartSeries pieSerie, ChartSerieImpl serie)
	{
		int num = 0;
		if (serie.ParentChart.ChartType != OfficeChartType.PieOfPie && serie.ParentChart.ChartType != OfficeChartType.Pie_Bar)
		{
			num = serie.SerieFormat.CommonSerieOptions.FirstSliceAngle;
		}
		pieSerie.ConfigItems.PieItem.AngleOffset = num + PieAngle;
		if (serie.SerieFormat.Percent != 0)
		{
			pieSerie.ExplodedAll = true;
			float num2 = serie.SerieFormat.Percent;
			if (num2 > 400f)
			{
				num2 = 400f;
			}
			pieSerie.ExplodedAll = true;
			pieSerie.ExplosionOffset = num2 * 96f / 400f;
		}
		else if (serie.SerieType.ToString().Contains("Pie_Exploded"))
		{
			pieSerie.ExplodedAll = true;
			pieSerie.ExplosionOffset = 20f;
		}
	}

	internal ChartSeries SfPieSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Pie;
		ChartSeriesCommon(serie, chartSeries, applyFill: false);
		BrushInfo borderBrush = null;
		double borderThickness = 0.0;
		GetFillOnPieDougnutSeries(serie, chartSeries, isPie: true, out borderBrush, out borderThickness);
		if (borderBrush != null && borderThickness != 0.0)
		{
			chartSeries.Style.Border.Color = borderBrush.BackColor;
			chartSeries.Style.Border.Width = (float)borderThickness;
		}
		else
		{
			GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		}
		SfChartDataLabel(serie, chartSeries);
		ApplyPieConfiguration(chartSeries, serie);
		return chartSeries;
	}

	internal ChartSeries SfDoughnutSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Pie;
		ChartSeriesCommon(serie, chartSeries, applyFill: false);
		ApplyPieConfiguration(chartSeries, serie);
		float doughnutCoeficient = (float)serie.SerieFormat.CommonSerieOptions.DoughnutHoleSize / 100f;
		chartSeries.ConfigItems.PieItem.DoughnutCoeficient = doughnutCoeficient;
		BrushInfo borderBrush = null;
		double borderThickness = 0.0;
		GetFillOnPieDougnutSeries(serie, chartSeries, isPie: true, out borderBrush, out borderThickness);
		if (borderBrush != null && borderThickness != 0.0)
		{
			chartSeries.Style.Border.Color = borderBrush.BackColor;
			chartSeries.Style.Border.Width = (float)borderThickness;
		}
		else
		{
			GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		}
		SfChartDataLabel(serie, chartSeries);
		chartSeries.Name = GetSerieName(serie);
		return chartSeries;
	}

	internal ChartSeries SfStackAreaSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingArea;
		ChartSeriesCommon(serie, chartSeries);
		SetErrorBarConfiguration(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackArea_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfStackAreaSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingArea;
		ChartSeriesCommon(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackArea100Series(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingArea100;
		ChartSeriesCommon(serie, chartSeries);
		SetErrorBarConfiguration(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackArea100_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfStackArea100Series3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingArea100;
		ChartSeriesCommon(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackBarSeries(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingBar;
		ChartSeriesCommon(serie, chartSeries, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackBar_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfStackBarSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingBar;
		ChartSeriesCommon(serie, chartSeries);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackBar100Series3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingBar100;
		ChartSeriesCommon(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackedColumnSeries(ChartSerieImpl serie, ChartControl control)
	{
		_ = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingColumn;
		ChartSeriesCommon(serie, chartSeries, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackColumn_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfStackedColumnSeries3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingColumn;
		ChartSeriesCommon(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfStackColum100Series(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingColumn100;
		ChartSeriesCommon(serie, chartSeries, out var _);
		SetErrorBarConfiguration(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.StackingGroup = "SfStackColumn100_" + (serie.UsePrimaryAxis ? "1" : "2");
		return chartSeries;
	}

	internal ChartSeries SfStackColum100Series3D(ChartSerieImpl serie, ChartControl control)
	{
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.StackingColumn100;
		ChartSeriesCommon(serie, chartSeries);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		return chartSeries;
	}

	internal ChartSeries SfRadarSeries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> products = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Radar;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeries);
		if (serie.UsePrimaryAxis && control.PrimaryXAxis.ValueType == ChartValueType.DateTime)
		{
			control.PrimaryXAxis.ValueType = ChartValueType.Category;
		}
		TryAnsEmptyPointDisplayInChart(serie, chartSeries);
		products = GetChartPointsValues(serie, chartSeries.ParentChart.AllowGapForEmptyPoints, (chartSeries.EmptyPointValue == EmptyPointValue.Average) ? 1 : 0, isBubbles: false, products);
		products = TryAndRemoveFirstEmptyPointsOnSeries(isLine: true, serie, chartSeries, products);
		chartSeries.SetItemSource(products);
		Dictionary<double, string> dictionary = new Dictionary<double, string>();
		for (int i = 0; i < products.Count; i++)
		{
			dictionary.Add(i + 1, products[i].X.ToString());
		}
		if (chartSeries.ParentChart.PrimaryXAxis.ValueType == ChartValueType.Category)
		{
			AxisLabelConverter axisLabelConverter = new AxisLabelConverter();
			axisLabelConverter.CustomRadarAxisLabels = dictionary;
			chartSeries.ParentChart.PrimaryXAxis.AxisLabelConverter = axisLabelConverter;
		}
		ChartBorderImpl chartBorderImpl = serie.SerieFormat.LineProperties as ChartBorderImpl;
		bool flag = true;
		if (parentWorkbook.Version == OfficeVersion.Excel97to2003)
		{
			flag = false;
		}
		BrushInfo brushInfo = null;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			if (!chartBorderImpl.AutoFormat)
			{
				brushInfo = GetBrushFromBorder(chartBorderImpl);
			}
			else if (!serie.SerieType.ToString().Contains("Radar_Filled"))
			{
				brushInfo = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out var color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(serie.Number, serie.ParentSeries.Count, !flag, isColorPalette: true))) : new BrushInfo(SfColor(color)));
			}
		}
		else
		{
			brushInfo = new BrushInfo(SfColor(Color.Transparent, 1.0));
		}
		switch (serie.SerieType)
		{
		case OfficeChartType.Radar:
		case OfficeChartType.Radar_Markers:
			chartSeries.ConfigItems.RadarItem.Type = ChartRadarDrawType.Line;
			if (!TryAndUpdateSegmentsColorsInLineSeries(serie, chartSeries, brushInfo, flag))
			{
				chartSeries.Style.Border.Color = brushInfo.BackColor;
				chartSeries.Style.Interior = new BrushInfo(brushInfo.BackColor);
			}
			break;
		case OfficeChartType.Radar_Filled:
		{
			chartSeries.ConfigItems.RadarItem.Type = ChartRadarDrawType.Area;
			BrushInfo brushInfo2 = null;
			brushInfo2 = ((!(serie.SerieFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || !flag) ? GetBrushFromDataFormat(serie.SerieFormat) : ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out var color2)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(serie.Number, serie.ParentSeries.Count, isBinary: false, isColorPalette: true))) : new BrushInfo(SfColor(color2))));
			if (brushInfo2 != null)
			{
				chartSeries.Style.Interior = brushInfo2;
			}
			if (brushInfo == null && chartBorderImpl.AutoFormat && !flag)
			{
				brushInfo = new BrushInfo(SfColor(0, 0, 0));
			}
			if (brushInfo2 != null)
			{
				chartSeries.Style.Interior = brushInfo2;
			}
			if (brushInfo == null && chartBorderImpl.AutoFormat && !flag)
			{
				brushInfo = new BrushInfo(SfColor(0, 0, 0, 1.0));
			}
			break;
		}
		}
		if (brushInfo != null)
		{
			chartSeries.Style.Border.Color = brushInfo.BackColor;
			chartSeries.Style.Border.Width = GetBorderThickness(chartBorderImpl);
			if (chartSeries.Style.Border.Width == 0f || chartBorderImpl.LineWeightString == null)
			{
				if (chartSeries.ConfigItems.RadarItem.Type == ChartRadarDrawType.Line && TryAndGetThicknessBasedOnElement(ChartElementsEnum.DataPointLineThickness, serie.ParentChart, out var thickness, null))
				{
					chartSeries.Style.Border.Width = thickness;
				}
				else if (chartSeries.ConfigItems.RadarItem.Type == ChartRadarDrawType.Area && !flag)
				{
					chartSeries.Style.Border.Width = 1f;
				}
				else
				{
					chartSeries.Style.Border.Width = (flag ? 2f : 1f);
				}
			}
		}
		else
		{
			chartSeries.Style.Border.Width = 0f;
		}
		SfChartDataLabel(serie, chartSeries);
		chartSeries.Name = GetSerieName(serie);
		return chartSeries;
	}

	internal ChartSeries SfScatterrSeries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> products = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Scatter;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeries);
		chartSeries.Style.Interior = null;
		chartSeries.Style.Border.Color = SfColor(Color.Transparent, 1.0);
		TryAnsEmptyPointDisplayInChart(serie, chartSeries);
		products = GetChartPointsValues(serie, chartSeries.ParentChart.AllowGapForEmptyPoints, (chartSeries.EmptyPointValue == EmptyPointValue.Average) ? 1 : 0, isBubbles: false, products);
		chartSeries.SetItemSource(products);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		SetErrorBarConfiguration(serie, chartSeries);
		chartSeries.Name = GetSerieName(serie);
		return chartSeries;
	}

	internal ChartSeries SfBubbleSeries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> products = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Bubble;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeries);
		products = GetChartPointsValues(serie, chartSeries.ParentChart.AllowGapForEmptyPoints, (chartSeries.EmptyPointValue == EmptyPointValue.Average) ? 1 : 0, isBubbles: true, products);
		TryAnsEmptyPointDisplayInChart(serie, chartSeries);
		chartSeries.SetItemSource(products);
		GetFillOnCommonSeries(serie, chartSeries, out var _);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.ConfigItems.BubbleItem.BubbleType = ChartBubbleType.Circle;
		chartSeries.Name = GetSerieName(serie);
		return chartSeries;
	}

	internal ChartSeries SfCandleSeries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> list = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Candle;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeries);
		chartSeries.Index = serie.Index;
		chartSeries.SerieType = serie.SerieType.ToString();
		if (serie.Index == 0)
		{
			list = (base.ListOfPoints.ContainsKey(serie.Index) ? base.ListOfPoints[serie.Index] : GetChartPointsValuesForStockChart(chartSeries.ParentChart.AllowGapForEmptyPoints, serie.ParentChart, serie, list, isCandleChart: true));
			chartSeries.SetItemSource(list);
		}
		ChartFormatImpl chartFormatImpl = serie.SerieFormat.CommonSerieOptions as ChartFormatImpl;
		ChartDropBarImpl chartDropBarImpl = (chartFormatImpl.IsDropBar ? (chartFormatImpl.FirstDropBar as ChartDropBarImpl) : null);
		ChartDropBarImpl chartDropBarImpl2 = ((chartDropBarImpl != null) ? (chartFormatImpl.SecondDropBar as ChartDropBarImpl) : null);
		if (chartDropBarImpl == null)
		{
			IOfficeChartSerie officeChartSerie = null;
			officeChartSerie = serie.ParentSeries.FirstOrDefault((IOfficeChartSerie x) => (x.SerieFormat.CommonSerieOptions as ChartFormatImpl).IsDropBar);
			if (officeChartSerie != null)
			{
				chartFormatImpl = officeChartSerie.SerieFormat.CommonSerieOptions as ChartFormatImpl;
			}
			chartDropBarImpl = chartFormatImpl.FirstDropBar as ChartDropBarImpl;
			chartDropBarImpl2 = chartFormatImpl.SecondDropBar as ChartDropBarImpl;
		}
		BrushInfo brushInfo = null;
		BrushInfo brushInfo2 = null;
		brushInfo = ((!chartDropBarImpl.IsAutomaticFormat) ? GetBrushFromDataFormat(chartDropBarImpl) : ((!TryAndGetColorBasedOnElement(ChartElementsEnum.UpBarFill, serie.ParentChart, out var color)) ? new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue)) : new BrushInfo(SfColor(color))));
		brushInfo2 = ((!chartDropBarImpl2.IsAutomaticFormat) ? GetBrushFromDataFormat(chartDropBarImpl2) : ((!TryAndGetColorBasedOnElement(ChartElementsEnum.DownBarFill, serie.ParentChart, out color)) ? new BrushInfo(SfColor(51, 51, 51)) : new BrushInfo(SfColor(color))));
		ChartStyleInfoIndexer styles = chartSeries.Styles;
		if (chartSeries.Points.Count > 0 && chartSeries.Points.Count == list.Count)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Open > (double)list[i].Close)
				{
					styles[i].Interior = brushInfo2;
				}
				else
				{
					styles[i].Interior = brushInfo;
				}
			}
		}
		chartSeries.Style.Interior = brushInfo2;
		ChartBorderImpl chartBorderImpl = chartDropBarImpl2.LineProperties as ChartBorderImpl;
		double num = 0.0;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			BrushInfo brushInfo3 = null;
			brushInfo3 = ((!chartBorderImpl.AutoFormat || !TryAndGetColorBasedOnElement(ChartElementsEnum.OtherLines, serie.ParentChart, out var color2)) ? GetBrushFromBorder(chartBorderImpl) : new BrushInfo(SfColor(color2)));
			if (brushInfo3 != null)
			{
				chartSeries.Style.Border.Color = brushInfo3.BackColor;
			}
			num = GetBorderThickness(chartBorderImpl);
			num = ((num < 1.0) ? 1.0 : num);
		}
		chartSeries.Style.Border.Width = (float)num;
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.Name = serie.Name;
		return chartSeries;
	}

	internal ChartSeries SfStockHiLoSeries(ChartSerieImpl serie, ChartControl control)
	{
		IList<ChartPointInternal> products = GetViewModel(serie).Products;
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.HiLo;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeries);
		chartSeries.Index = serie.Index;
		chartSeries.SerieType = serie.SerieType.ToString();
		if (serie.Index == 0)
		{
			products = (base.ListOfPoints.ContainsKey(serie.Index) ? base.ListOfPoints[serie.Index] : GetChartPointsValuesForStockChart(chartSeries.ParentChart.AllowGapForEmptyPoints, serie.ParentChart, serie, products, isCandleChart: false));
			chartSeries.SetItemSource(products);
		}
		ChartFormatImpl chartFormatImpl = null;
		chartFormatImpl = ((serie.SerieType != 0 || serie.ParentChart.SecondaryFormats.InnerList.Count <= 0) ? (serie.SerieFormat.CommonSerieOptions as ChartFormatImpl) : serie.ParentChart.SecondaryFormats.InnerList[0]);
		if (chartFormatImpl.HasHighLowLines && chartFormatImpl.HighLowLines != null)
		{
			if (chartFormatImpl.HighLowLines.LinePattern != OfficeChartLinePattern.None)
			{
				double num = 1.0;
				ChartBorderImpl chartBorderImpl = chartFormatImpl.HighLowLines as ChartBorderImpl;
				BrushInfo brushInfo = null;
				brushInfo = ((!chartBorderImpl.AutoFormat || !TryAndGetColorBasedOnElement(ChartElementsEnum.OtherLines, serie.ParentChart, out var color)) ? GetBrushFromBorder(chartBorderImpl) : new BrushInfo(SfColor(color)));
				if (brushInfo != null)
				{
					chartSeries.Style.Interior = brushInfo;
				}
				else
				{
					chartSeries.Style.Interior = new BrushInfo(SfColor(0, 0, 0, 1.0));
				}
				num = GetBorderThickness(chartBorderImpl);
				chartSeries.Style.Border.Width = (float)((num < 1.0) ? 1.0 : num);
			}
			else
			{
				chartSeries.Style.Border.Width = 0f;
			}
		}
		else
		{
			chartSeries.Style.Interior = new BrushInfo(SfColor(0, 0, 0, 1.0));
			chartSeries.Style.Border.Width = 1f;
		}
		SfChartDataLabel(serie, chartSeries);
		SfChartTrendLine(serie, chartSeries);
		chartSeries.Name = serie.Name;
		return chartSeries;
	}

	internal ChartSeries SfFunnelSeries(ChartSerieImpl serie, ChartControl control)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.ColumnRange;
		chartSeries.Rotate = true;
		GetChartPointsValues(serie, showEmptyPoints: false, -1, isBubbles: false, viewModel.Products);
		IList<ChartPointInternal> inPoints = ChangeFunnelItemsSourceToRangeColumn(viewModel.Products);
		chartSeries.SetItemSource(inPoints);
		GetFillOnCommonSeries(serie, chartSeries, out var negativeIndexes);
		chartSeries.Name = GetSerieName(serie);
		SetGapWidthandOverlap(chartSeries, serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		negativeIndexes = null;
		return chartSeries;
	}

	private void GetFillOnWaterFallSeries(ChartSerieImpl serie, ChartSeries waterfall, ViewModel model)
	{
		int deninedDPCount = (serie.DataPoints as ChartDataPointsCollection).DeninedDPCount;
		_ = 0;
		if ((serie.SerieFormat as ChartSerieDataFormatImpl).IsAutomaticFormat)
		{
			new BrushInfo(SfColor(91, 155, 213));
			new BrushInfo(SfColor(237, 125, 49));
			new BrushInfo(SfColor(165, 165, 165));
			_ = 0;
		}
		else
		{
			GetBrushFromDataFormat(serie.SerieFormat);
		}
	}

	internal ChartSeries SfHistogramOrPareto(ChartSerieImpl serie, ChartImpl _chart, out ChartSeries paretoLine, ChartControl control)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeries chartSeries = new ChartSeries(control);
		viewModel.Products = GetChartPointsForHistogram(serie, viewModel.Products);
		chartSeries.SetItemSource(viewModel.Products);
		SetGapWidthandOverlap(chartSeries, serie);
		GetFillOnCommonSeries(serie, chartSeries, out var _);
		chartSeries.Name = GetSerieName(serie);
		GetBorderOnCommonSeries(serie.SerieFormat.LineProperties as ChartBorderImpl, chartSeries);
		SfChartDataLabel(serie, chartSeries);
		if (serie.ParetoLineFormat != null && chartSeries.Points.Count > 1 && !serie.IsParetoLineHidden)
		{
			paretoLine = SfParetoLine(serie, chartSeries, viewModel.Products, chartSeries.ParentChart);
		}
		else
		{
			paretoLine = null;
		}
		return chartSeries;
	}

	private ChartSeries SfParetoLine(ChartSerieImpl serie, ChartSeries column, IList<ChartPointInternal> observableCollection, ChartControl control)
	{
		ViewModel viewModel = new ViewModel(observableCollection.Count);
		ChartSeries chartSeries = new ChartSeries(control);
		chartSeries.Type = ChartSeriesType.Line;
		double num = observableCollection.Sum((ChartPointInternal x) => x.Value);
		double num2 = 0.0;
		for (int i = 0; i < observableCollection.Count; i++)
		{
			viewModel.Products[i].X = observableCollection[i].X;
			viewModel.Products[i].Value = (num2 + observableCollection[i].Value) / num;
			num2 += observableCollection[i].Value;
		}
		chartSeries.SetItemSource(viewModel.Products);
		ChartBorderImpl chartBorderImpl = serie.ParetoLineFormat.LineProperties as ChartBorderImpl;
		BrushInfo brushInfo = null;
		if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			brushInfo = (chartBorderImpl.IsAutoLineColor ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor((serie.ParetoLineFormatIndex == -1) ? 1 : serie.ParetoLineFormatIndex, serie.ParentSeries.Count, isBinary: false, isColorPalette: true))) : GetBrushFromBorder(chartBorderImpl));
			if (brushInfo != null)
			{
				chartSeries.Style.Interior = brushInfo;
				chartSeries.Style.Border.Color = brushInfo.BackColor;
			}
			chartSeries.Style.Border.Width = GetBorderThickness(chartBorderImpl);
			if (chartSeries.Style.Border.Width < 1f)
			{
				chartSeries.Style.Border.Width = 2.25f;
			}
		}
		else
		{
			chartSeries.Style.Border.Width = 0f;
		}
		ChartAxisImpl chartAxisImpl = serie.ParentChart.SecondaryValueAxis as ChartAxisImpl;
		if (!chartAxisImpl.Deleted)
		{
			ChartAxis chartAxis = new ChartAxis
			{
				DrawGrid = false,
				OpposedPosition = true,
				Orientation = ChartOrientation.Vertical
			};
			chartSeries.XAxis = chartSeries.ParentChart.PrimaryXAxis;
			chartSeries.YAxis = chartAxis;
			chartSeries.YAxis.Range.Interval = 0.1;
			chartSeries.YAxis.Range.Max = 1.0;
			chartSeries.ParentChart.Axes.Add(chartAxis);
			SfSecondaryAxisCommon(serie.ParentChart, null, chartSeries.YAxis);
			if (chartAxisImpl.HasAxisTitle)
			{
				new RectangleF(-1f, -1f, -1f, -1f);
			}
			chartSeries.YAxis.OpposedPosition = true;
		}
		chartSeries.Name = "Pareto";
		return chartSeries;
	}

	private BrushInfo GetCommonColorBrush(ChartStyleInfoIndexer customBrushes, List<int> customDptIndexes)
	{
		BrushInfo interior = customBrushes[0].Interior;
		if (customBrushes.Count != customDptIndexes.Count)
		{
			for (int i = 0; i < customDptIndexes.Count; i++)
			{
				int num = i - 1;
				int num2 = i + 1;
				if (!customDptIndexes.Contains(num) && num > 0)
				{
					interior = customBrushes[num].Interior;
					break;
				}
				if (!customDptIndexes.Contains(num2) && num2 < customBrushes.Count)
				{
					interior = customBrushes[num2].Interior;
					break;
				}
			}
		}
		return interior;
	}

	private void GetFillOnCommonSeries(ChartSerieImpl serie, ChartSeries sfChartSerie, out List<int> negativeIndexes)
	{
		BrushInfo brushInfo = null;
		negativeIndexes = null;
		bool flag = true;
		if (parentWorkbook.Version == OfficeVersion.Excel97to2003 && !IsChartEx)
		{
			flag = false;
		}
		BrushInfo brushInfo2 = null;
		ChartPointIndexer points = sfChartSerie.Points;
		if (!(serie.SerieFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || !flag)
		{
			brushInfo = GetBrushFromDataFormat(serie.SerieFormat);
		}
		if (brushInfo == null)
		{
			brushInfo2 = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out var color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor((serie.Number != -1) ? serie.Number : 0, serie.ParentSeries.Count, isBinary: false, isColorPalette: false))) : new BrushInfo(SfColor(color)));
			brushInfo = brushInfo2;
		}
		sfChartSerie.Style.Interior = brushInfo;
		ChartDataPointsCollection chartDataPointsCollection = serie.DataPoints as ChartDataPointsCollection;
		int deninedDPCount = chartDataPointsCollection.DeninedDPCount;
		int num = ((serie.ValuesIRange != null) ? serie.ValuesIRange.Count : ((serie.EnteredDirectlyValues != null) ? serie.EnteredDirectlyValues.Length : 0));
		if (brushInfo != null)
		{
			ChartStyleInfoIndexer styles = sfChartSerie.Styles;
			bool flag2 = sfChartSerie.Type == ChartSeriesType.Bubble;
			BrushInfo brushInfo3 = null;
			bool flag3 = ((sfChartSerie.Type == ChartSeriesType.Pie) ? serie.GetCommonSerieFormat().IsVaryColor : IsVaryColorSupported(serie));
			int num2;
			if (!serie.InvertNegative.HasValue || !serie.InvertNegative.Value || !(serie.InvertIfNegativeColor != null))
			{
				_ = serie.InvertIfNegative;
				num2 = 0;
			}
			else
			{
				num2 = ((serie.SerieFormat.Fill.FillType == OfficeFillType.SolidColor) ? 1 : 0);
			}
			if (num2 != 0 && RectangleSerieTypes.Contains(sfChartSerie.Type))
			{
				TryParseNegativeIndexes(points, out negativeIndexes);
				if (negativeIndexes != null)
				{
					brushInfo3 = new BrushInfo(SfColor(serie.InvertIfNegativeColor.GetRGB(serie.InnerWorkbook), serie.SerieFormat.Fill.Transparency));
				}
			}
			else if (flag2 && serie.SerieFormat.CommonSerieOptions.ShowNegativeBubbles)
			{
				for (int i = 0; i < points.Count; i++)
				{
					if (!points[i].YValues[1].Equals(0.0) && points[i].YValues[1] < 0.0)
					{
						if (negativeIndexes == null)
						{
							negativeIndexes = new List<int>(points.Count);
						}
						negativeIndexes.Add(i);
					}
				}
				if (negativeIndexes != null)
				{
					brushInfo3 = new BrushInfo(SfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
			}
			if (flag2 && negativeIndexes != null)
			{
				negativeIndexes.Contains(0);
			}
			else
				_ = 0;
			for (int j = 0; j < points.Count; j++)
			{
				ChartDataPointImpl chartDataPointImpl = ((deninedDPCount > 0 && chartDataPointsCollection.m_hashDataPoints.ContainsKey(j)) ? chartDataPointsCollection.m_hashDataPoints[j] : null);
				BrushInfo brushInfo4 = brushInfo;
				if (brushInfo3 != null && negativeIndexes != null && negativeIndexes.Contains(j))
				{
					brushInfo4 = brushInfo3;
				}
				else if (chartDataPointImpl != null && (!(chartDataPointImpl.DataFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || !flag))
				{
					brushInfo4 = GetBrushFromDataFormat(chartDataPointImpl.DataFormat);
				}
				else if (flag3)
				{
					brushInfo4 = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, j, num - 1, out var color2)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(j, num, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color2)));
				}
				else if (brushInfo2 == null && brushInfo != null)
				{
					brushInfo4 = brushInfo;
				}
				else if (brushInfo2 != null)
				{
					brushInfo4 = brushInfo2;
				}
				else if (brushInfo2 == null)
				{
					brushInfo2 = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, flag3 ? j : serie.Number, flag3 ? (num - 1) : (serie.ParentSeries.Count - 1), out var color3)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(flag3 ? j : ((serie.Number != -1) ? serie.Number : 0), flag3 ? num : serie.ParentSeries.Count, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color3)));
					brushInfo4 = brushInfo2;
				}
				styles[j].Interior = brushInfo4;
				if (brushInfo3 != null && negativeIndexes != null && negativeIndexes.Contains(j) && brushInfo4 != null && brushInfo4.BackColor.ToArgb() == Color.FromArgb(255, 255, 255, 255).ToArgb())
				{
					styles[j].Border.Color = Color.Black;
					styles[j].Border.Width = 1f;
				}
				else if (chartDataPointImpl != null && chartDataPointImpl.DataFormatOrNull != null && (!(chartDataPointImpl.DataFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || chartDataPointImpl.DataFormat.HasLineProperties))
				{
					Color color4 = Color.Empty;
					float num3 = 0f;
					ChartSerieDataFormatImpl chartSerieDataFormatImpl = chartDataPointImpl.DataFormat as ChartSerieDataFormatImpl;
					if (chartSerieDataFormatImpl.LineProperties.LinePattern != OfficeChartLinePattern.None && chartDataPointImpl.DataFormat.HasLineProperties)
					{
						color4 = ((!chartDataPointImpl.DataFormat.LineProperties.IsAutoLineColor) ? GetBrushFromBorder(chartSerieDataFormatImpl.LineProperties as ChartBorderImpl).BackColor : SfColor(0, 0, 0, (parentWorkbook.Version != OfficeVersion.Excel97to2003) ? 1 : 0));
					}
					num3 = ((!(brushInfo3 != null) || negativeIndexes == null || !negativeIndexes.Contains(j)) ? GetBorderThickness(chartSerieDataFormatImpl.LineProperties as ChartBorderImpl) : 1f);
					styles[j].Border.Color = color4;
					if ((double)num3 != 0.0)
					{
						styles[j].Border.Width = num3;
					}
					else if (chartDataPointImpl.DataFormat.LineProperties.AutoFormat)
					{
						styles[j].Border.Width = 0f;
					}
				}
			}
			sfChartSerie.Style.Interior = brushInfo;
		}
		else
		{
			sfChartSerie.ParentChart.Palette = ChartColorPalette.Metro;
		}
	}

	private bool TryParseNegativeIndexes(ChartPointIndexer items, out List<int> listIndexes)
	{
		bool result = false;
		listIndexes = new List<int>();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].YValues[0] < 0.0)
			{
				listIndexes.Add(i);
				result = true;
			}
		}
		return result;
	}

	private bool GetFillOnPieDougnutSeries(ChartSerieImpl serie, ChartSeries sfChartSerie, bool isPie, out BrushInfo borderBrush, out double borderThickness)
	{
		bool result = false;
		bool flag = true;
		if (parentWorkbook.Version == OfficeVersion.Excel97to2003 && !IsChartEx)
		{
			flag = false;
		}
		borderBrush = null;
		borderThickness = 0.0;
		int num = ((serie.ValuesIRange != null) ? serie.ValuesIRange.Count : ((serie.EnteredDirectlyValues != null) ? serie.EnteredDirectlyValues.Length : 0));
		int deninedDPCount = (serie.DataPoints as ChartDataPointsCollection).DeninedDPCount;
		bool isVaryColor = serie.GetCommonSerieFormat().IsVaryColor;
		BrushInfo brushInfo = null;
		Color color;
		if (!(serie.SerieFormat as ChartSerieDataFormatImpl).IsAutomaticFormat)
		{
			brushInfo = GetBrushFromDataFormat(serie.SerieFormat);
		}
		else if (!isVaryColor)
		{
			brushInfo = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(serie.Number, serie.ParentSeries.Count, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color)));
		}
		if (deninedDPCount > 0)
		{
			ChartDataPointsCollection chartDataPointsCollection = serie.DataPoints as ChartDataPointsCollection;
			bool flag2 = true;
			for (int i = 0; i < num; i++)
			{
				BrushInfo brushInfo2 = null;
				ChartDataPointImpl chartDataPointImpl = (chartDataPointsCollection.m_hashDataPoints.ContainsKey(i) ? chartDataPointsCollection.m_hashDataPoints[i] : null);
				if (chartDataPointImpl != null)
				{
					if (!(chartDataPointImpl.DataFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || !flag)
					{
						brushInfo2 = GetBrushFromDataFormat(chartDataPointImpl.DataFormat);
						ChartBorderImpl chartBorderImpl = chartDataPointImpl.DataFormat.LineProperties as ChartBorderImpl;
						borderBrush = new BrushInfo(Color.Empty);
						if (chartBorderImpl != null && chartDataPointImpl.DataFormat.HasLineProperties && chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
						{
							if (chartDataPointImpl.DataFormat.LineProperties.IsAutoLineColor)
							{
								borderBrush = new BrushInfo(SfColor(0, 0, 0, (parentWorkbook.Version != OfficeVersion.Excel97to2003) ? 1 : 0));
							}
							else
							{
								BrushInfo brushFromBorder = GetBrushFromBorder(chartBorderImpl);
								borderThickness = GetBorderThickness(chartBorderImpl);
								borderBrush = brushFromBorder;
								flag2 = false;
							}
						}
					}
					else
					{
						brushInfo2 = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, isVaryColor ? i : serie.Number, isVaryColor ? (num - 1) : (serie.ParentSeries.Count - 1), out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(isVaryColor ? i : serie.Number, serie.ParentSeries.Count, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color)));
					}
				}
				else if (brushInfo == null)
				{
					brushInfo2 = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, isVaryColor ? i : serie.Number, isVaryColor ? (num - 1) : (serie.ParentSeries.Count - 1), out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(i, serie.ParentSeries.Count, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color)));
				}
				if (brushInfo2 != null)
				{
					sfChartSerie.Styles[i].Interior = brushInfo2;
					sfChartSerie.ParentChart.Palette = ChartColorPalette.Custom;
					if (brushInfo2.ToString() == "#00000000")
					{
						result = true;
					}
				}
				else if (brushInfo != null)
				{
					sfChartSerie.Styles[i].Interior = brushInfo;
					sfChartSerie.ParentChart.Palette = ChartColorPalette.Custom;
				}
				else if (isPie)
				{
					sfChartSerie.ParentChart.Palette = ChartColorPalette.Metro;
					break;
				}
				if (borderBrush != null)
				{
					sfChartSerie.Styles[i].Border.Color = borderBrush.BackColor;
					sfChartSerie.Styles[i].Border.Width = (float)borderThickness;
					borderBrush = null;
					borderThickness = 0.0;
				}
			}
			if (!flag2)
			{
				borderBrush = null;
				borderThickness = 0.0;
			}
		}
		else
		{
			sfChartSerie.ParentChart.Palette = ChartColorPalette.Custom;
			if (brushInfo != null)
			{
				sfChartSerie.Style.Interior = brushInfo;
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					BrushInfo interior = ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, j, num - 1, out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(j, serie.ParentSeries.Count, !flag, isColorPalette: false))) : new BrushInfo(SfColor(color)));
					sfChartSerie.Styles[j].Interior = interior;
				}
			}
		}
		return result;
	}

	private bool GetBorderOnCommonSeries(ChartBorderImpl border, ChartSeries sfChartSerie)
	{
		bool flag = true;
		bool result = false;
		if (parentWorkbook.Version == OfficeVersion.Excel97to2003)
		{
			flag = false;
		}
		float num = 0f;
		if (border.LinePattern != OfficeChartLinePattern.None)
		{
			if (border.HasLineProperties)
			{
				BrushInfo brushFromBorder = GetBrushFromBorder(border);
				if (!brushFromBorder.IsEmpty)
				{
					sfChartSerie.Style.Border.Color = brushFromBorder.BackColor;
				}
			}
			else
			{
				ChartSerieImpl chartSerieImpl = border.FindParent(typeof(ChartSerieImpl)) as ChartSerieImpl;
				if (flag && TryAndGetFillOrLineColorBasedOnPattern(chartSerieImpl.ParentChart, isLine: true, chartSerieImpl.Number, chartSerieImpl.ParentSeries.Count - 1, out var color))
				{
					sfChartSerie.Style.Border.Color = SfColor(color);
				}
				else
				{
					sfChartSerie.Style.Border.Color = SfColor(0, 0, 0, flag ? 1 : 0);
				}
				result = true;
			}
		}
		else
		{
			result = true;
			if (sfChartSerie.ParentChart.Series3D && sfChartSerie.Style.Interior != null)
			{
				sfChartSerie.Style.Border.Color = sfChartSerie.Style.Interior.BackColor;
			}
		}
		num = GetBorderThickness(border);
		if (num != 0f)
		{
			num = ((num < 1f) ? 1f : (num + 1f));
		}
		sfChartSerie.Style.Border.Width = num;
		return result;
	}

	internal void GetBorderOnLineSeries(ChartSerieImpl serie, ChartSeries sfChartSerie)
	{
		BrushInfo brushInfo = null;
		bool flag = true;
		if (parentWorkbook.Version == OfficeVersion.Excel97to2003 && !IsChartEx)
		{
			flag = false;
		}
		ChartBorderImpl chartBorderImpl = serie.SerieFormat.LineProperties as ChartBorderImpl;
		Color color;
		if (serie.SerieType.ToString().Contains("3D"))
		{
			brushInfo = ((!(serie.SerieFormat as ChartSerieDataFormatImpl).IsAutomaticFormat || !flag) ? GetBrushFromDataFormat(serie.SerieFormat) : ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor((serie.Number != -1) ? serie.Number : 0, serie.ParentSeries.Count, isBinary: false, isColorPalette: false))) : new BrushInfo(SfColor(color))));
			if (brushInfo != null)
			{
				sfChartSerie.Style.Border.Color = brushInfo.BackColor;
				sfChartSerie.Style.Interior = brushInfo;
			}
			sfChartSerie.Style.Border.Width = (flag ? 2f : 1f);
		}
		else if (chartBorderImpl.LinePattern != OfficeChartLinePattern.None)
		{
			brushInfo = ((!chartBorderImpl.IsAutoLineColor) ? GetBrushFromBorder(chartBorderImpl) : ((!TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: false, serie.Number, serie.ParentSeries.Count - 1, out color)) ? new BrushInfo(SfColor(serie.ParentChart.GetChartColor(serie.Number, serie.ParentSeries.Count, !flag, isColorPalette: true))) : new BrushInfo(SfColor(color))));
			TryAndUpdateSegmentsColorsInLineSeries(serie, sfChartSerie, brushInfo, flag);
			if (brushInfo != null)
			{
				sfChartSerie.Style.Border.Color = brushInfo.BackColor;
				sfChartSerie.Style.Interior = brushInfo;
			}
			sfChartSerie.Style.Border.Width = GetBorderThickness(chartBorderImpl);
			float thickness = -1f;
			if (sfChartSerie.Style.Border.Width == 0f || chartBorderImpl.LineWeightString == null)
			{
				if (TryAndGetThicknessBasedOnElement(ChartElementsEnum.DataPointLineThickness, serie.ParentChart, out thickness, null))
				{
					sfChartSerie.Style.Border.Width = thickness;
				}
				else
				{
					sfChartSerie.Style.Border.Width = (flag ? 2f : 1f);
				}
			}
			else if (sfChartSerie.Style.Border.Width < 1f)
			{
				sfChartSerie.Style.Border.Width = 1f;
			}
			if (!parentWorkbook.IsCreated && !parentWorkbook.IsConverted && flag && chartBorderImpl.LineWeightString == null && thickness == -1f)
			{
				sfChartSerie.Style.Border.Width = 2f;
			}
		}
		else
		{
			sfChartSerie.Style.Border.Width = 0f;
			sfChartSerie.Style.Interior = new BrushInfo(SfColor(Color.Transparent, 1.0));
			sfChartSerie.Style.Border.Color = SfColor(Color.Transparent, 1.0);
		}
	}

	private bool TryAndUpdateSegmentsColorsInLineSeries(ChartSerieImpl serie, ChartSeries sfChartSerie, BrushInfo brush, bool isXMLVersion)
	{
		bool result = false;
		bool flag = !IsChartEx && IsVaryColorSupported(serie);
		ChartDataPointsCollection chartDataPointsCollection = serie.DataPoints as ChartDataPointsCollection;
		ChartPointIndexer points = sfChartSerie.Points;
		int count = points.Count;
		ChartStyleInfoIndexer styles = sfChartSerie.Styles;
		if (flag || chartDataPointsCollection.DeninedDPCount > 0)
		{
			for (int i = 0; i < points.Count; i++)
			{
				ChartDataPointImpl chartDataPointImpl = (chartDataPointsCollection.m_hashDataPoints.ContainsKey(i) ? chartDataPointsCollection.m_hashDataPoints[i] : null);
				string text = serie.SerieType.ToString();
				if (chartDataPointImpl != null && chartDataPointImpl.DataFormatOrNull != null && chartDataPointImpl.DataFormat.HasLineProperties && !chartDataPointImpl.DataFormat.LineProperties.IsAutoLineColor && ((!text.Contains("Line") && !text.Contains("Scatter")) || chartDataPointImpl.Index != 0) && (!text.ToString().Contains("Line") || (serie.ParentChart.IsChartParsed ? chartDataPointImpl.HasDataPoint : (!chartDataPointImpl.IsDefault))))
				{
					Color color = Color.Empty;
					float num = 0f;
					ChartSerieDataFormatImpl chartSerieDataFormatImpl = chartDataPointImpl.DataFormat as ChartSerieDataFormatImpl;
					ChartBorderImpl chartBorderImpl = chartSerieDataFormatImpl.LineProperties as ChartBorderImpl;
					if (chartSerieDataFormatImpl.LineProperties.LinePattern != OfficeChartLinePattern.None && chartDataPointImpl.DataFormat.HasLineProperties)
					{
						if (chartDataPointImpl.DataFormat.LineProperties.IsAutoLineColor)
						{
							color = SfColor(0, 0, 0, (parentWorkbook.Version != OfficeVersion.Excel97to2003) ? 1 : 0);
						}
						else
						{
							color = GetBrushFromBorder(chartSerieDataFormatImpl.LineProperties as ChartBorderImpl).BackColor;
							styles[i].IsScatterBorderColor = true;
							sfChartSerie.Style.IsScatterBorderColor = true;
						}
					}
					num = GetBorderThickness(chartBorderImpl);
					float thickness = -1f;
					if (num == 0f || (num == 0.75f && (short)chartBorderImpl.LineWeight == -1 && isXMLVersion))
					{
						num = ((!TryAndGetThicknessBasedOnElement(ChartElementsEnum.DataPointLineThickness, serie.ParentChart, out thickness, null)) ? (isXMLVersion ? 2f : 1f) : thickness);
					}
					else if (num < 1f)
					{
						num = 1f;
					}
					if (!parentWorkbook.IsCreated && !parentWorkbook.IsConverted && isXMLVersion && chartBorderImpl.LineWeightString == null && thickness == -1f)
					{
						num = 2f;
					}
					styles[i].Border.Color = color;
					if ((double)num != 0.0)
					{
						styles[i].Border.Width = num;
					}
					else if (chartDataPointImpl.DataFormat.LineProperties.AutoFormat)
					{
						styles[i].Border.Width = 0f;
					}
					DashStyle dashStyle = DashStyle.Solid;
					if (GetStrokeDashArrayValues(chartBorderImpl.LinePattern, out dashStyle))
					{
						styles[i].Border.DashStyle = dashStyle;
					}
					DashCap cap = DashCap.Flat;
					GetDashCapStyle((DocGen.Drawing.LineCap)chartBorderImpl.CapStyle, out cap);
					styles[i].Border.DashCap = cap;
				}
				else if (flag)
				{
					if (TryAndGetFillOrLineColorBasedOnPattern(serie.ParentChart, isLine: true, i, points.Count - 1, out var color2))
					{
						new BrushInfo(SfColor(color2));
					}
					else
					{
						new BrushInfo(SfColor(serie.ParentChart.GetChartColor(i, count, !isXMLVersion, isColorPalette: true)));
					}
				}
				else if (chartDataPointImpl != null && chartDataPointImpl.DataFormatOrNull != null && chartDataPointImpl.DataFormat.HasLineProperties && chartDataPointImpl.DataFormat.LineProperties.LinePattern == OfficeChartLinePattern.None)
				{
					styles[i].Border.Color = Color.Empty;
				}
			}
		}
		return result;
	}

	private void ChartSeriesCommon(ChartSerieImpl serie, ChartSeries chartSeriesBase, IList<ChartPointInternal> values, bool applyFill, out List<int> negativeIndexes)
	{
		if (!IsChartEx)
		{
			TryAnsEmptyPointDisplayInChart(serie, chartSeriesBase);
		}
		values = GetChartPointsValues(serie, chartSeriesBase.ParentChart.AllowGapForEmptyPoints, (chartSeriesBase.EmptyPointValue == EmptyPointValue.Average) ? 1 : 0, isBubbles: false, values);
		values = TryAndRemoveFirstEmptyPointsOnSeries(serie.SerieType.ToString().Contains("Line"), serie, chartSeriesBase, values);
		chartSeriesBase.SetItemSource(values);
		base.ItemSource = values;
		SfSecondaryAxis(serie, serie.ParentChart, chartSeriesBase);
		negativeIndexes = null;
		if (applyFill)
		{
			GetFillOnCommonSeries(serie, chartSeriesBase, out negativeIndexes);
		}
		chartSeriesBase.Name = GetSerieName(serie);
	}

	private IList<ChartPointInternal> TryAndRemoveFirstEmptyPointsOnSeries(bool isLine, ChartSerieImpl serie, ChartSeries chartSeriesBase, IList<ChartPointInternal> values)
	{
		int displayBlanksAs = (int)serie.ParentChart.DisplayBlanksAs;
		if (displayBlanksAs == 0 || displayBlanksAs == 2)
		{
			IEnumerable<ChartPointInternal> source = values.SkipWhile((ChartPointInternal x) => double.IsNaN(x.Value) && x.X is DateTime);
			if (isLine && source.Count() <= 1)
			{
				chartSeriesBase.ParentChart.AllowGapForEmptyPoints = true;
				return values;
			}
			return source.ToList();
		}
		return values;
	}

	private void TryAnsEmptyPointDisplayInChart(ChartSerieImpl serie, ChartSeries chartSeriesBase)
	{
		switch ((int)serie.ParentChart.DisplayBlanksAs)
		{
		case 0:
			chartSeriesBase.ParentChart.AllowGapForEmptyPoints = true;
			break;
		case 1:
			chartSeriesBase.ParentChart.AllowGapForEmptyPoints = false;
			chartSeriesBase.EmptyPointValue = EmptyPointValue.Zero;
			break;
		case 2:
		{
			string text = serie.SerieType.ToString();
			if (text.ToLower().Contains("line") || text.Contains("Radar"))
			{
				chartSeriesBase.ParentChart.AllowGapForEmptyPoints = false;
				chartSeriesBase.EmptyPointValue = EmptyPointValue.Average;
			}
			break;
		}
		}
	}

	private void ChartSeriesCommon(ChartSerieImpl serie, ChartSeries chartSeriesBase, IList<ChartPointInternal> values, out List<int> negativeIndexes)
	{
		ChartSeriesCommon(serie, chartSeriesBase, values, applyFill: true, out negativeIndexes);
	}

	private void ChartSeriesCommon(ChartSerieImpl serie, ChartSeries chartSeriesBase)
	{
		ChartSeriesCommon(serie, chartSeriesBase, applyFill: true);
	}

	private void ChartSeriesCommon(ChartSerieImpl serie, ChartSeries chartSeriesBase, bool applyFill)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeriesCommon(serie, chartSeriesBase, viewModel.Products, applyFill, out var _);
	}

	private void ChartSeriesCommon(ChartSerieImpl serie, ChartSeries chartSeriesBase, out List<int> negativeIndexes)
	{
		ViewModel viewModel = GetViewModel(serie);
		ChartSeriesCommon(serie, chartSeriesBase, viewModel.Products, applyFill: true, out negativeIndexes);
	}

	internal void SetArrows(ChartSerieImpl serie, ChartSeries line, int dataPointsCount)
	{
		List<Arrow> list = new List<Arrow>();
		List<Arrow> list2 = new List<Arrow>();
		ChartBorderImpl obj = serie.SerieFormat.LineProperties as ChartBorderImpl;
		if (obj.BeginArrowType != OfficeArrowType.None)
		{
			line.BeginArrow = GetArrow(serie.SerieFormat.LineProperties as ChartBorderImpl, isBeginArrow: true);
		}
		if (obj.EndArrowType != OfficeArrowType.None)
		{
			line.EndArrow = GetArrow(serie.SerieFormat.LineProperties as ChartBorderImpl, isBeginArrow: false);
		}
		for (int i = 0; i < dataPointsCount; i++)
		{
			Arrow arrow = null;
			Arrow arrow2 = null;
			if ((serie.DataPoints as ChartDataPointsCollection).m_hashDataPoints.TryGetValue(i, out var value))
			{
				arrow = (value.HasDataPoint ? GetArrow(value.DataFormat.LineProperties as ChartBorderImpl, isBeginArrow: true) : null);
				arrow2 = (value.HasDataPoint ? GetArrow(value.DataFormat.LineProperties as ChartBorderImpl, isBeginArrow: false) : null);
			}
			if (arrow == null && i == 1)
			{
				arrow = line.BeginArrow;
			}
			if (arrow2 == null && i == dataPointsCount - 1)
			{
				arrow2 = line.EndArrow;
			}
			if (i > 0)
			{
				Arrow arrow3 = null;
				if ((serie.DataPoints as ChartDataPointsCollection).m_hashDataPoints.TryGetValue(i - 1, out value))
				{
					arrow3 = (value.HasDataPoint ? GetArrow(value.DataFormat.LineProperties as ChartBorderImpl, isBeginArrow: false) : null);
				}
				if (arrow3 != null && arrow == null)
				{
					arrow = line.BeginArrow;
				}
			}
			if (i < dataPointsCount - 1)
			{
				Arrow arrow4 = null;
				if ((serie.DataPoints as ChartDataPointsCollection).m_hashDataPoints.TryGetValue(i + 1, out value))
				{
					arrow4 = (value.HasDataPoint ? GetArrow(value.DataFormat.LineProperties as ChartBorderImpl, isBeginArrow: true) : null);
				}
				if (arrow4 != null && arrow2 == null)
				{
					arrow2 = line.EndArrow;
				}
			}
			list.Add(arrow);
			list2.Add(arrow2);
		}
		line.BeginArrows = list;
		line.EndArrows = list2;
	}

	internal Arrow GetArrow(ChartBorderImpl line, bool isBeginArrow)
	{
		Arrow arrow = new Arrow();
		if (isBeginArrow)
		{
			arrow.Type = line.BeginArrowType;
			GetArrowLengthAndWidth(line.BeginArrowSize, arrow);
		}
		else
		{
			arrow.Type = line.EndArrowType;
			GetArrowLengthAndWidth(line.EndArrowSize, arrow);
		}
		return arrow;
	}

	internal void SetErrorBarConfiguration(ChartSerieImpl serie, ChartSeries chartSeriesBase)
	{
		if (serie.HasErrorBarsX)
		{
			SetErrorBarProperties(serie, chartSeriesBase, isErrorBarX: true);
		}
		if (serie.HasErrorBarsY)
		{
			SetErrorBarProperties(serie, chartSeriesBase, isErrorBarX: false);
		}
	}

	internal void SetErrorBarProperties(ChartSerieImpl serie, ChartSeries chartSeriesBase, bool isErrorBarX)
	{
		chartSeriesBase.ConfigItems.ErrorBars.Enabled = true;
		IOfficeChartErrorBars officeChartErrorBars = null;
		Color empty = Color.Empty;
		DashStyle dashStyle = DashStyle.Solid;
		officeChartErrorBars = ((!isErrorBarX) ? serie.ErrorBarsY : serie.ErrorBarsX);
		if (serie.ParentChart.ChartType.ToString().Contains("Bar") || isErrorBarX)
		{
			chartSeriesBase.ConfigItems.ErrorBars.Orientation = ChartOrientation.Horizontal;
		}
		else
		{
			chartSeriesBase.ConfigItems.ErrorBars.Orientation = ChartOrientation.Vertical;
		}
		if (officeChartErrorBars.HasCap)
		{
			if (chartSeriesBase.ConfigItems.ErrorBars.Orientation == ChartOrientation.Horizontal)
			{
				chartSeriesBase.ConfigItems.ErrorBars.SymbolShape = ChartSymbolShape.VertLine;
			}
			else
			{
				chartSeriesBase.ConfigItems.ErrorBars.SymbolShape = ChartSymbolShape.HorizLine;
			}
		}
		else
		{
			chartSeriesBase.ConfigItems.ErrorBars.SymbolShape = ChartSymbolShape.None;
		}
		chartSeriesBase.ConfigItems.ErrorBars.ValueType = officeChartErrorBars.Type;
		chartSeriesBase.ConfigItems.ErrorBars.Type = officeChartErrorBars.Include;
		chartSeriesBase.ConfigItems.ErrorBars.FixedValue = officeChartErrorBars.NumberValue;
		empty = ((!officeChartErrorBars.Border.IsAutoLineColor) ? GetBrushFromBorder(officeChartErrorBars.Border as ChartBorderImpl).BackColor : SfColor(0, 0, 0, (parentWorkbook.Version != OfficeVersion.Excel97to2003) ? 1 : 0));
		chartSeriesBase.ConfigItems.ErrorBars.Color = empty;
		chartSeriesBase.ConfigItems.ErrorBars.Width = GetBorderThickness(officeChartErrorBars.Border as ChartBorderImpl);
		GetStrokeDashArrayValues(officeChartErrorBars.Border.LinePattern, out dashStyle);
		chartSeriesBase.ConfigItems.ErrorBars.DashStyle = dashStyle;
		chartSeriesBase.ConfigItems.ErrorBars.PlusValues = new List<double>();
		chartSeriesBase.ConfigItems.ErrorBars.MinusValues = new List<double>();
		if (officeChartErrorBars.Type != OfficeErrorBarType.Custom)
		{
			return;
		}
		object[] plusRangeValues;
		if ((officeChartErrorBars as ChartErrorBarsImpl).PlusRangeValues != null)
		{
			plusRangeValues = (officeChartErrorBars as ChartErrorBarsImpl).PlusRangeValues;
			foreach (object obj in plusRangeValues)
			{
				if (obj == null)
				{
					chartSeriesBase.ConfigItems.ErrorBars.PlusValues.Add(double.NaN);
				}
				else
				{
					chartSeriesBase.ConfigItems.ErrorBars.PlusValues.Add(double.Parse(obj.ToString()));
				}
			}
		}
		if ((officeChartErrorBars as ChartErrorBarsImpl).MinusRangeValues == null)
		{
			return;
		}
		plusRangeValues = (officeChartErrorBars as ChartErrorBarsImpl).MinusRangeValues;
		foreach (object obj2 in plusRangeValues)
		{
			if (obj2 == null)
			{
				chartSeriesBase.ConfigItems.ErrorBars.MinusValues.Add(double.NaN);
			}
			else
			{
				chartSeriesBase.ConfigItems.ErrorBars.MinusValues.Add(double.Parse(obj2.ToString()));
			}
		}
	}

	internal void GetArrowLengthAndWidth(OfficeArrowSize arrowsize, Arrow arrow)
	{
		switch (arrowsize)
		{
		case OfficeArrowSize.ArrowLSize1:
			arrow.ArrowLength = 5f;
			arrow.ArrowWidth = 5f;
			break;
		case OfficeArrowSize.ArrowLSize2:
			arrow.ArrowLength = 5f;
			arrow.ArrowWidth = 7f;
			break;
		case OfficeArrowSize.ArrowLSize3:
			arrow.ArrowLength = 5f;
			arrow.ArrowWidth = 10f;
			break;
		case OfficeArrowSize.ArrowLSize4:
			arrow.ArrowLength = 7f;
			arrow.ArrowWidth = 5f;
			break;
		case OfficeArrowSize.ArrowLSize5:
			arrow.ArrowLength = 7f;
			arrow.ArrowWidth = 7f;
			break;
		case OfficeArrowSize.ArrowLSize6:
			arrow.ArrowLength = 7f;
			arrow.ArrowWidth = 10f;
			break;
		case OfficeArrowSize.ArrowLSize7:
			arrow.ArrowLength = 10f;
			arrow.ArrowWidth = 5f;
			break;
		case OfficeArrowSize.ArrowLSize8:
			arrow.ArrowLength = 10f;
			arrow.ArrowWidth = 7f;
			break;
		case OfficeArrowSize.ArrowLSize9:
			arrow.ArrowLength = 10f;
			arrow.ArrowWidth = 10f;
			break;
		}
	}

	internal void DetachEventsForRadarAxes(ChartControl sfChart)
	{
	}

	internal void TryAndSortLegendItems(ChartControl sfChart, int[] sortedLegendOrders)
	{
	}
}
