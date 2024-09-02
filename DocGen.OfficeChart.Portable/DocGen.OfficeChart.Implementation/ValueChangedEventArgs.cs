using System;
using System.Diagnostics;

namespace DocGen.OfficeChart.Implementation;

[DebuggerStepThrough]
internal class ValueChangedEventArgs : EventArgs
{
	private static ValueChangedEventArgs _empty = new ValueChangedEventArgs();

	private object m_old;

	private object m_new;

	private string m_strName;

	private ValueChangedEventArgs m_next;

	public object newValue
	{
		[DebuggerStepThrough]
		get
		{
			return m_new;
		}
	}

	public object oldValue
	{
		[DebuggerStepThrough]
		get
		{
			return m_old;
		}
	}

	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return m_strName;
		}
	}

	public ValueChangedEventArgs Next
	{
		[DebuggerStepThrough]
		get
		{
			return m_next;
		}
		set
		{
			m_next = null;
		}
	}

	public new static ValueChangedEventArgs Empty
	{
		[DebuggerStepThrough]
		get
		{
			return _empty;
		}
	}

	private ValueChangedEventArgs()
	{
	}

	public ValueChangedEventArgs(object old, object newValue, string objectName)
		: this(old, newValue, objectName, null)
	{
	}

	public ValueChangedEventArgs(object old, object newValue, string objectName, ValueChangedEventArgs next)
	{
		m_old = old;
		m_new = newValue;
		m_strName = objectName;
		m_next = next;
	}
}
