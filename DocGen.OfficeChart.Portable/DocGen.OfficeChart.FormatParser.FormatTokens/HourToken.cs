using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class HourToken : FormatTokenBase
{
	private static readonly Regex HourRegex = new Regex("[hH]+", RegexOptions.None);

	private const string DEF_FORMAT_LONG = "00";

	private bool m_bAmPm;

	public override TokenType TokenType => TokenType.Hour;

	public bool IsAmPm
	{
		get
		{
			return m_bAmPm;
		}
		set
		{
			m_bAmPm = value;
		}
	}

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(HourRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		int num = DateTimeExtension.FromOADate(value).Hour;
		if (IsAmPm && num > 12)
		{
			num -= 12;
		}
		if (m_strFormat.Length > 1)
		{
			return num.ToString("00");
		}
		return num.ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
