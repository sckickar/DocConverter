using System;
using System.ComponentModel;

namespace SkiaSharp;

public struct SKJpegEncoderOptions : IEquatable<SKJpegEncoderOptions>
{
	public static readonly SKJpegEncoderOptions Default;

	private int fQuality;

	private SKJpegEncoderDownsample fDownsample;

	private SKJpegEncoderAlphaOption fAlphaOption;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete]
	public SKTransferFunctionBehavior BlendBehavior
	{
		readonly get
		{
			return SKTransferFunctionBehavior.Respect;
		}
		set
		{
		}
	}

	public int Quality
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

	public SKJpegEncoderDownsample Downsample
	{
		readonly get
		{
			return fDownsample;
		}
		set
		{
			fDownsample = value;
		}
	}

	public SKJpegEncoderAlphaOption AlphaOption
	{
		readonly get
		{
			return fAlphaOption;
		}
		set
		{
			fAlphaOption = value;
		}
	}

	static SKJpegEncoderOptions()
	{
		Default = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
	}

	public SKJpegEncoderOptions(int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
	{
		fQuality = quality;
		fDownsample = downsample;
		fAlphaOption = alphaOption;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use SKJpegEncoderOptions(int, SKJpegEncoderDownsample, SKJpegEncoderAlphaOption) instead.")]
	public SKJpegEncoderOptions(int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior)
	{
		fQuality = quality;
		fDownsample = downsample;
		fAlphaOption = alphaOption;
	}

	public readonly bool Equals(SKJpegEncoderOptions obj)
	{
		if (fQuality == obj.fQuality && fDownsample == obj.fDownsample)
		{
			return fAlphaOption == obj.fAlphaOption;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKJpegEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKJpegEncoderOptions left, SKJpegEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKJpegEncoderOptions left, SKJpegEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fQuality);
		hashCode.Add(fDownsample);
		hashCode.Add(fAlphaOption);
		return hashCode.ToHashCode();
	}
}
