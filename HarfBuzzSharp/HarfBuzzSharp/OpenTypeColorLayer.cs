using System;

namespace HarfBuzzSharp;

public struct OpenTypeColorLayer : IEquatable<OpenTypeColorLayer>
{
	private uint glyph;

	private uint color_index;

	public uint Glyph
	{
		readonly get
		{
			return glyph;
		}
		set
		{
			glyph = value;
		}
	}

	public uint ColorIndex
	{
		readonly get
		{
			return color_index;
		}
		set
		{
			color_index = value;
		}
	}

	public readonly bool Equals(OpenTypeColorLayer obj)
	{
		if (glyph == obj.glyph)
		{
			return color_index == obj.color_index;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeColorLayer obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeColorLayer left, OpenTypeColorLayer right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeColorLayer left, OpenTypeColorLayer right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(glyph);
		hashCode.Add(color_index);
		return hashCode.ToHashCode();
	}
}
