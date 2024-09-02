using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKCodecOptions : IEquatable<SKCodecOptions>
{
	public static readonly SKCodecOptions Default;

	public SKZeroInitialized ZeroInitialized { get; set; }

	public SKRectI? Subset { get; set; }

	public readonly bool HasSubset => Subset.HasValue;

	public int FrameIndex { get; set; }

	public int PriorFrame { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKTransferFunctionBehavior PremulBehavior
	{
		readonly get
		{
			return SKTransferFunctionBehavior.Respect;
		}
		set
		{
		}
	}

	static SKCodecOptions()
	{
		Default = new SKCodecOptions(SKZeroInitialized.No);
	}

	public SKCodecOptions(SKZeroInitialized zeroInitialized)
	{
		ZeroInitialized = zeroInitialized;
		Subset = null;
		FrameIndex = 0;
		PriorFrame = -1;
	}

	public SKCodecOptions(SKZeroInitialized zeroInitialized, SKRectI subset)
	{
		ZeroInitialized = zeroInitialized;
		Subset = subset;
		FrameIndex = 0;
		PriorFrame = -1;
	}

	public SKCodecOptions(SKRectI subset)
	{
		ZeroInitialized = SKZeroInitialized.No;
		Subset = subset;
		FrameIndex = 0;
		PriorFrame = -1;
	}

	public SKCodecOptions(int frameIndex)
	{
		ZeroInitialized = SKZeroInitialized.No;
		Subset = null;
		FrameIndex = frameIndex;
		PriorFrame = -1;
	}

	public SKCodecOptions(int frameIndex, int priorFrame)
	{
		ZeroInitialized = SKZeroInitialized.No;
		Subset = null;
		FrameIndex = frameIndex;
		PriorFrame = priorFrame;
	}

	public readonly bool Equals(SKCodecOptions obj)
	{
		if (ZeroInitialized == obj.ZeroInitialized)
		{
			SKRectI? subset = Subset;
			SKRectI? subset2 = obj.Subset;
			if (subset.HasValue == subset2.HasValue && (!subset.HasValue || subset.GetValueOrDefault() == subset2.GetValueOrDefault()) && FrameIndex == obj.FrameIndex)
			{
				return PriorFrame == obj.PriorFrame;
			}
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKCodecOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKCodecOptions left, SKCodecOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKCodecOptions left, SKCodecOptions right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(ZeroInitialized);
		hashCode.Add(Subset);
		hashCode.Add(FrameIndex);
		hashCode.Add(PriorFrame);
		return hashCode.ToHashCode();
	}
}
