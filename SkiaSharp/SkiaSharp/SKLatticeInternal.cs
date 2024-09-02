using System;

namespace SkiaSharp;

internal struct SKLatticeInternal : IEquatable<SKLatticeInternal>
{
	public unsafe int* fXDivs;

	public unsafe int* fYDivs;

	public unsafe SKLatticeRectType* fRectTypes;

	public int fXCount;

	public int fYCount;

	public unsafe SKRectI* fBounds;

	public unsafe uint* fColors;

	public unsafe readonly bool Equals(SKLatticeInternal obj)
	{
		if (fXDivs == obj.fXDivs && fYDivs == obj.fYDivs && fRectTypes == obj.fRectTypes && fXCount == obj.fXCount && fYCount == obj.fYCount && fBounds == obj.fBounds)
		{
			return fColors == obj.fColors;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKLatticeInternal obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKLatticeInternal left, SKLatticeInternal right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKLatticeInternal left, SKLatticeInternal right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add((void*)fXDivs);
		hashCode.Add((void*)fYDivs);
		hashCode.Add((void*)fRectTypes);
		hashCode.Add(fXCount);
		hashCode.Add(fYCount);
		hashCode.Add((void*)fBounds);
		hashCode.Add((void*)fColors);
		return hashCode.ToHashCode();
	}
}
