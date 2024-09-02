using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class FractionToken : SingleCharToken
{
	private const char DEF_FORMAT_CHAR = '/';

	public override char FormatChar => '/';

	public override TokenType TokenType => TokenType.Fraction;

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return m_strFormat;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return m_strFormat;
	}
}
