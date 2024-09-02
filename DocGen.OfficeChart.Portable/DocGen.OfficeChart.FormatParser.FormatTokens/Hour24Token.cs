using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class Hour24Token : FormatTokenBase
{
	private static readonly Regex HourRegex = new Regex("\\[[hH]+\\]", RegexOptions.None);

	public override TokenType TokenType => TokenType.Hour24;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(HourRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		double num = value;
		if (num <= 60.0)
		{
			num -= 1.0;
		}
		double num2 = num * 24.0;
		num2 = ((value > 0.0) ? Math.Floor(num2) : Math.Ceiling(num2));
		return ((int)num2).ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
