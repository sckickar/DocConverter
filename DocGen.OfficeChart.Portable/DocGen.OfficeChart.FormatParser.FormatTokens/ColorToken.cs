using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class ColorToken : InBracketToken
{
	private static readonly Regex ColorRegex = new Regex("[Color [0-9]+]");

	private static readonly string[] DEF_KNOWN_COLORS = new string[8] { "Black", "White", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };

	private const string DEF_COLOR = "Color";

	private const int DEF_MIN_COLOR_INDEX = 1;

	private const int DEF_MAX_COLOR_INDEX = 56;

	private const int DEF_COLOR_INCREMENT = 7;

	private int m_iColorIndex = -1;

	public override TokenType TokenType => TokenType.Color;

	public override int TryParse(string strFormat, int iStartIndex, int iIndex, int iEndIndex)
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
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		m_iColorIndex = FindColor(strFormat, iIndex);
		if (m_iColorIndex < 0)
		{
			m_iColorIndex = TryDetectColorNumber(strFormat, iIndex, iEndIndex);
			if (m_iColorIndex < 0)
			{
				return iStartIndex;
			}
			m_iColorIndex += 7;
			iIndex = iEndIndex + 1;
		}
		else
		{
			string text = DEF_KNOWN_COLORS[m_iColorIndex];
			iIndex += text.Length;
			m_strFormat = text;
			if (iIndex != iEndIndex)
			{
				return iStartIndex;
			}
			iIndex++;
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

	private int FindColor(string strFormat, int iIndex)
	{
		return FindString(DEF_KNOWN_COLORS, strFormat, iIndex, bIgnoreCase: true);
	}

	private int TryDetectColorNumber(string strFormat, int iIndex, int iEndIndex)
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
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		if (iEndIndex < 0 || iEndIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iEndIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		int length2 = "Color".Length;
		if (string.Compare(strFormat, iIndex, "Color", 0, length2, StringComparison.CurrentCultureIgnoreCase) == 0)
		{
			int num = iIndex + length2;
			if (double.TryParse(strFormat.Substring(num, iEndIndex - num), NumberStyles.Integer, null, out var result))
			{
				int num2 = (int)result;
				if (num2 < 1 || num2 > 56)
				{
					throw new ArgumentOutOfRangeException("Color index");
				}
				return num2;
			}
		}
		return -1;
	}
}
