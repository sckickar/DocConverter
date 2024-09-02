using System;

namespace SkiaSharp;

public struct SKRectI : IEquatable<SKRectI>
{
	public static readonly SKRectI Empty;

	private int left;

	private int top;

	private int right;

	private int bottom;

	public readonly int MidX => left + Width / 2;

	public readonly int MidY => top + Height / 2;

	public readonly int Width => right - left;

	public readonly int Height => bottom - top;

	public readonly bool IsEmpty => this == Empty;

	public SKSizeI Size
	{
		readonly get
		{
			return new SKSizeI(Width, Height);
		}
		set
		{
			right = left + value.Width;
			bottom = top + value.Height;
		}
	}

	public SKPointI Location
	{
		readonly get
		{
			return new SKPointI(left, top);
		}
		set
		{
			this = Create(value, Size);
		}
	}

	public readonly SKRectI Standardized
	{
		get
		{
			if (left > right)
			{
				if (top > bottom)
				{
					return new SKRectI(right, bottom, left, top);
				}
				return new SKRectI(right, top, left, bottom);
			}
			if (top > bottom)
			{
				return new SKRectI(left, bottom, right, top);
			}
			return new SKRectI(left, top, right, bottom);
		}
	}

	public int Left
	{
		readonly get
		{
			return left;
		}
		set
		{
			left = value;
		}
	}

	public int Top
	{
		readonly get
		{
			return top;
		}
		set
		{
			top = value;
		}
	}

	public int Right
	{
		readonly get
		{
			return right;
		}
		set
		{
			right = value;
		}
	}

	public int Bottom
	{
		readonly get
		{
			return bottom;
		}
		set
		{
			bottom = value;
		}
	}

	public SKRectI(int left, int top, int right, int bottom)
	{
		this.left = left;
		this.right = right;
		this.top = top;
		this.bottom = bottom;
	}

	public readonly SKRectI AspectFit(SKSizeI size)
	{
		return Floor(((SKRect)this).AspectFit(size));
	}

	public readonly SKRectI AspectFill(SKSizeI size)
	{
		return Floor(((SKRect)this).AspectFill(size));
	}

	public static SKRectI Ceiling(SKRect value)
	{
		return Ceiling(value, outwards: false);
	}

	public static SKRectI Ceiling(SKRect value, bool outwards)
	{
		checked
		{
			int num = (int)((outwards && value.Width > 0f) ? Math.Floor(value.Left) : Math.Ceiling(value.Left));
			int num2 = (int)((outwards && value.Height > 0f) ? Math.Floor(value.Top) : Math.Ceiling(value.Top));
			int num3 = (int)((outwards && value.Width < 0f) ? Math.Floor(value.Right) : Math.Ceiling(value.Right));
			int num4 = (int)((outwards && value.Height < 0f) ? Math.Floor(value.Bottom) : Math.Ceiling(value.Bottom));
			return new SKRectI(num, num2, num3, num4);
		}
	}

	public static SKRectI Inflate(SKRectI rect, int x, int y)
	{
		SKRectI result = new SKRectI(rect.left, rect.top, rect.right, rect.bottom);
		result.Inflate(x, y);
		return result;
	}

	public void Inflate(SKSizeI size)
	{
		Inflate(size.Width, size.Height);
	}

	public void Inflate(int width, int height)
	{
		left -= width;
		top -= height;
		right += width;
		bottom += height;
	}

	public static SKRectI Intersect(SKRectI a, SKRectI b)
	{
		if (!a.IntersectsWithInclusive(b))
		{
			return Empty;
		}
		return new SKRectI(Math.Max(a.left, b.left), Math.Max(a.top, b.top), Math.Min(a.right, b.right), Math.Min(a.bottom, b.bottom));
	}

	public void Intersect(SKRectI rect)
	{
		this = Intersect(this, rect);
	}

	public static SKRectI Round(SKRect value)
	{
		checked
		{
			int num = (int)Math.Round(value.Left);
			int num2 = (int)Math.Round(value.Top);
			int num3 = (int)Math.Round(value.Right);
			int num4 = (int)Math.Round(value.Bottom);
			return new SKRectI(num, num2, num3, num4);
		}
	}

	public static SKRectI Floor(SKRect value)
	{
		return Floor(value, inwards: false);
	}

	public static SKRectI Floor(SKRect value, bool inwards)
	{
		checked
		{
			int num = (int)((inwards && value.Width > 0f) ? Math.Ceiling(value.Left) : Math.Floor(value.Left));
			int num2 = (int)((inwards && value.Height > 0f) ? Math.Ceiling(value.Top) : Math.Floor(value.Top));
			int num3 = (int)((inwards && value.Width < 0f) ? Math.Ceiling(value.Right) : Math.Floor(value.Right));
			int num4 = (int)((inwards && value.Height < 0f) ? Math.Ceiling(value.Bottom) : Math.Floor(value.Bottom));
			return new SKRectI(num, num2, num3, num4);
		}
	}

	public static SKRectI Truncate(SKRect value)
	{
		checked
		{
			int num = (int)value.Left;
			int num2 = (int)value.Top;
			int num3 = (int)value.Right;
			int num4 = (int)value.Bottom;
			return new SKRectI(num, num2, num3, num4);
		}
	}

	public static SKRectI Union(SKRectI a, SKRectI b)
	{
		return new SKRectI(Math.Min(a.Left, b.Left), Math.Min(a.Top, b.Top), Math.Max(a.Right, b.Right), Math.Max(a.Bottom, b.Bottom));
	}

	public void Union(SKRectI rect)
	{
		this = Union(this, rect);
	}

	public readonly bool Contains(int x, int y)
	{
		if (x >= left && x < right && y >= top)
		{
			return y < bottom;
		}
		return false;
	}

	public readonly bool Contains(SKPointI pt)
	{
		return Contains(pt.X, pt.Y);
	}

	public readonly bool Contains(SKRectI rect)
	{
		if (left <= rect.left && right >= rect.right && top <= rect.top)
		{
			return bottom >= rect.bottom;
		}
		return false;
	}

	public readonly bool IntersectsWith(SKRectI rect)
	{
		if (left < rect.right && right > rect.left && top < rect.bottom)
		{
			return bottom > rect.top;
		}
		return false;
	}

	public readonly bool IntersectsWithInclusive(SKRectI rect)
	{
		if (left <= rect.right && right >= rect.left && top <= rect.bottom)
		{
			return bottom >= rect.top;
		}
		return false;
	}

	public void Offset(int x, int y)
	{
		left += x;
		top += y;
		right += x;
		bottom += y;
	}

	public void Offset(SKPointI pos)
	{
		Offset(pos.X, pos.Y);
	}

	public override readonly string ToString()
	{
		return $"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";
	}

	public static SKRectI Create(SKSizeI size)
	{
		return Create(SKPointI.Empty.X, SKPointI.Empty.Y, size.Width, size.Height);
	}

	public static SKRectI Create(SKPointI location, SKSizeI size)
	{
		return Create(location.X, location.Y, size.Width, size.Height);
	}

	public static SKRectI Create(int width, int height)
	{
		return new SKRectI(SKPointI.Empty.X, SKPointI.Empty.X, width, height);
	}

	public static SKRectI Create(int x, int y, int width, int height)
	{
		return new SKRectI(x, y, x + width, y + height);
	}

	public readonly bool Equals(SKRectI obj)
	{
		if (left == obj.left && top == obj.top && right == obj.right)
		{
			return bottom == obj.bottom;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRectI obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRectI left, SKRectI right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRectI left, SKRectI right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(left);
		hashCode.Add(top);
		hashCode.Add(right);
		hashCode.Add(bottom);
		return hashCode.ToHashCode();
	}
}
