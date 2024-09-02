using System.Xml;
using DocGen.ComponentModel;

namespace DocGen.Styles;

internal sealed class StyleInfoPropertyWriteXmlEventArgs : SyncfusionHandledEventArgs
{
	private XmlWriter writer;

	private StyleInfoStore store;

	private StyleInfoProperty sip;

	public XmlWriter Writer => writer;

	public StyleInfoStore Store => store;

	[TraceProperty(true)]
	public StyleInfoProperty Sip => sip;

	public StyleInfoPropertyWriteXmlEventArgs(XmlWriter writer, StyleInfoStore store, StyleInfoProperty sip)
	{
		this.writer = writer;
		this.store = store;
		this.sip = sip;
	}
}
