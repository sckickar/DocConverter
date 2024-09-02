using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class SecondTotalToken : FormatTokenBase
{
	private static readonly Regex HourRegex = new Regex("\\[[sS]+\\]", RegexOptions.None);

	public override TokenType TokenType => TokenType.SecondTotal;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(HourRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return ((int)(value * 86400.0)).ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
