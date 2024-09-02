using DocGen.ComponentModel;

namespace DocGen.Styles;

internal class StyleChangedEventArgs : SyncfusionEventArgs
{
	private StyleInfoProperty sip;

	[TraceProperty(true)]
	public StyleInfoProperty Sip => sip;

	public StyleChangedEventArgs(StyleInfoProperty sip)
	{
		this.sip = sip;
	}
}
