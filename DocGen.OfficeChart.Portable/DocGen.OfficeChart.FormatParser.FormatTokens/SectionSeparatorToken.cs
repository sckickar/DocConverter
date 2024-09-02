using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class SectionSeparatorToken : FormatTokenBase
{
	private const string DEF_SEPARATOR = ";";

	public override TokenType TokenType => TokenType.Section;

	public SectionSeparatorToken()
	{
		m_strFormat = ";";
	}

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
		int length2 = m_strFormat.Length;
		if (string.Compare(strFormat, iIndex, m_strFormat, 0, length2) == 0)
		{
			iIndex += length2;
		}
		return iIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		throw new NotSupportedException();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		throw new NotSupportedException();
	}
}
