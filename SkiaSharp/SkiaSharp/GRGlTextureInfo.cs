using System;

namespace SkiaSharp;

public struct GRGlTextureInfo : IEquatable<GRGlTextureInfo>
{
	private uint fTarget;

	private uint fID;

	private uint fFormat;

	public uint Target
	{
		readonly get
		{
			return fTarget;
		}
		set
		{
			fTarget = value;
		}
	}

	public uint Id
	{
		readonly get
		{
			return fID;
		}
		set
		{
			fID = value;
		}
	}

	public uint Format
	{
		readonly get
		{
			return fFormat;
		}
		set
		{
			fFormat = value;
		}
	}

	public GRGlTextureInfo(uint target, uint id)
	{
		fTarget = target;
		fID = id;
		fFormat = 0u;
	}

	public GRGlTextureInfo(uint target, uint id, uint format)
	{
		fTarget = target;
		fID = id;
		fFormat = format;
	}

	public readonly bool Equals(GRGlTextureInfo obj)
	{
		if (fTarget == obj.fTarget && fID == obj.fID)
		{
			return fFormat == obj.fFormat;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRGlTextureInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRGlTextureInfo left, GRGlTextureInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRGlTextureInfo left, GRGlTextureInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fTarget);
		hashCode.Add(fID);
		hashCode.Add(fFormat);
		return hashCode.ToHashCode();
	}
}
