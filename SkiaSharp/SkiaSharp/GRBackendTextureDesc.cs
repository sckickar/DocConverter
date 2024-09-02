using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use GRBackendTexture instead.")]
public struct GRBackendTextureDesc : IEquatable<GRBackendTextureDesc>
{
	public GRBackendTextureDescFlags Flags { get; set; }

	public GRSurfaceOrigin Origin { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public GRPixelConfig Config { get; set; }

	public int SampleCount { get; set; }

	public IntPtr TextureHandle { get; set; }

	public readonly SKSizeI Size => new SKSizeI(Width, Height);

	public readonly SKRectI Rect => new SKRectI(0, 0, Width, Height);

	public readonly bool Equals(GRBackendTextureDesc obj)
	{
		if (Flags == obj.Flags && Origin == obj.Origin && Width == obj.Width && Height == obj.Height && Config == obj.Config && SampleCount == obj.SampleCount)
		{
			return TextureHandle == obj.TextureHandle;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRBackendTextureDesc obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRBackendTextureDesc left, GRBackendTextureDesc right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRBackendTextureDesc left, GRBackendTextureDesc right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Flags);
		hashCode.Add(Origin);
		hashCode.Add(Width);
		hashCode.Add(Height);
		hashCode.Add(Config);
		hashCode.Add(SampleCount);
		hashCode.Add(TextureHandle);
		return hashCode.ToHashCode();
	}
}
