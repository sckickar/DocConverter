using System;

namespace DocGen.OfficeChart.Calculate;

internal class UpdateExternalFormulaEventArgs : EventArgs
{
	internal string formula;

	internal string parsedFormula = string.Empty;

	internal bool IsFormulaUpdated;

	internal UpdateExternalFormulaEventArgs(string formula)
	{
		this.formula = formula;
	}
}
