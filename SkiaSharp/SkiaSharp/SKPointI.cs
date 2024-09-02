using System;

namespace SkiaSharp;

public struct SKPointI : IEquatable<SKPointI>
{
	public static readonly SKPointI Empty;

	private int x;

	private int y;

	public readonly bool IsEmpty => this == Empty;

	public readonly int Length => (int)Math.Sqrt(x * x + y * y);

	public readonly int LengthSquared => x * x + y * y;

	public int X
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

	public int Y
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

	public SKPointI(SKSizeI sz)
	{
		x = sz.Width;
		y = sz.Height;
	}

	public SKPointI(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public void Offset(SKPointI p)
	{
		x += p.X;
		y += p.Y;
	}

	public void Offset(int dx, int dy)
	{
		x += dx;
		y += dy;
	}

	public override readonly string ToString()
	{
		return $"{{X={x},Y={y}}}";
	}

	public static SKPointI Normalize(SKPointI point)
	{
		int num = point.x * point.x + point.y * point.y;
		double num2 = 1.0 / Math.Sqrt(num);
		return new SKPointI((int)((double)point.x * num2), (int)((double)point.y * num2));
	}

	public static float Distance(SKPointI point, SKPointI other)
	{
		int num = point.x - other.x;
		int num2 = point.y - other.y;
		int num3 = num * num + num2 * num2;
		return (float)Math.Sqrt(num3);
	}

	public static float DistanceSquared(SKPointI point, SKPointI other)
	{
		int num = point.x - other.x;
		int num2 = point.y - other.y;
		return num * num + num2 * num2;
	}

	public static SKPointI Reflect(SKPointI point, SKPointI normal)
	{
		int num = point.x * point.x + point.y * point.y;
		return new SKPointI((int)((float)point.x - 2f * (float)num * (float)normal.x), (int)((float)point.y - 2f * (float)num * (float)normal.y));
	}

	public static SKPointI Ceiling(SKPoint value)
	{
		checked
		{
			int num = (int)Math.Ceiling(value.X);
			int num2 = (int)Math.Ceiling(value.Y);
			return new SKPointI(num, num2);
		}
	}

	public static SKPointI Round(SKPoint value)
	{
		checked
		{
			int num = (int)Math.Round(value.X);
			int num2 = (int)Math.Round(value.Y);
			return new SKPointI(num, num2);
		}
	}

	public static SKPointI Truncate(SKPoint value)
	{
		checked
		{
			int num = (int)value.X;
			int num2 = (int)value.Y;
			return new SKPointI(num, num2);
		}
	}

	public static SKPointI Add(SKPointI pt, SKSizeI sz)
	{
		return pt + sz;
	}

	public static SKPointI Add(SKPointI pt, SKPointI sz)
	{
		return pt + sz;
	}

	public static SKPointI Subtract(SKPointI pt, SKSizeI sz)
	{
		return pt - sz;
	}

	public static SKPointI Subtract(SKPointI pt, SKPointI sz)
	{
		return pt - sz;
	}

	public static SKPointI operator +(SKPointI pt, SKSizeI sz)
	{
		return new SKPointI(pt.X + sz.Width, pt.Y + sz.Height);
	}

	public static SKPointI operator +(SKPointI pt, SKPointI sz)
	{
		return new SKPointI(pt.X + sz.X, pt.Y + sz.Y);
	}

	public static SKPointI operator -(SKPointI pt, SKSizeI sz)
	{
		return new SKPointI(pt.X - sz.Width, pt.Y - sz.Height);
	}

	public static SKPointI operator -(SKPointI pt, SKPointI sz)
	{
		return new SKPointI(pt.X - sz.X, pt.Y - sz.Y);
	}

	public static explicit operator SKSizeI(SKPointI p)
	{
		return new SKSizeI(p.X, p.Y);
	}

	public static implicit operator SKPoint(SKPointI p)
	{
		return new SKPoint(p.X, p.Y);
	}

	public readonly bool Equals(SKPointI obj)
	{
		if (x == obj.x)
		{
			return y == obj.y;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKPointI obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKPointI left, SKPointI right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKPointI left, SKPointI right)
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
