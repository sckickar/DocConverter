using System;

namespace DocGen.OfficeChart.Implementation.Collections;

internal sealed class TabSheetMovedEventArgs : EventArgs
{
	private int m_iOldIndex;

	private int m_iNewIndex;

	public int OldIndex => m_iOldIndex;

	public int NewIndex => m_iNewIndex;

	private TabSheetMovedEventArgs()
	{
	}

	public TabSheetMovedEventArgs(int oldIndex, int newIndex)
	{
		m_iOldIndex = oldIndex;
		m_iNewIndex = newIndex;
	}
}
