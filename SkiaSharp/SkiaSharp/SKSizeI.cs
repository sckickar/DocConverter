using System;

namespace SkiaSharp;

public struct SKSizeI : IEquatable<SKSizeI>
{
	public static readonly SKSizeI Empty;

	private int w;

	private int h;

	public readonly bool IsEmpty => this == Empty;

	public int Width
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

	public int Height
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

	public SKSizeI(int width, int height)
	{
		w = width;
		h = height;
	}

	public SKSizeI(SKPointI pt)
	{
		w = pt.X;
		h = pt.Y;
	}

	public readonly SKPointI ToPointI()
	{
		return new SKPointI(w, h);
	}

	public override readonly string ToString()
	{
		return $"{{Width={w}, Height={h}}}";
	}

	public static SKSizeI Add(SKSizeI sz1, SKSizeI sz2)
	{
		return sz1 + sz2;
	}

	public static SKSizeI Subtract(SKSizeI sz1, SKSizeI sz2)
	{
		return sz1 - sz2;
	}

	public static SKSizeI operator +(SKSizeI sz1, SKSizeI sz2)
	{
		return new SKSizeI(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
	}

	public static SKSizeI operator -(SKSizeI sz1, SKSizeI sz2)
	{
		return new SKSizeI(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
	}

	public static explicit operator SKPointI(SKSizeI size)
	{
		return new SKPointI(size.Width, size.Height);
	}

	public readonly bool Equals(SKSizeI obj)
	{
		if (w == obj.w)
		{
			return h == obj.h;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKSizeI obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKSizeI left, SKSizeI right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKSizeI left, SKSizeI right)
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
