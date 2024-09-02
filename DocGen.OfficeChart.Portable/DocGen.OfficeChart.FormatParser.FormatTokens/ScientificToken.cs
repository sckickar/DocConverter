using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class ScientificToken : FormatTokenBase
{
	private static readonly string[] PossibleFormats = new string[2] { "E+", "E-" };

	private const string DEF_SHORT_FORM = "E";

	private int m_iFormatIndex = -1;

	public override TokenType TokenType => TokenType.Scientific;

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
		m_iFormatIndex = FindString(PossibleFormats, strFormat, iIndex, bIgnoreCase: false);
		if (m_iFormatIndex < 0)
		{
			return iIndex;
		}
		return iIndex + PossibleFormats[m_iFormatIndex].Length;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		if (m_iFormatIndex == 0 && value >= 0.0)
		{
			return PossibleFormats[0];
		}
		return "E";
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
