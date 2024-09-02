using DocGen.Drawing;

namespace DocGen.Pdf.Native;

internal struct SIZE
{
	public int cx;

	public int cy;

	public static implicit operator SizeF(SIZE rect)
	{
		return new SizeF(rect.cx, rect.cy);
	}
}
