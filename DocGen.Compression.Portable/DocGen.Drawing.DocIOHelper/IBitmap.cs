using System;

namespace DocGen.Drawing.DocIOHelper;

internal interface IBitmap : IImage, IDisposable
{
	IImage Decode(byte[] imageData);

	void SetResolution(float dpiX, float dpiY);

	IImage Clone(RectangleF cropRectangle, object pixelFormat);
}
