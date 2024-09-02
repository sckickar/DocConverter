using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class TextToken : SingleCharToken
{
	private const char DEF_FORMAT_CHAR = '@';

	public override TokenType TokenType => TokenType.Text;

	public override char FormatChar => '@';

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return value.ToString(culture);
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		if (m_strFormat != null)
		{
			return value;
		}
		return null;
	}
}
