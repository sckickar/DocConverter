using System;

namespace DocGen.Drawing.DocIOHelper;

internal interface IBrush : IDisposable
{
	Color Color { get; set; }
}
