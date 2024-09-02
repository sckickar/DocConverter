using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal abstract class SingleCharToken : FormatTokenBase
{
	public abstract char FormatChar { get; }

	public SingleCharToken()
	{
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
		char c = strFormat[iIndex];
		if (c == FormatChar)
		{
			iIndex++;
			m_strFormat = c.ToString();
		}
		else if (strFormat[iIndex] == '\\' && strFormat[iIndex + 1] == FormatChar)
		{
			m_strFormat = strFormat[iIndex + 1].ToString();
			iIndex += 2;
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
