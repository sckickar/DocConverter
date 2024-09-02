using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKPngEncoderOptions : IEquatable<SKPngEncoderOptions>
{
	public static readonly SKPngEncoderOptions Default;

	private SKPngEncoderFilterFlags fFilterFlags;

	private int fZLibLevel;

	private unsafe void* fComments;

	public SKPngEncoderFilterFlags FilterFlags
	{
		readonly get
		{
			return fFilterFlags;
		}
		set
		{
			fFilterFlags = value;
		}
	}

	public int ZLibLevel
	{
		readonly get
		{
			return fZLibLevel;
		}
		set
		{
			fZLibLevel = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKTransferFunctionBehavior UnpremulBehavior
	{
		readonly get
		{
			return SKTransferFunctionBehavior.Respect;
		}
		set
		{
		}
	}

	static SKPngEncoderOptions()
	{
		Default = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 6);
	}

	public unsafe SKPngEncoderOptions(SKPngEncoderFilterFlags filterFlags, int zLibLevel)
	{
		fFilterFlags = filterFlags;
		fZLibLevel = zLibLevel;
		fComments = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Using SKPngEncoderOptions(SKPngEncoderFilterFlags, int) instead.")]
	public unsafe SKPngEncoderOptions(SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior)
	{
		fFilterFlags = filterFlags;
		fZLibLevel = zLibLevel;
		fComments = null;
	}

	public unsafe readonly bool Equals(SKPngEncoderOptions obj)
	{
		if (fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel)
		{
			return fComments == obj.fComments;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKPngEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKPngEncoderOptions left, SKPngEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKPngEncoderOptions left, SKPngEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fFilterFlags);
		hashCode.Add(fZLibLevel);
		hashCode.Add(fComments);
		return hashCode.ToHashCode();
	}
}
