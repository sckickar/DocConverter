using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.Chart;
using DocGen.Drawing;

namespace DocGen.ChartToImageConverter;

internal class LabelConvertor
{
	internal Dictionary<int, object> ValueFromCells { get; set; }

	internal string SeriesName { get; set; }

	internal object[] CategoryNames { get; set; }

	internal double Percentage { get; set; }

	internal List<int> BlankIndexes { get; set; }

	internal Dictionary<int, DataLabelSetting> DataLabelSettings { get; set; }

	internal DataLabelSetting CommonDataLabelSetting { get; set; }

	internal IDictionary<int, Font> Fonts { get; set; }

	internal IDictionary<int, Color> FillColors { get; set; }

	internal IDictionary<int, Color> FontColors { get; set; }

	internal IDictionary<int, Color> BorderColors { get; set; }

	internal IDictionary<int, float> BorderWidths { get; set; }

	internal IDictionary<char, Color> ColorOnNumFmts { get; set; }

	internal Color BorderColor { get; set; }

	internal Color Color { get; set; }

	internal float BorderWidth { get; set; }

	internal bool IsFunnelLabel { get; set; }

	internal event Func<object, string, string> NumberFormatApplyEvent;

	internal LabelConvertor()
	{
		DataLabelSettings = new Dictionary<int, DataLabelSetting>();
		CommonDataLabelSetting = default(DataLabelSetting);
	}

	public void SetStyle(ChartSeries series, ChartStyleInfo style, int index, object parameter, ChartPoint point)
	{
		string text = "";
		DataLabelSetting? dataLabelSetting = null;
		string text2 = null;
		if (series.XAxis.ValueType == ChartValueType.DateTime)
		{
			text2 = ((series.XAxis.VisibleLabels.Length <= index) ? point.X.ToString(CultureInfo.InvariantCulture) : series.XAxis.VisibleLabels[index].CustomText);
		}
		if (BlankIndexes != null && BlankIndexes.Contains(index))
		{
			style.Text = "";
		}
		else if (DataLabelSettings.ContainsKey(index))
		{
			dataLabelSetting = DataLabelSettings[index];
			if (!dataLabelSetting.Value.IsDelete)
			{
				object value = point.YValues[0];
				text = ((dataLabelSetting.Value.CustomText != null) ? dataLabelSetting.Value.CustomText : LabelSettings(value, dataLabelSetting.Value, index, text2));
				text = UpdateDataLabelText(text, value, dataLabelSetting.Value, index, text2);
			}
			else
			{
				style.DisplayText = false;
			}
			if (style.TextOrientation != dataLabelSetting.Value.TextOrientation)
			{
				style.TextOrientation = dataLabelSetting.Value.TextOrientation;
			}
		}
		else
		{
			object value2 = (IsFunnelLabel ? point.Category : ((object)point.YValues[0]));
			text = LabelSettings(value2, CommonDataLabelSetting, index, text2);
			text = UpdateDataLabelText(text, value2, CommonDataLabelSetting, index, text2);
		}
		if (IsFunnelLabel && string.IsNullOrEmpty(text))
		{
			style.DisplayText = false;
			return;
		}
		style.Text = text;
		if (dataLabelSetting.HasValue && dataLabelSetting.Value.IsDelete)
		{
			style.DrawTextShape = false;
			return;
		}
		if (Fonts != null && Fonts.ContainsKey(index))
		{
			Font font = Fonts[index];
			style.Font.FontStyle = font.Style;
			style.Font.Facename = font.Name;
			style.Font.Size = font.Size;
			style.Font.Underline = font.Underline;
		}
		double num = 0.0;
		if (point != null)
		{
			num = point.YValues[0];
		}
		if (ColorOnNumFmts != null)
		{
			if (num < 0.0 && ColorOnNumFmts.ContainsKey('-'))
			{
				style.TextColor = ColorOnNumFmts['-'];
			}
			else if (num == 0.0 && ColorOnNumFmts.ContainsKey('0'))
			{
				style.TextColor = ColorOnNumFmts['0'];
			}
			else if (num > 0.0 && ColorOnNumFmts.ContainsKey('+'))
			{
				style.TextColor = ColorOnNumFmts['+'];
			}
		}
		else if (FontColors != null && FontColors.ContainsKey(index))
		{
			style.TextColor = FontColors[index];
		}
		else
		{
			style.TextColor = series.Style.TextColor;
		}
		if (FillColors != null && FillColors.ContainsKey(index))
		{
			style.TextShape.Color = FillColors[index];
		}
		else if (series.Style.DrawTextShape)
		{
			style.TextShape.Color = Color;
		}
		if (BorderColors != null && BorderColors.ContainsKey(index))
		{
			style.TextShape.BorderColor = BorderColors[index];
		}
		else if (series.Style.DrawTextShape)
		{
			style.TextShape.BorderColor = BorderColor;
		}
		if (BorderWidths != null && BorderWidths.ContainsKey(index))
		{
			style.TextShape.BorderWidth = BorderWidths[index];
		}
		else if (series.Style.DrawTextShape)
		{
			style.TextShape.BorderWidth = BorderWidth;
		}
	}

	internal string UpdateDataLabelText(string text, object value, DataLabelSetting setting, int index, string dateTimeText)
	{
		string text2 = null;
		if (setting.IsValueFromCells && text.Contains("[CELLRANGE]"))
		{
			text2 = UpdateDataLabelValuesFromCells(null, setting, index);
			text = (string.IsNullOrEmpty(text2) ? null : text.Replace("[CELLRANGE]", text2));
		}
		if (setting.IsSeriesName && text.Contains("[SERIES NAME]"))
		{
			text2 = UpdateDataLabelSeriesName(null, setting);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[SERIES NAME]", text2);
			}
		}
		if (setting.IsCategoryName && text.Contains("[CATEGORY NAME]"))
		{
			text2 = UpdateDataLabelCategoryName(null, setting, index, dateTimeText, value);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[CATEGORY NAME]", text2);
			}
		}
		if (setting.IsCategoryName && text.Contains("[X VALUE]"))
		{
			text2 = UpdateDataLabelCategoryName(null, setting, index, dateTimeText, value);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[X VALUE]", text2);
			}
		}
		if (setting.IsValue && text.Contains("[VALUE]"))
		{
			text2 = UpdateDataLabelValue(null, value, setting);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[VALUE]", text2);
			}
		}
		if (setting.IsValue && text.Contains("[Y VALUE]"))
		{
			text2 = UpdateDataLabelValue(null, value, setting);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[Y VALUE]", text2);
			}
		}
		if (setting.IsPercentage && text.Contains("[PERCENTAGE]"))
		{
			text2 = UpdateDataLabelPercentange(null, value, setting);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text.Replace("[PERCENTAGE]", text2);
			}
		}
		return text;
	}

	private string LabelSettings(object value, DataLabelSetting dataLabelSetting, int index, string catDateTimText)
	{
		string text = null;
		if (dataLabelSetting.IsValueFromCells && ValueFromCells != null && ValueFromCells.Count > 0)
		{
			text = UpdateDataLabelValuesFromCells(text, dataLabelSetting, index);
		}
		if (dataLabelSetting.IsSeriesName)
		{
			text = UpdateDataLabelSeriesName(text, dataLabelSetting);
		}
		if (dataLabelSetting.IsCategoryName)
		{
			text = UpdateDataLabelCategoryName(text, dataLabelSetting, index, catDateTimText, value);
		}
		if (IsFunnelLabel)
		{
			value = CategoryNames[index];
			if ((string)value == "0")
			{
				return "";
			}
		}
		if (dataLabelSetting.IsValue)
		{
			text = UpdateDataLabelValue(text, value, dataLabelSetting);
		}
		if (dataLabelSetting.IsPercentage)
		{
			text = UpdateDataLabelPercentange(text, value, dataLabelSetting);
		}
		return text;
	}

	internal string UpdateDataLabelValuesFromCells(string text, DataLabelSetting dataLabelSetting, int index)
	{
		object value = null;
		text = null;
		if (ValueFromCells != null && ValueFromCells.Count > 0 && ValueFromCells.TryGetValue(index, out value))
		{
			text = value.ToString();
		}
		return text;
	}

	internal string UpdateDataLabelSeriesName(string text, DataLabelSetting dataLabelSetting)
	{
		text += ((text != null) ? (dataLabelSetting.Seperator + SeriesName) : SeriesName);
		return text;
	}

	internal string UpdateDataLabelCategoryName(string text, DataLabelSetting dataLabelSetting, int index, string catDateTimText, object value)
	{
		object obj = (IsFunnelLabel ? value : CategoryNames[index]);
		if (catDateTimText != null)
		{
			obj = catDateTimText;
		}
		else if (!(CategoryNames[index] is string) && dataLabelSetting.NumberFormat != null && dataLabelSetting.NumberFormat.ToLower() != "general" && !dataLabelSetting.IsPercentage)
		{
			obj = this.NumberFormatApplyEvent(obj, dataLabelSetting.NumberFormat);
		}
		text += ((text != null) ? (dataLabelSetting.Seperator + obj) : obj);
		return text;
	}

	internal string UpdateDataLabelValue(string text, object value, DataLabelSetting dataLabelSetting)
	{
		string text2 = null;
		if (dataLabelSetting.NumberFormat != null)
		{
			if (dataLabelSetting.NumberFormat.ToLower() != "general")
			{
				if (!dataLabelSetting.IsPercentage || dataLabelSetting.IsSourceLinked)
				{
					value = this.NumberFormatApplyEvent(value, dataLabelSetting.NumberFormat);
				}
			}
			else if (value is double)
			{
				text2 = value.ToString();
				if (text2.Length > 9)
				{
					text2 = Math.Round((double)value, 9).ToString(CultureInfo.InvariantCulture).TrimEnd('0');
				}
			}
		}
		text2 = ((text2 != null) ? text2 : value.ToString());
		text += ((text != null) ? (dataLabelSetting.Seperator + text2) : text2);
		return text;
	}

	internal string UpdateDataLabelPercentange(string text, object value, DataLabelSetting dataLabelSetting)
	{
		double result = 0.0;
		if (!(value is string))
		{
			result = ((!(value is IConvertible)) ? 0.0 : Convert.ToDouble(value));
		}
		else
		{
			double.TryParse(value.ToString(), out result);
		}
		result /= Percentage;
		if (dataLabelSetting.NumberFormat != null && dataLabelSetting.NumberFormat.ToLower() != "general" && !dataLabelSetting.IsSourceLinked)
		{
			value = this.NumberFormatApplyEvent(result, dataLabelSetting.NumberFormat);
			text += ((text != null) ? (dataLabelSetting.Seperator + value) : value);
		}
		else
		{
			value = Math.Round(result * 100.0, MidpointRounding.AwayFromZero);
			text += ((text != null) ? (dataLabelSetting.Seperator + value?.ToString() + "%") : (value?.ToString() + "%"));
		}
		return text;
	}
}
