using System;
using System.IO;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class Bitmap : Image, IBitmap, IImage, IDisposable
{
	public Bitmap()
	{
	}

	public Bitmap(Stream stream)
		: base(stream)
	{
	}

	public Bitmap(SKBitmap sKBitmap)
		: base(sKBitmap)
	{
	}

	internal Bitmap(Image image)
		: base(image.SKBitmap)
	{
	}

	internal Bitmap(int width, int height)
		: base(width, height)
	{
	}

	internal Bitmap(int width, int height, PixelFormat pixelFormat)
		: base(width, height)
	{
		base.PixelFormat = pixelFormat;
	}

	internal Bitmap(Image image, int width, int height)
		: base(image.SKBitmap, width, height)
	{
	}

	public override IImage Decode(byte[] imageData)
	{
		return Decode(new MemoryStream(imageData));
	}

	public Color GetPixel(int x, int y)
	{
		return Extension.GetColor(base.SKBitmap.GetPixel(x, y));
	}

	public void SetPixel(int x, int y, Color color)
	{
		base.SKBitmap.SetPixel(x, y, Extension.GetSKColor(color));
	}

	public Bitmap Decode(Stream imageData)
	{
		return Decode(SKData.Create(imageData));
	}

	public Bitmap Decode(SKData sKData)
	{
		Bitmap bitmap = new Bitmap(SKBitmap.Decode(sKData));
		if (bitmap.SKBitmap != null && (bitmap.SKBitmap.ColorType == SKColorType.Rgb888x || bitmap.SKBitmap.ColorType == SKColorType.Gray8) && bitmap.SKBitmap.CanCopyTo(SKImageInfo.PlatformColorType))
		{
			bitmap.SetSKBitmap(bitmap.SKBitmap.Copy(SKImageInfo.PlatformColorType));
		}
		bitmap.ImageData = sKData.ToArray();
		return bitmap;
	}

	public override IImage Clone(RectangleF cropRectangle, object pixelFormat)
	{
		SKImage sKImage = SKImage.FromBitmap(base.SKBitmap);
		try
		{
			sKImage = sKImage.Subset(new SKRectI((int)cropRectangle.Left, (int)cropRectangle.Top, (int)cropRectangle.Right, (int)cropRectangle.Bottom));
		}
		catch (Exception)
		{
		}
		SKData sKData = sKImage.Encode(SKEncodedImageFormat.Png, 100);
		if (sKData != null)
		{
			return Decode(sKData);
		}
		return this;
	}

	public void SetResolution(float dpiX, float dpiY)
	{
		base.HorizontalResolution = dpiX;
		base.VerticalResolution = dpiY;
	}

	internal BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
	{
		return new BitmapData
		{
			Scan0 = (byte[])base.SKBitmap.Bytes.Clone(),
			Stride = base.SKBitmap.RowBytes,
			Width = base.SKBitmap.Width,
			Height = base.SKBitmap.Height
		};
	}

	internal void UnlockBits(BitmapData bitmapData)
	{
		byte[] scan = bitmapData.Scan0;
		SKColor[] array = new SKColor[scan.Length / 4];
		int num = 0;
		int num2;
		for (num2 = 0; num2 < scan.Length; num2++)
		{
			array[num] = new SKColor(scan[num2 + 2], scan[num2 + 1], scan[num2], scan[num2 + 3]);
			num2 += 3;
			num++;
		}
		base.SKBitmap.Pixels = array;
	}

	public static explicit operator Bitmap(DocGen.Drawing.Image image)
	{
		return (Bitmap)new Bitmap().Decode(image.ImageData);
	}

	internal void MakeTransparent(Color color)
	{
		Size size = base.Size;
		Bitmap bitmap = null;
		Graphics graphics = null;
		bitmap = new Bitmap(size.Width, size.Height);
		graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.Transparent);
		Rectangle destRect = new Rectangle(0, 0, size.Width, size.Height);
		ImageAttributes imageAttributes = null;
		imageAttributes = new ImageAttributes();
		ColorMap[] array = new ColorMap[1]
		{
			new ColorMap()
		};
		array[0].OldColor = Color.FromArgb(color.A, color);
		array[0].NewColor = Color.FromArgb(0, 0, 0, 0);
		imageAttributes.SetRemapTable(array);
		graphics.DrawImage(this, destRect, 0f, 0f, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes);
		Stream stream = new MemoryStream();
		graphics.SkSurface.Snapshot().Encode(GetImageFormat(base.RawFormat), 100).SaveTo(stream);
		stream.Position = 0L;
		SetSKBitmap(SKBitmap.Decode(SKData.Create(stream)));
		imageAttributes?.Dispose();
		graphics?.Dispose();
		bitmap?.Dispose();
	}
}
