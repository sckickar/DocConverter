using System;

namespace HarfBuzzSharp;

public struct FontExtents : IEquatable<FontExtents>
{
	private int ascender;

	private int descender;

	private int line_gap;

	private int reserved9;

	private int reserved8;

	private int reserved7;

	private int reserved6;

	private int reserved5;

	private int reserved4;

	private int reserved3;

	private int reserved2;

	private int reserved1;

	public int Ascender
	{
		readonly get
		{
			return ascender;
		}
		set
		{
			ascender = value;
		}
	}

	public int Descender
	{
		readonly get
		{
			return descender;
		}
		set
		{
			descender = value;
		}
	}

	public int LineGap
	{
		readonly get
		{
			return line_gap;
		}
		set
		{
			line_gap = value;
		}
	}

	public readonly bool Equals(FontExtents obj)
	{
		if (ascender == obj.ascender && descender == obj.descender && line_gap == obj.line_gap && reserved9 == obj.reserved9 && reserved8 == obj.reserved8 && reserved7 == obj.reserved7 && reserved6 == obj.reserved6 && reserved5 == obj.reserved5 && reserved4 == obj.reserved4 && reserved3 == obj.reserved3 && reserved2 == obj.reserved2)
		{
			return reserved1 == obj.reserved1;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is FontExtents obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(FontExtents left, FontExtents right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FontExtents left, FontExtents right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(ascender);
		hashCode.Add(descender);
		hashCode.Add(line_gap);
		hashCode.Add(reserved9);
		hashCode.Add(reserved8);
		hashCode.Add(reserved7);
		hashCode.Add(reserved6);
		hashCode.Add(reserved5);
		hashCode.Add(reserved4);
		hashCode.Add(reserved3);
		hashCode.Add(reserved2);
		hashCode.Add(reserved1);
		return hashCode.ToHashCode();
	}
}
