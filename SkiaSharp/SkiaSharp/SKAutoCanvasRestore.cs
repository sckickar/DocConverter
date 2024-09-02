using System;

namespace SkiaSharp;

public class SKAutoCanvasRestore : IDisposable
{
	private SKCanvas canvas;

	private readonly int saveCount;

	public SKAutoCanvasRestore(SKCanvas canvas)
		: this(canvas, doSave: true)
	{
	}

	public SKAutoCanvasRestore(SKCanvas canvas, bool doSave)
	{
		this.canvas = canvas;
		saveCount = 0;
		if (canvas != null)
		{
			saveCount = canvas.SaveCount;
			if (doSave)
			{
				canvas.Save();
			}
		}
	}

	public void Dispose()
	{
		Restore();
	}

	public void Restore()
	{
		if (canvas != null)
		{
			canvas.RestoreToCount(saveCount);
			canvas = null;
		}
	}
}
