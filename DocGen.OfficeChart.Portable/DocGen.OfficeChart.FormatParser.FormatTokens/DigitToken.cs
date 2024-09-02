using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal abstract class DigitToken : SingleCharToken
{
	private bool m_bLastDigit;

	private bool m_bCenterDigit;

	private double m_originalValue;

	public bool IsLastDigit
	{
		get
		{
			return m_bLastDigit;
		}
		set
		{
			m_bLastDigit = value;
		}
	}

	public bool IsCenterDigit
	{
		get
		{
			return m_bCenterDigit;
		}
		internal set
		{
			m_bCenterDigit = value;
		}
	}

	internal double OriginalValue
	{
		get
		{
			return m_originalValue;
		}
		set
		{
			m_originalValue = value;
		}
	}

	public DigitToken()
	{
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		int num = GetDigit(ref value);
		GetIndexOfZero(OriginalValue, num);
		if (num == 255)
		{
			num = 0;
		}
		return GetDigitString(value, num, bShowHiddenSymbols);
	}

	private void GetIndexOfZero(double value, int iDigit)
	{
		if (iDigit == 0)
		{
			int num = value.ToString().IndexOf(iDigit.ToString());
			int lastIndex = value.ToString().LastIndexOf(iDigit.ToString());
			if (num != value.ToString().Length - 1 && num != 0 && num != -1 && CheckIsZeroes(num, lastIndex, value.ToString()))
			{
				m_bCenterDigit = true;
			}
		}
	}

	private bool CheckIsZeroes(int startIndex, int lastIndex, string p)
	{
		bool result = true;
		if (startIndex == lastIndex)
		{
			return result;
		}
		for (int i = startIndex; i <= lastIndex; i++)
		{
			if (p[i] == '0')
			{
				result = false;
				continue;
			}
			result = true;
			break;
		}
		return result;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return value;
	}

	protected internal int GetDigit(ref double value)
	{
		value = FormatSection.Round(value);
		int num = (int)(value % 10.0);
		value /= 10.0;
		if (num == 0 && value == 0.0)
		{
			return 255;
		}
		int num2 = Math.Sign(value);
		value = ((value > 0.0) ? Math.Floor(value) : Math.Ceiling(value));
		value += (double)num2 * 0.1;
		return num;
	}

	protected internal virtual string GetDigitString(double value, int iDigit, bool bShowHiddenSymbols)
	{
		iDigit = Math.Abs(iDigit);
		if (iDigit > 9)
		{
			throw new ArgumentOutOfRangeException("iDigit", "Value cannot be less than -9 and greater than than 9.");
		}
		return iDigit.ToString();
	}
}
