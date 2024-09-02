using System;

namespace DocGen.ComponentModel;

internal class SyncfusionEventArgs : EventArgs
{
	public override string ToString()
	{
		return TraceProperties.ToString(this);
	}
}
