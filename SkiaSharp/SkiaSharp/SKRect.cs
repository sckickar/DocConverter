using System;

namespace SkiaSharp;

public struct SKRect : IEquatable<SKRect>
{
	public static readonly SKRect Empty;

	private float left;

	private float top;

	private float right;

	private float bottom;

	public readonly float MidX => left + Width / 2f;

	public readonly float MidY => top + Height / 2f;

	public readonly float Width => right - left;

	public readonly float Height => bottom - top;

	public readonly bool IsEmpty => this == Empty;

	public SKSize Size
	{
		readonly get
		{
			return new SKSize(Width, Height);
		}
		set
		{
			right = left + value.Width;
			bottom = top + value.Height;
		}
	}

	public SKPoint Location
	{
		readonly get
		{
			return new SKPoint(left, top);
		}
		set
		{
			this = Create(value, Size);
		}
	}

	public readonly SKRect Standardized
	{
		get
		{
			if (left > right)
			{
				if (top > bottom)
				{
					return new SKRect(right, bottom, left, top);
				}
				return new SKRect(right, top, left, bottom);
			}
			if (top > bottom)
			{
				return new SKRect(left, bottom, right, top);
			}
			return new SKRect(left, top, right, bottom);
		}
	}

	public float Left
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

	public float Top
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

	public float Right
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

	public float Bottom
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

	public SKRect(float left, float top, float right, float bottom)
	{
		this.left = left;
		this.right = right;
		this.top = top;
		this.bottom = bottom;
	}

	public readonly SKRect AspectFit(SKSize size)
	{
		return AspectResize(size, fit: true);
	}

	public readonly SKRect AspectFill(SKSize size)
	{
		return AspectResize(size, fit: false);
	}

	private readonly SKRect AspectResize(SKSize size, bool fit)
	{
		if (size.Width == 0f || size.Height == 0f || Width == 0f || Height == 0f)
		{
			return Create(MidX, MidY, 0f, 0f);
		}
		float width = size.Width;
		float height = size.Height;
		float num = width / height;
		float num2 = Width / Height;
		if (fit ? (num2 > num) : (num2 < num))
		{
			height = Height;
			width = height * num;
		}
		else
		{
			width = Width;
			height = width / num;
		}
		float x = MidX - width / 2f;
		float y = MidY - height / 2f;
		return Create(x, y, width, height);
	}

	public static SKRect Inflate(SKRect rect, float x, float y)
	{
		SKRect result = new SKRect(rect.left, rect.top, rect.right, rect.bottom);
		result.Inflate(x, y);
		return result;
	}

	public void Inflate(SKSize size)
	{
		Inflate(size.Width, size.Height);
	}

	public void Inflate(float x, float y)
	{
		left -= x;
		top -= y;
		right += x;
		bottom += y;
	}

	public static SKRect Intersect(SKRect a, SKRect b)
	{
		if (!a.IntersectsWithInclusive(b))
		{
			return Empty;
		}
		return new SKRect(Math.Max(a.left, b.left), Math.Max(a.top, b.top), Math.Min(a.right, b.right), Math.Min(a.bottom, b.bottom));
	}

	public void Intersect(SKRect rect)
	{
		this = Intersect(this, rect);
	}

	public static SKRect Union(SKRect a, SKRect b)
	{
		return new SKRect(Math.Min(a.left, b.left), Math.Min(a.top, b.top), Math.Max(a.right, b.right), Math.Max(a.bottom, b.bottom));
	}

	public void Union(SKRect rect)
	{
		this = Union(this, rect);
	}

	public static implicit operator SKRect(SKRectI r)
	{
		return new SKRect(r.Left, r.Top, r.Right, r.Bottom);
	}

	public readonly bool Contains(float x, float y)
	{
		if (x >= left && x < right && y >= top)
		{
			return y < bottom;
		}
		return false;
	}

	public readonly bool Contains(SKPoint pt)
	{
		return Contains(pt.X, pt.Y);
	}

	public readonly bool Contains(SKRect rect)
	{
		if (left <= rect.left && right >= rect.right && top <= rect.top)
		{
			return bottom >= rect.bottom;
		}
		return false;
	}

	public readonly bool IntersectsWith(SKRect rect)
	{
		if (left < rect.right && right > rect.left && top < rect.bottom)
		{
			return bottom > rect.top;
		}
		return false;
	}

	public readonly bool IntersectsWithInclusive(SKRect rect)
	{
		if (left <= rect.right && right >= rect.left && top <= rect.bottom)
		{
			return bottom >= rect.top;
		}
		return false;
	}

	public void Offset(float x, float y)
	{
		left += x;
		top += y;
		right += x;
		bottom += y;
	}

	public void Offset(SKPoint pos)
	{
		Offset(pos.X, pos.Y);
	}

	public override readonly string ToString()
	{
		return $"{{Left={Left},Top={Top},Width={Width},Height={Height}}}";
	}

	public static SKRect Create(SKPoint location, SKSize size)
	{
		return Create(location.X, location.Y, size.Width, size.Height);
	}

	public static SKRect Create(SKSize size)
	{
		return Create(SKPoint.Empty, size);
	}

	public static SKRect Create(float width, float height)
	{
		return new SKRect(SKPoint.Empty.X, SKPoint.Empty.Y, width, height);
	}

	public static SKRect Create(float x, float y, float width, float height)
	{
		return new SKRect(x, y, x + width, y + height);
	}

	public readonly bool Equals(SKRect obj)
	{
		if (left == obj.left && top == obj.top && right == obj.right)
		{
			return bottom == obj.bottom;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRect obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRect left, SKRect right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRect left, SKRect right)
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
