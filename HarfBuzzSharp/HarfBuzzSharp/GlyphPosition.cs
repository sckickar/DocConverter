using System;

namespace HarfBuzzSharp;

public struct GlyphPosition : IEquatable<GlyphPosition>
{
	private int x_advance;

	private int y_advance;

	private int x_offset;

	private int y_offset;

	private int var;

	public int XAdvance
	{
		readonly get
		{
			return x_advance;
		}
		set
		{
			x_advance = value;
		}
	}

	public int YAdvance
	{
		readonly get
		{
			return y_advance;
		}
		set
		{
			y_advance = value;
		}
	}

	public int XOffset
	{
		readonly get
		{
			return x_offset;
		}
		set
		{
			x_offset = value;
		}
	}

	public int YOffset
	{
		readonly get
		{
			return y_offset;
		}
		set
		{
			y_offset = value;
		}
	}

	public readonly bool Equals(GlyphPosition obj)
	{
		if (x_advance == obj.x_advance && y_advance == obj.y_advance && x_offset == obj.x_offset && y_offset == obj.y_offset)
		{
			return var == obj.var;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GlyphPosition obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GlyphPosition left, GlyphPosition right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GlyphPosition left, GlyphPosition right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(x_advance);
		hashCode.Add(y_advance);
		hashCode.Add(x_offset);
		hashCode.Add(y_offset);
		hashCode.Add(var);
		return hashCode.ToHashCode();
	}
}
