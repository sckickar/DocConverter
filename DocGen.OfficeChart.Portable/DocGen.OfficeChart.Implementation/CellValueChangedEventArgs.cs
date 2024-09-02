using System;

namespace DocGen.OfficeChart.Implementation;

internal class CellValueChangedEventArgs : EventArgs
{
	private object m_oldValue;

	private object m_newValue;

	private IRange m_range;

	public object OldValue
	{
		get
		{
			return m_oldValue;
		}
		set
		{
			m_oldValue = value;
		}
	}

	public object NewValue
	{
		get
		{
			return m_newValue;
		}
		set
		{
			m_newValue = value;
		}
	}

	public IRange Range
	{
		get
		{
			return m_range;
		}
		set
		{
			m_range = value;
		}
	}
}
