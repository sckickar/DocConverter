using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use GRBackendRenderTarget instead.")]
public struct GRBackendRenderTargetDesc : IEquatable<GRBackendRenderTargetDesc>
{
	public int Width { get; set; }

	public int Height { get; set; }

	public GRPixelConfig Config { get; set; }

	public GRSurfaceOrigin Origin { get; set; }

	public int SampleCount { get; set; }

	public int StencilBits { get; set; }

	public IntPtr RenderTargetHandle { get; set; }

	public readonly SKSizeI Size => new SKSizeI(Width, Height);

	public readonly SKRectI Rect => new SKRectI(0, 0, Width, Height);

	public readonly bool Equals(GRBackendRenderTargetDesc obj)
	{
		if (Width == obj.Width && Height == obj.Height && Config == obj.Config && Origin == obj.Origin && SampleCount == obj.SampleCount && StencilBits == obj.StencilBits)
		{
			return RenderTargetHandle == obj.RenderTargetHandle;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRBackendRenderTargetDesc obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRBackendRenderTargetDesc left, GRBackendRenderTargetDesc right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRBackendRenderTargetDesc left, GRBackendRenderTargetDesc right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Width);
		hashCode.Add(Height);
		hashCode.Add(Config);
		hashCode.Add(Origin);
		hashCode.Add(SampleCount);
		hashCode.Add(StencilBits);
		hashCode.Add(RenderTargetHandle);
		return hashCode.ToHashCode();
	}
}
