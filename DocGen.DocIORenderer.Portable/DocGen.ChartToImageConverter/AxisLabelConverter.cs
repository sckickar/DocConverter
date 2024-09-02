using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocGen.ChartToImageConverter;

internal class AxisLabelConverter
{
	internal string NumberFormat = "General";

	private Dictionary<double, string> m_customRadarAxisLabels;

	internal byte AxisTypeInByte;

	private Regex IntervalRegex = new Regex("(?<start>\\[|\\(){1}(?<first>[-0-9]+(\\.[0-9]+)*),(?<second>[-0-9]+(\\.[0-9]+)*)(?<end>\\]|\\)){1}");

	private Regex FlowBinRegex = new Regex("(?<first>[-0-9]+(\\.[0-9]+)*)");

	internal Dictionary<double, string> CustomRadarAxisLabels
	{
		get
		{
			return m_customRadarAxisLabels;
		}
		set
		{
			m_customRadarAxisLabels = value;
		}
	}

	internal double PreviousLabelValue { get; set; }

	internal event Func<object, string, string> NumberFormatApplyEvent;

	public string GetLabelText(double value, string content)
	{
		if (CustomRadarAxisLabels != null && CustomRadarAxisLabels.ContainsKey(value))
		{
			return CustomRadarAxisLabels[value];
		}
		string text = null;
		if ((AxisTypeInByte & 8u) != 0)
		{
			return string.Empty;
		}
		if (this.NumberFormatApplyEvent != null && NumberFormat != null && (!(NumberFormat.ToLower() == "general") || (AxisTypeInByte & 4u) != 0))
		{
			if ((AxisTypeInByte & 2u) != 0)
			{
				value /= 100.0;
				text = this.NumberFormatApplyEvent(value, NumberFormat);
			}
			else if (((uint)AxisTypeInByte & (true ? 1u : 0u)) != 0)
			{
				Match match = IntervalRegex.Match(content);
				if (match != null && match.Success)
				{
					if (double.TryParse(match.Groups["first"].Value, out var result) && double.TryParse(match.Groups["second"].Value, out var result2))
					{
						return match.Groups["start"].Value + this.NumberFormatApplyEvent(result, NumberFormat) + "," + this.NumberFormatApplyEvent(result2, NumberFormat) + match.Groups["end"].Value;
					}
				}
				else
				{
					match = FlowBinRegex.Match(content);
					if (match != null && match.Success && double.TryParse(match.Groups["first"].Value, out var result3))
					{
						return content.Replace(match.Groups["first"].Value, this.NumberFormatApplyEvent(result3, NumberFormat));
					}
				}
				text = content;
			}
			else
			{
				text = this.NumberFormatApplyEvent(value, NumberFormat);
			}
		}
		else if (((uint)AxisTypeInByte & (true ? 1u : 0u)) != 0 || (AxisTypeInByte & 4u) != 0)
		{
			text = content;
		}
		else
		{
			text = value.ToString();
			if (value != 0.0 && text.Contains("E") && content != null && content.Equals("0") && !(PreviousLabelValue - value).ToString().Contains("E"))
			{
				text = "0";
			}
			PreviousLabelValue = value;
		}
		return text;
	}
}
