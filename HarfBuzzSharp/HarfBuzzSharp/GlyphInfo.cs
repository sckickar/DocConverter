using System;

namespace HarfBuzzSharp;

public struct GlyphInfo : IEquatable<GlyphInfo>
{
	private uint codepoint;

	private uint mask;

	private uint cluster;

	private int var1;

	private int var2;

	public unsafe GlyphFlags GlyphFlags
	{
		get
		{
			fixed (GlyphInfo* info = &this)
			{
				return HarfBuzzApi.hb_glyph_info_get_glyph_flags(info);
			}
		}
	}

	public uint Codepoint
	{
		readonly get
		{
			return codepoint;
		}
		set
		{
			codepoint = value;
		}
	}

	public uint Mask
	{
		readonly get
		{
			return mask;
		}
		set
		{
			mask = value;
		}
	}

	public uint Cluster
	{
		readonly get
		{
			return cluster;
		}
		set
		{
			cluster = value;
		}
	}

	public readonly bool Equals(GlyphInfo obj)
	{
		if (codepoint == obj.codepoint && mask == obj.mask && cluster == obj.cluster && var1 == obj.var1)
		{
			return var2 == obj.var2;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GlyphInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GlyphInfo left, GlyphInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GlyphInfo left, GlyphInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(codepoint);
		hashCode.Add(mask);
		hashCode.Add(cluster);
		hashCode.Add(var1);
		hashCode.Add(var2);
		return hashCode.ToHashCode();
	}
}
