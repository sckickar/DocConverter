using System;
using System.IO;

namespace DocGen.Drawing.DocIOHelper;

internal interface IImage : IDisposable
{
	int Width { get; }

	int Height { get; }

	float HorizontalResolution { get; set; }

	float VerticalResolution { get; set; }

	ImageFormat RawFormat { get; set; }

	PixelFormat PixelFormat { get; set; }

	void Save(Stream stream, ImageFormat imageFormat);
}
