using System;

namespace SkiaSharp;

public struct SKImageInfo : IEquatable<SKImageInfo>
{
	public static readonly SKImageInfo Empty;

	public static readonly SKColorType PlatformColorType;

	public static readonly int PlatformColorAlphaShift;

	public static readonly int PlatformColorRedShift;

	public static readonly int PlatformColorGreenShift;

	public static readonly int PlatformColorBlueShift;

	public int Width { get; set; }

	public int Height { get; set; }

	public SKColorType ColorType { get; set; }

	public SKAlphaType AlphaType { get; set; }

	public SKColorSpace ColorSpace { get; set; }

	public readonly int BytesPerPixel => ColorType.GetBytesPerPixel();

	public readonly int BitsPerPixel => BytesPerPixel * 8;

	public readonly int BytesSize => Width * Height * BytesPerPixel;

	public readonly long BytesSize64 => (long)Width * (long)Height * BytesPerPixel;

	public readonly int RowBytes => Width * BytesPerPixel;

	public readonly long RowBytes64 => (long)Width * (long)BytesPerPixel;

	public readonly bool IsEmpty
	{
		get
		{
			if (Width > 0)
			{
				return Height <= 0;
			}
			return true;
		}
	}

	public readonly bool IsOpaque => AlphaType == SKAlphaType.Opaque;

	public readonly SKSizeI Size => new SKSizeI(Width, Height);

	public readonly SKRectI Rect => SKRectI.Create(Width, Height);

	unsafe static SKImageInfo()
	{
		PlatformColorType = SkiaApi.sk_colortype_get_default_8888().FromNative();
		fixed (int* a = &PlatformColorAlphaShift)
		{
			fixed (int* r = &PlatformColorRedShift)
			{
				fixed (int* g = &PlatformColorGreenShift)
				{
					fixed (int* b = &PlatformColorBlueShift)
					{
						SkiaApi.sk_color_get_bit_shift(a, r, g, b);
					}
				}
			}
		}
	}

	public SKImageInfo(int width, int height)
	{
		Width = width;
		Height = height;
		ColorType = PlatformColorType;
		AlphaType = SKAlphaType.Premul;
		ColorSpace = null;
	}

	public SKImageInfo(int width, int height, SKColorType colorType)
	{
		Width = width;
		Height = height;
		ColorType = colorType;
		AlphaType = SKAlphaType.Premul;
		ColorSpace = null;
	}

	public SKImageInfo(int width, int height, SKColorType colorType, SKAlphaType alphaType)
	{
		Width = width;
		Height = height;
		ColorType = colorType;
		AlphaType = alphaType;
		ColorSpace = null;
	}

	public SKImageInfo(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace)
	{
		Width = width;
		Height = height;
		ColorType = colorType;
		AlphaType = alphaType;
		ColorSpace = colorspace;
	}

	public readonly SKImageInfo WithSize(SKSizeI size)
	{
		return WithSize(size.Width, size.Height);
	}

	public readonly SKImageInfo WithSize(int width, int height)
	{
		SKImageInfo result = this;
		result.Width = width;
		result.Height = height;
		return result;
	}

	public readonly SKImageInfo WithColorType(SKColorType newColorType)
	{
		SKImageInfo result = this;
		result.ColorType = newColorType;
		return result;
	}

	public readonly SKImageInfo WithColorSpace(SKColorSpace newColorSpace)
	{
		SKImageInfo result = this;
		result.ColorSpace = newColorSpace;
		return result;
	}

	public readonly SKImageInfo WithAlphaType(SKAlphaType newAlphaType)
	{
		SKImageInfo result = this;
		result.AlphaType = newAlphaType;
		return result;
	}

	public readonly bool Equals(SKImageInfo obj)
	{
		if (ColorSpace == obj.ColorSpace && Width == obj.Width && Height == obj.Height && ColorType == obj.ColorType)
		{
			return AlphaType == obj.AlphaType;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKImageInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKImageInfo left, SKImageInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKImageInfo left, SKImageInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(ColorSpace);
		hashCode.Add(Width);
		hashCode.Add(Height);
		hashCode.Add(ColorType);
		hashCode.Add(AlphaType);
		return hashCode.ToHashCode();
	}
}
