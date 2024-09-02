using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class MonthToken : FormatTokenBase
{
	private static readonly Regex MonthRegex = new Regex("[Mm]{3,}", RegexOptions.None);

	private const string DEF_FORMAT_SHORT = "00";

	private const int DEF_FULL_NAME_LENGTH = 4;

	private const int DEF_SHORT_NAME_LENGTH = 3;

	private const int DEF_LONG_NUMBER_LENGTH = 3;

	private const string DEF_LONG_NUMBER_FORMAT = "00";

	public override TokenType TokenType => TokenType.Month;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(MonthRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return CalcEngineHelper.FromOADate(value).ToString(" " + m_strFormat, culture).Substring(1);
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}

	protected override void OnFormatChange()
	{
		base.OnFormatChange();
		m_strFormat = m_strFormat.ToUpper();
	}
}
