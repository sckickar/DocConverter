using System;

namespace DocGen.ComponentModel;

internal sealed class TracePropertyAttribute : Attribute
{
	private bool traceProperty;

	public static readonly TracePropertyAttribute Yes;

	public static readonly TracePropertyAttribute No;

	public static readonly TracePropertyAttribute Default;

	public bool TraceProperty => traceProperty;

	static TracePropertyAttribute()
	{
		Yes = new TracePropertyAttribute(traceProperty: true);
		No = new TracePropertyAttribute(traceProperty: false);
		Default = No;
	}

	public TracePropertyAttribute(bool traceProperty)
	{
		this.traceProperty = traceProperty;
	}

	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}

	public override int GetHashCode()
	{
		return traceProperty.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is TracePropertyAttribute tracePropertyAttribute)
		{
			return tracePropertyAttribute.TraceProperty == traceProperty;
		}
		return false;
	}
}
