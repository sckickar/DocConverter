using DocGen.Chart;
using DocGen.OfficeChart.Implementation.Charts;

namespace DocGen.ChartToImageConverter;

internal struct DataLabelSetting
{
	internal bool IsValueFromCells;

	internal bool IsValue;

	internal bool IsCategoryName;

	internal bool IsSeriesName;

	internal bool IsPercentage;

	internal bool IsDelete;

	internal bool IsSourceLinked;

	internal string Seperator;

	internal string NumberFormat;

	internal string CustomText;

	internal ChartTextOrientation TextOrientation;

	internal DataLabelSetting(ChartDataLabelsImpl dataLabelsImpl, bool isCircularSeries, ChartTextOrientation textOrientation)
	{
		IsValueFromCells = dataLabelsImpl.IsValueFromCells;
		IsValue = dataLabelsImpl.IsValue;
		IsCategoryName = dataLabelsImpl.IsCategoryName;
		IsSeriesName = dataLabelsImpl.IsSeriesName;
		IsPercentage = isCircularSeries && dataLabelsImpl.IsPercentage;
		IsDelete = dataLabelsImpl.IsDelete;
		Seperator = dataLabelsImpl.Delimiter;
		NumberFormat = dataLabelsImpl.NumberFormat;
		IsSourceLinked = dataLabelsImpl.IsSourceLinked;
		CustomText = dataLabelsImpl.Text;
		TextOrientation = textOrientation;
		if (NumberFormat == null)
		{
			NumberFormat = "General";
		}
		if (Seperator == null)
		{
			Seperator = ", ";
		}
		if (Seperator == "\n")
		{
			Seperator = " ";
		}
	}
}
