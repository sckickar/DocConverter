using System.Collections.Generic;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
internal abstract class OperationPtg : Ptg
{
	private const string DEFAULT_ARGUMENTS_SEPARATOR = ",";

	private string m_strOperationSymbol = string.Empty;

	private bool m_bPlaceAfter;

	public override bool IsOperation => true;

	public abstract TOperation OperationType { get; }

	public virtual int NumberOfOperands => OperationType switch
	{
		TOperation.TYPE_BINARY => 1, 
		TOperation.TYPE_UNARY => 2, 
		_ => 0, 
	};

	public string OperationSymbol
	{
		get
		{
			return m_strOperationSymbol;
		}
		set
		{
			m_strOperationSymbol = value;
		}
	}

	public bool IsPlaceAfter
	{
		get
		{
			return m_bPlaceAfter;
		}
		set
		{
			m_bPlaceAfter = value;
		}
	}

	protected abstract TokenAttribute[] Attributes { get; }

	[Preserve]
	public OperationPtg()
	{
	}

	[Preserve]
	protected OperationPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public virtual void PushResultToStack(Stack<object> operands)
	{
		PushResultToStack(null, operands, isForSerialization: false);
	}

	public abstract void PushResultToStack(FormulaUtil formulaUtil, Stack<object> operands, bool isForSerialization);

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return m_strOperationSymbol;
	}

	public abstract string[] GetOperands(string strFormula, ref int index, FormulaUtil formulaParser);

	public virtual OfficeParseFormulaOptions UpdateParseOptions(OfficeParseFormulaOptions options)
	{
		return options | OfficeParseFormulaOptions.ParseComplexOperand;
	}

	protected string GetOperandsSeparator(FormulaUtil formulaUtil)
	{
		if (formulaUtil != null)
		{
			return formulaUtil.OperandsSeparator;
		}
		return ",";
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		TokenAttribute[] attributes = Attributes;
		if (attributes == null)
		{
			return;
		}
		int i = 0;
		for (int num = attributes.Length; i < num; i++)
		{
			TokenAttribute tokenAttribute = attributes[i];
			if (tokenAttribute.FormulaType == TokenCode)
			{
				m_strOperationSymbol = tokenAttribute.OperationSymbol;
				m_bPlaceAfter = tokenAttribute.IsPlaceAfter;
				break;
			}
		}
	}
}
