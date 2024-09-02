using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class BitmapData
{
	internal int Width { get; set; }

	internal int Height { get; set; }

	internal int Stride { get; set; }

	internal PixelFormat PixelFormat { get; set; }

	internal byte[] Scan0 { get; set; }

	internal int Reserved { get; set; }

	internal BitmapData()
	{
	}
}
