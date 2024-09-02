using System;

namespace SkiaSharp;

internal struct SKCodecOptionsInternal : IEquatable<SKCodecOptionsInternal>
{
	public SKZeroInitialized fZeroInitialized;

	public unsafe SKRectI* fSubset;

	public int fFrameIndex;

	public int fPriorFrame;

	public unsafe readonly bool Equals(SKCodecOptionsInternal obj)
	{
		if (fZeroInitialized == obj.fZeroInitialized && fSubset == obj.fSubset && fFrameIndex == obj.fFrameIndex)
		{
			return fPriorFrame == obj.fPriorFrame;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKCodecOptionsInternal obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKCodecOptionsInternal left, SKCodecOptionsInternal right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKCodecOptionsInternal left, SKCodecOptionsInternal right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fZeroInitialized);
		hashCode.Add((void*)fSubset);
		hashCode.Add(fFrameIndex);
		hashCode.Add(fPriorFrame);
		return hashCode.ToHashCode();
	}
}
