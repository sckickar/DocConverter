using System;

namespace DocGen.OfficeChart.Calculate;

internal class UpdateNamedRangeEventArgs : EventArgs
{
	internal string Name;

	internal string Address = string.Empty;

	internal bool IsFormulaUpdated;

	internal UpdateNamedRangeEventArgs(string Name)
	{
		this.Name = Name;
	}
}
