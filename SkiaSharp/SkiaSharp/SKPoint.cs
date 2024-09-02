using System;

namespace SkiaSharp;

public struct SKPoint : IEquatable<SKPoint>
{
	public static readonly SKPoint Empty;

	private float x;

	private float y;

	public readonly bool IsEmpty => this == Empty;

	public readonly float Length => (float)Math.Sqrt(x * x + y * y);

	public readonly float LengthSquared => x * x + y * y;

	public float X
	{
		readonly get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		readonly get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public SKPoint(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public void Offset(SKPoint p)
	{
		x += p.x;
		y += p.y;
	}

	public void Offset(float dx, float dy)
	{
		x += dx;
		y += dy;
	}

	public override readonly string ToString()
	{
		return $"{{X={x}, Y={y}}}";
	}

	public static SKPoint Normalize(SKPoint point)
	{
		float num = point.x * point.x + point.y * point.y;
		double num2 = 1.0 / Math.Sqrt(num);
		return new SKPoint((float)((double)point.x * num2), (float)((double)point.y * num2));
	}

	public static float Distance(SKPoint point, SKPoint other)
	{
		float num = point.x - other.x;
		float num2 = point.y - other.y;
		float num3 = num * num + num2 * num2;
		return (float)Math.Sqrt(num3);
	}

	public static float DistanceSquared(SKPoint point, SKPoint other)
	{
		float num = point.x - other.x;
		float num2 = point.y - other.y;
		return num * num + num2 * num2;
	}

	public static SKPoint Reflect(SKPoint point, SKPoint normal)
	{
		float num = point.x * point.x + point.y * point.y;
		return new SKPoint(point.x - 2f * num * normal.x, point.y - 2f * num * normal.y);
	}

	public static SKPoint Add(SKPoint pt, SKSizeI sz)
	{
		return pt + sz;
	}

	public static SKPoint Add(SKPoint pt, SKSize sz)
	{
		return pt + sz;
	}

	public static SKPoint Add(SKPoint pt, SKPointI sz)
	{
		return pt + sz;
	}

	public static SKPoint Add(SKPoint pt, SKPoint sz)
	{
		return pt + sz;
	}

	public static SKPoint Subtract(SKPoint pt, SKSizeI sz)
	{
		return pt - sz;
	}

	public static SKPoint Subtract(SKPoint pt, SKSize sz)
	{
		return pt - sz;
	}

	public static SKPoint Subtract(SKPoint pt, SKPointI sz)
	{
		return pt - sz;
	}

	public static SKPoint Subtract(SKPoint pt, SKPoint sz)
	{
		return pt - sz;
	}

	public static SKPoint operator +(SKPoint pt, SKSizeI sz)
	{
		return new SKPoint(pt.x + (float)sz.Width, pt.y + (float)sz.Height);
	}

	public static SKPoint operator +(SKPoint pt, SKSize sz)
	{
		return new SKPoint(pt.x + sz.Width, pt.y + sz.Height);
	}

	public static SKPoint operator +(SKPoint pt, SKPointI sz)
	{
		return new SKPoint(pt.x + (float)sz.X, pt.y + (float)sz.Y);
	}

	public static SKPoint operator +(SKPoint pt, SKPoint sz)
	{
		return new SKPoint(pt.x + sz.X, pt.y + sz.Y);
	}

	public static SKPoint operator -(SKPoint pt, SKSizeI sz)
	{
		return new SKPoint(pt.X - (float)sz.Width, pt.Y - (float)sz.Height);
	}

	public static SKPoint operator -(SKPoint pt, SKSize sz)
	{
		return new SKPoint(pt.X - sz.Width, pt.Y - sz.Height);
	}

	public static SKPoint operator -(SKPoint pt, SKPointI sz)
	{
		return new SKPoint(pt.X - (float)sz.X, pt.Y - (float)sz.Y);
	}

	public static SKPoint operator -(SKPoint pt, SKPoint sz)
	{
		return new SKPoint(pt.X - sz.X, pt.Y - sz.Y);
	}

	public readonly bool Equals(SKPoint obj)
	{
		if (x == obj.x)
		{
			return y == obj.y;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKPoint obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKPoint left, SKPoint right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKPoint left, SKPoint right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(x);
		hashCode.Add(y);
		return hashCode.ToHashCode();
	}
}
