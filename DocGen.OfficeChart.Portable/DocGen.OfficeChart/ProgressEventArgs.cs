using System;

namespace DocGen.OfficeChart;

internal class ProgressEventArgs : EventArgs
{
	private long m_lPosition;

	private long m_lSize;

	public long Position => m_lPosition;

	public long FullSize => m_lSize;

	public ProgressEventArgs(long curPos, long fullSize)
	{
		m_lPosition = curPos;
		m_lSize = fullSize;
	}
}
