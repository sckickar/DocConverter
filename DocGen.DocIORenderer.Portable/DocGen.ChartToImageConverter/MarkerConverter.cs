using System.Collections.Generic;
using DocGen.Chart;
using DocGen.Drawing;

namespace DocGen.ChartToImageConverter;

internal class MarkerConverter
{
	internal Dictionary<int, MarkerSetting> MarkerSettings { get; set; }

	internal MarkerSetting CommonMarkerSetting { get; set; }

	internal List<int> AverageMarkerIndexes { get; set; }

	public void SetSymbolStyle(ChartSeries series, ChartStyleInfo style, int index, object parameter, ChartPoint point)
	{
		if (AverageMarkerIndexes != null && AverageMarkerIndexes.Contains(index))
		{
			style.Symbol.Shape = ChartSymbolShape.None;
		}
		else if (MarkerSettings.ContainsKey(index))
		{
			style.Symbol.Shape = ChartUtilities.GetMarkerSymbolShape(MarkerSettings[index].MarkerTypeInInt);
		}
		else
		{
			style.Symbol.Shape = ChartUtilities.GetMarkerSymbolShape(CommonMarkerSetting.MarkerTypeInInt);
		}
		if (AverageMarkerIndexes != null && AverageMarkerIndexes.Contains(index))
		{
			style.Symbol.Color = Color.FromArgb(0, 0, 0, 0);
		}
		else if (MarkerSettings.ContainsKey(index))
		{
			style.Symbol.Color = MarkerSettings[index].FillBrush;
		}
		else
		{
			style.Symbol.Color = CommonMarkerSetting.FillBrush;
		}
		if (AverageMarkerIndexes != null && AverageMarkerIndexes.Contains(index))
		{
			style.Symbol.Size = Size.Empty;
		}
		else if (MarkerSettings.ContainsKey(index))
		{
			style.Symbol.Size = new Size(MarkerSettings[index].MarkerSize, MarkerSettings[index].MarkerSize);
		}
		else
		{
			style.Symbol.Size = new Size(CommonMarkerSetting.MarkerSize, CommonMarkerSetting.MarkerSize);
		}
		if (AverageMarkerIndexes != null && AverageMarkerIndexes.Contains(index))
		{
			style.Symbol.Border.Color = Color.FromArgb(0, 0, 0, 0);
		}
		else if (MarkerSettings.ContainsKey(index))
		{
			style.Symbol.Border.Color = MarkerSettings[index].BorderBrush;
		}
		else
		{
			style.Symbol.Border.Color = CommonMarkerSetting.BorderBrush;
		}
		if (AverageMarkerIndexes != null && AverageMarkerIndexes.Contains(index))
		{
			style.Symbol.Border.Width = 0f;
		}
		else if (MarkerSettings.ContainsKey(index))
		{
			style.Symbol.Border.Width = MarkerSettings[index].BorderThickness * 1.25f;
		}
		else
		{
			style.Symbol.Border.Width = CommonMarkerSetting.BorderThickness * 1.25f;
		}
	}
}
