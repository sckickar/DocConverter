using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class PercentToken : SingleCharToken
{
	private const char DEF_FORMAT_CHAR = '%';

	public override TokenType TokenType => TokenType.Percent;

	public override char FormatChar => '%';

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return '%'.ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
