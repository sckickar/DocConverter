using System;

namespace SkiaSharp;

public struct SKSize : IEquatable<SKSize>
{
	public static readonly SKSize Empty;

	private float w;

	private float h;

	public readonly bool IsEmpty => this == Empty;

	public float Width
	{
		readonly get
		{
			return w;
		}
		set
		{
			w = value;
		}
	}

	public float Height
	{
		readonly get
		{
			return h;
		}
		set
		{
			h = value;
		}
	}

	public SKSize(float width, float height)
	{
		w = width;
		h = height;
	}

	public SKSize(SKPoint pt)
	{
		w = pt.X;
		h = pt.Y;
	}

	public readonly SKPoint ToPoint()
	{
		return new SKPoint(w, h);
	}

	public readonly SKSizeI ToSizeI()
	{
		checked
		{
			int width = (int)w;
			int height = (int)h;
			return new SKSizeI(width, height);
		}
	}

	public override readonly string ToString()
	{
		return $"{{Width={w}, Height={h}}}";
	}

	public static SKSize Add(SKSize sz1, SKSize sz2)
	{
		return sz1 + sz2;
	}

	public static SKSize Subtract(SKSize sz1, SKSize sz2)
	{
		return sz1 - sz2;
	}

	public static SKSize operator +(SKSize sz1, SKSize sz2)
	{
		return new SKSize(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
	}

	public static SKSize operator -(SKSize sz1, SKSize sz2)
	{
		return new SKSize(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
	}

	public static explicit operator SKPoint(SKSize size)
	{
		return new SKPoint(size.Width, size.Height);
	}

	public static implicit operator SKSize(SKSizeI size)
	{
		return new SKSize(size.Width, size.Height);
	}

	public readonly bool Equals(SKSize obj)
	{
		if (w == obj.w)
		{
			return h == obj.h;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKSize obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKSize left, SKSize right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKSize left, SKSize right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(w);
		hashCode.Add(h);
		return hashCode.ToHashCode();
	}
}
