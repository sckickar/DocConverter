using System;

namespace SkiaSharp;

public struct SKLattice : IEquatable<SKLattice>
{
	public int[] XDivs { get; set; }

	public int[] YDivs { get; set; }

	public SKLatticeRectType[] RectTypes { get; set; }

	public SKRectI? Bounds { get; set; }

	public SKColor[] Colors { get; set; }

	public readonly bool Equals(SKLattice obj)
	{
		if (XDivs == obj.XDivs && YDivs == obj.YDivs && RectTypes == obj.RectTypes && Bounds == obj.Bounds)
		{
			return Colors == obj.Colors;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKLattice obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKLattice left, SKLattice right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKLattice left, SKLattice right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(XDivs);
		hashCode.Add(YDivs);
		hashCode.Add(RectTypes);
		hashCode.Add(Bounds);
		hashCode.Add(Colors);
		return hashCode.ToHashCode();
	}
}
