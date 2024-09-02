using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Images.Decoder;

internal abstract class ImageDecoder
{
	private Stream m_internalStream;

	private int m_width;

	private int m_height;

	private byte[] m_imageData;

	private int m_bitsPerComponent;

	private ImageType m_format;

	private float m_horizontalResolution;

	private float m_verticalResolution;

	private float m_jpegDecoderOrientationAngle;

	private MemoryStream m_metadataStream;

	public Stream InternalStream
	{
		get
		{
			return m_internalStream;
		}
		set
		{
			m_internalStream = value;
		}
	}

	public int Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public int Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
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

	public int BitsPerComponent
	{
		get
		{
			return m_bitsPerComponent;
		}
		set
		{
			m_bitsPerComponent = value;
		}
	}

	public ImageType Format
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

	internal float JpegDecoderOrientationAngle
	{
		get
		{
			return m_jpegDecoderOrientationAngle;
		}
		set
		{
			m_jpegDecoderOrientationAngle = value;
		}
	}

	internal MemoryStream MetadataStream
	{
		get
		{
			return m_metadataStream;
		}
		set
		{
			m_metadataStream = value;
		}
	}

	public Size Size => new Size(Width, Height);

	public static bool TryGetDecoder(Stream stream, bool enableMetadata, out ImageDecoder decoder)
	{
		decoder = null;
		if (stream.IsPng())
		{
			decoder = new PngDecoder(stream, enableMetadata);
		}
		else if (stream.IsJpeg())
		{
			decoder = new JpegDecoder(stream, enableMetadata);
		}
		else if (stream.IsBmp())
		{
			decoder = new BmpDecoder(stream);
		}
		return decoder != null;
	}

	protected abstract void Initialize();

	internal abstract PdfStream GetImageDictionary();
}
