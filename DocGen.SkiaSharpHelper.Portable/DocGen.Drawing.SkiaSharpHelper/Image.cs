using System;
using System.IO;
using SkiaSharp;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Drawing.SkiaSharpHelper;

internal class Image : IDisposable, IImage
{
	private ImageFormat m_format;

	private PixelFormat m_pixelFormat;

	private Graphics m_graphics;

	private float m_verticalResolution = 96f;

	private float m_horizontalResolution = 96f;

	private SKBitmap m_sKBitmap;

	private byte[] m_imageData;

	public ImageFormat RawFormat
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public byte[] ImageData
	{
		get
		{
			return m_imageData;
		}
		set
		{
			m_imageData = value;
		}
	}

	public int Width => m_sKBitmap.Width;

	public int Height => m_sKBitmap.Height;

	internal Size Size => new Size(m_sKBitmap.Width, m_sKBitmap.Height);

	internal SKBitmap SKBitmap => m_sKBitmap;

	public float VerticalResolution
	{
		get
		{
			return m_verticalResolution;
		}
		set
		{
			m_verticalResolution = value;
		}
	}

	public float HorizontalResolution
	{
		get
		{
			return m_horizontalResolution;
		}
		set
		{
			m_horizontalResolution = value;
		}
	}

	public PixelFormat PixelFormat
	{
		get
		{
			return m_pixelFormat;
		}
		set
		{
			m_pixelFormat = value;
		}
	}

	internal Graphics Graphics
	{
		get
		{
			return m_graphics;
		}
		set
		{
			m_graphics = value;
		}
	}

	public Image(int width, int height)
	{
		m_sKBitmap = new SKBitmap(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
	}

	public Image(SKBitmap sKBitmap, int width, int height)
	{
		m_sKBitmap = sKBitmap.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Box);
	}

	public Image()
	{
	}

	public Image(SKBitmap sKBitmap)
	{
		m_sKBitmap = sKBitmap;
	}

	public Image(Stream stream)
	{
		if (stream != null)
		{
			if (this is Bitmap)
			{
				m_sKBitmap = (this as Bitmap).Decode(stream).SKBitmap;
			}
			else
			{
				m_sKBitmap = SKBitmap.Decode(SKData.Create(stream));
			}
		}
	}

	public virtual IImage Decode(byte[] imageData)
	{
		return null;
	}

	public virtual IImage Clone(RectangleF cropRectangle, object pixelFormat)
	{
		return null;
	}

	public void Save(Stream stream, ImageFormat imageFormat)
	{
		if (m_graphics == null)
		{
			m_graphics = Graphics.FromImage(this);
			m_graphics.DrawImage(this, new RectangleF(0f, 0f, m_sKBitmap.Width, m_sKBitmap.Height));
		}
		m_graphics.SkSurface.Snapshot().Encode(GetImageFormat(imageFormat), 100).SaveTo(stream);
	}

	public void Save(Stream stream)
	{
		Save(stream, ImageFormat.Png);
	}

	internal void Save(string file)
	{
		Save(file, ImageFormat.Png);
	}

	internal void Save(string file, ImageFormat imageFormat)
	{
		file = Path.GetFullPath(file);
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		FileStream fileStream = File.Create(file);
		Save(fileStream, imageFormat);
		fileStream.Dispose();
	}

	public void ApplyGrayScale()
	{
		Bitmap bitmap = this as Bitmap;
		if (bitmap == null)
		{
			bitmap = new Bitmap().Decode(ImageData) as Bitmap;
		}
		m_sKBitmap = SKBitmap.Decode(ImageData);
		if (m_sKBitmap.ColorType == SKColorType.Rgb888x && m_sKBitmap.CanCopyTo(SKColorType.Bgra8888))
		{
			SetSKBitmap(m_sKBitmap.Copy(SKColorType.Bgra8888));
		}
		if (bitmap == null)
		{
			return;
		}
		for (int i = 0; i < bitmap.Width; i++)
		{
			for (int j = 0; j < bitmap.Height; j++)
			{
				SKColor pixel = m_sKBitmap.GetPixel(i, j);
				byte b = (byte)(0.299 * (double)(int)pixel.Red + 0.587 * (double)(int)pixel.Green + 0.114 * (double)(int)pixel.Blue);
				SKColor color = new SKColor(b, b, b, pixel.Alpha);
				m_sKBitmap.SetPixel(i, j, color);
			}
		}
		ImageData = SKImage.FromBitmap(m_sKBitmap).Encode(SKEncodedImageFormat.Png, 100).ToArray();
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

	internal static Image FromStream(MemoryStream memoryStream)
	{
		Bitmap bitmap = new Bitmap();
		memoryStream.Position = 0L;
		bitmap.ImageData = memoryStream.ToArray();
		return bitmap.Decode(memoryStream);
	}

	public static Image FromImage(DocGen.Drawing.Image image)
	{
		Bitmap bitmap = (Bitmap)image;
		bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
		return bitmap;
	}

	public void Dispose()
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

	internal void SetSKBitmap(SKBitmap sKBitmap)
	{
		m_sKBitmap = sKBitmap;
	}
}
