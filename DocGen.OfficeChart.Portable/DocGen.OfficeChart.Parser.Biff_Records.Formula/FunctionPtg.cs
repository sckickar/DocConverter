using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tFunction1)]
[Token(FormulaToken.tFunction2)]
[Token(FormulaToken.tFunction3)]
[CLSCompliant(false)]
internal class FunctionPtg : OperationPtg
{
	private ExcelFunction m_FunctionIndex = ExcelFunction.NONE;

	private byte m_ArgumentsNumber;

	public const string OperandsDelimiter = ",";

	public ExcelFunction FunctionIndex
	{
		get
		{
			return m_FunctionIndex;
		}
		set
		{
			m_FunctionIndex = value;
		}
	}

	public byte NumberOfArguments
	{
		get
		{
			return m_ArgumentsNumber;
		}
		set
		{
			m_ArgumentsNumber = value;
		}
	}

	public override TOperation OperationType => TOperation.TYPE_FUNCTION;

	protected override TokenAttribute[] Attributes => null;

	[Preserve]
	public FunctionPtg()
	{
		TokenCode = FormulaToken.tFunction2;
	}

	[Preserve]
	public FunctionPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public FunctionPtg(ExcelFunction index)
	{
		m_FunctionIndex = index;
		TokenCode = FormulaToken.tFunction2;
		if (FormulaUtil.FunctionIdToParamCount.TryGetValue(index, out var value))
		{
			m_ArgumentsNumber = (byte)value;
		}
	}

	[Preserve]
	public FunctionPtg(string strFunctionName)
		: this(FormulaUtil.FunctionAliasToId[strFunctionName])
	{
	}

	protected string[] GetOperands(string strFormula, ref int index, bool checkParamCount, FormulaUtil formulaParser)
	{
		List<string> list = new List<string>();
		int num = 0;
		strFormula = strFormula.Substring(index + 1, strFormula.Length - index - 2);
		index = -1;
		if (strFormula.Length > 0)
		{
			while (index < strFormula.Length)
			{
				string functionOperand = formulaParser.GetFunctionOperand(strFormula, index);
				list.Add(functionOperand);
				index += functionOperand.Length + 1;
				num++;
			}
		}
		if (checkParamCount && num != m_ArgumentsNumber)
		{
			throw new ArgumentException("Too many or not enough arguments.");
		}
		return list.ToArray();
	}

	public static FormulaToken IndexToCode(int index)
	{
		return Ptg.IndexToCode(FormulaToken.tFunction1, index);
	}

	public override int GetSize(OfficeVersion version)
	{
		return 3;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		if (!FormulaUtil.FunctionIdToAlias.TryGetValue(m_FunctionIndex, out var value))
		{
			Excel2007Function functionIndex = (Excel2007Function)m_FunctionIndex;
			value = functionIndex.ToString();
		}
		if (isForSerialization && (FormulaUtil.IsExcel2010Function(m_FunctionIndex) || FormulaUtil.IsExcel2013Function(m_FunctionIndex)))
		{
			value = "_xlfn." + value;
		}
		return value;
	}

	public override void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization)
	{
		if (operands.Count < m_ArgumentsNumber)
		{
			throw new ArgumentOutOfRangeException("Not enough arguments.");
		}
		string operand = ToString(formulaUtil, 0, 0, bR1C1: false, null, isForSerialization);
		FormulaUtil.PushOperandToStack(operands, operand);
		operand = (string)operands.Pop();
		string text = ((m_ArgumentsNumber > 0) ? operands.Pop().ToString() : string.Empty);
		string operandsSeparator = formulaUtil.OperandsSeparator;
		for (int i = 1; i < m_ArgumentsNumber; i++)
		{
			text = operands.Pop().ToString() + operandsSeparator + text;
		}
		string operand2 = operand + "(" + text + ")";
		FormulaUtil.PushOperandToStack(operands, operand2);
	}

	public override string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser)
	{
		return GetOperands(strFormula, ref index, checkParamCount: true, formulaParser);
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		array[0] = (byte)TokenCode;
		BitConverter.GetBytes((ushort)m_FunctionIndex).CopyTo(array, 1);
		return array;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_FunctionIndex = (ExcelFunction)provider.ReadUInt16(offset);
		if (!FormulaUtil.FunctionIdToAlias.TryGetValue(m_FunctionIndex, out var _))
		{
			throw new ArgumentNullException("Unknown function");
		}
		if (FormulaUtil.FunctionIdToParamCount.TryGetValue(m_FunctionIndex, out var value2))
		{
			m_ArgumentsNumber = (byte)value2;
		}
		offset += 2;
	}
}
