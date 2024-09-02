using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class CharacterToken : FormatTokenBase
{
	private const char DEF_START = '\\';

	private const char DEF_FORMAT_CHAR = '@';

	public override TokenType TokenType => TokenType.Character;

	public override int TryParse(string strFormat, int iIndex)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		if (strFormat.Length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty.");
		}
		if (strFormat[iIndex] == '\\')
		{
			m_strFormat = strFormat[iIndex + 1].ToString();
			if (m_strFormat != '@'.ToString())
			{
				iIndex += 2;
			}
			else
			{
				m_strFormat = '@'.ToString();
			}
		}
		else if (strFormat[iIndex] == '[' && strFormat[iIndex + 2] == '$')
		{
			m_strFormat = strFormat[iIndex + 1].ToString();
			iIndex = strFormat.IndexOf(']', iIndex + 3) + 1;
		}
		return iIndex;
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
