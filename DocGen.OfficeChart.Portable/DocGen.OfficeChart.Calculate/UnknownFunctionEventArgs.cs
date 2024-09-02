using System;

namespace DocGen.OfficeChart.Calculate;

internal class UnknownFunctionEventArgs : EventArgs
{
	private string m_missingFunctionName;

	private string m_cellLocation;

	public string MissingFunctionName
	{
		get
		{
			return m_missingFunctionName;
		}
		internal set
		{
			m_missingFunctionName = value;
		}
	}

	public string CellLocation
	{
		get
		{
			return m_cellLocation;
		}
		internal set
		{
			m_cellLocation = value;
		}
	}
}
