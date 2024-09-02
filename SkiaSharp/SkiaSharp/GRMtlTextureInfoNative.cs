using System;

namespace SkiaSharp;

internal struct GRMtlTextureInfoNative : IEquatable<GRMtlTextureInfoNative>
{
	public unsafe void* fTexture;

	public unsafe readonly bool Equals(GRMtlTextureInfoNative obj)
	{
		return fTexture == obj.fTexture;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRMtlTextureInfoNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRMtlTextureInfoNative left, GRMtlTextureInfoNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRMtlTextureInfoNative left, GRMtlTextureInfoNative right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fTexture);
		return hashCode.ToHashCode();
	}
}
