using System;

namespace DocGen.Drawing.SkiaSharpHelper;

internal static class Marshal
{
	internal static void Copy(byte[] source, byte[] destination, int startIndex, int length)
	{
		Buffer.BlockCopy(source, startIndex, destination, startIndex, length);
	}

	internal static void Copy(byte[] source, int startIndex, byte[] destination, int length)
	{
		Buffer.BlockCopy(source, startIndex, destination, startIndex, length);
	}
}
