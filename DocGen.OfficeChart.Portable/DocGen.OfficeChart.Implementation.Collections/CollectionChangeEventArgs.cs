using System;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class CollectionChangeEventArgs<T> : EventArgs
{
	private int m_iIndex;

	private T m_value;

	public int Index => m_iIndex;

	public T Value => m_value;

	private CollectionChangeEventArgs()
	{
	}

	public CollectionChangeEventArgs(int index, T value)
	{
		m_iIndex = index;
		m_value = value;
	}
}
