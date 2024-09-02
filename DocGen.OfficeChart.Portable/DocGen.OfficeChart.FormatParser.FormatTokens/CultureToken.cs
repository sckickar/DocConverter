using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class CultureToken : FormatTokenBase
{
	private const string DEF_LOCALE_GROUP = "LocaleID";

	private const string DEF_CHAR_GROUP = "Character";

	private const int SystemSettingsLocaleId = 63488;

	private static readonly Regex CultureRegex = new Regex("\\[\\$(?<Character>.?)\\-(?<LocaleID>[0-9A-Za-z]+)\\]", RegexOptions.None);

	private int m_iLocaleId;

	private string m_strCharacter;

	public override TokenType TokenType => TokenType.Culture;

	public CultureInfo Culture => CultureInfo.CurrentCulture;

	public bool UseSystemSettings => m_iLocaleId == 63488;

	public override int TryParse(string strFormat, int iIndex)
	{
		Match m;
		int num = TryParseRegex(CultureRegex, strFormat, iIndex, out m);
		if (num != iIndex)
		{
			string value = m.Groups["LocaleID"].Value;
			m_iLocaleId = int.Parse(value, NumberStyles.HexNumber);
			m_strCharacter = m.Groups["Character"].Value;
		}
		return num;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		if (m_strCharacter == null)
		{
			return string.Empty;
		}
		return m_strCharacter;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		if (m_strCharacter == null)
		{
			return string.Empty;
		}
		return m_strCharacter;
	}
}
