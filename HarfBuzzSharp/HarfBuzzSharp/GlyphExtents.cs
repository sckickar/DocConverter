using System;

namespace HarfBuzzSharp;

public struct GlyphExtents : IEquatable<GlyphExtents>
{
	private int x_bearing;

	private int y_bearing;

	private int width;

	private int height;

	public int XBearing
	{
		readonly get
		{
			return x_bearing;
		}
		set
		{
			x_bearing = value;
		}
	}

	public int YBearing
	{
		readonly get
		{
			return y_bearing;
		}
		set
		{
			y_bearing = value;
		}
	}

	public int Width
	{
		readonly get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public int Height
	{
		readonly get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public readonly bool Equals(GlyphExtents obj)
	{
		if (x_bearing == obj.x_bearing && y_bearing == obj.y_bearing && width == obj.width)
		{
			return height == obj.height;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GlyphExtents obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GlyphExtents left, GlyphExtents right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GlyphExtents left, GlyphExtents right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(x_bearing);
		hashCode.Add(y_bearing);
		hashCode.Add(width);
		hashCode.Add(height);
		return hashCode.ToHashCode();
	}
}
