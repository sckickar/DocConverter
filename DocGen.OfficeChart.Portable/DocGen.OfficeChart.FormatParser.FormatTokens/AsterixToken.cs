using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class AsterixToken : FormatTokenBase
{
	private const char DEF_START = '*';

	public override TokenType TokenType => TokenType.Asterix;

	public override int TryParse(string strFormat, int iIndex)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		int length = strFormat.Length;
		if (length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty");
		}
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		if (strFormat[iIndex] == '*' && length > iIndex + 1)
		{
			m_strFormat = strFormat[iIndex + 1].ToString();
			iIndex += 2;
		}
		return iIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return string.Empty;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
