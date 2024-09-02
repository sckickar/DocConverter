using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class DayToken : FormatTokenBase
{
	private static readonly Regex DayRegex = new Regex("[Dd]+", RegexOptions.None);

	private string m_strFormatLower;

	public override TokenType TokenType => TokenType.Day;

	public override int TryParse(string strFormat, int iIndex)
	{
		int num = TryParseRegex(DayRegex, strFormat, iIndex);
		if (num != iIndex)
		{
			m_strFormatLower = m_strFormat.ToLower();
		}
		return num;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return CalcEngineHelper.FromOADate(value).ToString(" " + m_strFormatLower, culture).Substring(1);
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
