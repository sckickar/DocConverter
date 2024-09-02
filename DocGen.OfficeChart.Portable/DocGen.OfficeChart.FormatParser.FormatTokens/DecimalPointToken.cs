using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class DecimalPointToken : SingleCharToken
{
	private const char DEF_FORMAT = '.';

	public override char FormatChar => '.';

	public override TokenType TokenType => TokenType.DecimalPoint;

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		if (m_strFormat == null || m_strFormat != CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
		{
			return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
		}
		return m_strFormat;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
