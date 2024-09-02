using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class ReservedPlaceToken : FormatTokenBase
{
	private const char DEF_START = '_';

	private const string DEF_SPACE = " ";

	public override TokenType TokenType => TokenType.ReservedPlace;

	public override int TryParse(string strFormat, int iIndex)
	{
		if (strFormat == null)
		{
			throw new ArgumentNullException("strFormat");
		}
		int length = strFormat.Length;
		if (length == 0)
		{
			throw new ArgumentException("strFormat - string cannot be empty.");
		}
		bool flag = iIndex + 1 >= length;
		if (strFormat[iIndex] == '_' && !flag)
		{
			m_strFormat = strFormat[iIndex + 1].ToString();
			iIndex += 2;
		}
		return iIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return ApplyFormat(string.Empty, bShowHiddenSymbols);
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		if (bShowHiddenSymbols)
		{
			return m_strFormat;
		}
		return " ";
	}
}
