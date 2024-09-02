using System;
using System.Globalization;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class MilliSecondToken : FormatTokenBase
{
	private const string DEF_FORMAT_LONG = "000";

	private static readonly int DEF_MAX_LEN = "000".Length;

	private const string DEF_DOT = ".";

	private const double DEF_OLE_DOUBLE = 2958465.9999999884;

	private const double DEF_MAX_DOUBLE = 2958466.0;

	public override TokenType TokenType => TokenType.MilliSecond;

	public override int TryParse(string strFormat, int iIndex)
	{
		throw new NotImplementedException();
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		int num = CalcEngineHelper.FromOADate(value).Millisecond;
		int length = m_strFormat.Length;
		string empty = string.Empty;
		string text = string.Empty;
		if (length < DEF_MAX_LEN)
		{
			int num2 = DEF_MAX_LEN - length;
			num = (int)FormatSection.Round((double)num / Math.Pow(10.0, num2));
			empty = m_strFormat.Substring(1, length - 1);
		}
		else
		{
			empty = "000";
			text = m_strFormat.Substring(DEF_MAX_LEN);
		}
		return "." + num.ToString(empty) + text;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
