using System;

namespace SkiaSharp;

public class SKAutoMaskFreeImage : IDisposable
{
	private IntPtr image;

	public SKAutoMaskFreeImage(IntPtr maskImage)
	{
		image = maskImage;
	}

	public void Dispose()
	{
		if (image != IntPtr.Zero)
		{
			SKMask.FreeImage(image);
			image = IntPtr.Zero;
		}
	}
}
