using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp;

public class SKSvgCanvas
{
	private SKSvgCanvas()
	{
	}

	public static SKCanvas Create(SKRect bounds, Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SKManagedWStream sKManagedWStream = new SKManagedWStream(stream);
		return SKObject.Owned(Create(bounds, sKManagedWStream), sKManagedWStream);
	}

	public unsafe static SKCanvas Create(SKRect bounds, SKWStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return SKObject.Referenced(SKCanvas.GetObject(SkiaApi.sk_svgcanvas_create_with_stream(&bounds, stream.Handle)), stream);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Create(SKRect, Stream) instead.")]
	public unsafe static SKCanvas Create(SKRect bounds, SKXmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		SKCanvas @object = SKCanvas.GetObject(SkiaApi.sk_svgcanvas_create_with_writer(&bounds, writer.Handle));
		writer.RevokeOwnership(@object);
		return SKObject.Referenced(@object, writer);
	}
}
