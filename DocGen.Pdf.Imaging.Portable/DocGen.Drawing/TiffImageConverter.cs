using System;
using System.IO;
using BitMiracle.LibTiff.Classic;
using SkiaSharp;

namespace DocGen.Drawing;

internal class TiffImageConverter
{
	internal static MemoryStream ConvertToPng(Stream tiffStream)
	{
		MemoryStream memoryStream = new MemoryStream();
		using Tiff tiff = Tiff.ClientOpen("in-memory", "r", tiffStream, new TiffStream());
		int num = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
		int num2 = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
		int[] array = new int[num2 * num];
		if (!tiff.ReadRGBAImage(num, num2, array))
		{
			throw new Exception("Could not read Tiff image");
		}
		using SKBitmap sKBitmap = new SKBitmap(num, num2, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		byte[] array2 = new byte[sKBitmap.RowBytes * sKBitmap.Height];
		for (int i = 0; i < sKBitmap.Height; i++)
		{
			int num3 = i * sKBitmap.Width;
			int num4 = (sKBitmap.Height - i - 1) * sKBitmap.RowBytes;
			for (int j = 0; j < sKBitmap.Width; j++)
			{
				int num5 = array[num3++];
				array2[num4++] = (byte)((uint)(num5 >> 16) & 0xFFu);
				array2[num4++] = (byte)((uint)(num5 >> 8) & 0xFFu);
				array2[num4++] = (byte)((uint)num5 & 0xFFu);
				array2[num4++] = (byte)((uint)(num5 >> 24) & 0xFFu);
			}
		}
		SKColor[] array3 = new SKColor[array2.Length / 4];
		int num6 = 0;
		int num7;
		for (num7 = 0; num7 < array2.Length; num7++)
		{
			array3[num6] = new SKColor(array2[num7 + 2], array2[num7 + 1], array2[num7], array2[num7 + 3]);
			num7 += 3;
			num6++;
		}
		sKBitmap.Pixels = array3;
		sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(memoryStream);
		memoryStream.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}
}
