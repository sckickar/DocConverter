using System;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class SolidBrush : Brush, ISolidBrush, IBrush, IDisposable
{
	public SolidBrush(Color color)
		: base(color)
	{
	}
}
