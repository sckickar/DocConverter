using System;
using System.Collections.Generic;
using System.Text;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tFunctionVar1)]
[Token(FormulaToken.tFunctionVar2)]
[Token(FormulaToken.tFunctionVar3)]
[CLSCompliant(false)]
internal class FunctionVarPtg : FunctionPtg
{
	[Preserve]
	public FunctionVarPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public FunctionVarPtg(ExcelFunction funcIndex)
		: base(funcIndex)
	{
		TokenCode = FormulaToken.tFunctionVar2;
	}

	[Preserve]
	public FunctionVarPtg(string strFunctionName)
		: base(strFunctionName)
	{
		TokenCode = FormulaToken.tFunctionVar2;
	}

	[Preserve]
	public FunctionVarPtg()
	{
		TokenCode = FormulaToken.tFunctionVar2;
	}

	public override int GetSize(OfficeVersion version)
	{
		return base.GetSize(version) + 1;
	}

	public override string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser)
	{
		string[] operands = GetOperands(strFormula, ref index, checkParamCount: false, formulaParser);
		if (base.FunctionIndex != ExcelFunction.CustomFunction)
		{
			base.NumberOfArguments = (byte)operands.Length;
		}
		else
		{
			base.NumberOfArguments = (byte)(operands.Length + 1);
		}
		return operands;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		array[1] = base.NumberOfArguments;
		BitConverter.GetBytes((ushort)base.FunctionIndex).CopyTo(array, 2);
		return array;
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		if (operands == null)
		{
			throw new ArgumentNullException("operands");
		}
		if (operands.Count < base.NumberOfArguments)
		{
			throw new ArgumentException("Not enough elements in stack");
		}
		if (base.FunctionIndex == ExcelFunction.CustomFunction)
		{
			string operandsSeparator = formulaUtil.OperandsSeparator;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			int i = 1;
			for (int num = base.NumberOfArguments - 1; i <= num; i++)
			{
				string value = Convert.ToString(operands.Pop());
				stringBuilder.Insert(1, value);
				if (i != num)
				{
					stringBuilder.Insert(1, operandsSeparator);
				}
			}
			stringBuilder.Append(")");
			string text = (string)operands.Pop();
			int length = text.Length;
			if (text[length - 1] == '\'')
			{
				int num2 = text.LastIndexOf('\'', length - 2);
				if (num2 >= 0)
				{
					text = text.Substring(num2 + 1, length - num2 - 2);
				}
			}
			stringBuilder.Insert(0, text);
			string item = stringBuilder.ToString();
			operands.Push(item);
		}
		else
		{
			base.PushResultToStack(formulaUtil, operands, isForSerialization);
		}
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return Ptg.IndexToCode(FormulaToken.tFunctionVar1, index);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		base.NumberOfArguments = provider.ReadByte(offset++);
		base.FunctionIndex = (ExcelFunction)provider.ReadUInt16(offset);
		offset += 2;
	}
}
