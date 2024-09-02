using System;

namespace DocGen.OfficeChart;

internal class ReadOnlyFileEventArgs : EventArgs
{
	private bool m_bRewrite;

	public bool ShouldRewrite
	{
		get
		{
			return m_bRewrite;
		}
		set
		{
			m_bRewrite = value;
		}
	}
}
