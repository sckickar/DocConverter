using System;

namespace SkiaSharp;

public static class SKSwizzle
{
	public static void SwapRedBlue(IntPtr pixels, int count)
	{
		SwapRedBlue(pixels, pixels, count);
	}

	public unsafe static void SwapRedBlue(IntPtr dest, IntPtr src, int count)
	{
		if (dest == IntPtr.Zero)
		{
			throw new ArgumentException("dest");
		}
		if (src == IntPtr.Zero)
		{
			throw new ArgumentException("src");
		}
		SkiaApi.sk_swizzle_swap_rb((uint*)(void*)dest, (uint*)(void*)src, count);
	}

	public static void SwapRedBlue(Span<byte> pixels)
	{
		SwapRedBlue(pixels, pixels, pixels.Length);
	}

	public static void SwapRedBlue(ReadOnlySpan<byte> pixels, int count)
	{
		SwapRedBlue(pixels, pixels, count);
	}

	public unsafe static void SwapRedBlue(ReadOnlySpan<byte> dest, ReadOnlySpan<byte> src, int count)
	{
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (src == null)
		{
			throw new ArgumentNullException("src");
		}
		fixed (byte* dest2 = dest)
		{
			fixed (byte* src2 = src)
			{
				SkiaApi.sk_swizzle_swap_rb((uint*)dest2, (uint*)src2, count);
			}
		}
	}
}
