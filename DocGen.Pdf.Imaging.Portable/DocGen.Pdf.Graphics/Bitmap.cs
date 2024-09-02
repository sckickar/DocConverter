using System;
using System.IO;
using System.Linq;
using System.Text;
using SkiaSharp;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class Bitmap : ImageHelper, IDisposable
{
	internal ImageFormat m_format;

	internal GraphicsHelper m_graphics;

	internal SKBitmap m_sKBitmap;

	internal SKImageInfo m_sKImageInfo;

	internal byte[] m_imageData;

	internal new int Width => m_sKBitmap.Width;

	internal new int Height => m_sKBitmap.Height;

	public Bitmap(byte[] imageData)
	{
		m_imageData = imageData;
	}

	internal Bitmap(Stream stream)
	{
		if (stream == null)
		{
			return;
		}
		m_format = GetImageFormat(stream);
		stream.Position = 0L;
		if (this != null)
		{
			Bitmap bitmap = Decode(stream);
			if (bitmap != null)
			{
				m_sKBitmap = bitmap.m_sKBitmap;
			}
		}
		else
		{
			m_sKBitmap = SKBitmap.Decode(SKData.Create(stream));
		}
		if (m_sKBitmap != null)
		{
			m_sKImageInfo = m_sKBitmap.Info;
		}
	}

	internal Bitmap(SKBitmap sKBitmap)
	{
		m_sKImageInfo = sKBitmap.Info;
		m_sKBitmap = sKBitmap;
	}

	internal Bitmap(int width, int height)
	{
		m_sKImageInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
		m_sKBitmap = new SKBitmap(m_sKImageInfo);
	}

	internal Bitmap(Bitmap image, int width, int height)
	{
		m_sKImageInfo = new SKImageInfo(width, height);
		m_sKBitmap = image.m_sKBitmap.Resize(m_sKImageInfo, SKBitmapResizeMethod.Box);
	}

	internal Color GetPixel(int x, int y)
	{
		SKColor pixel = m_sKBitmap.GetPixel(x, y);
		return Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
	}

	internal void SetPixel(int x, int y, Color color)
	{
		m_sKBitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B, color.A));
	}

	internal Bitmap Decode(Stream imageData)
	{
		return Decode(SKData.Create(imageData));
	}

	internal Bitmap Decode(SKData sKData)
	{
		SKBitmap sKBitmap = SKBitmap.Decode(sKData);
		if (sKBitmap == null)
		{
			return null;
		}
		Bitmap bitmap = new Bitmap(sKBitmap);
		if (bitmap.m_sKBitmap != null && (bitmap.m_sKBitmap.ColorType == SKColorType.Rgb888x || bitmap.m_sKBitmap.ColorType == SKColorType.Gray8) && bitmap.m_sKBitmap.CanCopyTo(SKImageInfo.PlatformColorType))
		{
			bitmap.m_sKBitmap = bitmap.m_sKBitmap.Copy(SKImageInfo.PlatformColorType);
		}
		bitmap.m_imageData = sKData.ToArray();
		try
		{
			SKEncodedImageFormat encodedFormat = SKCodec.Create(sKData).EncodedFormat;
			bitmap.m_format = bitmap.GetImageFormat(encodedFormat);
		}
		catch
		{
			bitmap.m_format = ImageFormat.Tiff;
		}
		return bitmap;
	}

	internal void MakeTransparent(Color color)
	{
		Size size = new Size(Width, Height);
		Bitmap bitmap = null;
		GraphicsHelper graphicsHelper = null;
		bitmap = new Bitmap(size.Width, size.Height);
		graphicsHelper = new GraphicsHelper(bitmap);
		graphicsHelper.Clear(Color.Transparent);
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
		graphicsHelper.DrawImage(this, destRect, 0f, 0f, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes);
		Stream stream = new MemoryStream();
		graphicsHelper.m_surface.Snapshot().Encode(GetImageFormat(m_format), 100).SaveTo(stream);
		stream.Position = 0L;
		m_sKBitmap = SKBitmap.Decode(SKData.Create(stream));
		imageAttributes?.Dispose();
		graphicsHelper?.Dispose();
		bitmap?.Dispose();
	}

	internal new void Save(Stream stream, ImageFormat imageFormat)
	{
		if (m_graphics == null)
		{
			m_graphics = new GraphicsHelper(this);
			m_graphics.DrawImage(this, new RectangleF(0f, 0f, m_sKBitmap.Width, m_sKBitmap.Height));
		}
		m_graphics.m_surface.Snapshot().Encode(GetImageFormat(imageFormat), 100).SaveTo(stream);
	}

	internal SKEncodedImageFormat GetImageFormat(ImageFormat imageFormat)
	{
		return imageFormat switch
		{
			ImageFormat.Bmp => SKEncodedImageFormat.Bmp, 
			ImageFormat.Gif => SKEncodedImageFormat.Gif, 
			ImageFormat.Icon => SKEncodedImageFormat.Ico, 
			ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg, 
			ImageFormat.Png => SKEncodedImageFormat.Png, 
			_ => SKEncodedImageFormat.Png, 
		};
	}

	private ImageFormat GetImageFormat(Stream stream)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("BM");
		byte[] bytes2 = Encoding.ASCII.GetBytes("GIF");
		byte[] array = new byte[4] { 137, 80, 78, 71 };
		byte[] array2 = new byte[3] { 73, 73, 42 };
		byte[] array3 = new byte[3] { 77, 77, 42 };
		byte[] array4 = new byte[4] { 255, 216, 255, 224 };
		byte[] array5 = new byte[4] { 255, 216, 255, 225 };
		byte[] array6 = new byte[4];
		stream.Read(array6, 0, array6.Length);
		if (bytes.SequenceEqual(array6.Take(bytes.Length)))
		{
			return ImageFormat.Bmp;
		}
		if (bytes2.SequenceEqual(array6.Take(bytes2.Length)))
		{
			return ImageFormat.Gif;
		}
		if (array.SequenceEqual(array6.Take(array.Length)))
		{
			return ImageFormat.Png;
		}
		if (array2.SequenceEqual(array6.Take(array2.Length)))
		{
			return ImageFormat.Tiff;
		}
		if (array3.SequenceEqual(array6.Take(array3.Length)))
		{
			return ImageFormat.Tiff;
		}
		if (array4.SequenceEqual(array6.Take(array4.Length)))
		{
			return ImageFormat.Jpeg;
		}
		if (array5.SequenceEqual(array6.Take(array5.Length)))
		{
			return ImageFormat.Jpeg;
		}
		return ImageFormat.Unknown;
	}

	internal ImageFormat GetImageFormat(SKEncodedImageFormat imageFormat)
	{
		return imageFormat switch
		{
			SKEncodedImageFormat.Bmp => ImageFormat.Bmp, 
			SKEncodedImageFormat.Gif => ImageFormat.Gif, 
			SKEncodedImageFormat.Jpeg => ImageFormat.Jpeg, 
			SKEncodedImageFormat.Png => ImageFormat.Png, 
			_ => ImageFormat.Png, 
		};
	}

	internal static Bitmap FromStream(MemoryStream memoryStream)
	{
		memoryStream.Position = 0L;
		return new Bitmap(memoryStream.ToArray()).Decode(memoryStream);
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

	public new void Dispose()
	{
		if (m_graphics != null)
		{
			m_graphics.Dispose();
			m_graphics = null;
		}
		if (m_sKBitmap != null)
		{
			m_sKBitmap.Dispose();
			m_sKBitmap = null;
		}
		if (m_imageData != null)
		{
			m_imageData = null;
		}
	}
}
