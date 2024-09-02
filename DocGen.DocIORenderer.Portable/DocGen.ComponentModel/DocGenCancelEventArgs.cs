using System.ComponentModel;

namespace DocGen.ComponentModel;

internal class SyncfusionCancelEventArgs : CancelEventArgs
{
	public SyncfusionCancelEventArgs()
	{
	}

	public SyncfusionCancelEventArgs(bool cancel)
		: base(cancel)
	{
	}

	public override string ToString()
	{
		return TraceProperties.ToString(this);
	}
}
