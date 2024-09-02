using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class SecondToken : FormatTokenBase
{
	private static readonly Regex SecondRegex = new Regex("[sS]+", RegexOptions.None);

	private const string DEF_FORMAT_LONG = "00";

	private const int DEF_MILLISECOND_HALF = 500;

	private const double DEF_OLE_DOUBLE = 2958465.9999999884;

	private const double DEF_MAX_DOUBLE = 2958466.0;

	private bool m_bRound = true;

	public override TokenType TokenType => TokenType.Second;

	public bool RoundValue
	{
		get
		{
			return m_bRound;
		}
		set
		{
			m_bRound = value;
		}
	}

	public override int TryParse(string strFormat, int iIndex)
	{
		return TryParseRegex(SecondRegex, strFormat, iIndex);
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		DateTime dateTime = CalcEngineHelper.FromOADate(value);
		int num = dateTime.Second;
		int millisecond = dateTime.Millisecond;
		if (m_bRound && millisecond >= 500)
		{
			num++;
		}
		if (m_strFormat.Length > 1)
		{
			return num.ToString("00");
		}
		return num.ToString();
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}
}
