using System;

namespace SkiaSharp;

public class SKXmlStreamWriter : SKXmlWriter
{
	internal SKXmlStreamWriter(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	public SKXmlStreamWriter(SKWStream stream)
		: this(IntPtr.Zero, owns: true)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Handle = SkiaApi.sk_xmlstreamwriter_new(stream.Handle);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_xmlstreamwriter_delete(Handle);
	}
}
