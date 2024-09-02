namespace DocGen.ComponentModel;

internal class SyncfusionSuccessEventArgs : SyncfusionEventArgs
{
	private bool success;

	[TraceProperty(true)]
	public bool Success => success;

	public SyncfusionSuccessEventArgs()
		: this(success: true)
	{
	}

	public SyncfusionSuccessEventArgs(bool success)
	{
		this.success = success;
	}
}
