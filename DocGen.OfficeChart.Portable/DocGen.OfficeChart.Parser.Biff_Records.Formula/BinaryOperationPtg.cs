using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tAdd, "+")]
[Token(FormulaToken.tDiv, "/")]
[Token(FormulaToken.tMul, "*")]
[Token(FormulaToken.tSub, "-")]
[Token(FormulaToken.tPower, "^")]
[Token(FormulaToken.tConcat, "&")]
[Token(FormulaToken.tLessThan, "<")]
[Token(FormulaToken.tLessEqual, "<=")]
[Token(FormulaToken.tEqual, "=")]
[Token(FormulaToken.tNotEqual, "<>")]
[Token(FormulaToken.tGreater, ">")]
[Token(FormulaToken.tGreaterEqual, ">=")]
[Token(FormulaToken.tCellRangeIntersection, " ")]
[Token(FormulaToken.tCellRange, ":")]
internal class BinaryOperationPtg : OperationPtg
{
	private static readonly Dictionary<string, FormulaToken> NameToId;

	private static readonly Dictionary<FormulaToken, string> IdToName;

	private static readonly TokenAttribute[] s_arrAttributes;

	public override int NumberOfOperands => 2;

	public override TOperation OperationType => TOperation.TYPE_BINARY;

	protected override TokenAttribute[] Attributes => s_arrAttributes;

	[Preserve]
	static BinaryOperationPtg()
	{
		NameToId = new Dictionary<string, FormulaToken>(16);
		IdToName = new Dictionary<FormulaToken, string>(16);
		s_arrAttributes = new TokenAttribute[14]
		{
			new TokenAttribute(FormulaToken.tAdd, "+"),
			new TokenAttribute(FormulaToken.tDiv, "/"),
			new TokenAttribute(FormulaToken.tMul, "*"),
			new TokenAttribute(FormulaToken.tSub, "-"),
			new TokenAttribute(FormulaToken.tPower, "^"),
			new TokenAttribute(FormulaToken.tConcat, "&"),
			new TokenAttribute(FormulaToken.tLessThan, "<"),
			new TokenAttribute(FormulaToken.tLessEqual, "<="),
			new TokenAttribute(FormulaToken.tEqual, "="),
			new TokenAttribute(FormulaToken.tNotEqual, "<>"),
			new TokenAttribute(FormulaToken.tGreater, ">"),
			new TokenAttribute(FormulaToken.tGreaterEqual, ">="),
			new TokenAttribute(FormulaToken.tCellRangeIntersection, " "),
			new TokenAttribute(FormulaToken.tCellRange, ":")
		};
		int i = 0;
		for (int num = s_arrAttributes.Length; i < num; i++)
		{
			TokenAttribute obj = s_arrAttributes[i];
			FormulaToken formulaType = obj.FormulaType;
			string operationSymbol = obj.OperationSymbol;
			NameToId.Add(operationSymbol, formulaType);
			IdToName.Add(formulaType, operationSymbol);
		}
	}

	public static FormulaToken GetTokenId(string operationSign)
	{
		if (operationSign == null)
		{
			throw new ArgumentNullException("operationSign");
		}
		if (operationSign.Length == 0)
		{
			throw new ArgumentException("operationSign - string cannot be empty");
		}
		return NameToId[operationSign];
	}

	public static string GetTokenString(FormulaToken token)
	{
		return IdToName[token];
	}

	[Preserve]
	public BinaryOperationPtg()
	{
	}

	[Preserve]
	public BinaryOperationPtg(string operation)
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		if (operation.Length == 0)
		{
			throw new ArgumentException("operation - string cannot be empty");
		}
		if (!NameToId.ContainsKey(operation))
		{
			throw new ArgumentException("operation", "Unknown operation symbol");
		}
		base.OperationSymbol = operation;
		TokenCode = GetTokenId(operation);
	}

	[Preserve]
	public BinaryOperationPtg(FormulaToken operation)
	{
		if (!IdToName.ContainsKey(operation))
		{
			throw new ArgumentException("operation", "Unknown operation symbol");
		}
		base.OperationSymbol = GetTokenString(operation);
		TokenCode = operation;
	}

	[Preserve]
	public BinaryOperationPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		FormulaUtil.PushOperandToStack(operands, ToString(formulaUtil));
		string text = (string)operands.Pop();
		string text2 = (string)operands.Pop();
		string text3 = (string)operands.Pop();
		operands.Push(text3 + text + text2);
	}

	public override string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser)
	{
		index += base.OperationSymbol.Length;
		string rightBinaryOperand = formulaParser.GetRightBinaryOperand(strFormula, index, ToString());
		index += rightBinaryOperand.Length;
		return new string[1] { rightBinaryOperand };
	}

	public override int GetSize(OfficeVersion version)
	{
		return 1;
	}
}
