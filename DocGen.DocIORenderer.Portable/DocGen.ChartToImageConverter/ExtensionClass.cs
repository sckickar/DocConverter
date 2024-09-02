using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.Chart;

namespace DocGen.ChartToImageConverter;

internal static class ExtensionClass
{
	internal static void SetItemSource(this ChartSeries series, IList<ChartPointInternal> inPoints)
	{
		ChartPointIndexer chartPointIndexer = new ChartPointIndexer(series);
		bool flag = false;
		bool flag2 = false;
		if (inPoints.Count > 0)
		{
			flag = inPoints[0].X is double;
		}
		if (inPoints.Count > 0)
		{
			flag2 = inPoints[0].X is DateTime;
		}
		for (int i = 0; i < inPoints.Count; i++)
		{
			int num = 0;
			ChartPointInternal chartPointInternal = inPoints[i];
			if (series.Type == ChartSeriesType.Bubble)
			{
				num = chartPointIndexer.Add(Convert.ToDouble(chartPointInternal.X), chartPointInternal.Value, Convert.ToDouble(chartPointInternal.Size));
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Close != null) ? chartPointInternal.Close.ToString() : "");
			}
			else if (series.Type == ChartSeriesType.Candle)
			{
				num = (flag ? chartPointIndexer.Add(Convert.ToDouble(chartPointInternal.X), chartPointInternal.High, chartPointInternal.Low, chartPointInternal.Open, Convert.ToDouble(chartPointInternal.Close)) : ((!flag2) ? chartPointIndexer.Add(chartPointInternal.X.ToString(), chartPointInternal.High, chartPointInternal.Low, chartPointInternal.Open, Convert.ToDouble(chartPointInternal.Close)) : chartPointIndexer.Add(Convert.ToDateTime(chartPointInternal.X), chartPointInternal.High, chartPointInternal.Low, chartPointInternal.Open, Convert.ToDouble(chartPointInternal.Close))));
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Size != null) ? chartPointInternal.Size.ToString() : "");
			}
			else if (series.Type == ChartSeriesType.HiLo || series.Type == ChartSeriesType.ColumnRange)
			{
				num = (flag ? chartPointIndexer.Add(Convert.ToDouble(chartPointInternal.X), chartPointInternal.High, chartPointInternal.Low, Convert.ToDouble(chartPointInternal.Close)) : ((!flag2) ? chartPointIndexer.Add(chartPointInternal.X.ToString(), chartPointInternal.High, chartPointInternal.Low, Convert.ToDouble(chartPointInternal.Close)) : chartPointIndexer.Add(Convert.ToDateTime(chartPointInternal.X), chartPointInternal.High, chartPointInternal.Low, Convert.ToDouble(chartPointInternal.Close))));
				series.TempDataLabelsResult.Add(num, (series.Type == ChartSeriesType.ColumnRange) ? chartPointInternal.Value.ToString(CultureInfo.InvariantCulture) : ((chartPointInternal.Size != null) ? chartPointInternal.Size.ToString() : ""));
			}
			else if (series.Type == ChartSeriesType.Radar)
			{
				num = chartPointIndexer.Add(i + 1, chartPointInternal.Value);
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Close != null) ? chartPointInternal.Close.ToString() : "");
			}
			else if (series.Type == ChartSeriesType.Scatter || flag)
			{
				num = chartPointIndexer.Add(Convert.ToDouble(chartPointInternal.X), chartPointInternal.Value);
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Close != null) ? chartPointInternal.Close.ToString() : "");
			}
			else if (flag2)
			{
				num = chartPointIndexer.Add(Convert.ToDateTime(chartPointInternal.X), chartPointInternal.Value);
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Close != null) ? chartPointInternal.Close.ToString() : "");
			}
			else
			{
				num = chartPointIndexer.Add(chartPointInternal.X.ToString(), chartPointInternal.Value);
				series.TempDataLabelsResult.Add(num, (chartPointInternal.Close != null) ? chartPointInternal.Close.ToString() : "");
			}
			(series.SeriesModel as ChartSeriesModel).SetIsEmpty(num, chartPointInternal.IsSummary);
		}
	}
}
