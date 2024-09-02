using System;

namespace DocGen.OfficeChart.Implementation;

internal class NameIndexChangedEventArgs : EventArgs
{
	private int m_OldIndex;

	private int m_NewIndex;

	public int NewIndex => m_NewIndex;

	public int OldIndex => m_OldIndex;

	private NameIndexChangedEventArgs()
	{
	}

	public NameIndexChangedEventArgs(int oldIndex, int newIndex)
	{
		m_OldIndex = oldIndex;
		m_NewIndex = newIndex;
	}
}
