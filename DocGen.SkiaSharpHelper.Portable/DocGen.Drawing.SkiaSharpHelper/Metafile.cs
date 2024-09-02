using System.IO;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class Metafile : Bitmap
{
	internal Metafile()
	{
	}

	internal Metafile(Stream stream, nint hdc, Rectangle rect, MetafileFrameUnit metafileFrameUnit, EmfType emfType)
		: base(rect.Width, rect.Height)
	{
		Decode(stream);
	}
}
