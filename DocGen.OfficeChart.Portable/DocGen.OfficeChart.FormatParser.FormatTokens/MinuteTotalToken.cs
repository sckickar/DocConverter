using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class MinuteTotalToken : FormatTokenBase
{
	private static readonly Regex HourRegex = new Regex("\\[[m]+\\]", RegexOptions.None);

	public override TokenType TokenType => TokenType.MinuteTotal;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(HourRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return ((int)(value * 1440.0)).ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
