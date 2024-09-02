using System;

namespace SkiaSharp;

public struct SKMask : IEquatable<SKMask>
{
	private unsafe byte* fImage;

	private SKRectI fBounds;

	private uint fRowBytes;

	private SKMaskFormat fFormat;

	public unsafe IntPtr Image
	{
		readonly get
		{
			return (IntPtr)fImage;
		}
		set
		{
			fImage = (byte*)(void*)value;
		}
	}

	public SKRectI Bounds
	{
		readonly get
		{
			return fBounds;
		}
		set
		{
			fBounds = value;
		}
	}

	public uint RowBytes
	{
		readonly get
		{
			return fRowBytes;
		}
		set
		{
			fRowBytes = value;
		}
	}

	public SKMaskFormat Format
	{
		readonly get
		{
			return fFormat;
		}
		set
		{
			fFormat = value;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		get
		{
			fixed (SKMask* cmask = &this)
			{
				return SkiaApi.sk_mask_is_empty(cmask);
			}
		}
	}

	public unsafe readonly bool Equals(SKMask obj)
	{
		if (fImage == obj.fImage && fBounds == obj.fBounds && fRowBytes == obj.fRowBytes)
		{
			return fFormat == obj.fFormat;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKMask obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKMask left, SKMask right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKMask left, SKMask right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add((void*)fImage);
		hashCode.Add(fBounds);
		hashCode.Add(fRowBytes);
		hashCode.Add(fFormat);
		return hashCode.ToHashCode();
	}

	public unsafe SKMask(IntPtr image, SKRectI bounds, uint rowBytes, SKMaskFormat format)
	{
		fBounds = bounds;
		fRowBytes = rowBytes;
		fFormat = format;
		fImage = (byte*)(void*)image;
	}

	public unsafe SKMask(SKRectI bounds, uint rowBytes, SKMaskFormat format)
	{
		fBounds = bounds;
		fRowBytes = rowBytes;
		fFormat = format;
		fImage = null;
	}

	public unsafe Span<byte> GetImageSpan()
	{
		return new Span<byte>((void*)Image, (int)ComputeTotalImageSize());
	}

	public unsafe long AllocateImage()
	{
		fixed (SKMask* cmask = &this)
		{
			IntPtr intPtr = SkiaApi.sk_mask_compute_total_image_size(cmask);
			fImage = SkiaApi.sk_mask_alloc_image(intPtr);
			return (long)intPtr;
		}
	}

	public unsafe void FreeImage()
	{
		if (fImage != null)
		{
			FreeImage((IntPtr)fImage);
			fImage = null;
		}
	}

	public unsafe readonly long ComputeImageSize()
	{
		fixed (SKMask* cmask = &this)
		{
			return (long)SkiaApi.sk_mask_compute_image_size(cmask);
		}
	}

	public unsafe readonly long ComputeTotalImageSize()
	{
		fixed (SKMask* cmask = &this)
		{
			return (long)SkiaApi.sk_mask_compute_total_image_size(cmask);
		}
	}

	public unsafe readonly byte GetAddr1(int x, int y)
	{
		fixed (SKMask* cmask = &this)
		{
			return *SkiaApi.sk_mask_get_addr_1(cmask, x, y);
		}
	}

	public unsafe readonly byte GetAddr8(int x, int y)
	{
		fixed (SKMask* cmask = &this)
		{
			return *SkiaApi.sk_mask_get_addr_8(cmask, x, y);
		}
	}

	public unsafe readonly ushort GetAddr16(int x, int y)
	{
		fixed (SKMask* cmask = &this)
		{
			return *SkiaApi.sk_mask_get_addr_lcd_16(cmask, x, y);
		}
	}

	public unsafe readonly uint GetAddr32(int x, int y)
	{
		fixed (SKMask* cmask = &this)
		{
			return *SkiaApi.sk_mask_get_addr_32(cmask, x, y);
		}
	}

	public unsafe readonly IntPtr GetAddr(int x, int y)
	{
		fixed (SKMask* cmask = &this)
		{
			return (IntPtr)SkiaApi.sk_mask_get_addr(cmask, x, y);
		}
	}

	public unsafe static IntPtr AllocateImage(long size)
	{
		return (IntPtr)SkiaApi.sk_mask_alloc_image((IntPtr)size);
	}

	public unsafe static void FreeImage(IntPtr image)
	{
		SkiaApi.sk_mask_free_image((void*)image);
	}

	public static SKMask Create(byte[] image, SKRectI bounds, uint rowBytes, SKMaskFormat format)
	{
		return Create(image.AsSpan(), bounds, rowBytes, format);
	}

	public unsafe static SKMask Create(ReadOnlySpan<byte> image, SKRectI bounds, uint rowBytes, SKMaskFormat format)
	{
		SKMask result = new SKMask(bounds, rowBytes, format);
		int num = (int)result.ComputeTotalImageSize();
		if (image.Length != num)
		{
			long value = bounds.Height * rowBytes;
			string message = $"Length of image ({image.Length}) does not match the computed size of the mask ({value}). Check the {"bounds"} and {"rowBytes"}.";
			throw new ArgumentException(message);
		}
		result.AllocateImage();
		image.CopyTo(new Span<byte>((void*)result.Image, num));
		return result;
	}
}
