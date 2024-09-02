using System;
using System.IO;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.Pdf.Graphics;

internal class ImageHelper : IDisposable, IImage
{
	private ImageFormat m_format;

	private PixelFormat m_pixelFormat;

	private GraphicsHelper m_graphics;

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

	internal byte[] ImageData
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

	internal GraphicsHelper Graphics
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

	internal ImageHelper()
	{
	}

	internal ImageHelper(byte[] imageData)
	{
		m_sKBitmap = SKBitmap.Decode(imageData);
		m_imageData = imageData;
	}

	public static ImageHelper FromImage(ImageHelper image)
	{
		Bitmap bitmap = new Bitmap(image.m_imageData);
		bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
		return bitmap;
	}

	public virtual IImage Decode(byte[] imageData)
	{
		return null;
	}

	public virtual IImage Clone(RectangleF cropRectangle, object pixelFormat)
	{
		return null;
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

	public void Save(Stream stream, ImageFormat imageFormat)
	{
	}
}
