using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class GeneralToken : FormatTokenBase
{
	private const string DEF_FORMAT = "General";

	public override TokenType TokenType => TokenType.General;

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
		if (string.Compare(strFormat, iIndex, "General", 0, "General".Length, StringComparison.CurrentCultureIgnoreCase) == 0)
		{
			iIndex += "General".Length;
		}
		return iIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		string text;
		try
		{
			text = Convert.ToDecimal(value).ToString();
		}
		catch (OverflowException)
		{
			text = value.ToString();
		}
		if (text.Contains("0.0000") && text.Length < 12)
		{
			return text;
		}
		return value.ToString(culture);
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return value;
	}
}
