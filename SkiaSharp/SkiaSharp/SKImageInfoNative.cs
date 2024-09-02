using System;

namespace SkiaSharp;

internal struct SKImageInfoNative : IEquatable<SKImageInfoNative>
{
	public IntPtr colorspace;

	public int width;

	public int height;

	public SKColorTypeNative colorType;

	public SKAlphaType alphaType;

	public readonly bool Equals(SKImageInfoNative obj)
	{
		if (colorspace == obj.colorspace && width == obj.width && height == obj.height && colorType == obj.colorType)
		{
			return alphaType == obj.alphaType;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKImageInfoNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKImageInfoNative left, SKImageInfoNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKImageInfoNative left, SKImageInfoNative right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(colorspace);
		hashCode.Add(width);
		hashCode.Add(height);
		hashCode.Add(colorType);
		hashCode.Add(alphaType);
		return hashCode.ToHashCode();
	}

	public static void UpdateNative(ref SKImageInfo managed, ref SKImageInfoNative native)
	{
		native.colorspace = managed.ColorSpace?.Handle ?? IntPtr.Zero;
		native.width = managed.Width;
		native.height = managed.Height;
		native.colorType = managed.ColorType.ToNative();
		native.alphaType = managed.AlphaType;
	}

	public static SKImageInfoNative FromManaged(ref SKImageInfo managed)
	{
		SKImageInfoNative result = default(SKImageInfoNative);
		result.colorspace = managed.ColorSpace?.Handle ?? IntPtr.Zero;
		result.width = managed.Width;
		result.height = managed.Height;
		result.colorType = managed.ColorType.ToNative();
		result.alphaType = managed.AlphaType;
		return result;
	}

	public static SKImageInfo ToManaged(ref SKImageInfoNative native)
	{
		SKImageInfo result = default(SKImageInfo);
		result.ColorSpace = SKColorSpace.GetObject(native.colorspace);
		result.Width = native.width;
		result.Height = native.height;
		result.ColorType = native.colorType.FromNative();
		result.AlphaType = native.alphaType;
		return result;
	}
}
