using System;

namespace SkiaSharp;

public class SKOverdrawCanvas : SKNWayCanvas
{
	internal SKOverdrawCanvas(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKOverdrawCanvas(SKCanvas canvas)
		: this(IntPtr.Zero, owns: true)
	{
		if (canvas == null)
		{
			throw new ArgumentNullException("canvas");
		}
		Handle = SkiaApi.sk_overdraw_canvas_new(canvas.Handle);
	}
}
