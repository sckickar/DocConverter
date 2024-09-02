using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKSurfaceProperties instead.")]
public struct SKSurfaceProps : IEquatable<SKSurfaceProps>
{
	public SKPixelGeometry PixelGeometry { get; set; }

	public SKSurfacePropsFlags Flags { get; set; }

	public readonly bool Equals(SKSurfaceProps obj)
	{
		if (PixelGeometry == obj.PixelGeometry)
		{
			return Flags == obj.Flags;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKSurfaceProps obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKSurfaceProps left, SKSurfaceProps right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKSurfaceProps left, SKSurfaceProps right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(PixelGeometry);
		hashCode.Add(Flags);
		return hashCode.ToHashCode();
	}
}
