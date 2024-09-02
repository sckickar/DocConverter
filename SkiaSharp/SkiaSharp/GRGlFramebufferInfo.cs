using System;

namespace SkiaSharp;

public struct GRGlFramebufferInfo : IEquatable<GRGlFramebufferInfo>
{
	private uint fFBOID;

	private uint fFormat;

	public uint FramebufferObjectId
	{
		readonly get
		{
			return fFBOID;
		}
		set
		{
			fFBOID = value;
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

	public GRGlFramebufferInfo(uint fboId)
	{
		fFBOID = fboId;
		fFormat = 0u;
	}

	public GRGlFramebufferInfo(uint fboId, uint format)
	{
		fFBOID = fboId;
		fFormat = format;
	}

	public readonly bool Equals(GRGlFramebufferInfo obj)
	{
		if (fFBOID == obj.fFBOID)
		{
			return fFormat == obj.fFormat;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRGlFramebufferInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRGlFramebufferInfo left, GRGlFramebufferInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRGlFramebufferInfo left, GRGlFramebufferInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fFBOID);
		hashCode.Add(fFormat);
		return hashCode.ToHashCode();
	}
}
