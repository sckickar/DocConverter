using System;

namespace DocGen.OfficeChart.Calculate;

internal class FormulaComputedEventArgs : EventArgs
{
	private string formula;

	private string computedValue;

	private string cell;

	private bool isInnerFormula;

	private bool handled;

	internal string Formula => formula;

	internal string ComputedValue
	{
		get
		{
			return computedValue;
		}
		set
		{
			computedValue = value;
		}
	}

	internal string Cell => cell;

	internal bool IsInnerFormula => isInnerFormula;

	internal bool Handled
	{
		get
		{
			return handled;
		}
		set
		{
			handled = value;
		}
	}

	internal FormulaComputedEventArgs(string formula, string computedValue, string cell, bool isInnerFormula)
	{
		this.formula = formula;
		this.computedValue = computedValue;
		this.cell = cell;
		this.isInnerFormula = isInnerFormula;
	}
}
