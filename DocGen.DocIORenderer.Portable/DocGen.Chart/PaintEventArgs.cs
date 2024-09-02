using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class PaintEventArgs
{
	public Rectangle Rectangle { get; set; }

	public Graphics Graphics { get; set; }

	public PaintEventArgs(Graphics g, Rectangle rect)
	{
		Rectangle = rect;
		Graphics = g;
	}
}
