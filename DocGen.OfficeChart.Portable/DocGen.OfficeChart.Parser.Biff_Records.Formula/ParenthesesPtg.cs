using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tParentheses, "(")]
internal class ParenthesesPtg : UnaryOperationPtg
{
	private static readonly TokenAttribute[] s_arrAttributes;

	protected override TokenAttribute[] Attributes => s_arrAttributes;

	[Preserve]
	static ParenthesesPtg()
	{
		s_arrAttributes = new TokenAttribute[1]
		{
			new TokenAttribute(FormulaToken.tParentheses, "(")
		};
	}

	[Preserve]
	public ParenthesesPtg()
		: base("(")
	{
		TokenCode = FormulaToken.tParentheses;
	}

	[Preserve]
	public ParenthesesPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public ParenthesesPtg(string strFormula)
		: base("(")
	{
		if (strFormula != "(")
		{
			throw new ArgumentOutOfRangeException("strFormula");
		}
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "()";
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		object obj = operands.Pop();
		object obj2 = obj as AttrPtg;
		if (obj2 != null)
		{
			obj = operands.Pop();
		}
		else
		{
			obj2 = string.Empty;
		}
		string item = obj2.ToString() + "(" + obj.ToString() + ")";
		operands.Push(item);
	}

	public override string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser)
	{
		int num = FormulaUtil.FindCorrespondingBracket(strFormula, index);
		index = num + 1;
		string text = strFormula.Substring(1, num - 1);
		return new string[1] { text };
	}

	public override OfficeParseFormulaOptions UpdateParseOptions(OfficeParseFormulaOptions options)
	{
		return options;
	}
}
