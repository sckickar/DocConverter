using System;
using System.Globalization;
using DocGen.OfficeChart.Calculate;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class AmPmToken : FormatTokenBase
{
	private const string DefaultStart2 = "tt";

	private const string DEF_START = "AM/PM";

	private static readonly int DEF_LENGTH = "AM/PM".Length;

	private const int DEF_AMPM_EDGE = 12;

	private const string DEF_AM = "AM";

	private const string DEF_PM = "PM";

	public override TokenType TokenType => TokenType.AmPm;

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
		if (string.Compare(strFormat, iIndex, "AM/PM", 0, DEF_LENGTH, StringComparison.CurrentCultureIgnoreCase) == 0)
		{
			m_strFormat = "AM/PM";
			iIndex += DEF_LENGTH;
		}
		return iIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		if (CalcEngineHelper.FromOADate(value).Hour <= 12)
		{
			return culture.DateTimeFormat.AMDesignator;
		}
		return culture.DateTimeFormat.PMDesignator;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		throw new NotSupportedException();
	}

	internal static string CheckAndApplyAMPM(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException(format);
		}
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		int num = new HourToken().TryParse(format, 0);
		int num2 = new MinuteToken().TryParse(format, num);
		int num3 = new SecondToken().TryParse(format, num2);
		if ((num != 0 || num2 != 0 || num3 != 0) && format.Contains("tt") && currentCulture.DateTimeFormat.ShortTimePattern.Contains("tt"))
		{
			return format.Replace("tt", "AM/PM");
		}
		return format;
	}
}
