using DocGen.Drawing;

namespace DocGen.Pdf.Native;

internal struct POINT
{
	public int x;

	public int y;

	public POINT(int X, int Y)
	{
		x = X;
		y = Y;
	}

	public POINT(int lParam)
	{
		x = lParam & 0xFFFF;
		y = lParam >> 16;
	}

	public static implicit operator Point(POINT p)
	{
		return new Point(p.x, p.y);
	}

	public static implicit operator PointF(POINT p)
	{
		return new PointF(p.x, p.y);
	}

	public static implicit operator POINT(Point p)
	{
		return new POINT(p.X, p.Y);
	}
}
