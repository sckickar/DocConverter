namespace DocGen.ComponentModel;

internal class SyncfusionHandledEventArgs : SyncfusionEventArgs
{
	private bool handled;

	[TraceProperty(true)]
	public bool Handled
	{
		get
		{
			return handled;
		}
		set
		{
			handled = value;
		}
	}

	public SyncfusionHandledEventArgs()
	{
		handled = false;
	}

	public SyncfusionHandledEventArgs(bool handled)
	{
		this.handled = handled;
	}
}
