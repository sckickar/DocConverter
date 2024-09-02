using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKWebpEncoderOptions : IEquatable<SKWebpEncoderOptions>
{
	public static readonly SKWebpEncoderOptions Default;

	private SKWebpEncoderCompression fCompression;

	private float fQuality;

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

	public SKWebpEncoderCompression Compression
	{
		readonly get
		{
			return fCompression;
		}
		set
		{
			fCompression = value;
		}
	}

	public float Quality
	{
		readonly get
		{
			return fQuality;
		}
		set
		{
			fQuality = value;
		}
	}

	static SKWebpEncoderOptions()
	{
		Default = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 100f);
	}

	public SKWebpEncoderOptions(SKWebpEncoderCompression compression, float quality)
	{
		fCompression = compression;
		fQuality = quality;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKWebpEncoderOptions(SKWebpEncoderCompression, float) instead.")]
	public SKWebpEncoderOptions(SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior)
	{
		fCompression = compression;
		fQuality = quality;
	}

	public readonly bool Equals(SKWebpEncoderOptions obj)
	{
		if (fCompression == obj.fCompression)
		{
			return fQuality == obj.fQuality;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKWebpEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKWebpEncoderOptions left, SKWebpEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKWebpEncoderOptions left, SKWebpEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fCompression);
		hashCode.Add(fQuality);
		return hashCode.ToHashCode();
	}
}
