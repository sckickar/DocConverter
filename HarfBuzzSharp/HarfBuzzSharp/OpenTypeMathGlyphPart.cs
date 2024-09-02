using System;

namespace HarfBuzzSharp;

public struct OpenTypeMathGlyphPart : IEquatable<OpenTypeMathGlyphPart>
{
	private uint glyph;

	private int start_connector_length;

	private int end_connector_length;

	private int full_advance;

	private OpenTypeMathGlyphPartFlags flags;

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

	public int StartConnectorLength
	{
		readonly get
		{
			return start_connector_length;
		}
		set
		{
			start_connector_length = value;
		}
	}

	public int EndConnectorLength
	{
		readonly get
		{
			return end_connector_length;
		}
		set
		{
			end_connector_length = value;
		}
	}

	public int FullAdvance
	{
		readonly get
		{
			return full_advance;
		}
		set
		{
			full_advance = value;
		}
	}

	public OpenTypeMathGlyphPartFlags Flags
	{
		readonly get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public readonly bool Equals(OpenTypeMathGlyphPart obj)
	{
		if (glyph == obj.glyph && start_connector_length == obj.start_connector_length && end_connector_length == obj.end_connector_length && full_advance == obj.full_advance)
		{
			return flags == obj.flags;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is OpenTypeMathGlyphPart obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(glyph);
		hashCode.Add(start_connector_length);
		hashCode.Add(end_connector_length);
		hashCode.Add(full_advance);
		hashCode.Add(flags);
		return hashCode.ToHashCode();
	}
}
