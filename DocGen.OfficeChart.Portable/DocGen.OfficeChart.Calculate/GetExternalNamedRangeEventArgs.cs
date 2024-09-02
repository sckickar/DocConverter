using System;

namespace DocGen.OfficeChart.Calculate;

internal class GetExternalNamedRangeEventArgs : EventArgs
{
	internal string formula;

	internal string Addres = string.Empty;

	internal bool IsFormulaUpdated;

	internal GetExternalNamedRangeEventArgs(string formula)
	{
		this.formula = formula;
	}
}
