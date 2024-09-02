using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class MinuteToken : FormatTokenBase
{
	private static readonly Regex MinuteRegex = new Regex("[mM]{1,2}", RegexOptions.None);

	private const string DEF_FORMAT_LONG = "00";

	private const double DEF_OLE_DOUBLE = 2958465.9999999884;

	private const double DEF_MAX_DOUBLE = 2958466.0;

	public override TokenType TokenType => TokenType.Minute;

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(MinuteRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		int minute = DateTimeExtension.FromOADate(value).Minute;
		if (m_strFormat.Length > 1)
		{
			return minute.ToString("00");
		}
		return minute.ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}

	protected override void OnFormatChange()
	{
		base.OnFormatChange();
		m_strFormat = m_strFormat.ToLower();
	}
}
