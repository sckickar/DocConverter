using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.FormatParser.FormatTokens;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.FormatParser;

internal class FormatSection : CommonObject
{
	private static readonly object[] DEF_POSSIBLE_TOKENS = new object[12]
	{
		new TokenType[5]
		{
			TokenType.Unknown,
			TokenType.String,
			TokenType.ReservedPlace,
			TokenType.Character,
			TokenType.Color
		},
		OfficeFormatType.Unknown,
		new TokenType[2]
		{
			TokenType.General,
			TokenType.Culture
		},
		OfficeFormatType.General,
		new TokenType[9]
		{
			TokenType.Unknown,
			TokenType.String,
			TokenType.ReservedPlace,
			TokenType.Character,
			TokenType.Color,
			TokenType.Condition,
			TokenType.Text,
			TokenType.Asterix,
			TokenType.Culture
		},
		OfficeFormatType.Text,
		new TokenType[16]
		{
			TokenType.Unknown,
			TokenType.String,
			TokenType.ReservedPlace,
			TokenType.Character,
			TokenType.Color,
			TokenType.Condition,
			TokenType.SignificantDigit,
			TokenType.InsignificantDigit,
			TokenType.PlaceReservedDigit,
			TokenType.Percent,
			TokenType.Scientific,
			TokenType.ThousandsSeparator,
			TokenType.DecimalPoint,
			TokenType.Asterix,
			TokenType.Fraction,
			TokenType.Culture
		},
		OfficeFormatType.Number,
		new TokenType[17]
		{
			TokenType.Unknown,
			TokenType.Day,
			TokenType.String,
			TokenType.ReservedPlace,
			TokenType.Character,
			TokenType.Color,
			TokenType.Condition,
			TokenType.SignificantDigit,
			TokenType.InsignificantDigit,
			TokenType.PlaceReservedDigit,
			TokenType.Percent,
			TokenType.Scientific,
			TokenType.ThousandsSeparator,
			TokenType.DecimalPoint,
			TokenType.Asterix,
			TokenType.Fraction,
			TokenType.Culture
		},
		OfficeFormatType.Number,
		new TokenType[21]
		{
			TokenType.Unknown,
			TokenType.Hour,
			TokenType.Hour24,
			TokenType.Minute,
			TokenType.MinuteTotal,
			TokenType.Second,
			TokenType.SecondTotal,
			TokenType.Year,
			TokenType.Month,
			TokenType.Day,
			TokenType.String,
			TokenType.ReservedPlace,
			TokenType.Character,
			TokenType.AmPm,
			TokenType.Color,
			TokenType.Condition,
			TokenType.SignificantDigit,
			TokenType.DecimalPoint,
			TokenType.Asterix,
			TokenType.Fraction,
			TokenType.Culture
		},
		OfficeFormatType.DateTime
	};

	private static readonly TokenType[] DEF_BREAK_HOUR = new TokenType[1] { TokenType.Minute };

	private static readonly TokenType[] DEF_BREAK_SECOND = new TokenType[5]
	{
		TokenType.Minute,
		TokenType.Hour,
		TokenType.Day,
		TokenType.Month,
		TokenType.Year
	};

	private const int DEF_NOTFOUND_INDEX = -1;

	private static readonly TokenType[] DEF_MILLISECONDTOKENS = new TokenType[1] { TokenType.SignificantDigit };

	private const string DEF_THOUSAND_SEPARATOR = ",";

	private const string DEF_MINUS = "-";

	private const int DEF_ROUNDOFF_DIGIT = 9;

	private static readonly TokenType[] NotTimeTokens = new TokenType[3]
	{
		TokenType.Day,
		TokenType.Month,
		TokenType.Year
	};

	private static readonly TokenType[] NotDateTokens = new TokenType[4]
	{
		TokenType.Hour,
		TokenType.Minute,
		TokenType.Second,
		TokenType.AmPm
	};

	private List<FormatTokenBase> m_arrTokens;

	private bool m_bFormatPrepared;

	private int m_iDecimalPointPos = -1;

	private int m_iScientificPos = -1;

	private int m_iLastDigit = -1;

	private bool m_bLastGroups;

	private int m_iNumberOfFractionDigits;

	private int m_iNumberOfIntegerDigits;

	private int m_iFractionPos = -1;

	private bool m_bFraction;

	private int m_iFractionStart;

	private int m_iFractionEnd;

	private int m_iDenumaratorLen;

	private int m_iIntegerEnd = -1;

	private int m_iDecimalEnd = -1;

	private ConditionToken m_condition;

	private CultureToken m_culture;

	private OfficeFormatType m_formatType;

	private bool m_bGroupDigits;

	private bool m_bMultiplePoints;

	private bool m_bUseSystemDateFormat;

	internal bool IsTimeFormat
	{
		get
		{
			if (FormatType != OfficeFormatType.DateTime)
			{
				return false;
			}
			bool result = true;
			foreach (FormatTokenBase arrToken in m_arrTokens)
			{
				if (Array.IndexOf(NotTimeTokens, arrToken.TokenType) >= 0)
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}

	internal bool IsDateFormat
	{
		get
		{
			if (FormatType != OfficeFormatType.DateTime)
			{
				return false;
			}
			bool result = true;
			foreach (FormatTokenBase arrToken in m_arrTokens)
			{
				if (Array.IndexOf(NotDateTokens, arrToken.TokenType) >= 0)
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}

	public FormatTokenBase this[int index]
	{
		get
		{
			if (index < 0 || index > Count - 1)
			{
				throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count - 1.");
			}
			return m_arrTokens[index];
		}
	}

	public int Count => m_arrTokens.Count;

	public bool HasCondition => m_condition != null;

	public OfficeFormatType FormatType
	{
		get
		{
			if (m_formatType == OfficeFormatType.Unknown)
			{
				DetectFormatType();
			}
			return m_formatType;
		}
	}

	public CultureInfo Culture
	{
		get
		{
			if (m_culture == null)
			{
				return CultureInfo.CurrentCulture;
			}
			return m_culture.Culture;
		}
	}

	public bool IsFraction => m_bFraction;

	public bool IsScientific => m_iScientificPos >= 0;

	public bool IsThousandSeparator => m_bGroupDigits;

	public int DecimalNumber => m_iNumberOfFractionDigits;

	private FormatSection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public FormatSection(IApplication application, object parent, List<FormatTokenBase> arrTokens)
		: base(application, parent)
	{
		if (arrTokens == null)
		{
			throw new ArgumentNullException("arrTokens");
		}
		m_arrTokens = new List<FormatTokenBase>(arrTokens);
		PrepareFormat();
	}

	public void PrepareFormat()
	{
		if (!m_bFormatPrepared)
		{
			PreparePositions();
			m_iLastDigit = LocateLastFractionDigit();
			m_bLastGroups = LocateLastGroups(m_iLastDigit + 1);
			if (FormatType == OfficeFormatType.Number)
			{
				m_iNumberOfFractionDigits = CalculateFractionDigits();
				m_iNumberOfIntegerDigits = CalculateIntegerDigits();
				LocateFractionParts();
			}
			else if (FormatType == OfficeFormatType.DateTime)
			{
				SetRoundSeconds();
				m_iDecimalPointPos = -1;
				m_bFraction = false;
			}
			else
			{
				m_iNumberOfFractionDigits = -1;
				m_iNumberOfIntegerDigits = -1;
			}
			int count = Count;
			m_iDecimalEnd = count - 1;
			m_iIntegerEnd = count - 1;
			if (m_iScientificPos > 0)
			{
				m_iIntegerEnd = (m_iDecimalEnd = m_iScientificPos - 1);
			}
			else if (m_bFraction)
			{
				LocateFractionParts();
				m_iIntegerEnd = m_iFractionStart - 1;
			}
			if (m_iDecimalPointPos > 0)
			{
				m_iIntegerEnd = m_iDecimalPointPos - 1;
			}
			if (FormatType == OfficeFormatType.Number)
			{
				m_bGroupDigits = CheckGroupDigits();
			}
			m_bFormatPrepared = true;
			if (m_arrTokens.Count > 0 && m_arrTokens[0] is CultureToken cultureToken)
			{
				m_bUseSystemDateFormat = cultureToken.UseSystemSettings;
			}
		}
	}

	private bool CheckGroupDigits()
	{
		int i = 0;
		for (int num = m_iIntegerEnd - 1; i <= num; i++)
		{
			if (this[i].TokenType == TokenType.ThousandsSeparator && this[i - 1] is DigitToken && this[i + 1] is DigitToken)
			{
				return true;
			}
		}
		return false;
	}

	private void PreparePositions()
	{
		bool flag = false;
		m_bMultiplePoints = false;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			FormatTokenBase formatTokenBase = this[i];
			switch (formatTokenBase.TokenType)
			{
			case TokenType.AmPm:
			{
				HourToken hourToken = FindCorrespondingHourSection(i);
				if (hourToken != null)
				{
					hourToken.IsAmPm = true;
				}
				break;
			}
			case TokenType.Minute:
				CheckMinuteToken(i);
				break;
			case TokenType.DecimalPoint:
				if (m_iDecimalPointPos < 0)
				{
					AssignPosition(ref m_iDecimalPointPos, i);
				}
				else
				{
					m_bMultiplePoints = true;
				}
				break;
			case TokenType.Scientific:
				AssignPosition(ref m_iScientificPos, i);
				break;
			case TokenType.SignificantDigit:
			case TokenType.InsignificantDigit:
			case TokenType.PlaceReservedDigit:
			{
				DigitToken digitToken = (DigitToken)formatTokenBase;
				if (!flag)
				{
					digitToken.IsLastDigit = true;
					flag = true;
				}
				break;
			}
			case TokenType.Fraction:
				if (m_iFractionPos < 0)
				{
					m_iFractionPos = i;
					m_bFraction = true;
				}
				else
				{
					m_bFraction = false;
				}
				break;
			case TokenType.Condition:
				if (m_condition != null)
				{
					throw new FormatException("More than one condition was found in the section.");
				}
				m_condition = (ConditionToken)formatTokenBase;
				break;
			case TokenType.Culture:
				if (m_culture != null)
				{
					throw new FormatException("More than one culture information was found in the section");
				}
				m_culture = (CultureToken)formatTokenBase;
				break;
			}
		}
		PrepareInsignificantDigits();
	}

	private void PrepareInsignificantDigits()
	{
		if (m_iDecimalPointPos < 0)
		{
			return;
		}
		for (int num = Count - 1; num > m_iDecimalPointPos; num--)
		{
			if (m_arrTokens[num] is DigitToken digitToken)
			{
				if (!(digitToken is InsignificantDigitToken insignificantDigitToken))
				{
					break;
				}
				insignificantDigitToken.HideIfZero = true;
			}
		}
	}

	public HourToken FindCorrespondingHourSection(int index)
	{
		int num = index;
		do
		{
			num--;
			if (num < 0)
			{
				num += Count;
			}
			FormatTokenBase formatTokenBase = this[num];
			if (formatTokenBase.TokenType == TokenType.Hour)
			{
				return (HourToken)formatTokenBase;
			}
		}
		while (num != index);
		return null;
	}

	public string ApplyFormat(double value)
	{
		return ApplyFormat(value, bShowReservedSymbols: false);
	}

	public string ApplyFormat(string value)
	{
		return ApplyFormat(value, bShowReservedSymbols: false);
	}

	public string ApplyFormat(double value, bool bShowReservedSymbols)
	{
		PrepareFormat();
		PrepareValue(ref value, bShowReservedSymbols);
		if (m_bUseSystemDateFormat)
		{
			return CalcEngineHelper.FromOADate(value).ToString();
		}
		int count = Count;
		double dFraction = 0.0;
		double num = ((value != 0.0) ? Math.Floor(Math.Log10(Math.Abs(value))) : value);
		double num2 = value / Math.Pow(10.0, num);
		if (m_iScientificPos > 0)
		{
			int num3 = m_iNumberOfIntegerDigits - 1;
			num2 *= Math.Pow(10.0, num3);
			num -= (double)num3;
			value = num2;
		}
		bool flag = value < 0.0;
		if (m_formatType == OfficeFormatType.Number && !m_bFraction)
		{
			value = SplitValue(value, out dFraction);
			double num4 = Math.Pow(10.0, m_iNumberOfFractionDigits);
			dFraction *= num4;
			dFraction = Round(dFraction);
			if (dFraction >= num4)
			{
				dFraction -= num4;
				value = ((!flag) ? (value + 1.0) : (value - 1.0));
			}
		}
		PrepareDigits(0, m_arrTokens.Count, IsCenterDigit: false);
		if (value == 0.0)
		{
			flag = flag && dFraction > 0.0;
		}
		string text = ApplyFormat(value, bShowReservedSymbols, 0, m_iIntegerEnd, bForward: false, m_bGroupDigits, flag);
		if (m_iDecimalPointPos > 0)
		{
			int num5 = m_iDecimalPointPos + 1;
			int length = dFraction.ToString().Length;
			if (length < DecimalNumber && dFraction != 0.0)
			{
				PrepareDigits(num5, num5 + (DecimalNumber - length), IsCenterDigit: true);
			}
			text += ApplyFormat(dFraction, bShowReservedSymbols, m_iDecimalPointPos, m_iDecimalEnd, bForward: false);
		}
		if (m_iScientificPos > 0)
		{
			text += ApplyFormat(num, bShowReservedSymbols, m_iDecimalEnd + 1, count - 1, bForward: false, bGroupDigits: false, num < 0.0);
		}
		else if (m_bFraction)
		{
			dFraction = value;
			if (IsAnyDigit(0, m_iIntegerEnd))
			{
				dFraction -= ((value > 0.0) ? Math.Floor(value) : Math.Ceiling(value));
				dFraction = Math.Abs(dFraction);
			}
			if (dFraction != 0.0)
			{
				Fraction fraction = Fraction.ConvertToFraction(dFraction, m_iDenumaratorLen);
				long num6 = (long)fraction.Numerator;
				long num7 = (long)fraction.Denumerator;
				if (text == "1 ")
				{
					text = " ";
				}
				text += ApplyFormat(num6, bShowReservedSymbols, m_iIntegerEnd + 1, m_iFractionPos, bForward: false);
				text += ApplyFormat(num7, bShowReservedSymbols, m_iFractionPos + 1, m_iFractionEnd, bForward: false);
				text += ApplyFormat(0.0, bShowReservedSymbols, m_iFractionEnd + 1, count - 1, bForward: false);
			}
		}
		if (m_formatType == OfficeFormatType.General && decimal.TryParse(text, out var result) && text.Length > 9)
		{
			text = Math.Round(result, 9).ToString();
		}
		return text;
	}

	private void PrepareDigits(int startPos, int count, bool IsCenterDigit)
	{
		for (int i = startPos; i < count; i++)
		{
			if (m_arrTokens[i] is DigitToken digitToken)
			{
				digitToken.IsCenterDigit = IsCenterDigit;
			}
		}
	}

	public string ApplyFormat(string value, bool bShowReservedSymbols)
	{
		PrepareFormat();
		if ((m_formatType != OfficeFormatType.Text && m_formatType != 0 && m_formatType != OfficeFormatType.General) || m_bUseSystemDateFormat)
		{
			return value;
		}
		int count = m_arrTokens.Count;
		string result = string.Empty;
		if (count > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = count - 1; num >= 0; num--)
			{
				string value2 = m_arrTokens[num].ApplyFormat(value, bShowReservedSymbols);
				stringBuilder.Insert(0, value2);
			}
			result = stringBuilder.ToString();
		}
		else if (count == 1)
		{
			result = m_arrTokens[0].ApplyFormat(value, bShowReservedSymbols);
		}
		return result;
	}

	private void AssignPosition(ref int iToAssign, int iCurrentPos)
	{
		if (iToAssign >= 0)
		{
			throw new FormatException();
		}
		iToAssign = iCurrentPos;
	}

	private string ApplyFormat(double value, bool bShowReservedSymbols, int iStartToken, int iEndToken, bool bForward)
	{
		return ApplyFormat(value, bShowReservedSymbols, iStartToken, iEndToken, bForward, bGroupDigits: false, bAddNegativeSign: false);
	}

	private string ApplyFormat(double value, bool bShowReservedSymbols, int iStartToken, int iEndToken, bool bForward, bool bGroupDigits, bool bAddNegativeSign)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = (bForward ? 1 : (-1));
		int num2 = (bForward ? iStartToken : iEndToken);
		int iEndToken2 = (bForward ? iEndToken : iStartToken);
		int iDigitCounter = 0;
		CultureInfo culture = Culture;
		int num3 = -1;
		double num4 = value;
		for (int i = num2; CheckCondition(iEndToken2, bForward, i); i += num)
		{
			FormatTokenBase formatTokenBase = m_arrTokens[i];
			if (formatTokenBase is DigitToken digitToken)
			{
				digitToken.OriginalValue = num4;
				iDigitCounter = ApplyDigit(digitToken, i, num2, ref value, iDigitCounter, stringBuilder, bForward, bShowReservedSymbols, bGroupDigits);
				if (bForward)
				{
					if (bAddNegativeSign)
					{
						AddToBuilder(stringBuilder, bForward, "-");
						bAddNegativeSign = false;
					}
				}
				else
				{
					num3 = stringBuilder.Length;
				}
			}
			else
			{
				double value2 = num4;
				string text = formatTokenBase.ApplyFormat(ref value2, bShowReservedSymbols, culture, this);
				if (text != null)
				{
					AddToBuilder(stringBuilder, bForward, text);
				}
			}
		}
		if (num3 >= 0 && bAddNegativeSign)
		{
			stringBuilder.Insert(stringBuilder.Length - num3, "-");
		}
		return stringBuilder.ToString();
	}

	private int ApplyDigit(DigitToken digit, int iIndex, int iStart, ref double value, int iDigitCounter, StringBuilder builder, bool bForward, bool bShowHiddenSymbols, bool bGroupDigits)
	{
		if (digit == null)
		{
			throw new ArgumentNullException("digit");
		}
		CultureInfo culture = Culture;
		if (digit.IsLastDigit)
		{
			bool flag = value < 0.0;
			value = Math.Abs(value);
			do
			{
				string strTokenResult = digit.ApplyFormat(ref value, bShowHiddenSymbols, culture, this);
				iDigitCounter = ApplySingleDigit(iIndex, iStart, iDigitCounter, strTokenResult, builder, bForward, bGroupDigits);
			}
			while (value >= 1.0);
			if (flag)
			{
				value = 0.0 - value;
			}
		}
		else
		{
			string strTokenResult = digit.ApplyFormat(ref value, bShowHiddenSymbols, culture, this);
			iDigitCounter = ApplySingleDigit(iIndex, iStart, iDigitCounter, strTokenResult, builder, bForward, bGroupDigits);
		}
		return iDigitCounter;
	}

	private int ApplySingleDigit(int iIndex, int iStart, int iDigitCounter, string strTokenResult, StringBuilder builder, bool bForward, bool bGroupDigits)
	{
		if (strTokenResult == null)
		{
			throw new ArgumentNullException("strTokenResult");
		}
		iDigitCounter++;
		if (bGroupDigits && strTokenResult.Length > 0 && iIndex != iStart && iDigitCounter == 4)
		{
			AddToBuilder(builder, bForward, CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
			iDigitCounter = 1;
		}
		AddToBuilder(builder, bForward, strTokenResult);
		return iDigitCounter;
	}

	private void AddToBuilder(StringBuilder builder, bool bForward, string strValue)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		if (strValue == null)
		{
			throw new ArgumentNullException("strValue");
		}
		if (bForward)
		{
			builder.Append(strValue);
		}
		else
		{
			builder.Insert(0, strValue);
		}
	}

	private bool CheckCondition(int iEndToken, bool bForward, int iPos)
	{
		if (!bForward)
		{
			return iPos >= iEndToken;
		}
		return iPos <= iEndToken;
	}

	private int LocateLastFractionDigit()
	{
		int result = -1;
		int count = Count;
		for (int num = ((m_iScientificPos > 0) ? (m_iScientificPos - 1) : (count - 1)); num >= 0; num--)
		{
			if (this[num] is DigitToken)
			{
				_ = m_iDecimalPointPos;
				result = num;
				break;
			}
		}
		if (m_iScientificPos > 0)
		{
			for (int i = m_iScientificPos; i < count; i++)
			{
				if (this[i] is DigitToken digitToken)
				{
					digitToken.IsLastDigit = true;
					break;
				}
			}
		}
		return result;
	}

	private bool LocateLastGroups(int iStartIndex)
	{
		iStartIndex = Math.Max(m_iDecimalPointPos, iStartIndex);
		iStartIndex = Math.Max(0, iStartIndex);
		int count = Count;
		if (iStartIndex >= count)
		{
			return false;
		}
		ThousandsSeparatorToken thousandsSeparatorToken = m_arrTokens[iStartIndex] as ThousandsSeparatorToken;
		bool result = thousandsSeparatorToken != null;
		while (thousandsSeparatorToken != null)
		{
			thousandsSeparatorToken.IsAfterDigits = true;
			iStartIndex++;
			if (iStartIndex >= count)
			{
				break;
			}
			thousandsSeparatorToken = m_arrTokens[iStartIndex] as ThousandsSeparatorToken;
		}
		return result;
	}

	private void ApplyLastGroups(ref double value, bool bShowReservedSymbols)
	{
		if (m_bLastGroups)
		{
			int i = m_iLastDigit + 1;
			for (int count = Count; i < count && m_arrTokens[i] is ThousandsSeparatorToken thousandsSeparatorToken; i++)
			{
				value = thousandsSeparatorToken.PreprocessValue(value);
			}
		}
	}

	private void PrepareValue(ref double value, bool bShowReservedSymbols)
	{
		ApplyLastGroups(ref value, bShowReservedSymbols);
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (this[i].TokenType == TokenType.Percent)
			{
				value *= 100.0;
			}
		}
	}

	private int CalculateFractionDigits()
	{
		int result = 0;
		if (m_iDecimalPointPos > 0)
		{
			return CalculateDigits(m_iDecimalPointPos, m_iLastDigit);
		}
		return result;
	}

	private int CalculateIntegerDigits()
	{
		int iEndIndex = Count - 1;
		if (m_iDecimalPointPos > 0)
		{
			iEndIndex = m_iDecimalPointPos;
		}
		else if (m_iScientificPos > 0)
		{
			iEndIndex = m_iScientificPos;
		}
		return CalculateDigits(0, iEndIndex);
	}

	private int CalculateDigits(int iStartIndex, int iEndIndex)
	{
		int count = Count;
		if (iStartIndex < 0 || iStartIndex > count)
		{
			throw new ArgumentOutOfRangeException("iStartIndex", "Value cannot be less than 0 and greater than iCount.");
		}
		if (iEndIndex < 0 || iEndIndex > count)
		{
			throw new ArgumentOutOfRangeException("iEndIndex", "Value cannot be less than 0 and greater than iCount.");
		}
		int num = 0;
		for (int i = iStartIndex; i <= iEndIndex; i++)
		{
			if (m_arrTokens[i] is DigitToken)
			{
				num++;
			}
		}
		return num;
	}

	private void LocateFractionParts()
	{
		if (!m_bFraction)
		{
			return;
		}
		m_iFractionStart = GetDigitGroupStart(m_iFractionPos, bForward: false);
		m_iFractionEnd = GetDigitGroupStart(m_iFractionPos, bForward: true);
		m_iDenumaratorLen = m_iFractionEnd - GetDigitGroupStart(m_iFractionEnd, bForward: false) + 1;
		if (FormatType == OfficeFormatType.Number)
		{
			if (m_iFractionStart < 0)
			{
				throw new ArgumentException("Can't locate fraction digits");
			}
			if (m_arrTokens[m_iFractionStart] is DigitToken)
			{
				((DigitToken)m_arrTokens[m_iFractionStart]).IsLastDigit = true;
			}
		}
	}

	private int GetDigitGroupStart(int iStartPos, bool bForward)
	{
		int num = (bForward ? 1 : (-1));
		int i = iStartPos;
		int count = Count;
		bool flag = false;
		for (; i >= 0 && i < count; i += num)
		{
			if (m_arrTokens[i] is DigitToken || (m_arrTokens[i] is UnknownToken && m_arrTokens[i].Format != " "))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return -1;
		}
		for (i += num; i >= 0 && i < count && (m_arrTokens[i] is DigitToken || (m_arrTokens[i] is UnknownToken && m_arrTokens[i].Format != " ")); i += num)
		{
		}
		return i - num;
	}

	private bool IsAnyDigit(int iStartIndex, int iEndIndex)
	{
		int count = Count;
		iStartIndex = Math.Max(iStartIndex, 0);
		iEndIndex = Math.Min(iEndIndex, count - 1);
		for (int i = iStartIndex; i < iEndIndex; i++)
		{
			if (m_arrTokens[i] is DigitToken)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckCondition(double value)
	{
		if (HasCondition)
		{
			return m_condition.CheckCondition(value);
		}
		return false;
	}

	private void DetectFormatType()
	{
		m_formatType = OfficeFormatType.Unknown;
		int i = 0;
		for (int num = DEF_POSSIBLE_TOKENS.Length; i < num; i += 2)
		{
			TokenType[] arrPossibleTokens = (TokenType[])DEF_POSSIBLE_TOKENS[i];
			OfficeFormatType officeFormatType = (OfficeFormatType)DEF_POSSIBLE_TOKENS[i + 1];
			if ((officeFormatType != OfficeFormatType.Number || !m_bMultiplePoints) && CheckTokenTypes(arrPossibleTokens))
			{
				m_formatType = officeFormatType;
				break;
			}
		}
	}

	private bool CheckTokenTypes(TokenType[] arrPossibleTokens)
	{
		if (arrPossibleTokens == null)
		{
			throw new ArgumentNullException("arrPossibleTokens");
		}
		int count = Count;
		if (count == 0 && arrPossibleTokens.Length == 0)
		{
			return true;
		}
		int i = 0;
		for (int num = count; i < num; i++)
		{
			FormatTokenBase formatTokenBase = this[i];
			if (!ContainsIn(arrPossibleTokens, formatTokenBase.TokenType))
			{
				return false;
			}
		}
		return true;
	}

	private void CheckMinuteToken(int iTokenIndex)
	{
		FormatTokenBase formatTokenBase = this[iTokenIndex];
		if (formatTokenBase.TokenType != TokenType.Minute)
		{
			throw new ArgumentException("Wrong token type.");
		}
		if (FindTimeToken(iTokenIndex - 1, DEF_BREAK_HOUR, false, TokenType.Hour, TokenType.Hour24) == -1 && FindTimeToken(iTokenIndex + 1, DEF_BREAK_SECOND, true, TokenType.Second, TokenType.SecondTotal) == -1)
		{
			MonthToken monthToken = new MonthToken();
			monthToken.Format = formatTokenBase.Format;
			m_arrTokens[iTokenIndex] = monthToken;
		}
	}

	private int FindTimeToken(int iTokenIndex, TokenType[] arrBreakTypes, bool bForward, params TokenType[] arrTypes)
	{
		int count = Count;
		int num = (bForward ? 1 : (-1));
		while (iTokenIndex >= 0 && iTokenIndex < count)
		{
			TokenType tokenType = this[iTokenIndex].TokenType;
			if (Array.IndexOf(arrBreakTypes, tokenType) != -1)
			{
				break;
			}
			if (Array.IndexOf(arrTypes, tokenType) != -1)
			{
				return iTokenIndex;
			}
			iTokenIndex += num;
		}
		return -1;
	}

	private void SetRoundSeconds()
	{
		bool flag = true;
		int num = Count;
		for (int i = 0; i < num; i++)
		{
			if (this[i].TokenType == TokenType.DecimalPoint)
			{
				int num2 = i;
				string text = string.Empty;
				for (i++; i < num && Array.IndexOf(DEF_MILLISECONDTOKENS, this[i].TokenType) != -1; i++)
				{
					text += this[i].Format;
				}
				if (i != num2 + 1)
				{
					MilliSecondToken milliSecondToken = new MilliSecondToken();
					milliSecondToken.Format = text;
					int num3 = i - num2;
					m_arrTokens.RemoveRange(num2, num3);
					m_arrTokens.Insert(num2, milliSecondToken);
					num -= num3 - 1;
					flag = false;
				}
			}
		}
		if (flag)
		{
			return;
		}
		for (int j = 0; j < num; j++)
		{
			FormatTokenBase formatTokenBase = this[j];
			if (formatTokenBase.TokenType == TokenType.Second)
			{
				((SecondToken)formatTokenBase).RoundValue = false;
			}
		}
	}

	private static bool ContainsIn(TokenType[] arrPossibleTokens, TokenType token)
	{
		if (arrPossibleTokens == null)
		{
			throw new ArgumentNullException("arrPossibleTokens");
		}
		int num = 0;
		int num2 = arrPossibleTokens.Length - 1;
		while (num2 != num)
		{
			int num3 = (num2 + num) / 2;
			TokenType tokenType = arrPossibleTokens[num3];
			if (tokenType >= token)
			{
				if (num2 == num3)
				{
					break;
				}
				num2 = num3;
			}
			else if (tokenType < token)
			{
				if (num == num3)
				{
					break;
				}
				num = num3;
			}
		}
		if (arrPossibleTokens[num] != token)
		{
			return arrPossibleTokens[num2] == token;
		}
		return true;
	}

	private static double SplitValue(double value, out double dFraction)
	{
		bool flag = value > 0.0;
		int length = value.ToString().Length;
		dFraction = Math.Abs(value - (flag ? Math.Floor(value) : Math.Ceiling(value)));
		double num = Math.Abs(value) - dFraction;
		int num2 = length - num.ToString().Length + 1;
		if (num2 < length && num2 < 15)
		{
			dFraction = Math.Round(dFraction, num2);
		}
		if (!flag)
		{
			return 0.0 - num;
		}
		return num;
	}

	internal static double Round(double value)
	{
		bool num = value >= 0.0;
		double num2 = (num ? Math.Floor(value) : Math.Ceiling(value));
		int num3 = Math.Sign(value);
		if ((num ? (value - num2) : (num2 - value)) >= 0.49999999999999994)
		{
			num2 += (double)num3;
		}
		return num2;
	}

	public object Clone(object parent)
	{
		FormatSection formatSection = (FormatSection)MemberwiseClone();
		formatSection.SetParent(parent);
		formatSection.m_arrTokens = new List<FormatTokenBase>(m_arrTokens.Count);
		int i = 0;
		for (int count = m_arrTokens.Count; i < count; i++)
		{
			FormatTokenBase formatTokenBase = m_arrTokens[i];
			formatTokenBase = (FormatTokenBase)formatTokenBase.Clone();
			formatSection.m_arrTokens.Add(formatTokenBase);
			switch (formatTokenBase.TokenType)
			{
			case TokenType.Condition:
				formatSection.m_condition = (ConditionToken)formatTokenBase;
				break;
			case TokenType.Culture:
				formatSection.m_culture = (CultureToken)formatTokenBase;
				break;
			}
		}
		return formatSection;
	}

	internal void Clear()
	{
		m_arrTokens.Clear();
		m_arrTokens = null;
		Dispose();
	}
}
