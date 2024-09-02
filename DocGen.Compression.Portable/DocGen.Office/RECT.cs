using DocGen.Drawing;

namespace DocGen.Office;

internal struct RECT
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public int Width => right - left;

	public int Height => bottom - top;

	public Point TopLeft => new Point(left, top);

	public Size Size => new Size(Width, Height);

	public RECT(int x1, int y1, int x2, int y2)
	{
		left = x1;
		top = y1;
		right = x2;
		bottom = y2;
	}

	public override string ToString()
	{
		return $"{TopLeft}x{Size}";
	}

	public static implicit operator Rectangle(RECT rect)
	{
		return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
	}

	public static implicit operator RectangleF(RECT rect)
	{
		return RectangleF.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
	}

	public static implicit operator Size(RECT rect)
	{
		return new Size(rect.right - rect.left, rect.bottom - rect.top);
	}

	public static explicit operator RECT(Rectangle rect)
	{
		RECT result = default(RECT);
		result.left = rect.Left;
		result.right = rect.Right;
		result.top = rect.Top;
		result.bottom = rect.Bottom;
		return result;
	}
}
