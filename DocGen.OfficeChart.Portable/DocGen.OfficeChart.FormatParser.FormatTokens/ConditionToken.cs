using System;
using System.Globalization;

namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class ConditionToken : InBracketToken
{
	private enum CompareOperation
	{
		None,
		Equal,
		NotEqual,
		GreaterEqual,
		LessEqual,
		Less,
		Greater
	}

	private static readonly string[] CompareOperationStrings = new string[6] { "=", "<>", ">=", "<=", "<", ">" };

	private double m_dCompareNumber;

	private CompareOperation m_operation;

	public override TokenType TokenType => TokenType.Condition;

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
		int num = FindString(CompareOperationStrings, strFormat, iIndex, bIgnoreCase: false);
		if (num < 0)
		{
			return iStartIndex;
		}
		string text = CompareOperationStrings[num];
		iIndex += text.Length;
		m_operation = (CompareOperation)(num + 1);
		if (double.TryParse(strFormat.Substring(iIndex, iEndIndex - iIndex), NumberStyles.Any, null, out var result))
		{
			m_dCompareNumber = result;
			return iEndIndex + 1;
		}
		return iStartIndex;
	}

	public override string ApplyFormat(ref double value, bool bShowHiddenSymbols, CultureInfo culture, FormatSection section)
	{
		return string.Empty;
	}

	public override string ApplyFormat(string value, bool bShowHiddenSymbols)
	{
		return string.Empty;
	}

	public bool CheckCondition(double value)
	{
		return m_operation switch
		{
			CompareOperation.Equal => value == m_dCompareNumber, 
			CompareOperation.Greater => value > m_dCompareNumber, 
			CompareOperation.GreaterEqual => value >= m_dCompareNumber, 
			CompareOperation.Less => value < m_dCompareNumber, 
			CompareOperation.LessEqual => value <= m_dCompareNumber, 
			CompareOperation.NotEqual => value != m_dCompareNumber, 
			_ => throw new ArgumentOutOfRangeException("Compare operation"), 
		};
	}
}
