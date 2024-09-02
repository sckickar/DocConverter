using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class UnknownToken : FormatTokenBase
{
	public override TokenType TokenType => TokenType.Unknown;

	public override int TryParse(string strFormat, int iIndex)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		if (strFormat.Length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty");
		}
		m_strFormat = strFormat[iIndex].ToString();
		return iIndex + 1;
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
