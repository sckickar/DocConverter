using System;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal abstract class InBracketToken : FormatTokenBase
{
	private const char DEF_START = '[';

	private const char DEF_END = ']';

	public InBracketToken()
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
			throw new ArgumentException("strFormat - string cannot be empty.");
		}
		if (iIndex < 0 || iIndex > length - 1)
		{
			throw new ArgumentOutOfRangeException("iIndex", "Value cannot be less than 0 and greater than than format length - 1.");
		}
		int num = iIndex;
		if (strFormat[iIndex] != '[')
		{
			return iIndex;
		}
		iIndex++;
		int num2 = strFormat.IndexOf(']', iIndex);
		if (num2 < iIndex)
		{
			return num;
		}
		return TryParse(strFormat, num, iIndex, num2);
	}

	public abstract int TryParse(string strFormat, int iStartIndex, int iIndex, int iEndIndex);
}
