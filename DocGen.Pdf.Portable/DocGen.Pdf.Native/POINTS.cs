using DocGen.Drawing;

namespace DocGen.Pdf.Native;

internal struct POINTS
{
	public short x;

	public short y;

	public POINTS(short X, short Y)
	{
		x = X;
		y = Y;
	}

	public static implicit operator Point(POINTS p)
	{
		return new Point(p.x, p.y);
	}

	public static implicit operator PointF(POINTS p)
	{
		return new PointF(p.x, p.y);
	}

	public static implicit operator POINTS(Point p)
	{
		return new POINTS((short)p.X, (short)p.Y);
	}

	public static implicit operator POINTS(PointF p)
	{
		return new POINTS((short)p.X, (short)p.Y);
	}
}
