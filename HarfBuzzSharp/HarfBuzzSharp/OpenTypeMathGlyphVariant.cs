using System;

namespace HarfBuzzSharp;

public struct OpenTypeMathGlyphVariant : IEquatable<OpenTypeMathGlyphVariant>
{
	private uint glyph;

	private int advance;

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

	public int Advance
	{
		readonly get
		{
			return advance;
		}
		set
		{
			advance = value;
		}
	}

	public readonly bool Equals(OpenTypeMathGlyphVariant obj)
	{
		if (glyph == obj.glyph)
		{
			return advance == obj.advance;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeMathGlyphVariant obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(glyph);
		hashCode.Add(advance);
		return hashCode.ToHashCode();
	}
}
