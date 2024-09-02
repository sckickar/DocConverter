using System;

namespace DocGen.Chart;

internal class ChartPrepareStyleInfoEventArgs : EventArgs
{
	private bool m_handled;

	private int m_index;

	private ChartStyleInfo m_styleInfo;

	public bool Handled
	{
		get
		{
			return m_handled;
		}
		set
		{
			m_handled = value;
		}
	}

	public int Index => m_index;

	public ChartStyleInfo Style => m_styleInfo;

	internal ChartPrepareStyleInfoEventArgs(ChartStyleInfo styleInfo, int index)
	{
		m_styleInfo = styleInfo;
		m_index = index;
	}
}
