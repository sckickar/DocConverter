using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class ThousandsSeparatorToken : SingleCharToken
{
	private const char DEF_FORMAT = ',';

	private bool m_bAfterDigits;

	public override char FormatChar => ',';

	public override TokenType TokenType => TokenType.ThousandsSeparator;

	public bool IsAfterDigits
	{
		get
		{
			return m_bAfterDigits;
		}
		set
		{
			m_bAfterDigits = value;
		}
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return string.Empty;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}

	public double PreprocessValue(double value)
	{
		if (m_bAfterDigits)
		{
			value /= 1000.0;
		}
		return value;
	}
}
