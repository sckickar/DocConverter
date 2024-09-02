using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class TokenAttribute : Attribute
{
	private FormulaToken m_Code;

	private string m_strOperationSymbol = string.Empty;

	private bool m_bPlaceAfter;

	public FormulaToken FormulaType => m_Code;

	public string OperationSymbol => m_strOperationSymbol;

	public bool IsPlaceAfter => m_bPlaceAfter;

	private TokenAttribute()
	{
	}

	public TokenAttribute(FormulaToken Code)
	{
		m_Code = Code;
	}

	public TokenAttribute(FormulaToken Code, string OperationSymbol)
	{
		m_Code = Code;
		m_strOperationSymbol = OperationSymbol;
	}

	public TokenAttribute(FormulaToken Code, string OperationSymbol, bool bPlaceAfter)
	{
		m_Code = Code;
		m_strOperationSymbol = OperationSymbol;
		m_bPlaceAfter = bPlaceAfter;
	}
}
