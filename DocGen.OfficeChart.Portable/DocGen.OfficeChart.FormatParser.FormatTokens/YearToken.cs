using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class YearToken : FormatTokenBase
{
	private static readonly Regex YearRegex = new Regex("[yY]+", RegexOptions.None);

	private const string DEF_FORMAT_SHORT = "00";

	public override TokenType TokenType => TokenType.Year;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(YearRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		int year = CalcEngineHelper.FromOADate(value).Year;
		if (m_strFormat.Length > 2)
		{
			return year.ToString();
		}
		return (year % 100).ToString("00");
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
