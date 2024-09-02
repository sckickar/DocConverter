using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class StringToken : FormatTokenBase
{
	private static readonly Regex DayRegex = new Regex("\"[^\"]*\"", RegexOptions.None);

	public override TokenType TokenType => TokenType.String;

	public override int TryParse(string strFormat, int iIndex)
	{
		int num = TryParseRegex(DayRegex, strFormat, iIndex);
		if (num != iIndex)
		{
			m_strFormat = strFormat.Substring(iIndex + 1, m_strFormat.Length - 2);
		}
		return num;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return m_strFormat;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return m_strFormat;
	}
}
