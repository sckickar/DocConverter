using System;
using DocGen.ComponentModel;

namespace DocGen.Styles;

internal sealed class StyleInfoPropertyConvertEventArgs : SyncfusionHandledEventArgs
{
	private Type desiredType;

	private object value;

	[TraceProperty(true)]
	public object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	[TraceProperty(true)]
	public Type DesiredType => desiredType;

	public StyleInfoPropertyConvertEventArgs(object value, Type desiredType)
	{
		this.desiredType = desiredType;
		this.value = value;
	}
}
