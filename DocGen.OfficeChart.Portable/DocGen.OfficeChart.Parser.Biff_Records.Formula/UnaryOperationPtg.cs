using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tUnaryMinus, "-")]
[Token(FormulaToken.tUnaryPlus, "+")]
[Token(FormulaToken.tPercent, "%", true)]
internal class UnaryOperationPtg : OperationPtg
{
	private static readonly TokenAttribute[] s_arrAttributes;

	private static readonly Dictionary<string, TokenAttribute> NameToAttribute;

	public override TOperation OperationType => TOperation.TYPE_UNARY;

	protected override TokenAttribute[] Attributes => s_arrAttributes;

	[Preserve]
	static UnaryOperationPtg()
	{
		NameToAttribute = new Dictionary<string, TokenAttribute>(3);
		s_arrAttributes = new TokenAttribute[3]
		{
			new TokenAttribute(FormulaToken.tUnaryMinus, "-"),
			new TokenAttribute(FormulaToken.tUnaryPlus, "+"),
			new TokenAttribute(FormulaToken.tPercent, "%", bPlaceAfter: true)
		};
		int i = 0;
		for (int num = s_arrAttributes.Length; i < num; i++)
		{
			TokenAttribute tokenAttribute = s_arrAttributes[i];
			NameToAttribute.Add(tokenAttribute.OperationSymbol, tokenAttribute);
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
		return NameToAttribute[operationSign].FormulaType;
	}

	[Preserve]
	public UnaryOperationPtg()
	{
	}

	[Preserve]
	public UnaryOperationPtg(string strOperationSymbol)
	{
		if (!NameToAttribute.TryGetValue(strOperationSymbol, out var value))
		{
			TokenAttribute[] attributes = Attributes;
			if (attributes == null)
			{
				throw new ArgumentNullException("Unknown operation");
			}
			int num = attributes.Length;
			int i;
			for (i = 0; i < num; i++)
			{
				value = attributes[i];
				if (value.OperationSymbol == strOperationSymbol)
				{
					break;
				}
			}
			if (i == num)
			{
				throw new ArgumentNullException("Unknown operation.");
			}
		}
		base.OperationSymbol = strOperationSymbol;
		TokenCode = value.FormulaType;
		base.IsPlaceAfter = value.IsPlaceAfter;
	}

	[Preserve]
	public UnaryOperationPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return 1;
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		if (operands == null)
		{
			throw new ArgumentNullException("operands");
		}
		FormulaUtil.PushOperandToStack(operands, ToString());
		string text = (string)operands.Pop();
		string text2 = (string)operands.Pop();
		if (base.IsPlaceAfter)
		{
			operands.Push(text2 + text);
		}
		else
		{
			operands.Push(text + text2);
		}
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return base.OperationSymbol;
	}

	public override string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser)
	{
		string[] array = new string[1] { formulaParser.GetRightUnaryOperand(strFormula, index) };
		index += array[0].Length + ToString().Length;
		return array;
	}
}
