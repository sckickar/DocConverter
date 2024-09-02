using System;

namespace DocGen.OfficeChart.Calculate;

internal class QueryExternalWorksheetEventArgs : EventArgs
{
	internal string formula;

	internal ICalcData worksheet;

	internal string worksheetName = string.Empty;

	internal bool IsWorksheetUpdated;

	internal QueryExternalWorksheetEventArgs(string formula)
	{
		this.formula = formula;
	}
}
