using System;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class InsignificantDigitToken : DigitToken
{
	private const char DEF_FORMAT_CHAR = '#';

	private bool m_bHideIfZero;

	public override TokenType TokenType => TokenType.InsignificantDigit;

	public override char FormatChar => '#';

	public bool HideIfZero
	{
		get
		{
			return m_bHideIfZero;
		}
		set
		{
			m_bHideIfZero = value;
		}
	}

	protected internal override string GetDigitString(double value, int iDigit, bool bShowHiddenSymbols)
	{
		if (iDigit != 0 || (!(Math.Abs(value) < 1.0) && !HideIfZero))
		{
			return base.GetDigitString(value, iDigit, bShowHiddenSymbols);
		}
		if (base.IsCenterDigit)
		{
			return base.GetDigitString(value, iDigit, bShowHiddenSymbols);
		}
		return string.Empty;
	}
}
